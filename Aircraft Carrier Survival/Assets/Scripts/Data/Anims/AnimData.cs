using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

[Serializable]
public class AnimData
{
    [NonSerialized]
    public Dictionary<string, int> AnimStrings;
    [NonSerialized]
    public Dictionary<int, AnimGroupData> AnimGroups;
    public List<string> AnimStringsKeys = new List<string>();
    public List<int> AnimStringsValues = new List<int>();
    public List<int> AnimGroupsKeys = new List<int>();
    public List<AnimGroupData> AnimGroupsValues = new List<AnimGroupData>();

    public int WalkID;
    public int WalkInjuredID;
    public int RunID;
    public int RotateID;
    public int InjuredID;
    public List<int> IdleIDs;
    public List<int> FrightenIDs;

    public int RescuerIdleID;
    public int RescuerWalkID;
    public int RescueeWalkID;
    public int RescuerRotateID;
    public int RescueeRotateID;
    public List<int> RescuerIDs;
    public List<int> RescueeIDs;

    public AnimData()
    {
        AnimStrings = new Dictionary<string, int>();
        AnimGroups = new Dictionary<int, AnimGroupData>();

        FrightenIDs = new List<int>();
        IdleIDs = new List<int>();

        RescuerIDs = new List<int>();
        RescueeIDs = new List<int>();
    }

    public void PrepareToSerialize()
    {
        AnimStringsKeys = AnimStrings.Keys.ToList();
        AnimStringsValues = AnimStrings.Values.ToList();

        AnimGroupsKeys = AnimGroups.Keys.ToList();
        AnimGroupsValues = AnimGroups.Values.ToList();
    }

    public void RegenerateData()
    {
        if (AnimStrings == null)
        {
            AnimStrings = new Dictionary<string, int>();
        }
        if (AnimGroups == null)
        {
            AnimGroups = new Dictionary<int, AnimGroupData>();
        }

        for (int i = 0; i < AnimStringsKeys.Count; i++)
        {
            Assert.IsFalse(AnimStrings.ContainsKey(AnimStringsKeys[i]));
            AnimStrings[AnimStringsKeys[i]] = AnimStringsValues[i];
        }
        for (int i = 0; i < AnimGroupsKeys.Count; i++)
        {
            Assert.IsFalse(AnimGroups.ContainsKey(AnimGroupsKeys[i]));
            AnimGroups[AnimGroupsKeys[i]] = AnimGroupsValues[i];
        }
    }
}
