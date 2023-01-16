using FMODUnity;
using System;

[Serializable]
public class CaptionsData
{
    public string TextID;
    [NonSerialized]
    public string Text = null;
    public float Timer;

    [EventRef]
    public string FmodEvent;

#if UNITY_EDITOR
    public CaptionsData(string textID, float timer)
    {
        TextID = textID;
        Timer = timer;
    }
#endif
}
