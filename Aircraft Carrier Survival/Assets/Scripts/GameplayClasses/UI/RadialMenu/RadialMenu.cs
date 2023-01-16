using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    private Animator animator;
    //private bool isActive = false;

    private Person person;
    private SectionRoom section;
    public List<RadialButtonData> RadialButtonDatas;
    public Dictionary<EDCType, RadialButtonData> RadialButtonDict;

    private Image radialRing;

    void Awake()
    {
        RadialButtonDict = new Dictionary<EDCType, RadialButtonData>();
        foreach (var data in RadialButtonDatas)
        {
            Assert.IsFalse(RadialButtonDict.ContainsKey(data.Type));
            RadialButtonDict[data.Type] = data;
        }
//#warning radial todo
        //RadialButtonDict[EDCType.Repair].GetTargetSubsection = GetSubsectionRepairable;
        //RadialButtonDict[EDCType.Firefight].GetTargetSubsection = GetSubsectionFirefightable;
        //RadialButtonDict[EDCType.WaterPump].GetTargetSubsection = GetSubsectionWaterPumpable;
        //RadialButtonDict[EDCType.Rescue].GetTargetSubsection = GetSubsectionRescueable;

        //RadialButtonDict[EDCType.Repair].MetRequirements = () => GetSubsectionRepairable() != null;
        //RadialButtonDict[EDCType.Firefight].MetRequirements = () => GetSubsectionFirefightable() != null;
        //RadialButtonDict[EDCType.WaterPump].MetRequirements = () => GetSubsectionWaterPumpable() != null;
        //RadialButtonDict[EDCType.Rescue].MetRequirements = () => GetSubsectionRescueable() != null;
        //RadialButtonDict[EDCType.None].MetRequirements = () => true;

        //RadialButtonDict[EDCType.Repair].ButtonAction = () => CrewJob(EDCType.Repair);
        //RadialButtonDict[EDCType.Firefight].ButtonAction = () => CrewJob(EDCType.Firefight);
        //RadialButtonDict[EDCType.Rescue].ButtonAction = () => CrewJob(EDCType.Rescue);
        //RadialButtonDict[EDCType.WaterPump].ButtonAction = WaterPumpJob;
        //RadialButtonDict[EDCType.None].ButtonAction = LockJob;
    }

    private void Start()
    {
        radialRing = gameObject.GetComponent<Image>();
        animator = gameObject.GetComponent<Animator>();

        animator.SetBool("IsClosed", true);
    }

    public bool IsPointerOverRadialMenu()
    {
        //foreach (var data in RadialButtonDatas)
        //{
        //    if (data.Button.IsHovered)
        //    {
        //        return true;
        //    }
        //}
        return false;
    }

    public void ShowRadialMenu(Person person, SectionRoom section)
    {
        //Assert.IsNotNull(section);
        //this.person = person;
        //this.section = section;

        //transform.position = Input.mousePosition;

        //animator.SetBool("IsClosed", false);
    }

    public void CloseRadialMenu(bool success)
    {
        //if (success)
        //{
        //    foreach (var data in RadialButtonDatas)
        //    {
        //        if (data.Button.Press())
        //        {
        //            data.ButtonAction();
        //            break;
        //        }
        //    }
        //}

        //animator.SetBool("IsClosed", true);

        //if (person != null)
        //{
        //    //WorkerManager.Instance.DeselectWorker();
        //    person = null;
        //}
    }


    //public void EnabledRadialImage()
    //{
    //    radialRing.enabled = true;
    //}


    public void DisabledRadialImage()
    {
        //radialRing.enabled = false;
    }

    //public void ActivateRadial()
    //{
    //    isActive = true;
    //}

    public void DeactivateRadial()
    {
        //isActive = false;
    }

    //private void Update()
    //{
    //    if(isActive)
    //    {
    //        foreach (var data in RadialButtonDatas)
    //        {
    //            data.Button.SetInteractable(data.MetRequirements());
    //        }
    //    }

    //}
    //Subcrewman GetSubcrewman()
    //{
    //    return person?.GetFreeSubcrewman(null);
    //}

    //SubSectionRoom GetSubsectionRepairable()
    //{
    //    if (GetSubcrewman() != null)
    //    {
    //        float inverse = 1f - DamageControlManager.Instance.MaxFloodLevelToWork;
    //        foreach (var subsection in section.SubsectionRooms)
    //        {
    //            if ((subsection.CurrentState & ESubSectionRoomState.Damaged) == ESubSectionRoomState.Damaged &&
    //                !subsection.StateDatas[EDCType.Repair].Progresser.Positive &&
    //                (subsection.CurrentState & ESubSectionRoomState.Locked) != ESubSectionRoomState.Locked &&
    //                ((subsection.CurrentState & ESubSectionRoomState.AnyFlood) == 0 || (subsection.StateDatas[EDCType.WaterPump].State - inverse) > Mathf.Epsilon))
    //            {
    //                return subsection;
    //            }
    //        }
    //    }
    //    return null;
    //}

    //SubSectionRoom GetSubsectionFirefightable()
    //{
    //    if (GetSubcrewman() != null)
    //    {
    //        foreach (var subsection in section.SubsectionRooms)
    //        {
    //            if ((subsection.CurrentState & ESubSectionRoomState.Fire) == ESubSectionRoomState.Fire &&
    //                !subsection.StateDatas[EDCType.Firefight].Progresser.Positive &&
    //                (subsection.CurrentState & ESubSectionRoomState.Locked) != ESubSectionRoomState.Locked)
    //            {
    //                return subsection;
    //            }
    //        }
    //    }
    //    return null;
    //}

    //SubSectionRoom GetSubsectionWaterPumpable()
    //{
    //    var dcMan = DamageControlManager.Instance;
    //    if (dcMan.MaxWaterPumpingJobs > dcMan.CurrentPumpingJobs)
    //    {
    //        if (!section.PumpsOn)
    //        {
    //            foreach (var subsection in section.SubsectionRooms)
    //            {
    //                if ((subsection.CurrentState & ESubSectionRoomState.AnyFlood) != 0)
    //                {
    //                    return subsection;
    //                }
    //            }
    //        }
    //    }
    //    return null;
    //}


    //SubSectionRoom GetSubsectionRescueable()
    //{
    //    if (GetSubcrewman() != null)
    //    {
    //        foreach (var subsection in section.SubsectionRooms)
    //        {
    //            float inverse = 1f - DamageControlManager.Instance.MaxFloodLevelToWork;
    //            if ((subsection.CurrentState & ESubSectionRoomState.HasInjured) == ESubSectionRoomState.HasInjured &&
    //                !subsection.StateDatas[EDCType.Rescue].Progresser.Positive &&
    //                (subsection.CurrentState & ESubSectionRoomState.Locked) != ESubSectionRoomState.Locked &&
    //                ((subsection.CurrentState & ESubSectionRoomState.AnyFlood) == 0 || (subsection.StateDatas[EDCType.WaterPump].State - inverse) > Mathf.Epsilon))
    //            {
    //                return subsection;
    //            }
    //        }
    //    }
    //    return null;
    //}

    //bool IsSectionLockable()
    //{
    //    return (section.SubsectionRooms[0].CurrentState & ESubSectionRoomState.Locked) != ESubSectionRoomState.Locked;
    //}

    //private void CrewJob(EDCType type)
    //{
    //    Assert.IsTrue(type == EDCType.Repair || type == EDCType.Firefight || type == EDCType.Rescue);
    //    var subcrewman = person?.GetFreeSubcrewman(null);
    //    if (subcrewman != null)
    //    {
    //        var subsection = RadialButtonDict[type].GetTargetSubsection();
    //        if (subsection != null)
    //        {
    //            Assert.IsNull(subcrewman.Subsection);
    //            subcrewman.Subsection = subsection;
    //            subcrewman.UpdatePortrait(type, subsection);
    //            subsection.StateDatas[type].DCWorker = subcrewman;
    //            subsection.SetDCEffect(type, true);
    //        }
    //    }
    //}

    //private void WaterPumpJob()
    //{
    //    if (RadialButtonDict[EDCType.WaterPump].MetRequirements())
    //    {
    //        section.StartPumps();
    //        DamageControlManager.Instance.CurrentPumpingJobs++;
    //    }
    //}

    //private void LockJob()
    //{
    //    if (IsSectionLockable())
    //    {
    //        var dcMan = DamageControlManager.Instance;
    //        if (section.PumpsOn)
    //        {
    //            section.StopPumps();
    //        }
    //        foreach (var subsection in section.SubsectionRooms)
    //        {
    //            for (int i = 1; i < (int)EDCType.Count; i++)
    //            {
    //                if (i != (int)EDCType.WaterPump)
    //                {
    //                    dcMan.FinishDC((EDCType)i, subsection, true);
    //                }
    //            }
    //            subsection.CurrentState |= ESubSectionRoomState.Locked;
    //            subsection.StopFloodingNeighbours();
    //        }
    //    }
    //    else
    //    {
    //        foreach (var subsection in section.SubsectionRooms)
    //        {
    //            subsection.CurrentState &= ~ESubSectionRoomState.Locked;
    //            if (subsection.StateDatas[EDCType.WaterPump].State == 0f)
    //            {
    //                subsection.StartFloodingNeighbours();
    //            }
    //        }

    //    }
    //}
}
