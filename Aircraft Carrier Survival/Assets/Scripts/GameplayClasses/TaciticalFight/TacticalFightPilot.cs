using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TacticalFightPilot : MonoBehaviour
{
    public delegate void OnPilotHealthChange(int health);
    public static OnPilotHealthChange OnHealthChanged;

    [SerializeField]
    string pictureName = "";
    [SerializeField]
    int OnStartMoralePoints = 0;
    int currentMoralePoints;
    [SerializeField]
    List<TacticalFightPlayerUnit> PlayerAvailableUnitsPool = null;
    [SerializeField]
    List<TacticalFightAbility> pilotAbillities = null;
    List<int> playerIndexesOfBlockedUnits = new List<int>();

    public void InitializePilot()
    {
        currentMoralePoints = OnStartMoralePoints; 
        playerIndexesOfBlockedUnits = new List<int>();
    }

    public void DecreaseMoralePoints(int amount)
    {
        currentMoralePoints -= amount;

        if (currentMoralePoints <= 0)
        {
            currentMoralePoints = 0;
            TacticalFightHudManager.Instance.ShowEndGamePanel(false, "Your combat capacity has reached zero. You have been defeated.");
            TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.Lose);
        }

        OnHealthChanged?.Invoke(currentMoralePoints);
    }

    public void AddMoralePoints(int amount)
    {
        currentMoralePoints += amount;
        OnHealthChanged?.Invoke(currentMoralePoints);
    }
    
    public List<TacticalFightAbility> GetPilotAbillities()
    {
        return pilotAbillities;
    }

    public TacticalFightPlayerUnit GetPlayerUnitFromPool(int index)
    {
        return PlayerAvailableUnitsPool[index];
    }

    public List<TacticalFightPlayerUnit> GetPlayerUnits()
    {
        return PlayerAvailableUnitsPool;
    }

    public void BlockPlayerUnitInPool(int unitIndexToBlock)
    {
        playerIndexesOfBlockedUnits.Add(unitIndexToBlock);
        TacticalFightHudManager.Instance.SetBlockPlayerPlaneButton(unitIndexToBlock, true);
    }

    public void UnblockPlayerUnitInPool(int unitIndexToUnblock)
    {
        playerIndexesOfBlockedUnits.Remove(unitIndexToUnblock);
        TacticalFightHudManager.Instance.SetBlockPlayerPlaneButton(unitIndexToUnblock, false);
    }

    public bool CheckIsPlayerIndexBlocked(int unitIndexToCheck)
    {
        return playerIndexesOfBlockedUnits.Contains(unitIndexToCheck);
    }

    public string GetPictureName()
    {
        return pictureName;
    }

    public int GetCurrentMoralePoints()
    {
        return currentMoralePoints;
    }
}
