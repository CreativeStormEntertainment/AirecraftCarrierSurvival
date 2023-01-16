using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCSectionOutline : MonoBehaviour
{
    [SerializeField]
    private ESectionCategory category = ESectionCategory.DC;
    [SerializeField]
    private Color dcInColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField]
    private Color noDcInColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField]
    private MeshRenderer outline = null;
    [SerializeField]
    private List<SectionSegment> sectionSegments = null;

    private int dcCount;

    private void Start()
    {
        foreach (SectionSegment segment in sectionSegments)
        {
            segment.DCChanged += ChangeDcCount;
        }
        SetOutlineColor(false);
    }

    private void ChangeDcCount(bool value, ESectionCategory cat)
    {
        if (category == cat)
        {
            if (value)
            {
                dcCount++;
            }
            else
            {
                dcCount--;
            }
        }
        SetOutlineColor(dcCount > 0);
    }

    private void SetOutlineColor(bool value)
    {
        outline.material.color = value ? dcInColor : noDcInColor;
    }
}
