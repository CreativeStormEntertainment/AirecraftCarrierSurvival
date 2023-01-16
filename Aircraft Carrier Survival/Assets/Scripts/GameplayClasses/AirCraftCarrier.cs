using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class AirCraftCarrier : MonoBehaviour
{
    public static AirCraftCarrier Instance;

    [SerializeField] TextMeshPro carrierNameText = null;

    private float viewRangeEfficiency = 1f;
    public float ViewRangeEfficiency
    {
        get => viewRangeEfficiency;
        set
        {
            viewRangeEfficiency = value;
        }
    }

    private float viewRange = 1f;
    public float ViewRange
    {
        get => viewRange;
        set
        {
            viewRange = value;
        }
    }

    [SerializeField]
    bool movementLocked = false;
    public bool MovementLocked { get => movementLocked; set => movementLocked = value; }

    [SerializeField]
    bool courseLocked = false;
    public bool CourseLocked { get => courseLocked; set => courseLocked = value; }

    [SerializeField]
    float movementSpeedEfficiency = 1f;
    public float MovementSpeedEfficiency
    {
        get => movementSpeedEfficiency;
        set
        {
            movementSpeedEfficiency = value;
        }
    }


    [SerializeField]
    int maxMovementSpeed = 2;
    public int MaxMovementSpeed
    {
        get => maxMovementSpeed;
        set
        {
            maxMovementSpeed = value;
            MovementSpeed = MovementSpeed;
        }
    }

    [SerializeField]
    int movementSpeed;
    public int MovementSpeed
    {
        get => movementSpeed;
        set
        {
            movementSpeed = Mathf.Clamp(value, 0, maxMovementSpeed);
        }
    }

    [SerializeField]
    float aaPower;
    [SerializeField]
    float maxAAPower = 100f;
    public float AAPower
    {
        get => aaPower;
        set
        {
            aaPower = Mathf.Clamp(value, 0f, maxAAPower);
        }
    }

    [SerializeField]
    float dodgeChance;
    public float DodgeChance
    {
        get => dodgeChance;
        set
        {
            dodgeChance = Mathf.Clamp(value, 0f, 1f);
        }
    }

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    void Start()
    {
        //MapManager.Instance.OnCourseChanged += (id) => NextCourse = id;
//        movementProgresser = new Progresser(() => movementSpeed / SegmentLength, () => OnFleetProgress(movementProgresser.Progress), () =>
//        {
//            CurrentCourse = NextCourse;
//            if (CurrentCourse != -1)
//                OnFleetNextSegment();
//            else
//            {
//                movementProgresser.IsWorking = false;
//                movementProgresser.Progress = 1f;
//#warning todo
//            }
//        }) ;
//        movementProgresser.InvokeProgress();

        MovementSpeed = 1;
    }

    public void SetCarrierName(string name)
    {
        carrierNameText.text = name;
    }
}
