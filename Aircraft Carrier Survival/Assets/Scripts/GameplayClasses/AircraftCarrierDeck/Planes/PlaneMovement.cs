using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;

public class PlaneMovement : MonoBehaviour
{
    public event Action<PlaneMovement> Arrived = delegate { };
    public event Action<PlaneMovement> RotationFinished = delegate { };

    private static readonly string WingsOn = "WingsOn";
    private static readonly string WingsOff = "WingsOff";
    private static readonly string ForceWingsOn = "LoopOn";
    private static readonly string ForceWingsOff = "None";

    public bool HasStaticWings = true;

    public PlaneNode CurrentNode
    {
        get => currentNode;
        private set
        {
            if (!ignore)
            {
                Assert.IsNotNull(Squadron);
            }
            if (CurrentNode != null && CurrentNode.OccupiedBy == Squadron)
            {
                CurrentNode.OccupiedBy = null;
            }
            SetElevator();

            setElevator = CurrentNode;
            currentNode = value;
            if (CurrentNode != null)
            {
                CurrentNode.OccupiedBy = Squadron;
                if (setElevator != null)
                {
                    var deck = AircraftCarrierDeckManager.Instance;
                    if (setElevator.ElevatorDown && CurrentNode.ElevatorUp)
                    {
                        if (!savedData.HasValue && IsRotating())
                        {
                            setElevatorState = 1f;
                            lift = CurrentNode.Lift;
                        }
                        else
                        {
                            deck.SetLiftState(CurrentNode.Lift, 1f, !IntermediateCrewReady());
                        }
                    }
                    else if (setElevator.ElevatorUp && CurrentNode.ElevatorDown)
                    {
                        if (!savedData.HasValue && (IsRotating() || Crew.Find(x => x.IsSwitchingPush) != null))
                        {
                            setElevatorState = 0f;
                            lift = CurrentNode.Lift;
                        }
                        else if (CanLiftDown())
                        {
                            deck.SetLiftState(CurrentNode.Lift, 0f, !IntermediateCrewReady());
                        }
                    }
                }
            }
        }
    }

    public bool CrewIsReady
    {
        get => crewIsReady;
        set => crewIsReady = value;
    }

    public List<PlaneCrewMovement> Crew
    {
        get => crew;
        set => crew = value;
    }

    public float AnimSpeed
    {
        get;
        set;
    } = 1f;

    public float SpeedMultiplier
    {
        get;
        set;
    } = 1f;

    //public EPlaneRotation RotationConstraint
    //{
    //    get;
    //    set;
    //}

    //public EPlaneRotation LaterRotationConstraint
    //{
    //    get;
    //    set;
    //}

    public Quaternion RotationAtDestination
    {
        get;
        set;
    }

    public bool IgnoreOthers
    {
        get;
        set;
    }

    public bool IsPlayingAnimation
    {
        get;
        private set;
    }

    public MaterialChanger MaterialChanger
    {
        get;
        private set;
    }

    public Transform Trans
    {
        get
        {
            if (trans == null)
            {
                trans = transform;
            }

            return trans;
        }
    }

    [NonSerialized]
    public bool SnapRotation;

    [NonSerialized]
    public List<PlaneNode> Path;

    [NonSerialized]
    public PlaneSquadron Squadron;

    [NonSerialized]
    public EPlaneType Type;

    public Action<PlaneMovement> Callback;

    public float Speed;

    public List<Transform> PushFront;
    public List<Transform> PushBottom;

    public bool MoveBackwards;

    public Transform Plane;

    public HashSet<PlaneMovement> Dependencies;

    public HashSet<PlaneMovement> Dependencies2;

    public float Delay;

    [SerializeField]
    private float maxRotationSpeed = 180f;

    [SerializeField]
    private List<PlaneCrewMovement> crew;

    [SerializeField]
    private Animator secondaryAnimator = null;

