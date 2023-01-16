using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;

public class StrikeGroupManager : MonoBehaviour, ITickable
{
    public event Action ShipSunk = delegate { };
    public event Action NeutralShipDestroyed = delegate { };
    public event Action CustomEscortDestroyed = delegate { };
    public event Action<EStrikeGroupActiveSkill> StrikeGroupActiveSkillUsed = delegate { };

    public static StrikeGroupManager Instance;

    public StrikeGroupData Data;

    public StrikeGroupData CustomData;

    public List<int> Chosen = new List<int>();

    public HashSet<StrikeGroupMember> AliveMembers => aliveMembers;
    public Dictionary<StrikeGroupMemberData, StrikeGroupEffectData> CurrentActiveSkills => currentActiveSkills;
    public StrikeGroupMember CurrentlyActivatingSkill
    {
        get => currentlyActivatingSkill;
        set
        {
            if (value == null && currentlyActivatingSkill != null)
            {
                currentlyActivatingSkill.Button.SetButtonSprite(EButtonStates.Normal);
            }
            else
            {
                value.Button.SetButtonSprite(EButtonStates.Pressed);
            }
            currentlyActivatingSkill = value;
        }
    }

    public int EnabledShips
    {
        get;
        set;
    }

    public float EscortCooldownModifier
    {
        get;
        private set;
    } = 1f;

    [SerializeField]
    private StrikeGroupButton shipButtonPrefab = null;

    [SerializeField]
    private Transform escortPanelTransform = null;

    [SerializeField]
    private Transform modelSlotsRoot = null;
    [SerializeField]
    private StrikeGroupActiveSkillSelection strikeGroupActiveSkillSelection = null;

    [SerializeField]
    private GameObject anotherCarrier = null;
    [SerializeField]
    private GameObject director = null;

    private List<Transform> slots;

    private List<StrikeGroupMember> members = new List<StrikeGroupMember>();
    private HashSet<StrikeGroupMember> aliveMembers;

    private int defenceBonus = 0;
    private int escortBonus = 0;

    private List<StrikeGroupButton> currentButtons;

    private Dictionary<StrikeGroupMemberData, StrikeGroupEffectData> currentActiveSkills;
    private List<StrikeGroupMemberData> toRemove;
    private List<StrikeGroupMemberData> tempList = new List<StrikeGroupMemberData>();

    private StrikeGroupMember currentlyActivatingSkill;

    private VisualsSaveData saveData;
    private bool towship;

    private void Awake()
    {
        Instance = this;

        aliveMembers = new HashSet<StrikeGroupMember>();
        currentActiveSkills = new Dictionary<StrikeGroupMemberData, StrikeGroupEffectData>();
        toRemove = new List<StrikeGroupMemberData>();
        slots = new List<Transform>();
        for (int i = 0; i < modelSlotsRoot.childCount; ++i)
        {
            slots.Add(modelSlotsRoot.GetChild(i));
        }

        EnabledShips = -1;

        saveData = saveData.Duplicate();
    }

    private void Start()
    {
        TimeManager.Instance.AddTickable(this);
        HudManager.Instance.WindowStateChanged += OnWindowStateChanged;
    }

    public void Setup(SOTacticMap fabularMap)
    {
        WorldMap.Instance.Toggled += OnWorldMapToggled;

        var escorts = fabularMap.Overrides.Escort;
        if (escorts == null)
        {
            escorts = new List<int>();
            ref var data = ref SaveManager.Instance.Data.IntermissionData;
            foreach (var index in data.SelectedEscort)
            {
                if (index != -1)
                {
                    escorts.Add(data.OwnedEscorts[index]);
                }
            }
        }

        members = new List<StrikeGroupMember>();
        currentButtons = new List<StrikeGroupButton>();
        defenceBonus = 0;
        foreach (var escort in escorts)
        {
            if (escort != -1)
            {
                var data = Data.Data[escort];
                var member = new StrikeGroupMember(this, data, Instantiate(shipButtonPrefab, Vector3.zero, Quaternion.identity, escortPanelTransform), members.Count, 0);
                member.GameObject = Instantiate(member.Data.MainPrefab, Vector3.zero, Quaternion.identity, slots[members.Count]);
                member.GameObject.transform.localPosition = Vector3.zero;
                member.EscortAnim = member.GameObject.GetComponent<EscortAnim>();
                if (member.EscortAnim.SmokeParticles != null && member.EscortAnim.SmokeParticles.Count > 0)
                {
                    HudManager.Instance.SmokeController.AddSmokeMember(member.EscortAnim.SmokeParticles);
                }
                members.Add(member);
                aliveMembers.Add(member);
            }
        }
        escortPanelTransform.gameObject.SetActive(false);

        RecalculateStrikeGroupBonuses();
        strikeGroupActiveSkillSelection.Setup(this);

        EnemyAttacksManager.Instance.AttackOnUs += OnAttackOnUs;
    }

