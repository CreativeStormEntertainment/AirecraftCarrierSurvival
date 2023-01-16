using System;
using System.Collections.Generic;
using UnityEngine;

public class MarkerData
{
    public InteractableMarker MarkerObj;
    public RectTransform Transform;
    public RectTransform Container;
    public RectTransform MissionContainer;
    public RectTransform ObjTransform;
    public float UpdateTimer;
    public int SecondTooltipTimer;
    public int HideTimer;
    public TacticalObject Object;
    public MarkerHighlight Highlight;
    public List<RectTransform> Objectives = new List<RectTransform>();

    [NonSerialized]
    public ObjectiveUISpriteData MagicSprite;
}
