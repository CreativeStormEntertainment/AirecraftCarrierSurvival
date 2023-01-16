using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepartmentItem : MonoBehaviour, ITickable
{
    public int UnitsCount
    {
        get;
        private set;
    } = 0;

    public bool IsBoosted
    {
        get;
        set;
    }

    public bool IsSelected
    {
        get;
        set;
    }

    public float EfficiencyBonus
    {
        get;
        private set;
    }

    public CrewEfficiencyData Data
    {
        get;
        set;
    }

    public ItemSlot[] UnitsSlots => unitsSlots;
    public EDepartments Departments => department;
    public int MaxUnitsCount => UnitsSlots.Length;

    public int EfficiencyMinutes => Mathf.RoundToInt(EfficiencyBonus / 60f * TimeManager.Instance.TicksForHour);
    public float EfficiencyPercent => currentEfficiency / 100f;

    [SerializeField]
    private CrewManager crewManager = null;
    [SerializeField]
    private EDepartments department = EDepartments.Medical;
    [SerializeField]
    private Text efficiencyText = null;
    [SerializeField]
    private int baseEfficiency = 100;
    [SerializeField]
    private int currentEfficiency = 0;

    [SerializeField]
    private ItemSlot[] unitsSlots = null;

    private bool cooldown;

    private int specialityBonus = 7;
    private int aloneDepartBonus = 20;
    private int fullDepartBonus = 10;
    private int generalistBonus = 4;
    private int bronzeBonus = 5;
    private int silverAndGoldBonus = 7;

    private bool firstTime = true;

    private List<SectionRoom> departmentSections;
    private List<int> segmentCount;

    public void Setup()
    {
        departmentSections = new List<SectionRoom>();

        var sectionMan = SectionRoomManager.Instance;
        switch (department)
        {
            case EDepartments.Medical:
                departmentSections.Add(sectionMan.Sickbay);
                break;
            case EDepartments.Deck:
                departmentSections.Add(sectionMan.Deck);
                break;
            case EDepartments.Air:
                departmentSections.Add(sectionMan.Pilots);
                break;
            case EDepartments.Engineering:
                departmentSections.Add(sectionMan.AircraftWorkshop);
                break;
            case EDepartments.Navigation:
                departmentSections.Add(sectionMan.Engines);
                break;
            case EDepartments.AA:
                departmentSections.Add(sectionMan.AAGuns);
                if (sectionMan.AAGuns2 != null)
                {
                    departmentSections.Add(sectionMan.AAGuns2);
                }
                break;
        }
        TimeManager.Instance.AddTickable(this);

        segmentCount = new List<int>();
        foreach (var departmentSection in departmentSections)
        {
            int count = 0;
            foreach (var segment in departmentSection.GetAllSegments(true))
            {
                count++;
            }
            segmentCount.Add(count);
        }

        unitsSlots = GetComponentsInChildren<ItemSlot>();
        RecalculateEfficiency();
        if (department == EDepartments.AA)
        {
            var data = SaveManager.Instance.Data;
            int slots = crewManager.AaUpgrades[(int)data.SelectedAircraftCarrier].List[BinUtils.ExtractData(data.IntermissionData.CarriersUpgrades, 3, (int)data.SelectedAircraftCarrier + (int)ECarrierType.Count)];
            for (int i = slots; i < unitsSlots.Length; i++)
            {
                unitsSlots[i].gameObject.SetActive(false);
            }
            unitsSlots = GetComponentsInChildren<ItemSlot>();
        }
        for (int i = 0; i < unitsSlots.Length; i++)
        {
            unitsSlots[i].Setup(this);
        }

        IslandsAndOfficersManager.Instance.DepartmentBoostUpdated += (EDepartments depart, int boost) =>
        {
            if (depart == department)
            {
                RecalculateEfficiency();
            }
        };
    }

    public void LoadData(List<int> cooldowns)
    {
        cooldown = cooldowns.Count > 0;
        if (cooldown)
        {
            for (int i = 0; i < unitsSlots.Length; i++)
            {
                unitsSlots[i].Cooldown = cooldowns[i];
            }
            RecalculateEfficiency();
        }
    }

    public void SaveData(List<int> cooldowns)
    {
        cooldowns.Clear();
        if (cooldown)
        {
            foreach (var slot in unitsSlots)
            {
                if (slot.Cooldown > 0)
                {
                    cooldowns.Add(slot.Cooldown);
                }
                else
                {
                    cooldowns.Add(-1);
                }
            }
        }
    }

    public void Tick()
    {
        CheckSection();
        if (cooldown)
        {
            bool changed = false;
            cooldown = false;
            foreach (var slot in unitsSlots)
            {
                if (slot.Cooldown > 0)
                {
                    slot.Cooldown--;
                    changed = true;
                    if (slot.Cooldown > 0)
                    {
                        cooldown = true;
                    }
                }
            }
            if (changed)
            {
                RecalculateEfficiency();
            }
        }
    }

    public void ClearCooldown()
    {
        cooldown = false;
        foreach (var slot in unitsSlots)
        {
            slot.Cooldown = 0;
        }
        RecalculateEfficiency();
    }

    public void AAInCooldown(int ticks)
    {
        cooldown = true;
        foreach (var slot in unitsSlots)
        {
            if (slot.CurrentCrewUnit != null && slot.CurrentCrewUnit.IsCalculable && slot.Cooldown <= 0)
            {
                slot.Cooldown = ticks;
            }
        }
        RecalculateEfficiency();
    }

    public void ChangeAACooldown(int ticks)
    {
        foreach (var slot in unitsSlots)
        {
            if (slot.Cooldown > 0)
            {
                slot.Cooldown = ticks;
            }
        }
    }

    public void RecalculateEfficiency(bool test = false)
    {
        UnitsCount = 0;

        currentEfficiency = baseEfficiency + IslandsAndOfficersManager.Instance.GetCurrentDepartmentBoost(department);
        int fullSectionSkills = 0;
        bool hasAloneSectionSkill = false;
        int defencePoints = 0;
        for (int i = 0; i < unitsSlots.Length; i++)
        {
            var unit = unitsSlots[i].CurrentCrewUnit == null ? unitsSlots[i].CrewUnitBeforeDrag : unitsSlots[i].CurrentCrewUnit;
            if (unit)
            {
                if (unit.IsCalculable)
                {
                    UnitsCount++;
                    if (department == EDepartments.AA && unitsSlots[i].Cooldown <= 0)
                    {
                        defencePoints++;
                    }
                    currentEfficiency += unit.Skills.Count == 0 ? 5 : 7;
                    foreach (var skill in unit.Skills)
                    {
                        bool departmentBonus = false;
                        bool fullSectionSkill = false;
                        switch (skill)
                        {
                            case ECrewmanSpecialty.DepartmentSolo:
                                hasAloneSectionSkill = true;
                                break;
                            case ECrewmanSpecialty.DepartmentFull:
                                fullSectionSkill = true;
                                break;
                            case ECrewmanSpecialty.GeneralBoost:
                                    currentEfficiency += generalistBonus;
                                break;
                            default:
                                if (((int)skill - 1) == (int)Departments)
                                {
                                    departmentBonus = true;
                                }
                                break;
                        }
                        if (departmentBonus)
                        {
                            currentEfficiency += specialityBonus;
                        }
                        if (fullSectionSkill)
                        {
                            ++fullSectionSkills;
                        }
                    }
                }
            }
        }
        if (department == EDepartments.AA)
        {
            var enemyAttacksMan = EnemyAttacksManager.Instance;
            enemyAttacksMan.SetCrewManagerDefencePoints(defencePoints);
            enemyAttacksMan.SetCrewManagerEscortPoints(defencePoints);
        }

        if (UnitsCount == unitsSlots.Length && fullSectionSkills > 0)
        {
            currentEfficiency += fullSectionSkills * fullDepartBonus;
            //Debug.Log("Department " + Departments.ToString() + " x"+ fullSectionSkills+" full depart skill activated");
        }
        else if (UnitsCount == 1 && hasAloneSectionSkill)
        {
            currentEfficiency += aloneDepartBonus;
            //Debug.Log("Department " + Departments.ToString() + " alone depart skill activated");
        }

        switch (department)
        {
            case EDepartments.Medical:
                crewManager.RefreshHealSlots(UnitsCount);
                //CrewStatusManager.Instance.SetBonus(1f - EfficiencyPercent);
                break;
            case EDepartments.Deck:
                crewManager.MaxDcCount = UnitsCount;
                break;
            case EDepartments.Air:
                //AircraftCarrierDeckManager.Instance.SetBonusMaxSlots(UnitsCount);
                DeckOrderPanelManager.Instance.UpdateOrders();
                var deck = AircraftCarrierDeckManager.Instance;
                if ((deck.DeckSquadrons.Count > deck.MaxSlots) && deck.OrderQueue.Count == 0)
                {
                    deck.ForceGoToHangar();
                }
                break;
            case EDepartments.Engineering:
                break;
            case EDepartments.Navigation:
                HudManager.Instance.ChangeMaxSpeed(UnitsCount + 2, !firstTime);
                if (firstTime)
                {
                    firstTime = false;
                }
                break;
            case EDepartments.AA:
                break;
        }

        efficiencyText.text = string.Format("{0}%", currentEfficiency);
        //tooltip.SetParameter(currentEfficiency.ToString());

        float extraEfficiency = currentEfficiency - 100f;
        EfficiencyBonus = (Data.BaseValue - (extraEfficiency / Data.EfficiencyDivisor)) * extraEfficiency;
    }

    public void ShowEfficiencyChange(CrewUnit unit, bool decrease)
    {
        if (unit.UnitState == ECrewUnitState.Dead)
        {
            ClearEfficiencyChange();
            return;
        }

        int bonus = 0;
        foreach (var skill in unit.Skills)
        {
            bool aloneSkill = false;
            int fullDepartCount = 0;
            if (((int)skill - 1) == (int)Departments)
            {
                bonus += specialityBonus;
            }
            if (skill == ECrewmanSpecialty.DepartmentSolo)
            {
                aloneSkill = true;
            }
            if (skill == ECrewmanSpecialty.DepartmentFull)
            {
                fullDepartCount = 1;
            }
            if (skill == ECrewmanSpecialty.GeneralBoost)
            {
                bonus += generalistBonus;
            }

            int emptyCount = 0;
            bool hasUnit = false;
            for (int i = 0; i < unitsSlots.Length; i++)
            {
                if (unitsSlots[i].CurrentCrewUnit == null)
                {
                    emptyCount++;
                }
                else
                {
                    if (unitsSlots[i].CurrentCrewUnit == unit)
                    {
                        hasUnit = true;
                    }
                    else
                    {
                        aloneSkill = false;
                        if (unitsSlots[i].CurrentCrewUnit.Skills.Contains(ECrewmanSpecialty.DepartmentFull))
                        {
                            fullDepartCount++;
                        }
                    }
                }
            }
            if (aloneSkill)
            {
                bonus += aloneDepartBonus;
            }
            if (emptyCount == 0 || (emptyCount == 1 && !hasUnit))
            {
                bonus += fullDepartCount * fullDepartBonus;
            }
        }

        string color = decrease ? "red" : "green";
        string sign = decrease ? "-" : "+";
        efficiencyText.text = string.Format("{0}%<color={1}>{2}{3}</color>", currentEfficiency, color, sign, bonus);
    }

    public void ClearEfficiencyChange()
    {
        efficiencyText.text = string.Format("{0}%", currentEfficiency);
    }

    /*public void OnPointerClick(PointerEventData pointerEventData)
    {
        crewManager.SelectDepartment(this);
    }

    public void Select()
    {
        selectionIndicator.SetActive(true);
        isSelected = true;
    }

    public void Deselect()
    {
        selectionIndicator.SetActive(false);
        isSelected = false;
    }*/

    public void Boost(Toggle toggle)
    {
        if (!IsSelected)
        {
            return;
        }
        IsBoosted = toggle.isOn && CrewManager.Instance.CanBoost;
        RecalculateEfficiency();
    }

    public ItemSlot GetFreeSlot()
    {
        for (int i = 0; i < unitsSlots.Length; ++i)
        {
            if (unitsSlots[i].CurrentCrewUnit == null)
            {
                return unitsSlots[i];
            }
        }
        return null;
    }

    public List<ItemSlot> GetFreeSlots()
    {
        List<ItemSlot> list = new List<ItemSlot>();
        for (int i = 0; i < unitsSlots.Length; ++i)
        {
            if (unitsSlots[i].CurrentCrewUnit == null)
            {
                list.Add(unitsSlots[i]);
            }
        }
        return list;
    }

    public void SetupBonuses(int specialityBonus, int aloneDepartBonus, int fullDepartBonus, int generalistBonus, int bronzeBonus, int silverGoldBonus)
    {
        this.specialityBonus = specialityBonus;
        this.aloneDepartBonus = aloneDepartBonus;
        this.fullDepartBonus = fullDepartBonus;
        this.generalistBonus = generalistBonus;
        this.bronzeBonus = bronzeBonus;
        this.silverAndGoldBonus = silverGoldBonus;
    }

    private void CheckSection()
    {
        bool damagedAll = true;
        bool damagedMore = true;
        bool damagedOne = true;
        foreach (var departmentSection in departmentSections)
        {
            int damagedCount = 0;
            foreach (var segment in departmentSection.GetAllSegments(true))
            {
                if (segment.HasAnyDamageWithBroken())
                {
                    damagedCount++;
                }
                else
                {
                    damagedAll = false;
                }
            }
            if (damagedCount == 0)
            {
                damagedOne = false;
            }
            if (damagedCount < 2)
            {
                damagedMore = false;
            }
        }
        int damagedSlots;
        if (damagedAll)
        {
            damagedSlots = unitsSlots.Length;
        }
        else if (damagedMore)
        {
            damagedSlots = (unitsSlots.Length + 1) / 2;
        }
        else if (damagedOne)
        {
            damagedSlots = 1;
        }
        else
        {
            damagedSlots = 0;
        }

        if (department != EDepartments.Engineering)
        {
            damagedSlots -= crewManager.DepartmentDict[EDepartments.Engineering].UnitsCount;
        }

        bool removedCrew = false;
        for (int i = unitsSlots.Length; i > 0; i--)
        {
            if (damagedSlots-- > 0)
            {
                unitsSlots[i - 1].SetPadlock(true);
                if (unitsSlots[i - 1].CurrentCrewUnit != null)
                {
                    removedCrew = true;
                    crewManager.DisableCrew(unitsSlots[i - 1].CurrentCrewUnit);
                }
                unitsSlots[i - 1].Highlight(false);
            }
            else
            {
                unitsSlots[i - 1].SetPadlock(false);
            }
        }
        if (removedCrew)
        {
            RecalculateEfficiency();
        }
    }
}
