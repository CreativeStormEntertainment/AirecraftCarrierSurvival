using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Assertions;

public class WeatherManager : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Data/Load weather effects", false, 208)]
    static void LoadWeatherEffects()
    {
        var managers = GameObject.Find("Managers");
        var effMan = managers.GetComponent<EffectManager>();

        var weatherList = new List<WeatherData>();
        var lines = TSVUtils.LoadData(@"Assets\Data\TSV\Weather.tsv");
        foreach (var line in lines)
        {
            weatherList.Add(new WeatherData(effMan, line));
        }

        var weatherMan = managers.GetComponent<WeatherManager>();
        Undo.RecordObject(weatherMan, "Loaded weather effects");
        weatherMan.WeatherList = weatherList;
        EditorUtility.SetDirty(weatherMan);
        EditorSceneManager.MarkSceneDirty(weatherMan.gameObject.scene);
    }
#endif

    public static WeatherManager Instance;

    public List<WeatherData> WeatherList;
    [NonSerialized]
    public Dictionary<EWeatherType, List<EffectData>> WeatherEffects = new Dictionary<EWeatherType, List<EffectData>>();

//#warning setup current weather
    [SerializeField]
    private EWeatherType currentWeather = EWeatherType.None;

    public bool IsWeatherOkay
    {
        get;
        private set;
    } = true;

    public EWeatherType CurrentWeather
    {
        get => currentWeather;
        set
        {
            foreach (var effect in WeatherEffects[currentWeather])
            {
                EffectManager.Instance.OnEffectFinish(effect);
            }

            currentWeather = value;

            foreach (var effect in WeatherEffects[value])
            {
                EffectManager.Instance.OnEffectStart(effect);
            }
        }
    }

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    void Start()
    {
        var effMan = EffectManager.Instance;
        foreach (var data in WeatherList)
        {
            Assert.IsFalse(WeatherEffects.ContainsKey(data.Type));
            data.GameplayInit(effMan);
            WeatherEffects[data.Type] = data.Effects;
        }
    }
}
