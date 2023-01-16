using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EnemyStrategySubobject : MonoBehaviour
{
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text deffense = null;
    [SerializeField]
    private Text attack = null;
    [SerializeField]
    private Text description = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Image image2 = null;
    [SerializeField]
    private List<Image> durabilityImages = null;
    [SerializeField]
    private Sprite durabilityOn = null;
    [SerializeField]
    private Sprite durabilityOff = null;

    [SerializeField]
    private RectTransform maskTrans = null;
    [SerializeField]
    private RectTransform imageInMask = null;
    [SerializeField]
    private RectTransform shadow = null;
    [SerializeField]
    private GameObject sndImage = null;
    [SerializeField]
    private Mask mask = null;
    [SerializeField]
    private Image maskImg = null;

    [SerializeField]
    private float time = .5f;
    [SerializeField]
    private float percentage = .333333f;

    private float current;
    private float from;
    private float to;

    private void Update()
    {
        UpdateAnim(Time.unscaledDeltaTime / time);
    }

    public void Init(Vector2 size)
    {
        percentage -= 0.5f;
        from = -percentage * size.x;
        to = percentage * size.x;

        size.x -= 4f;
        size.y -= 4f;
        imageInMask.sizeDelta = size;
    }

    public void Setup(EnemyManeuverData data, int currentDurability, int durability)
    {
        Presetup(data, data.Sprite, currentDurability, durability, false);

        if (data.Sprite != null)
        {
            image.sprite = data.Sprite;
        }
    }

    public void Setup(EnemyManeuverData data, EnemyManeuverData data1, EnemyManeuverData data2)
    {
        Presetup(data, data1.Sprite, data.Durability, data.Durability, true);

        if (data2.Sprite != null)
        {
            image2.sprite = data2.Sprite;
        }

        current = 1f;
        UpdateAnim(0f);
    }

    public void Animate()
    {
        current = 0f;
        UpdateAnim(0f);
        enabled = true;
    }

    private void Presetup(EnemyManeuverData data, Sprite icon, int currentDurability, int durability, bool half)
    {
        if (icon != null)
        {
            image.sprite = icon;
        }

        var modifiers = Parameters.Instance.DifficultyParams;
        var minValues = data.MinValues;
        var maxValues = data.MaxValues;
        if (data.MisidentifiedType != EMisidentifiedType.Unique)
        {
            if (minValues.Attack > modifiers.EnemyBlocksAttackModifier)
            {
                minValues.Attack = Mathf.Max(minValues.Attack - modifiers.EnemyBlocksAttackModifier, 0);
                maxValues.Attack = Mathf.Max(maxValues.Attack - modifiers.EnemyBlocksAttackModifier, 0);
            }
            if (minValues.Defense > modifiers.EnemyBlocksDefenseModifier)
            {
                minValues.Defense = Mathf.Max(minValues.Defense - modifiers.EnemyBlocksDefenseModifier, 0);
                maxValues.Defense = Mathf.Max(maxValues.Defense - modifiers.EnemyBlocksDefenseModifier, 0);
            }
        }

        title.text = data.LocalizedName;
        description.text = data.LocalizedDescription;
        deffense.text = $"{minValues.Defense} -{maxValues.Defense}";
        attack.text = $"{minValues.Attack} -{maxValues.Attack}";

        int i;
        for (i = 0; i < durability; i++)
        {
            durabilityImages[i].gameObject.SetActive(true);
            durabilityImages[i].sprite = i < currentDurability ? durabilityOn : durabilityOff;
        }
        for (; i < durabilityImages.Count; i++)
        {
            durabilityImages[i].gameObject.SetActive(false);
        }

        enabled = half;

        if (mask == null)
        {
            Assert.IsFalse(half);
        }
        else
        {
            mask.enabled = half;
            maskImg.enabled = half;
            shadow.gameObject.SetActive(half);
            sndImage.SetActive(half);
        }
    }

    private void UpdateAnim(float t)
    {
        current += t;
        if (current >= 1f)
        {
            current = 1f;
            enabled = false;
        }
        var pos = new Vector2(Mathf.Lerp(from, to, (2f - current) * current), 0f);
        maskTrans.anchoredPosition = pos;
        shadow.anchoredPosition = pos;

        pos.x = -pos.x;
        imageInMask.anchoredPosition = pos;
    }
}
