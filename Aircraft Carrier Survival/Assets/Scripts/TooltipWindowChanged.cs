using UnityEngine;

public class TooltipWindowChanged : MonoBehaviour
{
    [SerializeField]
    private EWindowType windowType = EWindowType.EscortDefense;
    [SerializeField]
    private TooltipCaller caller = null;

    private void Awake()
    {
        caller.TooltipStateChanged += OnTooltipStateChanged;
    }

    private void OnTooltipStateChanged(bool opened)
    {
        HudManager.Instance.FireWindowStateChanged(windowType, opened);
    }
}
