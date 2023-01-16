using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffsPanel : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas = null;
    [SerializeField]
    private RectTransform root = null;
    [SerializeField]
    private Toggle allBuffsToggle = null;
    [SerializeField]
    private Toggle editToggle = null;
    [SerializeField]
    private Toggle selectedBuffsToggle = null;
    [SerializeField]
    private GameObject allBuffsPanel = null;
    [SerializeField]
    private GameObject selectedBuffsPanel = null;
    [SerializeField]
    private GameObject editPanel = null;
    [SerializeField]
    private Button confirmButton = null;
    [SerializeField]
    private Button cancelButton = null;
    [SerializeField]
    private IslandBuffDraggableInventory inventory = null;
    [SerializeField]
    private IslandBuffActiveInventory activeInventory = null;
    [SerializeField]
    private int maxSelectedBuffsCount = 6;
    [SerializeField]
    private int hoursForChangingSelectedBuffs = 4;

    private List<IslandBuffDragData> islandBuffs = new List<IslandBuffDragData>();
    private List<IslandBuffDragData> selected = new List<IslandBuffDragData>();
    private List<IslandBuffDragData> oldSelected = new List<IslandBuffDragData>();

    private void Awake()
    {
        allBuffsToggle.onValueChanged.AddListener(SetShowAllBuffsPanel);
        selectedBuffsToggle.onValueChanged.AddListener(SetShowSelectedBuffsPanel);
        editToggle.onValueChanged.AddListener(OnEditToggle);
        confirmButton.onClick.AddListener(() => ConfirmEditedBuffs(true));
        cancelButton.onClick.AddListener(CancelEditedBuffs);
    }

    public void Setup()
    {
        foreach (var buff in IslandsAndOfficersManager.Instance.IslandBuffs.Values)
        {
            if ((SaveManager.Instance.Data.MissionRewards.AdmiralUnlockedOrders & (1 << (int)buff.IslandBuffType)) != 0 || buff.BaseUnlocked)
            {
                IslandBuffDragData data = new IslandBuffDragData(buff, canvas, root);
                islandBuffs.Add(data);
            }
        }
        inventory.Setup(0);
        activeInventory.Init(root, canvas);
        inventory.Setup(islandBuffs, islandBuffs.Count);

        for (int i = 0; i < maxSelectedBuffsCount; i++)
        {
            activeInventory.Add();
            selected.Add(null);
        }
        int index = 0;
        foreach (var buff in IslandsAndOfficersManager.Instance.IslandBuffs.Values)
        {
            if ((SaveManager.Instance.Data.SandboxData.SelectedBuffs & (1 << (int)buff.IslandBuffType)) != 0)
            {
                var islandBuff = islandBuffs.Find(item => item.Buff == buff);
                OnItemChanged(index, islandBuff);
                activeInventory.Set(index, islandBuff);
            }
        }
        ConfirmEditedBuffs(false);

        activeInventory.ItemChanged += OnItemChanged;
    }

    private void OnItemChanged(int slot, IslandBuffDragData newBuff)
    {
        var oldBuff = selected[slot];
        if (oldBuff != null)
        {
            oldBuff.Selected = false;
        }
        if (newBuff == null)
        {
            selected[slot] = null;
        }
        else
        {
            selected[slot] = newBuff;
            newBuff.Selected = true;
        }

        inventory.Setup(islandBuffs, islandBuffs.Count);
    }

    private void ConfirmEditedBuffs(bool changeMainGoalTime)
    {
        oldSelected.Clear();
        var saveData = SaveManager.Instance.Data;
        var oldBuffs = saveData.SandboxData.SelectedBuffs;
        saveData.SandboxData.SelectedBuffs = 0;
        foreach (var buff in selected)
        {
            oldSelected.Add(buff);
            if (buff != null)
            {
                saveData.SandboxData.SelectedBuffs |= (1 << (int)buff.Buff.IslandBuffType);
            }
        }
        foreach (var buff in IslandsAndOfficersManager.Instance.IslandBuffs.Values)
        {
            buff.UpdateSandboxSelectedBuffs();
        }
        if (changeMainGoalTime && oldBuffs != saveData.SandboxData.SelectedBuffs)
        {
            SandboxManager.Instance.SandboxGoalsManager.ShortenMainGoalTime(hoursForChangingSelectedBuffs);
        }
        OnEditToggle(false);
    }

    private void CancelEditedBuffs()
    {
        int index = 0;
        foreach (var buff in oldSelected)
        {
            OnItemChanged(index, buff);
            activeInventory.Set(index, buff);
            index++;
        }
        OnEditToggle(false);
    }

    private void OnEditToggle(bool show)
    {
        editToggle.SetIsOnWithoutNotify(show);
        foreach (var data in islandBuffs)
        {
            data.CanBeDragged = show;
        }
        editPanel.SetActive(show);
        allBuffsPanel.SetActive(show || allBuffsToggle.isOn);
        selectedBuffsPanel.SetActive(show || selectedBuffsToggle.isOn);
    }

    private void SetShowAllBuffsPanel(bool show)
    {
        allBuffsPanel.SetActive(show);
    }

    private void SetShowSelectedBuffsPanel(bool show)
    {
        selectedBuffsPanel.SetActive(show);
    }
}
