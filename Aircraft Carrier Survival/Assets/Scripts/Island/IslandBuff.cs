using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class IslandBuff
{
    private event Action Start = delegate { };
    private event Action Finish = delegate { };

    public List<Officer> AssignedOfficers => assignedOfficers;

    public IslandBuffUIElement IslandBuffUIElement
    {
        get;
        set;
    }

    public EIslandBuff IslandBuffType;
    public int Air, Ship, CooldownInTicks;
    public EIslandBuffEffectParam param;
    public int ParamValue;

    public Sprite Icon;
    public bool BaseUnlocked = false;

    [HideInInspector]
    public bool Unlocked;

    [SerializeField]
    private bool sandboxUnlocked = false;

    private List<Officer> assignedOfficers = new List<Officer>();
    private bool active;

    private DCInstanceGroup group;

    public void Cancel()
    {
        if (!active)
        {
            RemoveOfficers(false);
        }
        IslandBuffUIElement.ActiveImage.SetActive(false);
    }

    public bool AssignOfficer(Officer officer)
    {
        AssignedOfficers.Add(officer);
        officer.Assigned = true;
        var islMan = IslandsAndOfficersManager.Instance;
        islMan.ShipPoints += officer.GetSkillLevel(EIslandRoomCategory.SHIP);
        islMan.AirPoints += officer.GetSkillLevel(EIslandRoomCategory.AIR);
        return islMan.ShipPoints >= Ship && islMan.AirPoints >= Air;
    }

    public bool RemoveOfficer(Officer officer, bool updatePoints)
    {
        officer.Assigned = false;
        officer.Occupied = false;
        var islMan = IslandsAndOfficersManager.Instance;
        if (updatePoints)
        {
            islMan.ShipPoints -= officer.GetSkillLevel(EIslandRoomCategory.SHIP);
            islMan.AirPoints -= officer.GetSkillLevel(EIslandRoomCategory.AIR);
        }
        return islMan.ShipPoints >= Ship && islMan.AirPoints >= Air;
    }

    public void RemoveOfficers(bool moveOfficers)
    {
        foreach (var officer in AssignedOfficers)
        {
            RemoveOfficer(officer, false);
            if (moveOfficers)
            {
                if (officer.LastIslandRoom != null && officer.LastIslandRoom.CanAddOfficer(officer))
                {
                    officer.LastIslandRoom.AddOfficer(officer);
                }
                else
                {
                    IslandsAndOfficersManager.Instance.GetRandomAvailableIslandRoom(officer).AddOfficer(officer);
                }
                officer.Cooldown = 0;
            }
        }
        AssignedOfficers.Clear();
    }

    public void Setup(IslandBuffUIElement islandBuffUIElement)
    {
        IslandBuffUIElement = islandBuffUIElement;
        var tacMan = TacticManager.Instance;
        var enemyAttacksMan = EnemyAttacksManager.Instance;
        var dcMan = DamageControlManager.Instance;
        var deckMan = AircraftCarrierDeckManager.Instance;
        var crewMan = CrewManager.Instance;
        var hudMan = HudManager.Instance;
        var timeMan = TimeManager.Instance;
        var parameters = Parameters.Instance;
        switch (IslandBuffType)
        {
            case EIslandBuff.Resupply:
                Start += () =>
                {
                    ResourceManager.Instance.RefillSupplies();
                    hudMan.SwitchSpeed(false);
                };
                Finish += () =>
                {
                    hudMan.SwitchSpeed(true);
                };
                break;
            case EIslandBuff.AddDC:
                Start += () =>
                {
                    dcMan.AddTempGroups(parameters.AddDcBuff);
                };
                Finish += () =>
                {
                    dcMan.RemoveTempGroups(parameters.AddDcBuff);
                };
                break;
            case EIslandBuff.IncreaseSpottingRange:
                Start += () =>
                {
                    tacMan.Carrier.SetIslandMultiplierBonus(parameters.IncreaseSpottingRange);
                };
                Finish += () =>
                {
                    tacMan.Carrier.SetIslandMultiplierBonus(1f);
                };
                break;
            case EIslandBuff.InstantMove:
                Start += () =>
                {
                    crewMan.InstantReassign = true;
                };
                Finish += () =>
                {
                    crewMan.InstantReassign = false;
                };
                break;
            case EIslandBuff.EnterDefencePosition:
                Start += () =>
                {
                    enemyAttacksMan.SetIslandBuffDefencePoints(parameters.EnterDefencePositionCarrier);
                    enemyAttacksMan.SetIslandBuffEscortPoints(parameters.EnterDefencePositionEscort);
                };
                Finish += () =>
                {
                    enemyAttacksMan.SetIslandBuffDefencePoints(-parameters.EnterDefencePositionCarrier);
                    enemyAttacksMan.SetIslandBuffEscortPoints(-parameters.EnterDefencePositionEscort);
                };
                break;
            case EIslandBuff.AACrewSwap:
                Start += () =>
                {
                    crewMan.ChangeAACooldown(1);
                };
                break;
            case EIslandBuff.HeavyManeuvers:
                Start += () =>
                {
                    deckMan.SetBlockOrders(true);
                    enemyAttacksMan.SetIslandBuffDefencePoints(parameters.HeavyManeuversDefence);
                };
                Finish += () =>
                {
                    deckMan.SetBlockOrders(false);
                    enemyAttacksMan.SetIslandBuffDefencePoints(-parameters.HeavyManeuversDefence);
                };
                break;
            case EIslandBuff.TacticalMissionFocus:
                Start += () =>
                {
                    var airstrike = tacMan.AddNewMission(EMissionOrderType.Airstrike);
                    airstrike.SetToObsoleteTime(parameters.TacticalMissionFocusExpireTime);
                    var recon = tacMan.AddNewMission(EMissionOrderType.Recon);
                    recon.SetToObsoleteTime(parameters.TacticalMissionFocusExpireTime);
                    airstrike.MissionToRemoveOnConfirm = recon;
                    recon.MissionToRemoveOnConfirm = airstrike;
                    airstrike.ExtraMission = true;
                    recon.ExtraMission = true;
                };
                break;
            case EIslandBuff.SurpriseAttack:
                enemyAttacksMan.DetectedChanged += SurpriseAttackInteractable;
                Start += () =>
                {
                    enemyAttacksMan.DetectedChanged -= SurpriseAttackInteractable;
                    tacMan.HasManeuversAttackBuff = true;
                    tacMan.BonusManeuversAttackBuff = parameters.SurpriseAttackManeuversAttack;
                };
                Finish += () =>
                {
                    tacMan.HasManeuversAttackBuff = false;
                    SurpriseAttackInteractable(enemyAttacksMan.IsDetected);
                    enemyAttacksMan.DetectedChanged += SurpriseAttackInteractable;
                };
                break;
            case EIslandBuff.CounterAttack:
                enemyAttacksMan.AttackFinished += CounterAttackInteractable;
                Start += () =>
                {
                    tacMan.AddNewMission(EMissionOrderType.Airstrike).SetToObsoleteTime(parameters.CounterAttackExpireTime);
                };
                break;
            case EIslandBuff.DecisiveBlow:
                Start += () =>
                {
                    tacMan.IslandBuffBonusManeuversDefence = parameters.DecisiveBlowManeuversDefence;
                };
                Finish += () =>
                {
                    tacMan.IslandBuffBonusManeuversDefence = 0;
                };
                break;
            case EIslandBuff.PlaneFuelManagement:
                Start += () =>
                {
                    tacMan.BonusRetrievalRange = parameters.PlaneFuelManagementRetrievalRange;
                    tacMan.RetrievalTimeModifier = parameters.PlaneFuelManagementRetrievalTimeModifier;
                };
                Finish += () =>
                {
                    tacMan.BonusRetrievalRange = 0;
                    tacMan.RetrievalTimeModifier = 1f;
                };
                break;
            case EIslandBuff.DeckControl:
                Start += () =>
                {
                    deckMan.BlockCrashes = true;
                    SectionSegment segment = null;
                    foreach (var segment2 in dcMan.WreckSection.GetAllSegments(true))
                    {
                        segment = segment2;
                        if (segment.DcCanEnter())
                        {
                            break;
                        }
                    }
                    if (!segment.DcCanEnter())
                    {
                        segment = SectionRoomManager.Instance.FindEmptySegment(null, segment);
                    }
                    Assert.IsNull(group);
                    group = dcMan.SpawnDcInSegment(segment);
                    group.CrashOnly = true;
                    group.Category = EDcCategory.Crash;
                    group.Portrait.gameObject.SetActive(false);
                    group.Button.gameObject.SetActive(false);
                };
                Finish += () =>
                {
                    deckMan.BlockCrashes = false;

                    group.CrashOnly = false;
                    dcMan.DespawnDC(group);
                    group = null;
                };
                break;
            case EIslandBuff.NightShift:
                timeMan.IsDayChanged += IsDayChanged;
                Start += () =>
                {
                    CooldownInTicks = timeMan.GetTicksToNightFinish();
                    dcMan.SpawnDC();
                    dcMan.SetNightShiftRepairSpeedModifier(parameters.NightShiftDcSpeedModifier);
                };
                Finish += () =>
                {
                    dcMan.DespawnDC();
                    dcMan.SetNightShiftRepairSpeedModifier(0f);
                    IsDayChanged();
                };
                break;
            case EIslandBuff.HastenedRepairs:
                Start += () =>
                {
                    dcMan.SetHastenedRepairsSpeedModifier(parameters.HastenedRepairsDcSpeedModifier);
                    dcMan.IslandInjuryChanceModifier = parameters.HastenedRepairsInjuryChanceModifier;
                };
                Finish += () =>
                {
                    dcMan.SetHastenedRepairsSpeedModifier(0f);
                    dcMan.IslandInjuryChanceModifier = 0f;
                };
                break;
            case EIslandBuff.CodeBreakers:
                Start += () =>
                {
                    int id = tacMan.GetRandomEnemyId();
                    if (id != -1)
                    {
                        tacMan.RevealObject(id);
                        tacMan.IdentifyObject(id, false);
                        ReportPanel.Instance.SetupIdentifyTarget(tacMan.GetShip(id), EMissionOrderType.IdentifyTargets);
                    }
                    else
                    {
                        ReportPanel.Instance.IdentifyFoundNothingSetup(EMissionOrderType.IdentifyTargets);
                    }
                };
                Finish += () =>
                {
                };
                break;
            case EIslandBuff.RepairSection:
                SectionRoomManager.Instance.SectionBrokenChanged += OnSectionBrokenChanged;
                Start += () =>
                {
                    SectionRoomManager.Instance.BrokenSubsections.GetRandomElement().IsBroken = false;
                };
                Finish += () =>
                {
                    OnSectionBrokenChanged();
                };
                break;
            case EIslandBuff.FireNotSpread:
                Start += () =>
                {
                    SectionRoomManager.Instance.BlockedByIslandBuff = true;
                };
                Finish += () =>
                {
                    SectionRoomManager.Instance.BlockedByIslandBuff = false;
                };
                break;
            case EIslandBuff.LeaveInstance:
                RadarManager.Instance.RadarChanged += OnRadarChanged;
                enemyAttacksMan.StartCoroutineActionAfterTime(OnRadarChanged, 1f);
                Start += () =>
                {
                };
                Finish += () =>
                {
                    AircraftCarrierDeckManager.Instance.LosePlanes();
                    GameStateManager.Instance.ShowMissionSummary(false, EMissionLoseCause.MissionAborted);
                };
                break;
        }
        var saveData = SaveManager.Instance.Data;
        if (saveData.GameMode == EGameMode.Fabular)
        {
            Unlocked = (saveData.MissionRewards.AdmiralUnlockedOrders & (1 << (int)IslandBuffType)) != 0 || BaseUnlocked;
            IslandBuffUIElement.Setup(IslandBuffType, Air, Ship, Icon, (CooldownInTicks / TimeManager.Instance.TicksForHour).ToString(), param, Unlocked);
        }
        else if (saveData.GameMode == EGameMode.Sandbox)
        {
            Unlocked = sandboxUnlocked || (saveData.MissionRewards.AdmiralUnlockedOrders & (1 << (int)IslandBuffType)) != 0;
            IslandBuffUIElement.Setup(IslandBuffType, Air, Ship, Icon, (CooldownInTicks / TimeManager.Instance.TicksForHour).ToString(), param, Unlocked);
            //UpdateSandboxSelectedBuffs();
        }
    }

    public void UpdateSandboxSelectedBuffs()
    {
        Unlocked = sandboxUnlocked || (SaveManager.Instance.Data.MissionRewards.AdmiralUnlockedOrders & (1 << (int)IslandBuffType)) != 0;
        IslandBuffUIElement.Setup(IslandBuffType, Air, Ship, Icon, (CooldownInTicks / TimeManager.Instance.TicksForHour).ToString(), param, Unlocked);
    }

    public void StartBuff()
    {
        Start();
        var islMan = IslandsAndOfficersManager.Instance;
        foreach (var officer in AssignedOfficers)
        {
            islMan.IslandRooms[EIslandRoomType.OrdersRoom].AddOfficer(officer, false, false);
            officer.Occupied = true;
        }
        active = true;
        IslandBuffUIElement.ActiveImage.SetActive(false);
        foreach (var officer in assignedOfficers)
        {
            officer.Portrait.SetCurrentBuffImage(IslandBuffUIElement.Image.sprite);
        }
        SetInteractable(false);
    }

    public void FinishBuff()
    {
        active = false;
        foreach (var officer in assignedOfficers)
        {
            officer.Portrait.SetCurrentBuffImage(null);
        }
        RemoveOfficers(true);
        SetInteractable(true);
        Finish();
    }

    public void SetInteractable(bool interactable)
    {
        IslandBuffUIElement.SetInteractable(interactable && !active);
    }

    private void SurpriseAttackInteractable(bool value)
    {
        SetInteractable(!value);
    }

    private void CounterAttackInteractable(EAttackResult result)
    {
        var islMan = IslandsAndOfficersManager.Instance;
        SetInteractable(true);
        islMan.TicksSinceLastAttack = 0;
        islMan.CountTicksSinceLastAttack = true;
    }

    private void IsDayChanged()
    {
        SetInteractable(!TimeManager.Instance.IsDay);
    }

    private void OnRadarChanged()
    {
        SetInteractable(!RadarManager.Instance.HasAnyAttack());
    }

    private void OnSectionBrokenChanged()
    {
        SetInteractable(SectionRoomManager.Instance.BrokenSubsections.Count > 0);
    }
}