    public void SendBackToPearlHarbour(StrikeGroupMember member)
    {
        Damage(member, member.CurrentDurability);
        members.Remove(member);
        aliveMembers.Remove(member);
        member.Destroy();
    }

    public void LoadData(ref StrikeGroupSaveData data, ref VisualsSaveData visualsData)
    {
        for (int i = 0, j = 0; i < data.Escort.Count; i++, j++)
        {
            int customIndex = data.Escort[i].Custom;
            if (customIndex > 0)
            {
                CreateCustomStrikeGroup(customIndex - 1);
                if (customIndex == 2)
                {
                    j--;
                    continue;
                }
            }
            if (members[j].LoadData(data.Escort[i]))
            {
                aliveMembers.Remove(members[j]);
            }
        }
        if (data.Towship)
        {
            CreateCustomStrikeGroup(1);
        }
        currentActiveSkills.Clear();
        for (int i = 0; i < data.Skills.Count; i++)
        {
            var escortWithSkill = members[data.Skills[i].Escort];

            StrikeGroupEffectData skillData;
            if (IsPersistent(escortWithSkill.Data.ActiveSkill))
            {
                UseSkill(escortWithSkill);
                skillData = currentActiveSkills[escortWithSkill.Data];
            }
            else
            {
                skillData = new StrikeGroupEffectData();
                skillData.MaxTimer = escortWithSkill.Duration;
                currentActiveSkills[escortWithSkill.Data] = skillData;
            }
            skillData.Preparing = data.Skills[i].Preparing;
            skillData.Timer = data.Skills[i].Timer;
            skillData.Count = data.Skills[i].Count;
        }
        RecalculateStrikeGroupBonuses();

        saveData = visualsData;
        if (saveData.Carrier)
        {
            ShowCarrier();
        }
        if (saveData.LaunchBombers)
        {
            if (director != null && director.TryGetComponent(out PlayableDirector dir))
            {
                dir.initialTime = dir.time = dir.duration - .5d;
                director.SetActive(true);
            }
        }
        TacticManager.Instance.LoadRanges(ref saveData);
    }

    public void LateLoadData(ref StrikeGroupSaveData saveData)
    {
        var tacticMan = TacticManager.Instance;
        for (int i = 0; i < saveData.Skills.Count; i++)
        {
            var skill = saveData.Skills[i];
            var memberData = members[skill.Escort].Data;
            currentActiveSkills[memberData].RescuingSurvivor = skill.Survivor <= 0 ? null : tacticMan.GetSurvivor(skill.Survivor - 1);
        }
    }

    public void SaveData(ref StrikeGroupSaveData data, ref VisualsSaveData visualsData)
    {
        data.Escort.Clear();
        data.Skills.Clear();
        for (int i = 0; i < members.Count; i++)
        {
            data.Escort.Add(members[i].SaveData());
        }
        if (towship)
        {
            data.Towship = true;
        }
        var tacMan = TacticManager.Instance;
        foreach (var pair in currentActiveSkills)
        {
            var skillData = new EscortActiveSkillSaveData();
            var key = pair.Key;
            skillData.Escort = members.FindIndex((x) => x.Data == key);
            if (skillData.Escort == -1)
            {
                continue;
            }
            skillData.Preparing = pair.Value.Preparing;
            skillData.Timer = pair.Value.Timer;
            skillData.Count = pair.Value.Count;
            skillData.Survivor = pair.Value.RescuingSurvivor == null ? 0 : tacMan.IndexOf(pair.Value.RescuingSurvivor) + 1;
            data.Skills.Add(skillData);
        }

        tacMan.SaveRanges(ref saveData);
        visualsData = saveData;
    }

