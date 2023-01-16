using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IntermissionTutorialStep : MonoBehaviour
{
    [NonSerialized]
    public UnityAction Action;

    public List<Button> TriggerButtons;

    public float Delay;

    public List<GameObject> Highlights;
    public List<GameObject> PersistentHighlights;
    public List<GameObject> HidePersistentHighlights;

    public Vector2 CustomPosition;
    public string TitleID;
    public string DescriptionID;
    public string ExtraDescriptionID;

    [EventRef]
    public string FmodEvent;

    public List<IntermissionTutorialStep> Next;
}
