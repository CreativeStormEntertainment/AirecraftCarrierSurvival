using UnityEngine;
using UnityEngine.UI;

public class LocalizationSprite : MonoBehaviour
{
    [SerializeField]
    private Image image = null;

    [SerializeField]
    private SpriteLocalizationData data = null;

    private void Awake()
    {
        image.sprite = data.GetSprite();
    }
}
