using GPUInstancer.CrowdAnimations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
public enum PrefixType
{
    Action = 0,
    Rescue,
    Repair,
    Firefighting,
    Injured,
    RepairDoors,
    Waterpump
}
#endif

[Serializable]
public class AnimationManager : MonoBehaviour, ISerializationCallbackReceiver
{
    public static AnimationManager Instance;

    public GPUICrowdManager GPUICrowdManager;
    //[HideInInspector]
    public AnimData AnimData = new AnimData();

    public float WalkSpeed = 1f;
    public float RunSpeed = 2f;
    public float NormalQuitSpeed = 2f;
    public float InjuredSpeed = 1f;

    public AnimationClip WalkClip;
    public AnimationClip DCWalkClip;
    public AnimationClip FirefightWalkClip;
    public AnimationClip RunClip;

    [NonSerialized]
    public AnimationClip WalkInjuredClip;
    [NonSerialized]
    public AnimationClip RotateClip;
    [NonSerialized]
    public List<AnimationClip> IdleClips;
    [NonSerialized]
    public List<AnimationClip> FrightenClips;
    public AnimationClip PushInWreckClip;
    public AnimationClip PushLoopWreckClip;
    public AnimationClip PushOutWreckClip;
    [NonSerialized]
    public AnimationClip RescuerIdleClip;
    [NonSerialized]
    public AnimationClip RescuerWalkClip;
    [NonSerialized]
    public AnimationClip RescueeWalkClip;
    [NonSerialized]
    public AnimationClip RescuerRotateClip;
    [NonSerialized]
    public AnimationClip RescueeRotateClip;

    public Dictionary<AnimationClip, int> RescueAnims;
    private HashSet<AnimationClip> rescueClips;

#if UNITY_EDITOR
    private const string InStr = "_IN";
    private const string LoopStr = "LOOP";
    private const string LoopStrSmall = "loop";
    private const string OutStr = "_OUT";

    public const string Walk = "Walk";
    private const string Run = "Run";
    private const string WalkInjured = "Walk_injured";
    private const string Turn = "Turn_180";
    private const string PushWreck = "Sailor_Push_";
    private const string Idle = "Idle";
    private const string Frighten = "1Idle_Frightened";

    private const string RescuerIdle = "Idle_Standing";
    private const string RescuerWalk = "Injured_Picker_Walk";
    private const string RescueeWalk = "Injured_Body_Walk";
    private const string RescuerRotate = "Injured_Picker_TURN180";
    private const string RescueeRotate = "Injured_Body_TURN180";
    private const string RescuerAnim = "Rescuer";
    private const string RescueeAnim = "Rescuee";

    private const string DataPath = @"Assets/Data/ScriptData/anims.dat";
    private const string MainSceneName = "MainScene";

