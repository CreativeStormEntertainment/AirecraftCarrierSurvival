using System;
using UnityEngine.UI;

[Serializable]
public class PanelData
{
    public EIntermissionCategory Category;
    public Button PanelButton;
    public Panel Panel;

    public void SetActive(bool active)
    {
        PanelButton.interactable = !active;
        Panel.gameObject.SetActive(active);
    }
}
