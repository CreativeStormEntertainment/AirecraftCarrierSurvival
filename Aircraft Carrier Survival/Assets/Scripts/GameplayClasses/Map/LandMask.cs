using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMask : MonoBehaviour
{
    [SerializeField] private RectTransform map = null;
    [SerializeField] private RectTransform rectTransform = null;

    void LateUpdate()
    {
        rectTransform.position = map.position;
        rectTransform.sizeDelta = map.sizeDelta;
    }
}