    [MenuItem("Tools/Animations/Update animations", false, 201)]
    static void UpdateAnimations()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != MainSceneName)
        {
            Debug.LogError("Update animations only in " + MainSceneName);
            return;
        }
        var animMan = GameObject.Find("Managers").GetComponent<AnimationManager>();

        var animData = new AnimData();
        var groupsDict = new ConcurrentDictionary<string, AnimGroupData>();
        var clipList = (animMan.GPUICrowdManager.prototypeList[0] as GPUICrowdPrototype).animationData.clipDataList;
        for (int i = 0; i < clipList.Count; i++)
        {
            var clipName = clipList[i].animationClip.name;
            Assert.IsFalse(animData.AnimStrings.ContainsKey(clipName), clipName + ", " + i);

            if (clipName == RescuerIdle)
            {
                animData.RescuerIdleID = i;
            }

            if (clipName.EndsWith(InStr))
            {
                GetAnimGroupData(ref clipName, InStr, groupsDict).inID = i;
                Assert.IsFalse(animData.AnimStrings.ContainsKey(clipName));
                animData.AnimStrings[clipName] = i;

                if (clipName.StartsWith(RescueeAnim))
                {
                    animData.RescueeIDs.Add(i);
                }
            }
            else if (clipName.EndsWith(LoopStr) || clipName.EndsWith(LoopStrSmall))
            {
                GetAnimGroupData(ref clipName, "_", groupsDict).loopIDs.Add(i);
            }
            else if (clipName.EndsWith(OutStr))
            {
                GetAnimGroupData(ref clipName, OutStr, groupsDict).outID = i;
            }
            else
            {
                animData.AnimStrings[clipName] = i;

                if (clipName == Run)
                {
                    animData.RunID = i;
                }
                else if (clipName == Walk)
                {
                    animData.WalkID = i;
                }
                else if (clipName == WalkInjured)
                {
                    animData.WalkInjuredID = i;
                }
                else if (clipName == Turn)
                {
                    animData.RotateID = i;
                }
                else if (clipName == RescuerWalk)
                {
                    animData.RescuerWalkID = i;
                }
                else if (clipName == RescueeWalk)
                {
                    animData.RescueeWalkID = i;
                }
                else if (clipName == RescuerRotate)
                {
                    animData.RescuerRotateID = i;
                }
                else if (clipName == RescueeRotate)
                {
                    animData.RescueeRotateID = i;
                }
                else
                {
                    foreach (var prefix in AnimUtils.Prefixes)
                    {
                        Assert.IsFalse(clipName.StartsWith(prefix), clipName);
                    }
                    if (clipName.StartsWith(Frighten))
                    {
                        animData.FrightenIDs.Add(i);
                    }
                    else if (clipName.StartsWith(Idle))
                    {
                        animData.IdleIDs.Add(i);
                    }
                    else if (clipName.StartsWith(RescuerAnim))
                    {
                        animData.RescuerIDs.Add(i);
                    }
                }
            }
        }
        foreach (var group in groupsDict.Values)
        {
            animData.AnimGroups[group.inID] = group;
            Assert.IsFalse(group.loopIDs.Count == 0);
        }
        Assert.IsTrue(animData.RescuerIDs.Count == animData.RescueeIDs.Count);

        animData.PrepareToSerialize();
        BinUtils.SaveBinary(animData, DataPath);
        animMan.AnimData = animData;
        EditorUtility.SetDirty(animMan.gameObject);
        UpdateAnimIDs();
    }

    [MenuItem("Tools/Animations/Update animation IDs", false, 202)]
    private static void UpdateAnimIDs()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != MainSceneName)
        {
            Debug.LogError("Update animation IDs only in " + MainSceneName);
            return;
        }

        var animMan = FindObjectOfType<AnimationManager>();
        var datas = FindObjectsOfType<WorkerPath>();
        var dataObjects = new GameObject[datas.Length];
        for (int i = 0; i < datas.Length; i++)
        {
            dataObjects[i] = datas[i].gameObject;
        }
        Undo.RecordObjects(dataObjects, "Updated animations IDs");

        bool change = false;
        foreach (var data in datas)
        {
            bool dataChange = false;
            foreach (var node in data.Waypoints)
            {
                if (node.Data.AnimType != EWaypointAnimType.Exit)
                {
                    try
                    {
                        int id = animMan.GetAnimID(node.Data.AnimName);
                        if (node.Data.AnimID != id)
                        {
                            node.Data.AnimID = id;
                            EditorUtility.SetDirty(node);
                            dataChange = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        data.AnimWaypoints.Remove(node);
                        node.Data.AnimType = EWaypointAnimType.BasicAnim;
                        node.Data.AnimID = animMan.AnimData.WalkID;

                        Debug.LogException(ex);
                        var logBuilder = new StringBuilder();
                        logBuilder.Append("Waypoints ");
                        logBuilder.Append(data.name);
                        logBuilder.Append("'s waypoint  ");
                        logBuilder.Append(node.transform.position);
                        logBuilder.Append(" from ");
                        var parent = data.transform.parent;
                        while (parent != null)
                        {
                            logBuilder.Append(", ");
                            logBuilder.Append(parent.name);
                            parent = parent.parent;
                        }
                        logBuilder.Append(" can't retrieve updated anim ID for ");
                        logBuilder.Append(node.Data.AnimName);
                        logBuilder.Append(". Will have none");
                        Debug.Log(logBuilder.ToString());

                        dataChange = true;
                        EditorUtility.SetDirty(node);
                    }
                }
                EditorUtility.SetDirty(node);
            }

            if (dataChange)
            {
                PrefabUtility.ApplyPrefabInstance(data.gameObject, InteractionMode.AutomatedAction);
                EditorUtility.SetDirty(data.gameObject);
                change = true;
            }
        }
        if (change)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
        else
        {
            EditorSceneManager.MarkSceneDirty(animMan.gameObject.scene);
        }
    }

    private static AnimGroupData GetAnimGroupData(ref string anim, string postfix, ConcurrentDictionary<string, AnimGroupData> groupsDict)
    {
        bool startsWith = false;
        foreach (var prefix in AnimUtils.Prefixes)
        {
            if (anim.StartsWith(prefix))
            {
                startsWith = true;
                break;
            }
        }

        if (!startsWith)
        {
            startsWith = anim.StartsWith(RescueeAnim) || anim.StartsWith(PushWreck);
        }
        Assert.IsTrue(startsWith, anim);
        anim = anim.Substring(0, anim.LastIndexOf(postfix));
        if (!groupsDict.TryGetValue(anim, out AnimGroupData group))
        {
            group = new AnimGroupData();
            groupsDict[anim] = group;
        }
        if (group.loopIDs == null)
        {
            group.loopIDs = new List<int>();
        }
        return group;
    }
