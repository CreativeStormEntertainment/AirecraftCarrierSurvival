using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityRandom = UnityEngine.Random;

public class PlaneCrewMovement : MonoBehaviour
{
    public enum EPlaneCrewState { IDLE, WALK, PUSH, AWAY, JUMP_DOWN, JUMP_UP, COUNT };
    public enum EPlaneCrewPathState { AT_SIDE, AT_PLANE, TO_PLANE, TO_SIDE };

    private static readonly string WalkAnim = "WALK";
    private static readonly string IdleAnim = "IDLE";
    private static readonly string PushingAnim = "PUSH";
    private static readonly string JumpDownAnim = "JUMP_DOWN";
    private static readonly string JumpUpAnim = "JUMP_UP";

    private PlaneCrewNode currentNode;

    public Transform PlaneCrew => planeCrew;
    //public bool IsBehindWings => planeCrew.parent == PushFront[0]; /*planeCrew.localEulerAngles == Vector3.zero;*/

    [SerializeField]
    private Transform planeCrew;

    public PlaneCrewNode CurrentNode
    {
        get => currentNode;
        private set
        {
            currentNode = value;
        }
    }

    [SerializeField]
    public List<PlaneCrewNode> Path;

    public float Speed;
    public bool ReadyState = true;
    public bool IsSwitchingPush
    {
        get;
        private set;
    } = false;

    public float SpeedMultiplier
    {
        get => speedMultiplier;
        set
        {
            speedMultiplier = value;
            animator.speed = value;
        }
    }

    float crouchSpeedModify = 1f;
    float crouchSpeed = 0.5f;

    public float Delay
    {
        get => delay;
        set
        {
            delay = value;
            if (delay > 0f)
            {
                isDelayed = true;
            }
            delayTS = 0f;
        }
    }
    public EPlaneCrewPathState CrewPathState = EPlaneCrewPathState.AT_PLANE;

    public bool MoveBackwards;

    public PlaneCrewRallyPoint RallyPoint = null;

    public int SquadronIndex = 0;

    public bool UseCrouch = false;
    public PlaneMovement PlaneToHideWings { set; get; } = null;
    public EPlaneCrewState CrewState => crewState;

    private bool isCrouchNeeded
    {
        get
        {
            var deckMan = AircraftCarrierDeckManager.Instance;
            int indexMinus = SquadronIndex - 1;
            int indexPlus = SquadronIndex + 1;
            return previousNode != null && 
                (UseCrouch && CurrentNode.ThisLineCrouch && previousNode.ThisLineCrouch) ||

                (CurrentNode.PrevLineCrouch && previousNode.PrevLineCrouch && indexMinus >= 0 && deckMan.DeckSquadrons.Count > indexMinus &&
                    deckMan.DeckSquadrons[indexMinus].Planes.Count > 0 && deckMan.DeckSquadrons[indexMinus].Planes[0].HasStaticWings) ||

                (CurrentNode.NextLineCrouch && previousNode.NextLineCrouch && indexPlus >= 0 && deckMan.DeckSquadrons.Count > indexPlus &&
                    deckMan.DeckSquadrons[indexPlus].Planes.Count > 0 && deckMan.DeckSquadrons[indexPlus].Planes[0].HasStaticWings);
        }
    }
    public bool InRandomIdleMode
    {
        get;
        private set;
    } = true;

    private PlaneCrewNode previousNode = null;
    private Vector3 prevPosition;
    public int CurrentIndex
    {
        get;
        private set;
    }
    private Vector3 direction;
    private float nodeLengthDone;
    private float nodeLength;

    private bool arrived;
    private Animator animator;

    private float time;
    private float timeMax;

    private float speedMultiplier;

    private Quaternion startRotation;
    private Quaternion desiredRotation;
    private string currentAnim;
    private int currentAnimInt;
    private int currentAnimIntDesired;
    private int animSwitchDelayCounter = 0;
    private int animSwitchDelay = 1;
    private EPlaneCrewState crewState = EPlaneCrewState.COUNT;


    private bool isDelayed;
    private float delay;
    private float delayTS;

    private bool alreadyJumped;

    private List<int> jumpUpHashes = new List<int>();
    private List<int> jumpDownHashes = new List<int>();
    private List<List<int>> idleHashes = new List<List<int>>();

    private int currentRandomIdle = 1;

    private bool isFeared;