    public void SaveLosses(ref IntermissionData data)
    {
        for (int i = members.Count; i > 0; i--)
        {
            int index = i - 1;
            if (!aliveMembers.Contains(members[index]) && data.SelectedEscort.Count > index)
            {
                int inventoryID = data.SelectedEscort[index];
                data.SelectedEscort[index] = -1;
                if (inventoryID >= 0)
                {
                    for (int j = 0; j < data.SelectedEscort.Count; j++)
                    {
                        if (data.SelectedEscort[j] > inventoryID)
                        {
                            --data.SelectedEscort[j];
                        }
                    }
                    data.OwnedEscorts.RemoveAt(inventoryID);
                }
            }
        }
    }

    public void CreateCustomStrikeGroup(int index)
    {
        int slot = slots.Count - 2 + index;
        if (index == 1)
        {
            towship = true;
            var go = Instantiate(CustomData.Data[index].MainPrefab, Vector3.zero, Quaternion.identity, slots[slot]);
            go.transform.localPosition = Vector3.zero;
            var anim = go.GetComponent<EscortAnim>();
            if (anim.SmokeParticles != null && anim.SmokeParticles.Count > 0)
            {
                HudManager.Instance.SmokeController.AddSmokeMember(anim.SmokeParticles);
            }
            return;
        }

        var member = new StrikeGroupMember(this, CustomData.Data[index], Instantiate(shipButtonPrefab, Vector3.zero, Quaternion.identity, escortPanelTransform), slot, index + 1);
        member.GameObject = Instantiate(member.Data.MainPrefab, Vector3.zero, Quaternion.identity, slots[slot]);
        member.GameObject.transform.localPosition = Vector3.zero;
        member.EscortAnim = member.GameObject.GetComponent<EscortAnim>();
        if (member.EscortAnim.SmokeParticles != null && member.EscortAnim.SmokeParticles.Count > 0)
        {
            HudManager.Instance.SmokeController.AddSmokeMember(member.EscortAnim.SmokeParticles);
        }
        //Assert.IsTrue(members.Count < slot);

        members.Add(member);
        aliveMembers.Add(member);
    }

    public bool HasNeutralShip()
    {
        return false;
    }

    public bool HasAnyEscort()
    {
        return aliveMembers.Count > 0;
    }

    public void DamageRandom(int damage)
    {
        Damage(RandomUtils.GetRandom(aliveMembers), damage);
    }

    public void UseActiveSkill(StrikeGroupMember member)
    {
        if (UseSkillImmediate(member))
        {
            UseSkill(member);
            StartCooldowns(member.Data);
        }
        else if (CurrentlyActivatingSkill == member)
        {
            CancelSkillActivation();
        }
        else
        {
            StartActivatingSkill(member);
        }
    }

    public void UseSquadronSkill(EPlaneType type, StrikeGroupMember member)
    {
        AircraftCarrierDeckManager.Instance.ReplenishSquadrons(member.Data.ActiveParam, type);

        member.Used++;
        StartCooldowns(member.Data);
    }

    public void EndActiveSkill(StrikeGroupMemberData member)
    {
        EndActiveSkill(member, false);
    }

    public void Tick()
    {
        foreach (var member in aliveMembers)
        {
            member.Update();
        }
        foreach (var pair in currentActiveSkills)
        {
            if (--pair.Value.Timer <= 0)
            {
                if (pair.Value.Preparing)
                {
                    pair.Value.Timer = pair.Value.MaxTimer;
                    pair.Value.Preparing = false;

                    switch (pair.Key.ActiveSkill)
                    {
                        case EStrikeGroupActiveSkill.RepositionStrikeGroup:
                            EnemyAttacksManager.Instance.SetStrikeGroupInvulnerable(true);
                            break;
                        case EStrikeGroupActiveSkill.SinkCargoShip:
                            TacticManager.Instance.DestroyBlock(pair.Value.SelectedEnemyIndex, EEnemyShipType.Cargo);
                            break;
                    }
                }
                else
                {
                    toRemove.Add(pair.Key);
                }
            }
        }
        foreach (var member in toRemove)
        {
            EndActiveSkill(member, true);
        }
        toRemove.Clear();
    }

