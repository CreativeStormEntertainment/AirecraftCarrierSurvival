using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class ScenarioManager
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Data/Scenarios to binaries", false, 210)]
    static void BinScenarios()
    {
        var scenarioPaths = Directory.GetFiles(@"Assets\Data\TSV\Scenarios\", "*.tsv");
        var scenarios = new List<ScenarioData>();
        var evMan = GameObject.Find("Managers").GetComponent<EventManager>();
        foreach (var path in scenarioPaths)
        {
            scenarios.Add(new ScenarioData(evMan, TSVUtils.LoadData(path)));
        }
        BinUtils.SaveBinary(scenarios, @"Assets\Resources\Scenarios.bytes");
    }
#endif

    private static ScenarioManager instance;
    public static ScenarioManager Instance
    {
        get
        {
            return instance ?? (instance = new ScenarioManager());
        }
    }

    public List<ScenarioData> Scenarios;

    private ScenarioManager()
    {
        Scenarios = BinUtils.LoadBinaryTextAsset<List<ScenarioData>>("Scenarios");
        Assert.IsNotNull(Scenarios);
    }
}
