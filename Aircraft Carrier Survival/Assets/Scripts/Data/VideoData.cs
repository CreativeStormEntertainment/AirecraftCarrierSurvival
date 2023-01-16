using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine.Video;

[Serializable]
public class VideoData
{
    public VideoClip Clip;
    [EventRef]
    public string AudioEvent;
    [EventRef]
    public string AudioSDEvent;
    public List<CaptionsData> Captions = new List<CaptionsData>();
}
