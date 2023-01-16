using UnityEngine;
using UnityEngine.UI;

public class CameraButton : Button, IEnableable
{
    [SerializeField]
    private CameraViewsPanel panel = null;
    [SerializeField]
    private TopRightButton button = null;

    private bool originalDisabled;
    private bool selectedDisabled;

    protected override void Awake()
    {
        base.Awake();
        onClick.AddListener(panel.SelectNextView);
    }

    public void SetEnable(bool enable)
    {
        originalDisabled = !enable;
        enable = enable && !selectedDisabled;
        interactable = enable;
        button.SetEnable(enable);
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        SetEnable(!originalDisabled);
    }
}
