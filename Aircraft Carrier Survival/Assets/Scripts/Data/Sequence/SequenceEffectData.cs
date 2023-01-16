using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SequenceEffectData
{
    public ESequenceEffectType EffectType;
    public EDirection Direction;
    public int Value;
    public SubSectionRoom TargetSubsection;
    public List<SequenceEffectData> PlaneReachedAircraftEffects;
    public List<GameObject> GameObjectsToActivate;
}
