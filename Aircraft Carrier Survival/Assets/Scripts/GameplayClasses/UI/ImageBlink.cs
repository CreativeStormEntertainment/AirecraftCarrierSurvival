using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageBlink : MonoBehaviour
{
    [SerializeField]
    private Image image = null;

    private float time;
    private int dir = 1;

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            time += (Time.unscaledDeltaTime * dir);
            if (time >= 1f)
            {
                time = 1f;
                dir = -1;
            }
            else if (time <= 0)
            {
                time = 0f;
                dir = 1;
            }
            image.color = new Color(image.color.r, image.color.g, image.color.b, time);
        }
    }
}
