using GPUInstancer.CrowdAnimations;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class AnimTest : MonoBehaviour
{
    public enum TestType 
    { 
        InOut,
        Loop,
        InLoop3,
        AllOnce,
        GoToInOut,
        GoToAllOnce
    };

    public bool Active = false;
    public TestType AnimType = TestType.InOut;
    public Animator Animator;

    public AnimationClip[] Clips;
    public AnimationClip[] GotoClips;
    public AnimationClip[] AnimClips;
    public AnimationClip[] RotationClips;
    public int CurrentAnim = 0;
    public float AnimTime = 0f;
    public float PathTime = 0f;
    public List<Vector3> Path = null;
    public float Speed;
    public int CurrentPath = -1;

    public float MaxTime = 0f;
    public bool InPlace;

    private void Update()
    {
#if UNITY_EDITOR
        EditorApplication.update -= AnimUpdate;
        EditorApplication.update += AnimUpdate;
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        EditorApplication.update -= AnimUpdate;
#endif
    }

    public void Setup(Transform start, Vector3 dir, AnimationManager animMan, List<GPUIAnimationClipData> animList, WaypointData data, bool rescuee, int rescueAnim, List<Vector3> path)
    {
        Active = false;
        CurrentPath = -1;

        bool rescue = data.AnimType == EWaypointAnimType.ActionAnim && (data.PossibleTasks == EWaypointTaskType.Rescue || data.PossibleTasks == EWaypointTaskType.Rescue2 || data.PossibleTasks == EWaypointTaskType.Rescue3);
        if (!rescuee && rescue)
        {
            Clips = new AnimationClip[1];
            Clips[0] = animList[animMan.AnimData.RescuerIDs[rescueAnim]].animationClip;
        }
        else
        {
            var group = animMan.AnimData.AnimGroups[rescue ? animMan.AnimData.RescueeIDs[rescueAnim] : data.AnimID];
            switch (AnimType)
            {
                case TestType.InOut:
                case TestType.GoToInOut:
                    Clips = new AnimationClip[2];
                    Clips[0] = animList[group.inID].animationClip;
                    Clips[1] = animList[group.outID].animationClip;
                    break;
                case TestType.InLoop3:
                    Clips = new AnimationClip[4];
                    Clips[0] = animList[group.inID].animationClip;
                    for (int i = 0; i < 3; i++)
                    {
                        Clips[i + 1] = animList[group.loopIDs[0]].animationClip;
                    }
                    break;
                case TestType.Loop:
                    Clips = new AnimationClip[group.loopIDs.Count];
                    for (int i = 0; i < group.loopIDs.Count; i++)
                    {
                        Clips[i] = animList[group.loopIDs[i]].animationClip;
                    }
                    break;
                case TestType.AllOnce:
                case TestType.GoToAllOnce:
                    Clips = new AnimationClip[3];
                    Clips[0] = animList[group.inID].animationClip;
                    Clips[1] = animList[group.loopIDs[0]].animationClip;
                    Clips[2] = animList[group.outID].animationClip;
                    break;
            }
        }
        CurrentAnim = 0;
        AnimTime = 0f;
        PathTime = 0f;
        Active = true;

        if (AnimType == TestType.GoToInOut || AnimType == TestType.GoToAllOnce)
        {
            AnimClips = Clips;
            if (RotationClips == null || RotationClips.Length == 0)
            {
                RotationClips = new AnimationClip[1];
                RotationClips[0] = animList[animMan.AnimData.RotateID].animationClip;
            }
            GotoClips = Clips = new AnimationClip[1];
            Clips[0] = animList[animMan.AnimData.RunID].animationClip;

            Path = path;
            Speed = animMan.RunSpeed;

            InPlace = true;
            InitPath();
        }
        else
        {
            transform.parent.position = start.position;
            transform.parent.rotation = Quaternion.Euler(0f, Vector2.SignedAngle(new Vector2(dir.x, dir.z).normalized, Vector2.down), 0f);
        }
    }

    private void AnimUpdate()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPaused)
        {
            return;
        }

        if (Animator == null)
        {
            Animator = GetComponent<Animator>();
        }
        if (Active)
        {
            if (Clips != null && Clips.Length > CurrentAnim && CurrentAnim >= 0 && Clips[CurrentAnim] != null)
            {
                while (Clips[CurrentAnim].length < AnimTime)
                {
                    AnimTime -= Clips[CurrentAnim].length;
                    CurrentAnim++;
                    if (CurrentAnim >= Clips.Length)
                    {
                        CurrentAnim = 0;
                        if (AnimType == TestType.GoToInOut || AnimType == TestType.GoToAllOnce)
                        {
                            if (Clips == AnimClips)
                            {
                                AnimTime = 0f;
                                Clips = RotationClips;

                                AnimUpdate();
                                return;
                            }
                            else if (Clips == RotationClips)
                            {
                                AnimTime = 0f;
                                Clips = GotoClips;

                                CurrentPath = 1;
                                PathTime = 0f;

                                AnimUpdate();
                                return;
                            }
                        }
                    }
                    if (Clips[CurrentAnim] == null)
                    {
                        return;
                    }
                }
                Clips[CurrentAnim].SampleAnimation(gameObject, AnimTime);
            }

            if (CurrentPath != -1 && Path != null && Path.Count > 0)
            {
                transform.parent.position = Vector3.Lerp(Path[CurrentPath - 1], Path[CurrentPath], Mathf.Min(PathTime / MaxTime, 1f));
                if (PathTime >= MaxTime)
                {
                    if (++CurrentPath == Path.Count)
                    {
                        PathTime = 0f;
                        Path.Reverse();
                        InitPath();
                        if (InPlace)
                        {
                            Clips = AnimClips;
                            CurrentAnim = 0;
                            CurrentPath = -1;
                            AnimTime = 0f;
                        }
                    }
                    else
                    {
                        InitPathSegment();
                    }
                }
            }
            AnimTime += Time.deltaTime;
            PathTime += Time.deltaTime;
        }
#endif
    }

    private void InitPath()
    {
        CurrentPath = 1;
        InitPathSegment();
        InPlace = !InPlace;
    }

    private void InitPathSegment()
    {
        var dir = Path[CurrentPath] - Path[CurrentPath - 1];
        MaxTime = dir.magnitude;

        transform.parent.position = Path[CurrentPath - 1];
        transform.parent.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z).normalized);

        PathTime = 0f;
    }
}
