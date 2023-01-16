using System;

[Serializable]
public class Strategy
{
    public string strategyName = "";
    public FightModifiersData Modifiers;
    public float radius = 0;
    public float fuel = 0;
    public int torpedoCount = 0;
    public int bombersCount = 0;
    public int fightersCount = 0;
    public string strategyDescription = "";
    public int damageBonusLvl = 0;
    public int loselBonusLvl = 0;
}
