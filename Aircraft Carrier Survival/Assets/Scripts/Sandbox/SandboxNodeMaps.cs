using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "SandboxNodeMaps", menuName = "Sandbox/SandboxNodeMaps")]
public class SandboxNodeMaps : ScriptableObject
{
    public List<SandboxNodeMapsData> NodeDatas;
}

[Serializable]
public class SandboxNodeMapsData
{
    public List<MapSpawnData> Maps;

    public SandboxNodeMapsData()
    {
        Maps = new List<MapSpawnData>();
    }
}

