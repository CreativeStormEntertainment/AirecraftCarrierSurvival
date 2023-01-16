using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePanel : MonoBehaviour
{
    [SerializeField]
    private List<TimeButton> buttons = null;

    private TimeButton selectedButton;

    public void SetBlockSpeeds(bool block)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].SetBlock(block);
        }
        if (selectedButton != null)
        {
            selectedButton.SetBlock(false);
            selectedButton.SetSelected(true);
        }
    }

    public void SelectButton(int index)
    {
        if (index == 0 && buttons.Count < 4)
        {
            return;
        }
        if (selectedButton != null)
        {
            selectedButton.SetSelected(false);
        }
        int i = buttons.Count == 4 ? index : index - 1;
        selectedButton = buttons[i];
        selectedButton.SetSelected(true);
    }
}
