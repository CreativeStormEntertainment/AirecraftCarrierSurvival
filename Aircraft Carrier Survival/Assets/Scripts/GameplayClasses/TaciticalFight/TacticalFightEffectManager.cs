using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Playables;

public class TacticalFightEffectManager : MonoBehaviour
{
    public static TacticalFightEffectManager Instance;

    TacticalFightMap map;

    void Awake()
    {
        Instance = this;
        map = GetComponentInChildren<TacticalFightMap>();
    }

    public List<TacticalFightMapField> SetFieldsToVisualizeForPlayerAbilityEffect(TacticalFightAbility playerAbility, TacticalFightMapField chosenField, ETacticalFightUnitRotationState rotationForAbility)
    {
        List<TacticalFightMapField> fieldsMarkToAttack = new List<TacticalFightMapField>();

        switch (playerAbility.EffectType)
        {
            case (ETacticalFightPlayerAbillityEffectType.DirectAttack):
                fieldsMarkToAttack.Add(chosenField);
                chosenField.SetIsPossibleToAttackByPlayer(true);
                chosenField.SetFieldVisualization();
                break;

            case (ETacticalFightPlayerAbillityEffectType.TorpedoStrike):
                fieldsMarkToAttack.Add(chosenField);
                fieldsMarkToAttack.AddRange(map.GetPossibleFieldsToMark(chosenField, 2, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility,3)));
                fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToAttackByPlayer(true));
                fieldsMarkToAttack.ForEach(x => x.SetFieldVisualization());
                break;

