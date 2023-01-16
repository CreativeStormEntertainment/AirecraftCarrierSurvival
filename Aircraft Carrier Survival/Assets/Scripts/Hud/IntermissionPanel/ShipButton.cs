using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShipButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public Button button;

    public Image ShipIcon;
    public Text NameText;

    public GameObject LockedIcon;
    [SerializeField]
    private List<Text> statTexts = new List<Text>();
    [SerializeField]
    private List<GameObject> statIcons = new List<GameObject>();

    private int shipIndex = 0;

    private GameObject panel = null;

    public static SlotShipButton CurrentChosenSlot = null;

    [SerializeField]
    GameObject highlightTexture = null;
    [SerializeField]
    GameObject pressedTexture = null;
    [SerializeField]
    GameObject pressedTexture2 = null;

    [SerializeField]
    private  FMODUnity.StudioEventEmitter emitter = null;

    [SerializeField]
    private ShowTooltip tooltipToShow = null;

    [SerializeField]
    private ShowTooltip secondaryTooltip = null;

    private ShowTooltip tooltip;

    private EscortSubpanel escortPanel;

    public void Setup(EscortSubpanel escortPanel, GameObject panel, int shipIndex, StrikeGroupMemberData data)
    {
        this.panel = panel;
        this.shipIndex = shipIndex;
        ShipIcon.sprite = data.Icon;
        var locMan = LocalizationManager.Instance;
        //tooltipToShow.IndexTooltip = (int)data.TooltipID;
        //secondaryTooltip.IndexTooltip = (int)data.TooltipID;
        NameText.text = locMan.GetText(data.NameID);
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
        this.escortPanel = escortPanel;
        for (int i = 0; i < statTexts.Count; ++i)
        {
            int end = i * (int)EStrikeGroupPassiveSkill.Count + (int)EStrikeGroupPassiveSkill.Count;
            if (i < data.PassiveSkills.Count)
            {
                switch (data.PassiveSkills[i].Skill)
                {
                    case EStrikeGroupPassiveSkill.MaxSupplies:
                        statTexts[i].text = data.PassiveSkills[i].Param+"%";
                        break;
                    case EStrikeGroupPassiveSkill.Escort:
                        statTexts[i].text = "+"+data.PassiveSkills[i].Param;
                        break;
                    case EStrikeGroupPassiveSkill.Defense:
                        statTexts[i].text = "+" + data.PassiveSkills[i].Param;
                        break;
                    case EStrikeGroupPassiveSkill.MaxSquadrons:
                        statTexts[i].text = "+"+data.PassiveSkills[i].Param;
                        break;
                }
                for (int j = i * (int)EStrikeGroupPassiveSkill.Count, n = 0; j < end && j < statIcons.Count; ++j, ++n)
                {
                    statIcons[j].SetActive(n == (int)data.PassiveSkills[i].Skill);
                }
                statTexts[i].gameObject.SetActive(true);
            }
            else
            {
                for (int j = i * (int)EStrikeGroupPassiveSkill.Count, n = 0; j < end && j < statIcons.Count; ++j, ++n)
                {
                    statIcons[j].SetActive(false);
                }
                statTexts[i].gameObject.SetActive(false);
            }
        }

        SwitchTooltip(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.enabled && button.interactable)
        {
            //BackgroundAudio.Instance.PlayEvent(EIntermissionClick.ShipHover);
            emitter.Play();
            if (tooltip)
            {
                tooltip.FillTooltip();
            }
            int index = CurrentChosenSlot.ShipIndex;

            highlightTexture.SetActive(true);
            CurrentChosenSlot.ShowHideHighlightSelections(index, false);
            CurrentChosenSlot.ShowHideHighlightSelections(shipIndex, true);

            escortPanel.SaveCurrentStats();
            CurrentChosenSlot.ShipIndex = shipIndex;
            escortPanel.ShowStats();
            CurrentChosenSlot.ShipIndex = index;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipToShow)
        {
            tooltipToShow.HideTooltip();
        }
        if (secondaryTooltip)
        {
            secondaryTooltip.HideTooltip();
        }

        highlightTexture.SetActive(false);

        CurrentChosenSlot.ShowHideHighlightSelections(shipIndex, false);
        CurrentChosenSlot.ShowHideHighlightSelections(CurrentChosenSlot.ShipIndex, true);

        escortPanel.BackToCurrentStats();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        highlightTexture.SetActive(false);
        pressedTexture.SetActive(true);
        pressedTexture2.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressedTexture.SetActive(false);
        pressedTexture2.SetActive(false);
    }

    public void SwitchTooltip(bool last)
    {
        tooltip = last ? secondaryTooltip : tooltipToShow;
    }

    private void OnClicked()
    {
        //BackgroundAudio.Instance.PlayEvent(EIntermissionClick.ShipClick);
        BackgroundAudio.Instance.ShipClick();
        panel.SetActive(false);

        //Debug.Log(CurrentChosenSlot.Nr + " " + CurrentChosenSlot.ShipIndex + " " + shipIndex);
        escortPanel.shipModels[CurrentChosenSlot.Nr][CurrentChosenSlot.ShipIndex].SetActive(false);
        CurrentChosenSlot.ShipIndex = shipIndex;
        CurrentChosenSlot.ShowShipAndIcon();
        CurrentChosenSlot.ShipName.text = NameText.text;

        CurrentChosenSlot.Deselect();

        escortPanel.ShowStats();
        if (tooltipToShow)
        {
            tooltipToShow.HideTooltip();
        }
        if (secondaryTooltip)
        {
            secondaryTooltip.HideTooltip();
        }
        /*
        var saveData = SaveManager.Instance.Data;

        var shipIndex = (int)shipType;
        var shipList = IntermissionManager.Instance.GetShipList();
        var costCurrentShip = shipList[shipIndex].GetUnlockCost();

        if ((saveData.LockedConvoyTypes & (1 << (shipIndex))) == (1 << (shipIndex)))
        {
            if (costCurrentShip <= saveData.CommandPoints)
            {
                LockedIcon.SetActive(false);
                PriceText.SetActive(false);
                Price.SetActive(false);

                //IntermissionPanel.Instance.ReduceCommandPoints(costCurrentShip);

                saveData.LockedConvoyTypes = saveData.LockedConvoyTypes & ~(1 << (shipIndex)); // unlock ship
            }
        }
        else
        {
            if (!IsChosen)
            {
                saveData.Convoy.Add(shipType);
            }
        }
        */
    }
}
