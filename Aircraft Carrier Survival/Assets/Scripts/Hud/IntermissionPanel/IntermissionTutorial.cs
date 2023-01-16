using FMODUnity;
using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class IntermissionTutorial : MonoBehaviour
{
    [SerializeField]
    private IntermissionTutorialStep start = null;

    [SerializeField]
    private GameObject container = null;
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text desc = null;
    [SerializeField]
    private Text desc2 = null;
    [SerializeField]
    private GameObject blocker = null;
    [SerializeField]
    private bool admiral = false;
    [SerializeField]
    private StudioEventEmitter highlightEmitter = null;

    private RectTransform rect;
    private Vector2 defaultAnchors;

    private HashSet<GameObject> activeHighlights;
    private Queue<IntermissionTutorialStep> buttonLessDatas;

    private Coroutine coroutine;

    private FMODEvent fmodEvent;
    private FMODEvent fmodEvent2;

    private void Start()
    {
        var data = SaveManager.Instance.Data;
        if (data.FinishedTutorial || data.GameMode == EGameMode.Sandbox)
        {
            enabled = false;
        }
        else
        {
            data.FinishedTutorial = !admiral;
            rect = container.GetComponent<RectTransform>();
            defaultAnchors = rect.anchorMin;

            activeHighlights = new HashSet<GameObject>();
            buttonLessDatas = new Queue<IntermissionTutorialStep>();

            if (start != null)
            {
                Setup(start);
            }
        }
    }

    private void Update()
    {
        if (coroutine != null)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            int count = buttonLessDatas.Count;
            for (int i = 0; i < count; i++)
            {
                Setup(buttonLessDatas.Dequeue());
            }
        }
    }

    private void OnDestroy()
    {
        fmodEvent?.Release();
    }

    public void SetPause(bool pause)
    {
        if (fmodEvent != null && fmodEvent.IsPlaying)
        {
            fmodEvent.SetPause(pause);
        }
    }

    private void Setup(IntermissionTutorialStep data)
    {
        Assert.IsNull(coroutine);
        if (data.Delay > 0f)
        {
            blocker.SetActive(true);
            float time = data.Delay;
            data.Delay = 0f;
            coroutine = this.StartCoroutineActionAfterTime(() =>
                {
                    Assert.IsNotNull(coroutine);
                    coroutine = null;
                    Setup(data);
                }, time);
            return;
        }

        foreach(var button in data.TriggerButtons)
        {
            button.onClick.RemoveListener(data.Action);
        }

        foreach (var highlight in activeHighlights)
        {
            highlight.SetActive(false);
        }
        activeHighlights.Clear();

        foreach (var highlight in data.Highlights)
        {
            highlight.SetActive(true);
            activeHighlights.Add(highlight);
        }
        foreach (var highlight in data.PersistentHighlights)
        {
            highlight.SetActive(true);
        }
        if (data.PersistentHighlights.Count > 0 || data.Highlights.Count > 0)
        {
            highlightEmitter.Play();
        }
        foreach (var highlight in data.HidePersistentHighlights)
        {
            highlight.SetActive(false);
        }

        fmodEvent?.Release();
        if (string.IsNullOrWhiteSpace(data.TitleID) && string.IsNullOrWhiteSpace(data.DescriptionID) && string.IsNullOrWhiteSpace(data.ExtraDescriptionID))
        {
            container.SetActive(false);
        }
        else
        {
            container.SetActive(true);

            var pos = data.CustomPosition;
            if (pos.x == 0f)
            {
                pos.x = defaultAnchors.x;
            }
            if (pos.y == 0f)
            {
                pos.y = defaultAnchors.y;
            }
            rect.anchorMax = pos;
            rect.anchorMin = pos;
            rect.anchoredPosition = Vector2.zero;

            SetText(title, data.TitleID);
            SetText(desc, data.DescriptionID);
            SetText(desc2, data.ExtraDescriptionID);

            if (string.IsNullOrWhiteSpace(data.FmodEvent))
            {
                fmodEvent = null;
            }
            else
            {
                if (fmodEvent2 == null)
                {
                    fmodEvent2 = new FMODEvent(data.FmodEvent);
                }
                else
                {
                    fmodEvent2.ReplaceEvent(data.FmodEvent);
                }
                fmodEvent = fmodEvent2;
                fmodEvent.Play();
                fmodEvent.SetPause(true);
            }
        }
        blocker.SetActive(false);
        foreach (var nextData in data.Next)
        {
            if (nextData.TriggerButtons.Count == 0)
            {
                buttonLessDatas.Enqueue(nextData);
            }
            else
            {
                nextData.Action = () => Setup(nextData);
                foreach (var button in nextData.TriggerButtons)
                {
                    button.onClick.AddListener(nextData.Action);
                }
            }
        }
        blocker.SetActive(buttonLessDatas.Count > 0);
    }

    private void SetText(Text text, string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            id = LocalizationManager.Instance.GetText(id);
        }
        text.text = id;
    }
}
