using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResupplySounds : SpeedSounds<EResupplySound>
{
    public void SetTimelinePosition(int seconds)
    {
        fmodEvent.SetTimelinePosition(seconds * 1000);
    }
}
