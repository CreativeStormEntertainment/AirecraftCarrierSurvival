using UnityEngine;
using UnityEngine.EventSystems;

public class MissionListExpand : Expandable
{
    private static readonly int Show = Animator.StringToHash("Show");

    [SerializeField]
    protected StateTooltip tooltip = null;

    private void OnEnable()
    {
        if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        {
            HideList();
        }
        else
        {
            ShowList();
        }
        tooltip.ChangeState(panelShowed ? ETooltipExpand.Collapse : ETooltipExpand.Expand);
    }

    public override void SetEnable(bool enable)
    {
        base.SetEnable(enable);
        TacticManager.Instance.MissionsEnabled = enable;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (TacticManager.Instance.Missions.Count > 0)
        {
            base.OnPointerClick(eventData);
        }
        else
        {
            BackgroundAudio.Instance.PlayEvent(EMainSceneUI.DisabledMissionPanel);
        }

        tooltip.ChangeState(panelShowed ? ETooltipExpand.Collapse : ETooltipExpand.Expand);
    }

    public void Reset()
    {
        ShowList();
    }

    public void ShowList()
    {
        animator.SetBool(Show, true);
        panelShowed = true;
    }

    public void HideList()
    {
        animator.SetBool(Show, false);
        panelShowed = false;
    }
}
