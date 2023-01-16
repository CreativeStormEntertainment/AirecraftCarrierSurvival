using UnityEngine;
using UnityEngine.UI;

public class SandboxEventResultPopup : MonoBehaviour, IPopupPanel
{
    public EWindowType Type => EWindowType.SandboxPopup;

    [SerializeField]
    private Text desc = null;
    [SerializeField]
    private Button button = null;

    private void Awake()
    {
        button.onClick.AddListener(Hide);
    }

    public void Setup(SandboxEventConsequence consequence)
    {
        string descIndex = "";
        if (consequence.PossibleCosts.Count > 1)
        {
            switch (consequence.DrawnCostIndex)
            {
                case 0:
                    descIndex = "A";
                    break;
                case 1:
                    descIndex = "B";
                    break;
                case 2:
                    descIndex = "C";
                    break;
            }
        }
        desc.text = LocalizationManager.Instance.GetText("Incident_" + (consequence.EventIndex + 1).ToString("00") + "_Result_" + (consequence.ConsequenceIndex + 1).ToString("00") + descIndex);
        gameObject.SetActive(true);
        var hudMan = HudManager.Instance;
        hudMan.PopupShown(this);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        var hudMan = HudManager.Instance;
        hudMan.PopupHidden(this);
    }
}