    public void ShowCarrier()
    {
        saveData.Carrier = true;
        if (director != null)
        {
            director.SetActive(false);
        }
        if (anotherCarrier != null)
        {
            anotherCarrier.SetActive(true);
        }
    }

    public void LaunchBombers()
    {
        saveData.LaunchBombers = true;
        if (director != null)
        {
            director.SetActive(true);
        }
    }

    public void UpdateCooldowns(float escortCooldownModifier)
    {
        float oldModifier = EscortCooldownModifier;
        EscortCooldownModifier = escortCooldownModifier;
        foreach (var member in members)
        {
            member.Cooldown = Mathf.Max((int)(member.Cooldown / oldModifier), member.CurrentDuration);
        }
    }

    public void ActivateBoolSelectionSkill(EStrikeGroupActiveSkill skill, bool isCarrier)
    {
        if (CurrentlyActivatingSkill != null && skill == CurrentlyActivatingSkill.Data.ActiveSkill)
        {
            switch (CurrentlyActivatingSkill.Data.ActiveSkill)
            {
                case EStrikeGroupActiveSkill.TemporaryCustomDefence:
                    var effectData = new StrikeGroupEffectData();
                    currentActiveSkills[CurrentlyActivatingSkill.Data] = effectData;
                    effectData.MaxTimer = effectData.Timer = CurrentlyActivatingSkill.Duration;
                    effectData.Carrier = isCarrier;
                    if (isCarrier)
                    {
                        EnemyAttacksManager.Instance.SetStrikeGroupExtraDefencePoints(CurrentlyActivatingSkill.Data.ActiveParam);
                    }
                    else
                    {
                        EnemyAttacksManager.Instance.SetStrikeGroupExtraEscortPoints(CurrentlyActivatingSkill.Data.ActiveParam);
                    }
                    break;
            }
            StartCooldowns(CurrentlyActivatingSkill.Data);
            CurrentlyActivatingSkill = null;
            StrikeGroupActiveSkillUsed(skill);
        }
    }

    public void ActivateEnemySelectionSkill(EStrikeGroupActiveSkill skill, TacticalEnemyShip enemy)
    {
        if (CurrentlyActivatingSkill != null && skill == CurrentlyActivatingSkill.Data.ActiveSkill)
        {
            switch (CurrentlyActivatingSkill.Data.ActiveSkill)
            {
                case EStrikeGroupActiveSkill.SinkCargoShip:
                    var effectData = new StrikeGroupEffectData();
                    currentActiveSkills[CurrentlyActivatingSkill.Data] = effectData;
                    effectData.Preparing = true;
                    effectData.MaxTimer = effectData.Timer = CurrentlyActivatingSkill.Prepare;
                    effectData.SelectedEnemyIndex = enemy.Id;
                    break;
                case EStrikeGroupActiveSkill.BonusManeuversDefence:
                    enemy.BonusManeuversDefence = CurrentlyActivatingSkill.Data.ActiveParam;
                    break;
            }
            StartCooldowns(CurrentlyActivatingSkill.Data);
            CurrentlyActivatingSkill = null;
            StrikeGroupActiveSkillUsed(skill);
        }
    }

    public void ActivateStrikeGroupSelectionSkill(EStrikeGroupActiveSkill skill, StrikeGroupMember member)
    {
        if (CurrentlyActivatingSkill != null && skill == CurrentlyActivatingSkill.Data.ActiveSkill)
        {
            switch (CurrentlyActivatingSkill.Data.ActiveSkill)
            {
                case EStrikeGroupActiveSkill.RepairEscort:
                    Damage(member, -CurrentlyActivatingSkill.Data.ActiveParam);
                    member.Used++;
                    break;
                case EStrikeGroupActiveSkill.ReduceEscortCooldown:
                    member.Cooldown /= member.Data.ActiveParam;
                    member.Cooldown = Mathf.Max(member.Cooldown, member.CurrentDuration);
                    break;
            }
            StartCooldowns(CurrentlyActivatingSkill.Data);
            CurrentlyActivatingSkill = null;
            StrikeGroupActiveSkillUsed(skill);
        }
    }

