using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;

public class TacticalFightSpawner : MonoBehaviour
{
    public bool IsLand;
    public bool IsClouded;
    public List<TacticalFightEnemyData> ListOfSpawningEnemies;

    Color lastSettedColor;
    string lastSettedText;

    public bool UpdateSpawner()
    {
        string chosentext;

        if (IsLand)
        {
            GetComponent<Image>().color = Color.green;
        }
        else
        {
            GetComponent<Image>().color = Color.blue;
        }

        if (ListOfSpawningEnemies.Count > 0)
        {
            chosentext = ListOfSpawningEnemies.Count.ToString();
        }
        else
        {
            chosentext = "";
        }

        if (lastSettedColor != GetComponent<Image>().color || lastSettedText != chosentext)
        {
            lastSettedColor = GetComponent<Image>().color;
            lastSettedText = chosentext;
           // GetComponentInChildren<Text>().text = lastSettedText;
            return true;
        }
        else
            return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();

        style.fontSize = 35;
        style.fontStyle = FontStyle.Bold;
        Handles.Label(transform.position - new Vector3(10, -20, 0), lastSettedText, style);

        //Texture2D texture = new Texture2D(40, 40);
        //texture.SetPixel(1, 1, lastSettedColor);
        //texture.wrapMode = TextureWrapMode.Repeat;
        //texture.Apply();
        //Handles.Label(transform.position - new Vector3(15,-20,0), texture);

    }
#endif
}
