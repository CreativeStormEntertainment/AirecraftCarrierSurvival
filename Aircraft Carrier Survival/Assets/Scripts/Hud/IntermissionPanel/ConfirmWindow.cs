using UnityEngine;
using UnityEngine.UI;

public class ConfirmWindow : MonoBehaviour
{
    public EDeckUpgradeType Mode { get; private set; }

    public Text UpgradePoints = null;

    [SerializeField]
    private Button backButton = null;
    [SerializeField]
    private Button confirmButton = null;
    [SerializeField]
    private string baseTextID = null;
    [SerializeField]
    private Text textToSet = null;
    [SerializeField]
    private Image iconToSet = null;

    private string baseText = "";

    private IntermissionPanel intermissionPanel;

    public void Setup()
    {
        intermissionPanel = IntermissionPanel.Instance;
        backButton.onClick.AddListener(Back);
        confirmButton.onClick.AddListener(Confirm);
        baseText = LocalizationManager.Instance.GetText(baseTextID);
    }

    private void Back()
    {
        gameObject.SetActive(false);
        IntermissionPanel.Instance.ActivateClickBlocker(false);
        intermissionPanel.StopUpgrade(Mode);
    }

    private void Confirm()
    {
        gameObject.SetActive(false);
        intermissionPanel.ActivateClickBlocker(false);
        intermissionPanel.UpdateStatisticsWindow();

        intermissionPanel.ConfirmUpgrade(Mode);
    }

    public void Setup(EDeckUpgradeType mode, string text, Sprite icon)
    {
        gameObject.SetActive(true);
        Mode = mode;
        iconToSet.sprite = icon;
        textToSet.text = baseText+" <b>" +LocalizationManager.Instance.GetText(text)+"</b>";
    }
}
