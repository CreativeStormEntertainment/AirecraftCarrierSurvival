using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;

public class ReorderPrototypes : EditorWindow
{
    [MenuItem("Tools/Animations/Reorder prototypes", false, 203)]
    static void ShowWindow()
    {
        GetWindow<ReorderPrototypes>().Show();
    }

    GPUICrowdManager manager;
    List<int> order;
    string[] texts;
    GUIContent button;
    float popupWidth;
    float buttonWidth;

    private void OnGUI()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<GPUICrowdManager>();
            texts = new string[manager.prototypeList.Count];
            order = new List<int>();

            string biggestText = "";
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i] = manager.prototypeList[i].name;
                order.Add(i);
                if (texts[i].Length > biggestText.Length)
                {
                    biggestText = texts[i];
                }
            }
            var biggest = new GUIContent(biggestText);
            button = new GUIContent("Reorder");
            popupWidth = GUI.skin.button.CalcSize(biggest).x;
            buttonWidth = GUI.skin.button.CalcSize(button).x;
        }

        EditorGUILayout.LabelField("Set order");
        EditorGUILayout.BeginHorizontal();
        float width = 0f;
        for (int i = 0; i < order.Count; i++)
        {
            width += popupWidth;
            if ((width + popupWidth / 2f) > position.width)
            {
                width = popupWidth;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
            int newOrder = EditorGUILayout.Popup(order[i], texts, GUILayout.Width(popupWidth));
            if (newOrder != order[i])
            {
                int index = order.FindIndex((x) => x == newOrder);
                Utils.Swap(order, i, index);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space((position.width - buttonWidth) / 2f);
        if (GUILayout.Button(button, GUILayout.Width(buttonWidth)))
        {
            var list = new List<GPUInstancerPrototype>();
            Assert.IsTrue(order.Count == manager.prototypeList.Count);
            Assert.IsTrue(order.Distinct().Count() == order.Count);
            foreach (var index in order)
            {
                list.Add(manager.prototypeList[index]);
            }
            manager.prototypeList = list;
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}
