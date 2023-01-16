using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HighlightTask
{
    public TutorialHighlight highlight;
    public RectTransform target;
    public RectTransform toHighlight;

    public Vector3 offset;
}
