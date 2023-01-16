using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetResourceText : SetPlanesTextBase, IPointerEnterHandler, IPointerExitHandler
{
    public event Action AnimChanged = delegate { };

    [SerializeField]
    private Text suppliesPercent = null;
    [SerializeField]
    private List<GameObject> bombersRepair = null;
    [SerializeField]
    private List<GameObject> fightersRepair = null;
    [SerializeField]
    private List<GameObject> torpedoesRepair = null;

    [SerializeField]
    private List<GameObject> offOnTooltip = null;
    [SerializeField]
    private RectTransform animBG = null;
    [SerializeField]
    private RectTransform animMask = null;

    [SerializeField]
    private float startPos = default;

    [SerializeField]
    private float time = 0.5f;
    [SerializeField]
    private float startBlend = 0.25f;

    [SerializeField]
    private List<Image> blendSprites = null;
    [SerializeField]
    private List<Text> blendTexts = null;

    private ResourceManager resourceManager;
    private Dictionary<EPlaneType, List<GameObject>> dict;

    private float current;
    private float scale;
    private float dest;

    private Vector2 endPos;
    private Vector2 endSize;
    private bool anim;
    private float interm;

    protected override void Start()
    {
        resourceManager = ResourceManager.Instance;
        
        var deck = AircraftCarrierDeckManager.Instance;
        resourceManager.SuppliesAmountChanged += SetFuel;
        deck.DeckSquadronsCountChanged += SetPlanes;
        deck.RepairPlaneChanged += OnRepairPlaneChanged;

        dict = new Dictionary<EPlaneType, List<GameObject>>();
        dict[EPlaneType.Bomber] = bombersRepair;
        dict[EPlaneType.Fighter] = fightersRepair;
        dict[EPlaneType.TorpedoBomber] = torpedoesRepair;

        endPos = animBG.anchoredPosition;
        endSize = animMask.sizeDelta;

        SetTooltip(false);
        Animate(1f);

        interm = startBlend / time;

        GameSceneManager.Instance.StartCoroutineActionAfterTime(SetPlanes, .2f);
    }

    private void Update()
    {
        if (!anim || Time.timeScale < 0.1f)
        {
            return;
        }
        Animate(Time.unscaledDeltaTime / time);
    }

    public void SetFuel(float value)
    {
        suppliesPercent.text = (Mathf.Min((int)(resourceManager.Supplies / resourceManager.SuppliesCapacity * 100), 100)).ToString() + "%";
    }

    public void OnRepairPlaneChanged(bool value, EPlaneType type)
    {
        foreach (var pair in dict)
        {
            bool active = value && pair.Key == type;
            foreach (var obj in pair.Value)
            {
                obj.SetActive(active);
            }
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        SetTooltip(true);
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.DefenceHoverIn);
    }

    public void OnPointerExit(PointerEventData data)
    {
        SetTooltip(false);
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.DefenceHoverOut);
    }

    private void SetTooltip(bool on)
    {
        foreach (var obj in offOnTooltip)
        {
            obj.SetActive(!on);
        }

        if (on)
        {
            current = 0f;
            scale = 1f;
            dest = 1f;

            foreach (var image in blendSprites)
            {
                image.gameObject.SetActive(true);
            }
            foreach (var text in blendTexts)
            {
                text.gameObject.SetActive(true);
            }
            animMask.gameObject.SetActive(true);
            SetPlanes();
        }
        else
        {
            current = 1f;
            scale = -1f;
            dest = 0f;
        }
        Animate(0f);
        anim = true;
        AnimChanged();
    }

    private void Animate(float t)
    {
        current += scale * t;

        bool show = scale > 0f;
        bool more = current > dest;
        if (current == dest || (show == more))
        {
            current = dest;
            anim = false;

            if (!show)
            {
                foreach (var image in blendSprites)
                {
                    image.gameObject.SetActive(false);
                }
                foreach (var text in blendTexts)
                {
                    text.gameObject.SetActive(false);
                }
                animMask.gameObject.SetActive(false);
            }
            AnimChanged();
        }

        float value = (2f - current) * current;
        animBG.anchoredPosition = new Vector2(endPos.x, Mathf.Lerp(startPos, endPos.y, value));
        animMask.sizeDelta = new Vector2(endSize.x, Mathf.Lerp(0f, endSize.y, value));

        float blend = Mathf.Clamp01((current - interm) / (1f - interm));
        blend = (2f - blend) * blend;
        foreach (var image in blendSprites)
        {
            var color = image.color;
            color.a = blend;
            image.color = color;
        }
        foreach (var text in blendTexts)
        {
            var color = text.color;
            color.a = blend;
            text.color = color;
        }
    }
}
