using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Wreck : MonoBehaviour
{
    private static readonly int DropHash = Animator.StringToHash("Drop");
    private static readonly string FinishCrashAnimTag = "NoCrash";
    private static readonly string FinishDropAnimTag = "NoDrop";
    private static readonly string IdleAnim = "Idle";

    public bool Move
    {
        get => move;
        set
        {
            move = value;
            if (wreckSounds != null)
            {
                if (move)
                {
                    wreckSounds.PlayEvent(EWreckSound.Push, TimeManager.Instance.CurrentSpeed);
                    currentSound = EWreckSound.Push;
                }
                else if (currentSound == EWreckSound.Push)
                {
                    //wreckSounds.Stop(true);
                }
            }
        }
    }

    public bool AnimCrash
    {
        get;
        private set;
    }

    [NonSerialized]
    public List<InstanceData> DC;

    [SerializeField]
    private Animator animator = null;

    [SerializeField]
    private List<Transform> dcPositions = null;

    [SerializeField]
    private Transform trans = null;

    [SerializeField]
    private Transform dropTransform = null;

    [SerializeField]
    private PlaneSpeedSound crashSound = null;

    [SerializeField]
    private WreckSounds wreckSounds = null;

    [SerializeField]
    private EWreckType type = EWreckType.Wreck;

    [SerializeField]
    private WreckAnimExplosion additional = null;

    [SerializeField]
    private float timeToStartAnim = 20f;

    [SerializeField]
    private GameObject fallParticles = null;

    [SerializeField]
    private float timeToSplash = 0f;

    private bool move;
    private bool drop;
    private Vector3 startPosition;
    private Vector3 finishPosition;
    private float time;
    private float maxTime;

    private EWreckSound currentSound;

    private float timer;

    private void Awake()
    {
        DC = new List<InstanceData>();
        AnimCrash = true;
        AircraftCarrierDeckManager.Instance.IsRunwayDamaged = true;
        animator.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (timer <= timeToStartAnim)
        {
            timer += Time.deltaTime;
            if (timer > timeToStartAnim)
            {
                animator.gameObject.SetActive(true);
                if (crashSound != null)
                {
                    crashSound.PlayEvent(TimeManager.Instance.CurrentSpeed);
                }
            }
        }
        if (AnimCrash)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag(FinishCrashAnimTag))
            {
                Crashed(false);
            }
        }
        else if (drop)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag(FinishDropAnimTag))
            {
                drop = false;
                var deck = AircraftCarrierDeckManager.Instance;
                switch (type)
                {
                    case EWreckType.Wreck:
                        deck.HasWreck = false;
                        break;
                    case EWreckType.FrontKamikaze:
                        deck.HasKamikazeFront = false;
                        break;
                    case EWreckType.EndKamikaze:
                        deck.HasKamikazeEnd = false;
                        break;
                }

                PlaneManager.Instance.StartCoroutineActionAfterTime(() =>
                {
                    if (this != null)
                    {
                        Destroy(gameObject);
                    }
                }, 10f);
            }
        }
        else if (Move)
        {
            time += Time.deltaTime;
            if (time >= maxTime)
            {
                trans.position = finishPosition;
                Move = false;
            }
            else
            {
                trans.position = Vector3.Lerp(startPosition, finishPosition, time / maxTime);
            }
            SetWaypoints();
            var worInMan = WorkerInstancesManager.Instance;
            foreach (var dc in DC)
            {
                dc.Trans.position = dc.GetDestination().Trans.position;
                worInMan.GPUICrowdManager.UpdateTransformDataForInstance(dc.Ref);
            }
            if (!Move)
            {
                Drop();
            }
        }
    }

    public void Drop()
    {
        drop = true;
        animator.SetBool(DropHash, true);
        var dcMan = DamageControlManager.Instance;
        dcMan.SetShowWreckButton(false, type);

        this.StartCoroutineActionAfterTime(FireSplash, timeToSplash);

        bool cleared = true;
        foreach (var wreck in PlaneMovementManager.Instance.CurrentWrecks)
        {
            if (wreck != null && !wreck.drop)
            {
                cleared = false;
                break;
            }
        }

        var wreckSubsection = dcMan.WreckSection.SubsectionRooms[(int)type];
        var data = wreckSubsection.Destruction.RepairData;
        data.Current = data.Max;
        wreckSubsection.Destruction.Update();
        AircraftCarrierDeckManager.Instance.IsRunwayDamaged = !cleared;
    }

    public void InstantRemove()
    {
        var worInMan = WorkerInstancesManager.Instance;
        foreach (var dc in DC)
        {
            dc.Trans.position = dc.GetDestination().Trans.position;
            worInMan.GPUICrowdManager.UpdateTransformDataForInstance(dc.Ref);
        }

        DamageControlManager.Instance.SetShowWreckButton(false, type);

        var deck = AircraftCarrierDeckManager.Instance;
        switch (type)
        {
            case EWreckType.Wreck:
                deck.HasWreck = false;
                break;
            case EWreckType.FrontKamikaze:
                deck.HasKamikazeFront = false;
                break;
            case EWreckType.EndKamikaze:
                deck.HasKamikazeEnd = false;
                break;
        }
        Destroy(gameObject);
    }

    public void SetAnimator(Animator animator, bool load)
    {
        this.animator = animator;
        animator.Play(IdleAnim);
        Crashed(load);
    }

    public void Load()
    {
        if (additional == null)
        {
            animator.Play(IdleAnim);
            Crashed(true);
        }
        else
        {
            additional.Load();
        }
        if (crashSound != null)
        {
            crashSound.Stop(true);
        }
    }

    private void Crashed(bool load)
    {
        AnimCrash = false;
        if (!load)
        {
            if (crashSound != null && type == EWreckType.Wreck)
            {
                crashSound.Stop(true);
            }
            if (additional != null && type != EWreckType.Wreck)
            {
                additional.Crash();
            }
        }

        var dcMan = DamageControlManager.Instance;
        startPosition = trans.position;
        int index = (int)type;
        finishPosition = dcMan.FinalWreckPositions[index].position;
        time = 0f;
        maxTime = Vector3.Distance(startPosition, finishPosition) / dcMan.WreckSpeed;

        dcMan.SetShowWreckButton(true, type);
        EventManager.Instance.AddWreck();

        SetWaypoints();
        dcMan.WreckSection.SubsectionRooms[index].IsBroken = true;
    }

    private void SetWaypoints()
    {
        SetWaypoints(DamageControlManager.Instance.WreckWaypoints[(int)type]);
    }

    private void SetWaypoints(List<Waypoint> waypoints)
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            waypoints[i].Trans.position = dcPositions[i].position;
        }
    }

    private void FireSplash()
    {
        if (wreckSounds != null)
        {
            currentSound = EWreckSound.Fall;
            wreckSounds.SetParameter(EWreckSound.Fall);
        }
        if (fallParticles != null)
        {
            fallParticles.SetActive(true);
        }
    }
}
