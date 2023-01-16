using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CrewTooltip : TooltipCaller
{
    [SerializeField]
    private CrewTooltipManager crewTooltipManager = null;

    private ItemSlot itemSlot = null;

    protected override void Awake()
    {
        Trans = GetComponent<RectTransform>();
        itemSlot = GetComponent<ItemSlot>();
        itemSlot.OnSlotUpdated += SlotUpdated;
        base.Awake();
    }

    private void Start()
    {
        SlotUpdated();
    }

    protected override void UpdateText()
    {
        base.UpdateText();
        var locMan = LocalizationManager.Instance;
        title = locMan.GetText(TitleID);
        description = locMan.GetText(DescriptionID);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (DragDrop.CurrentDrag)
        {
            //return;
        }
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
    }

    private void SlotUpdated()
    {
        var locMan = LocalizationManager.Instance;
        if (itemSlot.CurrentCrewUnit == null)
        {
            TitleID = crewTooltipManager.EmptyTitleID;
            DescriptionID = crewTooltipManager.EmptyDescID;
            title = locMan.GetText(crewTooltipManager.EmptyTitleID);
            description = locMan.GetText(crewTooltipManager.EmptyDescID);
        }
        else if (itemSlot.CurrentCrewUnit.UnitState == ECrewUnitState.Healthy)
        {
            TitleID = crewTooltipManager.HealthyTitleID;
            DescriptionID = crewTooltipManager.HealthyDescID;
            title = locMan.GetText(crewTooltipManager.HealthyTitleID);
            description = locMan.GetText(crewTooltipManager.HealthyDescID);
        }
        else if (itemSlot.CurrentCrewUnit.UnitState == ECrewUnitState.Injured)
        {
            TitleID = crewTooltipManager.InjuredTitleID;
            DescriptionID = crewTooltipManager.InjuredDescID;
            title = locMan.GetText(crewTooltipManager.InjuredTitleID);
            description = locMan.GetText(crewTooltipManager.InjuredDescID);
        }
        else if (itemSlot.CurrentCrewUnit.UnitState == ECrewUnitState.Dead)
        {
            TitleID = crewTooltipManager.DeadTitleID;
            DescriptionID = crewTooltipManager.DeadDescID;
            title = locMan.GetText(crewTooltipManager.DeadTitleID);
            description = locMan.GetText(crewTooltipManager.DeadDescID);
        }
    }
}
