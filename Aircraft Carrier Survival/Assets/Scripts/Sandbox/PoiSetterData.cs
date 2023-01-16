using System;
using System.Collections.Generic;

[Serializable]
public class PoiSetterData 
{
    public EMissionLength Length;
    public float ChanceToSpawnRepairPoint;
    public List<int> DifficultyMinimumPois; //0 - easy, 1 - medium, 2 - hard
}
