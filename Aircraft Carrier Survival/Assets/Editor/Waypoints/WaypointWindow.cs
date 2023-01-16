using GambitUtils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class WaypointWindow : EditorWindow
{
    public const string SceneName = "Waypoints";
    private const string NodeStartName = "loc.";
    private const string ExitNodeName = "loc.exit.";
    private const int SegmentLayer = Physics.DefaultRaycastLayers & ~(1 << 26);
    private const double MinKeyBreakTime = .75d;

    private static readonly List<string> Path = new List<string>() { @"Assets/GameplayAssets/Prefabs/Sections/", @"Assets/GameplayAssets/Prefabs/CV5/Sections/", @"Assets/GameplayAssets/Prefabs/CV9/Sections/" };

    private static readonly List<List<string>> SectionPaths = new List<List<string>>()
    {
        new List<string> { "1.1_meteo", "1.2_piloci", "1.3_pompy", "1.4_namierzanie", "1.5_paliwo", "1.6_komunikacja",
        "2.1_hangar", "2.2_szpital", "2.4_zaloga", "2.5_generatory",
        "3.2_bomby", "3.3_DC", "3.4_warsztat", "3.6_generatory", "3.7_generatory",
        "4.1_szatnie", "4.2_amunicja", "4.3_zaloga", "4.4_warsztaty", "4.5_generatory", "4.6_sterownia",
        "WreckSection", "Deck", "Island-bridge" },
        new List<string>() { "1_Meteorology", "2_Targeting", "3_Pilots", "4_FuelDeposit", "5_Comms", "6_Hangar",
        "8_AA", "9_Sickbay", "10_Engines", "11_CrewQuarters", "12_AmmoSupply", "13_MainGenerators", "14_DamageControl", "15_AircraftWorkshop",
        "16_Pumps", "17_BackupGenerators", "18_Deck", "20_Turbines", "21_Workshop", "22_Helm", "23_AA2", "24_Lockers",
        "WreckSection", "Deck", "Island-bridge" },
        new List<string>() { "1_Lockers", "2_DC", "3_Helm", "4_Crew_Quarters", "5_Engines", "6_Anti-ai_Guns", "7_Workshops",
        "8_Pumps_2", "9_Hangar", "10_DC_1", "11_Crew_quarters_1", "12_Meteorology", "13_Sickbay", "14_Targeting", "15_Turbines", "16_Comms", "17_Pilots", "18_Main_Generators",
        "19_Fuel_Deposit", "20_Aircraft_Workshop", "21_Ammo_Supply", "22_Pumps_1",
        "WreckSection", "Deck", "Island-bridge" }
    };

    private static readonly List<string> LightPath = new List<string>() { @"Assets/GameplayAssets/Prefabs/Lights_CV3.prefab", @"Assets/GameplayAssets/Prefabs/Lights_CV5.prefab", @"Assets/GameplayAssets/Prefabs/Lights_CV9.prefab" };

    private int wreckSectionID;
    private int deckID;
    private int bridgeID;
    private readonly string[] subsectionStrings = new string[9];

    private int currentSection = -1;
    private int loadedSection = -1;
    private int currentSubsection = -1;
    private GameObject workplace;
    private GameObject helpers;
    private MeshCollider collider;
    private GameObject colliderGO;
    private GameObject targetGO;
    private GameObject rootPrefab;
    private WorkerPath target;
    private Waypoint prefab;
    private int waypointSelectionLayer;
    private int waypointSelectionLayerID;
    private int waypointLayer;
    private int waypointLayerID;
    private string error = "";
    private Vector2 baseScrollPosition = Vector2.zero;
    private Vector2 errorScrollPosition = Vector2.zero;
    private Scene currentScene;
    private AnimationManager animMan;

    private double duplicateBreakTime = -100d;
    private double connectBreakTime = -100d;

    private Dictionary<Collider, ColliderData> connectors;

    private int selectedCarrierInt = -1;

    private Transform waypointsParent;

    [MenuItem("Tools/Waypoints/Editor", false, 203)]
    private static void ShowWindow()
    {
        GetWindow<WaypointWindow>().Show();
    }

    public WaypointWindow()
    {
        EditorApplication.hierarchyChanged += HierarchyChanged;
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;

        connectors = new Dictionary<Collider, ColliderData>();
        target = null;
        targetGO = null;
        rootPrefab = null;
        workplace = null;
        colliderGO = null;
        animMan = null;
    }

    private void OnFocus()
    {
        Preinit();
    }

    private void HierarchyChanged()
    {
        if (currentScene != SceneManager.GetActiveScene())
        {
            currentScene = SceneManager.GetActiveScene();
            Init();
        }
        if (SceneManager.GetActiveScene().name == SceneName && workplace == null)
        {
            Init();
        }
    }

    private void Preinit()
    {
        waypointLayer = LayerMask.GetMask("Waypoint");
        waypointLayerID = LayerMask.NameToLayer("Waypoint");
        waypointSelectionLayer = LayerMask.GetMask("WaypointSelection");
        waypointSelectionLayerID = LayerMask.NameToLayer("WaypointSelection");

        workplace = GameObject.Find("Workplace");
        colliderGO = GameObject.Find("MeshCollider");

        if (SceneManager.GetActiveScene().name == SceneName)
        {
            if (colliderGO == null)
            {
                colliderGO = new GameObject("MeshCollider");
                colliderGO.transform.SetParent(workplace.transform);
            }
            colliderGO.layer = waypointLayerID;
        }

        helpers = GameObject.Find("Helpers");

        prefab = AssetDatabase.LoadAssetAtPath<Waypoint>(@"Assets/Editor/Prefabs/Waypoint.prefab");

        error = "";
        errorScrollPosition = Vector2.zero;

        animMan = FindObjectOfType<AnimationManager>();
    }

    private void Init()
    {
        currentSection = -1;
        currentSubsection = -1;
        loadedSection = -1;
        selectedCarrierInt = -1;
        Preinit();
        targetGO = GameObject.Find("Target");
        if (targetGO != null)
        {
            DestroyImmediate(targetGO);
            targetGO = null;
        }
    }

    private void OnSceneGUI(SceneView view)
    {
        if (SceneManager.GetActiveScene().name == SceneName && workplace == null)
        {
            Init();
        }
        if (target != null)
        {
            //if (Event.current.type == EventType.Layout)
            //{
            //    HandleUtility.AddDefaultControl(0);
            //}
            //if (Event.current.shift)
            //{
            //    EditorGUIUtility.AddCursorRect(new Rect(Vector2.zero, GetWindow<SceneView>().maxSize), MouseCursor.ArrowPlus);
            //}
            //else if (Event.current.control)
            //{
            //    EditorGUIUtility.AddCursorRect(new Rect(Vector2.zero, GetWindow<SceneView>().maxSize), MouseCursor.ArrowMinus);
            //}
            Waypoint waypoint;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (!Event.current.shift && !Event.current.control)
                {
                    var hits = Physics.RaycastAll(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), 1000f, waypointSelectionLayer);
                    foreach (var hit in hits)
                    {
                        if (hit.collider.TryGetComponent(out waypoint))
                        {
                            SetSelection(waypoint);
                            //Event.current.Use();
                            break;
                        }
                    }
                }
                else
                {
                    bool raycast = Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out RaycastHit hit, 1000f, waypointSelectionLayer);
                    if (raycast || Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit, 1000f, waypointLayer))
                    {
                        ColliderData data;
                        connectors.TryGetValue(hit.collider, out data);
                        if (raycast && hit.transform.parent != GetWaypointsParent() && data == null)
                        {
                            Assert.IsTrue(hit.transform.TryGetComponent(out Waypoint w));
                            error = "Cannot change waypoints in sister subsection!";
                            SceneView.RepaintAll();
                        }
                        else
                        {

                            if (Event.current.shift)
                            {
                                bool ok = true;
                                waypoint = Instantiate(prefab, hit.point, Quaternion.identity, GetWaypointsParent());
                                waypoint.Init(waypointSelectionLayerID);
                                if (data == null)
                                {
                                    if (raycast)
                                    {
                                        int newLen = Selection.objects.Length + 1;
                                        var newObjects = new UnityEngine.Object[newLen];
                                        for (int i = 0; i < Selection.objects.Length; i++)
                                        {
                                            newObjects[i] = Selection.objects[i];
                                        }
                                        newObjects[newLen - 1] = hit.transform.gameObject;
                                        Selection.objects = newObjects;

                                        ok = false;
                                        DestroyImmediate(waypoint.gameObject);
                                    }
                                    else if (target.Waypoints.Count != 0)
                                    {
                                        Waypoint parentWaypoint;
                                        if (!(Selection.activeObject is GameObject go) || !go.TryGetComponent(out parentWaypoint))
                                        {
                                            parentWaypoint = target.Waypoints[0];
                                        }
                                        else if (!IsInTargetSubsection(parentWaypoint))
                                        {
                                            ok = false;
                                            DestroyImmediate(waypoint.gameObject);
                                        }
                                        //if (parentWaypoint.Data.PossibleTasks != EWaypointTaskType.All && parentWaypoint.Branches.Count > 0)
                                        //{
                                        //    ok = false;
                                        //    DestroyImmediate(waypoint.gameObject);
                                        //    error = "Cannot add waypoint connected with waypoint which has not all possible tasks available";
                                        //    SceneView.RepaintAll();
                                        //}
                                        //else
                                        {
                                            AddConnection(parentWaypoint, waypoint);
                                        }
                                    }
                                }
                                else
                                {
                                    data.Parent01.Remove(data.Parent02);
                                    AddConnection(data.Parent01, waypoint);
                                    AddConnection(data.Parent02, waypoint);
                                    DestroyConnector(hit.collider);
                                }
                                if (ok)
                                {
                                    target.Waypoints.Add(waypoint);
                                    if (waypoint.Data.AnimType == EWaypointAnimType.Exit)
                                    {
                                        target.Exits.Add(waypoint);
                                    }
                                    SetSelection(waypoint);
                                    EditorUtility.SetDirty(target);
                                }
                                //Event.current.Use();
                            }
                            else
                            {
                                if (data == null)
                                {
                                    if (hit.collider.TryGetComponent(out waypoint))
                                    {
                                        if (!IsInTargetSubsection(waypoint))
                                        {
                                            Repaint();
                                        }
                                        else
                                        {
                                            bool ok = true;
                                            if (target.Waypoints.Count > 1)
                                            {
                                                var otherWaypoint = waypoint == target.Waypoints[0] ? target.Waypoints[1] : target.Waypoints[0];
                                                var set = new HashSet<Waypoint>() { waypoint };
                                                otherWaypoint.GetConnectedPoints(set);
                                                foreach (var branch in waypoint.Branches)
                                                {
                                                    if (!set.Contains(branch))
                                                    {
                                                        ok = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (ok && target.Waypoints.Count > 2)
                                            {
                                                RemoveWaypoint(waypoint);
                                                if (Selection.activeObject == waypoint.gameObject)
                                                {
                                                    SetSelection(target.Waypoints.Count == 0 ? null : target.Waypoints[0]);
                                                }
                                                var branches = new List<Waypoint>(waypoint.Branches);
                                                foreach (var branch in branches)
                                                {
                                                    branch.Remove(waypoint);
                                                }
                                                var list = new List<Collider>();
                                                foreach (var pair in connectors)
                                                {
                                                    if (pair.Value.Parent01 == waypoint || pair.Value.Parent02 == waypoint)
                                                    {
                                                        list.Add(pair.Key);
                                                    }
                                                }
                                                foreach (var connector in list)
                                                {
                                                    DestroyConnector(connector);
                                                }
                                                DestroyImmediate(waypoint.gameObject);
                                                EditorUtility.SetDirty(target);
                                            }
                                            else
                                            {
                                                error = "Cannot remove breakpoint, will break path.";
                                                Repaint();
                                            }
                                        }
                                        //Event.current.Use();
                                    }
                                }
                                else
                                {
                                    data.Parent01.Remove(data.Parent02);

                                    var set = new HashSet<Waypoint>();
                                    data.Parent01.GetConnectedPoints(set);
                                    if (set.Contains(data.Parent02))
                                    {
                                        DestroyConnector(hit.collider);
                                    }
                                    else
                                    {
                                        data.Parent01.Add(data.Parent02);

                                        error = "Cannot break waypoints, will break path.";
                                        Repaint();
                                    }
                                    //Event.current.Use();
                                }

                            }
                        }
                    }
                }
            }
            if (Event.current.keyCode == KeyCode.N && (EditorApplication.timeSinceStartup - duplicateBreakTime) > MinKeyBreakTime)
            {
                duplicateBreakTime = EditorApplication.timeSinceStartup;
                if (Selection.gameObjects.Length == 1 && Selection.activeObject is GameObject go && go.TryGetComponent(out waypoint))
                {
                    if (IsInTargetSubsection(waypoint))
                    {
                        //if (waypoint.Data.PossibleTasks != EWaypointTaskType.All && waypoint.Branches.Count > 0)
                        //{
                        //    error = "Cannot duplicate waypoint which has not all possible tasks available";
                        //    SceneView.RepaintAll();
                        //}
                        //else
                        {
                            var pos = waypoint.transform.position;
                            var camera = SceneView.lastActiveSceneView.camera;
                            if (camera != null)
                            {
                                var forward = camera.transform.forward;
                                forward.y = 0f;
                                if (forward.sqrMagnitude > .15f)
                                {
                                    pos += forward * .4f;
                                }
                                else
                                {
                                    pos.x -= .4f;
                                    pos.z += .4f;
                                }
                            }
                            else
                            {
                                pos.x -= .4f;
                                pos.z += .4f;
                            }

                            Waypoint newWaypoint = Instantiate(prefab, pos, Quaternion.identity, GetWaypointsParent());

                            newWaypoint.Init(waypointSelectionLayerID);
                            newWaypoint.Data = waypoint.Data;
                            switch (newWaypoint.Data.AnimType)
                            {
                                case EWaypointAnimType.Exit:
                                    target.Exits.Add(newWaypoint);
                                    break;
                                case EWaypointAnimType.ActionAnim:
                                    target.AnimWaypoints.Add(newWaypoint);
                                    break;
                                case EWaypointAnimType.BasicAnim:
                                    break;
                            }
                            AddConnection(waypoint, newWaypoint);
                            target.Waypoints.Add(newWaypoint);
                            SetSelection(newWaypoint);
                        }
                    }
                    //Event.current.Use();
                }
            }

            if (Event.current.keyCode == KeyCode.G && (EditorApplication.timeSinceStartup - connectBreakTime) > MinKeyBreakTime)
            {
                connectBreakTime = EditorApplication.timeSinceStartup;
                if (Selection.gameObjects.Length == 2 && Selection.gameObjects[0].TryGetComponent(out Waypoint waypoint01) && Selection.gameObjects[1].TryGetComponent(out Waypoint waypoint02)
                    && IsInTargetSubsection(waypoint01) && IsInTargetSubsection(waypoint02))
                {
                    AddConnection(waypoint01, waypoint02);
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }

    private void OnGUI()
    {
        if (SceneManager.GetActiveScene().name == SceneName && workplace == null)
        {
            Init();
        }
        if (SceneManager.GetActiveScene().name == SceneName)
        {
            baseScrollPosition = EditorGUILayout.BeginScrollView(baseScrollPosition);
            EditorGUILayout.LabelField("Select aircraft carrier");

            EditorGUILayout.BeginHorizontal();
            bool enabled = GUI.enabled;
            for (int i = 0; i < (int)ECarrierType.Count; i++)
            {
                ECarrierType type = (ECarrierType)i;
                string title = type.ToString();
                if (i == selectedCarrierInt)
                {
                    GUI.enabled = false;
                }
                else
                {
                    title = title.ToLower();
                }
                if (GUILayout.Button(title))
                {
                    SelectCarrier(type);
                }
                GUI.enabled = enabled;
            }
            EditorGUILayout.EndHorizontal();

            int pathsCount = 0;
            if (selectedCarrierInt != -1)
            {
                EditorGUILayout.LabelField("Section");
                pathsCount = SectionPaths[selectedCarrierInt].Count;
                if (selectedCarrierInt == (int)ECarrierType.CV9)
                {
                    pathsCount -= 1;
                }
            }

            EditorGUILayout.BeginHorizontal();
            float width = 0f;

            float space = 3.5f;
            float maxWidth = position.width;

            var contentList = new List<GUIContent>();
            var widths = new List<float>();
            int rows = 1;
            for (int i = 0; i < pathsCount; i++)
            {
                var content = new GUIContent(currentSection == i ? SectionPaths[selectedCarrierInt][i].ToUpper() : SectionPaths[selectedCarrierInt][i]);
                contentList.Add(content);
                float buttonWidth = GUI.skin.button.CalcSize(content).x + space;
                widths.Add(buttonWidth);
                width += buttonWidth;
                if (width > maxWidth)
                {
                    width = buttonWidth;
                    rows++;
                }
            }
            width = 0f;

            if (position.height < (210f + rows * 21f))
            {
                maxWidth -= 12f;
            }

            if (selectedCarrierInt == -1)
            {
                pathsCount = 0;
            }
            for (int i = 0; i < pathsCount; i++)
            {
                var content = contentList[i];
                float buttonWidth = widths[i];
                width += buttonWidth;
                if (width > maxWidth)
                {
                    width = buttonWidth;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                if (i == currentSection)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button(content, GUILayout.Width(buttonWidth - space)))
                {
                    currentSection = i;

                    currentSubsection = -1;

                    if (currentSection == bridgeID)
                    {
                        if (currentSection == loadedSection)
                        {
                            currentSubsection = 0;
                        }
                        else
                        {
                            //#warning fix for island-bridge
                            currentSubsection = 0;
                            if (currentSection != loadedSection)
                            {
                                loadedSection = currentSection;
                                targetGO = GameObject.Find("Target");
                                if (targetGO != null)
                                {
                                    DestroyImmediate(targetGO);
                                }
                                targetGO = new GameObject("Target");

                                switch ((ECarrierType)selectedCarrierInt)
                                {
                                    case ECarrierType.CV3:
                                        Instantiate(AssetDatabase.LoadMainAssetAtPath(@"Assets/GameplayAssets/Prefabs/Sections/Island.prefab"), targetGO.transform);
                                        break;
                                    case ECarrierType.CV5:
                                        Instantiate(AssetDatabase.LoadMainAssetAtPath(@"Assets/GameplayAssets/Prefabs/CV5/Island.prefab"), targetGO.transform);
                                        break;
                                    case ECarrierType.CV9:
                                        Instantiate(AssetDatabase.LoadMainAssetAtPath(@"Assets/GameplayAssets/Prefabs/CV9/Island.prefab"), targetGO.transform);
                                        break;
                                }

                                rootPrefab = PrefabUtility.LoadPrefabContents(Path[selectedCarrierInt] + SectionPaths[selectedCarrierInt][currentSection] + ".prefab");
                                rootPrefab.transform.SetParent(targetGO.transform);
                                EditorUtility.UnloadUnusedAssetsImmediate(true);
                            }
                            foreach (var waypoint in rootPrefab.GetComponentsInChildren<Waypoint>())
                            {
                                waypoint.gameObject.layer = waypointSelectionLayerID;
                            }
                            var paths = rootPrefab.GetComponentsInChildren<WorkerPath>(true);
                            target = paths[currentSubsection];

                            ClearColliders();
                            if (target.Waypoints.Count > 0)
                            {
                                Selection.activeObject = target.Waypoints[0].gameObject;

                                foreach (var waypoint in target.Waypoints)
                                {
                                    foreach (var branch in waypoint.Branches)
                                    {
                                        AddConnection(waypoint, branch);
                                    }
                                }
                                Selection.activeObject = targetGO.transform;
                                SceneView.lastActiveSceneView.FrameSelected();
                            }
                            else
                            {
                                Selection.activeObject = null;
                            }

                            error = "";
                        }
                    }

                    else if (currentSection == loadedSection)
                    {
                        var paths = rootPrefab.GetComponentsInChildren<WorkerPath>(true);
                        currentSubsection = paths[0] == target ? 0 : 1;
                    }
                }
                GUI.enabled = enabled;
            }
            EditorGUILayout.EndHorizontal();

            if (currentSection != -1)
            {
                EditorGUILayout.LabelField("Subsection");
                EditorGUILayout.BeginHorizontal();
                int count;
                if (currentSection == deckID)
                {
                    count = subsectionStrings.Length;
                    for (int i = 0; i < count; i++)
                    {
                        subsectionStrings[i] = "Deck_" + (i + 1);
                    }
                }
                else if (currentSection == bridgeID)
                {
                    count = 0;
                }
                else if (currentSection == wreckSectionID)
                {
                    count = 3;
                    subsectionStrings[0] = "Subsection 1";
                    subsectionStrings[1] = "Subsection 2";
                    subsectionStrings[2] = "Subsection 3";
                }
                else
                {
                    count = 2;
                    subsectionStrings[0] = "Subsection 1";
                    subsectionStrings[1] = "Subsection 2";
                    //subsectionStrings[0] = selectedSectionPaths[currentSection].Insert(3, ".1");
                    //subsectionStrings[1] = selectedSectionPaths[currentSection].Insert(3, ".2");
                }
                for (int i = 0; i < count; i++)
                {
                    if (currentSection == loadedSection && i == currentSubsection)
                    {
                        GUI.enabled = false;
                    }
                    if (GUILayout.Button(currentSection == loadedSection && currentSubsection == i ? subsectionStrings[i].ToUpper() : subsectionStrings[i], GUILayout.MaxWidth(position.width / 2f)))
                    {
                        currentSubsection = i;
                        if (currentSection != loadedSection)
                        {
                            loadedSection = currentSection;
                            targetGO = GameObject.Find("Target");
                            if (targetGO != null)
                            {
                                DestroyImmediate(targetGO);
                            }
                            targetGO = new GameObject("Target");

                            if (currentSection == deckID || currentSection == wreckSectionID)
                            {
                                switch ((ECarrierType)selectedCarrierInt)
                                {
                                    case ECarrierType.CV3:
                                        Instantiate(AssetDatabase.LoadMainAssetAtPath("Assets/ArtAssets/Mesh/ships/US/cv3_caly.fbx"), targetGO.transform);
                                        break;
                                    case ECarrierType.CV5:
                                        Instantiate(AssetDatabase.LoadMainAssetAtPath("Assets/ArtAssets/Mesh/ships/US/cv5_yorktown.fbx"), targetGO.transform);
                                        break;
                                    case ECarrierType.CV9:
                                        Instantiate(AssetDatabase.LoadMainAssetAtPath("Assets/ArtAssets/Mesh/ships/US/cv9_essex.fbx"), targetGO.transform);
                                        break;
                                }
                            }
                            else
                            {
                                switch ((ECarrierType)selectedCarrierInt)
                                {
                                    case ECarrierType.CV3:
                                        break;
                                    case ECarrierType.CV5:
                                        Instantiate(AssetDatabase.LoadMainAssetAtPath("Assets/ArtAssets/Mesh/cv5_rooms.fbx"), targetGO.transform);
                                        break;
                                    case ECarrierType.CV9:
                                        break;
                                }
                                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(LightPath[selectedCarrierInt]), targetGO.transform);
                            }

                            rootPrefab = PrefabUtility.LoadPrefabContents(Path[selectedCarrierInt] + SectionPaths[selectedCarrierInt][currentSection] + ".prefab");
                            rootPrefab.transform.SetParent(targetGO.transform);
                            EditorUtility.UnloadUnusedAssetsImmediate(true);
                        }
                        foreach (var waypoint in rootPrefab.GetComponentsInChildren<Waypoint>())
                        {
                            waypoint.gameObject.layer = waypointSelectionLayerID;
                        }
                        var paths = rootPrefab.GetComponentsInChildren<WorkerPath>(true);
                        target = paths[currentSubsection];

                        if (currentSection != deckID)
                        {
                            if (!colliderGO.TryGetComponent(out collider))
                            {
                                collider = colliderGO.AddComponent<MeshCollider>();
                            }
                            collider.sharedMesh = target.GetComponent<MeshFilter>().sharedMesh;
                            collider.convex = false;
                        }

                        Selection.activeObject = target;
                        if (currentSection != deckID)
                        {
                            SceneView.lastActiveSceneView.FrameSelected();
                        }

                        ClearColliders();
                        if (target.Waypoints.Count > 0)
                        {
                            Selection.activeObject = target.Waypoints[0].gameObject;

                            foreach (var waypoint in target.Waypoints)
                            {
                                foreach (var branch in waypoint.Branches)
                                {
                                    AddConnection(waypoint, branch);
                                }
                            }
                            if (currentSection == deckID)
                            {
                                SceneView.lastActiveSceneView.FrameSelected();
                            }
                        }
                        else
                        {
                            Selection.activeObject = null;
                        }

                        error = "";
                    }
                    GUI.enabled = enabled;
                }
                EditorGUILayout.EndHorizontal();

                if (currentSubsection != -1 && GUILayout.Button("Save changes"))
                {
                    if (loadedSection != deckID && loadedSection != bridgeID)
                    {
                        var subsections = rootPrefab.GetComponentsInChildren<SubSectionRoom>();
                        foreach (var subsection in subsections)
                        {
                            foreach (var segment in subsection.Segments)
                            {
                                segment.Collider.convex = true;
                            }

                            var path = subsection.GetComponent<WorkerPath>();
                            foreach (var node in path.Waypoints)
                            {
                                if (node.Data.AnimType != EWaypointAnimType.BasicAnim && (node.Data.AnimType != EWaypointAnimType.ActionAnim || node.Data.PossibleTasks != EWaypointTaskType.Normal))
                                {
                                    if (node.Data.OverrideSegment != null)
                                    {
                                        node.Data.Segment = node.Data.OverrideSegment;
                                        continue;
                                    }

                                    var colliders = Physics.OverlapSphere(node.transform.position, .15f, SegmentLayer);
                                    if (colliders == null || colliders.Length == 0)
                                    {
                                        var text = "Subsection ";
                                        if (subsections[0] == subsection)
                                        {
                                            text += "1";
                                        }
                                        else if (subsections.Length == 2 || subsections[2] == subsection)
                                        {
                                            text += "2";
                                        }
                                        else
                                        {
                                            text += "3";
                                        }
                                        text += ", node (";
                                        text += node.transform.position.ToString("F3");
                                        text += "), ";
                                        text += (path.Waypoints.IndexOf(node) + 1);
                                        text += ", is outside segment";

                                        Debug.LogError(text, node);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        foreach (var collider in colliders)
                                        {
                                            if (collider.TryGetComponent(out SectionSegment segment))
                                            {
                                                if (found)
                                                {
                                                    var text = "Subsection ";
                                                    text += subsections[0] == subsection ? "1" : "2";
                                                    text += ", node (";
                                                    text += node.transform.position.ToString("F3");
                                                    text += "), ";
                                                    text += (path.Waypoints.IndexOf(node) + 1);
                                                    text += ", is in multiple segments";

                                                    Debug.LogError(text, node);
                                                    break;
                                                }
                                                found = true;
                                                node.Data.Segment = segment;
                                            }
                                        }
                                        if (!found)
                                        {
                                            var text = "Subsection ";
                                            text += subsections[0] == subsection ? "1" : "2";
                                            text += ", node (";
                                            text += node.transform.position.ToString("F3");
                                            text += "), ";
                                            text += (path.Waypoints.IndexOf(node) + 1);
                                            text += ", is outside segment";

                                            Debug.LogError(text, node);
                                        }
                                    }
                                    EditorUtility.SetDirty(node);
                                }
                            }
                            foreach (var segment in subsection.Segments)
                            {
                                segment.Collider.convex = false;
                            }
                        }
                    }
                    PrefabUtility.SaveAsPrefabAsset(rootPrefab, Path[selectedCarrierInt] + SectionPaths[selectedCarrierInt][loadedSection] + ".prefab");
                }
                if (currentSection != loadedSection && loadedSection != -1)
                {
                    EditorGUILayout.LabelField("Save changes first!!");
                }
                if (false && currentSubsection != -1 && loadedSection != bridgeID)
                {
                    EditorGUILayout.BeginHorizontal();
                    var content = new GUIContent("LOC to load");
                    EditorGUILayout.LabelField(content, GUILayout.Width(GUI.skin.label.CalcSize(content).x));
                    target.LOC = (GameObject)EditorGUILayout.ObjectField(target.LOC, typeof(GameObject), false);
                    if (GUILayout.Button("CLEAR and load from LOC"))
                    {
                        Selection.activeObject = null;
                        ClearColliders();
                        foreach (var waypoint in target.Waypoints)
                        {
                            DestroyImmediate(waypoint.gameObject, true);
                        }
                        ClearWaypoints();

                        foreach (Transform child in target.LOC.transform)
                        {
                            if (child.name.StartsWith(ExitNodeName))
                            {
                                if (child.childCount == 0)
                                {
                                    Debug.LogError("Exit doesn't have start point");
                                    return;
                                }
                                var waypoint = Instantiate(prefab, child.position, Quaternion.identity, target.transform);
                                waypoint.Init(waypointSelectionLayerID);
                                waypoint.Data.AnimType = EWaypointAnimType.Exit;
                                target.Waypoints.Add(waypoint);
                                target.Exits.Add(waypoint);
                                break;
                            }
                        }

                        if (target.Waypoints.Count != 1)
                        {
                            Debug.LogError("Couldn't found exit waypoint");
                            return;
                        }

                        foreach (Transform child in target.LOC.transform)
                        {
                            if (!child.name.StartsWith(ExitNodeName))
                            {
                                bool shouldContinue = false;
                                var c = child;
                                while (!c.name.StartsWith(NodeStartName))
                                {
                                    if (c.childCount == 0)
                                    {
                                        shouldContinue = true;
                                        break;
                                    }
                                    c = c.GetChild(0);
                                }
                                if (shouldContinue)
                                {
                                    continue;
                                }
                                if (c.childCount == 0)
                                {
                                    Debug.LogError("Point " + c.name + " doesn't have second point");
                                    continue;
                                }
                                var waypoint = Instantiate(prefab, c.GetChild(0).position, Quaternion.identity, target.transform);
                                waypoint.Init(waypointSelectionLayerID);
                                waypoint.Data.IsLocked = true;
                                waypoint.Data.AnimType = EWaypointAnimType.BasicAnim;
                                waypoint.Data.AnimName = "Walk";
                                waypoint.Data.AnimID = animMan.AnimData.WalkID;

                                target.Waypoints.Add(waypoint);
                                AddConnection(target.Waypoints[0], waypoint);

                                var newWaypoint = Instantiate(prefab, c.position, Quaternion.identity, target.transform);
                                newWaypoint.Init(waypointSelectionLayerID);
                                newWaypoint.Data.PossibleTasks = EWaypointTaskType.Normal;
                                newWaypoint.Data.IsLocked = true;
                                target.Waypoints.Add(newWaypoint);
                                AddConnection(waypoint, newWaypoint);
                                target.AnimWaypoints.Add(newWaypoint);

                                int startIndex = c.name.IndexOf('.') + 1;
                                string anim = c.name.Substring(startIndex, c.name.IndexOf('.', startIndex) - startIndex);
                                int animID = -1;

                                newWaypoint.Data.AnimType = EWaypointAnimType.ActionAnim;
                                newWaypoint.Data.AnimName = anim;
                                try
                                {
                                    animID = animMan.GetAnimID(anim);
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogException(ex);
                                    Debug.LogError("Cannot find animation " + anim);
                                    //return;
                                }
                                newWaypoint.Data.AnimID = animID;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                //if (GUILayout.Button("Remove all bad nodes"))
                //{
                //    var paths = new List<WorkerPath>(FindObjectsOfType<WorkerPath>());
                //    var allWaypoints = new List<Waypoint>(FindObjectsOfType<Waypoint>());

                //    int i = 0;
                //    bool todo = true;
                //    int j = 0;
                //    while (todo)
                //    {
                //        todo = i < 3;
                //        Assert.IsFalse(i++ > 100000);
                //        foreach (var path in paths)
                //        {
                //            int count = RemoveEmptyEntries(path.Waypoints);
                //            j += count;
                //            todo = todo || (count > 0);
                //            var waypoints = path.GetComponentsInChildren<Waypoint>();
                //            foreach (var waypoint in waypoints)
                //            {
                //                waypoint.Branches.RemoveAll((waypointu) => waypointu == null);
                //                if (!path.Waypoints.Contains(waypoint))
                //                {
                //                    j++;
                //                    todo = true;
                //                    DeleteWaypoint(paths, allWaypoints, waypoint);
                //                }
                //            }
                //        }
                //        for (int k = 0; k < allWaypoints.Count; k++)
                //        {
                //            allWaypoints[k].gameObject.layer = waypointSelectionLayerID;
                //            allWaypoints[k].Branches.RemoveAll((waypoint) => waypoint == null);
                //            if (paths.Find((path) => path.Waypoints.Contains(allWaypoints[k])) == null)
                //            {
                //                j++;
                //                todo = true;
                //                DeleteWaypoint(paths, allWaypoints, allWaypoints[k]);
                //                k = -1;
                //            }
                //        }
                //    }
                //    foreach (var path in paths)
                //    {
                //        path.Exits.Clear();
                //        foreach (var waypoint in path.Waypoints)
                //        {
                //            if (waypoint.Data.AnimType == EWaypointAnimType.Exit)
                //            {
                //                path.Exits.Add(waypoint);
                //            }
                //        }
                //        path.AnimWaypoints.Clear();
                //        foreach (var waypoint in path.Waypoints)
                //        {
                //            if (waypoint.Data.AnimType == EWaypointAnimType.ActionAnim)
                //            {
                //                path.AnimWaypoints.Add(waypoint);
                //            }
                //        }
                //    }

                //    ClearColliders();
                //    if (target.Waypoints.Count > 0)
                //    {
                //        foreach (var waypoint in target.Waypoints)
                //        {
                //            foreach (var branch in waypoint.Branches)
                //            {
                //                AddConnection(waypoint, branch);
                //            }
                //        }
                //    }
                //}

                //if (GUILayout.Button("Error check") && loadedSection != BridgeID)
                //{
                //    error = "";
                //    errorScrollPosition = Vector2.zero;
                //    if (loadedSection != -1)
                //    {
                //        error = "";

                //        var sailorText = "SailorInstancesPerSection";
                //        var officerText = "OfficerInstancesPerSection";
                //        int dc = 0;

                //        string text = System.IO.File.ReadAllText(@"Assets\GameplayAssets\Scenes\Production\MainScene.unity");

                //        if (loadedSection == DeckID)
                //        {
                //            sailorText = "SailorInstancesOnDeckPerPath";
                //            officerText = "OfficerInstancesOnDeckPerPath";
                //        }
                //        else
                //        {
                //            dc = GetVariableValue(text, "InstancesPerDC");
                //        }
                //        int sailors = GetVariableValue(text, sailorText);
                //        int officers = GetVariableValue(text, officerText);

                //        if (sailors == -1 || officers == -1 || dc == -1)
                //        {
                //            error = "Error occured :(";
                //        }
                //        else
                //        {
                //            if (loadedSection == DeckID)
                //            {
                //                dc = 0;
                //            }

                //            var builder = new StringBuilder();
                //            var typeList = Utils.ListWaypointsFlags(EWaypointTaskType.Normal);
                //            var paths = FindObjectsOfType<WorkerPath>();
                //            var orderedPaths = new List<WorkerPath>();
                //            var pathsParent = paths[0].transform.parent;
                //            for (int i = 0; i < pathsParent.childCount; i++)
                //            {
                //                var path = pathsParent.GetChild(i).GetComponent<WorkerPath>();

                //                if (path != null)
                //                {
                //                    orderedPaths.Add(path);
                //                }
                //                else
                //                {
                //                    Assert.IsFalse(loadedSection == DeckID);
                //                    Assert.IsTrue(i == 0);
                //                }
                //            }
                //            if (paths.Length != orderedPaths.Count)
                //            {
                //                builder.AppendLine("Some paths are not of this section!?");
                //            }
                //            foreach (var path in paths)
                //            {
                //                if (!orderedPaths.Contains(path))
                //                {
                //                    builder.AppendLine("Some paths are not of this section!?");
                //                    break;
                //                }
                //            }

                //            const string PathText = "Subsection 1:\n";
                //            for (int i = 0; i < orderedPaths.Count; i++)
                //            {
                //                string pathErrors = orderedPaths[i].ValidateWaypoints(sailors, officers, dc);
                //                if (pathErrors.Length > 0)
                //                {
                //                    builder.Append(PathText.Replace("1", (i + 1).ToString()));
                //                    builder.AppendLine(pathErrors);
                //                }
                //            }
                //            error = builder.ToString().Trim();
                //        }
                //    }
                //}
                EditorGUILayout.EndHorizontal();

                if (currentSubsection != -1)
                {
                    if (GUILayout.Button("Check errors"))
                    {
                        error = "";
                        var subsections = FindObjectsOfType<SubSectionRoom>();
                        var builder = new StringBuilder();
                        var list = new List<Dictionary<SectionSegment, int>>();
                        list.Add(new Dictionary<SectionSegment, int>());
                        list.Add(new Dictionary<SectionSegment, int>());
                        list.Add(new Dictionary<SectionSegment, int>());
                        list.Add(new Dictionary<SectionSegment, int>());
                        list.Add(new Dictionary<SectionSegment, int>());
                        list.Add(new Dictionary<SectionSegment, int>());
                        foreach (var subsection in subsections)
                        {
                            if (error.Length > 5)
                            {
                                error += "\n";
                            }
                            error += $"Subsection: {subsection.name}\nWaypoints:";
                            subsection.Path = subsection.GetComponent<WorkerPath>();
                            subsection.Path.ExitsBySegment = new Dictionary<SectionSegment, List<Waypoint>>();
                            if ((true))//selectedCarrierInt != (int)ECarrierType.CV3)
                            {
                                int segments = subsection.Segments.Count;
                                if (segments > subsection.Path.Exits.Count)
                                {
                                    builder.AppendLine($"There should be at least {segments} exits(nof segments), not {subsection.Path.Exits.Count}");
                                }
                                int actionCount = 0;
                                int ffCount = 0;
                                int wpCount = 0;
                                int repairCount = 0;
                                int rescueCount = 0;
                                int injuredCount = 0;
                                int idleCount = 0;
                                int sectionTransCount = 0;
                                int segmentTransCount = 0;
                                foreach (var dict in list)
                                {
                                    dict.Clear();
                                }
                                foreach (var waypoint in subsection.Path.Waypoints)
                                {
                                    if (waypoint.Data.AnimType != EWaypointAnimType.ActionAnim)
                                    {
                                        if (waypoint.Data.PossibleTasks != EWaypointTaskType.All)
                                        {
                                            builder.AppendLine($"Waypoint #{subsection.Path.Waypoints.IndexOf(waypoint) + 1} is not accessible to all");
                                        }
                                    }
                                    if (waypoint.Data.AnimType != EWaypointAnimType.BasicAnim && waypoint.Data.Segment == null && waypoint.Data.PossibleTasks != EWaypointTaskType.Normal)
                                    {
                                        builder.AppendLine($"Waypoint #{subsection.Path.Waypoints.IndexOf(waypoint) + 1} has no segment");
                                    }
                                    switch (waypoint.Data.AnimType)
                                    {
                                        case EWaypointAnimType.DCIdle:
                                            idleCount++;
                                            CheckerHelper(list[0], waypoint.Data.Segment);
                                            break;
                                        case EWaypointAnimType.DCSectionTransition:
                                            sectionTransCount++;
                                            break;
                                        case EWaypointAnimType.DCSegmentTransition:
                                            segmentTransCount++;
                                            break;
                                        case EWaypointAnimType.ActionAnim:
                                            switch (waypoint.Data.PossibleTasks)
                                            {
                                                case EWaypointTaskType.Normal:
                                                    actionCount++;
                                                    break;
                                                case EWaypointTaskType.Rescue:
                                                    CheckerHelper(list[1], waypoint.Data.Segment);
                                                    rescueCount++;
                                                    if (waypoint.Data.InjuredWaypoint)
                                                    {
                                                        injuredCount++;
                                                        CheckerHelper(list[2], waypoint.Data.Segment);
                                                    }
                                                    break;
                                                case EWaypointTaskType.Rescue2:
                                                case EWaypointTaskType.Rescue3:
                                                    break;
                                                case EWaypointTaskType.Repair:
                                                    repairCount++;
                                                    CheckerHelper(list[3], waypoint.Data.Segment);
                                                    break;
                                                case EWaypointTaskType.Firefighting:
                                                    ffCount++;
                                                    CheckerHelper(list[4], waypoint.Data.Segment);
                                                    break;
                                                case EWaypointTaskType.Waterpump:
                                                    wpCount++;
                                                    CheckerHelper(list[5], waypoint.Data.Segment);
                                                    break;
                                                case EWaypointTaskType.RepairDoor:
                                                    break;
                                                default:
                                                    builder.AppendLine($"Action waypoint has bad task: {waypoint.transform.GetSiblingIndex()}" + " " + waypoint.name);
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                if (actionCount < 4)
                                {
                                    builder.AppendLine($"There should be at least 4 action nodes, not {actionCount}");
                                }
                                int animCount = segments * 3;
                                if (ffCount < animCount)
                                {
                                    builder.AppendLine($"There should be at least {animCount} firefight nodes(3 per segment), not {ffCount}");
                                }
                                else
                                {
                                    LogDict(builder, list[4], segments, "firefight", true);
                                }
                                if (wpCount < animCount)
                                {
                                    builder.AppendLine($"There should be at least {animCount} waterpump nodes(3 per segment), not {wpCount}");
                                }
                                else
                                {
                                    LogDict(builder, list[5], segments, "waterpump", true);
                                }
                                if (repairCount < animCount)
                                {
                                    builder.AppendLine($"There should be at least {animCount} repair nodes(3 per segment), not {repairCount}");
                                }
                                else
                                {
                                    LogDict(builder, list[3], segments, "repair", true);
                                }
                                if (rescueCount != animCount)
                                {
                                    builder.AppendLine($"There should be {animCount} rescue nodes(3 per segment), not {rescueCount}");
                                }
                                else
                                {
                                    LogDict(builder, list[1], segments, "rescue", false);
                                }
                                if (injuredCount != segments)
                                {
                                    builder.AppendLine($"There should be {segments} rescue nodes with ticked injured waypoint(1 per segment), not {injuredCount}");
                                }
                                else
                                {
                                    LogDict(builder, list[2], segments, "injured", false, 1);
                                }
                                if (idleCount != animCount)
                                {
                                    builder.AppendLine($"There should be {animCount} idle nodes(3 per segment), not {idleCount}");
                                }
                                else
                                {
                                    LogDict(builder, list[0], segments, "idle", false);
                                }
                                if (sectionTransCount == 0)
                                {
                                    builder.AppendLine($"There is no section transition nodes, shouldn't you add some?");
                                }
                                if (segmentTransCount == 0)
                                {
                                    builder.AppendLine($"There is no segment transition nodes, shouldn't you add some?");
                                }
                                else if ((segmentTransCount % 2) == 1)
                                {
                                    builder.AppendLine($"There should be even count of segment transition nodes");
                                }

                                if (builder.Length < 5)
                                {
                                    error += " ok";
                                }
                                else
                                {
                                    error += "\n";
                                    error += builder.ToString();
                                }
                                builder.Clear();
                                error += "\n-----------------------------\nsection:";
                                var section = SceneUtils.FindObjectOfType<SectionRoom>();
                                if (section.Category == ESectionCategory.None)
                                {
                                    builder.AppendLine("Section should probably have specified category");
                                }
                                if (section.Hover == null)
                                {
                                    builder.AppendLine("Section doesn't have assigned hover");
                                }

                                if (builder.Length < 5)
                                {
                                    error += " ok";
                                }
                                else
                                {
                                    error += "\n";
                                    error += builder.ToString();
                                }
                                builder.Clear();

                                error += "\n-----------------------------\nrefs and script settings:";
                                if (subsection.TryGetComponent(out MeshFilter filter))
                                {
                                    if (filter.sharedMesh == null)
                                    {
                                        builder.AppendLine("Subsection mesh filter doesn't have merged mesh assigned");
                                    }
                                    else if (subsection.RoomMeshes.Count != 2 || subsection.RoomMeshes[0] == null || subsection.RoomMeshes[1] == null)
                                    {
                                        builder.AppendLine("Subsection doesn't have merged mesh and/or merged destroyed mesh assigned");
                                    }
                                }
                                else
                                {
                                    builder.AppendLine("Subsection don't have mesh filter component attached!");
                                }
                                if (subsection.TryGetComponent(out MeshRenderer renderer))
                                {
                                    if (renderer.sharedMaterial == null || !renderer.sharedMaterial.name.StartsWith("MasterMat"))
                                    {
                                        builder.AppendLine("Subsection mesh renderer doesn't have MasterMaterial assigned!");
                                    }
                                }
                                else
                                {
                                    builder.AppendLine("Subsection doesn't have mesh renderer component attached!");
                                }
                                if (segments == 0)
                                {
                                    builder.AppendLine("Subsection doesn't have any segment");
                                }
                                if (subsection.NeighbourInSection != null)
                                {
                                    builder.AppendLine("Subsection shouldn't have Neighbour In Section assigned");
                                }
                                if (subsection.DestructionEffect == null)
                                {
                                    builder.AppendLine("Subsection doesn't have Destruction Effect assigned");
                                }
                                if (subsection.WaterEffect != null)
                                {
                                    builder.AppendLine("Subsection shouldn't have Water Effect assigned");
                                }
                                if (string.IsNullOrWhiteSpace(subsection.Title) || subsection.Title.Trim() == "name")
                                {
                                    builder.AppendLine("Subsection doesn't have Title tooltip");
                                }
                                if (string.IsNullOrWhiteSpace(subsection.Destroyed) || subsection.Destroyed.Trim() == "destroyed effect")
                                {
                                    builder.AppendLine("Subsection doesn't have Destroyed tooltip");
                                }
                                if (string.IsNullOrWhiteSpace(subsection.NotDestroyed) || subsection.NotDestroyed.Trim() == "not destroyed effect")
                                {
                                    builder.AppendLine("Subsection doesn't have Not Destroyed tooltip");
                                }
                                foreach (var segment in subsection.Segments)
                                {
                                    if (!segment.TryGetComponent(out Neighbours _))
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have neighbours script attached");
                                    }
                                    if (segment.Collider == null || segment.Collider.sharedMesh == null || !segment.TryGetComponent(out filter) || filter.sharedMesh == null)
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have simple mesh attached");
                                    }
                                    if (segment.Selection == null)
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have Selection assigned");
                                    }
                                    if (segment.Icons == null)
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have Icons assigned");
                                    }
                                    else
                                    {
                                        if (!segment.Icons.gameObject.activeSelf)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons are turned off");
                                        }
                                        if (segment.Icons.Destruction == null)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons doesn't have Destruction assigned");
                                        }
                                        else if (!segment.Icons.Destruction.gameObject.activeSelf)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons's Destruction is turned off");
                                        }
                                        if (segment.Icons.Water == null)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons doesn't have Water assigned");
                                        }
                                        else if (!segment.Icons.Water.gameObject.activeSelf)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons's Water is turned off");
                                        }
                                        if (segment.Icons.Fire == null)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons doesn't have Fire assigned");
                                        }
                                        else if (!segment.Icons.Fire.gameObject.activeSelf)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons's Fire is turned off");
                                        }
                                        if (segment.Icons.Break == null)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons doesn't have Break assigned");
                                        }
                                        else if (!segment.Icons.Break.gameObject.activeSelf)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s Icons's Break is turned off");
                                        }
                                    }
                                    if (segment.FireEffect == null)
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have Fire Effect assigned");
                                    }
                                    if (segment.DamageEffect == null)
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have Damage Effect assigned");
                                    }
                                    if (segment.InjuredIcon == null)
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have Injured Icon assigned");
                                    }
                                    else
                                    {
                                        if (!segment.InjuredIcon.gameObject.activeSelf)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s InjuredIcon is turned off");
                                        }
                                        if (segment.InjuredIcon.TryGetComponent(out InjuredButton button))
                                        {
                                            if (button.ButtonTrans == null)
                                            {
                                                builder.AppendLine($"Segment {segment.name}'s InjuredIcon's InjuredButton doesn't have Button Trans assigned");
                                            }
                                            if (button.Highlight == null)
                                            {
                                                builder.AppendLine($"Segment {segment.name}'s InjuredIcon's InjuredButton doesn't have Highlight assigned");
                                            }
                                            if (button.Segment == null || button.Segment != segment)
                                            {
                                                builder.AppendLine($"Segment {segment.name}'s InjuredIcon's InjuredButton has bad segment assigned");
                                            }
                                        }
                                        else if (!button.gameObject.activeSelf)
                                        {
                                            builder.AppendLine($"Segment {segment.name}'s InjuredIcon's InjuredButton is turned off");
                                        }
                                    }
                                    if (segment.PumpingDCIcon == null)
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have Pumping DC Icon assigned");
                                    }
                                    if (segment.RepairingDCIcon == null)
                                    {
                                        builder.AppendLine($"Segment {segment.name} doesn't have Repairing DC Icon assigned");
                                    }
                                }

                                if (builder.Length < 5)
                                {
                                    error += " ok";
                                }
                                else
                                {
                                    error += "\n";
                                    error += builder.ToString();
                                }
                                builder.Clear();
                                continue;
                            }
                            else
                            {
                                throw new Exception("not done for cv3");
                            }
                            foreach (var exit in subsection.Path.Exits)
                            {
                                if (exit.Data.Segment == null)
                                {
                                    error += ("exit " + (subsection.Path.Waypoints.IndexOf(exit) + 1) + " (" + exit.transform.position.ToString("F3") + ") has no segment assigned");
                                    error += "\n";
                                    continue;
                                }
                                if (!subsection.Path.ExitsBySegment.TryGetValue(exit.Data.Segment, out List<Waypoint> exitList))
                                {
                                    exitList = new List<Waypoint>();
                                    subsection.Path.ExitsBySegment[exit.Data.Segment] = exitList;
                                }
                                exitList.Add(exit);
                            }

                            subsection.Path.SetupWaypoints(builder);
                            error += builder.ToString();
                            builder.Clear();
                            foreach (var segment in subsection.Segments)
                            {
                                if (!subsection.Path.ExitsBySegment.ContainsKey(segment))
                                {
                                    error += ("segment " + segment.name + " has no exits");
                                    error += "\n";
                                }
                                if (!subsection.Path.ActionsBySegments.ContainsKey(segment))
                                {
                                    error += ("segment " + segment.name + " has no action waypoints whatsoever");
                                    error += "\n";
                                }
                                else
                                {
                                    Check(subsection.Path, segment, EWaypointTaskType.Firefighting);
                                    Check(subsection.Path, segment, EWaypointTaskType.Repair);
                                    Check(subsection.Path, segment, EWaypointTaskType.Rescue);
                                    //uncheck
                                    //Check(subsection.Path, segment, EWaypointTaskType.Rescue2);
                                    //Check(subsection.Path, segment, EWaypointTaskType.Rescue3);
                                    if (segment.CanPumpWater)
                                    {
                                        Check(subsection.Path, segment, EWaypointTaskType.Waterpump);
                                    }

                                    //foreach (var data in segment.Neighbours)
                                    //{
                                    //    if (data.Door != null)
                                    //    {
                                    //        data.Door.CheckDoor(builder, segment);
                                    //    }
                                    //}
                                }
                            }
                            error += builder.ToString();
                            builder.Clear();
                        }

                        error += "\n-----------------------------\nsegment groups:";
                        var set = new HashSet<SectionSegment>();
                        foreach (var group in SceneUtils.FindObjectsOfType<SectionSegmentGroup>())
                        {
                            foreach (var segment in group.Group)
                            {
                                if (!set.Add(segment))
                                {
                                    builder.AppendLine($"Segment {segment.name} is inside more than 1 Section Segment Group!");
                                }
                            }
                        }
                        var allSegments = SceneUtils.FindObjectsOfType<SectionSegment>();
                        if (set.Count != allSegments.Count)
                        {
                            foreach (var segment in allSegments)
                            {
                                if (set.Add(segment))
                                {
                                    builder.AppendLine($"Segment {segment.name} has no Section Segment Group!");
                                }
                            }
                        }
                        if (builder.Length < 5)
                        {
                            error += " ok";
                        }
                        else
                        {
                            error += "\n";
                            error += builder.ToString();
                        }
                        builder.Clear();
                    }


                    GUI.enabled = false;
                    if (GUILayout.Button("Dirt fix"))
                    {
                        var paths = FindObjectsOfType<WorkerPath>();
                        foreach (var path in paths)
                        {
                            foreach (var waypoint in path.Waypoints)
                            {
                                if (waypoint.Data.AnimType >= EWaypointAnimType.DCIdle)
                                {
                                    waypoint.Data.PossibleTasks = EWaypointTaskType.All;
                                }
                            }
                        }
                    }
                    GUI.enabled = enabled;

                    if (GUILayout.Button("Snap waypoints"))
                    {
                        foreach (var subsection in rootPrefab.GetComponentsInChildren<SubSectionRoom>())
                        {
                            try
                            {
                                string meshName = subsection.GetComponent<MeshFilter>().sharedMesh.name;
                                if (meshName.EndsWith(" Instance"))
                                {
                                    meshName = meshName.Substring(0, meshName.Length - 9);
                                }

                                var p = System.IO.Directory.GetFiles("./", meshName + ".fbx", System.IO.SearchOption.AllDirectories)[0];
                                var objects = AssetDatabase.LoadAllAssetsAtPath(p.Substring(2));
                                var meshes = new HashSet<MeshFilter>();
                                var meshesData = new List<MeshData>();
                                foreach (var obj in objects)
                                {
                                    if (obj is MeshFilter filter)
                                    {
                                        Assert.IsTrue(meshes.Add(filter));
                                        meshesData.Add(new MeshData(filter));
                                    }
                                }

                                foreach (var waypoint in subsection.Path.Waypoints)
                                {
                                    var trans = waypoint.transform;
                                    var point = Vector3.zero;
                                    float dist = float.PositiveInfinity;

                                    foreach (var data in meshesData)
                                    {
                                        var localPosition = data.Transform.InverseTransformPoint(trans.position);
                                        for (int i = 0; i < data.Triangles.Length;)
                                        {
                                            var list = new List<Vector3>();
                                            list.Add(data.Vertices[data.Triangles[i++]]);
                                            list.Add(data.Vertices[data.Triangles[i++]]);
                                            list.Add(data.Vertices[data.Triangles[i++]]);

                                            var center = (list[0] + list[1] + list[2]) / 3f;
                                            float newDist = Vector3.SqrMagnitude(localPosition - center);
                                            if (float.IsNaN(newDist))
                                            {
                                                throw new Exception("NaN");
                                            }
                                            var normal = Vector3.Cross(list[1] - list[0], list[2] - list[0]).normalized;
                                            if (normal.y < .9f)
                                            {
                                                continue;
                                            }
                                            var project = center + Vector3.ProjectOnPlane(localPosition - center, normal);

                                            for (int j = 0; j < 3; j++)
                                            {
                                                var d = project - list[j];
                                                var v = list[(j + 1) % 3] - list[j];

                                                var sn = Vector3.Cross(v, normal);
                                                if (Vector3.Dot(d, sn) > 0f)
                                                {
                                                    float l = v.magnitude;
                                                    v /= l;
                                                    list.Add(list[j] + Mathf.Clamp(Vector3.Dot(d, v), 0f, l) * v);
                                                }
                                            }

                                            list.RemoveAt(2);
                                            list.RemoveAt(1);
                                            list.RemoveAt(0);

                                            while (list.Count > 1)
                                            {
                                                if (Vector3.SqrMagnitude(project - list[0]) < Vector3.SqrMagnitude(project - list[1]))
                                                {
                                                    list.RemoveAt(1);
                                                }
                                                else
                                                {
                                                    list.RemoveAt(0);
                                                }
                                            }

                                            var newPoint = list.Count > 0 ? list[0] : project;
                                            newDist = Vector3.SqrMagnitude(localPosition - newPoint);
                                            if (newDist < dist)
                                            {
                                                dist = newDist;
                                                point = data.Transform.TransformPoint(newPoint);
                                            }
                                        }
                                    }
                                    trans.position = point;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }

                    if (GUILayout.Button("Check integrity"))
                    {
                        var set = new HashSet<Waypoint>();
                        var set2 = new HashSet<Waypoint>();
                        var set3 = new HashSet<Waypoint>();
                        foreach (var path in rootPrefab.GetComponentsInChildren<WorkerPath>())
                        {
                            set.Clear();
                            foreach (var waypoint in path.Waypoints)
                            {
                                if (waypoint == null)
                                {
                                    Debug.LogError("Null waypoint");
                                }
                                else if (!set.Add(waypoint))
                                {
                                    Debug.LogError("Duplicated waypoint, " + waypoint.name + ", " + (path.Waypoints.IndexOf(waypoint) + 1) + ";" + (path.Waypoints.LastIndexOf(waypoint) + 1), path);
                                }
                            }
                            set2.Clear();
                            foreach (var waypoint in set)
                            {
                                set2.Add(waypoint);
                            }

                            set3.Clear();
                            Crawl(path.Waypoints[0], set, set2, set3);
                            var waypoints = path.GetComponentsInChildren<Waypoint>(true);
                            if (set2.Count != waypoints.Length)
                            {
                                foreach (var waypoint in waypoints)
                                {
                                    if (set2.Add(waypoint))
                                    {
                                        Debug.LogError("waypoint outside waypoint list", waypoint);
                                    }
                                }
                            }
                        }
                    }
                }

                if (error.Length > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    var errorContent = new GUIContent(error);
                    errorScrollPosition = EditorGUILayout.BeginScrollView(errorScrollPosition, GUILayout.MinHeight(55f), GUILayout.MaxHeight(EditorStyles.textArea.CalcSize(errorContent).y + 2000f));
                    GUI.enabled = false;
                    EditorGUILayout.LabelField(errorContent, EditorStyles.textArea, GUILayout.MaxWidth(position.width / 2f));
                    GUI.enabled = enabled;
                    EditorGUILayout.EndScrollView();
                    if (GUILayout.Button("Clear", GUILayout.MaxWidth(position.width / 2f)))
                    {
                        error = "";
                        errorScrollPosition = Vector2.zero;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            //if (GUILayout.Button("Reset") && target != null)
            //{
            //    var trans = target.transform;
            //    var list = new List<GameObject>();
            //    for(int i=0;i<trans.childCount;i++)
            //    {
            //        list.Add(trans.GetChild(i).gameObject);
            //    }
            //    foreach (var child in list)
            //    {
            //        DestroyImmediate(child);
            //    }
            //    waypoints.Clear();
            //    ClearColliders();
            //}
        }
        else
        {
            if (GUILayout.Button("Go to Waypoint scene") && UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/GameplayAssets/Scenes/Editors/Waypoints.unity");
            }
        }
    }

    private void CheckerHelper(Dictionary<SectionSegment, int> dict, SectionSegment segment)
    {
        if (segment == null)
        {
            Debug.LogError("null segment");
            return;
        }
        if (!dict.TryGetValue(segment, out int value))
        {
            dict[segment] = 0;
            value = 0;
        }
        dict[segment] = value + 1;
    }

    private void LogDict(StringBuilder builder, Dictionary<SectionSegment, int> dict, int count, string text, bool less, int nodeCount = 3)
    {
        if (dict.Count != count)
        {
            builder.AppendLine($"There is no {text} nodes for some segments! {dict.Count} != {count}");
        }
        foreach (var pair in dict)
        {
            if (less ? (pair.Value < nodeCount) : (pair.Value != nodeCount))
            {
                builder.AppendLine($"There should be at least {nodeCount} {text} nodes, not {pair.Value} in {pair.Key.name} segment");
            }
        }
    }

    private void SetSelection(Waypoint current)
    {
        Selection.activeObject = current == null ? null : current.gameObject;
    }

    private void AddConnection(Waypoint parent01, Waypoint parent02)
    {
        parent01.Add(parent02);
        EditorUtility.SetDirty(parent01);
        EditorUtility.SetDirty(parent02);
        foreach (var pair in connectors)
        {
            if ((pair.Value.Parent01 == parent01 && pair.Value.Parent02 == parent02) || (pair.Value.Parent01 == parent02 && pair.Value.Parent02 == parent01))
            {
                return;
            }
        }

        var go = new GameObject("Connector");
        go.layer = waypointSelectionLayerID;
        go.transform.SetParent(helpers.transform);
        var connector = go.AddComponent<BoxCollider>();
        var data = connectors[connector] = new ColliderData(parent01, parent02, connector);
        data.CalculatePosition();
        parent01.OnPositionChanged += data.CalculatePosition;
        parent02.OnPositionChanged += data.CalculatePosition;
    }

    private void DestroyConnector(Collider collider)
    {
        if (connectors.TryGetValue(collider, out ColliderData data))
        {
            data.Parent01.OnPositionChanged -= data.CalculatePosition;
            data.Parent02.OnPositionChanged -= data.CalculatePosition;
            connectors.Remove(collider);
        }
        DestroyImmediate(collider.gameObject);
        EditorUtility.SetDirty(data.Parent01);
        EditorUtility.SetDirty(data.Parent02);
    }

    private void RemoveWaypoint(Waypoint waypoint)
    {
        target.Waypoints.Remove(waypoint);
        target.Exits.Remove(waypoint);
        target.AnimWaypoints.Remove(waypoint);
    }

    private void ClearWaypoints()
    {
        target.Waypoints.Clear();
        target.Exits.Clear();
        target.AnimWaypoints.Clear();
    }

    private void ClearColliders()
    {
        foreach (var value in connectors.Values)
        {
            value.Parent01.OnPositionChanged -= value.CalculatePosition;
            value.Parent02.OnPositionChanged -= value.CalculatePosition;
        }
        connectors.Clear();
        if (helpers != null)
        {
            DestroyImmediate(helpers);
        }
        helpers = new GameObject("Helpers");
        helpers.transform.SetParent(workplace.transform);
    }

    private bool IsInTargetSubsection(Waypoint waypoint)
    {
        if (target.Waypoints.IndexOf(waypoint) == -1)
        {
            error = "Cannot change waypoints in sister subsection";
            SceneView.RepaintAll();
            return false;
        }
        return true;
    }

    private int RemoveEmptyEntries(List<Waypoint> waypoints)
    {
        return waypoints.RemoveAll((waypoint) => waypoint == null);
        //int z = 0;
        //int j = -1;
        //for (int i = 0; i < waypoints.Count;)
        //{
        //    Assert.IsFalse(z++ > 100000);
        //    if (j != -1)
        //    {
        //        if (j >= waypoints.Count)
        //        {
        //            break;
        //        }
        //        if (waypoints[j] != null)
        //        {
        //            waypoints[i] = waypoints[j];
        //            i++;
        //        }
        //        j++;
        //    }
        //    else if (waypoints[i] == null)
        //    {
        //        j = i + 1;
        //    }
        //    else
        //    {
        //        i++;
        //    }
        //}
        //while (waypoints.Count > 0 && waypoints[waypoints.Count - 1] == null)
        //{
        //    waypoints.RemoveAt(waypoints.Count - 1);
        //}
    }

    private void DeleteWaypoint(List<WorkerPath> paths, List<Waypoint> waypoints, Waypoint waypoint)
    {
        foreach (var path in paths)
        {
            path.Waypoints.Remove(waypoint);
        }

        foreach (var waypoint2 in waypoints)
        {
            waypoint2.Branches.Remove(waypoint);
        }
        waypoints.Remove(waypoint);
        DestroyImmediate(waypoint);
    }

    private void Check(WorkerPath path, SectionSegment segment, EWaypointTaskType task)
    {
        var animWaypoints = path.ActionsBySegments[segment];
        if (animWaypoints.ContainsKey(task))
        {
            var taskWaypoints = animWaypoints[task];
            if (taskWaypoints.Count > 3)
            {
                error += ("segment " + segment.name + " has more " + System.Enum.GetName(typeof(EWaypointTaskType), task) + " waypoints than needed, " + taskWaypoints.Count + ">3");
                error += "\n";
            }
            else if (taskWaypoints.Count < 3)
            {
                error += ("segment " + segment.name + " has too low " + System.Enum.GetName(typeof(EWaypointTaskType), task) + " waypoints than needed, 3<" + taskWaypoints.Count);
                error += "\n";
            }

            if ((task & EWaypointTaskType.Rescues) != 0)
            {
                bool found = false;
                foreach (var waypoint in taskWaypoints)
                {
                    if (waypoint.Data.InjuredWaypoint)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    error += ("segment " + segment.name + " has no injured bool ticked for rescue");
                    error += "\n";
                }
            }
        }
        else
        {
            error += ("segment " + segment.name + " doesn't contain " + System.Enum.GetName(typeof(EWaypointTaskType), task));
            error += "\n";
        }
    }

    private void SelectCarrier(ECarrierType type)
    {
        selectedCarrierInt = (int)type;
        currentSection = -1;
        loadedSection = -1;
        wreckSectionID = SectionPaths[selectedCarrierInt].Count - 3;
        deckID = SectionPaths[selectedCarrierInt].Count - 2;
        bridgeID = SectionPaths[selectedCarrierInt].Count - 1;
    }

    private Transform GetWaypointsParent()
    {
        Transform parent;
        if (selectedCarrierInt == (int)ECarrierType.CV3 || currentSection == deckID)
        {
            parent = target.transform;
        }
        else
        {
            parent = target.transform.GetChild(0);
        }

        return parent;
    }

    private void Crawl(Waypoint root, HashSet<Waypoint> set, HashSet<Waypoint> set2, HashSet<Waypoint> set3)
    {
        set3.Clear();
        foreach (var waypoint in root.Branches)
        {
            if (waypoint == root)
            {
                Debug.LogError("waypoint branches to itself", root);
            }
            else if (waypoint == null)
            {
                Debug.LogError("null waypoint in branches", root);
            }
            else
            {
                if (set2.Add(waypoint))
                {
                    Debug.LogError("waypoint outside waypoint list", waypoint);
                }
                if (!set3.Add(waypoint))
                {
                    Debug.LogError("duplicate branch waypoint", waypoint);
                }
            }
        }
        foreach (var waypoint in root.Branches)
        {
            if (waypoint != root && waypoint != null && set.Remove(waypoint))
            {
                Crawl(waypoint, set, set2, set3);
            }
        }
    }
}
