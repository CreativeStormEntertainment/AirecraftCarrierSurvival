using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSettings : MonoBehaviour
{
    private MenuSettingsButton currentButton;

    public void SelectButton(MenuSettingsButton settingButton)
    {
        if (currentButton != null)
        {
            currentButton.SetSelected(false);
        }
        currentButton = settingButton;
        currentButton.SetSelected(true);
    }
}
