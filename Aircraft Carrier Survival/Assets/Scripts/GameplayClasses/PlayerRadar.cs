using UnityEngine;
using UnityEngine.UI;

public class PlayerRadar : ToggleObject
{
    [SerializeField]
    private Sprite[] playerDetectedSprites = new Sprite[0];
    [SerializeField]
    private Image playerDetectedImage = null;
    [SerializeField]
    private Image eyeImage = null;

    [SerializeField]
    private RectTransform radarLight = null;
    [SerializeField]
    private float radarLightSpeed = 10f;
    [SerializeField]
    private Color undetectedColor = new Color(1f, 1f, 1f, 0.7f);

    [SerializeField]
    private StateTooltip detectedTooltip = null;

    private bool started;
    private bool canvasDisabled;

    private void Start()
    {
        started = true;
        SectionRoomManager.Instance.GeneratorsStateChanged += OnGeneratorsStateChanged;
    }

    private void OnEnable()
    {
        if (started && !canvasDisabled)
        {
            BackgroundAudio.Instance.PlayEvent(EMainSceneUI.RadarOn);
        }
        started = true;
        canvasDisabled = false;
    }

    private void OnDisable()
    {
        if (gameObject.activeSelf)
        {
            canvasDisabled = true;
        }
        else
        {
            canvasDisabled = false;
            BackgroundAudio.Instance.PlayEvent(EMainSceneUI.RadarOff);
        }
    }

    private void Update()
    {
        if (Time.timeScale > 0f)
        {
            radarLight.Rotate(new Vector3(0f, 0f, Time.unscaledDeltaTime * radarLightSpeed));
        }
    }

    public override void SetShow(bool show)
    {
        base.SetShow(show && SectionRoomManager.Instance.GeneratorsAreWorking);
    }

    public void SetPlayerDetected(bool detected)
    {
        if (detected)
        {
            eyeImage.color = Color.white;
            playerDetectedImage.sprite = playerDetectedSprites[1];
            detectedTooltip.ChangeState(ETooltipRadar.Detected);
        }
        else
        {
            eyeImage.color = undetectedColor;
            playerDetectedImage.sprite = playerDetectedSprites[0];
            detectedTooltip.ChangeState(ETooltipRadar.Undetected);
        }
    }

    private void OnGeneratorsStateChanged(bool active)
    {
        if (!active)
        {
            SetShow(false);
        }
    }
}