    public void ActivateSkill(EStrikeGroupActiveSkill skill)
    {
        if (CurrentlyActivatingSkill != null && skill == CurrentlyActivatingSkill.Data.ActiveSkill)
        {
            switch (CurrentlyActivatingSkill.Data.ActiveSkill)
            {
                case EStrikeGroupActiveSkill.SendMission:
                    AircraftCarrierDeckManager.Instance.FreeSquadrons = -1;
                    TacticManager.Instance.MissionPanel.HighlightButtons(EMissionList.Ready, false, EMissionOrderType.None);
                    break;
                case EStrikeGroupActiveSkill.SendAntiScoutMission:
                    AircraftCarrierDeckManager.Instance.FreeMission = EMissionOrderType.None;
                    TacticManager.Instance.MissionPanel.HighlightButtons(EMissionList.Ready, false, EMissionOrderType.None);
                    break;
                case EStrikeGroupActiveSkill.ReturnSquadrons:
                    AircraftCarrierDeckManager.Instance.EscortRetrievingSquadrons = false;
                    TacticManager.Instance.MissionPanel.HighlightButtons(EMissionList.Retrieval, false, EMissionOrderType.None);
                    break;
            }
            StartCooldowns(CurrentlyActivatingSkill.Data);
            CurrentlyActivatingSkill = null;
            StrikeGroupActiveSkillUsed(skill);
        }
    }

    public bool HaveEscortShipOfType(EStrikeGroupType type)
    {
        foreach (var escort in aliveMembers)
        {
            if (escort.Data.StrikeGroupType == type)
            {
                return true;
            }
        }
        return false;
    }

    public void ToggleAA(bool active)
    {
        foreach (var escort in aliveMembers)
        {
            if (escort.EscortAnim != null)
            {
                escort.EscortAnim.ToggleAntiAir(active);
            }
        }
    }

    private void OnAttackOnUs()
    {
        var enemyAttackMan = EnemyAttacksManager.Instance;
        foreach (var pair in currentActiveSkills)
        {
            if (pair.Key.ActiveSkill == EStrikeGroupActiveSkill.TemporaryDefence && !pair.Value.Removed)
            {
                enemyAttackMan.SetStrikeGroupExtraDefencePoints(-pair.Key.ActiveParam);
                enemyAttackMan.SetStrikeGroupExtraEscortPoints(-pair.Key.ActiveParam);
                pair.Value.Removed = true;
            }
        }
    }

    private bool UseSkillImmediate(StrikeGroupMember member)
    {
        bool start = true;
        switch (member.Data.ActiveSkill)
        {
            case EStrikeGroupActiveSkill.SendMission:
            case EStrikeGroupActiveSkill.ReturnSquadrons:
            case EStrikeGroupActiveSkill.SendAntiScoutMission:
            case EStrikeGroupActiveSkill.RepairEscort:
            case EStrikeGroupActiveSkill.ReduceEscortCooldown:
            case EStrikeGroupActiveSkill.SinkCargoShip:
            case EStrikeGroupActiveSkill.BonusManeuversDefence:
            case EStrikeGroupActiveSkill.TemporaryCustomDefence:
            case EStrikeGroupActiveSkill.ReplenishSquadrons:
                start = false;
                break;
        }
        return start;
    }

    private void CancelSkillActivation()
    {
        Assert.IsNotNull(CurrentlyActivatingSkill);
        switch (CurrentlyActivatingSkill.Data.ActiveSkill)
        {
            case EStrikeGroupActiveSkill.SendMission:
                AircraftCarrierDeckManager.Instance.FreeSquadrons = -1;
                break;
            case EStrikeGroupActiveSkill.SendAntiScoutMission:
                AircraftCarrierDeckManager.Instance.FreeMission = EMissionOrderType.None;
                break;
            case EStrikeGroupActiveSkill.ReturnSquadrons:
                AircraftCarrierDeckManager.Instance.EscortRetrievingSquadrons = false;
                break;
            case EStrikeGroupActiveSkill.RepairEscort:
            case EStrikeGroupActiveSkill.ReduceEscortCooldown:
            case EStrikeGroupActiveSkill.TemporaryCustomDefence:
            case EStrikeGroupActiveSkill.SinkCargoShip:
                UIManager.Instance.StrikeGroupSelectionWindow.Hide();
                break;
            case EStrikeGroupActiveSkill.ReplenishSquadrons:
                strikeGroupActiveSkillSelection.CloseWindow();
                break;
        }
        TacticManager.Instance.MissionPanel.HighlightButtons(EMissionList.Ready, false, EMissionOrderType.None);
        TacticManager.Instance.MissionPanel.HighlightButtons(EMissionList.Retrieval, false, EMissionOrderType.None);
        CurrentlyActivatingSkill = null;
    }

