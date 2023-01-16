using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static void Swap<T>(ref T item1, ref T item2)
    {
        var temp = item1;
        item1 = item2;
        item2 = temp;
    }

    public static void Swap<T>(List<T> list, int index1, int index2)
    {
        var temp = list[index1];
        list[index1] = list[index2];
        list[index2] = temp;
    }

    public static List<EWaypointTaskType> ListWaypointsFlags()
    {
        var result = new List<EWaypointTaskType>();
        result.Add(EWaypointTaskType.Normal);
        result.Add(EWaypointTaskType.Firefighting);
        result.Add(EWaypointTaskType.Rescue);
        result.Add(EWaypointTaskType.Repair);
        result.Add(EWaypointTaskType.RepairDoor);
        result.Add(EWaypointTaskType.Waterpump);
        result.Add(EWaypointTaskType.Rescue2);
        result.Add(EWaypointTaskType.Rescue3);
        //Assert.IsTrue(value == 0 || start != EWaypointTaskType.Normal, value.ToString());
        return result;
    }

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    public static float InverseLerp(Vector2 a, Vector2 b, Vector2 value)
    {
        Vector2 AB = b - a;
        Vector2 AV = value - a;
        return Vector2.Dot(AV, AB) / Vector2.Dot(AB, AB);
    }
}
