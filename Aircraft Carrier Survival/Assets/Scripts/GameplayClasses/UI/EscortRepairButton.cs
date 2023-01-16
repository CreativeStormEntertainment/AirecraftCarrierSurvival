using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscortRepairButton : MonoBehaviour
{
    public event Action<EscortRepairButton> Repaired = delegate { };

    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Text nameText = null;
    [SerializeField]
    private GameObject destroyed = null;
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private List<GameObject> emptyDurabilityIcons = null;
    [SerializeField]
    private List<GameObject> fillDurabilityIcons = null;

    private StrikeGroupMember member;

    private void Start()
    {
        button.onClick.AddListener(Repair);
    }

    public void Setup(StrikeGroupMember member)
    {
        this.member = member;
        var locMan = LocalizationManager.Instance;
        nameText.text = locMan.GetText(member.Data.NameID);
        icon.sprite = member.Data.Icon;
        UpdateDurability(member.Data.Durability, true);
        UpdateDurability(member.CurrentDurability);
        destroyed.SetActive(member.CurrentDurability == 0);
        button.gameObject.SetActive(member.CurrentDurability != 0 && member.CurrentDurability != member.Data.Durability);
        gameObject.SetActive(true);
    }

    public void UpdateDurability(int durability, bool baseDurability = false)
    {
        if (baseDurability)
        {
            for (int i = 0; i < emptyDurabilityIcons.Count; i++)
            {
                emptyDurabilityIcons[i].SetActive(i < durability);
                fillDurabilityIcons[i].SetActive(i < durability);
            }
        }
        else
        {
            for (int i = 0; i < fillDurabilityIcons.Count; i++)
            {
                fillDurabilityIcons[i].SetActive(i < durability);
            }
        }
    }

    private void Repair()
    {
        StrikeGroupManager.Instance.Damage(member, -99);
        Repaired(this);
    }
}