    [SerializeField]
    private StudioEventEmitter pushSound = null;
    [SerializeField]
    private PlaneSounds startSound = null;
    [SerializeField]
    private PlaneSpeedSound landSound = null;

    [SerializeField]
    private List<float> landingSoundDelay = new List<float> { 7.5f, 11.5f, 16f};

    [SerializeField]
    private PlayableDirector[] flyAnims = null;
    [SerializeField]
    private PlayableDirector[] landAnims = null;

    [SerializeField]
    private Transform container = null;

    [SerializeField]
    private List<Vector3> landingOffset = new List<Vector3>() { new Vector3(3.4564f, 25.568f, 1178.1f), new Vector3(23.067f, 20.585f, 1180.9f), new Vector3(-19.2f, 20.305f, 1171.4f) };

    [SerializeField]
    private float zOffset = 0f;

    private PlaneNode setElevator;

    private PlaneNode currentNode;

    private bool crewIsReady;

    private Vector3 prevPosition;
    private int currentIndex;
    private Vector3 direction;
    private float nodeLengthDone;
    private float nodeLength;

    private float desiredEuler;
    private Quaternion desiredRotation;

    private Transform trans = null;

    private bool arrived;
    private bool rotated;

    private bool start;
    private float timer;
    private float delay;
    private int delayPosition;

    private bool fireCrew;

    private bool pushedSound;
    private bool isInAir = false;

    private float setElevatorState = -1f;
    private int lift;
    private int hideWingCounter = 0;

    private PlanePathSaveData? savedData;
    private bool fly;
    private bool savedWasAnim;

    private bool secondaryAnim;
    private bool secondaryAnimState;

    private bool wait;

    private int landingPosition;

    private bool ignore;

    private void Awake()
    {
        Crew = new List<PlaneCrewMovement>();
        Path = new List<PlaneNode>();
        desiredRotation = Trans.rotation;

        Dependencies = new HashSet<PlaneMovement>();
        Dependencies2 = new HashSet<PlaneMovement>();

        MaterialChanger = GetComponent<MaterialChanger>();

        setElevatorState = -1f;
    }

    private void Update()
    {
        bool active = false;
        foreach (var anim in flyAnims)
        {
            if (anim.gameObject.activeSelf)
            {
                Assert.IsFalse(active);
                active = true;
            }
            UpdateAnim(anim, () => gameObject.SetActive(false));
        }
        foreach (var anim in landAnims)
        {
            if (anim.gameObject.activeSelf)
            {
                Assert.IsFalse(active);
                active = true;
            }
            UpdateAnim(anim, () => Trans.position -= landingOffset[landingPosition]);
        }

        if (start && CrewIsReady && !IsRotating())
        {
            if (fireCrew)
            {
                PlaneCrewMovementManager.Instance.MovePlaneCrewOnPlaneStart(this);
                fireCrew = false;
            }

            timer += Time.deltaTime;
            if (timer > delay)
            {
                start = false;
                isInAir = true;

                if (startSound != null)
                {
                    startSound.PlayEvent();
                }
                Trans.rotation = desiredRotation = Quaternion.identity;
                Trans.position -= new Vector3(0f, 0f, 2f * zOffset);
                Assert.IsFalse(delayPosition >= flyAnims.Length, delayPosition.ToString());
                PlayAnim(flyAnims[delayPosition], delayPosition, Callback);

                AnimCallback();
                return;
            }
        }
        secondaryAnim = false;
    }

