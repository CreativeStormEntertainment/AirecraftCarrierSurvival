using System;

[Serializable]
public class FightSquadronData
{
    public int SquadronCount => Bombers + Fighters + Torpedoes;
    public int Bombers;
    public int Fighters;
    public int Torpedoes;
    public bool PrioritizeBombers;
    public bool PrioritizeTorpedoes;
}
