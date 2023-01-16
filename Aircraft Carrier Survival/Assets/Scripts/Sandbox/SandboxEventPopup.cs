using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SandboxEventPopup : MonoBehaviour
{
    public event Action<SandboxEventConsequence> ButtonClicked = delegate { };

    [SerializeField]
    private Text fluffDescription = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private List<ConsequenceButton> buttons = null;

    private void Start()
    {
        foreach (var button in buttons)
        {
            button.OnClick += OnButtonClicked;
        }
    }

    public bool Setup(SandboxEvent sandboxEvent)
    {
        bool canShow = false;
        if (sandboxEvent.Sprite != null)
        {
            image.sprite = sandboxEvent.Sprite;
        }
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
        }
        for (int i = 0; i < sandboxEvent.Consequences.Count; i++)
        {
            if (buttons[i].Setup(sandboxEvent.Consequences[i]))
            {
                canShow = true;
            }
        }
        fluffDescription.text = LocalizationManager.Instance.GetText("Incident_" + (sandboxEvent.EventIndex + 1).ToString("00") + "_Fluff");
        gameObject.SetActive(canShow);
        if (canShow)
        {
            HudManager.Instance.OnPausePressed(false);
        }
        return canShow;
    }

    private void OnButtonClicked(SandboxEventConsequence consequence)
    {
        gameObject.SetActive(false);
        HudManager.Instance.PlayLastSpeed();
        ButtonClicked(consequence);
    }
}
