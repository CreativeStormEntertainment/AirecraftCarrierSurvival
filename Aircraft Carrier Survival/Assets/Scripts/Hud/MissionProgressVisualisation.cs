using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionProgressVisualisation : MonoBehaviour
{
    public RectTransform RectTransform => rectTransform;
    public RectTransform RangeRect => rangeRect;

    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private RectTransform rangeRect = null;
}
