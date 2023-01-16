using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanesPanel : SandboxPopup
{
    [SerializeField]
    private Button buttonC = null;

    private Action onOptionChoose;

    protected override void Start()
    {
        base.Start();
        buttonC.onClick.AddListener(OnClickC);
    }

    public void Show(Action onClick, SandboxPoi poi)
    {
        base.Show(poi);
        onOptionChoose = onClick;
    }

    protected override void OnClickA()
    {
        base.OnClickA();
        ReplenishSquadrons(EPlaneType.Fighter);
    }

    protected override void OnClickB()
    {
        base.OnClickB();
        ReplenishSquadrons(EPlaneType.Bomber);
    }

    private void OnClickC()
    {
        ReplenishSquadrons(EPlaneType.TorpedoBomber);
    }

    private void ReplenishSquadrons(EPlaneType planeType)
    {
        AircraftCarrierDeckManager.Instance.ReplenishSquadrons(2, planeType);
        onOptionChoose();
    }
}
