using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrewUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEnableable
{
    public DragDrop DragDrop => dragDrop;
    public GameObject TimerObject => timerObject;
    public ECrewUnitState UnitState => unitState;

    public CrewManager CrewManager
    {
        get;
        private set;
    }

    public List<ECrewmanSpecialty> Skills
    {
        get;
        private set;
    }

    public bool CanBeHealed => UnitState == ECrewUnitState.Injured &&
        ((dragDrop.SlotBeforeDrag != null && dragDrop.SlotBeforeDrag.IsHealing) || (dragDrop.SlotBeforeDrag == null && dragDrop.CurrentSlot != null && dragDrop.CurrentSlot.IsHealing));

    public bool IsCalculable
    {
        get;
        set;
    }

    public SectionSegment Segment
    {
        get;
        set;
    }

    public int DeathTicks = 0;
    public int HealTicks = 0;

    [SerializeField]
    private Image sailorImage = null;
    [SerializeField]
    private GameObject highlightImage = null;
    [SerializeField]
    private DragDrop dragDrop = null;
    [SerializeField]
    private List<CrewManSpecialityTooltip> specialityTooltips = null;
    [SerializeField]
    private CrewManTooltip tooltip = null;
    [SerializeField]
    private ECrewUnitState unitState = ECrewUnitState.Healthy;
    [SerializeField]
    private GameObject needHelpMask = null;
    [SerializeField]
    private Image injuredMask = null;
    [SerializeField]
    private GameObject deadMask = null;
    [SerializeField]
    private GameObject cooldownObject = null;

    [SerializeField]
    private Text timer = null;
    [SerializeField]
    private Text minutesText = null;
    [SerializeField]
    private GameObject timerObject = null;
    [SerializeField]
    private GameObject deathPauseImage = null;
    [SerializeField]
    private GameObject deathUnpauseImage = null;
    [SerializeField]
    private GameObject needHelpImage = null;
    [SerializeField]
    private Button button = null;

    private List<GameObject> allObjects;

    private Sprite greyIcon;
    private bool highlight;
    private bool disabled;
    private bool originalDisabled;
    private bool selectedDisabled;

    public void SetEnable(bool enable)
    {
        originalDisabled = !enable;
        enable = enable && !selectedDisabled;
        disabled = !enable;
        dragDrop.SetEnable(enable);
        highlightImage.SetActive(enable && highlight);
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        SetEnable(!originalDisabled);
    }

    public void Setup(Canvas canvas, ItemSlot itemSlot, Sprite image, Sprite greyImage, CrewManager manager)
    {
        CrewManager = manager;
        sailorImage.sprite = image;
        greyIcon = greyImage;
        dragDrop.Canvas = canvas;
        dragDrop.CurrentSlot = itemSlot;
        dragDrop.CurrentSlot = itemSlot;
        dragDrop.Setup();
        tooltip.Unit = this;

        button.onClick.AddListener(SnapToSegment);

        allObjects = new List<GameObject>();
        allObjects.Add(needHelpMask);
        allObjects.Add(injuredMask.gameObject);
        allObjects.Add(deadMask);
        allObjects.Add(needHelpImage);
        allObjects.Add(deathPauseImage);
        allObjects.Add(deathUnpauseImage);

        unitState = ECrewUnitState.Healthy;
    }

    public void LoadData(CrewSaveData data)
    {
        SetState(data.Health);
        HealTicks = data.HealTicks;
        DeathTicks = data.DeathTicks;

        dragDrop.LoadData(ref data);
    }

    public CrewSaveData SaveData()
    {
        var result = new CrewSaveData();

        result.Health = unitState;
        result.HealTicks = HealTicks;
        result.DeathTicks = DeathTicks;

        dragDrop.SaveData(ref result);

        return result;
    }

    public void SetState(ECrewUnitState state, int startingTick = 0)
    {
        this.unitState = state;
        tooltip.State = state;
        HealTicks = 0;
        DeathTicks = startingTick;

        foreach (var obj in allObjects)
        {
            obj.SetActive(false);
        }

        var eventMan = EventManager.Instance;
        switch (state)
        {
            case ECrewUnitState.Healthy:
                TimerObject.SetActive(false);
                eventMan.RemoveCrewInjured(this);
                break;
            case ECrewUnitState.SoonToInjure:
                TimerObject.SetActive(true);
                needHelpMask.SetActive(true);
                needHelpImage.SetActive(true);
                break;
            case ECrewUnitState.Injured:
                TimerObject.SetActive(true);
                deathUnpauseImage.SetActive(true);
                injuredMask.gameObject.SetActive(true);
                eventMan.AddCrewInjured(this);
                break;
            case ECrewUnitState.Dead:
                sailorImage.sprite = greyIcon;
                TimerObject.SetActive(false);
                deadMask.SetActive(true);
                CrewManager.DisableCrew(this);
                eventMan.RemoveCrewInjured(this);
                eventMan.AddCrewDead(this);
                break;
        }
    }

    public void SetIsHealing(bool heal)
    {
        if (!heal)
        {
            HealTicks = 0;
        }
        deathPauseImage.SetActive(heal);
        deathUnpauseImage.SetActive(!heal);
    }

    public void UpdateHeal(float value)
    {
        injuredMask.fillAmount = 1f - value;
    }

    public void UpdateCooldown(bool set)
    {
        cooldownObject.SetActive(set);
        //cooldownImage.fillAmount = value;
    }

    public bool CanArtificalDrop()
    {
        return dragDrop.CurrentDepartment != null;
    }

    public void ArtificalDrop(ItemSlot slot)
    {
        dragDrop.ArtificalDrop(slot);
    }

    public void SetDepartment(DepartmentItem department)
    {
        dragDrop.SetDepartment(department);
    }

    public void SetSkill(List<ECrewmanSpecialty> skills)
    {
        CrewmanSpecialty crewmanSpecialty = GetComponent<CrewmanSpecialty>();
        crewmanSpecialty.SetSpecialties(skills);
        Skills = skills;
        for (int i = 0; i < skills.Count; i++)
        {
            specialityTooltips[i].Speciality = skills[i];
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight = true;
        highlightImage.SetActive(!disabled);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight = false;
        highlightImage.SetActive(false);
    }

    public void SetTime(int minutes)
    {
        minutes = Mathf.Max(minutes, 0);
        timer.text = "-" + (minutes / 60).ToString("00");
        minutesText.text = (minutes % 60).ToString("00");
    }

    private void SnapToSegment()
    {
        CrewManager.SetShow(false);
        var camMan = CameraManager.Instance;
        camMan.SwitchMode(ECameraView.Sections);
        camMan.ZoomToSectionSegment(Segment);
    }
}
