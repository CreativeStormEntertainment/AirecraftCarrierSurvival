using System.Collections.Generic;
using UnityEngine;

public class MapUnitMaskManager : MonoBehaviour
{
    public static MapUnitMaskManager Instance = null;

    private List<MapUnit> units = new List<MapUnit>();

    public List<MapUnit> Units => units;

    private void Awake()
    {
        Instance = this;
    }

    public void AddUnitObject(GameObject obj, int radius)
    {
        MapUnit unit = new MapUnit {
            obj = obj,
            radius = radius
        };
        units.Add(unit);
    }

    public void RemoveUnitObject(GameObject obj)
    {
        MapUnit fow = units.Find(unit => unit.obj == obj);
        units.Remove(fow);
    }

    public void UpdateUnitObject(GameObject obj, int radius)
    {
        units.Find(unit => unit.obj == obj).radius = radius;
    }
}


[System.Serializable]
public class MapUnit
{
    public int radius = 0;
    public GameObject obj = null;
}
