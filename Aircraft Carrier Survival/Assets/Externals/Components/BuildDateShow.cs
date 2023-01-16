using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BuildDateShow : MonoBehaviour
{
    [SerializeField]
    private Text text = null;


#if !UNITY_EDITOR
     private void Awake()
    {
        text.text = File.ReadAllText(Application.dataPath.Remove(Application.dataPath.LastIndexOf('/') + 1) + "BuildDate.txt");
        Debug.Log($"[BUILD] {text.text}");
    }
#endif
}
