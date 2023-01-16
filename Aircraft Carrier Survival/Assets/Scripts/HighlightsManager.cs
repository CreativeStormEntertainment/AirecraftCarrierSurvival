using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class HighlightsManager : MonoBehaviour
{
    public static readonly string Path = "./highlights.txt";
    public static readonly string CloseString = "####";

    public static HighlightsManager Instance;

    [SerializeField]
    private List<Transform> categories = null;
    [SerializeField]
    private StudioEventEmitter emitter = null;

    private List<SortedDictionary<string, GameObject>> highlights;

    private List<HighlightData> queue;
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Highlights/Cache", false, 203)]
    private static void CacheHighlights()
    {
        var highlights = GambitUtils.SceneUtils.FindObjectOfType<HighlightsManager>();
        highlights.SetupHighlights();

        var text = "";
        foreach (var category in highlights.highlights)
        {
            foreach (var highlight in category.Keys)
            {
                text += highlight;
                text += "\n";
            }
            text += CloseString;
            text += "\n";
        }
        System.IO.File.WriteAllText(Path, text);
    }
#endif
    private void Awake()
    {
        Instance = this;

        queue = new List<HighlightData>();

        SetupHighlights();

        enabled = false;
    }

    private void Update()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            var data = queue[i];
            data.Time -= Time.unscaledDeltaTime;
            if (data.Time <= 0f)
            {
                data.Object.SetActive(true);
                
                TryActivateBindHighlightElements(data.Object);
                
                queue.RemoveAt(i--);
            }
        }
        if (queue.Count == 0)
        {
            enabled = false;
        }
    }

    public void SetHighlight(int category, string highlight, float time)
    {
        if (category == -1)
        {
            foreach (var dict in highlights)
            {
                foreach (var go in dict.Values)
                {
                    go.SetActive(false);
                    TryDeactivateBindHighlightElements(go);
                }
            }
            queue.Clear();
            enabled = false;
        }
        else
        {
            if (!emitter.IsPlaying())
            {
                emitter.Play();
            }
            var obj = highlights[category][highlight];
            if (time > 0f)
            {
                queue.Add(new HighlightData(time, obj));
            }
            else
            {
                obj.SetActive(true);
                TryActivateBindHighlightElements(obj);
            }
            enabled = true;
        }
    }

    private void TryDeactivateBindHighlightElements(GameObject go)
    {
        var bindHighlightElement = go.GetComponent<BindHighlightElement>();
        if (bindHighlightElement != null)
        {
            bindHighlightElement.DeactivateHighlight();
        }
    }

    private void TryActivateBindHighlightElements(GameObject go)
    {
        var bindHighlightElement = go.GetComponent<BindHighlightElement>();
        if (bindHighlightElement != null)
        {
            bindHighlightElement.ActivateHighlight();
        }
    }

    private void SetupHighlights()
    {
        highlights = new List<SortedDictionary<string, GameObject>>();
        foreach (var category in categories)
        {
            var dict = new SortedDictionary<string, GameObject>();
            foreach (Transform highlight in category)
            {
                dict.Add(highlight.name, highlight.gameObject);
                highlight.gameObject.SetActive(false);
            }

            highlights.Add(dict);
        }
    }
}