    private float switchPushTime;
    private float switchPushEndDelay = 0.1f;
    private bool afterSwitchPushEndDelay;

    private float switchPushTS;
    private Vector3 startSwitchPushPos = Vector3.zero;
    private Vector3 endSwitchPushPos = Vector3.zero;
    private bool switchToBottom;


    private void Awake()
    {
        planeCrew = transform;
        animator = GetComponent<Animator>();
        animator.SetInteger("IDLE", UnityRandom.Range(1, 12));
        ResetCrew();
    }

    private void Update()
    {
        UpdateInner(Time.deltaTime, false);
    }

    public void LoadUpdate()
    {
        UpdateInner(.1f, true);
    }

    public void Init(bool useCrouch, List<List<int>> idleHashes, List<int>jumpUpHashes, List<int>jumpDownHashes, float switchPushTime, float switchPushDelay)
    {
        this.UseCrouch = useCrouch;
        this.idleHashes = idleHashes;
        this.jumpUpHashes = jumpUpHashes;
        this.jumpDownHashes = jumpDownHashes;
        this.switchPushTime = switchPushTime;
        this.switchPushEndDelay = switchPushTime - switchPushDelay;
    }

    public bool UpdateEvent()
    {
        if (arrived)
        {
            if (ReadyState)
            {
                if (CrewState == EPlaneCrewState.PUSH)
                {
                    planeCrew.localPosition = Vector3.zero;
                }
            }
            else if (Path != null && Path.Count > 0)
            {
                FinalIdle(AircraftCarrierDeckManager.Instance.DeckMode == EDeckMode.Starting ? 0 : 1);
            }
            CurrentIndex = 0;
            arrived = false;
            return true;
        }
        return false;
    }

    public bool MyUpdate(float time)
    {
        if (crewState == EPlaneCrewState.JUMP_DOWN || crewState == EPlaneCrewState.JUMP_UP)
        {
            return false;
        }

        if (IsSwitchingPush)
        {
            switchPushTS += time;
            planeCrew.position = Vector3.Lerp(startSwitchPushPos, endSwitchPushPos, switchPushTS / switchPushEndDelay);
            if (!afterSwitchPushEndDelay && switchPushTS >= switchPushEndDelay)
            {
                afterSwitchPushEndDelay = true;
                Crouch(false);

                if (switchToBottom)
                {
                    planeCrew.localEulerAngles = Vector3.zero;
                }
                else
                {
                    planeCrew.localEulerAngles = new Vector3(0f, 180f, 0f);
                }
            }
            if (switchPushTS >= switchPushTime)
            {
                IsSwitchingPush = false;
                ReadyState = true;
            }
            else
            {
                return false;
            }
        }

        if (this.time != timeMax && !isDelayed)
        {
            this.time = Mathf.Min(this.time + SpeedMultiplier * crouchSpeedModify * time, timeMax);
            //planeCrew.rotation = Quaternion.Slerp(startRotation, desiredRotation, this.time / timeMax);
            //trans.rotation = Quaternion.RotateTowards(trans.rotation, desiredRotation, SpeedMultiplier * maxRotationSpeed * Time.deltaTime);
        }

        if (Path != null && Path.Count == 0)
        {
            arrived = true;
        }
        if (this.time == timeMax && Path.Count != 0 && !isDelayed)
        {
            int jump = -1;
            nodeLengthDone += SpeedMultiplier * crouchSpeedModify * Speed * time;
            while ((nodeLength - nodeLengthDone) < Mathf.Epsilon)
            {
                jump = CurrentNode.IsJump;
                nodeLengthDone -= nodeLength;
                if (Mathf.Approximately(nodeLengthDone, 0f))
                {
                    nodeLengthDone = 0f;
                }
                Next();
            }

            if (Path.Count == 0)
            {
                arrived = true;
                planeCrew.position = CurrentNode.Position;
            }
            else if (jump == 1 && !alreadyJumped)
            {
                Rotate(Path[CurrentIndex - 1].Rotation);
                JumpUp();
                jump = 2;
            }
            else if (jump == 0 && !alreadyJumped)
            {
                Rotate(Path[CurrentIndex - 1].Rotation);
                JumpDown();
                jump = 2;
            }
            else
            {
                planeCrew.position = prevPosition + nodeLengthDone * direction;
            }
        }
        return arrived;
    }

