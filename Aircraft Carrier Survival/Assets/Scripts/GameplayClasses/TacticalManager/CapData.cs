using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapData
{
    public TacticalMission Mission
    {
        get;
        private set;
    }
    public int Defence
    {
        get;
        private set;
    }
    public int Escort
    {
        get;
        private set;
    }
    public CapData(TacticalMission mission, int defence, int escort)
    {
        Mission = mission;
        Defence = defence;
        Escort = escort;
    }
}
