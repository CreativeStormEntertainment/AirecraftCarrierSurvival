using System.Collections.Generic;
using UnityEngine;

public class MovieSounds : MonoBehaviour
{
    private List<FMODEvent> events;
    private List<FMODEvent> soundEvents;

    private void Awake()
    {
        events = new List<FMODEvent>();
        soundEvents = new List<FMODEvent>();
    }

    private void OnDestroy()
    {
        ReleaseEvents();
    }

    public void Setup(List<VideoData> datas)
    {
        ReleaseEvents();
        for (int i = 0; i < datas.Count; i++)
        {
            if (FMODEvent.IsValid(datas[i].AudioEvent))
            {
                events.Add(new FMODEvent(datas[i].AudioEvent));
            }
            else
            {
                Debug.LogError("Invalid event, datas[i].AudioEvent");
                events.Add(null);
            }
            bool empty = string.IsNullOrWhiteSpace(datas[i].AudioSDEvent);
            if (!empty && FMODEvent.IsValid(datas[i].AudioSDEvent))
            {
                soundEvents.Add(new FMODEvent(datas[i].AudioSDEvent));
            }
            else
            {
                if (!empty)
                {
                    Debug.LogError("Invalid event, datas[i].AudioSDEvent");
                }
                soundEvents.Add(null);
            }
        }
    }

    public void Play(int id)
    {
        Stop();
        events[id]?.Play();
        soundEvents[id]?.Play();
    }

    public void Stop()
    {
        foreach (var fmodEvent in events)
        {
            fmodEvent?.Stop(true);
        }
        foreach (var fmodEvent in soundEvents)
        {
            fmodEvent?.Stop(true);
        }
    }

    private void ReleaseEvents()
    {
        foreach (var fmodEvent in events)
        {
            fmodEvent?.Release();
        }
        events.Clear();
        foreach (var fmodEvent in soundEvents)
        {
            fmodEvent?.Release();
        }
        soundEvents.Clear();
    }
}
