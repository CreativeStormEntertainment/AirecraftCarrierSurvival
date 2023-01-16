using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RedWaterData
{
    public RedWaterPopup Popup;
    public RectTransform Marker;
    public RectTransform Indicator;

    [Header("Idle Probability")]
    [Range(0f, 1f)]
    [SerializeField]
    private float idleChance = .1f;
    [SerializeField]
    private int idleTicksCap = 5;
    [SerializeField]
    private int gracePeriod = 5;

    public bool IsOnGracePeriod
    {
        get => isOnGracePeriod;
        set
        {
            if (value)
            {
                idleCounter = 0;
            }
            isOnGracePeriod = value;
        }
    }


    int idleCounter = 0;


    [Header("Road Probability")]
    [Range(0f, 1f)]
    [SerializeField]
    private float roadChance = .1f;
    [Range(0f, 1f)]
    public float RoadMin = .1f;
    [Range(0f, 1f)]
    public float RoadMax = .9f;

    [Range(0f, 1f)]
    public float AgressiveChance = .5f;

    private bool isOnGracePeriod = false;

    public bool Refresh()
    {
        if (Popup.gameObject.activeSelf)
        {
            return false;
        }
        ++idleCounter;
        if (IsOnGracePeriod)
        {
            if (idleCounter >= gracePeriod)
            {
                idleCounter = 0;
                IsOnGracePeriod = false;
            }
        }
        else
        {
            if (idleCounter >= idleTicksCap)
            {
                idleCounter = 0;
                return Random.value <= idleChance;
            }
        }
        return false;
    }

    public bool CheckOnRoad()
    {
        return !IsOnGracePeriod && Random.value <= roadChance;
    }
}