    public void Rotate(Quaternion quaternion)
    {
        planeCrew.rotation = quaternion;
        time = timeMax = 0f;
        //startRotation = planeCrew.rotation;
        //desiredRotation = quaternion;
        //timeMax = Quaternion.Angle(startRotation, desiredRotation) / maxRotationSpeed;
    }

    public void SetupAnimation(PlaneCrewNode startNode, Transform transform, bool pathChanged = false)
    {
        arrived = false;
        previousNode = null;
        CurrentNode = startNode;
        if (transform != null)
        {
            Path.Add(new PlaneCrewNode(transform.position));
        }
        gameObject.transform.SetParent(transform);
        planeCrew.position = CurrentNode.Position;
        prevPosition = CurrentNode.Position;
        if (pathChanged)
        {
            CurrentIndex = 0;
            Next();
        }
        else
        {
            Next();
            CurrentIndex = 0;
        }
        if (!isDelayed)
        {
            Walk(isCrouchNeeded);
        }
        else
        {
            Idle();
        }
    }

    public void SetupCrew(Transform transform, bool visibleState)
    {
        ResetCrew();

        var trans = gameObject.transform;
        trans.SetParent(transform);
        trans.position = Vector3.zero;
        trans.localPosition = Vector3.zero;
        gameObject.SetActive(visibleState);
        if (RallyPoint != null)
        {
            RallyPoint.IsFree = true;
        }
        RallyPoint = null;
    }

    public void Walk(bool crouch, bool forceCrouch = false)
    {
        if (IsSwitchingPush)
        {
            return;
        }
        animator.applyRootMotion = false;
        Crouch(crouch, forceCrouch);
        if (crewState != EPlaneCrewState.WALK)
        {
            ReadyState = false;
            MoveBackwards = false;
            currentAnim = WalkAnim;
            currentAnimIntDesired = 1;
            crewState = EPlaneCrewState.WALK;
            if (InRandomIdleMode)
            {
                InRandomIdleMode = false;
                animator.Play("WALK");
            }
        }
    }
    public void JumpDown()
    {
        if (crewState != EPlaneCrewState.JUMP_DOWN)
        {
            Crouch(false);
            animator.applyRootMotion = true;
            ReadyState = false;
            MoveBackwards = false;
            currentAnim = JumpDownAnim;
            currentAnimIntDesired = 3;
            crewState = EPlaneCrewState.JUMP_DOWN;
            animSwitchDelayCounter = 0;
            animator.SetBool("JUMP", true);
            animator.SetBool("JUMP_END", false);
            animator.Play(jumpDownHashes[UnityRandom.Range(0, jumpDownHashes.Count)]);
        }
    }

    public void JumpUp()
    {
        if (crewState == EPlaneCrewState.JUMP_UP)
        {
            Debug.LogError("wtf");
        }
        else
        {
            Crouch(false);
            animator.applyRootMotion = true;
            ReadyState = false;
            MoveBackwards = false;
            currentAnim = JumpUpAnim;
            currentAnimIntDesired = 4;
            crewState = EPlaneCrewState.JUMP_UP;
            animSwitchDelayCounter = 0;
            alreadyJumped = true;
            animator.SetBool("JUMP", true);
            animator.SetBool("JUMP_END", false);
            animator.Play(jumpUpHashes[UnityRandom.Range(0, jumpUpHashes.Count)]);
        }
    }


    public void Push(bool moveDirection)
    {
        if (IsSwitchingPush)
        {
            return;
        }
        animator.applyRootMotion = false;
        MoveBackwards = moveDirection;
        if (crewState != EPlaneCrewState.PUSH)
        {
            Crouch(false);
            currentAnim = PushingAnim;
            currentAnimIntDesired = 2;
            crewState = EPlaneCrewState.PUSH;
            animSwitchDelayCounter = 0;
            if (InRandomIdleMode)
            {
                InRandomIdleMode = false;
                animator.Play("PUSH");
            }
        }
    }

    public void Idle()
    {
        animator.applyRootMotion = false;
        if (crewState != EPlaneCrewState.IDLE)
        {
            Crouch(false);
            currentAnim = IdleAnim;
            currentAnimIntDesired = 0;
            crewState = EPlaneCrewState.IDLE;
            animSwitchDelayCounter = 0;
            if (InRandomIdleMode)
            {
                InRandomIdleMode = false;
                animator.Play("IDLE");
            }
        }
    }

