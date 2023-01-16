using System;

[Serializable]
public class PersonData
{
    public ETraits Trait;
    public EHealthStatus Health;
    public string Portrait;

    public PersonData(ETraits trait, string portrait)
    {
        Trait = trait;
        Portrait = portrait;
        Health = EHealthStatus.Healthy;
    }
}
