//using GPUInstancer;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Assertions;

//public class BridgeRoom : Room
//{
//    [SerializeField]
//    private bool isChangeLocked;
//    private int chosenModeIndex;

//    public List<EffectsDataWrapper> EffectsWrapper;
//    public float IdleTime;
//    public float LockChangeTime;
//    public int DefaultEffectIndex;

//    //public WaypointsGroup Waypoints;
//    [HideInInspector]
//    public List<InstanceData> WorkerInstances = new List<InstanceData>();

//    void Start()
//    {
//        foreach (var wrapper in EffectsWrapper)
//        {
//            wrapper.GameplayInit(EffectManager.Instance);
//        }

//#warning fix
//        EffectsWrapper = new List<EffectsDataWrapper>() { new EffectsDataWrapper() { Effects = new List<EffectData>() } };
//        DefaultEffectIndex = 0;

//        chosenModeIndex = DefaultEffectIndex;
//        LockRoomOnChangedEffect();
//    }

//    void Update()
//    {
//        //WorkerManager.Instance.MoveInstances(WorkerInstances, null, null);
//    }

//    public void ChangeEffect(int effectIndex)
//    {
//        if (!isChangeLocked)
//        {
//            foreach (var effect in EffectsWrapper[chosenModeIndex].Effects)
//            {
//                EffectManager.Instance.OnEffectFinish(effect);
//            }
//            chosenModeIndex = effectIndex;
//            LockRoomOnChangedEffect();
//        }
//    }

//    void UnIdleAndChangeEffect()
//    {
//        foreach (var effect in EffectsWrapper[chosenModeIndex].Effects)
//        {
//            EffectManager.Instance.OnEffectStart(effect);
//        }
//        //IsActive = true;
//    }

//    void UnlockChangeEffect()
//    {
//        isChangeLocked = false;
//    }

//    void LockRoomOnChangedEffect()
//    {
//        //IsActive = false;
//        isChangeLocked = true;
//        Invoke("UnIdleAndChangeEffect", IdleTime);
//        Invoke("UnlockChangeEffect", LockChangeTime);
//    }
//}
