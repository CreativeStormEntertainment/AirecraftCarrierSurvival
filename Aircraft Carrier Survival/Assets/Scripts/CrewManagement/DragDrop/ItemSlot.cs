/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Action OnSlotUpdated = delegate { };

    public int Index
    {
        get
        {
            if (index == 0)
            {
                if (!int.TryParse(name.Split('#')[1], out int result))
                {
                    Debug.LogError("ItemSlot have incorrect name.");
                }
                index = result - 1;
            }

            return index;
        }
    }

    public DepartmentItem CurrentDepartment
    {
        get => currentDepartment;
        set
        {
            currentDepartment = value;
        }
    }

    public int Cooldown
    {
        get => cooldown;
        set
        {
            cooldown = value;
            UpdateCooldown();
        }
    }

    public CrewUnit CrewUnitBeforeDrag
    {
        get;
        set;
    }

    public bool IsHealing
    {
        get => isHealing;
        set
        {
            isHealing = value;
            healing.SetActive(value);
        }
    }

    public CrewUnit CurrentCrewUnit => currentCrewUnit;

    public bool IsUsable = true;
    public Transform CrewSlot { get => crewSlot; }
    [SerializeField]
    private Transform crewSlot = null;
    [SerializeField]
    private GameObject highlight = null;

    [SerializeField]
    private GameObject padlock = null;
    [SerializeField]
    private GameObject cooldownObj = null;
    [SerializeField]
    private Text cooldownTextH = null;
    [SerializeField]
    private Text cooldownTextM = null;
    [SerializeField]
    private GameObject healing = null;

    private DepartmentItem currentDepartment;
    private CrewUnit currentCrewUnit;

    private int index = 0;

    private bool isHealing;
    private int cooldown;

    public void Setup(DepartmentItem currentDepartment)
    {
        this.currentDepartment = currentDepartment;
        UpdateCooldown();
    }

    public void UnpackUnit()
    {
        currentCrewUnit = null;
        OnSlotUpdated();
    }

    public void AssignUnit(CrewUnit unit)
    {
        currentCrewUnit = unit;
    }

    public void Highlight(bool show)
    {
        highlight.SetActive(show);
    }

    public void UpdateCooldown()
    {
        if (cooldownObj == null)
        {
            return;
        }
        if (Cooldown <= 0)
        {
            cooldownObj.SetActive(false);
        }
        else
        {
            cooldownObj.SetActive(true);

            float value = Cooldown / (float)TimeManager.Instance.TicksForHour;
            int hours = (int)value;
            cooldownTextH.text = "-" + hours.ToString("00");
            cooldownTextM.text = ((int)((value - hours) * 60f)).ToString("00");
        }
    }

    public void SetPadlock(bool state)
    {
        IsUsable = !state;
        if (padlock != null)
        {
            padlock.SetActive(state);
        }
    }
}
