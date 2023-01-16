using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[System.Serializable]
public class MapMarker : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvas = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private Image circleImage = null;
    [SerializeField] private bool startingState = false;

    public void ToggleVisibility(bool isVisible)
    {
        if (isVisible)
        {
            if (canvas != null)
                canvas.alpha = 1;
            iconImage.color = Color.white;
            circleImage.color = Color.white;
            return;
        }
        if (canvas != null)
            canvas.alpha = 0;
        iconImage.color = Color.clear;
        circleImage.color = Color.clear;
    }

    private void Start()
    {
        ToggleVisibility(startingState);
    }
}