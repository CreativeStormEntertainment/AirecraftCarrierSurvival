using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PortraitDC : PortraitBase, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<DCInstanceGroup> Selected = delegate { };

    private readonly int highlightBool = Animator.StringToHash("Highlighted");

    public Image PortraitImage => portraitImage;
    //private readonly int selectedBool = Animator.StringToHash("Selected");
    //private readonly int normalTrigger = Animator.StringToHash("Normal");

    [SerializeField]
    private Image portraitImage = null;
    [SerializeField]
    private Text place = null;
    [SerializeField]
    private Text activity = null;
    [SerializeField]
    private RectTransform fillImage = null;
    [SerializeField]
    private GameObject buttons = null;
    [SerializeField]
    private List<DcCategoryButton> buttonList = null;
    [SerializeField]
    private Image currentCategoryImage = null;

    private DCInstanceGroup group;
    private DamageControlManager dcMan;
    private BackgroundAudio bgAudio;

    private RepairableDanger job;
    private float time;
    private float tickTime;
    private float lastTickPercent;
    private float currentTickPercent;

    private Dictionary<EDcCategory, DcCategoryButton> buttonDict;

    private bool selectedDisabled;

    private bool hover;
    private EWaypointTaskType currentJob = (EWaypointTaskType)(-1);
    private SubSectionRoom currentRoom;

    private void Update()
    {
        if (job != null)
        {
            if (currentTickPercent != job.RepairData.Percent)
            {
                lastTickPercent = currentTickPercent;
                currentTickPercent = job.RepairData.Percent;
                time = 0f;
            }
            time += Time.deltaTime;
            float percent = Mathf.Lerp(lastTickPercent, currentTickPercent, time / tickTime);
            float fillAmount = -fillImage.rect.height * (1f - percent);
            fillImage.anchoredPosition = new Vector2(fillImage.anchoredPosition.x, fillAmount);
        }
        if (hover)
        {
            RefreshTexts();
        }
    }

    public override void SetEnable(bool enable)
    {
        disabled = !enable;
        if (!enable)
        {
            OnPointerExit(null);
        }
    }

    public override void SetSelected(bool value)
    {
        base.SetSelected(value);
        if (!value)
        {
            buttons.SetActive(false);
            currentCategoryImage.transform.parent.gameObject.SetActive(true);
            Animator.SetBool(highlightBool, false);
        }
    }

    public void Setup(DCInstanceGroup dcGroup)
    {
        tickTime = TimeManager.Instance.TickTime;
        group = dcGroup;
        dcMan = DamageControlManager.Instance;
        bgAudio = BackgroundAudio.Instance;

        buttonDict = new Dictionary<EDcCategory, DcCategoryButton>();
        buttonDict.Add(EDcCategory.Fire, buttonList[0]);
        buttonDict.Add(EDcCategory.Crash, buttonList[1]);
        buttonDict.Add(EDcCategory.Water, buttonList[2]);
        buttonDict.Add(EDcCategory.Injured, buttonList[3]);

        foreach (var pair in buttonDict)
        {
            pair.Value.Button.onClick.AddListener(() => SetButton(pair.Key, false));
        }

        SetButton(group.Category, true);
    }

    public void RefreshButtons()
    {
        int categories = (int)DamageControlManager.Instance.DcCategoryEnabled;
        foreach (var pair in buttonDict)
        {
            pair.Value.enabled = !disabled && ((categories & (1 << (int)pair.Key)) != 0);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!disabled)
        {
            hover = true;
            RefreshTexts();
            if (group.FinalSegment != null)
            {
                Animator.SetBool(highlightBool, !buttons.activeInHierarchy);
                if (!buttons.activeSelf)
                {
                    bgAudio.PlayEvent(EMainSceneUI.HoverInDCPanel);
                }
            }
            currentCategoryImage.transform.parent.gameObject.SetActive(!buttons.activeInHierarchy);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hover = false;
        if (!disabled && !buttons.activeSelf)
        {
            bgAudio.PlayEvent(EMainSceneUI.HoverOutDCPanel);
        }
        Animator.SetBool(highlightBool, false);
        currentCategoryImage.transform.parent.gameObject.SetActive(!buttons.activeInHierarchy);
    }

    public void SetImage(Sprite sprite)
    {
        portraitImage.sprite = sprite;
    }

    public void StartJob(RepairableDanger danger)
    {
        job = danger;
        fillImage.gameObject.SetActive(danger != null);
        lastTickPercent = 0f;
        currentTickPercent = 0f;
        time = 0f;
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        if (!enable)
        {
            OnPointerExit(null);
        }
    }

    public void ShowButtons(bool show)
    {
        buttons.SetActive(show);
    }

    public void SetButton(EDcCategory category, bool force)
    {
        var dcMan = DamageControlManager.Instance;
        int categories = (int)dcMan.DcCategoryEnabled;

        if (force || (!disabled && (categories & (1 << (int)category)) != 0))
        {
            if (buttonDict.TryGetValue(group.Category, out var button))
            {
                button.Button.interactable = true;
            }
            group.Category = category;
            if (buttonDict.TryGetValue(group.Category, out button))
            {
                button.Button.interactable = false;
                currentCategoryImage.gameObject.SetActive(true);
                currentCategoryImage.sprite = buttonDict[group.Category].Sprite;
            }
            else
            {
                currentCategoryImage.gameObject.SetActive(false);
            }
            currentCategoryImage.transform.parent.gameObject.SetActive(true);
            buttons.SetActive(false);
            dcMan.FireDcCategoryClicked();
        }
        if (disabled || categories != -1)
        {
            RefreshButtons();
        }
    }

    protected override void Click()
    {
        if (!disabled && !selectedDisabled)
        {
            Selected(group);
            SectionRoomManager.Instance.PlayEvent(ESectionUIState.DCSelect);
        }
        buttons.SetActive(!buttons.activeInHierarchy);
        currentCategoryImage.transform.parent.gameObject.SetActive(!buttons.activeInHierarchy);
        Animator.SetBool(highlightBool, false);
        Animator.SetTrigger(normalTrigger);
    }

    protected override void DoubleClick()
    {
        if (!disabled && !selectedDisabled)
        {
            var camMan = CameraManager.Instance;
            camMan.SwitchMode(ECameraView.Sections);
            camMan.ZoomToSectionSegment(group.CurrentSegment);
        }
    }

    private void RefreshTexts()
    {
        var segment = group.FinalSegment == null ? group.CurrentSegment : group.FinalSegment;
        if (currentRoom != segment.Parent)
        {
            currentRoom = segment.Parent;
            place.text = currentRoom.Title;
        }
        if (currentJob != group.Job)
        {
            currentJob = group.Job;
            activity.text = dcMan.LocalizedJobs[group.Job];
        }
    }
}