    public EPlaneUpdateResult LoadUpdate()
    {
        Assert.IsTrue(savedData.HasValue);
        int loopBreak = 0;
        while (true)
        {
            Assert.IsFalse(loopBreak++ > 100_000);
            if (savedWasAnim)
            {
                return EPlaneUpdateResult.AnimLoad;
            }
            if (start)
            {
                start = false;
                isInAir = true;
                if (fireCrew)
                {
                    PlaneCrewMovementManager.Instance.MovePlaneCrewOnPlaneStart(this);
                }
                Trans.position = new Vector3(0f, 1000f, 0f);
                if (savedData.Value.Anim)
                {
                    savedWasAnim = true;
                    return EPlaneUpdateResult.AnimLoad;
                }
                else
                {
                    return EPlaneUpdateResult.Anim;
                }
            }
            if (IsPlayingAnimation)
            {
                Assert.IsFalse(fly);

                if (secondaryAnimator != null)
                {
                    secondaryAnimator.Play(fly ? ForceWingsOn : ForceWingsOff);
                    secondaryAnim = true;
                    secondaryAnimState = true;
                }
                FinishAnim();
                if (savedData.Value.Anim)
                {
                    savedWasAnim = true;
                    return EPlaneUpdateResult.AnimLoad;
                }
                else
                {
                    return EPlaneUpdateResult.Anim;
                }
            }
            if (Path.Count == 0)
            {
                return EPlaneUpdateResult.None;
            }

            if (LoadArrived() || MyUpdate(.01f))
            {
                return LoadArrived() ? EPlaneUpdateResult.ArrivedLoad : EPlaneUpdateResult.Arrived;
            }
        }
    }

    public void LoadData(PlanePathSaveData data)
    {
        var planeMovementMan = PlaneMovementManager.Instance;
        data.CurrentNode = planeMovementMan.GetNode(data.NodeIndex);
        if (data.Path != null)
        {
            data.CurrentPath = new List<PlaneNode>();
            foreach (var index2 in data.Path)
            {
                data.CurrentPath.Add(planeMovementMan.GetNode(index2));
            }
        }
        savedData = data;
    }

    public bool LoadArrived()
    {
        if (savedData.Value.PathNode == currentIndex && nodeLengthDone >= savedData.Value.NodeLengthDone)
        {
            var savedPath = savedData.Value.CurrentPath;
            if (Path.Count != savedPath.Count)
            {
                return false;
            }
            for (int i = 0; i < Path.Count; i++)
            {
                if (Path[i] != savedPath[i])
                {
                    return false;
                }
            }
            Assert.IsFalse(arrived || rotated);
            return true;
        }
        return false;
    }

    public void LoadTeleport()
    {
        if (CurrentNode != savedData.Value.CurrentNode)
        {
            CurrentNode = savedData.Value.CurrentNode;
        }
        if (fly)
        {
            Trans.position = new Vector3(0f, -1000f, 0f);
        }
        else if (!savedWasAnim)
        {
            Trans.position = savedData.Value.Position;
            Trans.rotation = Quaternion.Euler(0f, savedData.Value.RotationY, 0f);
        }
        Delay = savedData.Value.Delay;
        savedData = null;
        savedWasAnim = false;
        if (secondaryAnim)
        {
            secondaryAnimator.Play(secondaryAnimState ? ForceWingsOn : ForceWingsOff, 0, .999f);
            secondaryAnim = false;
        }
    }

    public void SaveData(ref PlanePathSaveData data, bool path)
    {
        data.RotationY = desiredRotation.eulerAngles.y;
        data.Position = CurrentNode.Position;
        data.PathNode = -1;
        data.Path = null;
        var planeMovementMan = PlaneMovementManager.Instance;
        if (path)
        {
            data.Delay = Delay;
            if (start || (IsPlayingAnimation && !fly))
            {
                data.Anim = true;
            }
            else
            {
                data.Anim = false;
                if (Path.Count > 0)
                {
                    data.Position = Trans.position;
                    data.PathNode = currentIndex;
                    data.NodeLengthDone = nodeLengthDone;

                    data.Path = new List<int>();
                    foreach (var node in Path)
                    {
                        data.Path.Add(planeMovementMan.IndexOf(node));
                    }
                }
            }
        }
        else
        {
            data.Delay = 0f;
        }
        data.NodeIndex = planeMovementMan.IndexOf(CurrentNode);
    }

