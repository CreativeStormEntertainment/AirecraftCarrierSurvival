using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

public class TacticalFightEnemyUnit : TacticalFightBaseUnit
{
    protected static int attackDamageOnFields = 1;
    int amountOfPercentageToGetLower = 10;
    [SerializeField]
    List<TacticalFightEnemyAbility> enemyAbilities = null;
    [SerializeField]
    bool isNeutral = false;
    ETacticalFightEnemyDestinationType enemyDestinationType;
    Vector2Int pointToMove;
    TacticalFightBaseUnit unitToMove;
    bool canBeAttackedByPlayer;
    bool isMarked;
    Image roundCounterImage;
    Text roundCounterText;
    bool isHeavyDamaged;

    [SerializeField]
    ETacticalFightEnemyType enemyType = ETacticalFightEnemyType.ZeroPlane;

    List<TacticalFightMapField> listOfFieldsToAttack = new List<TacticalFightMapField>();

    public void InitializeEnemy()
    {
        roundCounterImage = transform.GetChild(1).GetComponent<Image>();
        roundCounterText = transform.GetChild(1).GetComponentInChildren<Text>(true);

        foreach (TacticalFightEnemyAbility abillityToActivate in enemyAbilities)
        {
            abillityToActivate.ResetCurrentAmountOfRoundsToWait();

            switch (abillityToActivate.EffectType)
            {
                case (ETacticalFightEnemyAbillityEffectType.KumaEffect):
                    attackDamageOnFields = 2;
                    break;

            }

            roundCounterText.text = abillityToActivate.GetCurrentAmountOfRoundsToWait().ToString();

            if(abillityToActivate.GetCurrentAmountOfRoundsToWait() == 0)
                roundCounterImage.gameObject.SetActive(false);
        }

        if (isNeutral)
            roundCounterImage.gameObject.SetActive(false);
    }

    public void MoveToNextRound()
    {
        if (isBlocked == false)
        {
            List<TacticalFightMapField> mapFieldsToGoOn = TacticalFightManager.Instance.GetPossibleToMoveForEnemyFields(this, GetAcquiredField(), movementSpeed, currentRotationState);
            
            if (!TacticalFightManager.Instance.GetIsPossibleToMoveByUnit(this))
                RemoveEnemyFromMap();

            if (mapFieldsToGoOn.Count > 0)
                Move(mapFieldsToGoOn[mapFieldsToGoOn.Count - 1], true);
        }
    }

    public override void Attack()
    {
        base.Attack();

        if (isBlocked == false)
        {
            foreach (TacticalFightEnemyAbility abillityToActivate in enemyAbilities)
            {
                bool isCounterStoped = false;

                if (abillityToActivate.EffectType == ETacticalFightEnemyAbillityEffectType.KagaEffect && TacticalFightManager.Instance.GetEnemyPlaneOnMapCount() > 2)
                    isCounterStoped = true;

                if (!isCounterStoped)
                {
                    if (abillityToActivate.GetCurrentAmountOfRoundsToWait() <= 0)
                    {
                        if (TacticalFightEffectManager.Instance.ActivateEnemyAbilityEffect(this, abillityToActivate))
                            abillityToActivate.ResetCurrentAmountOfRoundsToWait();
                    }
                    else
                    {
                        abillityToActivate.SetCurrentAmountOfRoundsToWait(abillityToActivate.GetCurrentAmountOfRoundsToWait() - 1);
                    }

                    roundCounterText.text = abillityToActivate.GetCurrentAmountOfRoundsToWait().ToString();
                }
            }
        }
    }

    public void ShowNextAttack()
    {
        UnSetVisualizationForAttack();
        SetVisualizationForAttack();
    }

    public override void TakeDamage(int receivedDamage)
    {
        if (!isNeutral)
        {
            base.TakeDamage(receivedDamage);

            if(!isHeavyDamaged && GetCurrentHealth() < GetHealthOnStart() * 0.5f)
            {
                isHeavyDamaged = true;
                // change mesh visualization here
            }


            if (isDestroyed)
            {
                RemoveEnemyFromMap();
            }
        }
    }

    public void RemoveEnemyFromMap()
    {
        UnSetVisualizationForAttack();
        TacticalFightManager.Instance.RemoveEnemyFromList(this);
        TacticalFightManager.Instance.ChoseRandomEnemy();
        Destroy(gameObject);
    }

    public int GetAttackDamageOnField()
    {
        return attackDamageOnFields;
    }

    public int GetAmountOfPercentageToGetLower()
    {
        return amountOfPercentageToGetLower;
    }

    public override void SetVisualizationForAttack()
    {
        if (enemyAbilities.FirstOrDefault(x => x.EffectType == ETacticalFightEnemyAbillityEffectType.WakatakeEffect) != null)
        {
            TacticalFightManager.Instance.GetFieldsAroundField(GetAcquiredField(), true).ForEach(x => x.SetIsWakatakeEffectOnField(true));
        }

        listOfFieldsToAttack = TacticalFightManager.Instance.GetPossibleToAttackFieldsForEnemyUnit(this, GetAcquiredField());
        listOfFieldsToAttack.ForEach(x => x.AddEnemyThatWillAttackOnFieldToList(this));
        listOfFieldsToAttack.ForEach(x => x.SetFieldVisualization());

        if (!TacticalFightHudManager.Instance.GetIsShowingAttackFieldsAll())
        {
            GetAcquiredField().ShowAttackDirectionImage(currentRotationState);
        }
    }

    public override void UnSetVisualizationForAttack()
    {
        if (enemyAbilities.FirstOrDefault(x => x.EffectType == ETacticalFightEnemyAbillityEffectType.WakatakeEffect) != null)
        {
            TacticalFightManager.Instance.GetFieldsAroundField(GetAcquiredField(), true).ForEach(x => x.SetIsWakatakeEffectOnField(false));
        }
        listOfFieldsToAttack.ForEach(x => x.RemoveEnemyThatWillAttackOnFieldFromList(this));
        listOfFieldsToAttack.ForEach(x => x.SetFieldVisualization());
        listOfFieldsToAttack = new List<TacticalFightMapField>();

        if (!TacticalFightHudManager.Instance.GetIsShowingAttackFieldsAll())
        {
            GetAcquiredField().HideAttackDirectionImage(currentRotationState);
        }
    }

    public void SetCanBeAttackedByPlayer(bool canBe)
    {
        canBeAttackedByPlayer = canBe;
    }

    public bool GetCanBeAttackedByPlayer()
    {
        return canBeAttackedByPlayer;
    }

    public void SetIsMarked(bool marked)
    {
        isMarked = marked;
        GetAcquiredField().SetFieldVisualization();
    }

    public bool GetIsMarked()
    {
        return isMarked;
    }

    public bool GetIsNeutral()
    {
        return isNeutral;
    }
    public void SetIsHeavyDamaged(bool isdamaged)
    {
        isHeavyDamaged = isdamaged;
    }
    public bool GetIsHeavyDamaged()
    {
        return isHeavyDamaged;
    }

    public List<TacticalFightEnemyAbility> GetEnemyAbilities()
    {
        return enemyAbilities;
    }

    public ETacticalFightEnemyType GetEnemyType()
    {
        return enemyType;
    }
}

public enum ETacticalFightEnemyDestinationType
{
    direction,
    point,
    unit
}
