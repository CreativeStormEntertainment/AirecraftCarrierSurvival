using GambitUtils;
using UnityEngine;
using UnityEngine.UI;

public class ObjectivesPanel : MonoBehaviour
{
    [SerializeField]
    private ToggleObjectSounds sounds = null;
    [SerializeField]
    private RectTransform panel = null;

    private HudManager hudMan;
    private void Awake()
    {
        hudMan = HudManager.Instance;
        //sounds.enabled = false;
        //hudMan.StartCoroutineActionAfterFrames(SetPanel, 1);
    }

    private void OnEnable()
    {
        if (hudMan != null)
        {
            Rebuild();
        }
    }

    private void SetPanel()
    {
        gameObject.SetActive(false);
        sounds.enabled = true;
        //panel.anchoredPosition = new Vector2(0f, panel.anchoredPosition.y);
    }

    private void Rebuild()
    {
        hudMan.StartCoroutineActionAfterFrames(() => LayoutRebuilder.MarkLayoutForRebuild(panel), 1);
        hudMan.StartCoroutineActionAfterFrames(() => LayoutRebuilder.MarkLayoutForRebuild(panel), 2);
        hudMan.StartCoroutineActionAfterFrames(() => LayoutRebuilder.MarkLayoutForRebuild(panel), 3);
    }
}
