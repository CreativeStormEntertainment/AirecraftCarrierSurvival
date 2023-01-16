using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    [SerializeField]
    private List<CreditsGroupData> groups = null;

    [SerializeField]
    private Sprite customSprite = null;

    [SerializeField]
    private RectTransform container = null;

    [SerializeField]
    private MenuManager menu = null;

    [SerializeField]
    private Text prefabGroup = null;
    [SerializeField]
    private Text prefabGroupee = null;
    [SerializeField]
    private Image prefabImage = null;
    [SerializeField]
    private float groupOffset = 30f;
    [SerializeField]
    private float spriteOffset = 30f;
    [SerializeField]
    private float startPos = 0f;
    [SerializeField]
    private float endPos = 0f;
    [SerializeField]
    private float speed = 1f;

    private Vector2 pos;

    private void Awake()
    {
        float startOffset = 0f;
        foreach (var group in groups)
        {
            Text(prefabGroup, group.Group, ref startOffset);
            if (group.Group.StartsWith("SOUNDS"))
            {
                Image(prefabImage, customSprite, ref startOffset);
                startOffset -= spriteOffset;
            }
            foreach (var groupee in group.Groupees)
            {
                Text(prefabGroupee, groupee, ref startOffset);
            }
            startOffset -= groupOffset;
        }

        //startOffset += spriteOffset;

        endPos -= startOffset;
    }

    private void OnEnable()
    {
        Show();
    }

    private void Update()
    {
        float speed = this.speed;
        if (Input.GetKey(KeyCode.Space))
        {
            speed *= 3f;
        }

        pos.y += speed * Time.deltaTime;
        if (pos.y >= endPos)
        {
            menu.Back();
        }

        container.anchoredPosition = pos;
    }

    public void Show()
    {
        pos = new Vector2(0f, startPos);
        container.anchoredPosition = pos;

        gameObject.SetActive(true);
    }

    private void Text(Text prefab, string txt, ref float offset)
    {
        var text = Instantiate(prefab, container);
        text.gameObject.SetActive(true);
        text.text = txt;

        var rect = text.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, offset);

        offset -= rect.sizeDelta.y;
    }

    private void Image(Image prefab, Sprite sprite, ref float offset)
    {
        var image = Instantiate(prefab, container);
        image.gameObject.SetActive(true);
        image.sprite = sprite;

        var rect = image.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, offset);

        var size = sprite.rect;
        rect.sizeDelta = size.size;
        offset -= size.size.y;
    }
}
