using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReconReport : MonoBehaviour
{
    public Sprite EnemyFleet => enemyFleet;
    public Sprite EnemyBase => enemyBase;
    public Sprite NeutralBase => neutralBase;
    public Sprite NeutralShip => neutralShip;
    public Sprite FriendlyShip => friendlyShip;
    public Sprite FriendlyBase => friendlyBase;
    public Sprite Whales => whales;
    public Sprite Survivors => survivors;

    [SerializeField]
    private Text description = null;
    [SerializeField]
    private List<ReconElement> elements = null;
    [SerializeField]
    private Sprite enemyFleet = null;
    [SerializeField]
    private Sprite enemyBase = null;
    [SerializeField]
    private Sprite neutralBase = null;
    [SerializeField]
    private Sprite neutralShip = null;
    [SerializeField]
    private Sprite friendlyShip = null;
    [SerializeField]
    private Sprite friendlyBase = null;
    [SerializeField]
    private Sprite whales = null;
    [SerializeField]
    private Sprite survivors = null;

    public void SetupRecon(List<ITacticalObjectHelper> objects, ReportPanel panel)
    {
        Clear();
        var enemySpotted = false;
        for (int i = 0; i < objects.Count; i++)
        {
            if (elements[i].Setup(objects[i], panel, this))
            {
                enemySpotted = true;
            }
            elements[i].gameObject.SetActive(true);
        }
        description.text = enemySpotted ? panel.ReconStrings.GoodText : panel.ReconStrings.BadText;
    }

    private void Clear()
    {
        foreach (var ele in elements)
        {
            ele.gameObject.SetActive(false);
        }
    }
}
