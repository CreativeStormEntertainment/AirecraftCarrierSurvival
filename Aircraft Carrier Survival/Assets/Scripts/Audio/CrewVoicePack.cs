using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CrewVoicePack
{
    public string Name;
    //public ECrewVoiceType Type;
    public List<AudioClip> Select;
    public List<AudioClip> Positive;
    public List<AudioClip> Negative;
    public List<AudioClip> Sleep;
}
