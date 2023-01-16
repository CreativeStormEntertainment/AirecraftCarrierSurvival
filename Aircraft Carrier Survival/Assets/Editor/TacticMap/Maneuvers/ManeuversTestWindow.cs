using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ManeuversTestWindow : EditorWindow
{
    private const int MaxPlayerManeuvers = 5;

    private readonly List<EnemyManeuverData> enemyManeuvers = new List<EnemyManeuverData>();
    private readonly List<PlayerManeuverData> playerManeuvers = new List<PlayerManeuverData>();
    private bool data;
    private FightSquadronData neededSquadrons;
    private AttackParametersData playerData;
    private List<AttackParametersData> playerValuesList;
    private AttackParametersData enemyData;
    private CasualtiesData casualtiesData = new CasualtiesData();

    private CultureInfo loc = new CultureInfo("en-us");

    [MenuItem("Tools/Manuevers/Editor", false, 201)]
    private static void ShowWindow()
    {
        GetWindow<ManeuversTestWindow>().Show();
    }

    public ManeuversTestWindow()
    {
        for (int i = 0; i < MaxPlayerManeuvers; i++)
        {
            playerManeuvers.Add(null);
        }
        enemyManeuvers.Add(null);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Player maneuvers");
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < playerManeuvers.Count; i++)
        {
            playerManeuvers[i] = (PlayerManeuverData)EditorGUILayout.ObjectField(playerManeuvers[i], typeof(PlayerManeuverData), true);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(8f);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Enemy maneuvers", GUILayout.MaxWidth(105f));
        //EditorGUILayout.Space(30f);
        if (GUILayout.Button("Add new enemy slot", GUILayout.MaxWidth(135f)))
        {
            enemyManeuvers.Add(null);
        }
        bool enabled = GUI.enabled;
        GUI.enabled = GUI.enabled && enemyManeuvers.Count > 1;
        if (GUILayout.Button("Remove last enemy slot", GUILayout.MaxWidth(145f)))
        {
            enemyManeuvers.RemoveAt(enemyManeuvers.Count - 1);
        }
        GUI.enabled = enabled;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < enemyManeuvers.Count; i++)
        {
            if (i % 5 == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
            var data = (EnemyManeuverData)EditorGUILayout.ObjectField(enemyManeuvers[i], typeof(EnemyManeuverData), true, GUILayout.MaxWidth(293.5f));
            enemyManeuvers[i] = data;
        }
        EditorGUILayout.EndHorizontal();
        bool hasPlayerManeuver = false;
        bool hasEnemyManeuver = false;
        foreach (var maneuver in playerManeuvers)
        {
            if (maneuver != null)
            {
                hasPlayerManeuver = true;
                break;
            }
        }
        foreach (var maneuver in enemyManeuvers)
        {
            if (maneuver != null)
            {
                hasEnemyManeuver = true;
                break;
            }
        }
        GUILayout.BeginHorizontal();
        GUI.enabled = GUI.enabled && hasPlayerManeuver && hasEnemyManeuver;
        if (GUILayout.Button("Calculate", GUILayout.MaxWidth(100f)))
        {
            var notnullEnemyManeuvers = enemyManeuvers.Where((x) => x != null).ToList();
            var durabilities = new List<int>();
            foreach (var man in notnullEnemyManeuvers)
            {
                durabilities.Add(man.Durability);
            }
            ManeuverCalculator.Calculate(playerManeuvers, notnullEnemyManeuvers, durabilities, -1, out neededSquadrons, out playerData, out playerValuesList, out enemyData, out casualtiesData,
                ECalculateType.Real, 18, 6, 10, 0, 0, false, 0, 0);
            data = true;
        }
        if (!hasPlayerManeuver)
        {
            GUILayout.Label("Add player maneuvers", GUILayout.MaxWidth(130f));
        }
        if (!hasEnemyManeuver)
        {
            GUILayout.Label("Add enemy maneuvers", GUILayout.MaxWidth(135f));
        }
        GUILayout.EndHorizontal();
        GUI.enabled = enabled;
        if (data && neededSquadrons != null)
        {
            string text = "Needs: ";
            string pref = "";
            text += Needed(neededSquadrons.Bombers, " bomber", ref pref);
            text += Needed(neededSquadrons.Fighters, " fighter", ref pref);
            text += Needed(neededSquadrons.Torpedoes, " torpedo", ref pref);
            text += ".";
            EditorGUILayout.LabelField(text);
            int integerCount = Mathf.Max(IntegerCount(playerData.Attack), IntegerCount(playerData.Defense), IntegerCount(enemyData.Attack), IntegerCount(enemyData.Defense)) - 1;
            int fractialCount = Mathf.Max(SimpleFractialCount(playerData.Attack), SimpleFractialCount(playerData.Defense), SimpleFractialCount(enemyData.Attack), SimpleFractialCount(enemyData.Defense));
            text = new string('#', integerCount) + "0" + (fractialCount > 0 ? ".0" : "") + "}";
            string text2 = "{0," + Mathf.Max(integerCount, 37) + ":" + text;
            text = "{0," + integerCount + ":" + text;

            EditorGUILayout.LabelField("Player attack:" + string.Format(text2, playerData.Attack, loc) + "; defence:" + string.Format(text, playerData.Defense, loc));
            EditorGUILayout.LabelField("Enemy attack:" + string.Format(text2, enemyData.Attack, loc) + "; defence:" + string.Format(text, enemyData.Defense, loc));
            text = "Destroyed enemy ships:\t\t";
            string postfix = "";
            if (casualtiesData.EnemyDestroyedIndices.Count == 0)
            {
                text += "NONE";
            }
            else
            {
                foreach (var enemyIndex in casualtiesData.EnemyDestroyedIndices)
                {
                    text += postfix;
                    text += enemyIndex;
                    postfix = ", ";
                }
            }
            EditorGUILayout.LabelField(text);
            WritePlayerCasualties(casualtiesData.SquadronsDestroyed, "Destroyed", "\t");
            WritePlayerCasualties(casualtiesData.SquadronsBroken, "Broken", "\t\t");

            EditorGUILayout.Space(8f);
            for (int i = 0; i < playerManeuvers.Count; i++)
            {
                var manev = playerManeuvers[i];
                if (manev != null)
                {
                    var values = playerValuesList[i];
                    EditorGUILayout.LabelField(manev.name + " - final attack: " + values.Attack.ToString("0.0", loc) + "; final defence: " + values.Defense.ToString("0.0", loc));
                }
            }
        }
    }

    private int IntegerCount(float value)
    {
        return ((int)value).ToString().Length;
    }

    private int SimpleFractialCount(float value)
    {
        return ((int)((value - (int)value) * 10f)) % 10;
    }

    private string Needed(int value, string type, ref string pref)
    {
        string result = "";
        if (value > 0)
        {
            result = pref + value + type;
            if (value > 1)
            {
                result += "s";
            }
            pref = ", ";
        }
        return result;
    }

    private void WritePlayerCasualties(Dictionary<EPlaneType, int> dict, string prefix, string postfix)
    {
        int bombers = dict[EPlaneType.Bomber];
        int fighters = dict[EPlaneType.Fighter];
        int torpedoes = dict[EPlaneType.TorpedoBomber];
        EditorGUILayout.LabelField(prefix + " player squadrons:" + postfix + bombers + (bombers == 1 ? " bomber;  " : " bombers; ") + fighters + (fighters == 1 ? " fighter;  " : " fighters; ") +
            torpedoes + (torpedoes == 1 ? " torpedo" : " torpedoes"));
    }
}