#endif

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        var animList = (GPUICrowdManager.prototypeList[0] as GPUICrowdPrototype).animationData.clipDataList;
        if (WalkClip == null)
        {
            WalkClip = animList[AnimData.WalkID].animationClip;
        }
        WalkInjuredClip = animList[AnimData.WalkInjuredID].animationClip;
        RotateClip = animList[AnimData.RotateID].animationClip;

        FrightenClips = new List<AnimationClip>();
        foreach (int id in AnimData.FrightenIDs)
        {
            FrightenClips.Add(animList[id].animationClip);
        }

        IdleClips = new List<AnimationClip>();
        if (IdleClips.Count == 0)
        {
            foreach (int id in AnimData.IdleIDs)
            {
                IdleClips.Add(animList[id].animationClip);
            }
        }
        if (IdleClips.Count == 0)
        {
            Debug.LogError("No idles");
            IdleClips.Add(WalkClip);
        }

        if (DCWalkClip == null)
        {
            DCWalkClip = WalkClip;
        }

        if (RunClip == null)
        {
            RunClip = WalkClip;
        }

        RescuerIdleClip = animList[AnimData.RescuerIdleID].animationClip;
        RescuerWalkClip = animList[AnimData.RescuerWalkID].animationClip;
        RescueeWalkClip = animList[AnimData.RescueeWalkID].animationClip;
        RescuerRotateClip = animList[AnimData.RescuerRotateID].animationClip;
        RescueeRotateClip = animList[AnimData.RescueeRotateID].animationClip;

        RescueAnims = new Dictionary<AnimationClip, int>();
        for (int i = 0; i < AnimData.RescuerIDs.Count; i++)
        {
            RescueAnims[animList[AnimData.RescuerIDs[i]].animationClip] = AnimData.RescueeIDs[i];
        }
        rescueClips = new HashSet<AnimationClip>();
    }

    public int GetAnimID(string name)
    {
        return AnimData.AnimStrings[name];
    }

    public void RandomRescue(out AnimGroupData rescueeAnims, out AnimationClip rescuerClip)
    {
        if (rescueClips.Count == 0)
        {
            foreach (var clip in RescueAnims.Keys)
            {
                rescueClips.Add(clip);
            }
        }

        rescuerClip = RandomUtils.GetRandom(rescueClips);
        rescueClips.Remove(rescuerClip);

        rescueeAnims = AnimData.AnimGroups[RescueAnims[rescuerClip]];
    }

    public void GetRescueAnim(int index, out AnimGroupData rescueeAnims, out AnimationClip rescuerClip)
    {
        var animList = (GPUICrowdManager.prototypeList[0] as GPUICrowdPrototype).animationData.clipDataList;
        rescuerClip = animList[index].animationClip;
        rescueeAnims = AnimData.AnimGroups[RescueAnims[rescuerClip]];
    }

    public int RescueIndexOf(AnimationClip rescuerClip)
    {
        var animList = (GPUICrowdManager.prototypeList[0] as GPUICrowdPrototype).animationData.clipDataList;
        return AnimData.RescuerIDs.Find((index) => animList[index].animationClip == rescuerClip);
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
#if UNITY_EDITOR
        AnimData = BinUtils.LoadBinary<AnimData>(DataPath);
#endif
        AnimData.RegenerateData();
    }
}
