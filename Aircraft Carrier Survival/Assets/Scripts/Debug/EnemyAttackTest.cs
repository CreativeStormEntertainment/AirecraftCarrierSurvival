using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAttackTest : MonoBehaviour
{
    [SerializeField]
    private InputField attackPowerInput = null;

    [SerializeField]
    private Toggle strikeGroupToggle = null;
    [SerializeField]
    private Toggle friendToggle = null;

    [SerializeField]
    private Text result = null;

    public void Attack()
    {
        if (!friendToggle.isOn)
        {
            var data = GetData();
            data.Type = EEnemyAttackType.Raid;
            EnemyAttacksManager.Instance.CreateEnemyAttack(data, 0, true, true);
        }
        else
        {
            var friendAttack = new EnemyAttackFriendData();
            friendAttack.FriendID = 0;
            friendAttack.AttackPower = 1;
            EnemyAttacksManager.Instance.CreateFriendlyAttack(friendAttack, 0);
        }
    }

    public void AttackRecon()
    {
        var data = GetData();
        data.Type = EEnemyAttackType.Scout;
        if (data != null)
        {
            EnemyAttacksManager.Instance.CreateEnemyAttack(data, 0, true, true);
        }
    }

    public void AttackSubmarine()
    {
        var data = GetData();
        data.Type = EEnemyAttackType.Submarine;
        if (data != null)
        {
            EnemyAttacksManager.Instance.CreateEnemyAttack(data, 0, true, true);
        }
    }

    public void AttackNoRadar()
    {
        var data = GetData();
        if (data != null)
        {
            if (data.Type != EEnemyAttackType.Raid)
            {
                result.text = "Fail, bad button";
                return;
            }

            EnemyAttacksManager.Instance.StartAttack(data);
        }
    }

    private EnemyAttackData GetData()
    {
        var data = new EnemyAttackData();
        data.AttackPower = -1;

        bool strikeGroup;

        try
        {
            data.AttackPower = int.Parse(attackPowerInput.text);
            strikeGroup = strikeGroupToggle.isOn;
        }
        catch (Exception)
        {
            result.text = "Fail";
            return null;
        }

        string text;

        if (data.AttackPower < 1)
        {
            result.text = "Fail";
            return null;
        }
        else
        {
            text = "AP:" + data.AttackPower;
        }
        text += ";target:";
        if (strikeGroup)
        {
            text += "strike group";
            data.CurrentTarget = EEnemyAttackTarget.StrikeGroup;
        }
        else
        {
            text += "carrier";
            data.CurrentTarget = EEnemyAttackTarget.Carrier;
        }

        result.text = text;
        return data;
    }
}
