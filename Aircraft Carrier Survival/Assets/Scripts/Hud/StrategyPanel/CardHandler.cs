using FMODUnity;
using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHandler : BaseUIManeuverCard, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public int DefaultAttack
    {
        get;
        private set;
    }

    public int DefaultDefence
    {
        get;
        private set;
    }

    public int BlockerIndex
    {
        get;
        set;
    }

    public List<int> DebuffIndices
    {
        get;
        private set;
    } = new List<int>();

    public List<int> DebuffIndexSecondaries
    {
        get;
        private set;
    } = new List<int>();

    public bool SlotModifierBlocked
    {
        get;
        set;
    }

    public bool Irreplaceable
    {
        get;
        set;
    }

    public GameObject Blocker => blocker;
    public GameObject ModifierBlocker => modifierBlocker;

    [SerializeField]
    private RectTransform rect = null;
    [SerializeField]
    private Image cardBackground = null;
    [SerializeField]
    private Text attackValue = null;
    [SerializeField]
    private Text defenceValue = null;
    [SerializeField]
    private GameObject blocker = null;
    [SerializeField]
    private GameObject modifierBlocker = null;

    [SerializeField]
    private float scale = 1.2f;

    [SerializeField]
    private Animator animator = null;

    [SerializeField]
    private StudioEventEmitter drag = null;
    [SerializeField]
    private StudioEventEmitter dropCompleted = null;
    [SerializeField]
    private StudioEventEmitter dropUncompleted = null;
    [SerializeField]
    private StudioEventEmitter hover = null;

    private Transform defaultParent = null;
    private RectTransform rectTransform = null;
    private Canvas canvas = null;
    private CardPlace cardPlace = null;

    private Transform parentDrag = null;

    private int animationNameHash = 0;

    private StrategySelectionPanel panel;
    private bool hovered;

    private void OnEnable()
    {
        Anim(false);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!blocker.activeSelf && !Irreplaceable)
        {
            hovered = true;
            rect.localScale = new Vector3(scale, scale, scale);
            base.OnPointerEnter(eventData);
            Anim(true);
            hover.Play();
            panel.PlayEvent(Data.CardSound);
        }

        if (blocker.activeSelf || modifierBlocker.activeSelf)
        {
            panel.SetShowHighlight(true, BlockerIndex);
        }
        foreach (int index in DebuffIndices)
        {
            panel.SetShowHighlight(true, index);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        panel.Stop(true);
        hovered = false;
        rect.localScale = Vector3.one;
        Anim(false);
        panel.SetShowHighlight(false, BlockerIndex);
        foreach (int index in DebuffIndices)
        {
            panel.SetShowHighlight(false, index);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        int slot = -1;
        if (eventData.button != PointerEventData.InputButton.Left || Irreplaceable ||
            (panel.ForcedStrategy != null && !panel.ForcedStrategy.TryGetValue(Data, out slot)))
        {
            return;
        }
        panel.SetShowHighlight(false, BlockerIndex);
        foreach (int index in DebuffIndices)
        {
            panel.SetShowHighlight(false, index);
        }
        if (slot != -1)
        {
            panel.DropHere(slot);
        }

        //cardBackground.raycastTarget = false;
        var trans = panel.Container;
        if (rectTransform.parent == trans)
        {
            ResetPlacement();
        }
        parentDrag = rectTransform.parent;
        rectTransform.SetParent(trans);
        rectTransform.SetAsLastSibling();

        drag.Play();
        Anim(false);

        panel.Drag = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        int slotIndex = -1;
        if (eventData.button != PointerEventData.InputButton.Left || Irreplaceable ||
            (panel.ForcedStrategy != null && !panel.ForcedStrategy.TryGetValue(Data, out slotIndex)))
        {
            return;
        }
        if (slotIndex != -1)
        {
            panel.ResetDrops(slotIndex);
        }
        panel.Drag = false;

        rectTransform.SetParent(parentDrag);
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            List<RaycastResult> result = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, result);
            bool placed = false;
            if (!blocker.activeSelf)
            {
                foreach (var item in result)
                {
                    if (item.gameObject != null && item.gameObject != cardBackground.gameObject &&
                        item.gameObject.TryGetComponent(out CardPlace cardPlace) && !cardPlace.Blocker.activeSelf &&
                        (cardPlace.CurrentCard == null || !cardPlace.CurrentCard.Irreplaceable) &&
                        (slotIndex == -1 || panel.IndexOf(cardPlace) == slotIndex))
                    {
                        CardToPlace(cardPlace);
                        dropCompleted.Play();
                        placed = true;
                        break;
                    }
                }
            }
            if (!placed)
            {
                dropUncompleted.Play();
                if (this.cardPlace != null)
                {
                    foreach (int index in DebuffIndices)
                    {
                        panel.SetShowHighlight(false, index);
                    }
                    this.cardPlace.CardPlaced(null);
                    this.cardPlace = null;
                }
            }

        }
        else if (this.cardPlace != null)
        {
            foreach (int index in DebuffIndices)
            {
                panel.SetShowHighlight(false, index);
            }
            this.cardPlace.CardPlaced(null);
            dropUncompleted.Play();
            this.cardPlace = null;
        }
        rectTransform.anchoredPosition = Vector2.zero;
        //cardBackground.raycastTarget = true;

        if (blocker.activeSelf || modifierBlocker.activeSelf)
        {
            panel.SetShowHighlight(true, BlockerIndex);
        }
        foreach (int index in DebuffIndices)
        {
            panel.SetShowHighlight(true, index);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || Irreplaceable ||
            (panel.ForcedStrategy != null && !panel.ForcedStrategy.TryGetValue(Data, out _)))
        {
            return;
        }
        if (!blocker.activeSelf)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void Setup(PlayerManeuverData data, Canvas canvas, StrategySelectionPanel panel)
    {
        Setup(data, data.Level);

        rectTransform = (RectTransform)transform;
        defaultParent = rectTransform.parent;
        this.canvas = canvas;

        this.panel = panel;

        DefaultAttack = (int)data.Values.Attack;
        DefaultDefence = (int)data.Values.Defense + TacticManager.Instance.BonusManeuversDefence;
        ResetValues();

        //animator.Play(emptyAnimationHash);
        animationNameHash = Animator.StringToHash(data.Clip.name);
    }

    public void CardToPlace(CardPlace place)
    {
        if (place == null)
        {
            return;
        }
        if (cardPlace != null)
        {
            cardPlace.CardPlaced(null);
        }
        cardPlace = place;
        cardPlace.CardPlaced(this);
        if (rectTransform == null)
        {
            rectTransform = (RectTransform)transform;
        }
        rectTransform.SetParent(cardPlace.transform);
        rectTransform.localPosition = Vector3.zero;
    }

    public void ResetPlacement()
    {
        if (hovered)
        {
            OnPointerExit(null);
        }

        rectTransform.SetParent(defaultParent);
        rectTransform.localPosition = Vector2.zero;
        this.cardPlace = null;
        ResetValues();
    }

    public void NewValues(float attack, float defense)
    {
        int attackInt = Mathf.RoundToInt(attack);
        attackValue.text = attackInt.ToString();
        Color color;
        if (attackInt == DefaultAttack)
        {
            color = Color.white;
        }
        else if (attackInt > DefaultAttack)
        {
            color = Color.green;
        }
        else
        {
            color = Color.red;
        }
        attackValue.color = color;

        int defenseInt = Mathf.RoundToInt(defense);
        defenceValue.text = defenseInt.ToString();
        if (defenseInt == DefaultDefence)
        {
            color = Color.white;
        }
        else if (defenseInt > DefaultDefence)
        {
            color = Color.green;
        }
        else
        {
            color = Color.red;
        }
        defenceValue.color = color;
    }

    private void ResetValues()
    {
        attackValue.text = DefaultAttack.ToString();
        attackValue.color = Color.white;
        defenceValue.text = DefaultDefence.ToString();
        defenceValue.color = Color.white;
    }

    private void Anim(bool play)
    {
        animator.Play(animationNameHash, 0, 0f);
        animator.speed = play ? 1f : 0f;
    }
}
