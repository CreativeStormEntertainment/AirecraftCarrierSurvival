using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Assertions;

public class EffectManager : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Data/Load effects", false, 201)]
    static void LoadEffects()
    {
        var lines = TSVUtils.LoadData(@"Assets\Data\TSV\Effects.tsv");
        var effects = new List<EffectData>();
        var reflections = new List<EffectReflectionData>();
        foreach (var line in lines)
        {
            effects.Add(new EffectData(line, reflections));
        }

        var effMan = GameObject.Find("Managers").GetComponent<EffectManager>();
        Undo.RecordObject(effMan, "Loading effects");
        effMan.Effects = effects;
        effMan.EffectReflections = reflections;
        EditorUtility.SetDirty(effMan);
        EditorSceneManager.MarkSceneDirty(effMan.gameObject.scene);
    }

    public int GetEffectIndex(string name)
    {
        int index = Effects.FindIndex((x) => x.EffectName == name);
        Assert.IsFalse(index == -1, name);
        return index;
    }
#endif

    public static EffectManager Instance;
    public float MinCrewEffectiveness = .5f;
    public float MaxCrewEffectiveness = 2f;
    [SerializeField]
    private float crewEffectiveness;
    public float CrewEffectiveness
    {
        get => crewEffectiveness;
        set
        {
            crewEffectiveness = Mathf.Clamp(value, MinCrewEffectiveness, MaxCrewEffectiveness);
        }
    }

    public List<EffectData> Effects = new List<EffectData>();

    public List<EffectReflectionData> EffectReflections;

    private Dictionary<object, EffectsCallData> effectCalls = new Dictionary<object, EffectsCallData>();

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        foreach (var data in EffectReflections)
        {
            var type = Type.GetType(data.ClassName);
            Assert.IsNotNull(type, data.ClassName);
            var instanceField = type.GetField("Instance");
            data.Instance = instanceField?.GetValue(null);
            data.Properties = new List<System.Reflection.PropertyInfo>();
            foreach (var propName in data.PropertyNames)
            {
                data.Properties.Add(type.GetProperty(propName));
            }
        }

        foreach (var effect in Effects)
        {
            effect.Init(EffectReflections);
        }
    }

    public void OnEffectStart(EffectData effect, object context = null)
    {
        effect.Effect(crewEffectiveness, context);
    }

    public void OnEffectFinish(EffectData effect, object context = null)
    {
        effect.InverseEffect(crewEffectiveness, context);
    }

    public void OnEffectsStart(object caller, List<EffectData> effects, float eff)
    {
        Assert.IsFalse(effectCalls.ContainsKey(caller));
        effectCalls[caller] = new EffectsCallData(effects, eff);
        foreach (var effect in effects)
        {
            effect.Effect(crewEffectiveness * eff);
        }
    }

    public void OnEffectsFinish(object caller)
    {
        Assert.IsTrue(effectCalls.ContainsKey(caller));
        var data = effectCalls[caller];
        effectCalls.Remove(caller);
        foreach (var effect in data.Effects)
        {
            effect.InverseEffect(crewEffectiveness * data.Effectiveness);
        }
    }
}
