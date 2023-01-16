using FMODUnity;
using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StrikeGroupButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform WindowContainer => windowContainer;
    public Button ActiveButton => activeButton;
    public Image CooldownImage => cooldownImage;
    public Text CooldownText => cooldownText;
    public Text MinutesText => minutesText;

    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Text nameText = null;

    [SerializeField]
    private Button activeButton = null;
    [SerializeField]
    private Image cooldownImage = null;
    [SerializeField]
    private Text cooldownText = null;
    [SerializeField]
    private Text minutesText = null;

    [SerializeField]
    private GameObject disabledImage = null;
    [SerializeField]
    private GameObject toHideOnDead = null;

    [SerializeField]
    private Sprite maxSuppliesSprite = null;
    [SerializeField]
    private Sprite escortSprite = null;
    [SerializeField]
    private Sprite defenceSprite = null;
    [SerializeField]
    private Sprite maxSquadronsSprite = null;
    [SerializeField]
    private Sprite dcReparirSpeedSprite = null;
    [SerializeField]
    private Sprite spottingBonusSprite = null;
    [SerializeField]
    private Sprite maneuversDefSprite = null;
    [SerializeField]
    private Sprite fasterResupplySprite = null;

    [SerializeField]
    private List<StrikeGroupShipStats> shipStats = null;

    [SerializeField]
    private Image background = null;
    [SerializeField]
    private Sprite normalBg = null;
    [SerializeField]
    private Sprite selectedBg = null;
    [SerializeField]
    private Color normalColor = Color.black;
    [SerializeField]
    private Color selectedColor = Color.white;
    [SerializeField]
    private Image mask = null;
    [SerializeField]
    private Image buttonImage = null;
    [SerializeField]
    private Sprite normalButton = null;
    [SerializeField]
    private Sprite selectedButton = null;
    [SerializeField]
    private Sprite redButton = null;
    [SerializeField]
    private Sprite greyButton = null;
    [SerializeField]
    private Image buttonArrowImage = null;
    [SerializeField]
    private Color normalIconColor = Color.white;
    [SerializeField]
    private Color transparentIconColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField]
    private List<GameObject> emptyDurabilityIcons = null;
    [SerializeField]
    private List<GameObject> fillDurabilityIcons = null;
    [SerializeField]
    private Text duration = null;
    [SerializeField]
    private Text cooldown = null;
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text description = null;
    [SerializeField]
    private GameObject info = null;
    [SerializeField]
    private GameObject activeInfo = null;
    [SerializeField]
    private RectTransform windowContainer = null;

    [SerializeField]
    private Image cooldownIcon = null;
    [SerializeField]
    private Sprite cooldownSprite = null;
    [SerializeField]
    private Sprite durationSprite = null;

    private RectTransform parent;

    public void Setup(StrikeGroupMemberData data, StrikeGroupMember member)
    {
        parent = transform.parent.GetComponent<RectTransform>();
        var locMan = LocalizationManager.Instance;
        int i = 0;
        for (; i < data.PassiveSkills.Count; i++)
        {
            shipStats[i].gameObject.SetActive(true);
            switch (data.PassiveSkills[i].Skill)
            {
                case EStrikeGroupPassiveSkill.Defense:
                    shipStats[i].Image.sprite = defenceSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.Escort:
                    shipStats[i].Image.sprite = escortSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.MaxSquadrons:
                    shipStats[i].Image.sprite = maxSquadronsSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.MaxSupplies:
                    shipStats[i].Image.sprite = maxSuppliesSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString() + "%";
                    break;
                case EStrikeGroupPassiveSkill.DcRepairSpeed:
                    shipStats[i].Image.sprite = dcReparirSpeedSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString() + "%";
                    break;
                case EStrikeGroupPassiveSkill.RadarRange:
                    shipStats[i].Image.sprite = spottingBonusSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.BonusManeuversDefense:
                    shipStats[i].Image.sprite = maneuversDefSprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString();
                    break;
                case EStrikeGroupPassiveSkill.FasterResupply:
                    shipStats[i].Image.sprite = fasterResupplySprite;
                    shipStats[i].Text.text = data.PassiveSkills[i].Param.ToString() + "%";
                    break;
                default:
                    break;
            }
        }
        for (; i < shipStats.Count; i++)
        {
            shipStats[i].gameObject.SetActive(false);
        }
        nameText.text = locMan.GetText(data.NameID);
        icon.sprite = data.Icon;

        if (data.ActiveSkill != EStrikeGroupActiveSkill.None)
        {
            title.text = LocalizationManager.Instance.GetText(data.ActiveTitle);
            string param = "";
            switch (data.ActiveSkill)
            {
                case EStrikeGroupActiveSkill.ReplenishSquadrons:
                    param = data.Uses.ToString();
                    break;
            }
            if (string.IsNullOrEmpty(param))
            {
                description.text = LocalizationManager.Instance.GetText(data.ActiveDescription);
            }
            else
            {
                description.text = LocalizationManager.Instance.GetText(data.ActiveDescription, param);
            }
        }
        duration.text = (data.DurationHours + data.PrepareHours).ToString() + "h";
        cooldown.text = data.CooldownHours.ToString() + "h";
        UpdateDurability(data.Durability, true);

        info.SetActive(false);
        activeInfo.SetActive(data.ActiveSkill != EStrikeGroupActiveSkill.None);

        //hover.Setup(this);
        //tooltipText1.text = LocalizationManager.Instance.GetText(data.PassiveDescription);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipText1.transform as RectTransform);
        //tooltipText2.text = LocalizationManager.Instance.GetText(data.ActiveDescription);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipText2.transform as RectTransform);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(hover.transform as RectTransform);
        //tooltiopObject.SetActive(false);
    }

    public void UpdateDurability(int durability, bool baseDurability = false)
    {
        if (baseDurability)
        {
            for (int i = 0; i < emptyDurabilityIcons.Count; i++)
            {
                emptyDurabilityIcons[i].SetActive(i < durability);
                fillDurabilityIcons[i].SetActive(i < durability);
            }
        }
        else
        {
            for (int i = 0; i < fillDurabilityIcons.Count; i++)
            {
                fillDurabilityIcons[i].SetActive(i < durability);
            }
            SetDead(durability <= 0);
        }
    }

    public void SetButtonSprite(EButtonStates state)
    {
        if (!activeInfo.activeSelf)
        {
            state = EButtonStates.None;
        }
        activeButton.gameObject.SetActive(true);
        switch (state)
        {
            case EButtonStates.Normal:
                buttonImage.sprite = normalButton;
                buttonArrowImage.color = normalIconColor;
                activeButton.enabled = true;
                break;
            case EButtonStates.Pressed:
                buttonImage.sprite = selectedButton;
                buttonArrowImage.color = normalIconColor;
                activeButton.enabled = true;
                break;
            case EButtonStates.Red:
                buttonImage.sprite = redButton;
                buttonArrowImage.color = transparentIconColor;
                activeButton.enabled = false;
                cooldownIcon.sprite = durationSprite;
                break;
            case EButtonStates.Grey:
                buttonImage.sprite = greyButton;
                buttonArrowImage.color = transparentIconColor;
                activeButton.enabled = false;
                cooldownIcon.sprite = cooldownSprite;
                break;
            case EButtonStates.None:
                activeButton.gameObject.SetActive(false);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);

        info.SetActive(!disabledImage.activeSelf);
        background.sprite = selectedBg;
        mask.color = selectedColor;
        Rebuild();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        info.SetActive(false);
        background.sprite = normalBg;
        mask.color = normalColor;
        Rebuild();
    }

    public void Rebuild()
    {
        for (int i = 0; i < 5; i++)
        {
            GameSceneManager.Instance.StartCoroutineActionAfterFrames(() => LayoutRebuilder.ForceRebuildLayoutImmediate(parent), i);
        }
    }

    private void SetDead(bool dead)
    {
        toHideOnDead.SetActive(!dead);
        disabledImage.SetActive(dead);
        info.SetActive(false);
    }
}

public enum EButtonStates
{
    Normal,
    Pressed,
    Red,
    Grey,
    None
}

