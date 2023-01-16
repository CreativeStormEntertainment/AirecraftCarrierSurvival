using System.Collections.Generic;

public class CasualtiesData
{
    public Dictionary<EPlaneType, int> SquadronsDestroyed;
    public Dictionary<EPlaneType, int> SquadronsBroken;
    public HashSet<int> EnemyDestroyedIndices;
    public List<EnemyManeuverData> EnemyDestroyed;

    public CasualtiesData()
    {
        SquadronsDestroyed = new Dictionary<EPlaneType, int>();
        SquadronsDestroyed[EPlaneType.Bomber] = 0;
        SquadronsDestroyed[EPlaneType.Fighter] = 0;
        SquadronsDestroyed[EPlaneType.TorpedoBomber] = 0;

        SquadronsBroken = new Dictionary<EPlaneType, int>();
        SquadronsBroken[EPlaneType.Bomber] = 0;
        SquadronsBroken[EPlaneType.Fighter] = 0;
        SquadronsBroken[EPlaneType.TorpedoBomber] = 0;

        EnemyDestroyedIndices = new HashSet<int>();
        EnemyDestroyed = new List<EnemyManeuverData>();
    }

    public bool AllSquadronsDestroyed(FightSquadronData squadron)
    {
        int lostBombers = SquadronsDestroyed[EPlaneType.Bomber];
        int lostFighters = SquadronsDestroyed[EPlaneType.Fighter];
        int lostTorpedoes = SquadronsDestroyed[EPlaneType.TorpedoBomber];
        return squadron.Bombers == lostBombers && squadron.Fighters == lostFighters && squadron.Torpedoes == lostTorpedoes;
    }

    public int GetDestroyedSquadronsCount()
    {
        int count = 0;
        foreach (var kvp in SquadronsDestroyed)
        {
            count += kvp.Value;
        }
        return count;
    }

    public int GetBrokenSquadronsCount()
    {
        int count = 0;
        foreach (var kvp in SquadronsBroken)
        {
            count += kvp.Value;
        }
        return count;
    }

    public int GetDamagedSquadronsCount()
    {
        return GetDestroyedSquadronsCount() + GetBrokenSquadronsCount();
    }

    public bool CompareSquadronsDestroyed(CasualtiesData other)
    {
        foreach (var kvp in SquadronsDestroyed)
        {
            if (other.SquadronsDestroyed[kvp.Key] != kvp.Value)
            {
                return false;
            }
        }
        return true;
    }
}
