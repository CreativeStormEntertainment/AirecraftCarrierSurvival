using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxQuestPopup : SandboxPopup
{
    public override void Show(SandboxPoi poi)
    {
        base.Show(poi);
        buttonA.interactable = CheckInRange();
    }

    protected override void OnClickA()
    {
        base.OnClickA();
        var poiMan = SandboxManager.Instance.PoiManager;
        poiMan.SpawnQuestMapPoi();
        poiMan.RemovePoi(poi);
    }

    protected override void OnClickB()
    {
        base.OnClickB();
    }
}
