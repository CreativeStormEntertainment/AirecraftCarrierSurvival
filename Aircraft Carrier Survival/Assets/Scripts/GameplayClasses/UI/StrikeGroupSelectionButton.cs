using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrikeGroupSelectionButton : MonoBehaviour
{
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private Text nameText = null;
    [SerializeField]
    private GameObject durability = null;
    [SerializeField]
    private List<GameObject> emptyDurabilityIcons = null;
    [SerializeField]
    private List<GameObject> fillDurabilityIcons = null;
    [SerializeField]
    private GameObject cooldown = null;
    [SerializeField]
    private Text hoursText = null;
    [SerializeField]
    private Text minutesText = null;

    private StrikeGroupMember member;

    private EStrikeGroupActiveSkill activeSkill;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if (cooldown.activeInHierarchy)
        {
            int hours = (int)(member.Cooldown / (float)TimeManager.Instance.TicksForHour);
            int minutes = (int)((member.Cooldown - (hours * TimeManager.Instance.TicksForHour)) / ((float)TimeManager.Instance.TicksForHour / 60f));
            hoursText.text = "-" + hours.ToString("00");
            minutesText.text = minutes.ToString("00");
        }
    }

    public void SetupCooldown(StrikeGroupMember member)
    {
        activeSkill = EStrikeGroupActiveSkill.ReduceEscortCooldown;
        cooldown.gameObject.SetActive(true);
        durability.gameObject.SetActive(false);
        Setup(member);
    }

    public void SetupDurability(StrikeGroupMember member)
    {
        activeSkill = EStrikeGroupActiveSkill.RepairEscort;
        durability.gameObject.SetActive(true);
        for (int i = 0; i < emptyDurabilityIcons.Count; i++)
        {
            emptyDurabilityIcons[i].SetActive(i < member.Data.Durability);
            fillDurabilityIcons[i].SetActive(i < member.CurrentDurability);
        }
        cooldown.gameObject.SetActive(false);
        Setup(member);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Setup(StrikeGroupMember member)
    {
        this.member = member;
        var locMan = LocalizationManager.Instance;
        nameText.text = locMan.GetText(member.Data.NameID);
        gameObject.SetActive(true);
    }

    private void OnClick()
    {
        StrikeGroupManager.Instance.ActivateStrikeGroupSelectionSkill(activeSkill, member);
        UIManager.Instance.StrikeGroupSelectionWindow.Hide();
    }
}
