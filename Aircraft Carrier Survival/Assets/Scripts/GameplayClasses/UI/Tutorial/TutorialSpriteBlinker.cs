using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSpriteBlinker : TutorialBlinker
{
    private SpriteRenderer spriteRenderer;
    private Color color;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = spriteRenderer.color;
    }

    protected override void SetColor(float power)
    {
        color.a = power;
        spriteRenderer.color = color;
    }
}
