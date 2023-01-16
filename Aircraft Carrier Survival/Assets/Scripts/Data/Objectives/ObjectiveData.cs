using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectiveData
{
    public string Title;
    public string Description;

    [NonSerialized]
    public string[] Params;

    [Readonly]
    public string Name = "Objective";

    public List<RectTransform> ObjectiveTargets;
    public List<int> ObjectiveTargetIDs = new List<int>();
    public List<Vector2> ObjectiveTargetVectors = new List<Vector2>();
    public bool UOObjectives;

    public bool InverseFinishStateInSummary;
    public EObjectiveCategory ObjectiveCategory;
    public EObjectiveType Type;
    public bool NotType;
    public EObjectiveTarget TargetType;
    public bool FailOnNegativeTarget;
    public int Count;
    public EMissionOrderType MissionType;
    public bool Simultaneously;
    public EnemyManeuverData TargetBlock;
    public List<Transform> TargetTranses;
    public List<int> Targets;
    public EnemyUnitDataHolder SecondaryTargetEnemy;
    public int SecondaryTarget;
    public EStrikeGroupActiveSkill StrikeGroupActiveSkill;
    public ECameraView CameraView;
    public EIslandRoomType RoomType;
    public EWindowType WindowType;
    public int IndexValue;
    public EPlaneType SquadronType;
    public EManeuverType ManeuverType;
    public float Range;
    public bool InverseComplete;
    public MapCornerData MapCornerData;

    public EMissionLoseCause LoseType;

    public List<ObjectiveEffectData> Effects;
    public bool Active;
    public bool Visible;

    [NonSerialized]
    public bool CurrentActive;
    [NonSerialized]
    public bool CurrentVisible;

#if UNITY_EDITOR
    [NonSerialized]
    public bool Expanded;

    public static SOTacticMap CurrentMap;

    private const string Data = "data[";
    private const char EndData = ']';

    public static bool GetObjectiveData(UnityEditor.SerializedProperty property, out SOTacticMap map, out ObjectiveData data)
    {
        if (property.serializedObject.targetObject is SOTacticMap map2)
        {
            map = map2;
            int index = property.propertyPath.IndexOf(Data) + Data.Length;
            int endIndex = property.propertyPath.IndexOf(EndData);
            index = int.Parse(property.propertyPath.Substring(index, endIndex - index));

            data = map.Objectives[index];
            return false;
        }
        else
        {
            map = CurrentMap;
            data = (property.serializedObject.targetObject as ObjectiveDataHolder).Data;
            return true;
        }
    }
#endif
}