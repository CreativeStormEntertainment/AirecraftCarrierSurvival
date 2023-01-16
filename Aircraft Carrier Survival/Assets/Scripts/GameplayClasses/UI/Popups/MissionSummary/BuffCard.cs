using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Text orderName = null;
    [SerializeField]
    private Text navyPoints = null;
    [SerializeField]
    private Text airPoints = null;
    [SerializeField]
    private Text orderDesc = null;
    [SerializeField]
    private Text effect = null;

    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private GameObject pressed = null;

    private IslandBuff islandBuff;
    private ChoseBuffWindow window;

    private void Start()
    {
        button.onClick.AddListener(UnlockOrder);
    }

    public void Setup(IslandBuff buff, bool enableButton, ChoseBuffWindow window = null)
    {
        this.window = window;
        button.enabled = enableButton;
        islandBuff = buff;
        var locMan = LocalizationManager.Instance;
        airPoints.text = buff.Air.ToString();
        navyPoints.text = buff.Ship.ToString();
        image.sprite = buff.Icon;
        var name = buff.IslandBuffType.ToString().Replace(" ", "");
        orderName.text = locMan.GetText(name + "Title");
        orderDesc.text = locMan.GetText(name + "Desc");
        effect.text = locMan.GetText(name + "Effect");
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.enabled)
        {
            highlight.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.enabled)
        {
            highlight.SetActive(false);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button.enabled)
        {
            pressed.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.enabled)
        {
            pressed.SetActive(false);
        }
    }

    private void UnlockOrder()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.MissionSuccessPopup.MissionRewards.AdmiralUnlockedOrders |= (1 << (int)islandBuff.IslandBuffType);
        }
        else
        {
            IntermissionManager.Instance.SandboxMainGoalSummary.MissionRewards.AdmiralUnlockedOrders |= (1 << (int)islandBuff.IslandBuffType);
        }
        window.AddToLeftPanel(islandBuff);
    }
}
