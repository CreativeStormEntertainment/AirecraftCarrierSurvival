using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SandboxAdmiralLevels", menuName = "Sandbox/SandboxAdmiralLevels")]
public class SandboxAdmiralLevels : ScriptableObject
{
    public List<SandboxAdmiralLevel> Levels;

    public int GetAdmiralLevel(int exp)
    {
        int admiralRenownLevel = 1;
        for (int i = 0; i < Levels.Count; i++)
        {
            if (exp >= Levels[i].RequiredExp)
            {
                admiralRenownLevel = Levels[i].Level;
            }
            else
            {
                break;
            }
        }
        return admiralRenownLevel;
    }
}
