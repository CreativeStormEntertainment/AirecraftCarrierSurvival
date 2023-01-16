
public class EnemyManeuverInstanceData
{
    public EnemyManeuverData Data;
    public EnemyManeuverData Alternative;
    public bool Visible;
    public bool Dead;
    public int CurrentDurability;

    public bool WasDetected;

    public EnemyManeuverInstanceData(EnemyManeuverData data)
    {
        Data = data;
        CurrentDurability = Data.Durability;
    }
}
