using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSettingsButton : ChooseMainMenuButtons
{
    [SerializeField]
    private GameObject panel = null;
    [SerializeField]
    private GameObject selected = null;
    [SerializeField]
    private MenuSettings menuSettings = null;

    private void Start()
    {
        button.onClick.AddListener(() => menuSettings.SelectButton(this));
    }

    public void SetSelected(bool select)
    {
        panel.SetActive(select);
        selected.SetActive(select);
        button.interactable = !select;
        if (select)
        {
            chooseButton.color = showImage;
            chooseButton.sprite = RibbonButton;
            text.color = selectedText;
        }
        else
        {
            chooseButton.color = hideImage;
            chooseButton.sprite = null;
            text.color = normalText;
            if (arrow != null)
            {
                arrow.color = hideArrow;
            }
        }
    }
}
