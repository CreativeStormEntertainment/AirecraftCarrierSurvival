using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvisorPopup : MonoBehaviour
{
    public event Action PopupClosed = delegate { };

    public static AdvisorPopup Instance;

    public TutorialPopup TutorialPopup;

    [SerializeField]
    protected Text pressAnyText = null;
    private float pressAnyTextTS = 0;
    [SerializeField]
    private Color pressAnyTextColor = Color.white;

    [SerializeField]
    private Text title; 
    [SerializeField]
    private Text content = null; 

    [SerializeField]
    private List<AdvisorData> texts = new List<AdvisorData>();
    [SerializeField]
    private float timeToClose = 10f;

    private Dictionary<EAdvisorText, AdvisorData> textsDict;
    private float time;

    private void Awake()
    {
        Instance = this;

        textsDict = new Dictionary<EAdvisorText, AdvisorData>();
        for (int i = 0; i < texts.Count; ++i)
        {
            textsDict.Add(texts[i].ID, texts[i]);
        }
        gameObject.SetActive(false);

        time = float.PositiveInfinity;
    }

    public void Show(EAdvisorText text)
    {
        if (textsDict.TryGetValue(text, out AdvisorData data))
        {
            content.text = LocalizationManager.Instance.GetText(data.DescriptionID);
            gameObject.SetActive(true);
            time = Time.realtimeSinceStartup + timeToClose;
            TutorialPopup.PlayVO(data.VO);
        } 
        else
        {
            Debug.LogWarning("No advisor data - " + text.ToString());
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        time = float.PositiveInfinity;
        TutorialPopup.FadeoutVO();
        PopupClosed();
    }

    private void Update()
    {
        if (Time.realtimeSinceStartup > time || Input.GetKeyDown(KeyCode.Space))
        {
            Hide();
        }
        pressAnyTextTS += Time.unscaledDeltaTime;
        if (pressAnyTextTS >= TutorialPopup.highlightTime)
        {
            pressAnyTextTS = 0f;
        }
        pressAnyTextColor.a = TutorialPopup.highlightCurve.Evaluate(pressAnyTextTS / TutorialPopup.highlightTime);
        pressAnyText.color = pressAnyTextColor;
    }
}
