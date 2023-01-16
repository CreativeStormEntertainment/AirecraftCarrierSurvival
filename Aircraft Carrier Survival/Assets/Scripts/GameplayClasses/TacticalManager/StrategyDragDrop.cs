/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StrategyDragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] public StrategyItem StrategyItem = null;
    private StrategyItemSlot currentSlot;
    private StrategyItemSlot lastSlot;
    public Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private EventSystem eventSystem;

    private Transform Transform;

    [SerializeField]
    private Text torpedoText = null;
    [SerializeField]
    private Text bomberText = null;
    [SerializeField]
    private Text fighterText = null;

    public GameObject LockImage = null;
    [SerializeField]
    private bool forceLock = false;

    private void Awake()
    {
        if (LockImage != null && !forceLock)
        {
            LockImage.SetActive(false);
        }
    }

    private void Start()
    {
        Transform = transform;
        StrategyItem = GetComponent<StrategyItem>();
        currentSlot = transform.GetComponentInParent<StrategyItemSlot>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        lastSlot = currentSlot;
        eventSystem = GameObject.FindObjectOfType<EventSystem>();
        if (StrategyItem != null)
        {
            if (StrategyItem.strategy.torpedoCount != 0)
            {
                torpedoText.text = StrategyItem.strategy.torpedoCount.ToString();
            }
            if (StrategyItem.strategy.bombersCount != 0)
            {
                bomberText.text = StrategyItem.strategy.bombersCount.ToString();
            }
            if (StrategyItem.strategy.fightersCount != 0)
            {
                fighterText.text = StrategyItem.strategy.fightersCount.ToString();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HudManager.Instance.HasNo(ETutorialMode.DisableFreeStrategyDrag) && !DemoMissionGame.Instance.unlockedStrategies.Contains(StrategyItem))
        {
            return;
        }
        Transform.SetParent(CrewManager.Instance.MainCanvas.transform);
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!HudManager.Instance.HasNo(ETutorialMode.DisableFreeStrategyDrag) && !DemoMissionGame.Instance.unlockedStrategies.Contains(StrategyItem))
        {
            return;
        }

        if (currentSlot != null)
        {
            if (!currentSlot.noAction)
                currentSlot.UnpackStrategy();
            currentSlot.Busy = false;
        }
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (eventSystem.IsPointerOverGameObject())
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            StrategyItemSlot slot = results[0].gameObject.GetComponent<StrategyItemSlot>();
            if (slot != null && !slot.Busy)//eventSystem.gameObject.GetComponent<Collider>().tag.Equals("Minimap"))
            {
                currentSlot = slot;
            }
            else
            {
                currentSlot = null;
            }
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        currentSlot = currentSlot == null && lastSlot != null ? lastSlot : currentSlot;
        if (!currentSlot.noAction)
            currentSlot.AssignStrategy(this);
        currentSlot.Busy = true;
        Transform.SetParent(currentSlot.transform);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        Transform.localPosition = Vector3.zero;
        lastSlot = currentSlot;
    }

    public void AssignToSlot(StrategyItemSlot slot)
    {
        currentSlot.Busy = false;
        currentSlot = slot;
        if (!currentSlot.noAction)
            currentSlot.AssignStrategy(this);
        currentSlot.Busy = true;
        Transform.SetParent(currentSlot.transform);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        Transform.localPosition = Vector3.zero;
        lastSlot = currentSlot;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentSlot != null && currentSlot.Busy)
        {
            StrategyItemTooltip.Instance.Show(StrategyItem.strategy);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StrategyItemTooltip.Instance.Hide();
    }

    public void SetLock(bool setLock)
    {
        if (LockImage != null)
        {
            LockImage.SetActive(setLock);
        }
        forceLock = setLock;
    }
}
