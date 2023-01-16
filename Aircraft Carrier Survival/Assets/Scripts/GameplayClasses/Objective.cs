using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Objective : ITickable
{
    public bool ObjectiveValidState => Visible || Activated || Finished;

    public bool Visible
    {
        get;
        private set;
    }

    public bool Activated
    {
        get;
        private set;
    }

    public bool Finished
    {
        get;
        private set;
    }

    public EObjectiveCategory Category
    {
        get;
        private set;
    }

    public int Index
    {
        get;
        private set;
    }

    public string Title
    {
        get;
        private set;
    }

    public bool Success
    {
        get;
        private set;
    }

    public readonly ObjectiveData Data;
    private int count;
    private int failCount;
    private float oldMiles;
    private HashSet<int> group;

    private List<int> progress;
    private bool textChanged;
    private string desc;
    private int paramA;
    private int paramB;
    private bool midwayCAP;

    private Vector2 customPosition;

    public Objective(ObjectiveData data, int index)
    {
        this.Data = data;
        Index = index;

        Category = data.ObjectiveCategory;
        group = new HashSet<int>();

        data.CurrentActive = false;
        data.CurrentVisible = false;

        ObjectivesManager.Instance.AddObjective(data.Title, data.Description, data.ObjectiveTargetIDs, data.ObjectiveTargetVectors, data.UOObjectives, data.Params);
    }

    public void LoadData(ObjectiveSaveData saveData)
    {
        if (saveData.Visible)
        {
            Data.CurrentVisible = true;
            SetShow(true);
        }
        if (saveData.Active)
        {
            Data.CurrentActive = true;
            progress = saveData.Progress;
        }

        var objMan = ObjectivesManager.Instance;
        if (saveData.Finished)
        {
            Finished = true;
            Success = saveData.Success;
            objMan.FinishObjective(Index, saveData.Success);
        }
        if (saveData.TextChanged)
        {
            objMan.SetObjectiveText(Index, saveData.Title, saveData.Description, saveData.ParamA, saveData.ParamB);
        }

        if (saveData.MidwayCAP)
        {
            midwayCAP = true;
            EnemyAttacksManager.Instance.FriendlyCAPIsMidway = true;
            EventManager.Instance.SetAllyEventIcon();
        }
    }

    public void LateLoad()
    {
        if (Data.CurrentActive)
        {
            Activate();
        }
    }

    public ObjectiveSaveData SaveData()
    {
        var result = new ObjectiveSaveData();

        result.Visible = Visible;
        result.Finished = Finished;
        result.Success = Success;
        result.Progress = new List<int>();

        result.TextChanged = textChanged;

        result.ParamA = paramA;
        result.ParamB = paramB;

        result.Title = Title ?? string.Empty;
        result.Description = desc ?? string.Empty;

        result.MidwayCAP = midwayCAP;

        foreach (var effect in Data.Effects)
        {
            switch (effect.EffectType)
            {
                case EObjectiveEffect.SetEnableUI:
                case EObjectiveEffect.SetEnableCameraInput:
                case EObjectiveEffect.SetEnableOfficer:
                case EObjectiveEffect.SetEnableIslandRoomSelection:
                case EObjectiveEffect.SetEnableSwitch:
                case EObjectiveEffect.SetEnableCoursePosition:
                case EObjectiveEffect.SetEnableCrew:
                case EObjectiveEffect.SetEnableDepartmentPlacement:
                case EObjectiveEffect.SetEnableSquadronType:
                case EObjectiveEffect.SetEnableDragPlanes:
                case EObjectiveEffect.SetEnableMissions:
                case EObjectiveEffect.SetEnableReconUO:
                case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                case EObjectiveEffect.SetEnableRecoveryTimeout:
                case EObjectiveEffect.SetEnableAirstrikeTarget:
                case EObjectiveEffect.SetEnableForcedStrategy:
                case EObjectiveEffect.SetEnableEscort:
                case EObjectiveEffect.SetEnableSpreadIssue:
                case EObjectiveEffect.SetEnableDCIssueDestination:
                case EObjectiveEffect.SetEnableDCInMaintenance:
                case EObjectiveEffect.SetEnableDCInPumps:
                case EObjectiveEffect.SetEnableRescueTimer:
                case EObjectiveEffect.SetEnableMoveTime:
                case EObjectiveEffect.SetEnableObsoleteMission:
                case EObjectiveEffect.SetEnableMaxPlanes:
                case EObjectiveEffect.SetEnableEvents:
                case EObjectiveEffect.SetEnableDCCategory:
                case EObjectiveEffect.ResetEnables:
                case EObjectiveEffect.SpawnInjuredCrew:
                case EObjectiveEffect.SetShowHighlight:
                case EObjectiveEffect.SetShowNarrator:
                case EObjectiveEffect.SetCamera:
                case EObjectiveEffect.FinishCourseSettingMode:
                case EObjectiveEffect.DisableEscortButton:
                case EObjectiveEffect.DisableCancelMission:
                case EObjectiveEffect.DisableEnemyDisappear:
                case EObjectiveEffect.DisableBuffClose:
                case EObjectiveEffect.DisableBuffDeallocation:
                case EObjectiveEffect.DisableOfficerDeallocation:
                    Debug.LogError("Objective have disallowed effect outside tutorial!");
                    break;
            }
        }

        if (Activated)
        {
            result.Active = true;
            switch (Data.Type)
            {
                case EObjectiveType.CompleteObjective:
                case EObjectiveType.Destroy:
                case EObjectiveType.Escort:
                case EObjectiveType.Hide:
                case EObjectiveType.StayHidden:
                case EObjectiveType.ReachMapEdge:
                case EObjectiveType.ReachMapCorner:
                case EObjectiveType.FinishCustomMission:
                case EObjectiveType.MissionAfterAction:
                case EObjectiveType.SendAirstrikeWithoutLosses:
                case EObjectiveType.UseStrikeGroupActiveSkill:
                case EObjectiveType.EnemyAttackSuccess:
                case EObjectiveType.ZoomCamera:
                case EObjectiveType.MoveCamera:
                case EObjectiveType.CameraView:
                case EObjectiveType.OfficerReachedRoom:
                case EObjectiveType.UseSwitch:
                case EObjectiveType.OpenWindow:
                case EObjectiveType.SetCourse:
                case EObjectiveType.SetCarrierSpeed:
                case EObjectiveType.UseBuffOrder:
                case EObjectiveType.DragCrew:
                case EObjectiveType.FinishCrewReposition:
                case EObjectiveType.PlanMission:
                case EObjectiveType.SendMission:
                case EObjectiveType.HoverMission:
                case EObjectiveType.ChangeDeckState:
                case EObjectiveType.StartPlanningMission:
                case EObjectiveType.MakeForcedStrategy:
                case EObjectiveType.SetRecoveryPosition:
                case EObjectiveType.ManeuverCategory:
                case EObjectiveType.StartCourseSettingMode:
                case EObjectiveType.ChooseBuff:
                case EObjectiveType.FinishBuffOfficerAssign:
                case EObjectiveType.ClickEvent:
                case EObjectiveType.ClickDcCategory:
                case EObjectiveType.FixIssue:
                case EObjectiveType.DcReachMaintenance:
                case EObjectiveType.ReachCriticalDamage:
                case EObjectiveType.FixAllIssues:
                case EObjectiveType.EnemyProximity:
                case EObjectiveType.OrderSendMission:
                case EObjectiveType.CustomEscortDestroyed:
                case EObjectiveType.RevealUO:
                case EObjectiveType.SeekAndDestroyReady:
                case EObjectiveType.RescueSpecificSurvivor:
                case EObjectiveType.SetSpecificCourse:
                    break;
                case EObjectiveType.DestroyBlock:
                case EObjectiveType.ReachMiles:
                case EObjectiveType.Time:
                case EObjectiveType.RescueSurvivors:
                case EObjectiveType.DeploySquadrons:
                case EObjectiveType.DestroyBlockCount:
                    result.Progress.Add(count);
                    break;
                case EObjectiveType.Reach:
                    if (Data.SecondaryTarget == -1 && Data.Simultaneously)
                    {
                        result.Progress.Add(count);
                    }
                    break;
                case EObjectiveType.Reveal:
                case EObjectiveType.RevealBlocks:
                    result.Progress.AddRange(group);
                    break;
                case EObjectiveType.FinishMissions:
                    result.Progress.Add(count);
                    result.Progress.AddRange(group);
                    break;
                case EObjectiveType.Instant:
                case EObjectiveType.Custom:
                default:
                    Debug.LogError("Bad objective activated");
                    break;
            }
        }
        else
        {
            result.Active = false;
        }

        return result;
    }

    public void Tick()
    {
        if (Activated && !Finished)
        {
            if (Data.Type == EObjectiveType.ReachMiles)
            {
                count = (int)(oldMiles - TacticManager.Instance.GetMilesTravelled());
            }
            else
            {
                Assert.IsTrue(Data.Type == EObjectiveType.Time);
                ++count;
            }
            if (count >= Data.Count)
            {
                ObjectivesManager.Instance.FinishObjective(Index, !Data.NotType);
            }
        }
    }

    public void Start()
    {
        if (Data.Visible)
        {
            Data.CurrentVisible = true;
            SetShow(true);
        }
        if (Data.Active)
        {
            Data.CurrentActive = true;
            Activate();
        }
    }

    public void Stop(bool force)
    {
        if (force && Data.CurrentVisible)
        {
            Data.CurrentVisible = false;
            SetShow(false);
        }
        if (Data.CurrentActive)
        {
            Data.CurrentActive = false;
            Deactivate();
        }
    }

    public void Update()
    {
        if (Data.CurrentActive)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
        SetShow(Data.CurrentVisible);
    }

    public void Finish(bool success)
    {
        if (Finished)
        {
            return;
        }

#if ALLOW_CHEATS
        if (HudManager.Instance.ObjectiveLogs)
        {
            Debug.LogWarning($"{Data.Name} {(success ? "succeed" : "failed")}");
        }
#endif
        Finished = true;

        Stop(false);
        if (success)
        {
            this.Success = true;
            var objMan = ObjectivesManager.Instance;
            var tacticMan = TacticManager.Instance;
            var deckMan = AircraftCarrierDeckManager.Instance;
            var uiMan = UIManager.Instance;
            var camMan = CameraManager.Instance;
            var islandMan = IslandsAndOfficersManager.Instance;
            var hudMan = HudManager.Instance;
            var enemyAttacksMan = EnemyAttacksManager.Instance;
            var strikeGroupMan = StrikeGroupManager.Instance;
            var tacMap = TacticalMap.Instance;
            var crewMan = CrewManager.Instance;
            var dragPlanesMan = DragPlanesManager.Instance;
            var sectionMan = SectionRoomManager.Instance;
            var dcMan = DamageControlManager.Instance;
            var eventMan = EventManager.Instance;
            foreach (var effectData in Data.Effects)
            {
                switch (effectData.EffectType)
                {
                    case EObjectiveEffect.ShowObjectives:
                        foreach (int id in effectData.Targets)
                        {
                            objMan.GetObjective(id).Data.CurrentVisible = !effectData.NotEffect;
                        }
                        break;
                    case EObjectiveEffect.ActivateObjectives:
                        foreach (int id in effectData.Targets)
                        {
                            objMan.GetObjective(id).Data.CurrentActive = !effectData.NotEffect;
                        }
                        break;
                    case EObjectiveEffect.SucceedObjectives:
                        foreach (int id in effectData.Targets)
                        {
                            objMan.FinishObjective(id, !effectData.NotEffect);
                        }
                        break;
                    case EObjectiveEffect.ShowMovie:
                        MovieManager.Instance.Play(effectData.Movie);
                        break;
                    case EObjectiveEffect.Reveal:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.UpdateRealtimeObject(id);
                            tacticMan.RevealObject(id);
                        }
                        break;
                    case EObjectiveEffect.Destroy:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.DestroyObject(id, true);
                        }
                        break;
                    case EObjectiveEffect.Win:
                        string descID;
                        bool win;
                        if (effectData.NotEffect)
                        {
                            descID = "";
                            win = false;
                        }
                        else
                        {
                            descID = effectData.OverrideWinDescriptionID;
                            win = true;
                        }
                        var gameStateMan = GameStateManager.Instance;
                        gameStateMan.StartCoroutineActionAfterFrames(() => gameStateMan.ShowMissionSummary(win, Data.LoseType, descID), 2);
                        break;
                    case EObjectiveEffect.ShowHidden:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.ShowHiddenObject(id);
                        }
                        break;
                    case EObjectiveEffect.Spawn:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.SpawnObject(id);
                        }
                        break;
                    case EObjectiveEffect.EnemyCanChase:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.GetShip(id).SetChase(effectData.NotEffect);
                        }
                        break;
                    case EObjectiveEffect.ShowTimer:
                        var timer = SimpleTimerLabel.Instance;
                        if (effectData.NotEffect)
                        {
                            timer.Stop();
                        }
                        else
                        {
                            timer.StartTimer(effectData.Minutes, effectData.TimerID, effectData.TimerTooltipTitleID, effectData.TimerTooltipDescID);
                        }
                        break;
                    case EObjectiveEffect.CustomMissionRetrieval:
                        tacticMan.AddCustomMissionRetrieval(EMissionOrderType.Airstrike, effectData.RetrievePosition, effectData.HoursToRetrieve,
                            effectData.BombersNeeded, effectData.FightersNeeded, effectData.TorpedoesNeeded);
                        break;
                    case EObjectiveEffect.AddMission:
                        if (effectData.NotEffect)
                        {
                            var list2 = tacticMan.Missions[effectData.MissionType];
                            int count2 = list2.Count;
                            for (int i = 0; i < count2; i++)
                            {
                                var mission = list2[count2 - i - 1];
                                mission.RemoveMission(false);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < effectData.Count; i++)
                            {
                                var mission = tacticMan.AddNewMission(effectData.MissionType);
                                foreach (int index in effectData.Targets)
                                {
                                    mission.PossibleMissionTargets.Add(tacticMan.GetShip(index));
                                }
                            }
                        }
                        break;
                    case EObjectiveEffect.SearchAndDestroy:
                        int count = effectData.Targets.Count;
                        int uneven = (count % 2);
                        int halfCount = count / 2 + uneven;
                        for (int i = 0; i < halfCount; i++)
                        {
                            var enemy = tacticMan.GetShip(effectData.Targets[i]);
                            int index = i + halfCount;
                            var target = (index < count) ? tacticMan.GetShip(effectData.Targets[index]) : null;
                            if (!enemy.Dead && !enemy.IsDisabled && !enemy.AlreadyRetreats)
                            {
                                enemy.SetSearchAndDestroy(!effectData.NotEffect, effectData.Power, target);
                            }
                        }
                        break;
                    case EObjectiveEffect.ShowBalancedForcesBar:
                        ForcesBar.Instance.SetBar(effectData.NotEffect ? null : effectData.Targets);
                        break;
                    case EObjectiveEffect.AttackCarrier:
                        enemyAttacksMan.InstantAttack(effectData.Power, effectData.AbsoluteForce);
                        break;
                    case EObjectiveEffect.SpawnSurvivors:
                        foreach (int index in effectData.Targets)
                        {
                            tacticMan.SpawnSurvivor(index);
                        }
                        break;
                    case EObjectiveEffect.SpawnRescueShip:
                        strikeGroupMan.CreateCustomStrikeGroup(0);
                        break;
                    case EObjectiveEffect.MakeDamageRange:
                        if (effectData.NotEffect)
                        {
                            tacticMan.HideDamageRange();
                        }
                        else
                        {
                            foreach (int target in effectData.Targets)
                            {
                                tacticMan.ShowDamageRange(effectData.Power, effectData.Hours, effectData.Range, target);
                            }
                        }
                        break;
                    case EObjectiveEffect.FinalestDestroySections:
                        foreach (var subsection in sectionMan.Engines.SubsectionRooms)
                        {
                            subsection.IsBroken = true;
                            subsection.Irrepairable = true;
                        }
                        foreach (var subsection in sectionMan.Helm.SubsectionRooms)
                        {
                            subsection.IsBroken = true;
                            subsection.Irrepairable = true;
                        }
                        break;
                    case EObjectiveEffect.ForceCarrierWaypoints:
                        foreach (int node in effectData.Targets)
                        {
                            tacMap.CustomAddWaypoint(tacticMan.MapNodes.Find(node).Position);
                        }
                        break;
                    case EObjectiveEffect.ForceCarrierSpeed:
                        hudMan.SetBlockCarrierSpeed(effectData.Power);
                        break;
                    case EObjectiveEffect.SpawnTowShip:
                        strikeGroupMan.CreateCustomStrikeGroup(1);
                        break;
                    case EObjectiveEffect.SetEnableUI:
                        uiMan.SetEnabledCategory(effectData.UICategories);
                        break;
                    case EObjectiveEffect.SetEnableCameraInput:
                        camMan.CameraInput = effectData.CameraInput;
                        break;
                    case EObjectiveEffect.SetEnableOfficer:
                        islandMan.OfficersEnabled = effectData.Power;
                        break;
                    case EObjectiveEffect.SetEnableIslandRoomSelection:
                        islandMan.IslandsEnabled = effectData.IslandRooms;
                        break;
                    case EObjectiveEffect.SetEnableSwitch:
                        islandMan.SwitchesEnabled = effectData.Power;
                        break;
                    case EObjectiveEffect.SetEnableCoursePosition:
                        if (effectData.Targets.Count == 0)
                        {
                            tacMap.EnableCourse();
                        }
                        else
                        {
                            tacMap.EnableCourse(tacticMan.MapNodes.Find(effectData.Targets[0]).Position, effectData.Targets.Count > 1 ? tacticMan.MapNodes.Find(effectData.Targets[1]).Position : (Vector2?)null);
                        }
                        break;
                    case EObjectiveEffect.SetEnableCrew:
                        crewMan.CrewEnabled = effectData.Power;
                        break;
                    case EObjectiveEffect.SetEnableDepartmentPlacement:
                        crewMan.DepartmentsEnabled = effectData.Departments;
                        break;
                    case EObjectiveEffect.SetEnableSquadronType:
                        dragPlanesMan.SquadronTypeEnabled = effectData.PlaneType;
                        break;
                    case EObjectiveEffect.SetEnableDragPlanes:
                        dragPlanesMan.SetSelectedEnable(!effectData.NotEffect);
                        break;
                    case EObjectiveEffect.SetEnableMissions:
                        tacticMan.EnabledMissions = effectData.Missions;
                        break;
                    case EObjectiveEffect.SetEnableReconUO:
                        tacMap.ReconWaypointInUoRange = !effectData.NotEffect;
                        break;
                    case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                        tacMap.RecoveryOnCarrier = !effectData.NotEffect;
                        break;
                    case EObjectiveEffect.SetEnableRecoveryTimeout:
                        tacticMan.RecoveryTimeoutDisabled = effectData.NotEffect;
                        break;
                    case EObjectiveEffect.SetEnableAirstrikeTarget:
                        tacticMan.SetAirstrikeDefaultTargets(effectData.NotEffect ? null : effectData.Targets);
                        break;
                    case EObjectiveEffect.SetEnableForcedStrategy:
                        tacticMan.StrategySelectionPanel.ForceStrategy(effectData.NotEffect ? null : effectData.Strategy);
                        break;
                    case EObjectiveEffect.SetEnableEscort:
                        strikeGroupMan.EnabledShips = effectData.Power;
                        break;
                    case EObjectiveEffect.SetEnableSpreadIssue:
                        foreach (var segment in sectionMan.GetAllSegments())
                        {
                            SetEnableSpreadIssue(effectData.Issue, segment, !effectData.NotEffect);
                        }
                        break;
                    case EObjectiveEffect.SetEnableDCIssueDestination:
                        if (effectData.NotEffect)
                        {
                            dcMan.IssueDestination = null;
                        }
                        else
                        {
                            dcMan.IssueDestination = effectData.Issue;
                        }
                        break;
                    case EObjectiveEffect.SetEnableDCInMaintenance:
                        dcMan.MaintenanceDCFreeze = effectData.NotEffect;
                        break;
                    case EObjectiveEffect.SetEnableDCInPumps:
                        dcMan.PumpsDCFreeze = effectData.NotEffect;
                        break;
                    case EObjectiveEffect.SetEnableRescueTimer:
                        crewMan.FreezeRescueTime = effectData.NotEffect;
                        break;
                    case EObjectiveEffect.SetEnableMoveTime:
                        crewMan.SkipMoveTime = effectData.NotEffect;
                        break;
                    case EObjectiveEffect.SetEnableObsoleteMission:
                        tacticMan.ObsoleteDisabled = effectData.NotEffect;
                        break;
                    case EObjectiveEffect.SetEnableMaxPlanes:
                        deckMan.SetMaxPlanes(effectData.PlaneType, effectData.Count);
                        break;
                    case EObjectiveEffect.SetEnableEvents:
                        eventMan.EventsEnabled = effectData.Events;
                        break;
                    case EObjectiveEffect.SetEnableDCCategory:
                        dcMan.DcCategoryEnabled = effectData.DcCategoryFlag;
                        break;
                    case EObjectiveEffect.ResetEnables:
                        uiMan.SetEnabledCategory((EUICategory)(-1));
                        camMan.CameraInput = (ECameraInputType)(-1);
                        islandMan.OfficersEnabled = -1;
                        islandMan.IslandsEnabled = (EIslandRoomFlag)(-1);
                        islandMan.SwitchesEnabled = -1;
                        tacMap.EnableCourse();
                        crewMan.CrewEnabled = -1;
                        crewMan.DepartmentsEnabled = (EDepartmentsFlag)(-1);
                        dragPlanesMan.SquadronTypeEnabled = EManeuverSquadronType.Any;
                        dragPlanesMan.SetSelectedEnable(true);
                        tacticMan.EnabledMissions = (EMissionOrderFlag)(-1);
                        tacMap.ReconWaypointInUoRange = false;
                        tacMap.RecoveryOnCarrier = false;
                        tacticMan.RecoveryTimeoutDisabled = false;
                        tacticMan.SetAirstrikeDefaultTargets(null);
                        tacticMan.StrategySelectionPanel.ForceStrategy(null);
                        strikeGroupMan.EnabledShips = -1;
                        foreach (var segment in sectionMan.GetAllSegments())
                        {
                            SetEnableSpreadIssue(EIssue.Any, segment, true);
                        }
                        dcMan.IssueDestination = null;
                        dcMan.MaintenanceDCFreeze = false;
                        dcMan.PumpsDCFreeze = false;
                        crewMan.FreezeRescueTime = false;
                        crewMan.SkipMoveTime = false;
                        tacticMan.ObsoleteDisabled = false;
                        deckMan.SetMaxPlanes(EManeuverSquadronType.Any, -1);
                        eventMan.EventsEnabled = (EEventFlag)(-1);
                        dcMan.DcCategoryEnabled = (EDcCategoryFlag)(-1);
                        CrewStatusManager.Instance.DisableDeath = false;
                        break;
                    case EObjectiveEffect.SetMaxCarrierSpeed:
                        if (effectData.NotEffect)
                        {
                            hudMan.SetObjectiveMinCarrierSpeed(effectData.Power);
                        }
                        else
                        {
                            hudMan.SetObjectiveMaxCarrierSpeed(effectData.Power);
                        }
                        break;
                    case EObjectiveEffect.SetTimeSpeed:
                        hudMan.ChangeTimeSpeed(effectData.Power);
                        break;
                    case EObjectiveEffect.SetSupplies:
                        ResourceManager.Instance.SetSupplies(effectData.Power);
                        break;
                    case EObjectiveEffect.SpawnNeutral:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.SpawnUO(effectData.UOType, id);
                        }
                        break;
                    case EObjectiveEffect.DestroyNeutrals:
                        tacticMan.DestroyNeutrals();
                        break;
                    case EObjectiveEffect.SetSentMissionDuration:
                        tacticMan.SetMissionIdleTime(effectData.MissionType, effectData.Hours);
                        break;
                    case EObjectiveEffect.SpawnAttack:
                        enemyAttacksMan.MakeAttack(effectData.Power, effectData.SecondSubsection, effectData.AttackStrikeGroup, effectData.Hours);
                        break;
                    case EObjectiveEffect.SpawnIssue:
                        var segment2 = sectionMan.GetSection(effectData.Section).SubsectionRooms[effectData.SecondSubsection ? 1 : 0].Segments[effectData.Power];
                        switch (effectData.Issue)
                        {
                            case EIssue.Fire:
                                segment2.MakeFire();
                                break;
                            case EIssue.Flood:
                                segment2.MakeFlood(false);
                                break;
                            case EIssue.Fault:
                                segment2.MakeDamage();
                                break;
                            case EIssue.Injured:
                                segment2.MakeInjured(EWaypointTaskType.Rescue, null, true);
                                break;
                        }
                        break;
                    case EObjectiveEffect.SpawnInjuredCrew:
                        var crewStatusMan = CrewStatusManager.Instance;
                        foreach (var crewUnit in crewMan.CrewUnits)
                        {
                            if (crewUnit.DragDrop.CurrentDepartment != null && crewUnit.DragDrop.CurrentDepartment.Departments == effectData.Department && crewUnit.UnitState == ECrewUnitState.Healthy)
                            {
                                crewStatusMan.AddInjured(crewUnit);
                                break;
                            }
                        }
                        break;
                    case EObjectiveEffect.SetObjectiveText:
                        foreach (int id in effectData.Targets)
                        {
                            objMan.SetObjectiveText(id, effectData.TimerTooltipTitleID, effectData.TimerTooltipDescID, effectData.ParamA, effectData.ParamB);
                        }
                        break;
                    case EObjectiveEffect.SetShowHighlight:
                        HighlightsManager.Instance.SetHighlight(effectData.Category, effectData.Highlight, effectData.Time);
                        break;
                    case EObjectiveEffect.SetShowNarrator:
                        var narratorMan = NarratorManager.Instance;
                        if (effectData.NotEffect)
                        {
                            narratorMan.Hide();
                        }
                        else
                        {
                            narratorMan.Show(effectData.TimerID, effectData.TimerTooltipTitleID, effectData.TimerTooltipDescID, effectData.SoundEventReference, effectData.Power, effectData.OverridePosition);
                        }
                        break;
                    case EObjectiveEffect.SetCamera:
                        camMan.SwitchMode(effectData.CameraView);
                        break;
                    case EObjectiveEffect.FinishCourseSettingMode:
                        if (tacMap.CourseSettingMode)
                        {
                            tacMap.ToggleCourseSetup(false);
                        }
                        break;
                    case EObjectiveEffect.DisableEscortButton:
                        uiMan.StrikeGroupSelectionWindow.DisableEscortButton = true;
                        break;
                    case EObjectiveEffect.DisableCancelMission:
                        tacticMan.MissionPanel.MissionDetails.CancelButton.interactable = false;
                        break;
                    case EObjectiveEffect.DisableEnemyDisappear:
                        tacticMan.Markers.DisableDisappear = true;
                        break;
                    case EObjectiveEffect.DisableBuffClose:
                        islandMan.DisableBuffClose = true;
                        break;
                    case EObjectiveEffect.DisableBuffDeallocation:
                        islandMan.DisableBuffDeallocation = true;
                        break;
                    case EObjectiveEffect.DisableOfficerDeallocation:
                        islandMan.DisableOfficerDeallocation = true;
                        break;
                    case EObjectiveEffect.DetectPlayer:
                        enemyAttacksMan.Detect();
                        break;
                    case EObjectiveEffect.SetShowObject:
                        uiMan.SetShow(effectData.Power, !effectData.NotEffect);
                        break;
                    case EObjectiveEffect.SwitchFriendlyCAPToMidway:
                        midwayCAP = true;
                        enemyAttacksMan.FriendlyCAPIsMidway = true;
                        eventMan.SetAllyEventIcon();
                        break;
                    case EObjectiveEffect.ShowPath:
                        tacticMan.SetShowPath(true);
                        break;
                    case EObjectiveEffect.ShowRescueRange:
                        tacticMan.SetShowRescueRangeToggle(!effectData.NotEffect);
                        break;
                    case EObjectiveEffect.DestroyBlock:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.DestroyBlock(id, effectData.Count);
                        }
                        break;
                    case EObjectiveEffect.TeleportEnemy:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.GetShip(id).Teleport(effectData.OverridePosition);
                        }
                        break;
                    case EObjectiveEffect.TeleportPlayer:
                        tacticMan.Carrier.Teleport(effectData.OverridePosition);
                        break;
                    case EObjectiveEffect.AdvanceTime:
                        TimeManager.Instance.AdvanceTime(effectData.Date);
                        break;
                    case EObjectiveEffect.ShowCannonRange:
                        tacticMan.SetShowCannonRangeToggle(!effectData.NotEffect);
                        break;
                    case EObjectiveEffect.ShowSurvivors:
                        tacticMan.ShowSurvivors();
                        break;
                    case EObjectiveEffect.KillSurvivor:
                        tacticMan.DestroySurvivor(effectData.Power);
                        break;
                    case EObjectiveEffect.ShowCarrier:
                        strikeGroupMan.ShowCarrier();
                        break;
                    case EObjectiveEffect.LaunchBombers:
                        strikeGroupMan.LaunchBombers();
                        break;
                    case EObjectiveEffect.SetShowSpriteStrategyPanel:
                        tacticMan.SetShowSpriteStrategyPanel(!effectData.NotEffect, effectData.Targets, effectData.ObjectiveTarget);
                        break;
                    case EObjectiveEffect.DestroyNotSentMissions:
                        var list = tacticMan.Missions[effectData.MissionType];
                        for (int i = list.Count; i > 0; i--)
                        {
                            var mission = list[i - 1];
                            if (mission.MissionStage < EMissionStage.Launching)
                            {
                                i = list.Count;
                                mission.RemoveMission(false);
                            }
                        }
                        break;
                    case EObjectiveEffect.ShowAlly:
                        uiMan.ShowAllies(!effectData.NotEffect, effectData.Targets);
                        break;
                    case EObjectiveEffect.DestroySection:
                        var section = sectionMan.GetSection(effectData.Section);
                        if (effectData.FirstSubsection)
                        {
                            section.SubsectionRooms[0].IsBroken = true;
                        }
                        if (effectData.SecondSubsection)
                        {
                            section.SubsectionRooms[1].IsBroken = true;
                        }
                        break;
                    case EObjectiveEffect.ShowMissionRange:
                        tacticMan.SetShowMissionRangeToggle(!effectData.NotEffect, effectData.Targets);
                        break;
                    case EObjectiveEffect.ShowMagicSprite:
                        if (effectData.Power == 0 || effectData.NotEffect)
                        {
                            tacticMan.SetShowMagicSpriteToggle(!effectData.NotEffect);
                        }
                        else
                        {
                            tacticMan.SetMagicSprite(effectData.Power > 1);
                        }
                        break;
                    case EObjectiveEffect.DisableAttacksOnAlly:
                        enemyAttacksMan.DisableAttacksOnAlly = true;
                        break;
                    case EObjectiveEffect.RemovePermamentlyMagicIdentify:
                        tacticMan.MagicIdentifyPermanentRemove = true;
                        break;
                    case EObjectiveEffect.SetSuperTimeSpeed:
                        hudMan.SetSuperTimeSpeed();
                        break;
                    case EObjectiveEffect.Identify:
                        foreach (int id in effectData.Targets)
                        {
                            tacticMan.IdentifyObject(id);
                        }
                        break;
                    case EObjectiveEffect.CancelMissions:
                        tacticMan.CancelMissions();
                        break;
                    case EObjectiveEffect.DisableDeath:
                        CrewStatusManager.Instance.DisableDeath = true;
                        break;
                }
            }
        }
    }

    public void SetText(string title, string description, int paramA, int paramB)
    {
        textChanged = true;
        Title = title;
        desc = description;

        Assert.IsTrue((paramA == 0) == (paramB == 0));
        if (paramA != 0)
        {
            this.paramA = paramA;
            this.paramB = paramB;
            Data.Params = new string[] { (paramA - 1).ToString(), (paramB - 1).ToString() };
        }
    }

    private void Activate()
    {
        if (!Activated && !Finished)
        {
#if ALLOW_CHEATS
            if (HudManager.Instance.ObjectiveLogs)
            {
                Debug.LogWarning($"{Data.Name} activated");
            }
#endif
            Activated = true;

            bool clearNot = true;
            switch (Data.Type)
            {
                case EObjectiveType.CompleteObjective:
                case EObjectiveType.ReachMiles:
                case EObjectiveType.Time:
                case EObjectiveType.FinishCustomMission:
                case EObjectiveType.FinishMissions:
                case EObjectiveType.RescueSurvivors:
                case EObjectiveType.ZoomCamera:
                case EObjectiveType.OpenWindow:
                case EObjectiveType.UseBuffOrder:
                case EObjectiveType.ChangeDeckState:
                case EObjectiveType.DcReachMaintenance:
                case EObjectiveType.EnemyProximity:
                    clearNot = false;
                    break;
            }
            if (clearNot)
            {
                Data.NotType = false;
            }
            if (Data.Type != EObjectiveType.CompleteObjective && Data.Type != EObjectiveType.RescueSurvivors)
            {
                Data.FailOnNegativeTarget = false;
            }

            var objMan = ObjectivesManager.Instance;
            var tacticMan = TacticManager.Instance;
            var enemyAttacksMan = EnemyAttacksManager.Instance;
            var strikeGroupMan = StrikeGroupManager.Instance;
            var timeMan = TimeManager.Instance;
            var camMan = CameraManager.Instance;
            var islandsMan = IslandsAndOfficersManager.Instance;
            var hudMan = HudManager.Instance;
            var tacticalMap = TacticalMap.Instance;
            var crewMan = CrewManager.Instance;
            var deck = AircraftCarrierDeckManager.Instance;
            var eventMan = EventManager.Instance;
            var dcMan = DamageControlManager.Instance;
            var sectionMan = SectionRoomManager.Instance;
            switch (Data.Type)
            {
                case EObjectiveType.CompleteObjective:
                    count = 0;
                    failCount = 0;

                    objMan.ObjectiveFinished -= OnObjectiveFinished;
                    objMan.ObjectiveFinished += OnObjectiveFinished;
                    foreach (int id in Data.Targets)
                    {
                        if (objMan.CheckFinished(id, out bool success))
                        {
                            OnObjectiveFinished(id, success);
                            if (Finished)
                            {
                                break;
                            }
                        }
                    }
                    break;
                case EObjectiveType.Destroy:
                    count = 0;
                    tacticMan.ObjectDestroyed -= OnObjectDestroyed;
                    tacticMan.ObjectDestroyed += OnObjectDestroyed;
                    foreach (int id in Data.Targets)
                    {
                        var ship = tacticMan.GetShip(id);
                        if (ship.Dead)
                        {
                            OnObjectDestroyed(id, !ship.Ignore);
                            if (Finished)
                            {
                                break;
                            }
                        }
                    }
                    break;
                case EObjectiveType.DestroyBlock:
                    count = progress == null ? 0 : progress[0];
                    tacticMan.BlockDestroyed -= OnBlockDestroyed;
                    tacticMan.BlockDestroyed += OnBlockDestroyed;

                    if (Data.Targets.Count != 0)
                    {
                        OnBlockDestroyed(Data.TargetBlock);
                    }
                    break;
                case EObjectiveType.Escort:
                    strikeGroupMan.NeutralShipDestroyed -= OnNeutralShipDestroyed;
                    strikeGroupMan.NeutralShipDestroyed += OnNeutralShipDestroyed;
                    if (!strikeGroupMan.HasNeutralShip())
                    {
                        OnNeutralShipDestroyed();
                    }
                    break;
                case EObjectiveType.Hide:
                    enemyAttacksMan.DetectedChanged -= OnDetectedChanged;
                    enemyAttacksMan.DetectedChanged += OnDetectedChanged;
                    OnDetectedChanged(enemyAttacksMan.IsDetected);
                    break;
                case EObjectiveType.StayHidden:
                    enemyAttacksMan.KindaDetected -= OnKindaDetected;
                    enemyAttacksMan.KindaDetected += OnKindaDetected;
                    OnKindaDetected();
                    break;
                case EObjectiveType.Reach:
                    count = (progress == null || progress.Count == 0) ? 0 : progress[0];
                    if (Data.SecondaryTarget == -1)
                    {
                        if (Data.Simultaneously)
                        {
                            count--;
                            OnDestinationReached();
                        }
                        else
                        {
                            group.Clear();
                            tacticMan.Trigger(this, Data.Targets, OnAnyDestinationReached);
                        }
                    }
                    else
                    {
                        Data.Simultaneously = false;
                        tacticMan.DestinationReached -= OnDestinationReached;
                        tacticMan.DestinationReached += OnDestinationReached;
                        int destination = 0;
                        while (!Finished && tacticMan.HasReachedDestination(Data.SecondaryTarget, destination, out int node))
                        {
                            destination++;
                            OnDestinationReached(Data.SecondaryTarget, node);
                        }
                    }
                    break;
                case EObjectiveType.ReachMiles:
                    timeMan.AddTickable(this);
                    count = progress == null ? 0 : progress[0];
                    oldMiles = tacticMan.GetMilesTravelled() - count;
                    break;
                case EObjectiveType.Reveal:
                    group.Clear();
                    if (progress != null)
                    {
                        foreach (int id in progress)
                        {
                            group.Add(id);
                        }
                    }
                    tacticMan.ObjectVisibilityChanged -= OnObjectVisibilityChanged;
                    tacticMan.ObjectVisibilityChanged += OnObjectVisibilityChanged;
                    foreach (int id in Data.Targets)
                    {
                        if (tacticMan.GetShip(id).Visible)
                        {
                            OnObjectVisibilityChanged(id, true);
                        }
                    }
                    break;
                case EObjectiveType.Time:
                    count = progress == null ? 0 : progress[0];
                    timeMan.AddTickable(this);
                    break;
                case EObjectiveType.Instant:
                    objMan.FinishObjective(Index, true);
                    break;
                case EObjectiveType.ReachMapEdge:
                    tacticMan.SetShowMapEdges(true);
                    tacticMan.EdgeMapReached -= OnEdgeMapReached;
                    tacticMan.EdgeMapReached += OnEdgeMapReached;
                    break;
                case EObjectiveType.ReachMapCorner:
                    Data.MapCornerData.Callback = OnEdgeMapReached;
                    tacticMan.SetMapCornerTrigger(Data.MapCornerData);
                    break;
                case EObjectiveType.FinishCustomMission:
                    tacticMan.CustomMissionFinished -= OnCustomMissionFinished;
                    tacticMan.CustomMissionFinished += OnCustomMissionFinished;
                    break;
                case EObjectiveType.Custom:
                    break;
                case EObjectiveType.FinishMissions:
                    group.Clear();
                    if (progress == null)
                    {
                        count = 0;
                    }
                    else
                    {
                        count = progress[0];
                        for (int i = 1; i < progress.Count; i++)
                        {
                            group.Add(progress[i]);
                        }
                    }
                    tacticMan.MissionFinished -= OnMissionFinished;
                    tacticMan.MissionFinished += OnMissionFinished;
                    break;
                case EObjectiveType.MissionAfterAction:
                    tacticMan.MissionAction -= OnMissionAction;
                    tacticMan.MissionAction += OnMissionAction;
                    break;
                case EObjectiveType.RescueSurvivors:
                    count = progress == null ? 0 : progress[0];
                    tacticMan.SurvivorObjectFinished -= OnSurvivorObjectFinished;
                    tacticMan.SurvivorObjectFinished += OnSurvivorObjectFinished;
                    break;
                case EObjectiveType.SendAirstrikeWithoutLosses:
                    tacticMan.AirstrikeAttacked -= OnAirstrikeAttack;
                    tacticMan.AirstrikeAttacked += OnAirstrikeAttack;
                    break;
                case EObjectiveType.UseStrikeGroupActiveSkill:
                    strikeGroupMan.StrikeGroupActiveSkillUsed -= OnStrikeGroupActiveSkillUsed;
                    strikeGroupMan.StrikeGroupActiveSkillUsed += OnStrikeGroupActiveSkillUsed;
                    break;
                case EObjectiveType.EnemyAttackSuccess:
                    enemyAttacksMan.EnemyAttackSucceed -= OnEnemyAttackSucceed;
                    enemyAttacksMan.EnemyAttackSucceed += OnEnemyAttackSucceed;
                    break;
                case EObjectiveType.ZoomCamera:
                    camMan.CameraZoomed -= OnCameraZoomed;
                    camMan.CameraZoomed += OnCameraZoomed;
                    break;
                case EObjectiveType.MoveCamera:
                    camMan.CameraMoved -= OnCameraMoved;
                    camMan.CameraMoved += OnCameraMoved;
                    break;
                case EObjectiveType.CameraView:
                    camMan.CameraTargetChanged -= OnCameraTargetChanged;
                    camMan.CameraTargetChanged += OnCameraTargetChanged;
                    break;
                case EObjectiveType.OfficerReachedRoom:
                    islandsMan.RoomReached -= OnRoomsReached;
                    islandsMan.RoomReached += OnRoomsReached;
                    break;
                case EObjectiveType.UseSwitch:
                    islandsMan.SwitchUsed -= OnSwitchUsed;
                    islandsMan.SwitchUsed += OnSwitchUsed;
                    break;
                case EObjectiveType.OpenWindow:
                    hudMan.WindowStateChanged -= OnWindowOpened;
                    hudMan.WindowStateChanged += OnWindowOpened;
                    break;
                case EObjectiveType.SetCourse:
                    tacticalMap.WaypointAdded -= OnWaypointAdded;
                    tacticalMap.WaypointAdded += OnWaypointAdded;
                    break;
                case EObjectiveType.SetCarrierSpeed:
                    hudMan.ShipSpeedChanged -= OnShipSpeedChanged;
                    hudMan.ShipSpeedChanged += OnShipSpeedChanged;
                    break;
                case EObjectiveType.UseBuffOrder:
                    islandsMan.BuffConfirmed -= OnBuffConfirmed;
                    islandsMan.BuffExpired -= OnBuffExpired;
                    if (Data.NotType)
                    {
                        islandsMan.BuffExpired += OnBuffExpired;
                    }
                    else
                    {
                        islandsMan.BuffConfirmed += OnBuffConfirmed;
                    }
                    break;
                case EObjectiveType.DragCrew:
                    crewMan.CrewDragged -= OnCrewDragged;
                    crewMan.CrewDragged += OnCrewDragged;
                    break;
                case EObjectiveType.FinishCrewReposition:
                    crewMan.RepositionFinished -= OnRepositionFinished;
                    crewMan.RepositionFinished += OnRepositionFinished;
                    break;
                case EObjectiveType.DeploySquadrons:
                    count = progress == null ? 0 : progress[0];
                    deck.SquadronCreated -= OnSquadronCreated;
                    deck.SquadronCreated += OnSquadronCreated;
                    break;
                case EObjectiveType.PlanMission:
                    tacticMan.MissionPlanned -= OnMissionPlanned;
                    tacticMan.MissionPlanned += OnMissionPlanned;
                    break;
                case EObjectiveType.SendMission:
                    tacticMan.MissionSent -= OnMissionSent;
                    tacticMan.MissionSent += OnMissionSent;
                    break;
                case EObjectiveType.HoverMission:
                    tacticMan.MissionPanel.MissionDetails.MissionHovered -= OnMissionHovered;
                    tacticMan.MissionPanel.MissionDetails.MissionHovered += OnMissionHovered;
                    break;
                case EObjectiveType.ChangeDeckState:
                    deck.DeckModeChanged -= OnDeckModeChanged;
                    deck.DeckModeChanged += OnDeckModeChanged;
                    OnDeckModeChanged();
                    break;
                case EObjectiveType.StartPlanningMission:
                    tacticMan.MissionPlanningStarted -= OnMissionPlanningStarted;
                    tacticMan.MissionPlanningStarted += OnMissionPlanningStarted;
                    break;
                case EObjectiveType.MakeForcedStrategy:
                    tacticMan.StrategySelectionPanel.ForcedStrategySet -= OnForcedStrategySet;
                    tacticMan.StrategySelectionPanel.ForcedStrategySet += OnForcedStrategySet;
                    break;
                case EObjectiveType.SetRecoveryPosition:
                    tacticalMap.MissionAreasSet -= OnMissionAreasSet;
                    tacticalMap.MissionAreasSet += OnMissionAreasSet;
                    break;
                case EObjectiveType.ManeuverCategory:
                    tacticMan.StrategySelectionPanel.CategorySwitched -= OnManeuverCategorySwitched;
                    tacticMan.StrategySelectionPanel.CategorySwitched += OnManeuverCategorySwitched;
                    OnManeuverCategorySwitched(tacticMan.StrategySelectionPanel.GetCurrentCategory());
                    break;
                case EObjectiveType.StartCourseSettingMode:
                    tacticalMap.CourseSettingModeChanged -= OnCourseSettingModeChanged;
                    tacticalMap.CourseSettingModeChanged += OnCourseSettingModeChanged;
                    OnCourseSettingModeChanged();
                    break;
                case EObjectiveType.ChooseBuff:
                    islandsMan.BuffSetupStarted -= OnBuffSetupStarted;
                    islandsMan.BuffSetupStarted += OnBuffSetupStarted;
                    break;
                case EObjectiveType.FinishBuffOfficerAssign:
                    islandsMan.BuffSetupReady -= OnBuffSetupReady;
                    islandsMan.BuffSetupReady += OnBuffSetupReady;
                    break;
                case EObjectiveType.ClickEvent:
                    eventMan.EventClicked -= OnEventClicked;
                    eventMan.EventClicked += OnEventClicked;
                    break;
                case EObjectiveType.ClickDcCategory:
                    dcMan.DcCategoryClicked -= OnDcCategoryClicked;
                    dcMan.DcCategoryClicked += OnDcCategoryClicked;
                    break;
                case EObjectiveType.FixIssue:
                    sectionMan.IssueFixed -= OnIssueFixed;
                    sectionMan.IssueFixed += OnIssueFixed;
                    break;
                case EObjectiveType.DcReachMaintenance:
                    dcMan.MaintenanceActivated -= OnDcReachedSection;
                    dcMan.PumpsActivated -= OnDcReachedSection;
                    if (Data.NotType)
                    {
                        dcMan.PumpsActivated += OnDcReachedSection;
                    }
                    else
                    {
                        dcMan.PumpsActivated += OnDcReachedSection;
                    }
                    break;
                case EObjectiveType.ReachCriticalDamage:
                    eventMan.CriticalDamageReached -= OnCriticalDamageReached;
                    eventMan.CriticalDamageReached += OnCriticalDamageReached;
                    break;
                case EObjectiveType.FixAllIssues:
                    sectionMan.AllIssuesFixed -= OnAllIssuesFixed;
                    sectionMan.AllIssuesFixed += OnAllIssuesFixed;
                    break;
                case EObjectiveType.RevealBlocks:
                    group.Clear();
                    if (progress != null)
                    {
                        foreach (int id in progress)
                        {
                            group.Add(id);
                        }
                    }
                    tacticMan.ObjectIdentified -= OnObjectIdentified;
                    tacticMan.ObjectIdentified += OnObjectIdentified;
                    foreach (int id in Data.Targets)
                    {
                        OnObjectIdentified(id);
                    }
                    break;
                case EObjectiveType.EnemyProximity:
                    tacticMan.RegisterProximity(this, tacticMan.GetShip(Data.Targets[0]), Data.Range, OnProximityChanged);
                    break;
                case EObjectiveType.DestroyBlockCount:
                    count = progress == null ? 0 : progress[0];
                    tacticMan.BlockDestroyed -= OnBlockCountDestroyed;
                    tacticMan.BlockDestroyed += OnBlockCountDestroyed;
                    break;
                case EObjectiveType.OrderSendMission:
                    tacticMan.OrderMissionSent -= OnOrderMissionSent;
                    tacticMan.OrderMissionSent += OnOrderMissionSent;
                    break;
                case EObjectiveType.CustomEscortDestroyed:
                    strikeGroupMan.CustomEscortDestroyed -= OnCustomEscortDestroyed;
                    strikeGroupMan.CustomEscortDestroyed += OnCustomEscortDestroyed;
                    break;
                case EObjectiveType.RevealUO:
                    tacticMan.UORevealed -= OnUORevealed;
                    tacticMan.UORevealed += OnUORevealed;
                    break;
                case EObjectiveType.SeekAndDestroyReady:
                    tacticMan.SearchAndDestroyReady -= OnSearchAndDestroyReady;
                    tacticMan.SearchAndDestroyReady += OnSearchAndDestroyReady;
                    break;
                case EObjectiveType.RescueSpecificSurvivor:
                    tacticMan.SurvivorObjectFinished -= OnSpecificSurvivorObjectFinished;
                    tacticMan.SurvivorObjectFinished += OnSpecificSurvivorObjectFinished;
                    break;
                case EObjectiveType.SetSpecificCourse:
                    customPosition = tacticMan.MapNodes.Find(Data.Targets[0]).Position;
                    tacticalMap.WaypointPositionAdded -= OnWaypointPositionAdded;
                    tacticalMap.WaypointPositionAdded += OnWaypointPositionAdded;
                    break;
            }
        }
        progress = null;
    }

    private void Deactivate()
    {
        if (Activated)
        {
#if ALLOW_CHEATS
            if (HudManager.Instance.ObjectiveLogs)
            {
                Debug.LogWarning($"{Data.Name} DEactivated");
            }
#endif
            Activated = false;

            var tacticMan = TacticManager.Instance;
            var strikeGroupMan = StrikeGroupManager.Instance;
            var enemyAttacksMan = EnemyAttacksManager.Instance;
            var timeMan = TimeManager.Instance;
            var camMan = CameraManager.Instance;
            var islandsMan = IslandsAndOfficersManager.Instance;
            var hudMan = HudManager.Instance;
            var tacticalMap = TacticalMap.Instance;
            var crewMan = CrewManager.Instance;
            var deck = AircraftCarrierDeckManager.Instance;
            var eventMan = EventManager.Instance;
            var dcMan = DamageControlManager.Instance;
            var sectionMan = SectionRoomManager.Instance;
            switch (Data.Type)
            {
                case EObjectiveType.CompleteObjective:
                    ObjectivesManager.Instance.ObjectiveFinished -= OnObjectiveFinished;
                    break;
                case EObjectiveType.Destroy:
                    tacticMan.ObjectDestroyed -= OnObjectDestroyed;
                    break;
                case EObjectiveType.DestroyBlock:
                    tacticMan.BlockDestroyed -= OnBlockDestroyed;
                    break;
                case EObjectiveType.Escort:
                    strikeGroupMan.NeutralShipDestroyed += OnNeutralShipDestroyed;
                    break;
                case EObjectiveType.Hide:
                    enemyAttacksMan.DetectedChanged -= OnDetectedChanged;
                    break;
                case EObjectiveType.StayHidden:
                    enemyAttacksMan.KindaDetected -= OnKindaDetected;
                    break;
                case EObjectiveType.Reach:
                    if (Data.SecondaryTarget == -1)
                    {
                        tacticMan.RemoveTriggers(this);
                    }
                    else
                    {
                        tacticMan.DestinationReached -= OnDestinationReached;
                    }
                    break;
                case EObjectiveType.ReachMiles:
                    timeMan.RemoveTickable(this);
                    break;
                case EObjectiveType.Reveal:
                    tacticMan.ObjectVisibilityChanged -= OnObjectVisibilityChanged;
                    break;
                case EObjectiveType.Time:
                    timeMan.RemoveTickable(this);
                    break;
                case EObjectiveType.ReachMapEdge:
                    tacticMan.SetShowMapEdges(false);
                    tacticMan.EdgeMapReached -= OnEdgeMapReached;
                    break;
                case EObjectiveType.ReachMapCorner:
                    tacticMan.SetMapCornerTrigger(null);
                    break;
                case EObjectiveType.FinishCustomMission:
                    tacticMan.CustomMissionFinished -= OnCustomMissionFinished;
                    break;
                case EObjectiveType.Custom:
                    break;
                case EObjectiveType.FinishMissions:
                    tacticMan.MissionFinished -= OnMissionFinished;
                    break;
                case EObjectiveType.MissionAfterAction:
                    tacticMan.MissionAction -= OnMissionAction;
                    break;
                case EObjectiveType.RescueSurvivors:
                    tacticMan.SurvivorObjectFinished -= OnSurvivorObjectFinished;
                    break;
                case EObjectiveType.SendAirstrikeWithoutLosses:
                    tacticMan.AirstrikeAttacked -= OnAirstrikeAttack;
                    break;
                case EObjectiveType.UseStrikeGroupActiveSkill:
                    strikeGroupMan.StrikeGroupActiveSkillUsed -= OnStrikeGroupActiveSkillUsed;
                    break;
                case EObjectiveType.EnemyAttackSuccess:
                    enemyAttacksMan.EnemyAttackSucceed -= OnEnemyAttackSucceed;
                    break;
                case EObjectiveType.ZoomCamera:
                    camMan.CameraZoomed -= OnCameraZoomed;
                    break;
                case EObjectiveType.MoveCamera:
                    camMan.CameraMoved -= OnCameraMoved;
                    break;
                case EObjectiveType.CameraView:
                    camMan.CameraTargetChanged -= OnCameraTargetChanged;
                    break;
                case EObjectiveType.OfficerReachedRoom:
                    islandsMan.RoomReached -= OnRoomsReached;
                    break;
                case EObjectiveType.UseSwitch:
                    islandsMan.SwitchUsed -= OnSwitchUsed;
                    break;
                case EObjectiveType.OpenWindow:
                    hudMan.WindowStateChanged -= OnWindowOpened;
                    break;
                case EObjectiveType.SetCourse:
                    tacticalMap.WaypointAdded -= OnWaypointAdded;
                    break;
                case EObjectiveType.SetCarrierSpeed:
                    hudMan.ShipSpeedChanged -= OnShipSpeedChanged;
                    break;
                case EObjectiveType.UseBuffOrder:
                    islandsMan.BuffConfirmed -= OnBuffConfirmed;
                    islandsMan.BuffExpired -= OnBuffExpired;
                    break;
                case EObjectiveType.DragCrew:
                    crewMan.CrewDragged -= OnCrewDragged;
                    break;
                case EObjectiveType.FinishCrewReposition:
                    crewMan.RepositionFinished -= OnRepositionFinished;
                    break;
                case EObjectiveType.DeploySquadrons:
                    deck.SquadronCreated -= OnSquadronCreated;
                    break;
                case EObjectiveType.PlanMission:
                    tacticMan.MissionPlanned -= OnMissionPlanned;
                    break;
                case EObjectiveType.SendMission:
                    tacticMan.MissionSent -= OnMissionSent;
                    break;
                case EObjectiveType.HoverMission:
                    tacticMan.MissionPanel.MissionDetails.MissionHovered -= OnMissionHovered;
                    break;
                case EObjectiveType.ChangeDeckState:
                    deck.DeckModeChanged -= OnDeckModeChanged;
                    break;
                case EObjectiveType.StartPlanningMission:
                    tacticMan.MissionPlanningStarted -= OnMissionPlanningStarted;
                    break;
                case EObjectiveType.MakeForcedStrategy:
                    tacticMan.StrategySelectionPanel.ForcedStrategySet -= OnForcedStrategySet;
                    break;
                case EObjectiveType.SetRecoveryPosition:
                    tacticalMap.MissionAreasSet -= OnMissionAreasSet;
                    break;
                case EObjectiveType.ManeuverCategory:
                    tacticMan.StrategySelectionPanel.CategorySwitched -= OnManeuverCategorySwitched;
                    break;
                case EObjectiveType.StartCourseSettingMode:
                    tacticalMap.CourseSettingModeChanged -= OnCourseSettingModeChanged;
                    break;
                case EObjectiveType.ChooseBuff:
                    islandsMan.BuffSetupReady -= OnBuffSetupStarted;
                    break;
                case EObjectiveType.FinishBuffOfficerAssign:
                    islandsMan.BuffSetupReady -= OnBuffSetupReady;
                    break;
                case EObjectiveType.ClickEvent:
                    eventMan.EventClicked -= OnEventClicked;
                    break;
                case EObjectiveType.ClickDcCategory:
                    dcMan.DcCategoryClicked -= OnDcCategoryClicked;
                    break;
                case EObjectiveType.FixIssue:
                    sectionMan.IssueFixed -= OnIssueFixed;
                    break;
                case EObjectiveType.DcReachMaintenance:
                    dcMan.MaintenanceActivated -= OnDcReachedSection;
                    dcMan.PumpsActivated -= OnDcReachedSection;
                    break;
                case EObjectiveType.ReachCriticalDamage:
                    eventMan.CriticalDamageReached -= OnCriticalDamageReached;
                    break;
                case EObjectiveType.FixAllIssues:
                    sectionMan.AllIssuesFixed -= OnAllIssuesFixed;
                    break;
                case EObjectiveType.RevealBlocks:
                    tacticMan.ObjectIdentified -= OnObjectIdentified;
                    break;
                case EObjectiveType.EnemyProximity:
                    tacticMan.UnregisterProximity(this);
                    break;
                case EObjectiveType.DestroyBlockCount:
                    tacticMan.BlockDestroyed -= OnBlockCountDestroyed;
                    break;
                case EObjectiveType.OrderSendMission:
                    tacticMan.OrderMissionSent -= OnOrderMissionSent;
                    break;
                case EObjectiveType.CustomEscortDestroyed:
                    strikeGroupMan.CustomEscortDestroyed -= OnCustomEscortDestroyed;
                    break;
                case EObjectiveType.RevealUO:
                    tacticMan.UORevealed -= OnUORevealed;
                    break;
                case EObjectiveType.SeekAndDestroyReady:
                    tacticMan.SearchAndDestroyReady -= OnSearchAndDestroyReady;
                    break;
                case EObjectiveType.RescueSpecificSurvivor:
                    tacticMan.SurvivorObjectFinished -= OnSpecificSurvivorObjectFinished;
                    break;
                case EObjectiveType.SetSpecificCourse:
                    tacticalMap.WaypointPositionAdded -= OnWaypointPositionAdded;
                    break;
            }
        }
    }

    private void SetShow(bool show)
    {
        if (Visible != show)
        {
            Visible = show;
            ObjectivesManager.Instance.SetShowObjective(Index, show);
        }
    }

    private void SetEnableSpreadIssue(EIssue issue, SectionSegment segment, bool spread)
    {
        var paramsMan = Parameters.Instance;
        switch (issue)
        {
            case EIssue.Fire:
                segment.Fire.SpreadData.Max = spread ? paramsMan.FireSpreadTimeTicks : 1e9f;
                break;
            case EIssue.Flood:
                segment.Group.SpreadTime = spread ? paramsMan.FloodSpreadTickTime : 100_000_000;
                break;
            case EIssue.Fault:
                segment.Damage.SpreadData.Max = spread ? paramsMan.FaultSpreadTickTime : 1e9f;
                break;
            case EIssue.Any:
                SetEnableSpreadIssue(EIssue.Fire, segment, spread);
                SetEnableSpreadIssue(EIssue.Flood, segment, spread);
                SetEnableSpreadIssue(EIssue.Fault, segment, spread);
                break;
        }
    }

    private void OnObjectiveFinished(int id, bool success)
    {
        DebugCheck();

        if (Data.Targets.Contains(id))
        {
            CheckCount(success);
        }
    }

    private void OnObjectDestroyed(int id, bool ok)
    {
        DebugCheck();
        if (ok && Data.Targets.Contains(id))
        {
            CheckCount(true);
        }
    }

    private void OnBlockDestroyed(EnemyManeuverData block)
    {
        DebugCheck();

        if (block != Data.TargetBlock)
        {
            return;
        }

        count = 0;
        var tacticMan = TacticManager.Instance;
        foreach (int id in Data.Targets)
        {
            bool found = false;
            bool destroyed = true;
            foreach (var data in tacticMan.GetShip(id).Blocks)
            {
                if (data.Data == this.Data.TargetBlock)
                {
                    found = true;
                    destroyed = destroyed && data.Dead;
                }
            }
            if (found && destroyed)
            {
                CheckCount(true);
            }
        }
    }

    private void OnNeutralShipDestroyed()
    {
        DebugCheck();
        ObjectivesManager.Instance.FinishObjective(Index, false);
    }

    private void OnDetectedChanged(bool detected)
    {
        DebugCheck();
        if (!detected)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnKindaDetected()
    {
        DebugCheck();
        var enemyAttacksMan = EnemyAttacksManager.Instance;
        if (enemyAttacksMan.IsDetected || enemyAttacksMan.AlreadyDetected)
        {
            ObjectivesManager.Instance.FinishObjective(Index, false);
        }
    }

    private void OnDestinationReached()
    {
        DebugCheck();
        CheckCount(true);
        count = Mathf.Max(count, 0);
        if (!Finished)
        {
            TacticManager.Instance.Trigger(this, Data.Targets[count], OnDestinationReached, Data.Range);
        }
    }

    private void OnAnyDestinationReached(int id)
    {
        DebugCheck();
        group.Add(id);
        count = group.Count - 1;
        CheckCount(true);
    }

    private void OnDestinationReached(int enemy, int node)
    {
        DebugCheck();
        if (Data.SecondaryTarget == enemy && Data.Targets.Contains(node))
        {
            CheckCount(true);
        }
    }

    private void OnObjectVisibilityChanged(int id, bool visible)
    {
        DebugCheck();
        if (!Data.Targets.Contains(id))
        {
            return;
        }
        if (visible)
        {
            group.Add(id);
        }
        else if (Data.Simultaneously)
        {
            group.Remove(id);
        }
        count = group.Count - 1;
        CheckCount(!Data.NotType);
    }

    private void OnEdgeMapReached()
    {
        ObjectivesManager.Instance.FinishObjective(Index, !Data.NotType);
    }

    private void OnCustomMissionFinished(bool success)
    {
        ObjectivesManager.Instance.FinishObjective(Index, success != Data.NotType);
    }

    private void OnMissionFinished(EMissionOrderType type, bool success, object target)
    {
        if (Data.MissionType != type)
        {
            return;
        }
        if ((Data.NotType && !success) || Data.Targets.Count == 0)
        {
            CheckCount(success);
        }
        else if (success && target is TacticalEnemyShip enemy && Data.Targets.Contains(enemy.Id))
        {
            group.Add(enemy.Id);

            count = group.Count - 1;
            CheckCount(true);
        }
    }

    private void OnMissionAction(TacticalMission mission)
    {
        if (Data.MissionType == mission.OrderType)
        {
            var objMan = ObjectivesManager.Instance;
            if (mission.OrderType == EMissionOrderType.Decoy)
            {
                if (Data.Targets.Count == 0)
                {
                    Debug.LogError("Decoy objective needs a target");
                }
                var tacMan = TacticManager.Instance;
                bool success = false;
                var decoyEnemies = mission.GetEnemiesInDecoyRange();
                for (int i = 0; i < Data.Targets.Count; i++)
                {
                    if (decoyEnemies.Contains(tacMan.GetShip(Data.Targets[i])))
                    {
                        success = true;
                        objMan.FinishObjective(Index, true);
                        break;
                    }
                }
                if (!success && !(tacMan.Missions[mission.OrderType].Count > 0))
                {
                    objMan.FinishObjective(Index, false);
                }
            }
            else
            {
                objMan.FinishObjective(Index, true);
            }
        }
    }

    private void OnSurvivorObjectFinished(int _, bool success)
    {
        CheckCount(success);
    }

    private void OnAirstrikeAttack(bool success, TacticalEnemyShip enemyShip)
    {
        if (success && enemyShip == TacticManager.Instance.GetShip(Data.Targets[0]))
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnStrikeGroupActiveSkillUsed(EStrikeGroupActiveSkill activeSkill)
    {
        if (Data.StrikeGroupActiveSkill == activeSkill)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnEnemyAttackSucceed()
    {
        ObjectivesManager.Instance.FinishObjective(Index, false);
    }

    private void OnCameraZoomed(bool zoomIn)
    {
        CheckCount(zoomIn);
    }

    private void OnCameraMoved()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnCameraTargetChanged(ECameraView view)
    {
        if (Data.CameraView == view)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnRoomsReached(EIslandRoomType type)
    {
        if (Data.RoomType == type)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnSwitchUsed()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnWindowOpened(EWindowType type, bool opened)
    {
        if (Data.WindowType == type)
        {
            CheckCount(opened);
        }
    }

    private void OnWaypointAdded()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnShipSpeedChanged(int speed)
    {
        if (Data.IndexValue == speed)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnBuffConfirmed()
    {
        CheckCount(true);
    }

    private void OnBuffExpired()
    {
        CheckCount(false);
    }

    private void OnCrewDragged(int crewIndex)
    {
        if (Data.IndexValue == crewIndex)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnRepositionFinished()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnSquadronCreated(EPlaneType type)
    {
        if (type == Data.SquadronType)
        {
            CheckCount(true);
        }
    }

    private void OnMissionPlanned(EMissionOrderType mission)
    {
        if (mission == Data.MissionType)
        {
            CheckCount(true);
        }
    }

    private void OnMissionSent(EMissionOrderType mission)
    {
        if (mission == Data.MissionType)
        {
            CheckCount(true);
        }
    }

    private void OnMissionHovered(EMissionOrderType mission)
    {
        if (mission == Data.MissionType)
        {
            CheckCount(true);
        }
    }

    private void OnDeckModeChanged()
    {
        CheckCount(AircraftCarrierDeckManager.Instance.DeckMode == EDeckMode.Starting);
    }

    private void OnMissionPlanningStarted(EMissionOrderType mission)
    {
        if (mission == Data.MissionType)
        {
            CheckCount(true);
        }
    }

    private void OnForcedStrategySet()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnMissionAreasSet()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnManeuverCategorySwitched(EManeuverType type)
    {
        if (Data.ManeuverType == EManeuverType.Any || type == Data.ManeuverType)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnCourseSettingModeChanged()
    {
        if (TacticalMap.Instance.CourseSettingMode)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnBuffSetupStarted()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnBuffSetupReady()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnEventClicked()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnDcCategoryClicked()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnIssueFixed()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnDcReachedSection()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnCriticalDamageReached()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnAllIssuesFixed()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnObjectIdentified(int id)
    {
        if (!Data.Targets.Contains(id))
        {
            return;
        }
        foreach (var block in TacticManager.Instance.GetShip(id).Blocks)
        {
            if (!block.Visible)
            {
                return;
            }
        }
        DebugCheck();
        group.Add(id);
        count = group.Count - 1;
        CheckCount(!Data.NotType);
    }

    private void OnProximityChanged(bool near)
    {
        if (near != Data.NotType)
        {
            ObjectivesManager.Instance.FinishObjective(Index, true);
        }
    }

    private void OnBlockCountDestroyed(EnemyManeuverData block)
    {
        DebugCheck();

        if (block != Data.TargetBlock)
        {
            return;
        }
        CheckCount(!Data.NotType);
    }

    private void OnOrderMissionSent(EMissionOrderType mission)
    {
        if (mission == Data.MissionType)
        {
            CheckCount(true);
        }
    }

    private void OnCustomEscortDestroyed()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnUORevealed()
    {
        CheckCount(true);
    }

    private void OnSearchAndDestroyReady()
    {
        ObjectivesManager.Instance.FinishObjective(Index, true);
    }

    private void OnSpecificSurvivorObjectFinished(int id, bool success)
    {
        if (id == Data.IndexValue)
        {
            CheckCount(success);
        }
    }

    private void OnWaypointPositionAdded(Vector2 position)
    {
        if (TacticManager.Instance.Map.CustomWaypointNear(position, customPosition))
        {
            CheckCount(true);
        }
    }

    private void CheckCount(bool success)
    {
        var objMan = ObjectivesManager.Instance;
        switch (Data.TargetType)
        {
            case EObjectiveTarget.Number:
                if (success == Data.NotType)
                {
                    if (Data.FailOnNegativeTarget)
                    {
                        //Assert.IsFalse(data.Targets.Count == 0);
                        failCount++;
                        if ((Data.Targets.Count - failCount) < Data.Count)
                        {
                            objMan.FinishObjective(Index, false);
                        }
                    }
                }
                else
                {
                    if (++count >= Data.Count)
                    {
                        objMan.FinishObjective(Index, true);
                    }
                }
                break;
            case EObjectiveTarget.All:
                if (success == Data.NotType)
                {
                    if (Data.FailOnNegativeTarget)
                    {
                        objMan.FinishObjective(Index, false);
                    }
                }
                else
                {
                    if (++count >= Data.Targets.Count)
                    {
                        objMan.FinishObjective(Index, true);
                    }
                }
                break;
        }
    }

    private void DebugCheck()
    {
        Assert.IsTrue(Activated);
        Assert.IsFalse(Finished);
    }
}
