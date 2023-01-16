using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class AnimAttackData
{
    [NonSerialized]
    public GameObject Camera;
    [NonSerialized]
    public GameObject Root;
    public PlayableDirector Director;
    public PlayableDirector DirectorSecondary;
    public StudioEventEmitter Emitter;
    public StudioEventEmitter EmitterExplosion;
    public float AADelay;
    public float ExplosionDelay;
}
