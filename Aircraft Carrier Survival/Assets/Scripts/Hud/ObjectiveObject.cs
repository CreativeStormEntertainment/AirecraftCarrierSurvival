using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveObject : MonoBehaviour
{
    public string Title
    {
        get;
        private set;
    }
    public List<ObjectiveStepObject> StepObjects
    {
        get => stepObjects;
        set => stepObjects = value;
    }

    public RectTransform MainRect
    {
        get => mainRect;
        private set => mainRect = value;
    }

    public RectTransform MainTextRect
    {
        get => mainTextRect;
        private set => mainTextRect = value;
    }

    public RectTransform ContentRect
    {
        get => contentRect;
        private set => contentRect = value;
    }

    public List<ObjectiveUISpriteData> TacticalMapImages => tacticalMapImages;

    public Text ObjectiveNumberText => objectiveNumberText;

    public ObjectiveUISpriteData UOMapImagePrefab;
    public List<ObjectiveUISpriteData> UOMapImages;

    public float mainRectDeltaSize;

    [SerializeField]
    private RectTransform mainRect = null;
    [SerializeField]
    private RectTransform contentRect = null;
    [SerializeField]
    private RectTransform mainTextRect = null;
    [SerializeField]
    private Text text = null;
    [SerializeField]
    private Text objectiveNumberText = null;
    [SerializeField]
    private TMP_Text description = null;
    [SerializeField]
    private Image icon = null;

    [SerializeField]
    private Sprite failed = null;
    [SerializeField]
    private Sprite complete = null;

    [SerializeField]
    private ObjectiveUISpriteData tacticalMapImage = null;

    /*[SerializeField]
    private RectTransform dropIconRect = null;
    [SerializeField]
    private TooltipCaller tooltipCaller = null;
    [SerializeField]
    private Button extendButton = null;*/

    private bool isContentClose = false;

    private List<ObjectiveStepObject> stepObjects = null;

    private Vector2 dropdownIconPos = new Vector2(0f, 2f);

    private List<ObjectiveUISpriteData> tacticalMapImages;

    public ObjectiveObject Setup(string title, string description, List<ObjectiveSpriteData> spritesData, string[] param)
    {
        Title = title;
        var locMan = LocalizationManager.Instance;
        this.description.fontStyle = FontStyles.Normal;
        this.description.text = string.IsNullOrWhiteSpace(description) ? description : locMan.GetText(description);

        LayoutRebuilder.ForceRebuildLayoutImmediate(mainTextRect);
        mainRectDeltaSize = mainTextRect.sizeDelta.x;

        mainRect.sizeDelta = new Vector2(mainTextRect.rect.width, 40f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        //contentRect.gameObject.SetActive(false);
        mainRect.sizeDelta = new Vector2(mainRect.sizeDelta.x, contentRect.sizeDelta.y + 35f);

        tacticalMapImage.gameObject.SetActive(false);
        tacticalMapImages = new List<ObjectiveUISpriteData>();
        foreach (var data in spritesData)
        {
            var img = Instantiate(tacticalMapImage, Vector2.zero, Quaternion.identity, data.Parent);
            var rect = img.GetComponent<RectTransform>();
            img.gameObject.SetActive(true);
            rect.anchoredPosition = data.Pos;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            if (data.UO)
            {
                img.ObjectObjective.SetActive(true);
                UOMapImagePrefab = img;
                UOMapImages = new List<ObjectiveUISpriteData>();
                continue;
            }

            if (data.Ship == null)
            {
                img.MapObjective.SetActive(true);
            }
            else
            {
                data.Ship.Objectives.Add(rect);
                img.ObjectObjective.SetActive(true);
            }

            tacticalMapImages.Add(img);
        }

        SetTitle(title, param);

        gameObject.SetActive(false);

        return this;
    }

    public void Finish(bool success)
    {
        icon.sprite = success ? complete : failed;
        description.fontStyle = FontStyles.Strikethrough;
    }

    public void UnlockStep(int step, bool setAsLastSibling)
    {
        StepObjects[step].gameObject.SetActive(true);
        if (setAsLastSibling)
        {
            StepObjects[step].transform.SetAsLastSibling();
        }
        UpdateRectSize();
    }

    public void HideStep(int step)
    {
        StepObjects[step].gameObject.SetActive(false);
        UpdateRectSize();
    }

    public void SetObjectiveNumber(int i, bool showText, string text)
    {
        objectiveNumberText.text = i.ToString();

        foreach (var image in tacticalMapImages)
        {
            SetImage(image, showText, text);
        }

        if (UOMapImagePrefab != null)
        {
            SetImage(UOMapImagePrefab, showText, text);
            foreach (var image in UOMapImages)
            {
                SetImage(image, showText, text);
            }
        }
    }

    public void ToogleObjectiveContent()
    {
        contentRect.gameObject.SetActive(isContentClose);
        isContentClose = !isContentClose;
        //dropIconRect.rotation = isContentClose ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 0f, 180f);
        //dropIconRect.anchoredPosition = isContentClose ? Vector2.zero : dropdownIconPos;
        UpdateRectSize();
    }

    public void SetStepState(int step, bool completed)
    {
        StepObjects[step].SetStepState(completed);
    }

    public void UpdateRectSize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        mainRect.sizeDelta = new Vector2(mainRect.sizeDelta.x, (isContentClose ? 0 : contentRect.sizeDelta.y) + 35f);
        ObjectivesManager.Instance.UpdateRect();
    }

    public void SetText(string title, string description, string[] param)
    {
        this.description.text = string.IsNullOrWhiteSpace(description) ? description : LocalizationManager.Instance.GetText(description);

        SetTitle(title, param);
    }

    public string GetTitle()
    {
        return text.text;
    }

    private void SetTitle(string title, string[] param)
    {
        var locMan = LocalizationManager.Instance;
        string txt;
        if (string.IsNullOrWhiteSpace(title))
        {
            txt = "";
        }
        else if (param == null)
        {
            txt = locMan.GetText(title);
        }
        else
        {
            txt = locMan.GetText(title, param);
        }
        text.text = txt;

        foreach (var img in tacticalMapImages)
        {
            img.GetComponent<TacticalMapTooltipCaller>().SetText(txt);
        }

        if (UOMapImagePrefab != null)
        {
            UOMapImagePrefab.GetComponent<TacticalMapTooltipCaller>().SetText(txt);
            foreach (var img in UOMapImages)
            {
                img.GetComponent<TacticalMapTooltipCaller>().SetText(txt);
            }
        }
    }

    private void SetImage(ObjectiveUISpriteData image, bool showText, string text)
    {
        image.gameObject.SetActive(showText);
        if (showText)
        {
            image.MapObjectiveText.text = text;
            image.ObjectObjectiveText.text = text;
        }
    }
}
