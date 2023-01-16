using System;

[Serializable]
public class FightModifiersData
{
    public int CasualtiesMin;
    public int CasualtiesMax;
    public int DamageMin;
    public int DamageMax;

    public FightModifiersData() : this(0)
    {

    }

    public FightModifiersData(int startCasualties)
    {
        CasualtiesMin = CasualtiesMax = startCasualties;
    }

    public override string ToString()
    {
        return "X:" + CasualtiesMin + ", Y:" + CasualtiesMax + ", Z:" + DamageMin + ", C:" + DamageMax;
    }

    public FightModifiersData Add(FightModifiersData data)
    {
        CasualtiesMin += data.CasualtiesMin;
        CasualtiesMax += data.CasualtiesMax;
        DamageMin += data.DamageMin;
        DamageMax += data.DamageMax;
        return this;
    }

    public FightModifiersData Multiply(int multiplier)
    {
        CasualtiesMin *= multiplier;
        CasualtiesMax *= multiplier;
        DamageMin *= multiplier;
        DamageMax *= multiplier;
        return this;
    }
}
