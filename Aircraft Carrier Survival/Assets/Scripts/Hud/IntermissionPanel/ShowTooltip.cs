using UnityEngine;
using UnityEngine.UI;
using System;

using GambitUtils;

public class ShowTooltip : MonoBehaviour
{

    public int IndexTooltip
    {
        get => (int)tooltip;
        set => tooltip = (EIntermissionTooltip)value;
    }

    public Text HeaderTooltip
    {
        get => headerTooltip;
        set => headerTooltip = value;
    }

    public Text DescriptionTooltip
    {
        get => descriptionTooltip;
        set => descriptionTooltip = value;
    }

    [SerializeField]
    private EIntermissionTooltip tooltip = default;
    [SerializeField]
    private Text headerTooltip = null;
    [SerializeField]
    private Text descriptionTooltip = null;
    [SerializeField]
    private RectTransform panel = null;

    public void FillTooltip()
    {
        var tooltipList = TooltipManager.Instance.GetTooltipsList();
        int tooltipIndex = (int)tooltip;
        var locMan = LocalizationManager.Instance;
        var header = tooltipList[tooltipIndex].GetHeaderTooltip();
        var desc = tooltipList[tooltipIndex].GetDescriptionTooltip();
        if (!string.IsNullOrWhiteSpace(header))
        {
            headerTooltip.gameObject.SetActive(true);
            headerTooltip.text = locMan.GetText(header);
        }
        else
        {
            headerTooltip.gameObject.SetActive(false);
            if (string.IsNullOrWhiteSpace(desc))
            {
                Debug.LogError(tooltip + "header and description are empty!");
            }
        }
        if (!string.IsNullOrWhiteSpace(desc))
        {
            descriptionTooltip.gameObject.SetActive(true);
            descriptionTooltip.text = locMan.GetText(desc);
        }
        else
        {
            descriptionTooltip.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
        IntermissionPanel.Instance.StartCoroutineActionAfterFrames(() => RebuildTooltip(), 1);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    public void Show(EIntermissionTooltip tooltip)
    {
        this.tooltip = tooltip;
        FillTooltip();
    }

    public void RebuildTooltip()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.parent.GetComponent<RectTransform>());
    }
}
