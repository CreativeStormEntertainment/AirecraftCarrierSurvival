using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SequenceEventData
{
    public float TimeToFire;

    [Header("Popup stuff")]
    public bool ShowPopup;
    public string Title;
    [TextArea]
    public string Description;
    public List<SequenceEffectData> EffectsOnAccept;
    public List<SequenceEffectData> EffectOnReject;

    [Header("Stuff independent of popup")]
    public List<DelayedEffectData> DelayedEffects;
}
