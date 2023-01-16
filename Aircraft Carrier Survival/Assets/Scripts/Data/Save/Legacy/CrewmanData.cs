using System;

[Serializable]
public class CrewmanData : PersonData
{
    public string NameDegree;
    public int UnlockCost;
    public string Description;

    public CrewmanData(ETraits trait, string portrait, string nameDegree, int unlockCost, string description) : base(trait, portrait)
    {
        NameDegree = nameDegree;
        UnlockCost = unlockCost;
        Description = description;
    }
}