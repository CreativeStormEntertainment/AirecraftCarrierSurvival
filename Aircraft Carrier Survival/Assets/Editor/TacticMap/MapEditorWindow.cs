using GambitUtils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapEditorWindow : EditorWindow
{
    public const string SceneName = "TacticMapEditor";
    public const string ScenePath = "Assets/GameplayAssets/Scenes/Editors/TacticMapEditor.unity";
    private const string DotPrefabPath = "Assets/Editor/Prefabs/DotPrefab.prefab";
    private const string PlayerPrefabPath = "Assets/Editor/Prefabs/PlayerPrefab.prefab";
    private const string EnemyPrefabPath = "Assets/Editor/Prefabs/EnemyPrefab.prefab";
    private const string MapCornerPointerPrefab = "Assets/Editor/Prefabs/MapCornerPointer.prefab";
    private const string RangePrefab = "Assets/Editor/Prefabs/Range.prefab";

    private const string FabularPath = "Assets/GameplayAssets/ScriptableData/TacticMaps/Fabular/";
    private const string SandboxPath = "Assets/GameplayAssets/ScriptableData/TacticMaps/Sandbox/";
    private const string RedWatersPath = "Assets/GameplayAssets/ScriptableData/TacticMaps/Sandbox/RedWaters/";
    private const string AssetPattern = "*.asset";

    public static SOTacticMap CurrentMap
    {
        get => _CurrentMap;
        private set
        {
            _CurrentMap = value;
            ObjectiveData.CurrentMap = value;
        }
    }

    public static TacticalMapGrid CurrentMapGrid
    {
        get;
        private set;
    }

    public static RectTransform MapCornerPointer
    {
        get;
        private set;
    }

    public static RectTransform MapCornerPointer2
    {
        get;
        private set;
    }

    public static RectTransform Range
    {
        get;
        private set;
    }

    public static RectTransform Range2
    {
        get;
        private set;
    }

    private static readonly string Tag = "Respawn";
    private static readonly Vector2 Size = new Vector2(45f, 45f);
    private static SOTacticMap _CurrentMap;

    private Scene currentScene;
    private Image mapImage;
    private RectTransform playerPosition;
    private RectTransform designerNodesRoot;
    private RectTransform enemiesRoot;
    private Transform objectivesRoot;
    private RectTransform additionalObjects;
    private RectTransform minMapPosition;
    private RectTransform maxMapPosition;

    private GameObject dotPrefab;
    private GameObject playerPrefab;
    private GameObject enemyPrefab;
    private RectTransform mapCornerPointerPrefab;
    private RectTransform rangePrefab;

    private EObjectiveType type;
    private EObjectiveEffect effect;

    private long nodes;

    private List<GUIObjectData> guiDatas;
    private int currentObjective = -1;

    [MenuItem("Tools/TacticMaps/Editor", false, 201)]
    private static void ShowWindow()
    {
        GetWindow<MapEditorWindow>().Show();
    }

    public MapEditorWindow()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        EditorApplication.update += OnUpdate;
    }

    private void OnFocus()
    {
        if (currentScene.name == null)
        {
            OnHierarchyChanged();
        }
    }

    private void OnGUI()
    {
        if (currentScene.name == SceneName)
        {
            Init();
        }
        float windowWidth = position.width;
        if (guiDatas == null)
        {
            guiDatas = new List<GUIObjectData> { new GUIObjectData(), new GUIObjectData(), new GUIObjectData() };
        }
        if (mapImage == null)
        {
            Buttons(windowWidth, true, 10f, new GUIInputData("Open editor", () =>
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(ScenePath);
                    }
                }));
            return;
        }

        if (dotPrefab == null)
        {
            dotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DotPrefabPath);
            playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            mapCornerPointerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MapCornerPointerPrefab).GetComponent<RectTransform>();
            rangePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RangePrefab).GetComponent<RectTransform>();
        }

        Label(windowWidth, "Load map");

        var list = new List<string>(Directory.GetFiles(FabularPath, AssetPattern));
        list.AddRange(Directory.GetFiles(SandboxPath, AssetPattern));
        list.AddRange(Directory.GetFiles(RedWatersPath, AssetPattern));

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        float width = 0f;
        foreach (var path in list)
        {
            int index = path.LastIndexOf('/') + 1;
            int index2 = path.LastIndexOf('.');
            var content = new GUIContent(path.Substring(index, index2 - index));
            float buttonWidth = GUI.skin.button.CalcSize(content).x;
            width += buttonWidth;
            if ((width + buttonWidth / 2f) > position.width)
            {
                width = buttonWidth;

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }
            if (GUILayout.Button(content, GUILayout.Width(buttonWidth)))
            {
                var map = AssetDatabase.LoadAssetAtPath<SOTacticMap>(path);
                nodes = 0;
                LoadMap(map);
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (CurrentMap != null)
        {
            Label(windowWidth, "Create objects:");

            Buttons(windowWidth, false, 0f,
                new GUIInputData("New node", () =>
                {
                    var obj = Create(Vector2.zero);
                    Selection.activeObject = obj;
                }), new GUIInputData("New enemy", () =>
                {
                    var obj = Create(new EnemyUnitData());
                    Selection.activeObject = obj;
                }), new GUIInputData("New objective", () =>
                {
                    var obj = Create(new ObjectiveData());
                    Selection.activeObject = obj;
                }));

            Gui(windowWidth, 0f, new GUIInputData(GUI.skin.button, "Find next objective with type", (data) => Button(data, () =>
                {
                    var objectives = SceneUtils.FindObjectsOfType<ObjectiveDataHolder>();
                    CheckCurrentObjective(objectives);
                    foreach (int index in OrderObjectives(objectives))
                    {
                        if (objectives[index].Data.Type == type)
                        {
                            currentObjective = index;
                            Selection.activeObject = objectives[currentObjective];
                            return;
                        }
                    }
                })), new GUIInputData(EditorStyles.popup, $"{type}   ", (data) =>
                {
                    var newType = (EObjectiveType)EditorGUI.EnumPopup(data.Rect, type);
                    if (newType != type)
                    {
                        type = newType;
                        currentObjective = -1;
                    }
                }));

            Gui(windowWidth, 0f, new GUIInputData(GUI.skin.button, "Find next objective with effect", (data) => Button(data, () =>
                {
                    var objectives = SceneUtils.FindObjectsOfType<ObjectiveDataHolder>();
                    CheckCurrentObjective(objectives);
                    foreach (int index in OrderObjectives(objectives))
                    {
                        foreach (var effectData in objectives[index].Data.Effects)
                        {
                            if (effectData.EffectType == effect)
                            {
                                currentObjective = index;
                                Selection.activeObject = objectives[currentObjective];
                                return;
                            }
                        }
                    }
                })), new GUIInputData(EditorStyles.popup, $"{effect}   ", (data) =>
                {
                    var newEffect = (EObjectiveEffect)EditorGUI.EnumPopup(data.Rect, effect);
                    if (newEffect != effect)
                    {
                        effect = newEffect;
                        currentObjective = -1;
                    }
                }));

            Buttons(windowWidth, true, 15f, new GUIInputData("Save map", SaveMap));
        }
    }

    private void Label(float windowWidth, string text)
    {
        guiDatas[0].Set(new GUIContent(text), GUI.skin.label);
        Center(windowWidth, guiDatas, 1);
        GUI.Label(guiDatas[0].Rect, guiDatas[0].Content);
    }

    private void Buttons(float windowWidth, bool space, float offset, params GUIInputData[] datas)
    {
        if (space)
        {
            EditorGUILayout.LabelField(" ");
        }
        foreach (var data in datas)
        {
            var local = data;
            local.Style = GUI.skin.button;
            local.Gui = (x) => Button(x, local.Effect);
        }
        Gui(windowWidth, offset, datas);
    }

    private void Gui(float windowWidth, float offset, params GUIInputData[] datas)
    {
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < datas.Length; i++)
        {
            guiDatas[i].Set(new GUIContent(datas[i].Text), datas[i].Style);
        }
        Center(windowWidth, guiDatas, datas.Length);
        for (int i = 0; i < datas.Length; i++)
        {
            guiDatas[i].Rect.center = new Vector2(guiDatas[i].Rect.center.x, guiDatas[i].Rect.center.y - offset);
            datas[i].Gui(guiDatas[i]);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void Button(GUIObjectData data, Action action)
    {
        if (GUI.Button(data.Rect, data.Content))
        {
            action();
        }
    }

    private void Init()
    {
        if (mapImage == null)
        {
            var go = GameObject.Find("MapBG");
            if (go != null)
            {
                mapImage = go.GetComponent<Image>();
            }
            CurrentMap = null;
        }
    }

    private void OnHierarchyChanged()
    {
        if (currentScene != SceneManager.GetActiveScene())
        {
            currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == SceneName)
            {
                Init();
            }
        }
    }

    private void OnUpdate()
    {
        if (Range != null && Range.gameObject.activeSelf && (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent(out ObjectiveDataHolder _)))
        {
            Range.gameObject.SetActive(false);
        }
        if (Range2 != null && Range2.gameObject.activeSelf && (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent(out ObjectiveDataHolder _)))
        {
            Range2.gameObject.SetActive(false);
        }
    }

    private void LoadMap(SOTacticMap map)
    {
        CurrentMap = map;
        currentObjective = -1;

        var toDestroy = new List<GameObject>();
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (!root.CompareTag(Tag))
            {
                toDestroy.Add(root);
            }
        }
        foreach (var obj in toDestroy)
        {
            DestroyImmediate(obj);
        }

        mapImage.sprite = map.Map;
        var trans = mapImage.transform;
        while (trans.parent != null)
        {
            trans = trans.parent;
        }
        DestroyObjects(trans);

        var rootParent = mapImage.transform.parent as RectTransform;
        playerPosition = Instantiate(playerPrefab).GetComponent<RectTransform>();
        playerPosition.name = "PlayerPos";
        SetupObject(playerPosition, rootParent, map.PlayerPosition, Size);

        designerNodesRoot = new GameObject("DesignerNodes", typeof(RectTransform)).GetComponent<RectTransform>();
        SetupObject(designerNodesRoot, rootParent, Vector2.zero, Vector2.zero);

        foreach (var node in CurrentMap.Nodes)
        {
            Create(node);
        }

        CurrentMapGrid = new TacticalMapGrid(CurrentMap.LandMask, 1920f, 1080f, 100, 200);

        enemiesRoot = new GameObject("Enemies", typeof(RectTransform)).GetComponent<RectTransform>();
        SetupObject(enemiesRoot, rootParent, Vector2.zero, Vector2.zero);
        foreach (var enemy in CurrentMap.EnemyUnits)
        {
            Create(enemy);
        }

        additionalObjects = new GameObject("Additional objects", typeof(RectTransform)).GetComponent<RectTransform>();
        SetupObject(additionalObjects, rootParent, Vector2.zero, Vector2.zero);

        minMapPosition = CreateObject("MinMapPosition", CurrentMap.MinMapPosition);
        maxMapPosition = CreateObject("MaxMapPosition", CurrentMap.MaxMapPosition);

        MapCornerPointer = Instantiate(mapCornerPointerPrefab);
        MapCornerPointer.name = "MapCornerPointer";
        var canvas = GambitUtils.SceneUtils.FindCanvas(null).GetComponent<RectTransform>();
        SetupObject(MapCornerPointer, canvas, new Vector2(-9000f, 540f), Size);

        MapCornerPointer2 = Instantiate(mapCornerPointerPrefab);
        MapCornerPointer2.name = "MapCornerPointer2";
        SetupObject(MapCornerPointer2, canvas, new Vector2(960f, -5000f), Size);

        Range = Instantiate(rangePrefab);
        Range.name = "ObjectiveReachRange";
        SetupObject(Range, canvas, new Vector2(0f, 0f), Size);
        Range.gameObject.SetActive(false);

        Range2 = Instantiate(rangePrefab);
        Range2.name = "DamageRange";
        SetupObject(Range2, canvas, new Vector2(0f, 0f), Size);
        Range2.gameObject.SetActive(false);

        objectivesRoot = new GameObject("Objectives").transform;
        foreach (var objective in CurrentMap.Objectives)
        {
            Create(objective);
        }
        foreach (var objective in CurrentMap.Objectives)
        {
            PostCreate(objective);
        }
    }

    private void SaveMap()
    {
        CurrentMap.PlayerPosition = playerPosition.anchoredPosition;

        CurrentMap.MinMapPosition = minMapPosition.anchoredPosition;
        CurrentMap.MaxMapPosition = maxMapPosition.anchoredPosition;

        CurrentMap.Nodes.Clear();
        var listNodes = new List<RectTransform>();
        foreach (RectTransform node in designerNodesRoot)
        {
            CurrentMap.Nodes.Add(node.anchoredPosition);
            listNodes.Add(node);
        }

        CurrentMap.EnemyUnits.Clear();
        foreach (RectTransform enemy in enemiesRoot)
        {
            var data = enemy.GetComponent<EnemyUnitDataHolder>().Data;
            data.Name = enemy.name;
            data.Position = enemy.anchoredPosition;
            foreach (var patrol in data.Patrols)
            {
                if (patrol == null)
                {
                    Debug.LogError("EMPTY/BROKEN PATROL");
                    continue;
                }
                patrol.Poses = new List<int>();
                foreach (var trans in patrol.Nodes)
                {
                    int index;
                    if (trans == null || ((index = listNodes.IndexOf(trans)) == -1))
                    {
                        Debug.LogError("EMPTY/BROKEN NODE");
                        continue;
                    }
                    patrol.Poses.Add(index);
                }
            }
            CurrentMap.EnemyUnits.Add(data);
        }

        if (CurrentMap.EnemyAttacks == null)
        {
            CurrentMap.EnemyAttacks = new EnemyAttackScenarioListData();
        }
        if (CurrentMap.EnemyAttacks.EnemiesAttackOnUs == null)
        {
            CurrentMap.EnemyAttacks.EnemiesAttackOnUs = new List<EnemyAttackOnUs24h>();
        }
        foreach (var item in CurrentMap.EnemyAttacks.EnemiesAttackOnUs)
        {
            while (item.EnemyAttacks.Count < CurrentMap.EnemyUnits.Count)
            {
                item.EnemyAttacks.Add(new EnemyAttackTimerCarrier());
            }
        }
        if (CurrentMap.EnemyAttacks.EnemiesAttacksOnFriends == null)
        {
            CurrentMap.EnemyAttacks.EnemiesAttacksOnFriends = new List<EnemyAttackFriend24h>();
        }
        foreach (var item in CurrentMap.EnemyAttacks.EnemiesAttacksOnFriends)
        {
            while (item.EnemyAttacks.Count < CurrentMap.EnemyUnits.Count)
            {
                item.EnemyAttacks.Add(new EnemyAttackTimerFriend());
            }
        }
        if (CurrentMap.EnemyAttacks.EnemiesAttacksOnFriends == null)
        {
            CurrentMap.EnemyAttacks.EnemiesSubmarinesAttacks = new List<EnemySubmarineAttackTimer>();
        }
        CurrentMap.Objectives.Clear();
        foreach (Transform objective in objectivesRoot)
        {
            if (!objective.TryGetComponent(out ObjectiveDataHolder holder))
            {
                Debug.LogError("EMPTY/BROKEN OBJECTIVE");
                continue;
            }
            holder.Data.Name = holder.name;
            var objectiveID = "objective " + holder.Data.Name;

            holder.Data.ObjectiveTargetIDs = new List<int>();
            holder.Data.ObjectiveTargetVectors = new List<Vector2>();
            foreach (var target in holder.Data.ObjectiveTargets)
            {
                int id = -1;
                var pos = target.anchoredPosition;
                if (target.TryGetComponent(out EnemyUnitDataHolder enemyHolder))
                {
                    id = CurrentMap.EnemyUnits.IndexOf(enemyHolder.Data);
                    pos = Vector2.zero;
                }
                holder.Data.ObjectiveTargetIDs.Add(id);
                holder.Data.ObjectiveTargetVectors.Add(pos);
            }

            holder.Data.SecondaryTarget = -1;
            if (holder.Data.Type == EObjectiveType.Reach && holder.Data.SecondaryTargetEnemy != null)
            {
                holder.Data.SecondaryTarget = CurrentMap.EnemyUnits.IndexOf(holder.Data.SecondaryTargetEnemy.Data);
                if (holder.Data.SecondaryTarget == -1)
                {
                    Debug.LogError("Cannot find secondary enemy target for " + objectiveID);
                }
            }

            CurrentMap.Objectives.Add(holder.Data);
        }

        for (int i = 0; i < CurrentMap.Objectives.Count; i++)
        {
            var objective = CurrentMap.Objectives[i];
            var objectiveID = "objective " + objective.Name;

            objective.Targets.Clear();
            switch (objective.Type)
            {
                case EObjectiveType.CompleteObjective:
                    AddObjectives(objective.TargetTranses, objective.Targets, objectiveID);
                    break;
                case EObjectiveType.Destroy:
                case EObjectiveType.DestroyBlock:
                case EObjectiveType.Reveal:
                case EObjectiveType.FinishMissions:
                case EObjectiveType.SendAirstrikeWithoutLosses:
                case EObjectiveType.RevealBlocks:
                case EObjectiveType.EnemyProximity:
                    AddEnemies(objective.TargetTranses, objective.Targets, objectiveID);
                    break;
                case EObjectiveType.Reach:
                case EObjectiveType.SetSpecificCourse:
                    AddNodes(objective.TargetTranses, listNodes, objective.Targets, objectiveID);
                    break;
            }
            foreach (var data in objective.Effects)
            {
                data.Targets.Clear();
                switch (data.EffectType)
                {
                    case EObjectiveEffect.ShowObjectives:
                    case EObjectiveEffect.ActivateObjectives:
                    case EObjectiveEffect.SucceedObjectives:
                    case EObjectiveEffect.SetObjectiveText:
                        AddObjectives(data.TargetTranses, data.Targets, objectiveID);
                        break;
                    case EObjectiveEffect.Reveal:
                    case EObjectiveEffect.Destroy:
                    case EObjectiveEffect.ShowHidden:
                    case EObjectiveEffect.Spawn:
                    case EObjectiveEffect.EnemyCanChase:
                    case EObjectiveEffect.AddMission:
                    case EObjectiveEffect.SearchAndDestroy:
                    case EObjectiveEffect.SpawnSurvivors:
                    case EObjectiveEffect.SpawnNeutral:
                    case EObjectiveEffect.DestroyBlock:
                    case EObjectiveEffect.TeleportEnemy:
                    case EObjectiveEffect.ShowAlly:
                    case EObjectiveEffect.ShowMissionRange:
                    case EObjectiveEffect.Identify:
                        AddEnemies(data.TargetTranses, data.Targets, objectiveID);
                        break;
                    case EObjectiveEffect.ForceCarrierWaypoints:
                    case EObjectiveEffect.SetEnableCoursePosition:
                        AddNodes(data.TargetTranses, listNodes, data.Targets, objectiveID);
                        break;
                    case EObjectiveEffect.SetShowSpriteStrategyPanel:
                        if (!data.NotEffect)
                        {
                            AddEnemies(data.TargetTranses, data.Targets, objectiveID);
                        }
                        data.ObjectiveTarget = AddObjective(data.ObjectiveTransform, objectiveID);
                        break;
                    case EObjectiveEffect.ShowTimer:
                    case EObjectiveEffect.Win:
                    case EObjectiveEffect.AttackCarrier:
                    case EObjectiveEffect.FinalestDestroySections:
                    case EObjectiveEffect.ForceCarrierSpeed:
                    case EObjectiveEffect.SpawnTowShip:
                    case EObjectiveEffect.SetEnableUI:
                    case EObjectiveEffect.SetEnableCameraInput:
                    case EObjectiveEffect.SetEnableOfficer:
                    case EObjectiveEffect.SetEnableIslandRoomSelection:
                    case EObjectiveEffect.SetEnableSwitch:
                    case EObjectiveEffect.SetEnableCrew:
                    case EObjectiveEffect.SetEnableDepartmentPlacement:
                    case EObjectiveEffect.SetEnableSquadronType:
                    case EObjectiveEffect.SetEnableDragPlanes:
                    case EObjectiveEffect.SetEnableMissions:
                    case EObjectiveEffect.SetEnableReconUO:
                    case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                    case EObjectiveEffect.SetEnableRecoveryTimeout:
                    case EObjectiveEffect.SetEnableForcedStrategy:
                    case EObjectiveEffect.SetEnableEscort:
                    case EObjectiveEffect.SetEnableSpreadIssue:
                    case EObjectiveEffect.SetEnableDCIssueDestination:
                    case EObjectiveEffect.SetEnableDCInMaintenance:
                    case EObjectiveEffect.SetEnableDCInPumps:
                    case EObjectiveEffect.SetEnableRescueTimer:
                    case EObjectiveEffect.SetEnableMoveTime:
                    case EObjectiveEffect.SetEnableObsoleteMission:
                    case EObjectiveEffect.SetEnableMaxPlanes:
                    case EObjectiveEffect.SetEnableEvents:
                    case EObjectiveEffect.SetEnableDCCategory:
                    case EObjectiveEffect.ResetEnables:
                    case EObjectiveEffect.SetMaxCarrierSpeed:
                    case EObjectiveEffect.SetTimeSpeed:
                    case EObjectiveEffect.SetSupplies:
                    case EObjectiveEffect.DestroyNeutrals:
                    case EObjectiveEffect.SetSentMissionDuration:
                    case EObjectiveEffect.SpawnAttack:
                    case EObjectiveEffect.SpawnIssue:
                    case EObjectiveEffect.SpawnInjuredCrew:
                    case EObjectiveEffect.SetShowHighlight:
                    case EObjectiveEffect.SetShowNarrator:
                    case EObjectiveEffect.SetCamera:
                    case EObjectiveEffect.FinishCourseSettingMode:
                    case EObjectiveEffect.DisableEscortButton:
                    case EObjectiveEffect.DisableCancelMission:
                    case EObjectiveEffect.DisableEnemyDisappear:
                    case EObjectiveEffect.DisableBuffClose:
                    case EObjectiveEffect.DisableBuffDeallocation:
                    case EObjectiveEffect.DisableOfficerDeallocation:
                    case EObjectiveEffect.DetectPlayer:
                    case EObjectiveEffect.SetShowObject:
                    case EObjectiveEffect.SwitchFriendlyCAPToMidway:
                    case EObjectiveEffect.ShowPath:
                    case EObjectiveEffect.ShowRescueRange:
                    case EObjectiveEffect.TeleportPlayer:
                    case EObjectiveEffect.AdvanceTime:
                    case EObjectiveEffect.ShowCannonRange:
                    case EObjectiveEffect.ShowSurvivors:
                    case EObjectiveEffect.KillSurvivor:
                    case EObjectiveEffect.ShowCarrier:
                    case EObjectiveEffect.LaunchBombers:
                    case EObjectiveEffect.DestroyNotSentMissions:
                    case EObjectiveEffect.DestroySection:
                    case EObjectiveEffect.ShowMagicSprite:
                    case EObjectiveEffect.DisableAttacksOnAlly:
                    case EObjectiveEffect.RemovePermamentlyMagicIdentify:
                    case EObjectiveEffect.SetSuperTimeSpeed:
                    case EObjectiveEffect.CancelMissions:
                    case EObjectiveEffect.DisableDeath:
                        break;
                    case EObjectiveEffect.ShowBalancedForcesBar:
                    case EObjectiveEffect.MakeDamageRange:
                    case EObjectiveEffect.SetEnableAirstrikeTarget:
                        if (!data.NotEffect)
                        {
                            AddEnemies(data.TargetTranses, data.Targets, objectiveID);
                        }
                        break;
                    case EObjectiveEffect.CustomMissionRetrieval:
                        if (data.RetrievePositionRect == null)
                        {
                            Debug.LogError("Retrieval position for mission retrieve is not set");
                        }
                        else
                        {
                            data.RetrievePosition = listNodes.IndexOf(data.RetrievePositionRect);
                            if (data.RetrievePosition == -1)
                            {
                                Debug.LogError("Empty/broken node in effects in " + objectiveID);
                            }
                        }
                        break;
                }
            }
        }

        EditorUtility.SetDirty(CurrentMap);
        AssetDatabase.SaveAssets();
    }

    private void DestroyObjects(Transform trans)
    {
        var toDestroy = new List<GameObject>();
        foreach (Transform child in trans)
        {
            if (child.gameObject.CompareTag(Tag))
            {
                DestroyObjects(child);
            }
            else
            {
                toDestroy.Add(child.gameObject);
            }
        }
        foreach (var obj in toDestroy)
        {
            DestroyImmediate(obj);
        }
    }

    private void SetupObject(RectTransform obj, RectTransform parent, Vector2 pos, Vector2 size)
    {
        obj.SetParent(parent);
        obj.anchoredPosition = pos;
        obj.localScale = Vector3.one;
        obj.sizeDelta = size;
    }

    private GameObject Create(Vector2 pos)
    {
        var obj = Instantiate(dotPrefab);
        obj.name = "Node" + (++nodes);
        SetupObject(obj.GetComponent<RectTransform>(), designerNodesRoot, pos, Size);
        return obj;
    }

    private GameObject Create(EnemyUnitData enemy)
    {
        var obj = Instantiate(enemyPrefab);
        obj.name = enemy.Name;
        SetupObject(obj.GetComponent<RectTransform>(), enemiesRoot, enemy.Position, Size);
        obj.GetComponent<EnemyUnitDataHolder>().Data = enemy;
        if (enemy.IsAlly)
        {
            obj.GetComponent<Image>().color = Color.green;
        }

        if (enemy.Patrols != null)
        {
            foreach (var patrol in enemy.Patrols)
            {
                patrol.Nodes.Clear();
                foreach (int pos in patrol.Poses)
                {
                    if (designerNodesRoot.childCount <= pos)
                    {
                        Debug.LogError("Cannot find node patrol in enemy " + enemy.Name);
                    }
                    else
                    {
                        patrol.Nodes.Add(designerNodesRoot.GetChild(pos) as RectTransform);
                    }
                }
            }
        }
        obj.GetComponent<EnemyUnitDataHolder>().MapGrid = CurrentMapGrid;
        obj.GetComponent<EnemyUnitDataHolder>().RecalculatePath();
        return obj;
    }

    private GameObject Create(ObjectiveData objective)
    {
        var obj = new GameObject(objective.Name, typeof(ObjectiveDataHolder));
        obj.transform.SetParent(objectivesRoot);
        obj.GetComponent<ObjectiveDataHolder>().Data = objective;
        return obj;
    }

    private void PostCreate(ObjectiveData objective)
    {
        string objectiveID = " for objective " + objective.Name;

        if (objective.ObjectiveTargets == null)
        {
            objective.ObjectiveTargets = new List<RectTransform>();
        }
        if (objective.ObjectiveTargetIDs == null)
        {
            objective.ObjectiveTargetIDs = new List<int>();
        }
        if (objective.ObjectiveTargetVectors == null)
        {
            objective.ObjectiveTargetVectors = new List<Vector2>();
        }

        objective.ObjectiveTargets.Clear();
        for (int i = 0; i < objective.ObjectiveTargetIDs.Count; i++)
        {
            if (objective.ObjectiveTargetIDs[i] > -1)
            {
                SetObject(objective.ObjectiveTargetIDs[i], enemiesRoot, out var enemy, "Objective enemy target ", objectiveID);
                objective.ObjectiveTargets.Add(enemy.GetComponent<RectTransform>());
            }
            else
            {
                objective.ObjectiveTargets.Add(CreateObject("Objective sprite position", objective.ObjectiveTargetVectors[i]));
            }
        }

        objective.TargetTranses.Clear();
        if (objective.Targets != null)
        {
            switch (objective.Type)
            {
                case EObjectiveType.CompleteObjective:
                    foreach (var id in objective.Targets)
                    {
                        AddObject(id, objectivesRoot, objective.TargetTranses, "Objective ", objectiveID);
                    }
                    break;
                case EObjectiveType.Destroy:
                case EObjectiveType.DestroyBlock:
                case EObjectiveType.Reveal:
                case EObjectiveType.FinishMissions:
                case EObjectiveType.SendAirstrikeWithoutLosses:
                case EObjectiveType.RevealBlocks:
                case EObjectiveType.EnemyProximity:
                    foreach (var id in objective.Targets)
                    {
                        AddObject(id, enemiesRoot, objective.TargetTranses, "Enemy ", objectiveID);
                    }
                    break;
                case EObjectiveType.Reach:
                case EObjectiveType.SetSpecificCourse:
                    foreach (var id in objective.Targets)
                    {
                        AddObject(id, designerNodesRoot, objective.TargetTranses, "Node ", objectiveID);
                    }
                    break;
            }
        }
        if (objective.Type == EObjectiveType.Reach && objective.SecondaryTarget > -1)
        {
            SetObject(objective.SecondaryTarget, enemiesRoot, out objective.SecondaryTargetEnemy, "Secondary enemy ", objectiveID);
        }
        if (objective.Effects != null)
        {
            foreach (var data in objective.Effects)
            {
                data.TargetTranses.Clear();
                data.RetrievePositionRect = null;
                switch (data.EffectType)
                {
                    case EObjectiveEffect.ShowObjectives:
                    case EObjectiveEffect.ActivateObjectives:
                    case EObjectiveEffect.SucceedObjectives:
                    case EObjectiveEffect.SetObjectiveText:
                        foreach (var id in data.Targets)
                        {
                            AddObject(id, objectivesRoot, data.TargetTranses, "Objective in effect ", objectiveID);
                        }
                        break;
                    case EObjectiveEffect.Reveal:
                    case EObjectiveEffect.Destroy:
                    case EObjectiveEffect.ShowHidden:
                    case EObjectiveEffect.Spawn:
                    case EObjectiveEffect.EnemyCanChase:
                    case EObjectiveEffect.AddMission:
                    case EObjectiveEffect.SearchAndDestroy:
                    case EObjectiveEffect.SpawnSurvivors:
                    case EObjectiveEffect.MakeDamageRange:
                    case EObjectiveEffect.SpawnNeutral:
                    case EObjectiveEffect.DestroyBlock:
                    case EObjectiveEffect.TeleportEnemy:
                    case EObjectiveEffect.ShowAlly:
                    case EObjectiveEffect.ShowMissionRange:
                    case EObjectiveEffect.Identify:
                        foreach (var id in data.Targets)
                        {
                            AddObject(id, enemiesRoot, data.TargetTranses, "Enemy in effect ", objectiveID);
                        }
                        break;
                    case EObjectiveEffect.ForceCarrierWaypoints:
                    case EObjectiveEffect.SetEnableCoursePosition:
                        foreach (var id in data.Targets)
                        {
                            AddObject(id, designerNodesRoot, data.TargetTranses, "Node in effect ", objectiveID);
                        }
                        break;
                    case EObjectiveEffect.ShowBalancedForcesBar:
                    case EObjectiveEffect.SetEnableAirstrikeTarget:
                        if (!data.NotEffect)
                        {
                            foreach (var id in data.Targets)
                            {
                                AddObject(id, enemiesRoot, data.TargetTranses, "Enemy in effect ", objectiveID);
                            }
                        }
                        break;
                    case EObjectiveEffect.SetShowSpriteStrategyPanel:
                        if (!data.NotEffect)
                        {
                            foreach (var id in data.Targets)
                            {
                                AddObject(id, enemiesRoot, data.TargetTranses, "Enemy in effect ", objectiveID);
                            }
                        }
                        data.ObjectiveTransform = AddObject(data.ObjectiveTarget, objectivesRoot, "Objective in effect ", objectiveID);
                        break;
                    case EObjectiveEffect.ShowTimer:
                    case EObjectiveEffect.Win:
                    case EObjectiveEffect.AttackCarrier:
                    case EObjectiveEffect.FinalestDestroySections:
                    case EObjectiveEffect.ForceCarrierSpeed:
                    case EObjectiveEffect.SpawnTowShip:
                    case EObjectiveEffect.SetEnableUI:
                    case EObjectiveEffect.SetEnableCameraInput:
                    case EObjectiveEffect.SetEnableOfficer:
                    case EObjectiveEffect.SetEnableIslandRoomSelection:
                    case EObjectiveEffect.SetEnableSwitch:
                    case EObjectiveEffect.SetEnableCrew:
                    case EObjectiveEffect.SetEnableDepartmentPlacement:
                    case EObjectiveEffect.SetEnableSquadronType:
                    case EObjectiveEffect.SetEnableDragPlanes:
                    case EObjectiveEffect.SetEnableMissions:
                    case EObjectiveEffect.SetEnableReconUO:
                    case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                    case EObjectiveEffect.SetEnableRecoveryTimeout:
                    case EObjectiveEffect.SetEnableForcedStrategy:
                    case EObjectiveEffect.SetEnableEscort:
                    case EObjectiveEffect.SetEnableSpreadIssue:
                    case EObjectiveEffect.SetEnableDCIssueDestination:
                    case EObjectiveEffect.SetEnableDCInMaintenance:
                    case EObjectiveEffect.SetEnableDCInPumps:
                    case EObjectiveEffect.SetEnableRescueTimer:
                    case EObjectiveEffect.SetEnableMoveTime:
                    case EObjectiveEffect.SetEnableObsoleteMission:
                    case EObjectiveEffect.SetEnableMaxPlanes:
                    case EObjectiveEffect.SetEnableEvents:
                    case EObjectiveEffect.SetEnableDCCategory:
                    case EObjectiveEffect.ResetEnables:
                    case EObjectiveEffect.SetMaxCarrierSpeed:
                    case EObjectiveEffect.SetTimeSpeed:
                    case EObjectiveEffect.SetSupplies:
                    case EObjectiveEffect.DestroyNeutrals:
                    case EObjectiveEffect.SetSentMissionDuration:
                    case EObjectiveEffect.SpawnAttack:
                    case EObjectiveEffect.SpawnIssue:
                    case EObjectiveEffect.SpawnInjuredCrew:
                    case EObjectiveEffect.SetShowHighlight:
                    case EObjectiveEffect.SetShowNarrator:
                    case EObjectiveEffect.SetCamera:
                    case EObjectiveEffect.FinishCourseSettingMode:
                    case EObjectiveEffect.DisableEscortButton:
                    case EObjectiveEffect.DisableCancelMission:
                    case EObjectiveEffect.DisableEnemyDisappear:
                    case EObjectiveEffect.DisableBuffClose:
                    case EObjectiveEffect.DisableBuffDeallocation:
                    case EObjectiveEffect.DisableOfficerDeallocation:
                    case EObjectiveEffect.DetectPlayer:
                    case EObjectiveEffect.SetShowObject:
                    case EObjectiveEffect.SwitchFriendlyCAPToMidway:
                    case EObjectiveEffect.ShowPath:
                    case EObjectiveEffect.ShowRescueRange:
                    case EObjectiveEffect.TeleportPlayer:
                    case EObjectiveEffect.AdvanceTime:
                    case EObjectiveEffect.ShowCannonRange:
                    case EObjectiveEffect.ShowSurvivors:
                    case EObjectiveEffect.KillSurvivor:
                    case EObjectiveEffect.ShowCarrier:
                    case EObjectiveEffect.LaunchBombers:
                    case EObjectiveEffect.DestroyNotSentMissions:
                    case EObjectiveEffect.DestroySection:
                    case EObjectiveEffect.ShowMagicSprite:
                    case EObjectiveEffect.DisableAttacksOnAlly:
                    case EObjectiveEffect.RemovePermamentlyMagicIdentify:
                    case EObjectiveEffect.SetSuperTimeSpeed:
                    case EObjectiveEffect.CancelMissions:
                    case EObjectiveEffect.DisableDeath:
                        break;
                    case EObjectiveEffect.CustomMissionRetrieval:
                        if (data.RetrievePosition < 0 || data.RetrievePosition >= designerNodesRoot.childCount)
                        {
                            Debug.LogError("Node " + data.RetrievePosition + " not found" + objectiveID);
                        }
                        else
                        {
                            data.RetrievePositionRect = designerNodesRoot.GetChild(data.RetrievePosition).GetComponent<RectTransform>();
                        }
                        break;
                }
            }
        }
    }

    private void Center(float totalWidth, List<GUIObjectData> datas, int count)
    {
        float offset = -2f;
        for (int i = 0; i < count; i++)
        {
            datas[i].Rect = GUILayoutUtility.GetRect(datas[i].Content, datas[i].Style, GUILayout.ExpandWidth(false));
            offset += datas[i].Rect.width;
            offset += 2f;
        }
        offset = (totalWidth - offset) / 2f;
        for (int i = 0; i < count; i++)
        {
            float width = datas[i].Rect.width;
            datas[i].Rect.center = new Vector2(offset + width / 2f, datas[i].Rect.center.y);
            offset += width + 2f;
        }
    }

    private RectTransform GetNode(Vector2 pos)
    {
        float dist = Mathf.Infinity;
        RectTransform node = null;
        foreach (RectTransform newNnode in designerNodesRoot)
        {
            float newDist = Vector2.SqrMagnitude(newNnode.anchoredPosition - pos);
            if (newDist < dist)
            {
                dist = newDist;
                node = newNnode;
            }
        }
        if (dist > 10f)
        {
            return null;
        }
        return node;
    }

    private void AddObjectives(List<Transform> transes, List<int> list, string debugText)
    {
        foreach (var trans in transes)
        {
            int index = AddObjective(trans, debugText);
            if (index != -1)
            {
                list.Add(index);
            }
        }
    }

    private int AddObjective(Transform trans, string debugText)
    {
        var error = "Empty/broken objective in effects in " + debugText;
        if (trans == null || !trans.TryGetComponent(out ObjectiveDataHolder holder))
        {
            Debug.LogError(error);
            return -1;
        }
        int index = CurrentMap.Objectives.IndexOf(holder.Data);
        if (index == -1)
        {
            Debug.LogError(error);
        }
        return index;
    }


    private void AddEnemies(List<Transform> transes, List<int> list, string debugText)
    {
        foreach (var trans in transes)
        {
            var error = "Empty/broken enemy in effects in " + debugText;
            if (trans == null || !trans.TryGetComponent(out EnemyUnitDataHolder holder))
            {
                Debug.LogError(error);
                continue;
            }
            int index = CurrentMap.EnemyUnits.IndexOf(holder.Data);
            if (index == -1)
            {
                Debug.LogError(error);
            }
            else
            {
                list.Add(index);
            }
        }
    }

    private void AddNodes(List<Transform> transes, List<RectTransform> nodes, List<int> list, string debugText)
    {
        foreach (RectTransform trans in transes)
        {
            var error = "Empty/broken node in effects in " + debugText;
            int index = nodes.IndexOf(trans);
            if (index == -1)
            {
                Debug.LogError(error);
            }
            else
            {
                list.Add(index);
            }
        }
    }

    private RectTransform CreateObject(string name, Vector2 pos)
    {
        var obj = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        SetupObject(obj, additionalObjects, pos, Size);
        return obj;
    }

    private void SetObject(int id, RectTransform root, out EnemyUnitDataHolder obj, string debug, string debug2)
    {
        if (id < 0 || id >= root.childCount)
        {
            obj = null;
            Debug.LogError(debug + id + " not found" + debug2);
        }
        else
        {
            var trans = root.GetChild(id);
            if (!trans.TryGetComponent(out obj))
            {
                Debug.LogError(debug + id + " not found" + debug2);
            }
        }
    }

    private void AddObject(int id, Transform root, List<Transform> list, string debug, string debug2)
    {
        var trans = AddObject(id, root, debug, debug2);
        if (trans != null)
        {
            list.Add(trans);
        }
    }

    private Transform AddObject(int id, Transform root, string debug, string debug2)
    {
        if (id < 0 || id >= root.childCount)
        {
            Debug.LogError(debug + id + " not found" + debug2);
            return null;
        }
        else
        {
            return root.GetChild(id);
        }
    }

    private void CheckCurrentObjective(List<ObjectiveDataHolder> objectives)
    {
        bool found = false;
        if (currentObjective >= 0 && currentObjective <= objectives.Count)
        {
            foreach (var go in Selection.gameObjects)
            {
                if (go == objectives[currentObjective].gameObject)
                {
                    found = true;
                    break;
                }
            }
        }
        if (!found)
        {
            currentObjective = -1;
        }
    }

    private IEnumerable<int> OrderObjectives(List<ObjectiveDataHolder> objectives)
    {
        for (int i = 1; i <= objectives.Count; i++)
        {
            yield return (i + currentObjective) % objectives.Count;
        }
        currentObjective = -1;
        Debug.Log("Not found");
    }
}
