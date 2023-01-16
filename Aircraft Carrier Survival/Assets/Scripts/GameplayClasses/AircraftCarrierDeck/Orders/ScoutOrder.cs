using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class ScoutOrder : ACOrder
//{
//    SquadronSlot squadronSlot;

//    public ScoutOrder(AircraftCarrierDeckManager deck, PlaneSquadron squadron, SquadronSlot slot)
//    {
//        this.deck = deck;
//        this.orderType = EOrderType.Scout;
//        this.squadron = squadron;
//        this.timer = Mathf.CeilToInt(deck.StartingLandingTime / PlaneManager.Instance.PlaneStartSpeed);
//        this.squadronSlot = slot;
//    }

//    public override void OnStart()
//    {
//        base.OnStart();
//        squadronSlot.Clear();
//        deck.AnimToTakeoff(squadron.VisualPlane);
//        squadron.PlayStartingAnimation();
//    }

//    public override void Execute()
//    {
//        deck.SendSquadronToScout(squadron);
//        squadron.HideAnimatingPlane();
//    }

//    public override void OnCancel()
//    {
//        //squadron.UnlockFrame();
//    }
//}
