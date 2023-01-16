using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class IslandBuffUIElement : MonoBehaviour
{
    public GameObject ActiveImage => activeImage;
    public Image Image => image;

    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text airText = null;
    [SerializeField]
    private Text navyText = null;
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private GameObject activeImage = null;
    [SerializeField]
    private StudioEventEmitter emitter = null;

    private Color active = Color.white;
    private Color unactive = new Color(1f, 1f, 1f, .5f);

    [SerializeField]
    private OrderTooltip tooltip = null;

    public void Setup(EIslandBuff buff, int air, int navy, Sprite sprite, string time, EIslandBuffEffectParam param, bool unlocked)
    {
        button.onClick.AddListener(() =>
        {
            if (IslandsAndOfficersManager.Instance.StartBuffSetup(buff, true))
            {
                emitter.Play();
                UIManager.Instance.CurrentBuffUi.Setup(buff, air, navy, sprite, time, param, unlocked);
                IslandsAndOfficersManager.Instance.SetShowBuffPanel(false);
            }
        });

        airText.text = air.ToString();
        navyText.text = navy.ToString();
        if (tooltip != null)
        {
            tooltip.Setup(buff.ToString(), air, navy, time, param);
        }
        title.text = LocalizationManager.Instance.GetText(buff.ToString() + "Title");
        image.sprite = sprite;
        gameObject.SetActive(unlocked);
    }

    public void SetInteractable(bool isActive)
    {
        button.interactable = isActive;
        UpdateImage();
    }

    public void SetEnabled(bool enabled)
    {
        button.enabled = enabled;
        UpdateImage();
    }

    private void UpdateImage()
    {
        image.color = button.interactable && button.enabled ? active : unactive;
    }
}
