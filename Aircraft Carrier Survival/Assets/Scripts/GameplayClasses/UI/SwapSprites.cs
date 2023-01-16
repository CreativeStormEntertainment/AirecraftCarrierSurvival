using UnityEngine;
using UnityEngine.UI;

public class SwapSprites : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites = null;

    private int index = 1;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void ChangeSprite()
    {
        image.sprite = sprites[index++];
        image.SetNativeSize();
        if (index >= sprites.Length)
        {
            index = 0;
        }
    }

}
