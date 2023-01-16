using System;
using UnityEngine;

[Serializable]
public struct EventQueueData
{
    public EEventType Type;
    public EAudio Audio;
    public Sprite Sprite;
    public string Text;
    public bool Critical;

    public EventQueueData(EEventType type, EAudio audio, Sprite sprite, string text, bool critical)
    {
        Type = type;
        Audio = audio;
        Sprite = sprite;
        Text = text;
        Critical = critical;
    }
}
