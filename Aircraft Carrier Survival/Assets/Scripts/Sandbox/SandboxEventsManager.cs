using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxEventsManager : MonoBehaviour
{
    [SerializeField]
    private List<SandboxEvent> sandboxEvents = null;

    [SerializeField, Range(0f, 1f)]
    private float eventChance = 0.15f;
    [SerializeField]
    private int hoursToDraw = 12;

    private int ticksPassed;
    private SandboxEventConsequence chosenConsequence;
    private SandboxEvent drawnEvent;
    private List<SandboxEvent> basket = new List<SandboxEvent>();

    public void Setup()
    {
        basket = new List<SandboxEvent>(sandboxEvents);
        UIManager.Instance.SandboxEventPopup.ButtonClicked += Consequence;
        var saveData = SaveManager.Instance.Data;
        if (saveData.SandboxData.IsSaved)
        {
            drawnEvent = sandboxEvents[saveData.SandboxData.DrawnEvent];
            chosenConsequence = drawnEvent.Consequences[saveData.SandboxData.ChosenConsequence];
        }
        foreach (var e in sandboxEvents)
        {
            e.EventIndex = sandboxEvents.IndexOf(e);
            for (int i = 0; i < e.Consequences.Count; i++)
            {
                e.Consequences[i].EventIndex = e.EventIndex;
                e.Consequences[i].ConsequenceIndex = i;
            }
        }
    }

    public void Save(ref SandboxSaveData data)
    {
        data.EventDrawHoursPassed = ticksPassed;
        if (drawnEvent != null && chosenConsequence != null)
        {
            data.DrawnEvent = sandboxEvents.IndexOf(drawnEvent);
            data.ChosenConsequence = drawnEvent.Consequences.IndexOf(chosenConsequence);
        }
    }

    public void Tick()
    {
        if (!SaveManager.Instance.Data.SandboxData.CurrentMissionSaveData.MapInstanceInProgress && drawnEvent == null && basket.Count > 0 &&
            (ticksPassed += TimeManager.Instance.WorldMapTickQuotient) >= hoursToDraw * TimeManager.Instance.TicksForHour)
        {
            ticksPassed = 0;
            Draw();
        }
    }

    public void MapInstanceEntranceConsequence()
    {
        if (chosenConsequence != null)
        {
            switch (chosenConsequence.PossibleCosts[chosenConsequence.DrawnCostIndex].ConsequenceType)
            {
                case ESandboxEventConsequence.CrewMemberInjured:
                    for (int i = 0; i < chosenConsequence.PossibleCosts[chosenConsequence.DrawnCostIndex].Value; i++)
                    {
                        CrewStatusManager.Instance.AddInjured(RandomUtils.GetRandom(CrewManager.Instance.CrewUnits));
                    }
                    break;
                case ESandboxEventConsequence.CrewMemberKilled:
                    break;
                case ESandboxEventConsequence.EscortShipDamaged:
                    break;
                case ESandboxEventConsequence.EscortShipDestroyed:
                    break;
                case ESandboxEventConsequence.EscortShipToPearlHarbour:
                    break;
                case ESandboxEventConsequence.SquadronsLost:
                    break;
                case ESandboxEventConsequence.Time:
                    break;
            }
        }
    }

    private void Consequence(SandboxEventConsequence consequence)
    {
        chosenConsequence = consequence;
        float rand = Random.Range(0f, 1f);
        float chance = 0f;
        for (int i = 0; i < consequence.PossibleCosts.Count; i++)
        {
            if (rand - chance < consequence.PossibleCosts[i].Chance)
            {
                consequence.DrawnCostIndex = i;
            }
            chance = consequence.PossibleCosts[i].Chance;
        }
        var drawnCost = consequence.PossibleCosts[consequence.DrawnCostIndex];
        var strikeGroupMan = StrikeGroupManager.Instance;
        switch (drawnCost.ConsequenceType)
        {
            case ESandboxEventConsequence.CrewMemberInjured:
                break;
            case ESandboxEventConsequence.CrewMemberKilled:
                for (int i = 0; i < drawnCost.Value; i++)
                {
                    CrewStatusManager.Instance.KillCrew(RandomUtils.GetRandom(CrewManager.Instance.CrewUnits));
                }
                break;
            case ESandboxEventConsequence.EscortShipDamaged:
                strikeGroupMan.DamageRandom(drawnCost.Value);
                break;
            case ESandboxEventConsequence.EscortShipDestroyed:
                strikeGroupMan.DamageRandom(99);
                break;
            case ESandboxEventConsequence.EscortShipToPearlHarbour:
                strikeGroupMan.SendBackToPearlHarbour(RandomUtils.GetRandom(strikeGroupMan.AliveMembers));
                break;
            case ESandboxEventConsequence.SquadronsLost:
                AircraftCarrierDeckManager.Instance.DestroySquadrons(drawnCost.Value);
                break;
            case ESandboxEventConsequence.Time:
                (WorldMap.Instance.MapShip as WorldMapShip).SetBlockedTime(drawnCost.Value);
                break;
            case ESandboxEventConsequence.CommandPointsLost:
                SaveManager.Instance.Data.IntermissionData.CommandPoints += drawnCost.Value;
                break;
            case ESandboxEventConsequence.EnemySpawned:
                SandboxManager.Instance.WorldMapFleetsManager.SpawnFleet(true);
                break;
            case ESandboxEventConsequence.Wait6HAndWpawnEnemy:
                (WorldMap.Instance.MapShip as WorldMapShip).SetBlockedTime(6);
                SandboxManager.Instance.WorldMapFleetsManager.SpawnFleet(true);
                break;
            case ESandboxEventConsequence.Wait6HAndBuffAttackCards:
                (WorldMap.Instance.MapShip as WorldMapShip).SetBlockedTime(6);
                TacticManager.Instance.SetBonusConsequenceManeuversAttack(drawnCost.Value);
                break;
            case ESandboxEventConsequence.SuppliesChanged:
                ResourceManager.Instance.ChangeSuppliesAmount(drawnCost.Value);
                break;
            case ESandboxEventConsequence.OfficerBlocked:
                IslandsAndOfficersManager.Instance.BlockOfficer(drawnCost.Value);
                break;
            case ESandboxEventConsequence.BlockTwoOfficers:
                IslandsAndOfficersManager.Instance.BlockOfficer(drawnCost.Value);
                IslandsAndOfficersManager.Instance.BlockOfficer(drawnCost.Value);
                break;
            case ESandboxEventConsequence.BlockDefensivePosition:
                IslandsAndOfficersManager.Instance.IslandBuffs[EIslandBuff.EnterDefencePosition].SetInteractable(false);
                break;
        }
        UIManager.Instance.SandboxEventResultPopup.Setup(consequence);
    }

    private void Draw()
    {
        if (Random.value <= eventChance)
        {
            CreateEvent();
        }
    }

    private void CreateEvent()
    {
        drawnEvent = RandomUtils.GetRandom(basket);
        while (!UIManager.Instance.SandboxEventPopup.Setup(drawnEvent))
        {
            basket.Remove(drawnEvent);
            if (basket.Count == 0)
            {
                drawnEvent = null;
                return;
            }
            drawnEvent = RandomUtils.GetRandom(basket);
        }
    }
}
