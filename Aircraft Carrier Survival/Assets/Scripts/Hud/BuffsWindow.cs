using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffsWindow : MonoBehaviour
{
    [SerializeField]
    private Text text = null;
    [SerializeField]
    private List<string> stepsIDs = null;
    [SerializeField]
    private List<Button> tabButtons = null;
    [SerializeField]
    private List<GameObject> selectedIcons = null;

    public void Init(int unlockedBuffs)
    {
        if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                int tab = i;
                tabButtons[tab].onClick.AddListener(() => SelectBuffsTab(tab));
            }
        }
        UpdateTabs(unlockedBuffs);
    }

    public void UpdateTabs(int unlockedBuffs)
    {
        if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                tabButtons[i].gameObject.SetActive(unlockedBuffs >= i * 8);
            }
            SelectBuffsTab(0);
        }
    }

    public void SetTipText(int step)
    {
        text.text = LocalizationManager.Instance.GetText(stepsIDs[step]);
    }

    public void SelectBuffsTab(int tab)
    {
        var islMan = IslandsAndOfficersManager.Instance;
        for (int tabIndex = 0; tabIndex < tabButtons.Count; tabIndex++)
        {
            selectedIcons[tabIndex].SetActive(tabIndex == tab);
            var count = Mathf.Min(islMan.UnlockedBuffs.Count, (1 + tabIndex) * 8);
            for (int i = tabIndex * 8; i < count; i++)
            {
                islMan.UnlockedBuffs[i].IslandBuffUIElement.gameObject.SetActive(tab == tabIndex);
            }
        }
    }
}
