using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveStepObject : MonoBehaviour
{
    [SerializeField]
    private Text text = null;
    //[SerializeField]
    //private Image completedBoxImage = null;
    //[SerializeField]
    //private Image completedBackgroundImage = null;
    //[SerializeField]
    //private Image failedBackgroundImage = null;
    [SerializeField]
    private RectTransform textTransform = null;
    [SerializeField]
    private RectTransform mainTransform = null;

    [SerializeField]
    private Image checkboxImage = null;
    [SerializeField]
    private Sprite completedCheckbox = null;
    [SerializeField]
    private Sprite failedCheckbox = null;

    public ObjectiveStepObject Setup(string stepText, bool hidden)
    {
        var loc = LocalizationManager.Instance;
        text.text = loc.GetText(stepText);
        RebuildText();
        //failedBackgroundImage.enabled = false;
        //completedBackgroundImage.enabled = false;
        //completedBoxImage.enabled = false;
        if (hidden)
        {
            gameObject.SetActive(false);
        }
        return this;
    }

    public void SetStepState(bool completed)
    {
        if (completed)
        {
            checkboxImage.sprite = completedCheckbox;
            //completedBackgroundImage.enabled = true;
            //completedBoxImage.enabled = true;
        }
        else
        {
            checkboxImage.sprite = failedCheckbox;
            //failedBackgroundImage.enabled = true;
        }
    }

    public void RebuildText()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(textTransform);
        mainTransform.sizeDelta = new Vector2(mainTransform.sizeDelta.x, textTransform.rect.height);
    }

}
