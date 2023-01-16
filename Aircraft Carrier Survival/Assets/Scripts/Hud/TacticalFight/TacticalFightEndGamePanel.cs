using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TacticalFightEndGamePanel : MonoBehaviour
{
    [SerializeField]
    Text endTitle = null;
    [SerializeField]
    Text endDescription = null;
    Button closePanelButton;

    private void Awake()
    {
    }

    public void InitializeEndGamePanel(bool isGameWon, string message)
    {
        gameObject.SetActive(true);

        endDescription.text = message;

        if (isGameWon)
        {
            endTitle.text = "Game is Won !!!";
        }
        else
        {
            endTitle.text = "Game is Lost !!!";
        }
    }

    public void ClosePanelAction()
    {
        TacticalFightHudManager.Instance.PlayOnButtonClickClip();
        GameSceneManager.Instance.ChangeToAircraftModule();
        gameObject.SetActive(false);
    }
}
