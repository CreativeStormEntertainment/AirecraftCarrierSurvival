using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReconElement : MonoBehaviour
{
    [SerializeField]
    private List<ReportObject> enemyShips = null;
    [SerializeField]
    private Text objectName = null;
    [SerializeField]
    private Image enemyTypeImage = null;
    [SerializeField]
    private Image notEnemyImage = null;

    public bool Setup(ITacticalObjectHelper tacticalObject, ReportPanel panel, ReconReport recon)
    {
        Clear();
        if (tacticalObject is TacticalEnemyShip enemyFleet && enemyFleet.Side == ETacticalObjectSide.Enemy)
        {
            enemyTypeImage.gameObject.SetActive(true);
            switch (enemyFleet.Type)
            {
                case ETacticalObjectType.Outpost:
                    enemyTypeImage.sprite = recon.EnemyBase;
                    break;
                case ETacticalObjectType.StrikeGroup:
                    enemyTypeImage.sprite = recon.EnemyFleet;
                    break;
            }
            for (int i = 0; i < enemyFleet.Blocks.Count; i++)
            {
                objectName.text = enemyFleet.LocalizedName;
                var data = enemyFleet.Blocks[i];
                Sprite sprite;
                if (data.Visible)
                {
                    if (data.Dead)
                    {
                        sprite = panel.Dead;
                    }
                    else
                    {
                        sprite = data.WasDetected ? panel.NotDead : panel.Detected;
                    }
                }
                else
                {
                    sprite = panel.NotDead;
                }
                enemyShips[i].Setup(sprite, panel.JapanFlag, (data.Visible ? data.Data.LocalizedName : "???"), (data.Dead ? 0 : data.CurrentDurability), (data.Visible ? data.Data.Durability : 0));
            }
            return true;
        }
        else
        {
            bool showImage = true;
            switch (tacticalObject.Type)
            {
                case ETacticalObjectType.Nothing:
                    showImage = false;
                    objectName.text = panel.NothingSpottedText;
                    break;
                case ETacticalObjectType.Outpost:
                    objectName.text = panel.OutpostText;
                    notEnemyImage.sprite = recon.NeutralBase;
                    break;
                case ETacticalObjectType.StrikeGroup:
                    objectName.text = panel.StrikeGroupText;
                    notEnemyImage.sprite = recon.NeutralShip;
                    break;
                case ETacticalObjectType.Whales:
                    objectName.text = panel.WhalesText;
                    notEnemyImage.sprite = recon.Whales;
                    break;
                case ETacticalObjectType.Survivors:
                    objectName.text = panel.SurvivorsText;
                    notEnemyImage.sprite = recon.Survivors;
                    break;
            }
            notEnemyImage.gameObject.SetActive(showImage);
            notEnemyImage.SetNativeSize();
            notEnemyImage.transform.parent.gameObject.SetActive(true);
            return false;
        }
    }

    private void Clear()
    {
        foreach (var enemy in enemyShips)
        {
            enemy.gameObject.SetActive(false);
        }
        enemyTypeImage.gameObject.SetActive(false);
        notEnemyImage.transform.parent.gameObject.SetActive(false);
    }
}
