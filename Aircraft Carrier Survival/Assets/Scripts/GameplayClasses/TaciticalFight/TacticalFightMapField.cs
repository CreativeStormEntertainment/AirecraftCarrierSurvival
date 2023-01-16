using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TacticalFightMapField : MonoBehaviour
{
    bool isLand;
    bool isFlightVisualizationActive;
    bool isPossibleToMove;
    bool isPossibleToAttackByPlayer;
    bool isPossibleToSpawnHerePlayerUnit;
    bool isPossibleToMoveEnemyByPlayer;
    bool isFieldClouded;
    bool isHighLighted;
    TacticalFightBaseUnit acquiredShipUnit;
    TacticalFightBaseUnit acquiredPlaneUnit;
    [SerializeField]
    int xPosOfElement;
    [SerializeField]
    int yPosOfElement;
    Image fieldImage;
    Image aboveFieldImage;
    List<Image> attackDirectionImages;
    List<TacticalFightEnemyData> enemyUnitsAssignedToSpawnOnField = new List<TacticalFightEnemyData>();
    List<TacticalFightEnemyUnit> enemiesThatwillAttackOnThisField = new List<TacticalFightEnemyUnit>();
    ETacticalFightUnitType unitType;
    bool isSelected;
    bool isWakatakeEffectOnField;

    void Awake()
    {
        fieldImage = transform.GetChild(0).GetComponent<Image>(); //GetComponent<Image>();
        aboveFieldImage = transform.GetChild(1).GetComponent<Image>(); //transform.GetChild(1).GetComponent<Image>();
        attackDirectionImages = transform.GetChild(2).GetComponentsInChildren<Image>(true).ToList();
    }

    public void ResetField()
    {
        enemyUnitsAssignedToSpawnOnField = new List<TacticalFightEnemyData>();
        enemiesThatwillAttackOnThisField = new List<TacticalFightEnemyUnit>();
        acquiredShipUnit = null;
        acquiredPlaneUnit = null;
        isFieldClouded = false;
        isLand = false;
        isWakatakeEffectOnField = false;
        isSelected = false;
        isHighLighted = false;
        isPossibleToMove = false;
        isPossibleToAttackByPlayer = false;
    }

    public void InitializeField()
    {
        if (isLand)
            unitType = ETacticalFightUnitType.FieldWithLand;
        else
            unitType = ETacticalFightUnitType.FieldWithWater;

        if (isFieldClouded)
        {
            aboveFieldImage.gameObject.SetActive(true);
            aboveFieldImage.color = new Color(1, 1, 1, 0.6f);
            aboveFieldImage.sprite = TacticalFightVisualizationManager.Instance.GetRandomCloudSprite();
        }
        else
        {
            aboveFieldImage.gameObject.SetActive(false);
        }


        SetFieldVisualization();
    }

    public void CreateEnemyOnField()
    {
        TacticalFightEnemyData enemyDataFromList = GetNextEnemyDataFromList();

        if(enemyDataFromList != null)
        {
            if (!IsAcquiredEnemyShipUnitToField() && !IsAcquiredEnemyPlaneUnitToField())
            {
                TacticalFightEnemyUnit createdEnemy = TacticalFightManager.Instance.InstiateEnemyUnit(TacticalFightEnemyManager.Instance.GetEnemyUnit(enemyDataFromList.TypeOfEnemy), this);
                createdEnemy.SetRotationState(enemyDataFromList.EnemyDirection);
                createdEnemy.ShowNextAttack();
            }
        }
    }

    public void SetFieldVisualization()
    {
        fieldImage.color = new Color(0, 0, 0, 0.01f);//Color.clear;
        fieldImage.sprite = TacticalFightVisualizationManager.Instance.FieldVisualizationAttackedByPlayer;

        if (GetIsHighLighted())
        {
            fieldImage.color = Color.white;
            fieldImage.sprite = TacticalFightVisualizationManager.Instance.FieldOnHighLight;
        }

        if (TacticalFightHudManager.Instance.GetIsShowingAttackFieldsAll())
        {
            if (GetIsFieldAttackedByEnemy())
            {
                fieldImage.color = Color.white;
                fieldImage.sprite = TacticalFightVisualizationManager.Instance.FieldVisualizationAttackcedByEnemy;
            }
        }

        if (isPossibleToAttackByPlayer)
        {
            fieldImage.color = Color.white;
            fieldImage.sprite = TacticalFightVisualizationManager.Instance.FieldVisualizationAttackedByPlayer;
            //if (isPossibleToMoveEnemyByPlayer)
            //    fieldImage.sprite = TacticalFightVisualizationManager.Instance.FieldVisualizationCanEnemyBeMovedAfterPlayerAttack;
        }

        if (isLand && TacticalFightHudManager.Instance.GetIsShowingLand())
        {
            fieldImage.color = Color.green;
            fieldImage.sprite = TacticalFightVisualizationManager.Instance.FieldVisualizationAttackedByPlayer;
        }

        fieldImage.alphaHitTestMinimumThreshold = 0.001f;
    }


    public void ShowAttackDirectionImage(ETacticalFightUnitRotationState rotationState)
    {
        attackDirectionImages[(int)rotationState].gameObject.SetActive(true);
    }

    public void HideAttackDirectionImage(ETacticalFightUnitRotationState rotationState)
    {
        attackDirectionImages[(int)rotationState].gameObject.SetActive(false);
    }

    public TacticalFightPlayerUnit GetAcquiredPlayerPlaneUnit()
    {
        return acquiredPlaneUnit as TacticalFightPlayerUnit;
    }

    public TacticalFightEnemyUnit GetAcquiredEnemyShipUnit()
    {
        return acquiredShipUnit as TacticalFightEnemyUnit;
    }

    public TacticalFightEnemyUnit GetAcquiredEnemyPlaneUnit()
    {
        return acquiredPlaneUnit as TacticalFightEnemyUnit;
    }

    public bool IsAcquiredPlayerUnitToField()
    {
        return acquiredPlaneUnit != null && acquiredPlaneUnit is TacticalFightPlayerUnit;
    }

    public bool IsAcquiredEnemyShipUnitToField()
    {
        return acquiredShipUnit != null && acquiredShipUnit is TacticalFightEnemyUnit;
    }

    public bool IsAcquiredEnemyPlaneUnitToField()
    {
        return acquiredPlaneUnit != null && acquiredPlaneUnit is TacticalFightEnemyUnit;
    }
    public bool IsAcquiredAnyEnemyUnitToField()
    {
        return acquiredPlaneUnit is TacticalFightEnemyUnit || acquiredShipUnit is TacticalFightEnemyUnit;
    }

    public bool IsAcquiredUnitToField()
    {
        return acquiredPlaneUnit != null || acquiredShipUnit != null;
    }

    public void PlaceShipUnitOnField(TacticalFightBaseUnit tacticalFightShipUnit)
    {
        acquiredShipUnit = tacticalFightShipUnit;
        acquiredShipUnit.SetAcquiredField(this);
    }

    public void PlacePlaneUnitOnField(TacticalFightBaseUnit tacticalFightPlaneUnit)
    {
        acquiredPlaneUnit = tacticalFightPlaneUnit;
        acquiredPlaneUnit.SetAcquiredField(this);
    }

    public void RemoveAcquiredUnitFromField(TacticalFightBaseUnit unitToRemove)
    {
        if (acquiredPlaneUnit == unitToRemove)
            acquiredPlaneUnit = null;
        else if (acquiredShipUnit == unitToRemove)
            acquiredShipUnit = null;
    }

    public void SetIsLand(bool isGround)
    {
        isLand = isGround;
    }

    public bool GetIsLand()
    {
        return isLand;
    }

    public void SetIsFieldClouded(bool isClouded)
    {
        isFieldClouded = isClouded;
    }

    public bool GetIsFieldClouded()
    {
        return isFieldClouded;
    }

    public bool GetIsPossibleToMove()
    {
        return isPossibleToMove;
    }

    public void SetIsPossibleToMove(bool isPossible)
    {
        isPossibleToMove = isPossible;
    }

    public void SetIsPossibleToSpawnHerePlayerUnit(bool isPossible)
    {
        isPossibleToSpawnHerePlayerUnit = isPossible;
    }

    public void SetIsPossibleToMoveEnemyByPlayer(bool isPossible)
    {
        isPossibleToMoveEnemyByPlayer = isPossible;
    }

    public void SetIsFlightVisualizationActive(bool isActive)
    {
        isFlightVisualizationActive = isActive;
    }

    public bool GetIsFieldAttackedByEnemy()
    {
        return enemiesThatwillAttackOnThisField.Count > 0;
    }

    public bool GetIsFieldAttackedByMoreThanOneEnemy()
    {
        return enemiesThatwillAttackOnThisField.Count > 1;
    }

    public void AddEnemyThatWillAttackOnFieldToList(TacticalFightEnemyUnit enemyUnit)
    {
        enemiesThatwillAttackOnThisField.Add(enemyUnit);
    }

    public void RemoveEnemyThatWillAttackOnFieldFromList(TacticalFightEnemyUnit enemyUnit)
    {
        enemiesThatwillAttackOnThisField.Remove(enemyUnit);
    }

    public int GetAmountOfChanceToGetLowerOnField()
    {
        int amount = 0;
        foreach(TacticalFightEnemyUnit unitWillAttackOnField in enemiesThatwillAttackOnThisField)
        {
            if (amount < unitWillAttackOnField.GetAmountOfPercentageToGetLower())
                amount = unitWillAttackOnField.GetAmountOfPercentageToGetLower();
        }

        return amount;
    }

    public TacticalFightEnemyUnit GetFirstEnemyThatWillAttackOnField()
    {
        return enemiesThatwillAttackOnThisField[0];
    }
    public List<TacticalFightEnemyUnit> GetEnemiesThatWillAttackOnField()
    {
        return enemiesThatwillAttackOnThisField;
    }

    public bool GetIsPossibleToAttackByPlayer()
    {
        return isPossibleToAttackByPlayer;
    }

    public void SetIsPossibleToAttackByPlayer(bool isPossible)
    {
        isPossibleToAttackByPlayer = isPossible;
    }


    public void SetPositionOfElement(int x, int y)
    {
        xPosOfElement = x;
        yPosOfElement = y;
    }

    public Vector2Int GetPositionOnMap()
    {
        return new Vector2Int(xPosOfElement, yPosOfElement);
    }

    public void SetEnemyListOnField(List<TacticalFightEnemyData> enemyDatas)
    {
        enemyUnitsAssignedToSpawnOnField = enemyDatas;
    }

    public ETacticalFightUnitType GetUnitType()
    {
        return unitType;
    }

    public void SetIsSelected(bool selected)
    {
        isSelected = selected;
    }

    public bool GetIsSelected()
    {
        return isSelected;
    }

    public void SetIsHighLighted(bool highLighted)
    {
        isHighLighted = highLighted;
    }

    public bool GetIsHighLighted()
    {
        return isHighLighted;
    }

    public void SetIsWakatakeEffectOnField(bool isEffectOnField)
    {
        isWakatakeEffectOnField = isEffectOnField;
    }

    public bool GetIsWakatakeEffectOnField()
    {
        return isWakatakeEffectOnField;
    }

    private TacticalFightEnemyData GetNextEnemyDataFromList()
    {
        if (enemyUnitsAssignedToSpawnOnField.Count == 1)
        {
            return enemyUnitsAssignedToSpawnOnField[0];
        }
        else if (enemyUnitsAssignedToSpawnOnField.Count > 1)
        {
            return enemyUnitsAssignedToSpawnOnField[Random.Range(0, enemyUnitsAssignedToSpawnOnField.Count - 1)];
        }
        else
        {
            return null;
        }
    }
}
