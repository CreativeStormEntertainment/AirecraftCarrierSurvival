using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TacticalFightBaseUnit : MonoBehaviour
{
    [SerializeField]
    List<Sprite> unitSprites = null;
    [SerializeField]
    List<Sprite> unitShadowSprites = null;
    [SerializeField]
    protected ETacticalFightUnitType unitType = ETacticalFightUnitType.None;
    protected bool isDestroyed;
    [SerializeField]
    protected int healthOnStart = 1;
    protected int currentHealth;
    [SerializeField]
    protected int movementSpeed = 0;
    [SerializeField]
    protected int attackRange = 0;
    TacticalFightMapField acquiredField;
    protected ETacticalFightUnitRotationState currentRotationState;
    [SerializeField]
    ETacticalFightUnitMovementType unitMovementType = ETacticalFightUnitMovementType.None;
    ETacticalFightUnitDamageType damageTypeCanTake;
    ETacticalFightUnitDamageType damageTypeCanGive = ETacticalFightUnitDamageType.None;
    protected bool isBlocked;
    private Image shadowImage;
    private Image unitImage;
    [SerializeField]
    string unitName = "";
    [SerializeField]
    string unitDescription = "";

    private void Awake()
    {
        shadowImage = GetComponent<Image>();
        unitImage = transform.GetChild(0).GetComponent<Image>();
        currentHealth = healthOnStart;
    }

    public virtual void Move(TacticalFightMapField fieldToMove, bool isAttackPerformedAfterMove, float time=1f)
    {
        Vector2 startPosition;
        Vector2 endPosition;

        if(acquiredField != null)
            startPosition = acquiredField.GetComponent<RectTransform>().anchoredPosition;
        else
            startPosition = fieldToMove.GetComponent<RectTransform>().anchoredPosition;

        endPosition = fieldToMove.GetComponent<RectTransform>().anchoredPosition;

        StartCoroutine(MoveUnitToPositionAndAttack(startPosition, endPosition, fieldToMove, isAttackPerformedAfterMove, time));
    }

    public virtual void Attack()
    {
    }

    public virtual void TakeDamage(int amountOfDamage)
    {
        currentHealth -= amountOfDamage;

        if (currentHealth <= 0)
        {
            isDestroyed = true;
        }
    }

    public void SetRotationState(ETacticalFightUnitRotationState rotationStateToSet)
    {
        currentRotationState = rotationStateToSet;
        shadowImage.sprite = unitShadowSprites[(int)currentRotationState];
        unitImage.sprite = unitSprites[(int)currentRotationState];
    }

    public void SetAcquiredField(TacticalFightMapField mapField)
    {
        acquiredField = mapField;
    }

    public string GetUnitName()
    {
        return unitName;
    }

    public string GetUnitDescription()
    {
        return unitDescription;
    }

    public TacticalFightMapField GetAcquiredField()
    {
        return acquiredField;
    }

    public int GetMovementSpeed()
    {
        return movementSpeed;
    }

    public ETacticalFightUnitMovementType GetUnitMovementType()
    {
        return unitMovementType;
    }

    public ETacticalFightUnitDamageType GetDamageTypeGiven()
    {
        return damageTypeCanGive;
    }

    public ETacticalFightUnitRotationState GetRotationState()
    {
        return currentRotationState;
    }

    public int GetAttackRange()
    {
        return attackRange;
    }
    public void SetAttackRange(int range)
    {
       attackRange = range;
    }

    public int GetMaxHealth()
    {
        return healthOnStart;
    }

    public void SetCurrentHealth(int amountOfHealth)
    {
        currentHealth = amountOfHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public ETacticalFightUnitType GetUnitType()
    {
        return unitType;
    }

    public virtual void SetVisualizationForAttack()
    {

    }
    public virtual void UnSetVisualizationForAttack()
    {

    }

    public virtual void BlockUnit()
    {
        isBlocked = true;
    }

    public virtual void UnblockUnit()
    {
        isBlocked = false;
    }

    public int GetHealthOnStart()
    {
        return healthOnStart;
    }

    private IEnumerator MoveUnitToPositionAndAttack(Vector2 startPosition, Vector2 endPosition, TacticalFightMapField fieldToMove, bool isAttackPerformedAfterMove, float time = 1)
    {
        float elapsedTime = 0;

        UnSetVisualizationForAttack();

        acquiredField.RemoveAcquiredUnitFromField(this);
        if(unitType.HasFlag(ETacticalFightUnitType.Ship))
            fieldToMove.PlaceShipUnitOnField(this);
        else
            fieldToMove.PlacePlaneUnitOnField(this);

        while (elapsedTime < time)
        {
            GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPosition, endPosition, (elapsedTime / time));
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        GetComponent<RectTransform>().anchoredPosition = endPosition;
        SetVisualizationForAttack();

        if (isAttackPerformedAfterMove == true)
        {
            if (this is TacticalFightPlayerUnit)
                Attack();
        }
        else
        {
            TakeDamage(1);
            TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.EnemyRound);
        }
        yield return null;
    }

}
