using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapChapter
{
    private List<MapIndicator> mainMissions = null;
    private List<MapIndicator> sideMissions = null;

    public int MainMissionCount { get => mainMissions.Count; }
    public MapIndicator First { get => mainMissions[0]; }
    public List<MapIndicator> MainMissions { get => mainMissions; }
    public List<MapIndicator> SideMissions { get => sideMissions; }

    public WorldMapChapter(Transform mainMissionParent, Transform sideMissionParent)
    {
        mainMissions = new List<MapIndicator>();
        sideMissions = new List<MapIndicator>();
        foreach (Transform t in mainMissionParent)
        {
            mainMissions.Add(t.GetComponent<MapIndicator>());
        }
        foreach (Transform t in sideMissionParent)
        {
            sideMissions.Add(t.GetComponent<MapIndicator>());
        }
    }

    public void Remove(MapIndicator mapIndicator)
    {
        mainMissions.Remove(mapIndicator);
    }
    public MapIndicator GetMainMission(int index)
    {
        return mainMissions[index];
    }
    public void ShowMainMission(int index)
    {
        HideMainMissions();
        mainMissions[index].gameObject.SetActive(true);
    }
    public void ShowSideMissions()
    {
        foreach (var sm in sideMissions)
            sm.gameObject.SetActive(true);
    }
    public void HideMainMissions()
    {
        foreach (var mi in mainMissions)
            mi.gameObject.SetActive(false);
    }
    public void HideSideMissions()
    {
        foreach (var sm in sideMissions)
            sm.gameObject.SetActive(false);
    }
}
