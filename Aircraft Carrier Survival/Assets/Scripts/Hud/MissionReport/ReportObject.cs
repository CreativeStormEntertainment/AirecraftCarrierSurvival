using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReportObject : MonoBehaviour
{
    [SerializeField]
    private Image backgroud = null;
    [SerializeField]
    private Image flag = null;
    [SerializeField]
    private Text shipName = null;

    [SerializeField]
    private List<Image> durability = null;
    [SerializeField]
    private Sprite durabilityOn = null;
    [SerializeField]
    private Sprite durabilityOff = null;

    public void Setup(Sprite bg, Sprite fl, string ship, int currentDur, int maxDur)
    {
        backgroud.sprite = bg;
        flag.sprite = fl;
        shipName.text = ship;
        gameObject.SetActive(true);

        int count = durability.Count;
        int hideDurability = count - maxDur;
        int i;
        for (i = 0; i < hideDurability; i++)
        {
            durability[i].gameObject.SetActive(false);
        }
        for (int j = 0; i < durability.Count; i++, j++)
        {
            durability[i].gameObject.SetActive(true);
            durability[i].sprite = (j < currentDur ? durabilityOn : durabilityOff);
        }
    }
}