    ///Before skill used
    private void StartActivatingSkill(StrikeGroupMember member)
    {
        if (CurrentlyActivatingSkill != null)
        {
            if (CurrentlyActivatingSkill == member)
            {
                return;
            }
            CancelSkillActivation();
        }
        CurrentlyActivatingSkill = member;
        var window = UIManager.Instance.StrikeGroupSelectionWindow;
        switch (member.Data.ActiveSkill)
        {
            case EStrikeGroupActiveSkill.SendMission:
                AircraftCarrierDeckManager.Instance.FreeSquadrons = member.Data.ActiveParam;
                TacticManager.Instance.MissionPanel.HighlightButtons(EMissionList.Ready, true, EMissionOrderType.None);
                break;
            case EStrikeGroupActiveSkill.SendAntiScoutMission:
                AircraftCarrierDeckManager.Instance.FreeMission = EMissionOrderType.CounterHostileScouts;
                TacticManager.Instance.MissionPanel.HighlightButtons(EMissionList.Ready, true, EMissionOrderType.CounterHostileScouts);
                break;
            case EStrikeGroupActiveSkill.ReturnSquadrons:
                AircraftCarrierDeckManager.Instance.EscortRetrievingSquadrons = true;
                TacticManager.Instance.MissionPanel.HighlightButtons(EMissionList.Retrieval, true, EMissionOrderType.None);
                break;
            case EStrikeGroupActiveSkill.RepairEscort:
                window.ShowDamagedShips();
                window.SetWindowPosition(member);
                break;
            case EStrikeGroupActiveSkill.ReduceEscortCooldown:
                window.ShowShipsOnCooldown();
                window.SetWindowPosition(member);
                break;
            case EStrikeGroupActiveSkill.SinkCargoShip:
                window.ShowEnemyCargoShips();
                window.SetWindowPosition(member);
                break;
            case EStrikeGroupActiveSkill.BonusManeuversDefence:
                window.ShowEnemyBases();
                window.SetWindowPosition(member);
                break;
            case EStrikeGroupActiveSkill.TemporaryCustomDefence:
                window.ShowCarrierAndEscort();
                window.SetWindowPosition(member);
                break;
            case EStrikeGroupActiveSkill.ReplenishSquadrons:
                strikeGroupActiveSkillSelection.OpenPanel(member);
                break;
        }
    }

    ///Use skill with timer
    private void UseSkill(StrikeGroupMember member)
    {
        bool persistent = IsPersistent(member.Data.ActiveSkill);
        if (!currentActiveSkills.TryGetValue(member.Data, out var effectData))
        {
            effectData = new StrikeGroupEffectData();
            currentActiveSkills[member.Data] = effectData;
            var enemyAttackMan = EnemyAttacksManager.Instance;
            var tacMan = TacticManager.Instance;
            switch (member.Data.ActiveSkill)
            {
                case EStrikeGroupActiveSkill.SetUndetected:
                    enemyAttackMan.Undetect(member.Data.ActiveParam);
                    break;
                case EStrikeGroupActiveSkill.RepairSquadrons:
                    AircraftCarrierDeckManager.Instance.RepairRandom(member.Data.ActiveParam);
                    break;
                case EStrikeGroupActiveSkill.RedirectAttack:
                    RadarManager.Instance.RedirectAttack(member.Data.ActiveParam);
                    break;
                case EStrikeGroupActiveSkill.RepositionStrikeGroup:
                    effectData.Preparing = true;
                    break;
                case EStrikeGroupActiveSkill.TemporaryDefence:
                    enemyAttackMan.SetStrikeGroupExtraDefencePoints(member.Data.ActiveParam);
                    enemyAttackMan.SetStrikeGroupExtraEscortPoints(member.Data.ActiveParam);
                    break;
                case EStrikeGroupActiveSkill.Spotting:
                    tacMan.Carrier.SetSubmaringSpottingBonus(true);
                    break;
                case EStrikeGroupActiveSkill.SubmarineBlock:
                    EnemyAttacksManager.Instance.SubmarinesBlocked = true;
                    tacMan.DespawnAllSubmarines();
                    break;
                case EStrikeGroupActiveSkill.RescueSurvivors:
                    effectData.RescuingSurvivor = tacMan.GetSurvivorObjectInRange();
                    break;
                case EStrikeGroupActiveSkill.AddDc:
                    DamageControlManager.Instance.AddTempGroups(member.Data.ActiveParam);
                    break;
                case EStrikeGroupActiveSkill.RefreshAACooldown:
                    CrewManager.Instance.ChangeAACooldown(member.Data.ActiveParam);
                    break;
                case EStrikeGroupActiveSkill.RadarRange:
                    tacMan.Carrier.SetEscortSpottingBonus(member.Data.ActiveParam);
                    break;
            }
            StrikeGroupActiveSkillUsed(member.Data.ActiveSkill);
            effectData.Timer = effectData.Preparing ? member.Prepare : member.Duration;
            effectData.MaxTimer = member.Duration;
        }
        if (persistent)
        {
            effectData.Count++;
            if (!effectData.Preparing)
            {
                effectData.Timer = effectData.MaxTimer;
            }
        }
    }

