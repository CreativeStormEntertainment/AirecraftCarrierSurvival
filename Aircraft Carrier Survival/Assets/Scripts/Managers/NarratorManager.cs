using UnityEngine;
using UnityEngine.UI;

public class NarratorManager : MonoBehaviour
{
    public static NarratorManager Instance;

    [SerializeField]
    private GameObject container = null;
    [SerializeField]
    private Text desc = null;
    [SerializeField]
    private Text subTitle = null;
    [SerializeField]
    private Text subDesc = null;

    private FMODEvent fmodEvent;

    private string id;

    private float defaultWidth;
    private Vector2 defaultAnchors;
    private RectTransform rect;

    private void Awake()
    {
        Instance = this;
        rect = container.GetComponent<RectTransform>();
        defaultWidth = rect.sizeDelta.x;
        defaultAnchors = rect.anchorMin;
    }

    private void OnDestroy()
    {
        fmodEvent?.Release();
    }

    public void Show(string desc, string subTitle, string subDesc, string sound, float overrideWidth, Vector2 pos)
    {
        container.SetActive(true);

        var locMan = LocalizationManager.Instance;

        this.desc.text = locMan.GetText(desc);
        this.subTitle.text = locMan.GetText(subTitle);
        this.subDesc.text = locMan.GetText(subDesc);

        if (id != desc)
        {
            id = desc;
            if (fmodEvent != null)
            {
                fmodEvent.Stop();
                fmodEvent.Release();
            }
            if (FMODEvent.IsValid(sound))
            {
                if (fmodEvent == null)
                {
                    fmodEvent = new FMODEvent(sound);
                }
                else
                {
                    fmodEvent.ReplaceEvent(sound);
                }
                fmodEvent.Play();
            }
        }

        rect.sizeDelta = new Vector2(overrideWidth > 0f ? overrideWidth : defaultWidth, rect.sizeDelta.y);
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
    }

    public void Hide()
    {
        if (fmodEvent != null)
        {
            fmodEvent.Stop(true);
            fmodEvent.Release();
        }
        id = string.Empty;
        container.SetActive(false);
    }

    public void SetPause(bool pause)
    {
        if (fmodEvent != null && fmodEvent.IsPlaying)
        {
            fmodEvent.SetPause(pause);
        }
    }
}
