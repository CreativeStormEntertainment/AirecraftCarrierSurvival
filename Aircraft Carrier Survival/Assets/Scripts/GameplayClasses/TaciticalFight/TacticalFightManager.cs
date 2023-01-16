using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;

public class TacticalFightManager : MonoBehaviour
{
    public static TacticalFightManager Instance;

    public delegate void OnRoundEnded();
    public static OnRoundEnded OnRoundEnd;

    public delegate void OnBeforeEnemyMovement();
    public static OnBeforeEnemyMovement OnBeforeEnemyMove;

    //[SerializeField]
    //string mapDataFileName = "";
    [SerializeField]
    TacticalFightPilot chosenPilot = null;
    [SerializeField]
    TacticalFightEnemyUnit currentlySelectedEnemyUnit = null;
    TacticalFightPlayerUnit currentlySelectedUnit;
    TacticalFightPlayerUnit currentlySelectedUnitFlyVisualization;

    ETacticalFightUnitRotationState lastSelectedRotation;
    TacticalFightMapField lastFoundedField;
    int currentlySelectedPoolIndex;

    TacticalFightHudManager hudManager;
    TacticalFightEffectManager effectManager;
    TacticalFightMap map;
    GraphicRaycaster graphicRaycaster;
    Canvas canvas;
    PointerEventData pointerEventData;
    ETacticalFightGameState currentTacticalFightGameState;

    List<TacticalFightPlayerUnit> playerUnits;
    List<TacticalFightEnemyUnit> enemyUnits;
    List<TacticalFightMapField> markedFieldsToAttack;
    List<TacticalFightMapField> markedFieldsToMove;
    bool isFlightBlocked;
    int damageOnFlight;
    int chancesToGetDoneTheAbillity;
    bool canBeVisualized;
    bool isPlayerUnitSelectedByButton;

    void Awake()
    {
        Instance = this;
        hudManager = GetComponent<TacticalFightHudManager>();
        map = GetComponentInChildren<TacticalFightMap>();
        effectManager = GetComponent<TacticalFightEffectManager>();
        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        canvas = GetComponentInParent<Canvas>();
        pointerEventData = new PointerEventData(EventSystem.current);
        playerUnits = new List<TacticalFightPlayerUnit>();
        enemyUnits = new List<TacticalFightEnemyUnit>();
        markedFieldsToAttack = new List<TacticalFightMapField>();
        markedFieldsToMove = new List<TacticalFightMapField>();
    }

    public void InitializeFight(TacticalFightPilot chosenPilot, string mapDataFilename)
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        playerUnits = new List<TacticalFightPlayerUnit>();
        enemyUnits = new List<TacticalFightEnemyUnit>();
        markedFieldsToAttack = new List<TacticalFightMapField>();
        markedFieldsToMove = new List<TacticalFightMapField>();

        TacticalFightEventManager.Instance.InitializeEventManager();

        map.InitializeMap(mapDataFilename);
        chosenPilot.InitializePilot();

        foreach (TacticalFightMapField mapField in map.GetMapFields())
        {
            mapField.InitializeField();
            mapField.CreateEnemyOnField();
        }

        hudManager.InitializeHud(chosenPilot, enemyUnits);

