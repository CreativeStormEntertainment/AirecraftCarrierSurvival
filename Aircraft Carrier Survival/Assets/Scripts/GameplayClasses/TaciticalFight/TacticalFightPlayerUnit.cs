using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TacticalFightPlayerUnit : TacticalFightBaseUnit
{
    [SerializeField]
    List<TacticalFightAbility> playerUnitAbillities = null;
    [SerializeField]
    ETacticalFightPlayerPlaneType planeType = ETacticalFightPlayerPlaneType.Torpedo;
    TacticalFightAbility chosenPlayerAbility;
    int pilotPoolIndex;
    TacticalFightMapField attackedField;
    ETacticalFightUnitRotationState directionOfAttack;
    int chancesToActivateAbillity;


    public void ActivateAbillity(List<TacticalFightMapField> fieldsToMove, TacticalFightMapField fieldToAttack, ETacticalFightUnitRotationState directionOfComingAttack, int chancesToGetDoneTheAbillity)
    {
        SetRotationState(TacticalFightUtils.GetRotationStateWithOffset(directionOfComingAttack, 3));
        fieldsToMove.Reverse();
        //fieldsToMove.Add(fieldToAttack);
        // change to repeat this action after each other

        //foreach (TacticalFightMapField fieldToMoveOn in fieldsToMove)
        //{
        //    base.Move(fieldToMoveOn);
        //}
        if (GetChosenPlayerAbillity().AbilityType == ETacticalFightAbilityType.OnFieldAttack)
        {
            TacticalFightEffectManager.Instance.ActivatePlayerAbility(this, fieldToAttack, directionOfComingAttack, chancesToGetDoneTheAbillity);
            base.Move(fieldsToMove[0],true,0.1f);
        }
        else 
            base.Move(fieldsToMove[fieldsToMove.Count-1],true,fieldsToMove.Count*TacticalFightVisualizationManager.Instance.OnAbillityUseMovementSpeed);

        attackedField = fieldToAttack;
        directionOfAttack = directionOfComingAttack;
        chancesToActivateAbillity = chancesToGetDoneTheAbillity;
    }

    public override void Attack()
    {
        base.Attack();

        if (GetChosenPlayerAbillity().AbilityType == ETacticalFightAbilityType.DirectionalAttack)
            TacticalFightEffectManager.Instance.ActivatePlayerAbility(this, attackedField, directionOfAttack, chancesToActivateAbillity);
    }

    public override void TakeDamage(int amountOfDamage)
    {
        if (playerUnitAbillities.FirstOrDefault(x => x.EffectType == ETacticalFightPlayerAbillityEffectType.Agile) != null)
            amountOfDamage = amountOfDamage / 2;

        TacticalFightManager.Instance.GetChosenPilot().DecreaseMoralePoints(amountOfDamage);
        //base.TakeDamage(amountOfDamage);

        //if (isDestroyed)
        //{
        //    TacticalFightManager.Instance.RemovePlayerUnitFromList(this);
        //    Destroy(gameObject);
        //}
    }

    public void SetCurrentChosenAbbillity(int index)
    {
        chosenPlayerAbility = playerUnitAbillities[index];
    }

    public void SetCurrentChosenAbbillity(TacticalFightAbility ability)
    {
        chosenPlayerAbility = ability;
    }


    public TacticalFightAbility GetChosenPlayerAbillity()
    {
        return chosenPlayerAbility;
    }

    public void SetPilotPoolIndex(int poolIndex) 
    {
        pilotPoolIndex = poolIndex;
    }

    public int GetPilotPoolIndex()
    {
        return pilotPoolIndex;
    }

    public void TakeFromMap()
    {
        TacticalFightManager.Instance.RemovePlayerUnitFromList(this);
        Destroy(gameObject);
    }

    public override void BlockUnit()
    {
        base.BlockUnit();
        TacticalFightManager.Instance.GetChosenPilot().BlockPlayerUnitInPool(pilotPoolIndex);
    }

    public override void UnblockUnit()
    {
        base.UnblockUnit();
        TacticalFightManager.Instance.GetChosenPilot().UnblockPlayerUnitInPool(pilotPoolIndex);
    }

    public List<TacticalFightAbility> GetPlayerAbillities()
    {
        return playerUnitAbillities;
    }

    public ETacticalFightPlayerPlaneType GetPlaneType()
    {
        return planeType;
    }
}
