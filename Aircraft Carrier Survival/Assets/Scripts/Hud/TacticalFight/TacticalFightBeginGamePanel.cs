using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TacticalFightBeginGamePanel : MonoBehaviour
{
    Button closePanelButton;

    private void Awake()
    {
        closePanelButton = GetComponentInChildren<Button>(true);
        closePanelButton.onClick.AddListener(ClosePanelAction);
    }

    public void ClosePanelAction()
    {
        TacticalFightHudManager.Instance.PlayOnButtonClickClip();
        TacticalFightManager.Instance.StartGame();
        transform.gameObject.SetActive(false);
    }

}