    public void Fly(Action<PlaneMovement> callback, int position, float delay)
    {
        fly = true;
        start = true;
        fireCrew = true;
        timer = 0f;
        this.delay = delay;
        delayPosition = position;
        this.Callback = callback;

        Rotate(Quaternion.Euler(0f, 180f, 0f));
    }

    public void SetWings(bool state)
    {
        if (secondaryAnimator != null)
        {
            secondaryAnim = true;
            secondaryAnimState = state;
            secondaryAnimator.Play(state ? WingsOn : WingsOff);
        }
    }

    public void HideWingsByCrew()
    {
        if (--hideWingCounter <= 0)
        {
            SetWings(false);
        }
    }

    public void Land(Action<PlaneMovement> callback, int position)
    {
        if (landSound != null)
        {
            StartCoroutine(PlayLandSoundWithDelay(landingSoundDelay[position]));
        }
        if (!isInAir)
        {
            SetWings(true);
        }
        isInAir = true;
        hideWingCounter = 2;

        landingPosition = position;
        Trans.rotation = desiredRotation = Quaternion.identity;
        Trans.position += landingOffset[position];
        container.localPosition = Vector3.zero;

        PlayAnim(landAnims[position], position, (x) =>
        {
            isInAir = false;
            callback(x);
        });

        //var anim = landAnims[position];
        //anim.time = anim.duration - .2d;
    }

    public void Setup(PlaneSquadron squadron, PlaneNode startNode, bool snapRotation)
    {
        Assert.IsTrue(squadron.PlaneType == Type);
        Assert.IsNull(Squadron);
        Squadron = squadron;
        CurrentNode = startNode;
        Trans.position = CurrentNode.Position;
        SnapRotation = snapRotation;
        isInAir = false;
    }

    public void Remove()
    {
        Assert.IsTrue(Path.Count == 0);

        CurrentNode = null;
        Squadron = null;
        Trans.position = new Vector3(0f, -1000f, 0f);
        isInAir = false;
        SetWings(false);
    }

    public void StartMovement()
    {
        Assert.IsFalse(Path.Count == 0);
        Assert.IsTrue(Path[0] == CurrentNode);

        bool needCrew = false;
        foreach (var crew in Crew)
        {
            if ((!crew.ReadyState && crew.CrewPathState != PlaneCrewMovement.EPlaneCrewPathState.AT_PLANE))
            {
                needCrew = true;
                break;
            }
        }
        //check if there is already crew moving to plane!!!!111111
        if (needCrew)
        {
            var planeCrewMan = PlaneCrewMovementManager.Instance;
            switch (PlaneMovementManager.Instance.CurrentMovement)
            {
                case EPlaneDirection.DeckRecoveringToDeckLaunching:
                case EPlaneDirection.HangarToDeckLaunching:
                case EPlaneDirection.DeckRecoveringToHangar:
                case EPlaneDirection.SwapRecovering:
                case EPlaneDirection.SwapFrontRecovering:
                    planeCrewMan.MovePlaneCrewToPlane(this, EPlaneNodeGroup.DeckRecovering);
                    break;
                case EPlaneDirection.DeckLaunchingToDeckRecovering:
                    planeCrewMan.MovePlaneCrewToPlane(this, EPlaneNodeGroup.DeckLaunching);
                    break;
                case EPlaneDirection.HangarToDeckRecovering:
                case EPlaneDirection.DeckLaunchingToAirLaunching:
                case EPlaneDirection.DeckLaunchingToHangar:
                case EPlaneDirection.SwapLaunching:
                case EPlaneDirection.SwapFrontLaunching:
                    //case EPlaneDirection.LandingToDeckRecovering:
                    planeCrewMan.MovePlaneCrewToPlane(this, EPlaneNodeGroup.DeckLaunching);
                    break;
                default:
                    break;
            }
        }
        currentIndex = 0;
        prevPosition = CurrentNode.Position;
        Next();
    }

    public void StartCrewMovementAnimation()
    {
        if (isInAir)
        {
            return;
        }
        foreach (PlaneCrewMovement crew in Crew)
        {
            crew.Push(MoveBackwards);
        }
    }

