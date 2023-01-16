using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Portrait : PortraitBase, IPointerEnterHandler, IPointerExitHandler, IEnableable
{
    private static readonly int HighlightBool = Animator.StringToHash("Highlighted");
    private static readonly int HiddenBool = Animator.StringToHash("Hidden");

    public RectTransform RootRect;
    public Image Hightlight;
    public Image PortraitImage;
    public Text Name;
    public Text Title;
    public Text Desc;
    public Color SelectionColor;
    public Image Frame;
    public Officer Officer;
    public GameObject NavyImage;
    public GameObject AirImage;
    public Text AirLvl;
    public Text NavyLvl;
    public Button Button;
    public RectTransform EnduranceTransform;

    public float CooldownTime
    {
        get;
        set;
    }

    [SerializeField]
    private IntTooltip tooltipCaller = null;
    [SerializeField]
    private Image activeBuffImage = null;
    [SerializeField]
    private Image activeBuffTooltipImage = null;

    [SerializeField]
    private Image cooldownImage = null;

    private List<Transform> enduranceIcons;

    private LocalizationManager locMan;

    private bool originalDisabled;
    private bool selectedDisabled;

    private bool showBuffs;

    private void Start()
    {
        locMan = LocalizationManager.Instance;

        if (Officer.CanBeAssignedToCategory(EIslandRoomCategory.AIR))
        {
            if (Officer.CanBeAssignedToCategory(EIslandRoomCategory.SHIP))
            {
                tooltipCaller.TitleID = "OfficerAdmiralTitle";
                //title = SaveManager.Instance.Data.AdmiralName;
                //description = locMan.GetText("AdmiralDesc");
            }
            else
            {
                tooltipCaller.TitleID = "OfficerAirTitle";
                //title = locMan.GetText("AirOfficerTitle");
                //description = locMan.GetText("AirOfficerDesc");
            }
        }
        else
        {
            tooltipCaller.TitleID = "OfficerNavyTitle";
            //title = locMan.GetText("NavyOfficerTitle");
            //description = locMan.GetText("NavyOfficerDesc");
        }
        tooltipCaller.FireParamsChanged();
    }

    private void Update()
    {
        int cooldownTicks = Parameters.Instance.OfficerCooldownTicks;
        if (Officer != null && Officer.Cooldown > 0)
        {
            CooldownTime += Time.deltaTime;
            cooldownImage.fillAmount = Mathf.InverseLerp(cooldownTicks, 0f, CooldownTime);
        }
        else
        {
            cooldownImage.fillAmount = 0f;
        }
    }

    public override void SetEnable(bool enable)
    {
        originalDisabled = !enable;
        enable = enable && !selectedDisabled;
        base.SetEnable(enable);
        if (!enable)
        {
            OnPointerExit(null);
        }
    }

    public void Init(Officer officer)
    {
        locMan = LocalizationManager.Instance;
        Officer = officer;
        Officer.Portrait = this;
        PortraitImage.sprite = officer.PortraitSprite;
        Name.text = officer.Name;
        if (officer.Desc != null)
        {
            Desc.text = locMan.GetText(officer.Desc);
        }
        if (!string.IsNullOrEmpty(officer.Title))
        {
            Title.text = locMan.GetText(officer.Title);
        }
        // RoomImage.sprite = officer.CurrentIslandRoom.RoomIcon;

        //enduranceIcons = new List<Transform>();
        //foreach (RectTransform t in EnduranceTransform)
        //{
        //    enduranceIcons.Add(t.GetChild(0));
        //}
        //enduranceIcons.Reverse();

        UpdateSkillsInfo();

        if (!Officer.CanBeAssignedToCategory(EIslandRoomCategory.AIR))
        {
            AirLvl.rectTransform.parent.gameObject.SetActive(false);
        }
        if (!Officer.CanBeAssignedToCategory(EIslandRoomCategory.SHIP))
        {
            NavyLvl.rectTransform.parent.gameObject.SetActive(false);
        }
        AirImage.SetActive(false);
        NavyImage.SetActive(false);
        if (Officer.CanBeAssignedToCategory(EIslandRoomCategory.AIR))
        {
            AirImage.SetActive(true);
            if (Officer.CanBeAssignedToCategory(EIslandRoomCategory.SHIP))
            {
                NavyImage.SetActive(true);
            }
        }
        else
        {
            NavyImage.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var islMan = IslandsAndOfficersManager.Instance;
        if (!disabled && !islMan.BuffsPanelOpen)
        {
            if (showBuffs)
            {
                activeBuffImage.gameObject.SetActive(false);
            }
            Officer.CurrentIslandRoom.OutlineBlink(true);
            IslandsAndOfficersManager.Instance.PlayEvent(EIslandUIState.HoverIn);
            Animator.SetBool(HighlightBool, true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var islMan = IslandsAndOfficersManager.Instance;
        if (showBuffs)
        {
            activeBuffImage.gameObject.SetActive(true);
        }
        Officer.CurrentIslandRoom.OutlineBlink(false);
        if (!disabled && !islMan.BuffsPanelOpen)
        {
            islMan.PlayEvent(EIslandUIState.HoverOut);
        }

        Animator.SetBool(HighlightBool, false);
    }

    public void UpdateIcon()
    {

    }

    public void UpdateSkillsInfo()
    {
        AirLvl.text = Officer.GetSkillLevel(EIslandRoomCategory.AIR).ToString();
        NavyLvl.text = Officer.GetSkillLevel(EIslandRoomCategory.SHIP).ToString();
        //foreach (Transform t in enduranceIcons)
        //{
        //    t.gameObject.SetActive(false);
        //    t.GetChild(0).gameObject.SetActive(true);
        //}
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        SetEnable(!originalDisabled);
    }

    public void SetHighlighted(bool highlighted)
    {
        Hightlight.gameObject.SetActive(highlighted);
    }

    public void SetHidden(bool hidden)
    {
        Animator.SetBool(HiddenBool, Officer.Occupied);
        Animator.SetBool(selectedBool, !hidden && !Officer.Occupied);
        Animator.SetTrigger(normalTrigger);
    }

    public void SetCurrentBuffImage(Sprite sprite)
    {
        if (sprite != null)
        {
            activeBuffImage.sprite = sprite;
            activeBuffTooltipImage.sprite = sprite;
            activeBuffImage.gameObject.SetActive(true);
            activeBuffTooltipImage.gameObject.SetActive(true);
            showBuffs = true;
        }
        else
        {
            activeBuffImage.gameObject.SetActive(false);
            activeBuffTooltipImage.gameObject.SetActive(false);
            showBuffs = false;
        }
    }

    protected override void Click()
    {
        if (!disabled && !Officer.Occupied)
        {
            PortraitManager.Instance.SelectOfficer(Officer);
            VoiceSoundsManager.Instance.PlaySelect(Officer.Voice);
        }
    }

    protected override void DoubleClick()
    {

    }
}
