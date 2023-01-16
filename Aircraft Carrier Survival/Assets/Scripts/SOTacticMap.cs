using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TacticMap#", menuName = "ScriptableObjects/SOTacticMap", order = 1)]
public class SOTacticMap : ScriptableObject
{
    public string MissionTitleID;
    public Sprite MapShadow;
    public Sprite Map;
    public Texture2D LandMask;
    public RenderTexture MapShadowRenderTexture;
    public RenderTexture MapRenderTexture;
    public List<MapRandomFleetData> Fleets;
    public List<Vector2> ShipPositions;

    public bool Sandbox;

    public Vector2 PlayerPosition;
    public List<Vector2> Nodes;
    public List<EnemyUnitData> EnemyUnits;
    public List<ObjectiveData> Objectives;

    public int AdditionalObjectsToSpawnMin;
    public int AdditionalObjectsToSpawnMax;

    public Vector2 MinMapPosition = new Vector2(-721f, -454f);
    public Vector2 MaxMapPosition = new Vector2(721f, 454f);

    public List<VideoData> Movies;

    public EnemyAttackScenarioListData EnemyAttacks;

    public int EnemyAttackPowerModifier;

    public string PlaceholderBriefingID = "";

    public GameObject CloudsPrefab;
    public GameObject AdditionalCloudsPrefab;
    public Vector2 CloudDirection = Vector2.left;
    public float CloudSpeed = 1f;

    public DayTime Date;

    public MissionPrepareData Overrides;

    public int TutorialID;

    public string BriefsFile;

    [HideInInspector]
    public EMissionDifficulty Difficulty;
    [HideInInspector]
    public EEnemiesCount EnemiesCount;
    [HideInInspector]
    public bool AllyAttacks;
}