            case (ETacticalFightPlayerAbillityEffectType.Bombing):
                fieldsMarkToAttack.Add(chosenField);
                fieldsMarkToAttack.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 1)));
                fieldsMarkToAttack.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, -1)));
                fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToAttackByPlayer(true));
                fieldsMarkToAttack.ForEach(x => x.SetFieldVisualization());
                break;

            case (ETacticalFightPlayerAbillityEffectType.FlyBy):
                fieldsMarkToAttack.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3)));
                fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToMoveEnemyByPlayer(true));
                fieldsMarkToAttack.Add(chosenField);
                fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToAttackByPlayer(true));
                fieldsMarkToAttack.ForEach(x => x.SetFieldVisualization());
                break;

            //case (ETacticalFightPlayerAbillityEffectType.DiveBombing):
            //    List<TacticalFightMapField> possibleFieldsToMoveEnemy = new List<TacticalFightMapField>();
            //    possibleFieldsToMoveEnemy.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, chosenField.GetAcquiredEnemyUnit().GetRotationState()));
            //    possibleFieldsToMoveEnemy.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(chosenField.GetAcquiredEnemyUnit().GetRotationState(), -1)));
            //    possibleFieldsToMoveEnemy.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(chosenField.GetAcquiredEnemyUnit().GetRotationState(), 1)));
            //    TacticalFightMapField fieldToMoveEnemy = map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3))[0];

            //    if (possibleFieldsToMoveEnemy.Contains(fieldToMoveEnemy))
            //    {
            //        fieldsMarkToAttack.Add(fieldToMoveEnemy);
            //        fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToMoveEnemyByPlayer(true));
            //    }

            //    fieldsMarkToAttack.Add(chosenField);
            //    fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToAttackByPlayer(true));
            //    fieldsMarkToAttack.ForEach(x => x.SetFieldVisualization());
            //    break;

            case (ETacticalFightPlayerAbillityEffectType.Distraction):
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsMarkToAttack.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, rotationState));
                }
                fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToAttackByPlayer(true));
                fieldsMarkToAttack.ForEach(x => x.SetFieldVisualization());
                break;

            case (ETacticalFightPlayerAbillityEffectType.TacticalPositioning):
                foreach(ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsMarkToAttack.AddRange(map.GetPossibleFieldsToMark(chosenField, 2, rotationState));
                }
                fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToAttackByPlayer(true));
                fieldsMarkToAttack.ForEach(x => x.SetFieldVisualization());
                break;

            case (ETacticalFightPlayerAbillityEffectType.Escort):
                fieldsMarkToAttack.Add(chosenField);
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsMarkToAttack.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, rotationState));
                }
                fieldsMarkToAttack.ForEach(x => x.SetIsPossibleToAttackByPlayer(true));
                fieldsMarkToAttack.ForEach(x => x.SetFieldVisualization());
                break;
        }

        return fieldsMarkToAttack;

    }

    public void ActivatePlayerAbility(TacticalFightPlayerUnit playerUnit, TacticalFightMapField chosenField, ETacticalFightUnitRotationState rotationForAbility, int chancesToActivateAbillity)
    {
        bool isHitted = chancesToActivateAbillity > UnityEngine.Random.Range(0, 100);

        PlayableDirector directorToPlay = null;
        TacticalFightEnemyUnit foundedUnit = null;
        List<TacticalFightMapField> fieldsToActivatEffectOn = new List<TacticalFightMapField>();

        switch (playerUnit.GetChosenPlayerAbillity().EffectType)
        {
            case (ETacticalFightPlayerAbillityEffectType.Bombing):
                if (isHitted)
                {
                    directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(chosenField.GetAcquiredEnemyShipUnit()).GetCurrentlySelectedVisualization().BomberAbillityHit;

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndActivateAbillity(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                    {
                        ActivatePlayerAbilityEffect(playerUnit, chosenField, rotationForAbility);
                    }

                }
                else
                {
                    directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(chosenField.GetAcquiredEnemyShipUnit()).GetCurrentlySelectedVisualization().BomberAbillityNotHit;

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndGoToNextRound(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                        OnNotHittedAbillityAction(playerUnit);
                }
                break;

            case (ETacticalFightPlayerAbillityEffectType.FlyBy):
                if (isHitted)
                {
                    if (playerUnit.GetChosenPlayerAbillity().OnUnitCanStartAbillity.HasFlag(ETacticalFightUnitType.Ship))
                    {
                        directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(chosenField.GetAcquiredEnemyShipUnit()).GetCurrentlySelectedVisualization().TorpedoAbillityHit;
                    }
                    //else
                    //{
                    //    directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(chosenField.GetAcquiredEnemyPlaneUnit()).GetCurrentlySelectedVisualization().FighterAbillityHit;
                    //}

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndActivateAbillity(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                    {
                        ActivatePlayerAbilityEffect(playerUnit, chosenField, rotationForAbility);
                    }

                }
                else
                {
                    if (playerUnit.GetChosenPlayerAbillity().OnUnitCanStartAbillity.HasFlag(ETacticalFightUnitType.Ship))
                    {
                        directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(chosenField.GetAcquiredEnemyShipUnit()).GetCurrentlySelectedVisualization().TorpedoAbillityNotHit;
                    }
                    //else
                    //{
                    //    directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(chosenField.GetAcquiredEnemyPlaneUnit()).GetCurrentlySelectedVisualization().FighterAbillityNotHit;
                    //}

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndGoToNextRound(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                        OnNotHittedAbillityAction(playerUnit);
                }
                break;

            case (ETacticalFightPlayerAbillityEffectType.TorpedoStrike):
                fieldsToActivatEffectOn.Add(chosenField);
                fieldsToActivatEffectOn.AddRange(map.GetPossibleFieldsToMark(chosenField, 2, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3)));

                foreach (TacticalFightMapField mapFieldForEffect in fieldsToActivatEffectOn)
                {
                    if (mapFieldForEffect.IsAcquiredEnemyShipUnitToField() && foundedUnit == null)
                    {
                        if(!mapFieldForEffect.GetAcquiredEnemyShipUnit().GetIsNeutral())
                            foundedUnit = mapFieldForEffect.GetAcquiredEnemyShipUnit();
                    }
                }

                if (foundedUnit != null)
                    TacticalFightManager.Instance.OnEnemyChosenChange(foundedUnit);

                if (isHitted)
                {
                    if(foundedUnit != null)
                        directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(foundedUnit).GetCurrentlySelectedVisualization().TorpedoSecondAbillityHit;

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndActivateAbillity(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                    {
                        ActivatePlayerAbilityEffect(playerUnit, chosenField, rotationForAbility);
                    }

                }
                else
                {
                    if (foundedUnit != null)
                        directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(foundedUnit).GetCurrentlySelectedVisualization().TorpedoSecondAbillityNotHit;

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndGoToNextRound(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                        OnNotHittedAbillityAction(playerUnit);
                }
                break;

            case (ETacticalFightPlayerAbillityEffectType.Escort):
                TacticalFightPlayerUnit foundedPlayerUnit = null;

                //if (chosenField.IsAcquiredPlayerUnitToField())
                //{
                //    foundedPlayerUnit = chosenField.GetAcquiredPlayerPlaneUnit();
                //    TacticalFightManager.Instance.OnPlayerChosenChange(foundedPlayerUnit);
                //}

                if (isHitted)
                {
                    if(foundedPlayerUnit != null)
                        directorToPlay = TacticalFightCameraSwitcher.Instance.GetPlayerVisualizationSlot(foundedPlayerUnit).GetCurrentlySelectedVisualization().FighterSecondAbillityHit;

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndActivateAbillity(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                    {
                        ActivatePlayerAbilityEffect(playerUnit, chosenField, rotationForAbility);
                    }

                }
                else
                {
                    if (foundedPlayerUnit != null)
                        directorToPlay = TacticalFightCameraSwitcher.Instance.GetPlayerVisualizationSlot(foundedPlayerUnit).GetCurrentlySelectedVisualization().FighterSecondAbillityNotHit;

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndGoToNextRound(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                        OnNotHittedAbillityAction(playerUnit);
                }
                break;

            case (ETacticalFightPlayerAbillityEffectType.Distraction):
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsToActivatEffectOn.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, rotationState));
                }

                foreach (TacticalFightMapField mapFieldForEffect in fieldsToActivatEffectOn)
                {
                    if (mapFieldForEffect.IsAcquiredEnemyShipUnitToField() && foundedUnit == null)
                    {
                        if (!mapFieldForEffect.GetAcquiredEnemyShipUnit().GetIsNeutral())
                            foundedUnit = mapFieldForEffect.GetAcquiredEnemyShipUnit();
                    }

                    //if (mapFieldForEffect.IsAcquiredEnemyPlaneUnitToField() && foundedUnit == null)
                    //{
                    //    foundedUnit = mapFieldForEffect.GetAcquiredEnemyPlaneUnit();
                    //}
                }

                if (foundedUnit != null)
                    TacticalFightManager.Instance.OnEnemyChosenChange(foundedUnit);

                if (isHitted)
                {
                    if (foundedUnit != null)
                        directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(foundedUnit).GetCurrentlySelectedVisualization().BomberSecondAbillityHit;

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndActivateAbillity(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                    {
                        ActivatePlayerAbilityEffect(playerUnit, chosenField, rotationForAbility);
                    }

                }
                else
                {
                    if (foundedUnit != null)
                        directorToPlay = TacticalFightCameraSwitcher.Instance.GetEnemyVisualizationSlot(foundedUnit).GetCurrentlySelectedVisualization().BomberSecondAbillityNotHit;

                    if (directorToPlay != null)
                        StartCoroutine(PlayAnimationAndGoToNextRound(playerUnit, chosenField, rotationForAbility, directorToPlay));
                    else
                        OnNotHittedAbillityAction(playerUnit);
                }
                break;

        }

    }

    public void OnNotHittedAbillityAction(TacticalFightPlayerUnit playerUnit)
    {
        TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.EnemyRound);
        TacticalFightHudManager.Instance.DeselectAllPlaneButtons();
    }

    public void ActivatePlayerAbilityEffect(TacticalFightPlayerUnit playerUnit, TacticalFightMapField chosenField,ETacticalFightUnitRotationState rotationForAbility)
    {
        List<TacticalFightMapField> fieldsToActivatEffectOn = new List<TacticalFightMapField>();

        switch (playerUnit.GetChosenPlayerAbillity().EffectType)
        {
            case (ETacticalFightPlayerAbillityEffectType.DirectAttack):
                if(playerUnit.GetChosenPlayerAbillity().OnUnitCanStartAbillity.HasFlag(ETacticalFightUnitType.Ship))
                    chosenField.GetAcquiredEnemyShipUnit().TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);
                else
                    chosenField.GetAcquiredEnemyPlaneUnit().TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);

                //TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightBlockUnitEvent(1, playerUnit));                
                //playerUnit.TakeFromMap();
                TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
                break;

            case (ETacticalFightPlayerAbillityEffectType.TorpedoStrike):
                fieldsToActivatEffectOn.Add(chosenField);
                fieldsToActivatEffectOn.AddRange(map.GetPossibleFieldsToMark(chosenField, 2, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3)));

                foreach (TacticalFightMapField mapFieldForEffect in fieldsToActivatEffectOn)
                {
                    if (mapFieldForEffect.IsAcquiredEnemyShipUnitToField())
                    {
                        mapFieldForEffect.GetAcquiredEnemyShipUnit().TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);
                    }

                    //if (mapFieldForEffect.IsAcquiredEnemyPlaneUnitToField())
                    //{
                    //    mapFieldForEffect.GetAcquiredEnemyPlaneUnit().TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);
                    //}
                }

                TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
                break;

            case (ETacticalFightPlayerAbillityEffectType.Bombing):
                fieldsToActivatEffectOn.Add(chosenField);
                fieldsToActivatEffectOn.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 1)));
                fieldsToActivatEffectOn.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, -1)));

                foreach (TacticalFightMapField mapFieldForEffect in fieldsToActivatEffectOn)
                {
                    //if (playerUnit.GetChosenPlayerAbillity().OnUnitCanStartAbillity.HasFlag(ETacticalFightUnitType.Ship))
                    //{
                        if (mapFieldForEffect.IsAcquiredEnemyShipUnitToField())
                        {
                            mapFieldForEffect.GetAcquiredEnemyShipUnit().TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);
                        }
                    //}
                    //else 
                    //{
                        //if (mapFieldForEffect.IsAcquiredEnemyPlaneUnitToField())
                        //{
                        //    mapFieldForEffect.GetAcquiredEnemyPlaneUnit().TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);
                        //}
                    //}
                }

                TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
                break;


            case (ETacticalFightPlayerAbillityEffectType.FlyBy):
                TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
                if (playerUnit.GetChosenPlayerAbillity().OnUnitCanStartAbillity.HasFlag(ETacticalFightUnitType.Ship))
                {
                    TacticalFightEnemyUnit enemyunit = chosenField.GetAcquiredEnemyShipUnit();
                    if (TacticalFightManager.Instance.GetPossibleToMoveForEnemyFields(chosenField.GetAcquiredEnemyShipUnit(), chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3)).Count > 0)
                    {
                        enemyunit.Move(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3))[0], false);
                    }
                    else
                    {
                        enemyunit.TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);
                        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.EnemyRound);
                    }
                }
                else
                {
                    TacticalFightEnemyUnit enemyunit = chosenField.GetAcquiredEnemyPlaneUnit();
                    if (TacticalFightManager.Instance.GetPossibleToMoveForEnemyFields(enemyunit, chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3)).Count > 0)
                    {
                        enemyunit.Move(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3))[0], false);
                    }
                    else
                    {
                        enemyunit.TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);
                        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.EnemyRound);
                    }
                }
                break;

            //case (ETacticalFightPlayerAbillityEffectType.DiveBombing):
            //    chosenField.GetAcquiredEnemyUnit().TakeDamage(playerUnit.GetChosenPlayerAbillity().AmountOfDamage);
            //    if(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3)).Count > 0)
            //        chosenField.GetAcquiredEnemyUnit().Move(map.GetPossibleFieldsToMark(chosenField, 1, TacticalFightUtils.GetRotationStateWithOffset(rotationForAbility, 3))[0],false);
            //    else
            //        TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.EnemyRound);
            //    TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
            //    break;

            case (ETacticalFightPlayerAbillityEffectType.Distraction):
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsToActivatEffectOn.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, rotationState));
                }

                foreach (TacticalFightMapField mapFieldToActiveEffectOn in fieldsToActivatEffectOn)
                {
                    if (mapFieldToActiveEffectOn.IsAcquiredEnemyPlaneUnitToField())
                    {
                        TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightDisctractionEffectEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, mapFieldToActiveEffectOn.GetAcquiredEnemyPlaneUnit()));
                    }

                    if (mapFieldToActiveEffectOn.IsAcquiredEnemyShipUnitToField())
                    {
                        TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightDisctractionEffectEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, mapFieldToActiveEffectOn.GetAcquiredEnemyShipUnit()));
                    }
                }

                TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
                break;

            case (ETacticalFightPlayerAbillityEffectType.TacticalPositioning):
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsToActivatEffectOn.AddRange(map.GetPossibleFieldsToMark(chosenField, 2, rotationState));
                }

                foreach(TacticalFightMapField mapFieldToActiveEffectOn in fieldsToActivatEffectOn)
                {
                    if (mapFieldToActiveEffectOn.IsAcquiredEnemyPlaneUnitToField())
                    {
                        TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightBlockUnitEvent(1, mapFieldToActiveEffectOn.GetAcquiredEnemyPlaneUnit()));
                    }

                    if (mapFieldToActiveEffectOn.IsAcquiredEnemyShipUnitToField())
                    {
                        TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightBlockUnitEvent(1, mapFieldToActiveEffectOn.GetAcquiredEnemyShipUnit()));
                    }
                }

                TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
                playerUnit.TakeDamage(1);
                break;

            case (ETacticalFightPlayerAbillityEffectType.Escort):
                fieldsToActivatEffectOn.Add(chosenField);
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsToActivatEffectOn.AddRange(map.GetPossibleFieldsToMark(chosenField, 1, rotationState));
                }

                foreach (TacticalFightMapField mapFieldToActiveEffectOn in fieldsToActivatEffectOn)
                {
                    if (mapFieldToActiveEffectOn.IsAcquiredPlayerUnitToField())
                    {
                        TacticalFightEventManager.Instance.EndEventOnPlayerUnit(mapFieldToActiveEffectOn.GetAcquiredPlayerPlaneUnit());
                    }
                }
                TacticalFightEventManager.Instance.AddEventToQueue(new TacticalFightPlayerUnitStayOnMapEvent(playerUnit.GetChosenPlayerAbillity().AmountOfRoundsPlaneWillStayOnMap, playerUnit));
                break;
        }

        if (playerUnit.GetChosenPlayerAbillity().EffectType != ETacticalFightPlayerAbillityEffectType.FlyBy && playerUnit.GetChosenPlayerAbillity().EffectType != ETacticalFightPlayerAbillityEffectType.DiveBombing)
            TacticalFightManager.Instance.ChangeTacticalFightGameState(ETacticalFightGameState.EnemyRound);
    }

    public bool ActivateEnemyAbilityEffect(TacticalFightEnemyUnit enemyUnitThatActivateEffect, TacticalFightEnemyAbility abillityToActivate)
    {
        List<TacticalFightMapField> fieldsToActivateEffectOn = new List<TacticalFightMapField>();
        bool hasAbillityBeenActivated = false;

        switch (abillityToActivate.EffectType)
        {
            case (ETacticalFightEnemyAbillityEffectType.ZeroEffect):
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsToActivateEffectOn.AddRange(map.GetPossibleFieldsToMark(enemyUnitThatActivateEffect.GetAcquiredField(), 1, rotationState));
                }

                foreach (TacticalFightMapField fieldToActiveEffectOn in fieldsToActivateEffectOn)
                {
                    if (fieldToActiveEffectOn.IsAcquiredPlayerUnitToField())
                    {
                        fieldToActiveEffectOn.GetAcquiredPlayerPlaneUnit().TakeDamage(abillityToActivate.AmountOfDamage);
                    }
                }
                hasAbillityBeenActivated = true;

                break;

            case (ETacticalFightEnemyAbillityEffectType.YamatoEffect):
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsToActivateEffectOn.AddRange(map.GetPossibleFieldsToMark(enemyUnitThatActivateEffect.GetAcquiredField(), 2, rotationState));
                }

                foreach (TacticalFightMapField fieldToActiveEffectOn in fieldsToActivateEffectOn)
                {
                    if (fieldToActiveEffectOn.IsAcquiredPlayerUnitToField())
                    {
                        fieldToActiveEffectOn.GetAcquiredPlayerPlaneUnit().TakeDamage(abillityToActivate.AmountOfDamage);
                        hasAbillityBeenActivated = true;
                    }
                }
                break;

            case (ETacticalFightEnemyAbillityEffectType.KagaEffect):
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    if (map.GetPossibleFieldsToMark(enemyUnitThatActivateEffect.GetAcquiredField(), 1, rotationState).Count > 0 && !hasAbillityBeenActivated)
                    {
                        TacticalFightMapField fieldToActiveEffectOn = map.GetPossibleFieldsToMark(enemyUnitThatActivateEffect.GetAcquiredField(), 1, rotationState)[0];
                        if (!fieldToActiveEffectOn.IsAcquiredUnitToField() && !hasAbillityBeenActivated)
                        {
                            TacticalFightEnemyUnit createdEnemy = TacticalFightManager.Instance.InstiateEnemyUnit(TacticalFightEnemyManager.Instance.GetEnemyUnit(ETacticalFightEnemyType.ZeroPlane), fieldToActiveEffectOn);
                            createdEnemy.ShowNextAttack();
                            hasAbillityBeenActivated = true;
                        }
                    }
                }

                if (!hasAbillityBeenActivated)
                {
                    foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                    {
                        if (map.GetPossibleFieldsToMark(enemyUnitThatActivateEffect.GetAcquiredField(), 2, rotationState).Count > 1 && !hasAbillityBeenActivated)
                        {
                            TacticalFightMapField fieldToActiveEffectOn = map.GetPossibleFieldsToMark(enemyUnitThatActivateEffect.GetAcquiredField(), 2, rotationState)[1];
                            if (!fieldToActiveEffectOn.IsAcquiredUnitToField() && !hasAbillityBeenActivated)
                            {
                                TacticalFightEnemyUnit createdEnemy = TacticalFightManager.Instance.InstiateEnemyUnit(TacticalFightEnemyManager.Instance.GetEnemyUnit(ETacticalFightEnemyType.ZeroPlane), fieldToActiveEffectOn);
                                createdEnemy.ShowNextAttack();
                                hasAbillityBeenActivated = true;
                            }
                        }
                    }

                }
                break;
            case (ETacticalFightEnemyAbillityEffectType.TankerEffect):
                foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
                {
                    fieldsToActivateEffectOn.AddRange(map.GetPossibleFieldsToMark(enemyUnitThatActivateEffect.GetAcquiredField(), 2, rotationState));
                }

                foreach (TacticalFightMapField fieldToActiveEffectOn in fieldsToActivateEffectOn)
                {
                    if (fieldToActiveEffectOn.IsAcquiredAnyEnemyUnitToField() && !hasAbillityBeenActivated)
                    {
                        if (enemyUnitThatActivateEffect.GetIsNeutral())
                        {
                            enemyUnitThatActivateEffect.SetCurrentHealth(enemyUnitThatActivateEffect.GetCurrentHealth()-abillityToActivate.AmountOfDamage);
                            if (enemyUnitThatActivateEffect.GetCurrentHealth() < enemyUnitThatActivateEffect.GetHealthOnStart() * 0.5f)
                                enemyUnitThatActivateEffect.SetIsHeavyDamaged(true);
                            hasAbillityBeenActivated = true;
                        }
                    }
                }
                break;
        }

        return hasAbillityBeenActivated;
    }

    IEnumerator PlayAnimationAndActivateAbillity(TacticalFightPlayerUnit playerUnit, TacticalFightMapField chosenField, ETacticalFightUnitRotationState rotationForAbility, PlayableDirector directorToPlay)
    {
        directorToPlay.Play();

        while(directorToPlay.state == PlayState.Playing)
        {
            yield return null;
        }

        ActivatePlayerAbilityEffect(playerUnit, chosenField, rotationForAbility);
        yield return null;
    }

    IEnumerator PlayAnimationAndGoToNextRound(TacticalFightPlayerUnit playerUnit, TacticalFightMapField chosenField, ETacticalFightUnitRotationState rotationForAbility, PlayableDirector directorToPlay)
    {
        directorToPlay.Play();

        while (directorToPlay.state == PlayState.Playing)
        {
            yield return null;
        }

        OnNotHittedAbillityAction(playerUnit);

        yield return null;
    }
}
