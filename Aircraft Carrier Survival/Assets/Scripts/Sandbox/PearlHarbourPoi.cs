
public class PearlHarbourPoi : SandboxPoi
{
    public override void OnClick()
    {
        base.OnClick();
        if (InRange)
        {
            if (SandboxManager.Instance.SandboxGoalsManager.MainGoal.DaysToFinish == 0)
            {
                var data = SaveManager.Instance.Data;
                ref var intermissionData = ref data.IntermissionData;
                CrewManager.Instance.SaveLosses(ref data.MissionRewards, ref intermissionData);
                StrikeGroupManager.Instance.SaveLosses(ref intermissionData);
                data.IntermissionData.Broken.Clear();
                data.IntermissionData.Broken.AddRange(data.IntermissionData.SelectedEscort);
                TacticManager.Instance.SaveLosses(ref intermissionData);
                GameStateManager.Instance.BackToPearlHarbour();
            }
            else
            {
                SandboxManager.Instance.ShowSandboxPearlHarbourPopup(this);
            }
        }
    }
}