        if (enemyUnits.Count > 0)
        {
            currentlySelectedEnemyUnit = enemyUnits[0];
            OnEnemyChosenChange(currentlySelectedEnemyUnit);
        }
    }

    public void ResetFight()
    {
        if (enemyUnits != null)
        {
            foreach(TacticalFightEnemyUnit createdEnemy in enemyUnits)
            {
                createdEnemy.UnSetVisualizationForAttack();
                Destroy(createdEnemy.gameObject);
            }
        }

        if(playerUnits != null)
        {
            foreach (TacticalFightPlayerUnit createdPlayerUnit in playerUnits)
            {
                Destroy(createdPlayerUnit.gameObject);
            }
        }

        UnSetVisualizationForAttackForSelectedUnit();
        UnSetVisualizationForMovementForSelectedUnit();
        UnSelectUnit();

        TacticalFightCameraSwitcher.Instance.UnSetAllEnemyVisualizations();

        foreach (TacticalFightMapField mapField in map.GetMapFields())
        {
            mapField.ResetField();
        }

        currentTacticalFightGameState = ETacticalFightGameState.GameStarted;
    }

    void Update()
    {
        switch (currentTacticalFightGameState)
        {
            case (ETacticalFightGameState.PlayerRound):

                TacticalFightMapField chosenField = GetChosenField();
                canBeVisualized = false;

                if (chosenField != null)
                {
                    if (lastFoundedField != chosenField)
                    {
                        if (lastFoundedField != null)
                        {
                            lastFoundedField.SetIsHighLighted(false);
                            lastFoundedField.SetFieldVisualization();
                        }
                        lastFoundedField = chosenField;
                        lastFoundedField.SetIsHighLighted(true);
                        lastFoundedField.SetFieldVisualization();
                    }

                    if (chosenField.IsAcquiredEnemyPlaneUnitToField())
                    {
                        TacticalFightHudManager.Instance.ShowEnemyInfoPanel(chosenField.GetAcquiredEnemyPlaneUnit());

                        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        {
                            if (chosenField.IsAcquiredEnemyShipUnitToField())
                            {
                                TacticalFightHudManager.Instance.ShowEnemyInfoPanel(chosenField.GetAcquiredEnemyShipUnit());
                            }
                        }
                        else
                            TacticalFightHudManager.Instance.ShowEnemyInfoPanel(chosenField.GetAcquiredEnemyPlaneUnit());

                    }
                    else if (chosenField.IsAcquiredPlayerUnitToField())
                    {
                        TacticalFightHudManager.Instance.ShowPlayerInfoPanel(chosenField.GetAcquiredPlayerPlaneUnit());

                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        {
                            if (chosenField.IsAcquiredEnemyShipUnitToField())
                            {
                                TacticalFightHudManager.Instance.ShowEnemyInfoPanel(chosenField.GetAcquiredEnemyShipUnit());
                            }
                        }
                        else
                            TacticalFightHudManager.Instance.ShowPlayerInfoPanel(chosenField.GetAcquiredPlayerPlaneUnit());
                    }
                    else if (chosenField.IsAcquiredEnemyShipUnitToField())
                    {
                        TacticalFightHudManager.Instance.ShowEnemyInfoPanel(chosenField.GetAcquiredEnemyShipUnit());
                    }
                    else
                        TacticalFightHudManager.Instance.HideUnitInfoPanel();

                    if (IsCurrentlySelectedUnitToSpawn())
                    {
                        foreach(TacticalFightAbility playerPlaneAbility in currentlySelectedUnit.GetPlayerAbillities())
                        {
                            if (chosenField.IsAcquiredEnemyPlaneUnitToField())
                            {
                                if (playerPlaneAbility.OnUnitCanStartAbillity.HasFlag(chosenField.GetAcquiredEnemyPlaneUnit().GetUnitType()))
                                {
                                    if (chosenField.GetAcquiredEnemyPlaneUnit().GetCanBeAttackedByPlayer()) 
                                    {
                                        currentlySelectedUnit.SetCurrentChosenAbbillity(playerPlaneAbility);
                                        canBeVisualized = true;
                                    }
                                }
                            }

                            if (chosenField.IsAcquiredEnemyShipUnitToField())
                            {
                                if (playerPlaneAbility.OnUnitCanStartAbillity.HasFlag(chosenField.GetAcquiredEnemyShipUnit().GetUnitType()))
                                {
                                    if (chosenField.GetAcquiredEnemyShipUnit().GetCanBeAttackedByPlayer())
                                    {
                                        currentlySelectedUnit.SetCurrentChosenAbbillity(playerPlaneAbility);
                                        canBeVisualized = true;
                                    }
                                }
                            }

                            if (chosenField.IsAcquiredPlayerUnitToField())
                            {
                                if (playerPlaneAbility.OnUnitCanStartAbillity.HasFlag(chosenField.GetAcquiredPlayerPlaneUnit().GetUnitType()))
                                {
                                    currentlySelectedUnit.SetCurrentChosenAbbillity(playerPlaneAbility);
                                    canBeVisualized = true;
                                }
                            }

                            if (playerPlaneAbility.OnUnitCanStartAbillity.HasFlag(chosenField.GetUnitType()))
                            {
                                if (!chosenField.IsAcquiredUnitToField())
                                {
                                    currentlySelectedUnit.SetCurrentChosenAbbillity(playerPlaneAbility);
                                    canBeVisualized = true;
                                }
                            }

                        }

                        if (canBeVisualized)
                        {
                            ETacticalFightUnitRotationState chosenRotation = GetDirectionFromMousePositionOnField(chosenField);
                            if (lastSelectedRotation != chosenRotation)
                            {
                                hudManager.ResetInfoText();
                                lastSelectedRotation = chosenRotation;
                                UnSetVisualizationForAttackForSelectedUnit();
                                UnSetVisualizationForMovementForSelectedUnit();
                                SetVisualizationForAttackForSelectedPlayerUnit(chosenField, chosenRotation);

                                damageOnFlight = 0;
                                chancesToGetDoneTheAbillity = 100;

                                TacticalFightHudManager.Instance.PlayOnMapAbillityHoverClip();

                                if (currentlySelectedUnit.GetChosenPlayerAbillity().AbilityType == ETacticalFightAbilityType.DirectionalAttack)
                                    SetVisualizationForMovementInDirectionForSelectedUnit(chosenRotation, chosenField);
                                else if (currentlySelectedUnit.GetChosenPlayerAbillity().AbilityType == ETacticalFightAbilityType.OnFieldAttack)
                                    SetVisualizationForMovementForField(chosenField, chosenRotation);
                            }

                        }
                        else
                        {
                            hudManager.ResetInfoText();
                            UnSetVisualizationForAttackForSelectedUnit();
                            UnSetVisualizationForMovementForSelectedUnit();
                        }
                    }
                    

                    if (Input.GetMouseButtonDown(0))
                    {
                        //if (chosenField.IsAcquiredPlayerUnitToField())
                        //{
                        //    OnPlayerChosenChange(chosenField.GetAcquiredPlayerPlaneUnit());
                        //}

                        if (chosenField.IsAcquiredEnemyShipUnitToField())
                        {
                            currentlySelectedEnemyUnit = chosenField.GetAcquiredEnemyShipUnit();
                            OnEnemyChosenChange(currentlySelectedEnemyUnit);
                        }

                        //if (chosenField.IsAcquiredEnemyPlaneUnitToField())
                        //{
                        //    currentlySelectedEnemyUnit = chosenField.GetAcquiredEnemyPlaneUnit();
                        //    OnEnemyChosenChange(currentlySelectedEnemyUnit);
                        //}

                        if (!isFlightBlocked && IsCurrentlySelectedUnitToSpawn() && canBeVisualized)
                        {
                            TacticalFightPlayerUnit createdPlayerUnit = InstiatePlayerUnit(currentlySelectedUnit, markedFieldsToMove[markedFieldsToMove.Count - 1]);
                            createdPlayerUnit.SetCurrentChosenAbbillity(currentlySelectedUnit.GetChosenPlayerAbillity());
                            createdPlayerUnit.SetPilotPoolIndex(currentlySelectedPoolIndex);
                            createdPlayerUnit.TakeDamage(damageOnFlight);
                            createdPlayerUnit.ActivateAbillity(markedFieldsToMove, chosenField, lastSelectedRotation, chancesToGetDoneTheAbillity);
                            currentTacticalFightGameState = ETacticalFightGameState.PlayerInAction;
                            UnSelectUnit();
                        }
                    }
                }
                else
                {
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    TacticalFightHudManager.Instance.ChangeIsShowingAttackFieldsAllState();
                }

                break;
        }
    }

    public void OnEnemyChosenChange(TacticalFightEnemyUnit enemyUnit)
    {
        currentlySelectedEnemyUnit = enemyUnit;
        TacticalFightCameraSwitcher.Instance.SetCameraEnemyUnitActive(currentlySelectedEnemyUnit);
    }

    public void ChoseRandomEnemy()
    {
        if (enemyUnits.Count > 0)
        {
            currentlySelectedEnemyUnit = enemyUnits[UnityEngine.Random.Range(0, enemyUnits.Count - 1)];
            OnEnemyChosenChange(currentlySelectedEnemyUnit);
        }
    }

    public void OnPlayerChosenChange(TacticalFightPlayerUnit playerUnit)
    {
        TacticalFightCameraSwitcher.Instance.SetCameraPlayerUnitActive(playerUnit);
    }

    public void ChangeTacticalFightGameState(ETacticalFightGameState tacticalFightStateTochange)
    {
        switch (tacticalFightStateTochange)
        {
            case (ETacticalFightGameState.EnemyRound):
                List<TacticalFightEnemyUnit> unitsToMoveThisRound = new List<TacticalFightEnemyUnit>();
                unitsToMoveThisRound.AddRange(enemyUnits);

                foreach (TacticalFightEnemyUnit enemyUnit in unitsToMoveThisRound)
                {
                    enemyUnit.Attack();
                }
                OnBeforeEnemyMove?.Invoke();

                foreach (TacticalFightEnemyUnit enemyUnit in unitsToMoveThisRound)
                {
                    enemyUnit.MoveToNextRound();
                }
                GetChosenPilot().DecreaseMoralePoints(1);
                OnRoundEnd?.Invoke();
                currentTacticalFightGameState = tacticalFightStateTochange;
                tacticalFightStateTochange = ETacticalFightGameState.PlayerRound;
                break;

            case (ETacticalFightGameState.PlayerRetreat):
                TacticalFightHudManager.Instance.ShowEndGamePanel(false, "Player retreated his units.");
                hudManager.SetInfoTextMessage("Player retreated his units. Game is Lost !!!");
                currentTacticalFightGameState = tacticalFightStateTochange;
                break;

            case (ETacticalFightGameState.Victory):
                hudManager.SetInfoTextMessage("Game Won !!!");
                currentTacticalFightGameState = tacticalFightStateTochange;
                break;

            case (ETacticalFightGameState.Lose):
                hudManager.SetInfoTextMessage("Game Lost !!!");
                currentTacticalFightGameState = tacticalFightStateTochange;
                break;
        }

        if (currentTacticalFightGameState == ETacticalFightGameState.EnemyRound) 
            currentTacticalFightGameState = tacticalFightStateTochange;
    }

    public void StartGame()
    {
        currentTacticalFightGameState = ETacticalFightGameState.PlayerRound;
    }

    private bool IsCurrentlySelectedUnitToSpawn()
    {
        return currentlySelectedUnit != null && isPlayerUnitSelectedByButton == true;
    }

    public void UnSelectUnit()
    {
        UnSetVisualizationForAttackForSelectedUnit();
        UnSetVisualizationForMovementForSelectedUnit();
        currentlySelectedUnit = null;
        currentlySelectedPoolIndex = -1;
        isPlayerUnitSelectedByButton = false;
    }

    public void SelectUnit(TacticalFightPlayerUnit unit,int indexOfUnit)
    {
        currentlySelectedUnit = unit;
        currentlySelectedPoolIndex = indexOfUnit;
        currentlySelectedUnit.SetCurrentChosenAbbillity(0);
        isPlayerUnitSelectedByButton = true;
    }

    private TacticalFightMapField GetChosenField()
    {
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        RaycastResult foundedResult = results.FirstOrDefault(x => x.gameObject.layer == 9);

        // SIMPLE RESOLUTION FOR NOW
        //float test=0f;
        if (foundedResult.gameObject != null)
        {
            
            TacticalFightMapField chosenField = foundedResult.gameObject.GetComponent<TacticalFightMapField>();
            
            return chosenField;
        }
        else
        {
            return null;
        }
    }

    public List<TacticalFightMapField> GetPossibleToAttackFieldsForEnemyUnit(TacticalFightEnemyUnit fightUnit, TacticalFightMapField unitField)
    {
        List<TacticalFightMapField> fieldsMarkToAttack = new List<TacticalFightMapField>();
        int rangeOfAttack = fightUnit.GetAttackRange();

        fieldsMarkToAttack.AddRange(map.GetPossibleFieldsToMark(unitField, rangeOfAttack, fightUnit.GetRotationState()));

        return fieldsMarkToAttack;
    }

    public List<TacticalFightMapField> GetPossibleToMoveForEnemyFields(TacticalFightBaseUnit unitWillMove,TacticalFightMapField chosenMapField, int rangeOfMovement, ETacticalFightUnitRotationState rotationState)
    {
        List<TacticalFightMapField> fieldsMarkToMove = new List<TacticalFightMapField>();

        fieldsMarkToMove.AddRange(map.GetPossibleFieldsToMark(chosenMapField, rangeOfMovement, rotationState));

        List<TacticalFightMapField> fieldsToRemove = new List<TacticalFightMapField>();

        foreach (TacticalFightMapField field in fieldsMarkToMove)
        {
            if (unitWillMove.GetUnitType().HasFlag(ETacticalFightUnitType.Ship) && field.IsAcquiredEnemyShipUnitToField())
                fieldsToRemove.Add(field);

            if((unitWillMove.GetUnitType().HasFlag(ETacticalFightUnitType.PlayerPlane) || unitWillMove.GetUnitType().HasFlag(ETacticalFightUnitType.Plane)) && (field.IsAcquiredPlayerUnitToField() || field.IsAcquiredEnemyPlaneUnitToField()))
                fieldsToRemove.Add(field);

            if (unitWillMove.GetUnitMovementType().HasFlag(ETacticalFightUnitMovementType.ByLand) && field.GetIsLand() == false)
                fieldsToRemove.Add(field);

            if (unitWillMove.GetUnitMovementType().HasFlag(ETacticalFightUnitMovementType.BySea) && field.GetIsLand() == true)
                fieldsToRemove.Add(field);
        }

        foreach (TacticalFightMapField field in fieldsToRemove)
            fieldsMarkToMove.Remove(field);

        return fieldsMarkToMove;
    }

    public bool GetIsPossibleToMoveByUnit(TacticalFightBaseUnit unitWillMove)
    {
        if (map.GetPossibleFieldsToMark(unitWillMove.GetAcquiredField(), unitWillMove.GetMovementSpeed(), unitWillMove.GetRotationState()).Count > 0)
            return true;
        else
            return false;
    }


    private void SetVisualizationForMovementInDirectionForSelectedUnit(ETacticalFightUnitRotationState rotationState, TacticalFightMapField chosenField)
    {
        markedFieldsToMove = new List<TacticalFightMapField>();
        markedFieldsToMove.AddRange(map.GetPossibleFieldsToMark(chosenField, int.MaxValue, rotationState));

        HashSet<TacticalFightEnemyUnit> enemyUnitsOnPath = new HashSet<TacticalFightEnemyUnit>();
        List<TacticalFightMapField> fieldsWhereEnemyWillAttack = new List<TacticalFightMapField>();

        int fieldsWithCloudsOnPath = 0;
        // change to one for each in future

        if (markedFieldsToMove.FirstOrDefault(x => x.IsAcquiredPlayerUnitToField() == true) == null && markedFieldsToMove.FirstOrDefault(x => x.IsAcquiredEnemyPlaneUnitToField() == true) == null  && markedFieldsToMove.Count > 0)
        {
            markedFieldsToMove.ForEach(x => x.SetIsPossibleToMove(true));
            markedFieldsToMove[0].SetIsPossibleToSpawnHerePlayerUnit(true);
            fieldsWhereEnemyWillAttack = markedFieldsToMove.Where(x => x.GetIsFieldAttackedByEnemy() == true).ToList();

            fieldsWhereEnemyWillAttack.ForEach(i => i.GetEnemiesThatWillAttackOnField().ForEach(x => enemyUnitsOnPath.Add(x)));
            markedFieldsToMove.Where(x => x.GetIsFieldClouded() == true).ToList().ForEach(i => fieldsWithCloudsOnPath++);

            if (fieldsWhereEnemyWillAttack.Count > 0)
            {
                foreach (TacticalFightEnemyUnit unitWillAttackOnField in enemyUnitsOnPath)
                {
                    if (damageOnFlight < unitWillAttackOnField.GetAttackDamageOnField())
                        damageOnFlight = unitWillAttackOnField.GetAttackDamageOnField();
                }
            }

            if(markedFieldsToMove.FirstOrDefault(x => x.GetIsWakatakeEffectOnField() == true) != null)
                chancesToGetDoneTheAbillity -= 30;

            fieldsWhereEnemyWillAttack.ForEach(i => chancesToGetDoneTheAbillity -= i.GetAmountOfChanceToGetLowerOnField());

            if (fieldsWithCloudsOnPath > 0)
            {
                damageOnFlight = 0;
                chancesToGetDoneTheAbillity -= 30;
            }

            hudManager.SetInfoTextMessage("Your unit will take " + damageOnFlight + " damage. Your chances to get done the operation is " + chancesToGetDoneTheAbillity + "%.");
            isFlightBlocked = false;

            currentlySelectedUnitFlyVisualization = Instantiate(currentlySelectedUnit, map.transform);
            currentlySelectedUnitFlyVisualization.GetComponent<RectTransform>().anchoredPosition = markedFieldsToMove[markedFieldsToMove.Count - 1].GetComponent<RectTransform>().anchoredPosition;
            currentlySelectedUnitFlyVisualization.SetRotationState(TacticalFightUtils.GetRotationStateWithOffset(rotationState, 3));
            StartCoroutine(FlyOnPathVisualization(currentlySelectedUnitFlyVisualization,
                markedFieldsToMove[markedFieldsToMove.Count - 1].GetComponent<RectTransform>().anchoredPosition,
                markedFieldsToMove[0].GetComponent<RectTransform>().anchoredPosition, 
                TacticalFightVisualizationManager.Instance.OnHoverMovementSpeed * markedFieldsToMove.Count()));
        }
        else
        {
            markedFieldsToMove.ForEach(x => x.SetIsPossibleToMove(false));
            isFlightBlocked = true;
        }

        markedFieldsToMove.ForEach(x => x.SetIsFlightVisualizationActive(true));
        markedFieldsToMove.ForEach(x => x.SetFieldVisualization());

        if(markedFieldsToMove.Count > 0)
            TacticalFightVisualizationManager.Instance.SetFlightVisualization(markedFieldsToMove[0], rotationState, isFlightBlocked);
    }

    private void SetVisualizationForMovementForField(TacticalFightMapField chosenField, ETacticalFightUnitRotationState chosenRotation)
    {
        markedFieldsToMove = new List<TacticalFightMapField>();
        if (!chosenField.IsAcquiredUnitToField())
        {
            markedFieldsToMove.Add(chosenField);
            markedFieldsToMove.ForEach(x => x.SetIsPossibleToMove(true));
            markedFieldsToMove.ForEach(x => x.SetIsPossibleToSpawnHerePlayerUnit(true));
            markedFieldsToMove.ForEach(x => x.SetIsFlightVisualizationActive(true));
            markedFieldsToMove.ForEach(x => x.SetFieldVisualization());
            isFlightBlocked = false;
        }
        else if(map.GetPossibleFieldsToMark(chosenField, 1, chosenRotation).Count > 0)
        {
            chosenField.SetIsPossibleToMove(false);
            markedFieldsToMove.Add(chosenField);
            TacticalFightMapField anotherField = map.GetPossibleFieldsToMark(chosenField, 1, chosenRotation)[0];
            if (anotherField.IsAcquiredUnitToField())
            {
                anotherField.SetIsPossibleToMove(false);
                isFlightBlocked = true;
            }
            else
            {
                anotherField.SetIsPossibleToMove(true);
                anotherField.SetIsPossibleToSpawnHerePlayerUnit(true);
                isFlightBlocked = false;
            }
            markedFieldsToMove.Add(anotherField); 
            markedFieldsToMove.ForEach(x => x.SetIsFlightVisualizationActive(true));
            markedFieldsToMove.ForEach(x => x.SetFieldVisualization());
        }
    }

    private void SetVisualizationForAttackForSelectedPlayerUnit(TacticalFightMapField chosenField,ETacticalFightUnitRotationState rotationOfAttack)
    {
        markedFieldsToAttack = effectManager.SetFieldsToVisualizeForPlayerAbilityEffect(currentlySelectedUnit.GetChosenPlayerAbillity(), chosenField, rotationOfAttack);
    }

    private void UnSetVisualizationForMovementForSelectedUnit()
    {
        if(currentlySelectedUnitFlyVisualization != null)
        {
            StopAllCoroutines();
            Destroy(currentlySelectedUnitFlyVisualization.gameObject);
        }

        TacticalFightVisualizationManager.Instance.UnSetFlightVisualization();

        foreach (TacticalFightMapField markedField in markedFieldsToMove)
        {
            markedField.SetIsFlightVisualizationActive(false);
            markedField.SetIsPossibleToMove(false);
            markedField.SetIsPossibleToSpawnHerePlayerUnit(false);
            markedField.SetFieldVisualization();
        }
    }

    private void UnSetVisualizationForAttackForSelectedUnit()
    {
        foreach (TacticalFightMapField markedField in markedFieldsToAttack)
        {
            markedField.SetIsPossibleToAttackByPlayer(false);
            markedField.SetFieldVisualization();
        }
    }
    private TacticalFightBaseUnit InstiateUnitOnField(TacticalFightBaseUnit unitToSpawn, TacticalFightMapField fieldToSpawnOn)
    {
        TacticalFightBaseUnit createdunit = Instantiate(unitToSpawn, map.transform);
        createdunit.GetComponent<RectTransform>().anchoredPosition = fieldToSpawnOn.GetComponent<RectTransform>().anchoredPosition;
        if(createdunit.GetUnitType().HasFlag(ETacticalFightUnitType.Ship))
            fieldToSpawnOn.PlaceShipUnitOnField(createdunit);
        else
            fieldToSpawnOn.PlacePlaneUnitOnField(createdunit);


        return createdunit;
    }

    public TacticalFightEnemyUnit InstiateEnemyUnit(TacticalFightEnemyUnit enemyUnit, TacticalFightMapField fieldToSpawnOn)
    {
        TacticalFightEnemyUnit createdEnemyUnit = InstiateUnitOnField(enemyUnit, fieldToSpawnOn) as TacticalFightEnemyUnit;
        if(!createdEnemyUnit.GetIsNeutral())
            createdEnemyUnit.SetCanBeAttackedByPlayer(true);
        createdEnemyUnit.InitializeEnemy();

        if (TacticalFightCameraSwitcher.Instance.GetFirstFreeEnemyVisualizationSlot() != null)
            TacticalFightCameraSwitcher.Instance.GetFirstFreeEnemyVisualizationSlot().SetEnemyForSlot(createdEnemyUnit);

        enemyUnits.Add(createdEnemyUnit);

        return createdEnemyUnit;
    }

    public void RemoveEnemyFromList(TacticalFightEnemyUnit enemyUnitToRemove)
    {
        enemyUnits.Remove(enemyUnitToRemove);
    }

    private TacticalFightPlayerUnit InstiatePlayerUnit(TacticalFightPlayerUnit playerUnit, TacticalFightMapField fieldToSpawnOn)
    {
        TacticalFightPlayerUnit createdPlayerUnit = InstiateUnitOnField(playerUnit, fieldToSpawnOn) as TacticalFightPlayerUnit;

        playerUnits.Add(createdPlayerUnit);

        return createdPlayerUnit;
    }

    public void RemovePlayerUnitFromList(TacticalFightPlayerUnit playerUnitToRemove) 
    { 
        playerUnits.Remove(playerUnitToRemove);
    }

    public List<TacticalFightEnemyUnit> GetEnemyListOnMap()
    {
        return enemyUnits;
    }

    public TacticalFightPilot GetChosenPilot()
    {
        return chosenPilot;
    }

    public int GetEnemyPlaneOnMapCount()
    {
        int amountOfPlanes = 0;

        amountOfPlanes += enemyUnits.Where(x => x.GetUnitType().HasFlag(ETacticalFightUnitType.Plane)).ToList().Count;

        return amountOfPlanes;
    }

    public List<TacticalFightMapField> GetFieldsAroundField(TacticalFightMapField field,bool withCenter)
    {
        List<TacticalFightMapField> fieldsAroundField = new List<TacticalFightMapField>();

        if (withCenter)
            fieldsAroundField.Add(field);

        foreach (ETacticalFightUnitRotationState rotationState in Enum.GetValues(typeof(ETacticalFightUnitRotationState)))
        {
            fieldsAroundField.AddRange(map.GetPossibleFieldsToMark(field, 1, rotationState));
        }

        return fieldsAroundField;
    }

    private ETacticalFightUnitRotationState GetDirectionFromMousePositionOnField(TacticalFightMapField chosenField)
    {
        Vector2 mousePosOnCanvas;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            map.transform as RectTransform,
            Input.mousePosition, canvas.worldCamera,
            out mousePosOnCanvas);

        float angle = Mathf.Atan2(mousePosOnCanvas.y - chosenField.transform.localPosition.y, mousePosOnCanvas.x - chosenField.transform.localPosition.x) * 180 / Mathf.PI;

        if (angle > 60 && angle < 120)
            return ETacticalFightUnitRotationState.Up;
        else if (angle < 60 && angle > 0)
            return ETacticalFightUnitRotationState.RightUp;
        else if (angle < 180 && angle > 120)
            return ETacticalFightUnitRotationState.LeftUp;
        else if (angle > -180 && angle < -120)
            return ETacticalFightUnitRotationState.LeftDown;
        else if (angle > -120 && angle < -60)
            return ETacticalFightUnitRotationState.Down;
        else 
            return ETacticalFightUnitRotationState.RightDown;
    }

    private IEnumerator FlyOnPathVisualization(TacticalFightPlayerUnit unitToVisualize, Vector2 startPosition, Vector2 endPosition, float time = 1)
    {
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            unitToVisualize.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPosition, endPosition, (elapsedTime / time));
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        unitToVisualize.GetComponent<RectTransform>().anchoredPosition = endPosition;
        StartCoroutine(FlyOnPathVisualization(currentlySelectedUnitFlyVisualization,
                                             markedFieldsToMove[markedFieldsToMove.Count - 1].GetComponent<RectTransform>().anchoredPosition,
                                             markedFieldsToMove[0].GetComponent<RectTransform>().anchoredPosition,
                                             TacticalFightVisualizationManager.Instance.OnHoverMovementSpeed * markedFieldsToMove.Count()));
        yield return null;
    }
}
