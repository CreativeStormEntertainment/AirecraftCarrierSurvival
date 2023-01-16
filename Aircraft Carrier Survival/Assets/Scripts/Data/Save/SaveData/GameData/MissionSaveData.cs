using System;
using System.Collections.Generic;

[Serializable]
public struct MissionSaveData
{
    public float Time;
    public EMissionOrderType Type;
    public EMissionStage Stage;
    public bool Canceled;
    public bool ForceAdd;

    public MyVector2 AttackPosition;
    public MyVector2 ReturnPosition;
    public MyVector2 StartPosition;

    public List<int> Strategies;

    public int SelectedObject;
    public int ConfirmedTarget;

    public bool CustomMission;

    public List<SquadronsSaveData> SentSquadrons;
    public List<SquadronsSaveData> SentSquadronsLeft;
    public List<bool> SentSquadronsDirections;
    public List<bool> AllSentSquadronsDirections;
    public List<int> RecoveryDirections;
    public int Bombers;
    public int Fighters;
    public int Torpedoes;
    public int LostBombers;
    public int LostFighters;
    public int LostTorpedoes;

    public int FriendID;

    public bool HadAction;
    public bool Confirmed;

    public float MissionProgressTime;
    public float MissionProgressEnd;
    public bool IsReturning;

    public EMissionDeployedStage DeployedStage;

    public int BonusAttack;

    public EEnemyAttackTarget Target;

    public int TargetID;
    public int RetrievalRange;

    public bool UseTorpedoes;

    public List<int> PossibleMissionTargets;

    public bool EscortFreeMission;

    public MissionSaveData Duplicate()
    {
        var result = this;
        result.Strategies = new List<int>(Strategies);
        result.SentSquadrons = new List<SquadronsSaveData>(SentSquadrons);
        result.SentSquadronsLeft = new List<SquadronsSaveData>(SentSquadronsLeft);
        result.RecoveryDirections = new List<int>(RecoveryDirections);
        result.PossibleMissionTargets = new List<int>(PossibleMissionTargets);
        return result;
    }
}
