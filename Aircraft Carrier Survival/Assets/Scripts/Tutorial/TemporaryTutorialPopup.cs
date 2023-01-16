using UnityEngine;
using UnityEngine.UI;

public class TemporaryTutorialPopup : MonoBehaviour
{
    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private RectTransform blocker = null;
    [SerializeField]
    private Image blockerImage = null;

    private Transform parent;
    private Transform button;

    private bool canBeUsed = false;

    private void Start()
    {
        highlight.SetActive(true);
        button = highlight.transform.parent;
        parent = button.parent;
        button.SetParent(blocker);
        blockerImage.enabled = true;
        canBeUsed = true;
    }

    public void Click()
    {
        if (canBeUsed)
        {
            highlight.SetActive(false);
            gameObject.SetActive(false);
            SaveManager.Instance.TransientData.HighlightHelp = false;
            button.SetParent(parent);
            button.SetAsFirstSibling();
            blockerImage.enabled = false;
            canBeUsed = false;
        }
    }
}
