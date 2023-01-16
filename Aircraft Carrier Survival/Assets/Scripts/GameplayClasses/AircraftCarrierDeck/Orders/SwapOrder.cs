using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SwapOrder : SwapToFrontOrder
{
    private int indexB;
    private int newIndexA = -1;
    private int newIndexB = -1;

    public SwapOrder(AircraftCarrierDeckManager deck, PlaneSquadron squadronA, int indexB) : base(deck, squadronA)
    {
        this.indexB = indexB;
        lastCan = true;
    }

    public SwapOrder(AircraftCarrierDeckManager deck, DeckOrderSaveData data, bool executing) : base(deck, data, executing)
    {
        if (executing)
        {
            indexB = data.Params[3];
            newIndexA = data.Params[4];
            newIndexB = data.Params[5];
        }
        else
        {
            indexB = data.Params[2];
        }
    }

    public override void SaveData(ref DeckOrderSaveData data, bool executing)
    {
        base.SaveData(ref data, executing);
        data.Params.Add(indexB);
        if (executing)
        {
            data.Params.Add(newIndexA);
            data.Params.Add(newIndexB);
        }
    }

    public override bool CanBeDone()
    {
        var eventMan = EventManager.Instance;
        if (base.CanBeDone())
        {
            int indexB = this.indexB;
            if (deckMode != deck.DeckMode)
            {
                indexB = deck.DeckSquadrons.Count - indexB - 1;
            }
            if (this.indexB >= 0 && this.indexB < deck.DeckSquadrons.Count && deck.DeckSquadrons[indexB] != SquadronA)
            {
                return true;
            }
            else
            {
                eventMan.OrderCannotBeDonePopup();
            }
        }
        return false;
    }

    public override void OnStart()
    {
        BackgroundAudio.Instance.PlayEvent(EMegaphoneVoice.Swap);

        if (deckMode != deck.DeckMode)
        {
            indexB = deck.DeckSquadrons.Count - indexB - 1;
        }

        var plaMovMan = PlaneMovementManager.Instance;
        plaMovMan.MovementFinished -= OnMovementFinished;
        plaMovMan.MovementFinished += OnMovementFinished;
        var from = (deck.DeckMode == EDeckMode.Starting ? EPlaneNodeGroup.DeckLaunching : EPlaneNodeGroup.DeckRecovering);
        plaMovMan.Swap(SquadronA, deck.DeckSquadrons[indexB], deck.DeckSquadrons, from, deck.PlaneSpeed, out indexA, out newIndexA, out newIndexB);
    }

    public override void Execute()
    {
        if (Mathf.Abs(indexA - indexB) != 1)
        {
            deck.DeckSquadrons[newIndexB].Planes.Reverse();
        }
        if ((newIndexA + 1) == deck.DeckSquadrons.Count)
        {
            deck.DeckSquadrons[newIndexA].Planes.Reverse();
        }

        (deck.DeckSquadrons[indexA], deck.DeckSquadrons[indexB]) = (deck.DeckSquadrons[indexB], deck.DeckSquadrons[indexA]);
        float z = float.PositiveInfinity;
        foreach (var squadron in deck.DeckSquadrons)
        {
            var pos = squadron.Planes[0].CurrentNode.Position;
            Assert.IsTrue(pos.x > 0f);
            Assert.IsTrue(Mathf.Abs(z) > Mathf.Abs(pos.z));
            Assert.IsTrue(Mathf.Abs(z) > Mathf.Abs(squadron.Planes[1].CurrentNode.Position.z));
            if (squadron.Planes.Count > 2)
            {
                Assert.IsTrue(Mathf.Abs(z) > Mathf.Abs(squadron.Planes[2].CurrentNode.Position.z));
                Assert.IsTrue(squadron.Planes[2].CurrentNode.Position.x < 0f);
            }
            z = pos.z;
        }
    }

    public override void ForceCancel()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
    }

    private void OnMovementFinished()
    {
        PlaneMovementManager.Instance.MovementFinished -= OnMovementFinished;
        deck.SetOrderTimer(0);
    }

    private IEnumerable<PlaneMovement> GetAllPlanes()
    {
        foreach (var plane in deck.DeckSquadrons[indexB].Planes)
        {
            yield return plane;
        }
        foreach (var plane in SquadronA.Planes)
        {
            yield return plane;
        }
    }
}
