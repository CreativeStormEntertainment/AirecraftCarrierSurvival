using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class AttackAnimCameraData
{
    public GameObject CameraGo;
    public PlayableDirector Director;
    public PlayableDirector DirectorSecondary;
    public float AADelay;
    public float ExplosionDelay;
    public GameObject DirectorGo;
    public StudioEventEmitter Emitter;
    public StudioEventEmitter EmitterExplosion;
    public int DataIndex;
    public int AnimIndex;
    public TacticalMission TacticalMission;
    [NonSerialized]
    public bool EmitterChanged;

    public GameObject Root;

    public AttackAnimCameraData(AnimAttackData data, int animIndex)
    {
        Root = data.Root;
        Root.SetActive(true);

        CameraGo = data.Camera;
        Director = data.Director;
        DirectorSecondary = data.DirectorSecondary;
        DirectorGo = data.Director.gameObject;
        Emitter = data.Emitter;
        EmitterExplosion = data.EmitterExplosion;
        if (Emitter == null)
        {
            Debug.LogError("Null emitter " + DirectorGo.name);
        }

        AADelay = data.AADelay;
        ExplosionDelay = data.ExplosionDelay;

        AnimIndex = animIndex;
    }

    public AttackAnimCameraData(List<StartReconAnimData> datas, int dataIndex, int animIndex)
    {
        var data = datas[dataIndex];
        data.Root.SetActive(true);

        CameraGo = data.Camera;
        Director = data.Directors[animIndex];
        DirectorGo = Director.gameObject;
        Emitter = data.Emitters[animIndex];

        DataIndex = dataIndex;
        AnimIndex = animIndex;

        if (Emitter == null)
        {
            Debug.LogError("Null emitter " + animIndex);
        }
    }
}
