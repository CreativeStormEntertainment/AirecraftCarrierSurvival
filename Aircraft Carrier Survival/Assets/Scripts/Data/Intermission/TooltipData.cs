using System;
using UnityEngine;

[Serializable]
public class TooltipData
{
    [SerializeField] private EIntermissionTooltip tooltip = 0;
    [SerializeField] private string headerTooltip = "";
    [SerializeField] private string descriptionTooltip = "";

    public EIntermissionTooltip Tooltip { get => tooltip; set => tooltip = value; }

    public string GetHeaderTooltip()
    {
        return headerTooltip;
    }

    public string GetDescriptionTooltip()
    {
        return descriptionTooltip;
    }

    public int GetTooltipIndex()
    {
        return (int)tooltip;
    }
}
