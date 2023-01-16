using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftIntermission : MonoBehaviour
{
    [SerializeField]
    private List<AircraftSkin> tiers = null;

    public void SetSkin(int index)
    {
        foreach (var skin in tiers)
        {
            skin.SetSkin(index);
        }
    }

    public void ShowTier(int tier)
    {
        for (int i = 0; i < tiers.Count; i++)
        {
            tiers[i].gameObject.SetActive(i == tier);
        }
    }
}
