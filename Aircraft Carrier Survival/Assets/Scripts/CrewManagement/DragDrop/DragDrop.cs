/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, ITickable, IBeginDragHandler, IEndDragHandler, IDragHandler, IEnableable
{
    public static DragDrop CurrentDrag = null;

    public Vector3 SlotOffset => slotOffset;
    public DepartmentItem CurrentDepartment
    {
        get;
        protected set;
    }

    public ItemSlot CurrentSlot;
    public ItemSlot SlotBeforeDrag;
    public Canvas Canvas;

    [SerializeField]
    private float timerTickHours = 0.5f;

    [SerializeField]
    private Vector3 slotOffset = Vector3.zero;

    private int timeToPass;

    private DepartmentItem prevDepartment;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private CrewUnit unit;
    private ItemSlot lastSlot;

    private EventSystem eventSystem;

    private bool runTimer;

    private PointerEventData lastPointerData;

    private int cooldownTime;

    private ItemSlot slotToSwap;
    private bool disabled;
    private bool ignore;

    private bool dragging;

    private void Awake()
    {
        HudManager.QuickSaved += OnQuickSaved;
    }

    private void Update()
    {
        if (dragging && !Input.GetMouseButton(0))
        {
            OnEndDrag(null);
        }
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
        enabled = enable;
    }

    public void Setup()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        unit = GetComponent<CrewUnit>();
        lastSlot = CurrentSlot;
        eventSystem = FindObjectOfType<EventSystem>();
        transform.localPosition = slotOffset;

        cooldownTime = Mathf.RoundToInt(timerTickHours * (float)TimeManager.Instance.TicksForHour);

        timeToPass = 0;
    }

    public void LoadData(ref CrewSaveData data)
    {
        if (data.Cooldown > 0)
        {
            timeToPass = data.Cooldown;
            unit.UpdateCooldown(true);
            TimeManager.Instance.AddTickable(this);
            runTimer = true;
            unit.IsCalculable = false;
        }
    }

    public void SaveData(ref CrewSaveData data)
    {
        data.Cooldown = timeToPass;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (disabled || eventData.button != PointerEventData.InputButton.Left)
        {
            ignore = disabled;
            Debug.Log($"{name} begin ignored");
            return;
        }
        Assert.IsFalse(dragging);
        dragging = true;
        SlotBeforeDrag = CurrentSlot;
        if (SlotBeforeDrag != null)
        {
            SlotBeforeDrag.CrewUnitBeforeDrag = unit;
        }
        lastPointerData = eventData;
        if (!HudManager.Instance.HasNo(ETutorialMode.DisableCrewDrag))
        {
            unit.CrewManager.PlayEvent(ECrewUIState.Unavailable);
            return;
        }
        unit.CrewManager.PlayEvent(ECrewUIState.DragStart);
        rectTransform.SetParent(unit.CrewManager.MainCanvas.transform);
        if (timeToPass <= 0)
        {
            TimeManager.Instance.RemoveTickable(this);
        }
        runTimer = false;
        unit.UpdateCooldown(false);
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
        prevDepartment = CurrentDepartment;
        CurrentDrag = this;
        Tooltip.Instance.Hide();

        EfficiencyChange();

        slotToSwap = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            Debug.Log($"{name} drag ignored");
            return;
        }
        if (disabled || ignore || !HudManager.Instance.HasNo(ETutorialMode.DisableCrewDrag) || !dragging)
        {
            Debug.Log($"{name} drag ignored.");
            return;
        }
        HighlightEmptySlots(true, unit.UnitState == ECrewUnitState.Dead);
        if (CurrentSlot != null)
        {
            CurrentSlot.UnpackUnit();
        }
        rectTransform.anchoredPosition += eventData.delta / Canvas.scaleFactor;

        EfficiencyChange();

        slotToSwap = null;

        bool hasSlot = false;
        //if (SectionRoomManager.Instance.Lockers.IsWorking)
        {
            if (eventSystem.IsPointerOverGameObject())
            {
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);
                ItemSlot slot = results[0].gameObject.GetComponentInParent<ItemSlot>();

                if (slot != null && slot.IsUsable &&
                    (HudManager.Instance.HasNo(ETutorialMode.DisableFreeCrewDrag) || DemoMissionGame.Instance.EnabledDepartment == slot.CurrentDepartment))
                {
                    int departments = (int)unit.CrewManager.DepartmentsEnabled;
                    if (slot.CurrentDepartment == null ? ((departments & (int)EDepartmentsFlag.Idle) != 0 || ((departments & (int)EDepartmentsFlag.Healing) != 0 && slot.IsHealing)) :
                        (departments & (1 << (int)slot.CurrentDepartment.Departments)) != 0)
                    {
                        if (slot.CurrentCrewUnit == null)
                        {
                            hasSlot = true;
                            if (unit.UnitState == ECrewUnitState.Dead && slot.CurrentDepartment == null)
                            {
                                CurrentSlot = slot;
                            }
                            else if (unit.UnitState != ECrewUnitState.Dead)
                            {
                                CurrentSlot = slot;
                                if (CurrentDepartment != slot.CurrentDepartment)
                                {
                                    if (CurrentDepartment != null)
                                    {
                                        CurrentDepartment.ClearEfficiencyChange();
                                    }
                                    if (slot.CurrentDepartment != null)
                                    {
                                        unit.CrewManager.PlayEvent(ECrewUIState.HoverDrag);
                                    }
                                }
                                CurrentDepartment = slot.CurrentDepartment;
                            }
                        }
                        else if (!GameStateManager.Instance.Tutorial && slot.CurrentCrewUnit != unit && slot.CrewUnitBeforeDrag != unit && (SlotBeforeDrag.CurrentDepartment == null || slot.CurrentDepartment == null) &&
                            ((SlotBeforeDrag.CurrentDepartment == null && slot.CurrentDepartment == null) || (unit.UnitState != ECrewUnitState.Dead && slot.CurrentCrewUnit.UnitState != ECrewUnitState.Dead)))
                        {
                            slotToSwap = slot;
                            if (CurrentDepartment != null)
                            {
                                CurrentDepartment.ClearEfficiencyChange();
                            }
                        }
                    }
                }
            }
        }

        if (!hasSlot)
        {
            if (CurrentDepartment != null)
            {
                CurrentDepartment.ClearEfficiencyChange();
            }
            CurrentSlot = null;
            CurrentDepartment = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
        {
            Debug.Log($"{name} end ignored");
            return;
        }
        DragEnd();
    }

    public void Tick()
    {
        if (!runTimer)
        {
            return;
        }
        timeToPass--;
        if (timeToPass <= 0)
        {
            unit.IsCalculable = true;
            unit.CrewManager.PlayEvent(ECrewUIState.TimerFinish);
            CurrentDepartment.RecalculateEfficiency();
            runTimer = false;
            unit.UpdateCooldown(false);
            unit.CrewManager.FireRepositionFinished();
        }
    }

    public void ResetTimer()
    {
        if (!runTimer)
        {
            return;
        }
        timeToPass = 0;
        unit.IsCalculable = true;
        CurrentDepartment.RecalculateEfficiency();
        runTimer = false;
        unit.UpdateCooldown(false);
    }

    public void ArtificalDrop(ItemSlot slot)
    {
        if (CurrentDrag == this)
        {
            CurrentDrag = null;
        }

        prevDepartment = CurrentDepartment;
        if (CurrentSlot != null)
        {
            CurrentSlot.UnpackUnit();
            CurrentSlot.Highlight(false);
        }
        CurrentSlot = slot;
        lastSlot = slot;

        CurrentDepartment = slot.CurrentDepartment;
        if (prevDepartment != null)
        {
            prevDepartment.RecalculateEfficiency();
        }
        CurrentSlot.AssignUnit(unit);
        transform.SetParent(slot.CrewSlot);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.localPosition = slotOffset;
        unit.IsCalculable = false;
        runTimer = false;
        unit.UpdateCooldown(false);
        CurrentSlot.Highlight(true);

        unit.CrewManager.FireDepartmentsUnitsChanged();
        CrewManager.Instance.FindNewInjured();
    }

    public void SetDepartment(DepartmentItem department)
    {
        CurrentDepartment = department;
    }

    public void ForceEndDrag()
    {
        if (!canvasGroup.blocksRaycasts && !Input.GetMouseButton(0))
        {
            dragging = true;
        }
        OnEndDrag(null);
        if (lastPointerData.pointerDrag != null)
        {
            lastPointerData.pointerDrag = null;
        }
        eventSystem.SetSelectedGameObject(null, null);
    }

    public void RecalculateCooldown()
    {
        if (unit.CrewManager.InstantReassign)
        {
            timeToPass = 0;
            runTimer = false;
            unit.UpdateCooldown(false);
        }
        else
        {
            timeToPass = CrewManager.Instance.SkipMoveTime ? 1 : cooldownTime;
            unit.UpdateCooldown(true);
            TimeManager.Instance.AddTickable(this);
            runTimer = true;
        }
        unit.IsCalculable = !runTimer;
    }

    private void HighlightEmptySlots(bool highlight, bool isDead = false)
    {
        List<ItemSlot> slots = unit.CrewManager.AllSlots.FindAll(slot => slot.CurrentCrewUnit == null && slot.IsUsable);
        if (isDead)
        {
            slots = slots.FindAll(x => x.CurrentDepartment == null);
        }
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Highlight(highlight);
        }
    }

    private void EfficiencyChange()
    {
        if (unit.UnitState == ECrewUnitState.Dead || prevDepartment == CurrentDepartment)
        {
            if (prevDepartment != null)
            {
                prevDepartment.ClearEfficiencyChange();
            }
            if (CurrentDepartment != null)
            {
                CurrentDepartment.ClearEfficiencyChange();
            }
        }
        else
        {
            if (prevDepartment != null)
            {
                prevDepartment.ShowEfficiencyChange(unit, true);
            }
            if (CurrentDepartment != null)
            {
                CurrentDepartment.ShowEfficiencyChange(unit, false);
            }
        }
    }

    private void DragEnd()
    {
        if (dragging)
        {
            dragging = false;
        }
        else
        {
            return;
        }
        if (SlotBeforeDrag != null)
        {
            SlotBeforeDrag.CrewUnitBeforeDrag = null;
        }
        SlotBeforeDrag = null;
        CurrentDrag = null;
        if (disabled || ignore || !HudManager.Instance.HasNo(ETutorialMode.DisableCrewDrag))
        {
            ignore = false;
            unit.CrewManager.ClearBugged();
            return;
        }
        HighlightEmptySlots(false);
        unit.CrewManager.PlayEvent(CurrentSlot == null ? ECrewUIState.DragFail : ECrewUIState.DragSuccess);

        var prevSlot = lastSlot;
        bool noDrag = false;// !SectionRoomManager.Instance.Lockers.IsWorking;
        if (noDrag)
        {
            CurrentSlot = lastSlot;
            slotToSwap = null;
        }
        if (CurrentSlot == null)
        {
            CurrentSlot = lastSlot;
        }
        CurrentSlot.AssignUnit(unit);
        bool newDrag = CurrentSlot != lastSlot;
        if (CurrentDepartment != null)
        {
            CurrentDepartment.ClearEfficiencyChange();
        }
        if (prevDepartment != null)
        {
            prevDepartment.ClearEfficiencyChange();
            if (prevDepartment != CurrentSlot.CurrentDepartment)
            {
                prevDepartment.RecalculateEfficiency();
            }
        }
        CurrentDepartment = CurrentSlot.CurrentDepartment;
        rectTransform.SetParent(CurrentSlot.CrewSlot);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.localPosition = slotOffset;
        lastSlot = CurrentSlot;
        CurrentSlot.Highlight(true);

        if (CurrentDepartment != null && unit.UnitState != ECrewUnitState.Dead)
        {
            CurrentDepartment.ClearEfficiencyChange();
            if (prevDepartment != null && CurrentDepartment.Departments == prevDepartment.Departments)
            {
                if (timeToPass > 0 && !runTimer)
                {
                    unit.UpdateCooldown(true);
                    runTimer = true;
                }
                unit.CrewManager.ClearBugged();
                if (prevSlot == CurrentSlot && slotToSwap != null && slotToSwap.CurrentCrewUnit != null && slotToSwap.CurrentCrewUnit != unit)
                {
                    unit.CrewManager.SwapUnits(CurrentSlot, slotToSwap);
                    unit.CrewManager.ClearBugged();
                }
                return;
            }
            else
            {
                TimeManager.Instance.RemoveTickable(this);
            }
            RecalculateCooldown();
            CurrentDepartment.ClearEfficiencyChange();
            CurrentDepartment.RecalculateEfficiency();
        }
        else
        {
            if (unit.UnitState == ECrewUnitState.Dead)
            {
                unit.IsCalculable = false;
            }
            runTimer = false;
            unit.UpdateCooldown(false);
        }
        CurrentDrag = null;
        if (CurrentSlot.IsUsable)
        {
            unit.CrewManager.FireDepartmentsUnitsChanged();
            CrewManager.Instance.FindNewInjured();

            unit.CrewManager.ClearBugged();
            if (prevSlot == CurrentSlot && slotToSwap != null && slotToSwap.CurrentCrewUnit != null && slotToSwap.CurrentCrewUnit != unit && slotToSwap.IsUsable && slotToSwap.CurrentCrewUnit.UnitState != ECrewUnitState.Dead)
            {
                timeToPass = 0;
                runTimer = false;
                unit.UpdateCooldown(false);
                unit.IsCalculable = true;

                unit.CrewManager.SwapUnits(CurrentSlot, slotToSwap);
                unit.CrewManager.ClearBugged();
            }
        }
        else
        {
            var depart = CurrentDepartment;
            unit.CrewManager.DisableCrew(unit);
            depart.RecalculateEfficiency();
        }

        unit.CrewManager.ClearBugged();

        slotToSwap = null;
        if (newDrag)
        {
            unit.CrewManager.FireCrewDragged(unit);
        }
    }

    private void OnQuickSaved()
    {
        DragEnd();
    }
}
