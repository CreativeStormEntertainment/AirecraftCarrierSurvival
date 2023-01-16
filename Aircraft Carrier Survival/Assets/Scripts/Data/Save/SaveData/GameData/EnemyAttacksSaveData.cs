using System;
using System.Collections.Generic;

[Serializable]
public struct EnemyAttacksSaveData
{
    public bool Detected;
    public bool CloudsUndetected;
    public int DetectionTicks;

    public bool DisableAttacksOnAlly;

    public bool AlreadyDetected;
    //public bool WasInClouds;

    //chosen enemy attacks day pattern
    public List<int> ChosenPatterns;

    public List<SandboxAttacksSaveData> SandboxAttacks;

    //attacks, recons, submarines and others in progress
    //atacks - aa + set time
    public List<AttackSaveData> Attacks;
    public List<ReconSaveData> ReconsAnims;
    public List<SubmarineSaveData> SubmarineAttacks;

    public EnemyAttacksSaveData Duplicate()
    {
        var result = this;
        result.ChosenPatterns = new List<int>(ChosenPatterns);
        result.ReconsAnims = new List<ReconSaveData>(ReconsAnims);
        result.SubmarineAttacks = new List<SubmarineSaveData>(SubmarineAttacks);
        if (SandboxAttacks != null)
        {
            result.SandboxAttacks = new List<SandboxAttacksSaveData>(SandboxAttacks);
        }

        result.Attacks = new List<AttackSaveData>();
        for (int i = 0; i < Attacks.Count; i++)
        {
            result.Attacks.Add(Attacks[i].Duplicate());
        }
        return result;
    }
}
