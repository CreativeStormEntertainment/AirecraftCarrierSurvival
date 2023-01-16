using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Assertions;

public class GlobalResourceManager : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Data/Load low resources effects", false, 205)]
    static void LoadLowResourcesEffects()
    {
        var managers = GameObject.Find("Managers");
        var effMan = managers.GetComponent<EffectManager>();

        var lines = TSVUtils.LoadData(@"Assets\Data\TSV\LowResourcesEffects.tsv");
        var indicesList = new List<List<int>>();
        Assert.IsTrue(lines.Count == 2);
        foreach (var line in lines)
        {
            Assert.IsTrue(line.Length == 2);
            indicesList.Add(TSVUtils.GetEffectIndices(effMan, line[1]));
        }

        var globResMan = managers.GetComponent<GlobalResourceManager>();
        Undo.RecordObject(globResMan, "Loaded low resources effects");
        globResMan.LowResourcesData.EffectIndices = indicesList[0];
        globResMan.CriticalResourcesData.EffectIndices = indicesList[1];
        EditorUtility.SetDirty(globResMan);
        EditorSceneManager.MarkSceneDirty(globResMan.gameObject.scene);
    }
#endif

    public static GlobalResourceManager Instance;

    public int BaseSuppliesAmount;

    public LowResData LowResourcesData = new LowResData(.5f);
    public LowResData CriticalResourcesData = new LowResData(.25f);

    [SerializeField]
    private int currentSuppliesAmount;

    private CommandProducer producer = new CommandProducer();

    public int CurrentSuppliesAmount
    {
        get => currentSuppliesAmount;
        set
        {
            currentSuppliesAmount = Mathf.Clamp(value, 0, BaseSuppliesAmount);
            float percent = currentSuppliesAmount / BaseSuppliesAmount;
            if (percent > LowResourcesData.ResLevel)
            {
                DeactivateLowResourcesEffects();
                DeactivateCriticalResourcesEffects();
            }
            else if (percent > CriticalResourcesData.ResLevel)
            {
                ActivateLowResourcesEffects();
                DeactivateCriticalResourcesEffects();
            }
            else
            {
                ActivateLowResourcesEffects();
                ActivateCriticalResourcesEffects();
            }
        }
    }

    public int CurrentCommandAmount
    {
        get => producer.CurrentAmount;
        set
        {
            Assert.IsTrue(value >= 0);
            producer.CurrentAmount = value;
        }
    }
    public int ProduceCommandCount
    {
        get => producer.ProduceCount;
        set
        {
            Assert.IsTrue(value >= 0);
            producer.ProduceCount = value;
        }
    }

    public int ProduceCommandTime
    {
        get => producer.ProduceTime;
        set
        {
            Assert.IsTrue(value > 0);
            producer.ProduceTime = value;
        }
    }

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
        producer = new CommandProducer();
    }

    void Start()
    {
        var effMan = EffectManager.Instance;
        LowResourcesData.GameplayInit(effMan);
        CriticalResourcesData.GameplayInit(effMan);
        CurrentSuppliesAmount = BaseSuppliesAmount;
    }

    void ActivateLowResourcesEffects()
    {
        if (!LowResourcesData.IsActive)
        {
            LowResourcesData.Effects.ForEach(effect => EffectManager.Instance.OnEffectStart(effect));
            LowResourcesData.IsActive = true;
        }
    }

    void DeactivateLowResourcesEffects()
    {
        if (LowResourcesData.IsActive)
        {
            LowResourcesData.Effects.ForEach(effect => EffectManager.Instance.OnEffectFinish(effect));
            LowResourcesData.IsActive = false;
        }
    }

    void ActivateCriticalResourcesEffects()
    {
        if (!CriticalResourcesData.IsActive)
        {
            CriticalResourcesData.Effects.ForEach(effect => EffectManager.Instance.OnEffectStart(effect));
            CriticalResourcesData.IsActive = true;
        }
    }

    void DeactivateCriticalResourcesEffects()
    {
        if (CriticalResourcesData.IsActive)
        {
            CriticalResourcesData.Effects.ForEach(effect => EffectManager.Instance.OnEffectFinish(effect));
            CriticalResourcesData.IsActive = false;
        }
    }
}
