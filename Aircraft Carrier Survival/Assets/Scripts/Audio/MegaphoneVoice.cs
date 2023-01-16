using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MegaphoneVoice
{
    public string Name;
    public EMegaphoneVoice Type;
    public List<AudioClip> Clips;
}
