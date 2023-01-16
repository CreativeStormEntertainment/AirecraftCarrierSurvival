using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class SectionRoom : Room
{
    public event Action<bool> SectionWorkingChanged = delegate { };

    private static readonly int Color = Shader.PropertyToID("_Color");

    public bool IsWorking
    {
        get;
        private set;
    }

    public bool IsActive
    {
        get
        {
            foreach (var subsection in SubsectionRooms)
            {
                if (subsection.IsActive)
                {
                    Assert.IsTrue(isActive, gameObject.name);
                    if (isActive)
                    {
                        return true;
                    }
                }
            }
            Assert.IsFalse(isActive, gameObject.name);
            return isActive;
        }
        set
        {
            isActive = value;
            isActive = IsActive;
        }
    }

    public List<SubSectionRoom> SubsectionRooms;

    public EffectsDataWrapper BaseEffectsWrapper;
    public EffectsDataWrapper BonusEffectsWrapper;

    public ESectionCategory Category;
    public Sprite Icon;
    public float ZCenter;
    public GameObject Hover;

    [Header("Damage tooltips")]
    [FormerlySerializedAs("damageTooltipTitleID")]
    public string DamageTooltipTitleID;
    [FormerlySerializedAs("damageTooltipDescID")]
    public string DamageTooltipDescID;

    [SerializeField]
    private bool ignoreEvent = false;
    [SerializeField]
    private bool wreckSection = false;

    private StringBuilder descriptionBuilder = new StringBuilder();

    private Material material;

    private void Awake()
    {
        isActive = false;
        SubsectionRooms = GetComponentsInChildren<SubSectionRoom>(true).ToList();

        SubsectionRooms[0].NeighbourInSection = SubsectionRooms[1];
        SubsectionRooms[1].NeighbourInSection = SubsectionRooms[0];

        foreach (SubSectionRoom subSectionRoom in SubsectionRooms)
        {
            subSectionRoom.ParentSection = this;
            subSectionRoom.RoomName = this.RoomName;
        }
        IsWorking = true;
    }

    private void Update()
    {
        foreach (var subsection in SubsectionRooms)
        {
            if (wreckSection)
            {
                if (!subsection.IsWorking)
                {
                    break;
                }
            }
            else if (subsection.IsWorking)
            {
                if (!IsWorking)
                {
                    IsWorking = true;
                    SectionWorkingChanged(true);
                    EventManager.Instance.RemoveSectionShutdown(this);
                }
                return;
            }
        }
        if (IsWorking && !ignoreEvent)
        {
            EventManager.Instance.AddSectionShutdown(this);
        }
        IsWorking = false;
        SectionWorkingChanged(false);
    }

    private void OnDestroy()
    {
        if (material != null)
        {
            Destroy(material);
        }
    }

    public void UpdateCrew()
    {
        isActive = false;
        foreach (var subsection in SubsectionRooms)
        {
            if (subsection.IsActive)
            {
                IsActive = true;
            }
        }

        if (IsActive)
        {
            foreach (var subsection in SubsectionRooms)
            {
                if (subsection.IsActive)
                {
                    subsection.StartWorkers();
                }
            }
        }
    }

    public IEnumerable<SectionSegment> GetAllSegments(bool all)
    {
        foreach (var subsection in SubsectionRooms)
        {
            if (all || subsection.IsWorking)
            {
                foreach (var segment in subsection.Segments)
                {
                    yield return segment;
                }
            }
        }
    }

    public void ChangeFrame()
    {
        material = Hover.GetComponent<MeshRenderer>().material;
        material.SetColor(Color, new Color32(0xC5, 0x40, 0x38, 255));
    }
}
