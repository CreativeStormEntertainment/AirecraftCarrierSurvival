using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gif : MonoBehaviour
{
    [SerializeField]
    private Image img;
    [SerializeField]
    private List<Sprite> sprites = new List<Sprite>();
    [SerializeField]
    private float animSpeed = 1f;
    private float finalAnimTime = 1f;

    private float animTimer = 0;
    private int animPtr = 0;

    bool timeFreezed = false;
    float ts = 0;

    private void Start()
    {
        TimeManager.Instance.TimeScaleChanged += CheckTimeScale;
        CheckTimeScale();
    }

    private void LateUpdate()
    {
        if (timeFreezed)
        {
            return;
        }
        animTimer += Time.deltaTime;
        if (animTimer >= finalAnimTime)
        {
            animTimer = 0;
            ++animPtr;
            if (animPtr >= sprites.Count)
            {
                animPtr = 0;
            }
            img.sprite = sprites[animPtr];
        }
    }

    private void CheckTimeScale()
    {
        if (Time.timeScale == 0)
        {
            timeFreezed = true; 
        }
        else
        {
            finalAnimTime = animSpeed * Time.timeScale;
            timeFreezed = false;
        }
    }
}
