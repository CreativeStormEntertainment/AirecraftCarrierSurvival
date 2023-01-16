using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfficerLevelImage : MonoBehaviour
{
    [SerializeField]
    private Image fillImage = null;
    [SerializeField]
    private Image frameImage= null;
    [SerializeField]
    private Sprite lightSprite = null;
    [SerializeField]
    private Sprite darkSprite = null;

    public void SetFillAmount(float percent)
    {
        fillImage.fillAmount = percent;
        frameImage.sprite = percent < 1f ? darkSprite : lightSprite;
    }
}
