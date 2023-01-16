using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct MissionSave
{
    public int MissionID;
    public int ChapterID;

    public void Setup(int missionID, int chapterID)
    {
        MissionID = missionID;
        ChapterID = chapterID;
    }
}
