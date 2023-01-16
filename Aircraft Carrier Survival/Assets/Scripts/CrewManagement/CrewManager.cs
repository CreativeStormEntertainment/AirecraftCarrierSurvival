using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class CrewManager : ParameterEventBase<ECrewUIState>, IPopupPanel, IEnableable, ITickable
{
    public event Action DepartmentsUnitsChanged = delegate { };
    public event Action<int> CrewDragged = delegate { };
    public event Action RepositionFinished = delegate { };
    public event Action<bool> CrewPanelOpened = delegate { };
    public event Action CrewDead = delegate { };

    public static CrewManager Instance;

    public List<CrewUnit> CrewUnits
    {
        get;
        private set;
    } = new List<CrewUnit>();

    public RectTransform MainRectTransform
    {
        get;
        set;
    }

    public int MaxDcCount
    {
        get => maxDcCount;
        set
        {
            if (value != maxDcCount)
            {
                int diff = Mathf.Abs(maxDcCount - value);
                if (maxDcCount < value)
                {
                    for (int i = 0; i < diff; ++i)
                    {
                        DamageControlManager.Instance.SpawnDC();
                    }
                }
                else
                {
                    for (int i = 0; i < diff; ++i)
                    {
                        DamageControlManager.Instance.DespawnDC();
                    }
                }
                maxDcCount = value;
            }
        }
    }

    public int CrewEnabled
    {
        get => crewEnabled;
        set
        {
            crewEnabled = value;
            for (int i = 0; i < CrewUnits.Count; i++)
            {
                CrewUnits[i].SetSelectedEnable((value & (1 << i)) != 0);
            }
        }
    }

    public EDepartmentsFlag DepartmentsEnabled
    {
        get;
        set;
    }

    public bool FreezeRescueTime
    {
        get;
        set;
    }

    public bool SkipMoveTime
    {
        get;
        set;
    }

    public List<ItemSlot> AllSlots => allSlots;
    public Canvas MainCanvas => mainCanvas;
    public CrewManagerTooltip Tooltip => tooltip;
    public Dictionary<ECrewmanSpecialty, CrewSpecialityData> CrewSpecialtiesDict => crewSpecialtiesDict;
    public string CrewSpecialityTitle => crewSpecialityTitle;
    public string OperationsDesc => operationsDesc;
    public string DeckDesc => deckDesc;
    public string AirDesc => airDesc;
    public string CommunicationsDesc => communicationsDesc;
    public string EngineeringDesc => engineeringDesc;
    public string NavigationsDesc => navigationsDesc;
    public string LoneWolfDesc => loneWolfDesc;
    public string TeamPlayerDesc => teamPlayerDesc;
    public string MedicalDesc => medicalDesc;
    public string AntiAirDesc => antiAirDesc;
    public string GeneralDesc => generalDesc;
    public CrewDataList CrewDataList => crewData;
    public EWindowType Type => EWindowType.CrewManagement;
    public List<ListInt> AaUpgrades => aaUpgrades;
    public int SpecialityBonus => specialityBonus;
    public int AloneDepartBonus => aloneDepartBonus;
    public int FullDepartBonus => fullDepartBonus;
    public int GeneralistBonus => generalistBonus;

    public Dictionary<EDepartments, DepartmentItem> DepartmentDict;
    public bool CanBoost;
    public bool InstantReassign;
    public List<EDepartments> startSlotAssign = new List<EDepartments>();

    [SerializeField]
    private Transform idleSlotsRoot = null;

    [SerializeField]
    private Transform departmentsParent = null;
    [SerializeField]
    private GameObject crewItemPrefab = null;
    [SerializeField]
    private Canvas mainCanvas = null;
    [SerializeField]
    private List<DepartmentItem> departments = null;

    [SerializeField]
    private CrewManagerTooltip tooltip = null;

    [SerializeField]
    private int AACooldownTicks = 30;

    [SerializeField]
    private int specialityBonus = 7;

    [SerializeField]
    private int aloneDepartBonus = 20;

    [SerializeField]
    private int fullDepartBonus = 10;

    [SerializeField]
    private int generalistBonus = 4;

    [SerializeField]
    private int bronzeBonus = 5;

    [SerializeField]
    private int silverGoldBonus = 7;

    [SerializeField]
    private string crewSpecialityTitle = "PP134.1";
    [SerializeField]
    private string operationsDesc = "PP134.1Desc";
    [SerializeField]
    private string deckDesc = "PP134.2Desc";
    [SerializeField]
    private string airDesc = "PP134.3Desc";
    [SerializeField]
    private string communicationsDesc = "PP134.4Desc";
    [SerializeField]
    private string engineeringDesc = "PP134.5Desc";
    [SerializeField]
    private string navigationsDesc = "PP134.6Desc";
    [SerializeField]
    private string loneWolfDesc = "PP134.7Desc";
    [SerializeField]
    private string teamPlayerDesc = "PP134.8Desc";
    [SerializeField]
    private string generalDesc = "PP134.9Desc";
    [SerializeField]
    private string medicalDesc = "NPP001Desc";
    [SerializeField]
    private string antiAirDesc = "NPP002Desc";

    [SerializeField]
    private List<CrewEfficiencyData> efficiencyDatas = null;

    [SerializeField]
    private GameObject panel = null;
    [SerializeField]
    private Button closeButton = null;
    [SerializeField]
    private Transform timeButtons = null;

    [SerializeField]
    private CrewDataList crewData = null;

    [SerializeField]
    private List<ListInt> aaUpgrades = null;

    private Dictionary<ECrewmanSpecialty, CrewSpecialityData> crewSpecialtiesDict = new Dictionary<ECrewmanSpecialty, CrewSpecialityData>();

    private int healingSlots;

    private List<ItemSlot> idleSlots = new List<ItemSlot>();

    private List<ItemSlot> allSlots = new List<ItemSlot>();
    private List<ItemSlot> itemSlots = new List<ItemSlot>();

    private int maxDcCount = 0;

    private int silence;

    private Transform originalTimeButtonsParent;
    private int crewEnabled;

    protected override void Awake()
    {
        base.Awake();

        Instance = this;
        foreach (var data in CrewDataList.CrewSpecialtyDatas)
        {
            crewSpecialtiesDict.Add(data.Speciality, data);
        }
        allSlots = new List<ItemSlot>();
        for (int i = 0; i < idleSlotsRoot.childCount; ++i)
        {
            var s = idleSlotsRoot.GetChild(i).GetComponent<ItemSlot>();
            idleSlots.Add(s);
            allSlots.Add(s);
            s.Highlight(false);
        }

        foreach (Transform child in departmentsParent)
        {
            DepartmentItem dep = child.GetComponent<DepartmentItem>();
            if (dep != null)
            {
                dep.SetupBonuses(specialityBonus, aloneDepartBonus, fullDepartBonus, generalistBonus, bronzeBonus, silverGoldBonus);
                departments.Add(dep);
            }
        }

        DepartmentDict = new Dictionary<EDepartments, DepartmentItem>();
        foreach (var department in departments)
        {
            DepartmentDict[department.Departments] = department;
        }

        foreach (var data in efficiencyDatas)
        {
            DepartmentDict[data.Department].Data = data;
        }

        healingSlots = 0;

        originalTimeButtonsParent = timeButtons.parent;
        closeButton.onClick.AddListener(() => SetShow(false));

        crewEnabled = -1;
        DepartmentsEnabled = (EDepartmentsFlag)(-1);
    }

    private void Start()
    {
        var locMan = LocalizationManager.Instance;
        crewSpecialityTitle = locMan.GetText(crewSpecialityTitle);
    }

    public void SetEnable(bool enable)
    {
        foreach (var unit in CrewUnits)
        {
            unit.SetEnable(enable);
        }
    }

    public void Setup(SOTacticMap map)
    {
        WorldMap.Instance.Toggled += OnWorldMapToggle;
        TimeManager.Instance.AddTickable(this);
        MainRectTransform = MainCanvas.GetComponent<RectTransform>();
        foreach (DepartmentItem department in departments)
        {
            department.Setup();
            for (int i = 0; i < department.UnitsSlots.Length; ++i)
            {
                allSlots.Add(department.UnitsSlots[i]);
                department.UnitsSlots[i].Highlight(false);
            }
        }
        AssignSlots(map);
        foreach (DepartmentItem department in departments)
        {
            department.RecalculateEfficiency();
        }
    }

    public void Tick()
    {
        //bool clearBugged = false;
        //foreach (var crew in CrewUnits)
        //{
        //    if (crew.DragDrop.SlotBeforeDrag != null)
        //    {
        //        clearBugged = true;
        //        break;
        //    }
        //}
        //if (clearBugged)
        //{
        //    ClearBugged();
        //}
        //foreach (var crew in CrewUnits)
        //{
        //    if (crew.DragDrop.SlotBeforeDrag != null)
        //    {
        //        crew.DragDrop.OnEndDrag(null);
        //        break;
        //    }
        //}
    }

    public void LoadData(ref CrewManagerSaveData data)
    {
        for (int i = 0; i < CrewUnits.Count; i++)
        {
            CrewUnits[i].LoadData(data.Crew[i]);
        }
        CrewStatusManager.Instance.OnLoad();
        foreach (var department in DepartmentDict.Values)
        {
            department.RecalculateEfficiency();
        }

        DepartmentDict[EDepartments.AA].LoadData(data.AACooldown);
    }

    public void SaveData(ref CrewManagerSaveData data)
    {
        data.Crew.Clear();
        foreach (var crew in CrewUnits)
        {
            data.Crew.Add(crew.SaveData());
        }

        DepartmentDict[EDepartments.AA].SaveData(data.AACooldown);
    }

    public void SaveLosses(ref MissionRewards data, ref IntermissionData intermissionData)
    {
        for (int i = CrewUnits.Count - 1; i >= 0; i--)
        {
            if (CrewUnits[i].UnitState == ECrewUnitState.Dead)
            {
                data.CrewsUpgrades.RemoveAt(i);
                intermissionData.CrewData.Selected.Remove(i);
                for (int j = 0; j < intermissionData.CrewData.Selected.Count; j++)
                {
                    if (intermissionData.CrewData.Selected[j] > i)
                    {
                        intermissionData.CrewData.Selected[j]--;
                    }
                }
            }
        }
    }

    public int IndexOf(CrewUnit unit)
    {
        int result = CrewUnits.IndexOf(unit);
        Assert.IsFalse(result == -1);
        return result;
    }

    public CrewUnit GetCrew(int index)
    {
        return CrewUnits[index];
    }

    public void SetShow(bool show)
    {
        panel.SetActive(show);
        var hudMan = HudManager.Instance;
        hudMan.SetAcceptInput(true);
        if (show)
        {
            timeButtons.SetParent(panel.transform);
            hudMan.PopupShown(this);
            CrewPanelOpened(true);
        }
        else
        {
            timeButtons.SetParent(originalTimeButtonsParent);
            hudMan.PopupHidden(this);
            CrewPanelOpened(false);
        }
    }

    public bool Shown()
    {
        return panel.activeSelf;
    }

    public void ToggleShow()
    {
        SetShow(!Shown());
    }

    public void RefreshHealSlots(int slots)
    {
        healingSlots = slots;
        for (int i = 0; i < idleSlots.Count; i++)
        {
            idleSlots[i].IsHealing = i < healingSlots;
        }
        FindNewInjured();
    }

    public bool GetCrewState()
    {
        var deadCount = 0;
        foreach (var crew in CrewUnits)
        {
            if (crew.UnitState == ECrewUnitState.Dead)
            {
                deadCount++;
                if (deadCount >= (CrewUnits.Count * 0.75f))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void OpenTutorial()
    {
        MovieManager.Instance.Play(ETutorialType.CrewManagement);
    }

    public void StartAACooldown()
    {
        DepartmentDict[EDepartments.AA].AAInCooldown(AACooldownTicks - DepartmentDict[EDepartments.AA].EfficiencyMinutes);
    }

    public void ChangeAACooldown(int ticks)
    {
        DepartmentDict[EDepartments.AA].ChangeAACooldown(ticks);
    }

    public void UpdateAADefence()
    {
        DepartmentDict[EDepartments.AA].RecalculateEfficiency();
    }

    public void FireDepartmentsUnitsChanged()
    {
        DepartmentsUnitsChanged();
    }

    public new void PlayEvent(ECrewUIState paramValue)
    {
        if (paramValue != ECrewUIState.Hover || silence < 1)
        {
            if (paramValue != ECrewUIState.Hover)
            {
                silence++;
                this.StartCoroutineActionAfterFrames(() => silence--, 3);
            }
            base.PlayEvent(paramValue);
        }
    }

    public void FindNewInjured()
    {
        for (int i = 0; i < healingSlots; i++)
        {
            if (idleSlots[i].CrewUnitBeforeDrag == null &&
                (idleSlots[i].CurrentCrewUnit == null || (idleSlots[i].CurrentCrewUnit.UnitState != ECrewUnitState.Injured && DragDrop.CurrentDrag != idleSlots[i].CurrentCrewUnit.DragDrop)))
            {
                if (!FindNewInjured(idleSlots[i]))
                {
                    break;
                }
            }
        }
    }

    public void FindNewInjured(CrewUnit freshlyHealed)
    {
        FindNewInjured(freshlyHealed.DragDrop.CurrentSlot);
    }

    public IEnumerable<CrewUnit> GetHealthyAACrew()
    {
        var aa = DepartmentDict[EDepartments.AA];
        for (int i = 0; i < aa.UnitsSlots.Length; i++)
        {
            if (aa.UnitsSlots[i].CurrentCrewUnit != null && aa.UnitsSlots[i].CurrentCrewUnit.IsCalculable)
            {
                yield return aa.UnitsSlots[i].CurrentCrewUnit;
            }
        }
    }

    public void DisableCrew(CrewUnit crewUnit)
    {
        if (crewUnit.CanArtificalDrop())
        {
            for (int i = 0; i < itemSlots.Count; ++i)
            {
                if (itemSlots[i].CrewUnitBeforeDrag == null && itemSlots[i].CurrentCrewUnit == null)
                {
                    crewUnit.ArtificalDrop(itemSlots[i]);
                    break;
                }
            }
        }
    }

    public void ClearBugged()
    {
        for (int i = 0; i < allSlots.Count; ++i)
        {
            if (allSlots[i].CurrentCrewUnit != null && allSlots[i].CrewSlot.childCount == 0)
            {
                allSlots[i].AssignUnit(null);
                allSlots[i].Highlight(false);
            }
        }
    }

    public void SwapUnits(ItemSlot first, ItemSlot second)
    {
        var crew1 = first.CurrentCrewUnit;
        var crew2 = second.CurrentCrewUnit;

        first.UnpackUnit();
        first.Highlight(false);
        second.UnpackUnit();
        second.Highlight(false);
        if (crew1 != null)
        {
            DropCrew(crew1, first, second);
        }
        if (crew2 != null)
        {
            DropCrew(crew2, second, first);
        }
    }

    public void Hide()
    {
        SetShow(false);
    }

    public void SaveCrewmenPositions()
    {
        var data = SaveManager.Instance.Data;
        data.CrewsSlots.Clear();
        for (int i = 0; i < CrewUnits.Count; ++i)
        {
            var slotData = new CrewSlotData();

            var dept = CrewUnits[i].DragDrop.CurrentDepartment;
            var slot = CrewUnits[i].DragDrop.CurrentSlot;
            if (dept == null)
            {
                slotData.Department = EDepartments.Count;
                slotData.Slot = allSlots.IndexOf(slot);
            }
            else
            {
                slotData.Department = dept.Departments;
                slotData.Slot = -1;
                for (int j = 0; j < dept.UnitsSlots.Length; j++)
                {
                    if (slot == dept.UnitsSlots[j])
                    {
                        slotData.Slot = j;
                        break;
                    }
                }
            }

            data.CrewsSlots.Add(slotData);
        }
    }

    public void FireCrewDragged(CrewUnit unit)
    {
        CrewDragged(CrewUnits.IndexOf(unit));
    }

    public void FireRepositionFinished()
    {
        foreach (var unit in CrewUnits)
        {
            if (!unit.IsCalculable)
            {
                return;
            }
        }
        RepositionFinished();
    }

    public void FireCrewDead()
    {
        CrewDead();
    }

    public void UpdateSkills()
    {
        var saveData = SaveManager.Instance.Data;
        for (int i = 0; i < saveData.IntermissionData.CrewData.Selected.Count; i++)
        {
            if (saveData.IntermissionData.CrewData.Selected[i] != -1)
            {
                int crewIndex = saveData.IntermissionData.CrewData.Selected[i];
                var crewUpgrade = saveData.MissionRewards.CrewsUpgrades[crewIndex];
                var savedSpecialities = new List<ECrewmanSpecialty>(crewUpgrade.GetSpecialties());
                CrewUnits[i].SetSkill(savedSpecialities);
            }
        }
    }

    private bool FindNewInjured(ItemSlot healingSlot)
    {
        for (int i = healingSlots; i < idleSlots.Count; ++i)
        {
            if (idleSlots[i].CrewUnitBeforeDrag != null || idleSlots[i].CurrentCrewUnit == null)
            {
                continue;
            }
            else if (idleSlots[i].CurrentCrewUnit.UnitState == ECrewUnitState.Injured && DragDrop.CurrentDrag != idleSlots[i].CurrentCrewUnit.DragDrop)
            {
                SwapUnits(idleSlots[i], healingSlot);
                return true;
            }
        }
        return false;
    }

    private void AssignSlots(SOTacticMap map)
    {
        int i;
        var saveData = SaveManager.Instance.Data;
        var overridenCrewData = map.Overrides.CrewData;
        if (overridenCrewData == null)
        {            
            for (i = 0; i < saveData.IntermissionData.CrewData.Selected.Count; i++)
            {
                if (saveData.IntermissionData.CrewData.Selected[i] != -1)
                {
                    int crewIndex = saveData.IntermissionData.CrewData.Selected[i];
                    var crewUpgrade = saveData.MissionRewards.CrewsUpgrades[crewIndex];
                    var savedSpecialities = new List<ECrewmanSpecialty>(crewUpgrade.GetSpecialties());

                    SetupCrew(idleSlots[i], crewIndex, crewUpgrade.CrewDataIndex, savedSpecialities, i < saveData.CrewsSlots.Count ? saveData.CrewsSlots[i] : new CrewSlotData() { Slot = -1 });
                }
            }
        }
        else
        {
            for (i = 0; i < overridenCrewData.Count; i++)
            {
                var data = overridenCrewData[i];
                var slotData = new CrewSlotData();
                slotData.Department = data.Location;
                slotData.Slot = -1;
                if (slotData.Department == EDepartments.Count)
                {
                    slotData.Slot = i;
                }
                else
                {
                    var slots = DepartmentDict[slotData.Department].UnitsSlots;
                    for (int j = 0; j < slots.Length; j++)
                    {
                        if (slots[j].CurrentCrewUnit == null)
                        {
                            slotData.Slot = j;
                            break;
                        }
                    }
                }
                if (slotData.Slot == -1)
                {
                    Debug.LogError("Not enough slots in " + slotData.Department);
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                }
                var list = new List<ECrewmanSpecialty>();
                if (data.Specialty != ECrewmanSpecialty.None)
                {
                    list.Add(data.Specialty);
                }
                SetupCrew(idleSlots[i], i, i % crewData.List.Count, list, slotData);
            }
        }
        for (; i < idleSlots.Count; i++) 
        {
            idleSlots[i].gameObject.SetActive(false);
        }

        CrewStatusManager.Instance.Init(this);
    }

    private void SetupCrew(ItemSlot slot, int index, int crewDataIndex, List<ECrewmanSpecialty> specialty, CrewSlotData slotData)
    {
        GameObject g = Instantiate(crewItemPrefab, transform, false);

        itemSlots.Add(slot);

        CrewUnit unit = g.GetComponent<CrewUnit>();

        unit.SetSkill(specialty);
        var crewmanData = crewData.List[crewDataIndex];

        ItemSlot ss = null;
        if (slotData.Slot != -1)
        {
            if (slotData.Department == EDepartments.Count)
            {
                if (idleSlotsRoot.childCount > slotData.Slot)
                {
                    ss = allSlots[slotData.Slot];
                }
            }
            else
            {
                var slots = DepartmentDict[slotData.Department].UnitsSlots;
                if (slots.Length > slotData.Slot)
                {
                    ss = slots[slotData.Slot];
                    if (ss.CurrentCrewUnit == null)
                    {
                        unit.SetDepartment(ss.CurrentDepartment);
                    }
                    else
                    {
                        ss = null;
                    }
                }
            }
        }

        if (ss == null)
        {
            if (index < startSlotAssign.Count)
            {
                ss = DepartmentDict[startSlotAssign[index]].GetFreeSlot();
                unit.SetDepartment(DepartmentDict[startSlotAssign[index]]);
            }
            if (ss == null)
            {
                ss = RandomSlot(out var depart);
                unit.SetDepartment(depart);
            }
        }
        ss.AssignUnit(unit);
        ss.Highlight(true);
        unit.IsCalculable = true;

        var trans = g.transform;
        trans.SetParent(ss.CrewSlot);
        trans.localPosition = unit.DragDrop.SlotOffset;
        trans.localScale = Vector3.one;

        unit.Setup(mainCanvas, ss, crewmanData.Portrait, crewmanData.GreySprite, this);
        CrewUnits.Add(unit);
    }

    private ItemSlot RandomSlot(out DepartmentItem department)
    {
        List<List<ItemSlot>> slots = new List<List<ItemSlot>>();
        List<DepartmentItem> departments = new List<DepartmentItem>();
        for (EDepartments i = EDepartments.Medical; i < EDepartments.AA; ++i)
        {
            List<ItemSlot> free = DepartmentDict[i].GetFreeSlots();
            if (free.Count > 0)
            {
                slots.Add(free);
                departments.Add(DepartmentDict[i]);
            }
        }
        int rand = UnityEngine.Random.Range(0, slots.Count);
        department = departments[rand];
        return slots[rand][UnityEngine.Random.Range(0, slots[rand].Count)];
    }

    private void DropCrew(CrewUnit crew, ItemSlot from, ItemSlot to)
    {
        crew.DragDrop.CurrentSlot = null;
        crew.ArtificalDrop(to);
        crew.DragDrop.CurrentSlot.Highlight(true);
        if (to.CurrentDepartment != null)
        {
            if (from.CurrentDepartment != to.CurrentDepartment)
            {
                crew.DragDrop.RecalculateCooldown();
            }
            to.CurrentDepartment.ClearEfficiencyChange();
            to.CurrentDepartment.RecalculateEfficiency();
        }
    }

    private void OnWorldMapToggle(bool open)
    {
        SetEnable(!open);
        if (open)
        {
            DepartmentDict[EDepartments.AA].ClearCooldown();
            foreach (var crew in CrewUnits)
            {
                crew.DragDrop.ResetTimer();
            }
            UpdateSkills();
        }
    }
}