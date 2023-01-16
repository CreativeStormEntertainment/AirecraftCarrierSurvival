using UnityEngine;

public class CameraSwitchPoint : MonoBehaviour, IInteractive, IEnableable
{
    [SerializeField]
    private CameraManager cameraManager = null;
    [SerializeField]
    private ECameraView mode = ECameraView.Deck;

    private MeshRenderer render;
    private bool disabled;
    private ECameraInputType viewFlag;

    private void Awake()
    {
        render = GetComponent<MeshRenderer>();

        viewFlag = (ECameraInputType)(1 << (31 - (int)mode)) | ECameraInputType.ClickSwitch;
    }

    public float GetClickHoldTime()
    {
        return 1e9f;
    }

    public float GetHoverStayTime()
    {
        return 1e9f;
    }

    public void OnClickEnd(bool success)
    {

    }

    public void OnClickHold()
    {

    }

    public void OnClickStart()
    {
        if (!disabled && cameraManager.InputAvailable(viewFlag))
        {
            cameraManager.SwitchMode(mode);
        }
    }

    public void OnHoverEnter()
    {
        render.enabled = !disabled && cameraManager.InputAvailable(viewFlag);
    }

    public void OnHoverExit()
    {
        render.enabled = false;
    }

    public void OnHoverStay()
    {

    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
        if (!enable)
        {
            OnHoverExit();
        }
    }
}
