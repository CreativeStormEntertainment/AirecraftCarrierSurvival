
public class PlaneData
{
    public int Free;
    public int Damaged;

    public PlaneData(int startCount)
    {
        Free = startCount;
    }

    public void LoadData(PlaneSaveData data)
    {
        Free = data.Free;
        Damaged = data.Broken;
    }

    public PlaneSaveData SaveData()
    {
        var result = new PlaneSaveData();

        result.Free = Free;
        result.Broken = Damaged;

        return result;
    }
}