    public void StopCrewMovementAnimation()
    {
        //return;
        foreach (PlaneCrewMovement crew in Crew)
        {
            crew.Idle();
        }
    }

    public void StopCrewmates()
    {
        PlaneCrewMovementManager.Instance.FreeCrews(this);
    }

    public void SetCrew(bool crewIsReady)
    {
        //return;
        for (int i = 0; i < Crew.Count; i++)
        {
            PlaneCrewMovement crewmate = Crew[i];
            crewmate.SetupCrew(PushFront[i], crewIsReady);
        }
    }

    public void Return()
    {
        Assert.IsFalse(SnapRotation);
        nodeLengthDone = nodeLength - nodeLengthDone;
        direction = -direction;
        LookAt(direction);
        prevPosition = CurrentNode.Position;

        Assert.IsFalse(Path.Count == 0);
        Assert.IsFalse(currentIndex == 0);
        currentIndex = Path.Count - currentIndex;
        Assert.IsFalse(currentIndex < 1);
        Path.Reverse();
        CurrentNode = Path[currentIndex];
    }

    public bool MyUpdate(float time)
    {
        bool changeElevator = false;
        if (!savedData.HasValue)
        {
            CrewIsReady = Squadron != null && CheckCrew();
            if (Path.Count == 0)
            {
                foreach (var crew in Crew)
                {
                    if (crew.Path.Count == 0 && !crew.IsSwitchingPush && !crew.InRandomIdleMode)
                    {
                        crew.Idle();
                    }
                }
            }

            if (!CrewIsReady)
            {
                return false;
            }

            if (Delay > 0f)
            {
                Delay -= time;
                return false;
            }

            if (IsRotating() && !isInAir && Crew.Find(x => x.IsSwitchingPush) == null)
            {
                var euler = Trans.rotation.eulerAngles;
                float direction = GetRotationDirection(euler);
                StartCrewMovementAnimation();

                float oldEuler = euler.y;
                float antinormalDesiredEuler = ((direction * oldEuler) < (direction * desiredEuler)) ? desiredEuler : desiredEuler + direction * 360f;
                euler.y += direction * time * SpeedMultiplier * maxRotationSpeed;
                if ((direction * euler.y) > (direction * antinormalDesiredEuler))
                {
                    Trans.rotation = desiredRotation;
                    //RotationConstraint = EPlaneRotation.Shorter;
                    rotated = true;
                }
                else
                {
                    Trans.rotation = Quaternion.Euler(euler);
                }
            }
            else
            {
                changeElevator = true;
            }
        }
        else
        {
            changeElevator = true;
        }
        if (changeElevator && setElevatorState >= 0f)
        {
            var deck = AircraftCarrierDeckManager.Instance;
            bool ready = IntermediateCrewReady();
            deck.SetLiftState(lift, setElevatorState, !ready);
            if (ready)
            {
                deck.UpdateLift(lift);
            }
            setElevatorState = -1f;
        }
        if ((savedData.HasValue || !IsRotating()) && Path.Count != 0)
        {
            if (!pushedSound && pushSound != null)
            {
                pushSound.Play();
                pushedSound = true;
            }
            if (!savedData.HasValue)
            {
                StartCrewMovementAnimation();
            }
            nodeLengthDone += SpeedMultiplier * Speed * time;
            while ((nodeLength - nodeLengthDone) < Mathf.Epsilon)
            {
                nodeLengthDone -= nodeLength;
                if (CurrentNode.ElevatorDown || CurrentNode.ElevatorUp || Mathf.Approximately(nodeLengthDone, 0f))
                {
                    nodeLengthDone = 0f;
                }
                if (!savedData.HasValue || !LoadArrived())
                {
                    Next();
                    if (wait)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (Path.Count == 0)
            {
                Trans.position = CurrentNode.Position;
            }
            else
            {
                Trans.position = prevPosition + nodeLengthDone * direction;
            }
            if (savedData.HasValue && LoadArrived())
            {
                nodeLengthDone = savedData.Value.NodeLengthDone;
                return true;
            }
        }
        return arrived || rotated;
    }

    public void UpdateEvent()
    {
        if (arrived)
        {
            if (pushedSound)
            {
                pushSound.Stop();
                pushedSound = false;
            }
            arrived = false;
            Arrived(this);
        }

        if (rotated)
        {
            rotated = false;
            RotationFinished(this);
        }
    }

    public void Teleport(PlaneNode node, Quaternion rotation)
    {
        Path.Clear();
        CurrentNode = node;
        currentIndex = 0;
        Trans.position = node.Position;
        Trans.rotation = rotation;
        prevPosition = Trans.position;
        nodeLengthDone = 0;
    }

    public bool IsRotating()
    {
        return Squadron != null && Mathf.Abs(Quaternion.Angle(Trans.rotation, desiredRotation)) > .1f;
        //return Trans.rotation != desiredRotation;
    }

    public void Rotate(Quaternion quaternion)
    {
        desiredRotation = quaternion;
        if (SnapRotation)
        {
            Trans.rotation = desiredRotation;
            SnapRotation = false;
            RotationFinished(this);
        }
        else
        {
            Assert.IsFalse(Mathf.Approximately(quaternion.x, 0f) && Mathf.Approximately(quaternion.y, 0f) && Mathf.Approximately(quaternion.z, 0f) && Mathf.Approximately(quaternion.w, 0f));
            if (desiredRotation == Trans.rotation)
            {
                RotationFinished(this);
            }
            else
            {
                desiredEuler = desiredRotation.eulerAngles.y;

                bool tmp = MoveBackwards;

                float value = Mathf.Abs(desiredEuler - 180f);
                MoveBackwards = value > 90f;
                if (tmp != MoveBackwards)
                {
                    if (Mathf.Abs(value - 90f) < 5f)
                    {
                        MoveBackwards = tmp;
                    }
                    else
                    {
                        for (int i = 0; i < Crew.Count; ++i)
                        {
                            Crew[i].SetPush(MoveBackwards ? PushBottom[i] : PushFront[i]);
                        }
                    }
                }

                if (MoveBackwards)
                {
                    desiredEuler += 180f;
                    if (desiredEuler > 360f)
                    {
                        desiredEuler -= 360f;
                    }
                    desiredRotation = Quaternion.Euler(new Vector3(0f, desiredEuler, 0f));
                }
            }
        }
    }

    public void SnapRotate()
    {
        bool snap = SnapRotation;
        SnapRotation = true;
        Rotate(desiredRotation);
        SnapRotation = snap;
    }

    public void AnimCallback()
    {
        if (Callback != null)
        {
            var callb = Callback;
            Callback = null;
            callb(this);
        }
    }

    public void Clear()
    {
        Assert.IsFalse(ignore);
        ignore = true;
        CurrentNode = null;
        Assert.IsTrue(ignore);
        ignore = false;
        Squadron = null;
        Trans.position = new Vector3(0f, -1000f, 0f);
        Trans.rotation = Quaternion.identity;
        container.localPosition = Vector3.zero;
    }

    public void ResetNode(PlaneNode node)
    {
        CurrentNode = node;
        Trans.position = CurrentNode.Position;
    }

    public void ResetNode(PlaneNode node, Quaternion rotation, bool force)
    {
        if (force)
        {
            isInAir = false;
        }
        SnapRotation = true;
        Rotate(rotation);
        ResetNode(node);
    }

    public void ResetMovement()
    {
        setElevatorState = -1f;

        Trans.rotation = Quaternion.identity;

        arrived = false;
        rotated = false;
        Path.Clear();
        Delay = 0f;

        isInAir = true;
        fly = false;
        start = false;

        Callback = null;

        if (IsPlayingAnimation)
        {
            FinishAnim();
        }

        if (secondaryAnimator != null)
        {
            secondaryAnim = true;
            secondaryAnimState = false;
            secondaryAnimator.Play(ForceWingsOff);
        }

        if (pushedSound && pushSound != null)
        {
            bool allowFadeout = pushSound.AllowFadeout;
            pushSound.AllowFadeout = false;
            pushSound.Stop();
            pushSound.AllowFadeout = allowFadeout;

            pushedSound = false;
        }
    }

    public bool IsBehindWings(int crewIndex)
    {
        return Crew[crewIndex].PlaneCrew.parent == PushFront[crewIndex];
    }

    private void UpdateAnim(PlayableDirector anim, Action finished)
    {
        if (anim.gameObject.activeSelf)
        {
            Assert.IsTrue(IsPlayingAnimation);
            anim.time += Time.deltaTime * AnimSpeed;
            anim.Evaluate();
            if (anim.time >= anim.duration)
            {
                finished?.Invoke();
                FinishAnim();
                AnimCallback();
            }
        }
    }

    private IEnumerator PlayLandSoundWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        landSound.PlayEvent(TimeManager.Instance.CurrentSpeed);
    }

    private void Next()
    {
        if (savedData.HasValue && savedData.Value.CurrentPath != null && savedData.Value.PathNode <= currentIndex)
        {
            var savedPath = savedData.Value.CurrentPath;
            bool thisPath = Path.Count == savedPath.Count;
            if (thisPath)
            {
                for (int i = 0; i < Path.Count; i++)
                {
                    if (Path[i] != savedPath[i])
                    {
                        thisPath = false;
                        break;
                    }
                }
            }
            if (thisPath)
            {
                nodeLengthDone = 0f;

                var data = savedData.Value;
                data.NodeLengthDone = -5f;
                savedData = data;

                return;
            }
        }

        if (!wait && ++currentIndex == Path.Count)
        {
            Path.Clear();
            SetElevator();
            Assert.IsFalse(SnapRotation);
            arrived = true;
            StopCrewMovementAnimation();

            nodeLengthDone = 0f;
            nodeLength = 100f;
        }
        else
        {
            if (crewIsReady)
            {
                if (Mathf.Approximately(prevPosition.x, CurrentNode.Position.x))
                {
                    StopCrewMovementAnimation();
                }
                else
                {
                    StartCrewMovementAnimation();
                }
            }
            if (!savedData.HasValue && CurrentNode.WaitForNode != null && Squadron != null && Squadron.Planes.Count > 2)
            {
                if (!CurrentNode.Wait)
                {
                    CurrentNode.Wait = true;
                    CurrentNode.Wait2 = true;
                }

                if (!CurrentNode.Wait2 || CurrentNode.WaitForNode.Wait)
                {
                    wait = false;
                    CurrentNode.Wait = false;
                    CurrentNode.Wait2 = false;
                    CurrentNode.WaitForNode.Wait2 = false;
                }
                else
                {
                    StopCrewMovementAnimation();
                    wait = true;
                    nodeLengthDone = nodeLength;
                    return;
                }
            }

            prevPosition = CurrentNode.Position;
            var diff = Path[currentIndex].Position - prevPosition;
            nodeLength = diff.magnitude;
            if (Mathf.Approximately(nodeLength, 0f))
            {
                direction = Vector3.zero;
            }
            else
            {
                direction = diff / nodeLength;
                if (diff.y != nodeLength)
                {
                    LookAt(direction);
                }
            }
            CurrentNode = Path[currentIndex];
        }
    }

    private void LookAt(Vector3 direction)
    {
        direction.y = 0f;
        if (direction == Vector3.zero)
        {
            Rotate(Quaternion.identity);
        }
        else
        {
            direction = direction.normalized;
            Rotate(Quaternion.LookRotation(direction, Vector3.up));
        }
    }

    private void PlayAnim(PlayableDirector anim, int position, Action<PlaneMovement> callback)
    {
        Assert.IsFalse(IsPlayingAnimation);
        //Assert.IsNull(this.callback);
        //Assert.IsNotNull(callback);
        Assert.IsFalse(position < 0);
        Assert.IsFalse(position > 2);
        this.Callback = callback;
        IsPlayingAnimation = true;

        SetAnim();

        anim.time = 0f;
        anim.gameObject.SetActive(true);
    }

    private void FinishAnim()
    {
        foreach (var anim in flyAnims)
        {
            anim.gameObject.SetActive(false);
        }
        foreach (var anim in landAnims)
        {
            anim.gameObject.SetActive(false);
        }
        container.localPosition = Vector3.zero;
        container.localRotation = Quaternion.identity;
        fly = false;
        IsPlayingAnimation = false;
        SetAnim();
        Trans.rotation = desiredRotation = Quaternion.Euler(0f, 180f, 0f);
        if (startSound != null)
        {
            startSound.Stop(false);
        }
        if (landSound != null)
        {
            landSound.Stop(true);
        }
    }

    private void SetAnim()
    {
        Plane.localRotation = Quaternion.Euler(0f, (IsPlayingAnimation ? 0f : 180f), 0f);
    }

    private void SetElevator()
    {
        if (setElevator != null)
        {
            var deck = AircraftCarrierDeckManager.Instance;

            if (setElevator.ElevatorDown && (PlaneMovementManager.Instance.ParallelLifts != 3 || Path.Count == 0 || setElevator.Lift != 1))
            {
                deck.SetLiftState(setElevator.Lift, 1f, !IntermediateCrewReady());
            }
            else if (setElevator.ElevatorUp && CanLiftDown())
            {
                deck.SetLiftState(setElevator.Lift, 0f, !IntermediateCrewReady());
            }
            setElevator = null;
        }
    }

    private bool CheckCrew()
    {
        foreach (var plane in Squadron.Planes)
        {
            if ((plane.Path.Count <= 0 || Squadron.FromHangar || currentIndex > 2 || IgnoreOthers) && plane != this)
            {
                continue;
            }
            foreach (var crew in plane.Crew)
            {
                if (!crew.ReadyState || crew.IsSwitchingPush)
                {
                    return false;
                }
            }
        }
        foreach (var plane in Dependencies)
        {
            foreach (var crew in plane.Crew)
            {
                if (!crew.ReadyState || crew.IsSwitchingPush)
                {
                    return false;
                }
            }
        }
        foreach (var plane in Dependencies2)
        {
            foreach (var crew in plane.Crew)
            {
                if (!crew.ReadyState || crew.IsSwitchingPush)
                {
                    return false;
                }
            }
        }
        Dependencies.Clear();
        return true;
    }

    private float GetRotationDirection(Vector3 euler)
    {
        Assert.IsTrue(Mathf.Abs(euler.y - desiredEuler) < 361f);

        bool sign;
        float value = Mathf.Abs(desiredEuler - euler.y);
        sign = (desiredEuler > euler.y) == (value < (360f - value));

        //switch (RotationConstraint)
        //{
        //    case EPlaneRotation.Shorter:
        return sign ? 1f : -1f;
        //    case EPlaneRotation.Anticlockwise:
        //        return -1f;
        //    case EPlaneRotation.Clockwise:
        //    default:
        //        return 1f;
        //}
    }

    private bool IntermediateCrewReady()
    {
        foreach (var crew in Crew)
        {
            if (!crew.ReadyState)
            {
                return false;
            }
        }
        return true;
    }

    private bool CanLiftDown()
    {
        if (Squadron == null)
        {
            return true;
        }
        int index;
        bool positive = true;
        if (Squadron.Planes.Count == 2)
        {
            index = 0;
            if (PlaneMovementManager.Instance.ParallelLifts != 1)
            {
                return false;
            }
        }
        else if (PlaneMovementManager.Instance.ParallelLifts == 1)
        {
            index = 2;
            positive = false;
        }
        else
        {
            index = 1;
        }
        return (Squadron.Planes[index] == this) == positive;
    }
}