using System.Collections.Generic;

public class MissionOrder : ACOrder
{
    private List<PlaneSquadron> squadrons;

    public MissionOrder(AircraftCarrierDeckManager deck, TacticalMission mission) : base(deck, EOrderType.Mission)
    {
        Mission = mission;
    }

    public MissionOrder(AircraftCarrierDeckManager deck, DeckOrderSaveData data, bool executing) : base(deck, EOrderType.Mission)
    {
        Mission = TacticManager.Instance.GetMission(data.MissionType, data.Mission);

        if (executing)
        {
            squadrons = new List<PlaneSquadron>();
            foreach (var squadron in data.Squadrons)
            {
                squadrons.Add(deck.GetSquadron(squadron));
            }

            var planeMovementMan = PlaneMovementManager.Instance;
            planeMovementMan.MovementFinished -= OnMovementFinished;
            planeMovementMan.MovementFinished += OnMovementFinished;
        }
    }

    public override void SaveData(ref DeckOrderSaveData data, bool executing)
    {
        data.MissionType = Mission.OrderType;
        data.Mission = TacticManager.Instance.IndexOf(Mission);

        data.Squadrons.Clear();
        if (executing)
        {
            foreach (var squadron in squadrons)
            {
                data.Squadrons.Add(deck.IndexOf(squadron));
            }
        }
    }

    public override bool CanBeDone()
    {
        var eventMan = EventManager.Instance;
        if (deck.BlockOrders)
        {
            eventMan.HeavyManeuversPopup();
        }
        else if (deck.IsRunwayDamaged)
        {
            eventMan.RunwayDamagedPopup();
        }
        else if (deck.DeckMode == EDeckMode.Landing)
        {
            eventMan.DeckModeIncorrectPopup();
        }
        else
        {
            bool correctDayTime = TimeManager.Instance.IsDay;
            if (Mission != null)
            {
                switch (Mission.OrderType)
                {
                    case EMissionOrderType.NightAirstrike:
                    case EMissionOrderType.MagicNightScouts:
                    case EMissionOrderType.NightScouts:
                        correctDayTime = true;
                        break;
                }
            }
            if (!correctDayTime)
            {
                eventMan.DayTimeIncorrectPopup();
            }
            else if (ResourceManager.Instance.Supplies <= 0f)
            {
                eventMan.NoSuppliesPopup();
            }
            else
            {
                squadrons = deck.GetMissionSquadronList(Mission);
                if (squadrons == null)
                {
                    EventManager.Instance.SquadronIsNotOnDeckPopup();
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override void OnStart()
    {
        Mission.Launching();

        var plaMovMan = PlaneMovementManager.Instance;
        plaMovMan.MovementFinished -= OnMovementFinished;
        plaMovMan.MovementFinished += OnMovementFinished;

        plaMovMan.Launch(squadrons, deck.DeckSquadrons, deck.PlaneSpeed);
        BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.Starting);
    }

    public override void Execute()
    {
        Mission.SendMission();
        Mission.SentSquadrons.Clear();
        Mission.SentSquadrons.AddRange(squadrons);
        Mission.SentSquadronsReportManager.AddRange(squadrons);

        if (Mission.Canceled)
        {
            Mission.ReplaceSentSquadrons();
        }

        foreach (var squadron in squadrons)
        {
            deck.DeckSquadrons.Remove(squadron);
        }
        deck.FireDeckSquadronsCountChanged();
        deck.FirePlaneCountChanged();
        Mission.Deploy();
    }

    public override void ForceCancel()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
        foreach (var squadron in squadrons)
        {
            deck.DeckSquadrons.Remove(squadron);
        }
    }

    private void OnMovementFinished()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
        deck.SetOrderTimer(0);
    }

    public override IEnumerable<PlaneSquadron> GetSquadrons()
    {
        foreach (var squadron in squadrons)
        {
            yield return squadron;
        }
    }
}
