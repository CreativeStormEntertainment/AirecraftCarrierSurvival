using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PearlHarbourPopup : SandboxPopup, IPopupPanel
{
    public EWindowType Type => EWindowType.SandboxPopup;

    public override void Show(SandboxPoi poi)
    {
        base.Show(poi);
        buttonA.interactable = CheckInRange();
        var hudMan = HudManager.Instance;
        hudMan.PopupShown(this);
    }

    public override void Hide()
    {
        var hudMan = HudManager.Instance;
        hudMan.PopupHidden(this);
        base.Hide();
    }

    protected override void OnClickA()
    {
        base.OnClickA();
        var data = SaveManager.Instance.Data;
        ref var intermissionData = ref data.IntermissionData;
        CrewManager.Instance.SaveLosses(ref data.MissionRewards, ref intermissionData);
        StrikeGroupManager.Instance.SaveLosses(ref intermissionData);
        TacticManager.Instance.SaveLosses(ref intermissionData);
        data.IntermissionData.Broken.Clear();
        data.IntermissionData.Broken.AddRange(data.IntermissionData.SelectedEscort);
        GameStateManager.Instance.BackToPearlHarbour();
    }

    protected override void OnClickB()
    {
        base.OnClickB();
    }
}
