using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    [SerializeField] private List<TooltipData> tooltips = null;

    public static TooltipManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public List<TooltipData> GetTooltipsList()
    {
        return tooltips;
    }

    public void RestoreTooltipEnums()
    {
        for (int i = 0; i < tooltips.Count; i++)
        {
            tooltips[i].Tooltip = (EIntermissionTooltip)i;
        }
    }
}
