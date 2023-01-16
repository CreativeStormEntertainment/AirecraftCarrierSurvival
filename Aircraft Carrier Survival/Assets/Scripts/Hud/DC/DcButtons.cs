using UnityEngine;
using UnityEngine.UI;

public class DcButtons : MonoBehaviour, IEnableable
{
    public Button Button => button;
    public bool IsFlood => flood;

    [SerializeField]
    private Button button = null;

    [SerializeField]
    private Image image = null;

    [SerializeField]
    private Sprite pressedSprite = null;

    [SerializeField]
    private bool flood = false;
    private Sprite normalSprite;

    private void Awake()
    {
        normalSprite = image.sprite;
        button.onClick.AddListener(() => DamageControlManager.Instance.OnDCButtonClicked(this));
    }

    public void SetEnable(bool enable)
    {
        enabled = enable;
    }

    public void Press()
    {
        button.enabled = false;
        image.sprite = pressedSprite;
    }

    public void Stop()
    {
        image.sprite = normalSprite;
        if (enabled)
        {
            button.enabled = true;
        }
    }
}
