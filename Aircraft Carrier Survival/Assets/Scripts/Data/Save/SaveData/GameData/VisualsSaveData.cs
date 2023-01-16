using System;
using System.Collections.Generic;

[Serializable]
public struct VisualsSaveData
{
    public bool TowShip;

    public bool Carrier;
    public bool LaunchBombers;

    public bool RescueRange;
    public bool CannonRange;
    public bool MissionRange;
    public bool MagicSprite;
    public bool MagicSprite2;
    public bool MagicSprite3;
    public bool PathRange;

    public List<int> MissionTargets;

    public List<int> Showables;

    public StrategyVisualsSaveData StrategyVisuals;

    public VisualsSaveData Duplicate()
    {
        var result = this;

        if (MissionTargets == null)
        {
            result.MissionTargets = new List<int>();
            result.Showables = new List<int>();
        }
        else
        {
            result.MissionTargets = new List<int>(MissionTargets);
            result.Showables = new List<int>(Showables);
        }

        result.StrategyVisuals = StrategyVisuals.Duplicate();

        return result;
    }
}
