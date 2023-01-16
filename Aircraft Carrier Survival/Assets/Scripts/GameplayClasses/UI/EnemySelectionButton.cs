using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySelectionButton : MonoBehaviour
{
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private Text nameText = null;
    [SerializeField]
    private Text attackText = null;
    [SerializeField]
    private Text defText = null;

    private TacticalEnemyShip enemy;

    private EStrikeGroupActiveSkill activeSkill;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    public void SetupEnemy(TacticalEnemyShip enemyShip, EStrikeGroupActiveSkill skill)
    {
        activeSkill = skill;
        enemy = enemyShip;
        nameText.text = enemy.LocalizedName;
        var minDef = 0f;
        var minAtt = 0f;
        var maxDef = 0f;
        var maxAtt = 0f;
        foreach (EnemyManeuverInstanceData instanceData in enemyShip.Blocks)
        {
            if (!instanceData.Dead && instanceData.Visible)
            {
                var manouver = instanceData.Data;
                minDef += manouver.MinValues.Defense;
                minAtt += manouver.MinValues.Attack;
                maxDef += manouver.MaxValues.Defense;
                maxAtt += manouver.MaxValues.Attack;
            }
        }
        defText.text = minDef.ToString() + "-" + maxDef.ToString();
        attackText.text = minAtt.ToString() + "-" + maxAtt.ToString();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnClick()
    {
        StrikeGroupManager.Instance.ActivateEnemySelectionSkill(activeSkill, enemy);
        UIManager.Instance.StrikeGroupSelectionWindow.Hide();
    }
}
