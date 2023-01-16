using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class VoiceSoundsManager : MonoBehaviour
{
#if UNITY_EDITOR
    private const string VoicePath = "Assets/GameplayAssets/ScriptableData/Voices";
    private const string VoiceEnumPath = "Assets/Scripts/Enums/EVoiceType.cs";
    private const string VoiceEnumStart = "\npublic enum EVoiceType\n{";
    private const string VoiceEnumEnd = "\n    DC = 200,\n}";

    private const string Admiral = "Admiral";
    private const string Officer = "Officer";
    private const string DC = "DC";

    [UnityEditor.MenuItem("Tools/Update voices")]
    private static void UpdateVoices()
    {
        var files = System.IO.Directory.GetFiles(VoicePath, "*.asset");
        var voiceDict = new Dictionary<string, VoiceEventData>();
        var admiralList = new List<string>();
        var officerList = new List<string>();

        foreach (var file in files)
        {
            var fileData = UnityEditor.AssetDatabase.LoadAssetAtPath<VoiceEventData>(file);
            if (fileData.name.StartsWith(Admiral))
            {
                admiralList.Add(fileData.name);
            }
            else if (fileData.name.StartsWith(Officer))
            {
                officerList.Add(fileData.name);
            }
            else
            {
                Assert.IsTrue(fileData.name == DC);
            }
            voiceDict[fileData.name] = fileData;
        }

        admiralList.Sort();
        officerList.Sort();

        var manager = FindObjectOfType<VoiceSoundsManager>();
        manager.admiralList = new List<VoiceEventData>();

        var content = new StringBuilder(VoiceEnumStart);
        for (int i = 0; i < admiralList.Count; i++)
        {
            Assert.IsTrue(admiralList[i] == (Admiral + (i + 1)));
            content.Append("\n    ");
            content.Append(admiralList[i]);
            content.Append(",");

            var voiceData = voiceDict[admiralList[i]];
            manager.admiralList.Add(voiceData);
        }

        manager.officerList = new List<VoiceEventData>();
        for (int i = 0; i < officerList.Count; i++)
        {
            Assert.IsTrue(officerList[i] == (Officer + (i + 1)));
            content.Append("\n    ");
            content.Append(officerList[i]);
            content.Append(" = ");
            content.Append(i + 100);
            content.Append(",");

            var voiceData = voiceDict[officerList[i]];
            manager.officerList.Add(voiceData);
        }
        content.Append(VoiceEnumEnd);
        if (voiceDict.TryGetValue(DC, out var data))
        {
            manager.dc = data;
        }

        System.IO.File.WriteAllText(VoiceEnumPath, content.ToString());

        UnityEditor.EditorUtility.SetDirty(manager);
    }
#endif

    public static VoiceSoundsManager Instance;

    [SerializeField]
    [HideInInspector]
    private List<VoiceEventData> admiralList = null;
    [SerializeField]
    [HideInInspector]
    private List<VoiceEventData> officerList = null;
    [SerializeField]
    [HideInInspector]
    private VoiceEventData dc = null;

    private Dictionary<EVoiceType, VoiceSoundData> sounds;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (sounds != null)
        {
            foreach (var data in sounds.Values)
            {
                data.Deinit();
            }
            sounds.Clear();
        }
    }

    public void Setup(EVoiceType admiral, int maxOfficers)
    {
        sounds = new Dictionary<EVoiceType, VoiceSoundData>();
        sounds[admiral] = new VoiceSoundData(admiralList[(int)admiral]);
        for (int i = 0; i < maxOfficers; i++)
        {
            sounds[EVoiceType.Officer1 + i] = new VoiceSoundData(officerList[i]);
        }
        sounds[EVoiceType.DC] = new VoiceSoundData(dc);
    }

    public void PlaySelect(EVoiceType type)
    {
        sounds[type].SelectEvent.Play();
    }

    public void PlayPositive(EVoiceType type)
    {
        sounds[type].PositiveEvent.Play();
    }

    public void PlayNegative(EVoiceType type)
    {
        sounds[type].NegativeEvent.Play();
    }
}
