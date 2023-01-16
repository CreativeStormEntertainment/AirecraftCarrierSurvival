using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class InstanceSounds : MonoBehaviour
{
    public RescueSounds RescueSounds => rescueSounds;

    [SerializeField]
    private StudioEventEmitter firefightSound = null;
    [SerializeField]
    private StudioEventEmitter pompSound = null;
    [SerializeField]
    private StudioEventEmitter repairSound = null;
    [SerializeField]
    private RescueSounds rescueSounds = null;

    private Dictionary<EWaypointTaskType, StudioEventEmitter> dict;
    private StudioEventEmitter current;

    private void Awake()
    {
        dict = new Dictionary<EWaypointTaskType, StudioEventEmitter>();
        if (firefightSound != null)
        {
            dict[EWaypointTaskType.Firefighting] = firefightSound;
        }
        if (pompSound != null)
        {
            dict[EWaypointTaskType.Waterpump] = pompSound;
        }
        if (repairSound != null)
        {
            dict[EWaypointTaskType.Repair] = repairSound;
        }
    }

    public void Play(EWaypointTaskType task)
    {
        Stop();
        if (dict.TryGetValue(task, out current))
        {
            current.Play();
        }
    }

    public void Stop()
    {
        if (current != null)
        {
            current.Stop();
            current = null;
        }
    }
}
