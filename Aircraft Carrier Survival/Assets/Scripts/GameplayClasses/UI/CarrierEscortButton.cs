using UnityEngine;
using UnityEngine.UI;

public class CarrierEscortButton : MonoBehaviour
{
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private bool isCarrierButton = false;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    public void DisableButton()
    {
        button.interactable = false;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnClick()
    {
        StrikeGroupManager.Instance.ActivateBoolSelectionSkill(EStrikeGroupActiveSkill.TemporaryCustomDefence, isCarrierButton);
        UIManager.Instance.StrikeGroupSelectionWindow.Hide();
    }
}
