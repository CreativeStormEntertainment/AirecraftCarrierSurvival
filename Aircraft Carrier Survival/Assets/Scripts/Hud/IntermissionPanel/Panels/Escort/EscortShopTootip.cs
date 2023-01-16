using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscortShopTootip : MonoBehaviour
{
    [SerializeField]
    private Text text = null;

    public void Show(StrikeGroupMemberData data)
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
