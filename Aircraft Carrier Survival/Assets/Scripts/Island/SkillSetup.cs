using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSetup
{
    public static int GetCurrentExperienceThreshold(byte lvl)
    {
        return (int)(100 * Mathf.Pow(1.3f, lvl - 1));
    }
}
