using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class StartReconAnimData
{
    public GameObject Root;
    public GameObject Camera;
    public List<PlayableDirector> Directors;
    public List<StudioEventEmitter> Emitters;
}
