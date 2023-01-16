using System;

[Serializable]
public class CarrierUpgradeData
{
    public int Elevators;
    public int SquadronSlots;
    public int Sections;
    public int IslandRooms;

    public int Radar;
    public int AA;
    public int Hangar;
    public int Crew;
    public int Officer;

    public int Price;

    [NonSerialized]
    public bool Active;
}
