using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class WorkerInstancesManager : MonoBehaviour, IEnableable
{
    public static WorkerInstancesManager Instance;

    public GPUICrowdManager GPUICrowdManager;

    public int SailorPrototypeStartIndex = 0;
    public int PilotPrototypeStartIndex = 1;
    public int OfficerPrototypeStartIndex = 2;
    public int DCPrototypeStartIndex = 3;
    public int FirefighterPrototypeStartIndex = 4;
    public int InjuredPrototypeStartIndex = 5;

    public int SailorInstancesPerSection = 5;
    public int OfficerInstancesPerSection = 1;
    public int InstancesPerDC = 4;

    public int SailorInstancesOnDeckPerPath = 20;
    public int OfficerInstancesOnDeckPerPath = 0;

    public float SameEntryDelay = .5f;

    public const string SaturationBufferName = "saturationBuffer";
    public float SaturationNotSelected = .25f;
    public float SaturationSelected = 1f;

    public GameObject Deck;
    [HideInInspector]
    public GPUICrowdPrototype InstancePrototype;
    private Dictionary<EWorkerType, List<GPUICrowdPrefab>> prefabs;
    private List<GPUICrowdPrefab> crowdPrefabs;

    private List<InstanceGroup> deckGroups;

    private bool setup;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        InstancePrototype = GPUICrowdManager.prototypeList[0] as GPUICrowdPrototype;

        var prototypesIndices = new Dictionary<EWorkerType, int>();
        prototypesIndices[EWorkerType.Sailor] = SailorPrototypeStartIndex;
        prototypesIndices[EWorkerType.Pilot] = PilotPrototypeStartIndex;
        prototypesIndices[EWorkerType.Officer] = OfficerPrototypeStartIndex;
        prototypesIndices[EWorkerType.DC] = DCPrototypeStartIndex;
        prototypesIndices[EWorkerType.Firefighter] = FirefighterPrototypeStartIndex;
        prototypesIndices[EWorkerType.Injured] = InjuredPrototypeStartIndex;

        prefabs = new Dictionary<EWorkerType, List<GPUICrowdPrefab>>();
        var currentPrototype = new KeyValuePair<EWorkerType, int>();
        var nextPrototype = prototypesIndices.First((x) => x.Value == 0);
        for (int i = 0; i < GPUICrowdManager.prototypeList.Count; i++)
        {
            if (nextPrototype.Value == i)
            {
                currentPrototype = nextPrototype;
                int value = int.MaxValue;
                foreach (var pair in prototypesIndices)
                {
                    if (pair.Value > i && pair.Value < value)
                    {
                        value = pair.Value;
                        nextPrototype = pair;
                    }
                }
            }
            List<GPUICrowdPrefab> prototypePrefabs;
            if (!prefabs.TryGetValue(currentPrototype.Key, out prototypePrefabs))
            {
                prototypePrefabs = new List<GPUICrowdPrefab>();
                prefabs[currentPrototype.Key] = prototypePrefabs;
            }
            //(GPUICrowdManager.prototypeList[i] as GPUICrowdPrototype).animationData.animationTexture = InstancePrototype.animationData.animationTexture;
            prototypePrefabs.Add(GPUICrowdManager.prototypeList[i].prefabObject.GetComponent<GPUICrowdPrefab>());

            Assert.IsTrue(GPUICrowdManager.prototypeList[i] is GPUInstancerPrefabPrototype);
            GPUInstancerAPI.DefinePrototypeVariationBuffer<float>(GPUICrowdManager, GPUICrowdManager.prototypeList[i] as GPUInstancerPrefabPrototype, SaturationBufferName);
        }

        crowdPrefabs = new List<GPUICrowdPrefab>();
    }

    private void Update()
    {
        foreach (var group in deckGroups)
        {
            foreach (var workerInstance in group.Instances)
            {
                if (workerInstance.CurrentAnim == null)
                {
                    workerInstance.Delay -= Time.deltaTime;
                    if (workerInstance.Delay < Mathf.Epsilon)
                    {
                        workerInstance.Delay = 0f;
                        if (workerInstance.Go(Time.deltaTime))
                        {
                            Assert.IsTrue(workerInstance.GetDestination().Data.AnimType == EWaypointAnimType.ActionAnim);
                            StartAnim(workerInstance, workerInstance.StartAnimation());
                        }
                        GPUInstancerAPI.UpdateTransformDataForInstance(GPUICrowdManager, workerInstance.Ref);
                    }
                }
            }
        }
    }

    public void SetEnable(bool enable)
    {
        DamageControlManager.Instance.SetEnableDC(enable);
    }

    public void CreateWorkers(InstanceGroup group, EWorkerType workerType, int count, EWaypointTaskType taskType, WorkerPath path, AnimationClip walkClip, Transform parent)
    {
        Assert.IsFalse(setup);
        for (int i = 0; i < count; i++)
        {
            var instanceData = new InstanceData(group, path, Instantiate(RandomUtils.GetRandom(prefabs[workerType]), parent), walkClip);
            GPUInstancerAPI.UpdateVariation(GPUICrowdManager, instanceData.Ref, SaturationBufferName, SaturationNotSelected);
            instanceData.TaskType = taskType;
            group.Instances.Add(instanceData);
            crowdPrefabs.Add(instanceData.Ref);
        }
    }

    public void Setup()
    {
        deckGroups = new List<InstanceGroup>();

        var walkClip = AnimationManager.Instance.WalkClip;
        var trans = transform;
        var paths = Deck.GetComponentsInChildren<WorkerPath>();
        foreach (var path in paths)
        {
            var group = new InstanceGroup(EWaypointTaskType.Normal);
            CreateWorkers(group, EWorkerType.Sailor, SailorInstancesOnDeckPerPath, EWaypointTaskType.Normal, path, walkClip, trans);
            CreateWorkers(group, EWorkerType.Officer, OfficerInstancesOnDeckPerPath, EWaypointTaskType.Normal, path, walkClip, trans);
            deckGroups.Add(group);
        }

        GPUInstancerAPI.RegisterPrefabInstanceList(GPUICrowdManager, crowdPrefabs);
        GPUInstancerAPI.InitializeGPUInstancer(GPUICrowdManager);
        setup = true;

        foreach (var group in deckGroups)
        {
            var dict = new Dictionary<Waypoint, float>();
            foreach (var workerInstance in group.Instances)
            {
                workerInstance.UpdateSpeed(false, false);
                var start = workerInstance.SetStartAndDestination(workerInstance.GetNewDestination());
                if (!dict.ContainsKey(start))
                {
                    dict[start] = 0f;
                }
                workerInstance.Delay = SameEntryDelay * dict[start];
                dict[start]++;
                workerInstance.WalkAnim();
                GPUInstancerAPI.UpdateTransformDataForInstance(GPUICrowdManager, workerInstance.Ref);
            }
        }
    }

    public void StartAnim(InstanceData workerInstance, IEnumerator anim)
    {
        Assert.IsNull(workerInstance.CurrentAnim);
        Assert.IsNull(workerInstance.CoroutineAnim);
        workerInstance.CurrentAnim = anim;
        workerInstance.CoroutineAnim = StartCoroutine(anim);
    }

    public void StopAnim(InstanceData workerInstance)
    {
        Assert.IsNotNull(workerInstance.CurrentAnim);
        StopCoroutine(workerInstance.CoroutineAnim);
        workerInstance.CurrentAnim = null;
        workerInstance.CoroutineAnim = null;
    }
}