    ///On skill time expire / on member destroyed
    private void EndActiveSkill(StrikeGroupMemberData data, bool force)
    {
        if (currentActiveSkills.TryGetValue(data, out var effectData))
        {
            effectData.Count--;
            if (effectData.Count == 0 || force)
            {
                var tacMan = TacticManager.Instance;
                var enemyAttackMan = EnemyAttacksManager.Instance;
                switch (data.ActiveSkill)
                {
                    case EStrikeGroupActiveSkill.RedirectAttack:
                        break;
                    case EStrikeGroupActiveSkill.RepositionStrikeGroup:
                        if (!effectData.Preparing)
                        {
                            enemyAttackMan.SetStrikeGroupInvulnerable(false);
                        }
                        break;
                    case EStrikeGroupActiveSkill.TemporaryDefence:
                        if (effectData.Removed)
                        {
                            effectData.Removed = false;
                        }
                        else
                        {
                            enemyAttackMan.SetStrikeGroupExtraDefencePoints(-data.ActiveParam);
                            enemyAttackMan.SetStrikeGroupExtraEscortPoints(-data.ActiveParam);
                        }
                        break;
                    case EStrikeGroupActiveSkill.Spotting:
                        tacMan.Carrier.SetSubmaringSpottingBonus(false);
                        break;
                    case EStrikeGroupActiveSkill.SubmarineBlock:
                        enemyAttackMan.SubmarinesBlocked = false;
                        break;
                    case EStrikeGroupActiveSkill.RescueSurvivors:
                        if (effectData.RescuingSurvivor != null)
                        {
                            tacMan.FireSurvivorObjectFinished(tacMan.IndexOf(effectData.RescuingSurvivor), true);
                            tacMan.DestroySurvivor(effectData.RescuingSurvivor);
                        }
                        break;
                    case EStrikeGroupActiveSkill.AddDc:
                        DamageControlManager.Instance.RemoveTempGroups(data.ActiveParam);
                        break;
                    case EStrikeGroupActiveSkill.RadarRange:
                        tacMan.Carrier.SetEscortSpottingBonus(-data.ActiveParam);
                        break;
                    case EStrikeGroupActiveSkill.TemporaryCustomDefence:
                        if (effectData.Carrier)
                        {
                            enemyAttackMan.SetStrikeGroupExtraDefencePoints(-data.ActiveParam);
                        }
                        else
                        {
                            enemyAttackMan.SetStrikeGroupExtraEscortPoints(-data.ActiveParam);
                        }
                        break;
                        //#error
                }
                currentActiveSkills.Remove(data);
            }
        }
    }

    public void Damage(StrikeGroupMember member, int damage)
    {
        if (member.Damage(damage))
        {
            aliveMembers.Remove(member);
            EndActiveSkill(member.Data);
            RecalculateStrikeGroupBonuses();
            if (CurrentlyActivatingSkill == member)
            {
                CancelSkillActivation();
            }

            ShipSunk();
            if (member.Custom > 0)
            {
                CustomEscortDestroyed();
            }
        }
    }

