using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using FMODUnity;

[Serializable]
public class TutorialVideoData
{
    public ETutorialType Type;
    public string TitleID;
    [NonSerialized]
    public string Title;
    public VideoClip Clip;
    public float Volume = 1f;
    public List<CaptionsData> Captions = new List<CaptionsData>();
    [NonSerialized]
    public MovieButton Button;
}
