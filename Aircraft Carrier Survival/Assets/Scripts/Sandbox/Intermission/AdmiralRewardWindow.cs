using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmiralRewardWindow : MonoBehaviour
{
    public void Setup(SandboxAdmiralLevel levelReward)
    {
        if (levelReward.Maneuver)
        {
            SetupMeneuverChoice();
        }
        else if(levelReward.Order)
        {
            SetupBuffChoice();
        }
        else
        {
            SetupUpgradePoints();
        }
    }

    public void SetupMeneuverChoice()
    {

    }

    public void SetupBuffChoice()
    {

    }


    public void SetupUpgradePoints()
    {

    }
}
