using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoPanelObject : MonoBehaviour
{
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Sprite dead = null;
    [SerializeField]
    private Sprite enemy = null;
    [SerializeField]
    private Sprite friend = null;
    [SerializeField]
    private Color destroyedColor = new Color(0.8f, 0.8f, 0.8f);

    [SerializeField]
    private List<Image> durability = null;
    [SerializeField]
    private Sprite durabilityOn = null;
    [SerializeField]
    private Sprite durabilityOff = null;

    public void Setup(EnemyManeuverInstanceData data, bool isFriend)
    {
        gameObject.SetActive(true);
        title.gameObject.SetActive(true);
        title.color = data.Dead ? destroyedColor : Color.white;
        if (data.Dead)
        {
            image.sprite = dead;
        }
        else
        {
            image.sprite = isFriend ? friend : enemy;
        }
        title.text = data.Visible ? data.Data.LocalizedName : "???";
        if (isFriend || !data.Visible || data.Dead)
        {
            foreach (var image in durability)
            {
                image.gameObject.SetActive(false);
            }
        }
        else
        {
            int count = durability.Count;
            int hideDurability = count - data.Data.Durability;
            int i;
            for (i = 0; i < hideDurability; i++)
            {
                durability[i].gameObject.SetActive(false);
            }
            for (int j = 0; i < durability.Count; i++, j++)
            {
                durability[i].gameObject.SetActive(true);
                durability[i].sprite = (j < data.CurrentDurability ? durabilityOn : durabilityOff);
            }
        }
    }
}
