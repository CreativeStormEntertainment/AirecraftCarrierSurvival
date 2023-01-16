using UnityEngine;

public class OfficerButton : GameButton
{
    public GameObject Outline;

    private Transform trans;
    private Officer officer;
    [SerializeField]
    private SpriteRenderer backgroundImage = null;
    [SerializeField]
    private SpriteRenderer pressed = null;

    private Vector3 normalScale;

    private bool originalDisabled;
    private bool selectedDisabled;
    private float offsetX;
    protected override void Awake()
    {
        base.Awake();
        trans = transform;

        Outline.SetActive(false);

        CameraManager.Instance.ViewChanged += OnViewChanged;
        switch (SaveManager.Instance.Data.SelectedAircraftCarrier)
        {
            case ECarrierType.CV3:
                offsetX = -10f;
                break;
            case ECarrierType.CV5:
                offsetX = -7.2f;
                break;
            case ECarrierType.CV9:
                offsetX = -7.5f;
                break;
        }
    }

    private void LateUpdate()
    {
        trans.position = new Vector3(offsetX, officer.Transform.position.y + 2f, officer.Transform.position.z);
    }

    public override void OnClickStart()
    {
        if (!disabled)
        {
            pressed.gameObject.SetActive(true);
        }
    }

    public override void OnClickEnd(bool success)
    {
        pressed.gameObject.SetActive(false);
        if (!disabled)
        {
            if (success)
            {
                VoiceSoundsManager.Instance.PlaySelect(officer.Voice);

                var islandMan = IslandsAndOfficersManager.Instance;
                islandMan.PlayEvent(EIslandUIState.Click);
                islandMan.SelectedOfficer = officer;
            }
        }
    }

    public override void SetEnable(bool enable)
    {
        originalDisabled = !enable;
        enable = enable && !selectedDisabled;
        base.SetEnable(enable);
        if (!enable)
        {
            pressed.gameObject.SetActive(false);
        }
    }

    public void Setup(Officer officer)
    {
        this.officer = officer;
        officer.Button = this;
        trans.position = new Vector3(-10f, officer.Transform.position.y + 2f, officer.Transform.position.z);
        backgroundImage.sprite = officer.CircleSprite;

        normalScale = trans.localScale;
        //List<Sprite> images = null;
        //if (officer.CanBeAssignedToCategory(EIslandRoomCategory.AIR))
        //{
        //    if (officer.CanBeAssignedToCategory(EIslandRoomCategory.SHIP))
        //    {
        //        images = IslandsAndOfficersManager.Instance.BothOfficerBtn;
        //    }
        //    else
        //    {
        //        images = IslandsAndOfficersManager.Instance.AirOfficerBtn;
        //    }
        //}
        //else
        //{
        //    images = IslandsAndOfficersManager.Instance.NavyOfficerBtn;
        //}
        //backgroundImage.sprite = images[0];
        //outlineImage.sprite = images[1];
        //arrowImage.sprite = images[2];
    }

    public void SetSelected(bool select)
    {
        Outline.SetActive(select);
        trans.localScale = select ? normalScale * 1.5f : normalScale;
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        SetEnable(!originalDisabled);
    }

    private void OnViewChanged(ECameraView view)
    {
        gameObject.SetActive(view == ECameraView.Island);
    }
}