    public void Away()
    {
        if (crewState != EPlaneCrewState.AWAY)
        {
            Crouch(false);
            ReadyState = false;
            currentAnim = IdleAnim;
            currentAnimIntDesired = 0;
            crewState = EPlaneCrewState.AWAY;
            animSwitchDelayCounter = 0;
        }
    }

    public void TeleportToRallyPoint(PlaneCrewRallyPoint rallyPoint, EPlaneNodeGroup node)
    {
        if (this.RallyPoint != null)
        {
            RallyPoint.IsFree = true;
        }
        this.RallyPoint = rallyPoint;
        ReadyState = false;

        gameObject.transform.SetParent(null);
        planeCrew.position = rallyPoint.Transf.position;
        Idle();
        FinalIdle(node == EPlaneNodeGroup.DeckLaunching ? 0 : 1);
        planeCrew.localEulerAngles = new Vector3(0, planeCrew.localEulerAngles.y, 0);
        prevPosition = planeCrew.position;
        alreadyJumped = false;
        animator.SetBool("JUMP", false);
        animator.SetBool("JUMP_END", false);
        Assert.IsFalse(crewState == EPlaneCrewState.JUMP_UP || crewState == EPlaneCrewState.JUMP_DOWN);
    }

    public void EndJump()
    {
        animator.SetBool("JUMP", false);

        Path.Insert(CurrentIndex, new PlaneCrewNode(planeCrew.position));
        currentNode = Path[CurrentIndex];
        Walk(isCrouchNeeded);
        animator.SetInteger("STATE", currentAnimIntDesired);

        Next();
        Assert.IsFalse(crewState == EPlaneCrewState.JUMP_UP || crewState == EPlaneCrewState.JUMP_DOWN);
    }

    public void SetFear(bool state)
    {
        isFeared = state;
        animator.SetBool("FEARED", state);
        if (state)
        {
            if (crewState == EPlaneCrewState.IDLE)
            {
                animator.SetInteger("FEAR_VARIANT", UnityRandom.Range(1, 6));
                animator.Play(idleHashes[animator.GetInteger("FINAL_IDLE")][0]);
            }
            else
            {
                animator.SetInteger("FEAR_VARIANT", UnityRandom.Range(1, 3));
            }
        }
    }

    public void IdleEnded()
    {
        if (!isFeared)
        {
            var r = UnityRandom.value;
            if (r < 0.4f)
            {
                r = UnityRandom.Range(0, idleHashes[currentRandomIdle - 1].Count);
                animator.Play(idleHashes[currentRandomIdle - 1][(int)r]);
            }
            else
            {
                int tmpRandom = 1 + UnityRandom.Range(0, idleHashes.Count);
                while (tmpRandom == currentRandomIdle)
                {
                    tmpRandom = 1 + UnityRandom.Range(0, idleHashes.Count);
                }
                currentRandomIdle = tmpRandom;
                animator.SetInteger("FINAL_IDLE", currentRandomIdle);
            }
        }
    }

    public void SetPush(Transform pushSlot)
    {
        if (planeCrew.parent != pushSlot)
        {
            ReadyState = false;
            Walk(true, true);
            IsSwitchingPush = true;
            switchPushTS = 0;
            planeCrew.SetParent(pushSlot);
            startSwitchPushPos = planeCrew.position;
            endSwitchPushPos = pushSlot.position;

            if (transform.parent.name.Contains("Bottom"))
            {
                planeCrew.localEulerAngles = Vector3.zero;
                switchToBottom = false;
            }
            else
            {
                planeCrew.localEulerAngles = new Vector3(0, 180, 0);
                switchToBottom = true;
            }
            afterSwitchPushEndDelay = false;
        }
    }

    public void ResetMovement(PlaneCrewRallyPoint rally, EPlaneNodeGroup deck)
    {
        CrewPathState = EPlaneCrewPathState.AT_SIDE;
        Path.Clear();
        if (RallyPoint != null)
        {
            RallyPoint.IsFree = true;
        }
        RallyPoint = rally;
        CurrentNode = new PlaneCrewNode(rally.Transf.position);

        gameObject.transform.SetParent(null);
        planeCrew.position = RallyPoint.Transf.position;
        Idle();
        FinalIdle(deck == EPlaneNodeGroup.DeckLaunching ? 0 : 1);
        planeCrew.localEulerAngles = new Vector3(0f, planeCrew.localEulerAngles.y, 0f);
        prevPosition = planeCrew.position;
        alreadyJumped = false;
        animator.SetBool("JUMP", false);
        animator.SetBool("JUMP_END", false);
        Assert.IsFalse(crewState == EPlaneCrewState.JUMP_UP || crewState == EPlaneCrewState.JUMP_DOWN);
    }