    private bool IsPersistent(EStrikeGroupActiveSkill skill)
    {
        switch (skill)
        {
            case EStrikeGroupActiveSkill.RedirectAttack:
            case EStrikeGroupActiveSkill.RepositionStrikeGroup:
            case EStrikeGroupActiveSkill.TemporaryDefence:
            case EStrikeGroupActiveSkill.Spotting:
            case EStrikeGroupActiveSkill.SubmarineBlock:
            case EStrikeGroupActiveSkill.AddDc:
            case EStrikeGroupActiveSkill.RadarRange:
                return true;
            case EStrikeGroupActiveSkill.SetUndetected:
            case EStrikeGroupActiveSkill.ReplenishSquadrons:
            case EStrikeGroupActiveSkill.RepairSquadrons:
            case EStrikeGroupActiveSkill.RefreshAACooldown:
                return false;
        }
        return false;
    }

    private void RecalculateStrikeGroupBonuses()
    {
        defenceBonus = 0;
        escortBonus = 0;
        int suppliesBonus = 0;
        int squadronBonus = 0;
        int resupplySpeedModifier = 0;
        int repairSpeedModifier = 0;
        int radarRange = 0;
        int bonusManeuversDefense = 0;

        var tacMan = TacticManager.Instance;

        foreach (var member in aliveMembers)
        {
            foreach (var data in member.Data.PassiveSkills)
            {
                switch (data.Skill)
                {
                    case EStrikeGroupPassiveSkill.Defense:
                        defenceBonus += data.Param;
                        break;
                    case EStrikeGroupPassiveSkill.Escort:
                        escortBonus += data.Param;
                        break;
                    case EStrikeGroupPassiveSkill.MaxSupplies:
                        suppliesBonus += data.Param;
                        break;
                    case EStrikeGroupPassiveSkill.MaxSquadrons:
                        squadronBonus += data.Param;
                        break;
                    case EStrikeGroupPassiveSkill.FasterResupply:
                        resupplySpeedModifier += data.Param;
                        break;
                    case EStrikeGroupPassiveSkill.DcRepairSpeed:
                        repairSpeedModifier += data.Param;
                        break;
                    case EStrikeGroupPassiveSkill.RadarRange:
                        radarRange += data.Param;
                        break;
                    case EStrikeGroupPassiveSkill.BonusManeuversDefense:
                        bonusManeuversDefense += data.Param;
                        break;
                }
            }
        }
        var enemyAttacks = EnemyAttacksManager.Instance;
        enemyAttacks.SetStrikeGroupDefencePoints(defenceBonus);
        enemyAttacks.SetStrikeGroupEscortPoints(escortBonus);

        var resourceMan = ResourceManager.Instance;
        resourceMan.SetMaxSuppliesBonus(suppliesBonus / 100f);
        resourceMan.SetResupplySpeedModifier(resupplySpeedModifier / 100f);

        AircraftCarrierDeckManager.Instance.SetMaxSquadronsBonus(squadronBonus);

        DamageControlManager.Instance.SetEscortRepairSpeedModifier(repairSpeedModifier / 100f);

        tacMan.Carrier.SetEscortPassiveSpottingBonus(radarRange);
        tacMan.BonusManeuversDefence = bonusManeuversDefense;
    }

    private void StartCooldowns(StrikeGroupMemberData memberData)
    {
        foreach (var member in members)
        {
            if (member.Data == memberData)
            {
                member.StartCooldown();
            }
        }
    }

    private void OnWorldMapToggled(bool state)
    {
        if (state)
        {
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (aliveMembers.Contains(member) && member.Custom > 0)
                {
                    i--;
                    SendBackToPearlHarbour(member);
                }
            }
            if (currentlyActivatingSkill != null)
            {
                CancelSkillActivation();
            }
            tempList.Clear();
            foreach (var skill in currentActiveSkills.Keys)
            {
                tempList.Add(skill);
            }
            foreach (var s in tempList)
            {
                EndActiveSkill(s);
            }
            foreach (var member in aliveMembers)
            {
                member.Cooldown = 0;
                member.Update();
            }
        }
    }

    private void OnWindowStateChanged(EWindowType type, bool opened)
    {
        if (currentlyActivatingSkill != null && type == EWindowType.EscortPanel)
        {
            CancelSkillActivation();
        }
    }
}
