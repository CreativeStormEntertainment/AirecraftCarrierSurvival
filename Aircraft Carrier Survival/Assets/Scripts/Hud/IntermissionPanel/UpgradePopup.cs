using System;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePopup : MonoBehaviour, IEscapeablePopup
{
    public event Action<bool> PopupClosed = delegate { };

    [SerializeField]
    private Text description = null;

    [SerializeField]
    private Button acceptButton = null;
    [SerializeField]
    private Button cancelButton = null;
    [SerializeField]
    private Toggle skipPopupButton = null;

    [SerializeField]
    private Text cost = null;

    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Sprite command = null;
    [SerializeField]
    private Sprite upgrade = null;
    [SerializeField]
    private Color commandColor = Color.yellow;
    [SerializeField]
    private Color upgradeColor = Color.green;

    [SerializeField]
    private string buyId = "";
    [SerializeField]
    private string unlockId = "";
    [SerializeField]
    private string upgradeId = "";

    private void Awake()
    {
        acceptButton.onClick.AddListener(() => Hide(true));
        cancelButton.onClick.AddListener(() => Hide(false));
        skipPopupButton.onValueChanged.AddListener((value) => SaveManager.Instance.Data.IntermissionData.SkipPopup = value);
    }

    public void Show(bool isUnlock, UpgradePopupData data)
    {
        var locMan = LocalizationManager.Instance;
        if (data.Command)
        {
            description.text = isUnlock ? locMan.GetText(unlockId) : locMan.GetText(buyId);
        }
        else
        {
            description.text =  locMan.GetText(upgradeId);
        }
        cost.text = data.Cost.ToString();
        icon.sprite = data.Command ? command : upgrade;
        cost.color = data.Command ? commandColor : upgradeColor;

        gameObject.SetActive(true);
        IntermissionManager.Instance.RegisterEscapeable(this);
    }

    public void Hide(bool success)
    {
        PopupClosed(success);
        gameObject.SetActive(false);
    }

    public bool OnEscape()
    {
        bool success = gameObject.activeSelf;
        Hide(false);
        return success;
    }
}
