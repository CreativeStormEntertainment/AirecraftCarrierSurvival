using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VO
{
    public string Name;
    public EVoiceOver ID;
    public AudioClip Clip;

    public VO(string Name, EVoiceOver ID, AudioClip Clip)
    {
        this.Name = Name;
        this.ID = ID;
        this.Clip = Clip;
    }
}
