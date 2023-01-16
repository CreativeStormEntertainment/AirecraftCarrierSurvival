using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CrewVoiceContainer
{
    public string Name;
    public List<CrewVoicePack> Data;
    public List<BuffVoicePack> BuffPacks;
    public List<MegaphoneVoice> Megaphone;
}

