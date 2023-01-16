using UnityEngine;
using System.Collections;

[System.Serializable]
public class TacticalFightEnemyAbility
{
    public string AbillityName;
    public string AbillityDescription;
    public ETacticalFightEnemyAbillityEffectType EffectType;
    public int AmountOfDamage;
    public int AmountOfRoundsToWait;
    int currentAmountOfRoundsToWait;

    public void SetCurrentAmountOfRoundsToWait(int amount)
    {
        currentAmountOfRoundsToWait = amount;
    }

    public int GetCurrentAmountOfRoundsToWait()
    {
        return currentAmountOfRoundsToWait;
    }

    public void ResetCurrentAmountOfRoundsToWait()
    {
        currentAmountOfRoundsToWait = AmountOfRoundsToWait;
    }
}
