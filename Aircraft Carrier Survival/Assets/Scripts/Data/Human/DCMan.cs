
public class DCMan : Person
{
    protected override EHealthStatus HealthStatus
    {
        get => MaxCrewmen > 0 ? EHealthStatus.Healthy : EHealthStatus.Dead;
        set => base.HealthStatus = value;
    }

    public DCMan(PersonData data) : base(data)
    {
        Effectiveness = 10f;
    }
}
