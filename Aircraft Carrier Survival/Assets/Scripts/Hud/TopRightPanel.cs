using System.Collections.Generic;
using UnityEngine;

public class TopRightPanel : MonoBehaviour
{
    [SerializeField]
    private List<TopRightButton> buttons = null;
    [SerializeField]
    private Expandable expandable = null;

    private void Start()
    {
        foreach (var button in buttons)
        {
            if (button.ToggleObject != null)
            {
                button.Button.onClick.AddListener(() =>
                {
                    HideOthers(button.ToggleObject);
                });
            }
            else
            {
                button.Button.onClick.AddListener(() => HideOthers());
            }
        }
    }

    public void HideOthers(ToggleObject obj = null)
    {
        foreach (var button in buttons)
        {
            if (button.ToggleObject && obj != button.ToggleObject)
            {
                button.ToggleObject.SetShow(false);
            }
        }
        if (obj != null)
        {
            expandable.Hide();
        }
    }
}
