using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class LightData
{
    public List<int> LightmapIndices = new List<int>();
    public List<Vector4> LightmapScaleOffsets = new List<Vector4>();
    public List<int> LightProbes = new List<int>();
    public List<SphericalHarmonicsL2> ProbesData = new List<SphericalHarmonicsL2>();

    [NonSerialized]
    public Dictionary<int, List<SphericalHarmonicsL2>> ProbesDict = new Dictionary<int, List<SphericalHarmonicsL2>>();
}