    public void ResetMovement(Transform pos)
    {
        SetupCrew(pos, false);
    }

    private void UpdateInner(float delta, bool load)
    {
        if (isDelayed)
        {
            delayTS += delta;
            if (delayTS >= Delay)
            {
                isDelayed = false;
                Delay = 0f;
                delayTS = 0f;
                Walk(isCrouchNeeded);
            }
            else
            {
                Idle();
            }
        }

        if (currentAnimInt != currentAnimIntDesired)
        {
            if (animSwitchDelayCounter >= animSwitchDelay)
            {
                currentAnimInt = currentAnimIntDesired;
                animator.SetInteger("STATE", currentAnimInt);
            }
            ++animSwitchDelayCounter;
        }
        if ((CrewState == EPlaneCrewState.JUMP_DOWN || CrewState == EPlaneCrewState.JUMP_UP) && (load || animator.GetBool("JUMP_END")))
        {
            EndJump();
        }
    }

    private void ResetCrew()
    {
        ReadyState = true;
        planeCrew = transform;
        speedMultiplier = 1f;
        Path = new List<PlaneCrewNode>();
        currentAnim = IdleAnim;
        currentAnimInt = 0;
        currentAnimIntDesired = 0;
        Delay = 0f;
        delayTS = 0f;
        isDelayed = false;
        Idle();
    }

    private void Next()
    {
        alreadyJumped = false;
        if (++CurrentIndex == Path.Count)
        {
            Idle();
            if (transform.parent != null && transform.parent.name.Contains("Bottom"))
            {
                planeCrew.localEulerAngles = new Vector3(0f, 180f, 0f);
            }
            else if (transform.parent != null && transform.parent.name.Contains("Front"))
            {
                planeCrew.localEulerAngles = Vector3.zero;
            }
            else
            {
                FinalIdle(AircraftCarrierDeckManager.Instance.DeckMode == EDeckMode.Starting ? 0 : 1);
            }
            Path.Clear();
            arrived = true;
            if (CrewPathState == EPlaneCrewPathState.TO_PLANE)
            {
                CrewPathState = EPlaneCrewPathState.AT_PLANE;
            }
            else if (CrewPathState == EPlaneCrewPathState.TO_SIDE)
            {
                CrewPathState = EPlaneCrewPathState.AT_SIDE;
            }
        }
        else
        {
            prevPosition = CurrentNode.Position;
            if (Path.Count > CurrentIndex)
            {
                CurrentNode = Path[CurrentIndex];
                if (CurrentIndex - 1 >= 0)
                {
                    previousNode = Path[CurrentIndex - 1];
                }

                var diff = CurrentNode.Position - prevPosition;
                nodeLength = diff.magnitude;
                if (Mathf.Approximately(nodeLength, 0f))
                {
                    direction = Vector3.zero;
                }
                else
                {
                    direction = diff / nodeLength;
                    LookAt(direction);
                }
                Walk(isCrouchNeeded);
            }
        }
    }

    private void LookAt(Vector3 direction)
    {
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            //if (MoveBackwards)
            //{
            //    direction = -direction;
            //}
            Rotate(Quaternion.LookRotation(direction.normalized, Vector3.up));
        }
    }

    private void FinalIdle(int lookAtIndex)
    {
        planeCrew.eulerAngles = new Vector3
        (
            0f,
            (planeCrew.position.x > PlaneCrewMovementManager.Instance.RallyPointLookAt[lookAtIndex].position.x ? -90f : 90f) + UnityRandom.Range(-25f, 25f),
            0f
        );
        InRandomIdleMode = true;
        currentRandomIdle = 1 + UnityRandom.Range(0, idleHashes.Count);
        animator.Play(idleHashes[currentRandomIdle - 1][0]);
    }

    private void Crouch(bool state, bool forceCrouch = false)
    {
        if (state && (UseCrouch || forceCrouch))
        {
            animator.SetBool("CROUCH", true);
            crouchSpeedModify = crouchSpeed;
            animator.Play("CROUCH");
        }
        else
        {
            animator.SetBool("CROUCH", false);
            crouchSpeedModify = 1;
        }
    }
}