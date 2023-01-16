
[System.Serializable]
public class SubsectionStateData
{
    public TwoWayProgresser Progresser;
    public Subcrewman DCWorker;
    public float State;

    public SubsectionStateData(float state, TwoWayProgresser progresser)
    {
        State = state;
        Progresser = progresser;
    }

    public float GetEffectiveness()
    {
        return EffectManager.Instance.CrewEffectiveness * DCWorker.Parent.Effectiveness;
    }
}
