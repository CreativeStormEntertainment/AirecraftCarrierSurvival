using UnityEngine;
using UnityEngine.UI;

public class CarrierPurchase : MonoBehaviour
{
    public Button CarrierButton => carrierButton;
    public GameObject DisableObj => disabled;

    [SerializeField]
    private ECarrierType type = ECarrierType.CV3;
    [SerializeField]
    private Button buyButton = null;
    [SerializeField]
    private Text costField = null;
    [SerializeField]
    private int cost = 1;

    [SerializeField]
    private GameObject disabled = null;
    [SerializeField]
    private Button carrierButton = null;

    public void Setup(bool active)
    {
        disabled.SetActive(true);
        if (active)
        {
            costField.text = cost.ToString();
            buyButton.onClick.AddListener(OnClicked);

            gameObject.SetActive(true);

            Refresh();
        }
    }

    public void Refresh()
    {
        buyButton.interactable = IntermissionManager.Instance.CurrentUpgradePoints >= cost;
    }

    private void OnClicked()
    {
        IntermissionManager.Instance.ShowUpgradePopup(false, new UpgradePopupData(cost, false, OnBought), true);
    }

    private void OnBought()
    {
        IntermissionManager.Instance.FireCarrierBought(type);
        SaveManager.Instance.Data.IntermissionData.AvailableCarriers |= (1 << (int)type);
        carrierButton.interactable = true;
        disabled.SetActive(false);
        gameObject.SetActive(false);
    }
}
