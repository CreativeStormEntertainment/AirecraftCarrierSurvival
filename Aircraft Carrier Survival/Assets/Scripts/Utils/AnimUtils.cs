using System.Collections.Generic;
using UnityEngine.Assertions;

public static class AnimUtils
{
#if UNITY_EDITOR
    public static readonly List<string> Prefixes = new List<string>() { "Action", "Medic", "Repair", "Firefighting", "Injured", "Welding", "Waterpump" };

    public static int GetActionIndex(string action)
    {
        bool goodLength = false;
        foreach (var prefix in Prefixes)
        {
            if (action.StartsWith(prefix) && action.Length == (prefix.Length + 2))
            {
                goodLength = true;
                break;
            }
        }
        Assert.IsTrue(goodLength, action);
        return int.Parse(action.Substring(action.Length - 2));
    }

    public static string GetName(string prefix, int id)
    {
        return prefix + id.ToString("D2");
    }

    public static PrefixType GetPrefix(EWaypointTaskType type)
    {
        switch (type)
        {
            case EWaypointTaskType.Firefighting:
                return PrefixType.Firefighting;
            case EWaypointTaskType.Rescue:
                return PrefixType.Rescue;
            case EWaypointTaskType.Rescue2:
                return PrefixType.Rescue;
            case EWaypointTaskType.Rescue3:
                return PrefixType.Rescue;
            case EWaypointTaskType.Repair:
                return PrefixType.Repair;
            case EWaypointTaskType.RepairDoor:
                return PrefixType.RepairDoors;
            case EWaypointTaskType.Waterpump:
                return PrefixType.Waterpump;
            default:
                return PrefixType.Action;
        }
    }
#endif
}
