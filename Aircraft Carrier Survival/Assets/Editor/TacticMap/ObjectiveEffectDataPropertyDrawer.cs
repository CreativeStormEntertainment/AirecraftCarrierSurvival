using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[CustomPropertyDrawer(typeof(ObjectiveEffectData))]
public class ObjectiveEffectDataPropertyDrawer : PropertyDrawer
{
    private const string Effect = nameof(ObjectiveEffectData.EffectType);
    private const string NotEffect = nameof(ObjectiveEffectData.NotEffect);
    private const string TargetTranses = nameof(ObjectiveEffectData.TargetTranses);
    private const string Targets = nameof(ObjectiveEffectData.Targets);
    private const string Hours = nameof(ObjectiveEffectData.Hours);
    private const string Minutes = nameof(ObjectiveEffectData.Minutes);
    private const string MissionType = nameof(ObjectiveEffectData.MissionType);
    private const string Power = nameof(ObjectiveEffectData.Power);
    private const string AbsoluteForce = nameof(ObjectiveEffectData.AbsoluteForce);
    private const string Range = nameof(ObjectiveEffectData.Range);
    private const string Count = nameof(ObjectiveEffectData.Count);

    private const string ObjectiveTarget = nameof(ObjectiveEffectData.ObjectiveTarget);
    private const string ObjectiveTransform = nameof(ObjectiveEffectData.ObjectiveTransform);

    private const string HoursToRetrieve = nameof(ObjectiveEffectData.HoursToRetrieve);
    private const string RetrievePositionRect = nameof(ObjectiveEffectData.RetrievePositionRect);
    private const string RetrievePosition = nameof(ObjectiveEffectData.RetrievePosition);
    private const string BombersNeeded = nameof(ObjectiveEffectData.BombersNeeded);
    private const string FightersNeeded = nameof(ObjectiveEffectData.FightersNeeded);
    private const string TorpedoesNeeded = nameof(ObjectiveEffectData.TorpedoesNeeded);
    private const string OverrideWinDescriptionID = nameof(ObjectiveEffectData.OverrideWinDescriptionID);
    private const string TimerID = nameof(ObjectiveEffectData.TimerID);
    private const string TimerTooltipID = nameof(ObjectiveEffectData.TimerTooltipTitleID);
    private const string TimerTooltipDescID = nameof(ObjectiveEffectData.TimerTooltipDescID);
    private const string Time = nameof(ObjectiveEffectData.Time);
    private const string OverridePosition = nameof(ObjectiveEffectData.OverridePosition);
    private const string Date = nameof(ObjectiveEffectData.Date);

    private const string UICategories = nameof(ObjectiveEffectData.UICategories);
    private const string CameraInput = nameof(ObjectiveEffectData.CameraInput);
    private const string IslandRooms = nameof(ObjectiveEffectData.IslandRooms);
    private const string Departments = nameof(ObjectiveEffectData.Departments);
    private const string PlaneType = nameof(ObjectiveEffectData.PlaneType);
    private const string Strategy = nameof(ObjectiveEffectData.Strategy);
    private const string UOType = nameof(ObjectiveEffectData.UOType);
    private const string Section = nameof(ObjectiveEffectData.Section);
    private const string FirstSubsection = nameof(ObjectiveEffectData.FirstSubsection);
    private const string SecondSubsection = nameof(ObjectiveEffectData.SecondSubsection);
    private const string Issue = nameof(ObjectiveEffectData.Issue);
    private const string Department = nameof(ObjectiveEffectData.Department);
    private const string AttackStrikeGroup = nameof(ObjectiveEffectData.AttackStrikeGroup);
    private const string Missions = nameof(ObjectiveEffectData.Missions);
    private const string Events = nameof(ObjectiveEffectData.Events);
    private const string DcCategoryFlag = nameof(ObjectiveEffectData.DcCategoryFlag);
    private const string SoundEventReference = nameof(ObjectiveEffectData.SoundEventReference);
    private const string CameraView = nameof(ObjectiveEffectData.CameraView);

    private static List<string[]> Highlights;
    private static List<int[]> HighlightsInts;

    private static List<string[]> SlotMasksTexts;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        bool editor = GetEffect(property, out var map, out var effect);
        if (effect.Expanded)
        {
            float height = EditorGUIUtility.singleLineHeight;

            float lineHeight = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            height += lineHeight;

            bool show = false;
            switch (effect.EffectType)
            {
                case EObjectiveEffect.ShowObjectives:
                case EObjectiveEffect.ActivateObjectives:
                case EObjectiveEffect.Win:
                case EObjectiveEffect.SucceedObjectives:
                case EObjectiveEffect.EnemyCanChase:
                case EObjectiveEffect.ShowTimer:
                case EObjectiveEffect.AddMission:
                case EObjectiveEffect.SearchAndDestroy:
                case EObjectiveEffect.ShowBalancedForcesBar:
                case EObjectiveEffect.MakeDamageRange:
                case EObjectiveEffect.SetEnableDragPlanes:
                case EObjectiveEffect.SetEnableReconUO:
                case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                case EObjectiveEffect.SetEnableRecoveryTimeout:
                case EObjectiveEffect.SetEnableAirstrikeTarget:
                case EObjectiveEffect.SetEnableForcedStrategy:
                case EObjectiveEffect.SetEnableSpreadIssue:
                case EObjectiveEffect.SetEnableDCIssueDestination:
                case EObjectiveEffect.SetEnableDCInMaintenance:
                case EObjectiveEffect.SetEnableDCInPumps:
                case EObjectiveEffect.SetEnableRescueTimer:
                case EObjectiveEffect.SetEnableMoveTime:
                case EObjectiveEffect.SetEnableObsoleteMission:
                case EObjectiveEffect.SetMaxCarrierSpeed:
                case EObjectiveEffect.SetShowNarrator:
                case EObjectiveEffect.SetShowObject:
                case EObjectiveEffect.ShowRescueRange:
                case EObjectiveEffect.ShowCannonRange:
                case EObjectiveEffect.SetShowSpriteStrategyPanel:
                case EObjectiveEffect.ShowAlly:
                case EObjectiveEffect.ShowMissionRange:
                case EObjectiveEffect.ShowMagicSprite:
                    show = true;
                    break;
            }
            if (show)
            {
                height += lineHeight;
                if (effect.EffectType == EObjectiveEffect.SetShowSpriteStrategyPanel || (effect.EffectType == EObjectiveEffect.ShowTimer && !effect.NotEffect))
                {
                    height += lineHeight;
                }
            }

            show = true;
            switch (effect.EffectType)
            {
                case EObjectiveEffect.Win:
                case EObjectiveEffect.ShowTimer:
                case EObjectiveEffect.CustomMissionRetrieval:
                case EObjectiveEffect.AttackCarrier:
                case EObjectiveEffect.SpawnRescueShip:
                case EObjectiveEffect.FinalestDestroySections:
                case EObjectiveEffect.ForceCarrierSpeed:
                case EObjectiveEffect.SpawnTowShip:
                case EObjectiveEffect.SetEnableUI:
                case EObjectiveEffect.SetEnableCameraInput:
                case EObjectiveEffect.SetEnableOfficer:
                case EObjectiveEffect.SetEnableIslandRoomSelection:
                case EObjectiveEffect.SetEnableSwitch:
                case EObjectiveEffect.SetEnableCrew:
                case EObjectiveEffect.SetEnableDepartmentPlacement:
                case EObjectiveEffect.SetEnableSquadronType:
                case EObjectiveEffect.SetEnableDragPlanes:
                case EObjectiveEffect.SetEnableMissions:
                case EObjectiveEffect.SetEnableReconUO:
                case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                case EObjectiveEffect.SetEnableRecoveryTimeout:
                case EObjectiveEffect.SetEnableForcedStrategy:
                case EObjectiveEffect.SetEnableEscort:
                case EObjectiveEffect.SetEnableSpreadIssue:
                case EObjectiveEffect.SetEnableDCIssueDestination:
                case EObjectiveEffect.SetEnableDCInMaintenance:
                case EObjectiveEffect.SetEnableDCInPumps:
                case EObjectiveEffect.SetEnableRescueTimer:
                case EObjectiveEffect.SetEnableMoveTime:
                case EObjectiveEffect.SetEnableObsoleteMission:
                case EObjectiveEffect.SetEnableMaxPlanes:
                case EObjectiveEffect.SetEnableEvents:
                case EObjectiveEffect.SetEnableDCCategory:
                case EObjectiveEffect.ResetEnables:
                case EObjectiveEffect.SetMaxCarrierSpeed:
                case EObjectiveEffect.SetTimeSpeed:
                case EObjectiveEffect.SetSupplies:
                case EObjectiveEffect.DestroyNeutrals:
                case EObjectiveEffect.SetSentMissionDuration:
                case EObjectiveEffect.SpawnAttack:
                case EObjectiveEffect.SpawnIssue:
                case EObjectiveEffect.SpawnInjuredCrew:
                case EObjectiveEffect.SetShowHighlight:
                case EObjectiveEffect.SetShowNarrator:
                case EObjectiveEffect.SetCamera:
                case EObjectiveEffect.FinishCourseSettingMode:
                case EObjectiveEffect.DisableEscortButton:
                case EObjectiveEffect.DisableCancelMission:
                case EObjectiveEffect.DisableEnemyDisappear:
                case EObjectiveEffect.DisableBuffClose:
                case EObjectiveEffect.DisableBuffDeallocation:
                case EObjectiveEffect.DisableOfficerDeallocation:
                case EObjectiveEffect.DetectPlayer:
                case EObjectiveEffect.SetShowObject:
                case EObjectiveEffect.SwitchFriendlyCAPToMidway:
                case EObjectiveEffect.ShowPath:
                case EObjectiveEffect.ShowRescueRange:
                case EObjectiveEffect.TeleportPlayer:
                case EObjectiveEffect.AdvanceTime:
                case EObjectiveEffect.ShowCannonRange:
                case EObjectiveEffect.ShowSurvivors:
                case EObjectiveEffect.KillSurvivor:
                case EObjectiveEffect.ShowCarrier:
                case EObjectiveEffect.LaunchBombers:
                case EObjectiveEffect.DestroyNotSentMissions:
                case EObjectiveEffect.DestroySection:
                case EObjectiveEffect.ShowMagicSprite:
                case EObjectiveEffect.DisableAttacksOnAlly:
                case EObjectiveEffect.RemovePermamentlyMagicIdentify:
                case EObjectiveEffect.SetSuperTimeSpeed:
                case EObjectiveEffect.CancelMissions:
                case EObjectiveEffect.DisableDeath:
                    show = false;
                    break;
                case EObjectiveEffect.ShowBalancedForcesBar:
                case EObjectiveEffect.MakeDamageRange:
                case EObjectiveEffect.ShowMissionRange:
                case EObjectiveEffect.SetShowSpriteStrategyPanel:
                    if (editor && effect.NotEffect)
                    {
                        show = false;
                    }
                    break;
                case EObjectiveEffect.AddMission:
                    if (editor && effect.MissionType != EMissionOrderType.AttackJapan && effect.MissionType != EMissionOrderType.Decoy && effect.MissionType != EMissionOrderType.RescueVIP &&
                        effect.MissionType != EMissionOrderType.MidwayAirstrike)
                    {
                        show = false;
                    }
                    break;
                case EObjectiveEffect.ShowMovie:
                    height += lineHeight;
                    show = false;
                    break;
            }
            if (show)
            {
                var prop = property.FindPropertyRelative(editor ? TargetTranses : Targets);
                height += EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            switch (effect.EffectType)
            {
                case EObjectiveEffect.Win:
                case EObjectiveEffect.ForceCarrierSpeed:
                case EObjectiveEffect.SetMaxCarrierSpeed:
                case EObjectiveEffect.SetTimeSpeed:
                case EObjectiveEffect.SetEnableUI:
                case EObjectiveEffect.SetEnableCameraInput:
                case EObjectiveEffect.SetEnableOfficer:
                case EObjectiveEffect.SetEnableIslandRoomSelection:
                case EObjectiveEffect.SetEnableSwitch:
                case EObjectiveEffect.SetEnableCrew:
                case EObjectiveEffect.SetEnableDepartmentPlacement:
                case EObjectiveEffect.SetEnableSquadronType:
                case EObjectiveEffect.SetEnableMissions:
                case EObjectiveEffect.SetEnableEscort:
                case EObjectiveEffect.SetEnableEvents:
                case EObjectiveEffect.SetEnableDCCategory:
                case EObjectiveEffect.SetSupplies:
                case EObjectiveEffect.SpawnNeutral:
                case EObjectiveEffect.SpawnInjuredCrew:
                case EObjectiveEffect.SetEnableSpreadIssue:
                case EObjectiveEffect.SetCamera:
                case EObjectiveEffect.SetShowObject:
                case EObjectiveEffect.DestroyBlock:
                case EObjectiveEffect.TeleportEnemy:
                case EObjectiveEffect.TeleportPlayer:
                case EObjectiveEffect.KillSurvivor:
                case EObjectiveEffect.DestroyNotSentMissions:
                    height += lineHeight;
                    break;
                case EObjectiveEffect.SearchAndDestroy:
                case EObjectiveEffect.SetEnableForcedStrategy:
                case EObjectiveEffect.SetEnableDCIssueDestination:
                case EObjectiveEffect.ShowMagicSprite:
                    if (!effect.NotEffect)
                    {
                        height += lineHeight;
                    }
                    break;
                case EObjectiveEffect.SetEnableMaxPlanes:
                    height += lineHeight;
                    if (!effect.NotEffect)
                    {
                        height += lineHeight;
                    }
                    break;
                case EObjectiveEffect.AttackCarrier:
                case EObjectiveEffect.SetSentMissionDuration:
                case EObjectiveEffect.SetObjectiveText:
                    height += 2 * lineHeight;
                    break;
                case EObjectiveEffect.AddMission:
                    height += lineHeight;
                    if (!effect.NotEffect)
                    {
                        height += lineHeight;
                    }
                    break;
                case EObjectiveEffect.ShowTimer:
                case EObjectiveEffect.DestroySection:
                    height += 3 * lineHeight;
                    break;
                case EObjectiveEffect.MakeDamageRange:
                    if (!effect.NotEffect)
                    {
                        height += 3 * lineHeight;
                    }
                    break;
                case EObjectiveEffect.SpawnAttack:
                case EObjectiveEffect.SpawnIssue:
                    height += 4 * lineHeight;
                    break;
                case EObjectiveEffect.CustomMissionRetrieval:
                    height += 5 * lineHeight;
                    break;
                case EObjectiveEffect.SetShowNarrator:
                    if (!effect.NotEffect)
                    {
                        height += 6 * lineHeight;
                    }
                    break;
                case EObjectiveEffect.SetShowHighlight:
                    if (Highlights == null)
                    {
                        SetupHighlights();
                    }
                    height += 3f * lineHeight;
                    if (Highlights.Count != 0 && effect.Category != -1)
                    {
                        height += lineHeight;
                    }
                    break;
                case EObjectiveEffect.AdvanceTime:
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(Date));
                    break;
            }
            return height;
        }
        else
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        bool test = position.height <= 2f;
        if (!test)
        {
            position.height = EditorGUIUtility.singleLineHeight;
        }
        float lineHeight = test ? 0f : EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

        bool editor = GetEffect(property, out var map, out var effect);
        effect.Expanded = EditorGUI.Foldout(position, effect.Expanded, property.displayName);
        position.y += lineHeight;
        if (effect.Expanded)
        {
            EditorGUI.indentLevel++;

            var prop = property.FindPropertyRelative(Effect);

            EditorGUI.PropertyField(position, prop);
            position.y += lineHeight;

            bool show = false;
            switch (effect.EffectType)
            {
                case EObjectiveEffect.ShowObjectives:
                case EObjectiveEffect.ActivateObjectives:
                case EObjectiveEffect.Win:
                case EObjectiveEffect.SucceedObjectives:
                case EObjectiveEffect.EnemyCanChase:
                case EObjectiveEffect.ShowTimer:
                case EObjectiveEffect.AddMission:
                case EObjectiveEffect.SearchAndDestroy:
                case EObjectiveEffect.ShowBalancedForcesBar:
                case EObjectiveEffect.MakeDamageRange:
                case EObjectiveEffect.SetEnableDragPlanes:
                case EObjectiveEffect.SetEnableReconUO:
                case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                case EObjectiveEffect.SetEnableRecoveryTimeout:
                case EObjectiveEffect.SetEnableAirstrikeTarget:
                case EObjectiveEffect.SetEnableForcedStrategy:
                case EObjectiveEffect.SetEnableSpreadIssue:
                case EObjectiveEffect.SetEnableDCIssueDestination:
                case EObjectiveEffect.SetEnableDCInMaintenance:
                case EObjectiveEffect.SetEnableDCInPumps:
                case EObjectiveEffect.SetEnableRescueTimer:
                case EObjectiveEffect.SetEnableMoveTime:
                case EObjectiveEffect.SetEnableObsoleteMission:
                case EObjectiveEffect.SetMaxCarrierSpeed:
                case EObjectiveEffect.SetShowNarrator:
                case EObjectiveEffect.SetShowObject:
                case EObjectiveEffect.ShowRescueRange:
                case EObjectiveEffect.ShowCannonRange:
                case EObjectiveEffect.SetShowSpriteStrategyPanel:
                case EObjectiveEffect.ShowAlly:
                case EObjectiveEffect.ShowMissionRange:
                case EObjectiveEffect.ShowMagicSprite:
                    show = true;
                    break;
            }

            if (show)
            {
                prop = property.FindPropertyRelative(NotEffect);

                EditorGUI.PropertyField(position, prop, new GUIContent("Inverse effect type"));
                position.y += lineHeight;

                if (effect.EffectType == EObjectiveEffect.ShowTimer && !prop.boolValue)
                {
                    prop = property.FindPropertyRelative(Minutes);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Start minutes"));
                    position.y += lineHeight;
                }
            }

            GUIContent content = null;
            if (editor)
            {
                prop = property.FindPropertyRelative(TargetTranses);
                switch (effect.EffectType)
                {
                    case EObjectiveEffect.ShowObjectives:
                    case EObjectiveEffect.ActivateObjectives:
                    case EObjectiveEffect.SucceedObjectives:
                    case EObjectiveEffect.SetObjectiveText:
                        content = new GUIContent("Target objectives");
                        break;
                    case EObjectiveEffect.Reveal:
                    case EObjectiveEffect.Destroy:
                    case EObjectiveEffect.ShowHidden:
                    case EObjectiveEffect.Spawn:
                    case EObjectiveEffect.EnemyCanChase:
                    case EObjectiveEffect.SearchAndDestroy:
                    case EObjectiveEffect.SpawnSurvivors:
                    case EObjectiveEffect.SetEnableAirstrikeTarget:
                    case EObjectiveEffect.SpawnNeutral:
                    case EObjectiveEffect.DestroyBlock:
                    case EObjectiveEffect.ShowAlly:
                    case EObjectiveEffect.Identify:
                        content = new GUIContent("Target enemies");
                        break;
                    case EObjectiveEffect.ShowBalancedForcesBar:
                    case EObjectiveEffect.MakeDamageRange:
                    case EObjectiveEffect.ShowMissionRange:
                    case EObjectiveEffect.SetShowSpriteStrategyPanel:
                        if (editor && effect.NotEffect)
                        {
                            prop = null;
                        }
                        else
                        {
                            content = new GUIContent("Target enemies");
                        }
                        break;
                    case EObjectiveEffect.AddMission:
                        if (effect.MissionType == EMissionOrderType.AttackJapan || effect.MissionType == EMissionOrderType.Decoy || effect.MissionType == EMissionOrderType.RescueVIP ||
                            effect.MissionType == EMissionOrderType.MidwayAirstrike)
                        {
                            content = new GUIContent("Target enemies");
                        }
                        else
                        {
                            prop = null;
                        }
                        break;
                    case EObjectiveEffect.ForceCarrierWaypoints:
                    case EObjectiveEffect.SetEnableCoursePosition:
                        content = new GUIContent("Target designer nodes");
                        break;
                    case EObjectiveEffect.Win:
                    case EObjectiveEffect.ShowTimer:
                    case EObjectiveEffect.CustomMissionRetrieval:
                    case EObjectiveEffect.AttackCarrier:
                    case EObjectiveEffect.SpawnRescueShip:
                    case EObjectiveEffect.FinalestDestroySections:
                    case EObjectiveEffect.ForceCarrierSpeed:
                    case EObjectiveEffect.SpawnTowShip:
                    case EObjectiveEffect.SetEnableUI:
                    case EObjectiveEffect.SetEnableCameraInput:
                    case EObjectiveEffect.SetEnableOfficer:
                    case EObjectiveEffect.SetEnableIslandRoomSelection:
                    case EObjectiveEffect.SetEnableSwitch:
                    case EObjectiveEffect.SetEnableCrew:
                    case EObjectiveEffect.SetEnableDepartmentPlacement:
                    case EObjectiveEffect.SetEnableSquadronType:
                    case EObjectiveEffect.SetEnableDragPlanes:
                    case EObjectiveEffect.SetEnableMissions:
                    case EObjectiveEffect.SetEnableReconUO:
                    case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                    case EObjectiveEffect.SetEnableRecoveryTimeout:
                    case EObjectiveEffect.SetEnableForcedStrategy:
                    case EObjectiveEffect.SetEnableEscort:
                    case EObjectiveEffect.SetEnableSpreadIssue:
                    case EObjectiveEffect.SetEnableDCIssueDestination:
                    case EObjectiveEffect.SetEnableDCInMaintenance:
                    case EObjectiveEffect.SetEnableDCInPumps:
                    case EObjectiveEffect.SetEnableRescueTimer:
                    case EObjectiveEffect.SetEnableMoveTime:
                    case EObjectiveEffect.SetEnableObsoleteMission:
                    case EObjectiveEffect.SetEnableMaxPlanes:
                    case EObjectiveEffect.SetEnableEvents:
                    case EObjectiveEffect.SetEnableDCCategory:
                    case EObjectiveEffect.ResetEnables:
                    case EObjectiveEffect.SetMaxCarrierSpeed:
                    case EObjectiveEffect.SetTimeSpeed:
                    case EObjectiveEffect.SetSupplies:
                    case EObjectiveEffect.DestroyNeutrals:
                    case EObjectiveEffect.SetSentMissionDuration:
                    case EObjectiveEffect.SpawnAttack:
                    case EObjectiveEffect.SpawnIssue:
                    case EObjectiveEffect.SpawnInjuredCrew:
                    case EObjectiveEffect.SetShowHighlight:
                    case EObjectiveEffect.SetShowNarrator:
                    case EObjectiveEffect.SetCamera:
                    case EObjectiveEffect.FinishCourseSettingMode:
                    case EObjectiveEffect.DisableEscortButton:
                    case EObjectiveEffect.DisableCancelMission:
                    case EObjectiveEffect.DisableEnemyDisappear:
                    case EObjectiveEffect.DisableBuffClose:
                    case EObjectiveEffect.DisableBuffDeallocation:
                    case EObjectiveEffect.DisableOfficerDeallocation:
                    case EObjectiveEffect.DetectPlayer:
                    case EObjectiveEffect.SetShowObject:
                    case EObjectiveEffect.SwitchFriendlyCAPToMidway:
                    case EObjectiveEffect.ShowPath:
                    case EObjectiveEffect.ShowRescueRange:
                    case EObjectiveEffect.TeleportPlayer:
                    case EObjectiveEffect.AdvanceTime:
                    case EObjectiveEffect.ShowCannonRange:
                    case EObjectiveEffect.ShowSurvivors:
                    case EObjectiveEffect.KillSurvivor:
                    case EObjectiveEffect.ShowCarrier:
                    case EObjectiveEffect.LaunchBombers:
                    case EObjectiveEffect.DestroyNotSentMissions:
                    case EObjectiveEffect.DestroySection:
                    case EObjectiveEffect.ShowMagicSprite:
                    case EObjectiveEffect.DisableAttacksOnAlly:
                    case EObjectiveEffect.RemovePermamentlyMagicIdentify:
                    case EObjectiveEffect.SetSuperTimeSpeed:
                    case EObjectiveEffect.CancelMissions:
                    case EObjectiveEffect.DisableDeath:
                        prop = null;
                        break;
                    case EObjectiveEffect.ShowMovie:
                        prop = null;
                        if (map.Movies.Count == 0)
                        {
                            EditorGUI.LabelField(position, "Add movies first");
                            position.y += lineHeight;

                            effect.Movie = -1;
                        }
                        else
                        {
                            if (effect.Movie == -1)
                            {
                                effect.Movie = 0;
                            }
                            effect.Movie = EditorGUI.Popup(position, "Select movie to show", effect.Movie, GetMoviesTexts(map));
                            position.y += lineHeight;
                        }
                        break;
                }
            }
            else
            {
                prop = property.FindPropertyRelative(Targets);
                switch (effect.EffectType)
                {
                    case EObjectiveEffect.Win:
                    case EObjectiveEffect.ShowTimer:
                    case EObjectiveEffect.CustomMissionRetrieval:
                    case EObjectiveEffect.AttackCarrier:
                    case EObjectiveEffect.SpawnRescueShip:
                    case EObjectiveEffect.FinalestDestroySections:
                    case EObjectiveEffect.ForceCarrierSpeed:
                    case EObjectiveEffect.SpawnTowShip:
                    case EObjectiveEffect.SetEnableUI:
                    case EObjectiveEffect.SetEnableCameraInput:
                    case EObjectiveEffect.SetEnableOfficer:
                    case EObjectiveEffect.SetEnableIslandRoomSelection:
                    case EObjectiveEffect.SetEnableSwitch:
                    case EObjectiveEffect.SetEnableCrew:
                    case EObjectiveEffect.SetEnableDepartmentPlacement:
                    case EObjectiveEffect.SetEnableSquadronType:
                    case EObjectiveEffect.SetEnableDragPlanes:
                    case EObjectiveEffect.SetEnableMissions:
                    case EObjectiveEffect.SetEnableReconUO:
                    case EObjectiveEffect.SetEnableRecoveryOnCarrier:
                    case EObjectiveEffect.SetEnableRecoveryTimeout:
                    case EObjectiveEffect.SetEnableForcedStrategy:
                    case EObjectiveEffect.SetEnableEscort:
                    case EObjectiveEffect.SetEnableSpreadIssue:
                    case EObjectiveEffect.SetEnableDCIssueDestination:
                    case EObjectiveEffect.SetEnableDCInMaintenance:
                    case EObjectiveEffect.SetEnableDCInPumps:
                    case EObjectiveEffect.SetEnableRescueTimer:
                    case EObjectiveEffect.SetEnableMoveTime:
                    case EObjectiveEffect.SetEnableObsoleteMission:
                    case EObjectiveEffect.SetEnableMaxPlanes:
                    case EObjectiveEffect.SetEnableEvents:
                    case EObjectiveEffect.SetEnableDCCategory:
                    case EObjectiveEffect.ResetEnables:
                    case EObjectiveEffect.SetMaxCarrierSpeed:
                    case EObjectiveEffect.SetTimeSpeed:
                    case EObjectiveEffect.SetSupplies:
                    case EObjectiveEffect.DestroyNeutrals:
                    case EObjectiveEffect.SetSentMissionDuration:
                    case EObjectiveEffect.SpawnAttack:
                    case EObjectiveEffect.SpawnIssue:
                    case EObjectiveEffect.SpawnInjuredCrew:
                    case EObjectiveEffect.SetShowHighlight:
                    case EObjectiveEffect.SetShowNarrator:
                    case EObjectiveEffect.SetCamera:
                    case EObjectiveEffect.FinishCourseSettingMode:
                    case EObjectiveEffect.DisableEscortButton:
                    case EObjectiveEffect.DisableCancelMission:
                    case EObjectiveEffect.DisableEnemyDisappear:
                    case EObjectiveEffect.DisableBuffClose:
                    case EObjectiveEffect.DisableBuffDeallocation:
                    case EObjectiveEffect.DisableOfficerDeallocation:
                    case EObjectiveEffect.DetectPlayer:
                    case EObjectiveEffect.SetShowObject:
                    case EObjectiveEffect.SwitchFriendlyCAPToMidway:
                    case EObjectiveEffect.ShowPath:
                    case EObjectiveEffect.ShowRescueRange:
                    case EObjectiveEffect.TeleportPlayer:
                    case EObjectiveEffect.AdvanceTime:
                    case EObjectiveEffect.ShowCannonRange:
                    case EObjectiveEffect.ShowSurvivors:
                    case EObjectiveEffect.KillSurvivor:
                    case EObjectiveEffect.ShowCarrier:
                    case EObjectiveEffect.LaunchBombers:
                    case EObjectiveEffect.DestroyNotSentMissions:
                    case EObjectiveEffect.DestroySection:
                    case EObjectiveEffect.ShowMagicSprite:
                    case EObjectiveEffect.DisableAttacksOnAlly:
                    case EObjectiveEffect.RemovePermamentlyMagicIdentify:
                    case EObjectiveEffect.SetSuperTimeSpeed:
                    case EObjectiveEffect.CancelMissions:
                    case EObjectiveEffect.DisableDeath:
                        prop = null;
                        break;
                    case EObjectiveEffect.ShowMovie:
                        prop = null;
                        if (effect.Movie < 0)
                        {
                            EditorGUI.LabelField(position, "Movie not added");
                            position.y += lineHeight;
                        }
                        else
                        {
                            bool enabled = GUI.enabled;
                            GUI.enabled = false;
                            EditorGUI.IntField(position, effect.Movie);
                            position.y += lineHeight;
                            GUI.enabled = enabled;
                        }
                        break;
                }
            }
            if (prop != null)
            {
                if (!test)
                {
                    position.height = EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                }
                if (content == null)
                {
                    EditorGUI.PropertyField(position, prop, true);
                }
                else
                {
                    EditorGUI.PropertyField(position, prop, content, true);
                }
                if (!test)
                {
                    position.y += position.height;
                    position.height = EditorGUIUtility.singleLineHeight;
                }
            }

            switch (effect.EffectType)
            {
                case EObjectiveEffect.Win:
                    prop = property.FindPropertyRelative(OverrideWinDescriptionID);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.ShowTimer:
                    prop = property.FindPropertyRelative(TimerID);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(TimerTooltipID);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(TimerTooltipDescID);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.CustomMissionRetrieval:
                    prop = property.FindPropertyRelative(HoursToRetrieve);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(editor ? RetrievePositionRect : RetrievePosition);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Retrieval position"));
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(BombersNeeded);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(FightersNeeded);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(TorpedoesNeeded);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.AddMission:
                    prop = property.FindPropertyRelative(MissionType);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    if (!effect.NotEffect)
                    {
                        prop = property.FindPropertyRelative(Count);

                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                    }
                    break;
                case EObjectiveEffect.SearchAndDestroy:
                    if (!effect.NotEffect)
                    {
                        prop = property.FindPropertyRelative(Power);

                        EditorGUI.PropertyField(position, prop, new GUIContent("Attack time in ticks"));
                        position.y += lineHeight;
                    }
                    break;
                case EObjectiveEffect.AttackCarrier:
                    prop = property.FindPropertyRelative(Power);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Force over defense"));
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(AbsoluteForce);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.MakeDamageRange:
                    if (!effect.NotEffect)
                    {
                        prop = property.FindPropertyRelative(Hours);

                        EditorGUI.PropertyField(position, prop, new GUIContent("Attack every hours"));
                        position.y += lineHeight;

                        prop = property.FindPropertyRelative(Range);

                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;

                        prop = property.FindPropertyRelative(Power);

                        EditorGUI.PropertyField(position, prop, new GUIContent("Segment count to damage"));
                        position.y += lineHeight;
                    }
                    break;
                case EObjectiveEffect.ForceCarrierSpeed:
                case EObjectiveEffect.SetMaxCarrierSpeed:
                case EObjectiveEffect.SetTimeSpeed:
                    prop = property.FindPropertyRelative(Power);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Speed index"));
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableUI:
                    prop = property.FindPropertyRelative(UICategories);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableCameraInput:
                    prop = property.FindPropertyRelative(CameraInput);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableOfficer:
                    effect.Power = Toggles(ref position, lineHeight, position.width, effect.Power, "Enabled officers: ", 3);
                    break;
                case EObjectiveEffect.SetEnableIslandRoomSelection:
                    prop = property.FindPropertyRelative(IslandRooms);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableSwitch:
                    effect.Power = Toggles(ref position, lineHeight, position.width, effect.Power, "Enabled switches: ", 3);
                    break;
                case EObjectiveEffect.SetEnableCrew:
                    effect.Power = Toggles(ref position, lineHeight, position.width, effect.Power, "Enabled crew: ", 12);
                    break;
                case EObjectiveEffect.SetEnableDepartmentPlacement:
                    prop = property.FindPropertyRelative(Departments);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableSquadronType:
                    prop = property.FindPropertyRelative(PlaneType);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableMissions:
                    prop = property.FindPropertyRelative(Missions);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableForcedStrategy:
                    if (!effect.NotEffect)
                    {
                        prop = property.FindPropertyRelative(Strategy);

                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                    }
                    break;
                case EObjectiveEffect.SetEnableEscort:
                    effect.Power = Toggles(ref position, lineHeight, position.width, effect.Power, "Enabled escorts: ", 5);
                    break;
                case EObjectiveEffect.SetEnableSpreadIssue:
                    prop = property.FindPropertyRelative(Issue);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableDCIssueDestination:
                    if (!effect.NotEffect)
                    {
                        prop = property.FindPropertyRelative(Issue);

                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                    }
                    break;
                case EObjectiveEffect.SetEnableMaxPlanes:
                    prop = property.FindPropertyRelative(PlaneType);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    if (!effect.NotEffect)
                    {
                        prop = property.FindPropertyRelative(Count);

                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                    }
                    break;
                case EObjectiveEffect.SetEnableEvents:
                    prop = property.FindPropertyRelative(Events);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetEnableDCCategory:
                    prop = property.FindPropertyRelative(DcCategoryFlag);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetSupplies:
                    prop = property.FindPropertyRelative(Power);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Supplies count"));
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SpawnNeutral:
                    prop = property.FindPropertyRelative(UOType);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetSentMissionDuration:
                    prop = property.FindPropertyRelative(MissionType);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(Hours);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Hours count"));
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SpawnAttack:
                    prop = property.FindPropertyRelative(Power);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Attack power"));
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(SecondSubsection);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Attack power relative to players defense"));
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(AttackStrikeGroup);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(Hours);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Hours to attack"));
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SpawnIssue:
                    prop = property.FindPropertyRelative(Section);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(SecondSubsection);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(Power);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Segment index"));
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(Issue);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SpawnInjuredCrew:
                    prop = property.FindPropertyRelative(Department);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetObjectiveText:
                    prop = property.FindPropertyRelative(TimerTooltipID);

                    EditorGUI.PropertyField(position, prop, new GUIContent("New title id"));
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(TimerTooltipDescID);

                    EditorGUI.PropertyField(position, prop, new GUIContent("New description id"));
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetShowHighlight:
                    if (Highlights == null)
                    {
                        SetupHighlights();
                    }
                    if (Highlights.Count == 0)
                    {
                        EditorGUI.LabelField(position, "Cache highlights first");
                        position.y += lineHeight;
                    }
                    else
                    {
                        effect.Category = EditorGUI.IntSlider(position, Mathf.Clamp(effect.Category, -1, Highlights.Count - 1), -1, Highlights.Count - 1);
                        position.y += lineHeight;
                        if (effect.Category != -1)
                        {
                            var highlights = Highlights[effect.Category];
                            if (highlights.Length == 0)
                            {
                                EditorGUI.LabelField(position, "No highlights in this category");
                                position.y += lineHeight;
                                effect.Highlight = "";
                            }
                            else
                            {
                                int index = EditorGUI.IntPopup(position, Array.IndexOf(highlights, effect.Highlight), highlights, HighlightsInts[effect.Category]);
                                position.y += lineHeight;
                                effect.Highlight = highlights[Mathf.Clamp(index, 0, highlights.Length - 1)];
                            }
                        }
                    }

                    prop = property.FindPropertyRelative(Time);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    if (GUI.Button(position, "Refresh highlights"))
                    {
                        SetupHighlights();
                    }
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetShowNarrator:
                    if (!effect.NotEffect)
                    {
                        prop = property.FindPropertyRelative(TimerID);

                        EditorGUI.PropertyField(position, prop, new GUIContent("Description id"));
                        position.y += lineHeight;

                        prop = property.FindPropertyRelative(TimerTooltipID);

                        EditorGUI.PropertyField(position, prop, new GUIContent("Objective title id"));
                        position.y += lineHeight;

                        prop = property.FindPropertyRelative(TimerTooltipDescID);

                        EditorGUI.PropertyField(position, prop, new GUIContent("Objective description id"));
                        position.y += lineHeight;

                        prop = property.FindPropertyRelative(SoundEventReference);

                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;

                        prop = property.FindPropertyRelative(Power);

                        EditorGUI.PropertyField(position, prop, new GUIContent("Override max width"));
                        position.y += lineHeight;

                        prop = property.FindPropertyRelative(OverridePosition);

                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                    }
                    break;
                case EObjectiveEffect.SetCamera:
                    prop = property.FindPropertyRelative(CameraView);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.SetShowObject:
                    prop = property.FindPropertyRelative(Power);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Object index"));
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.DestroyBlock:
                    prop = property.FindPropertyRelative(Count);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Block index"));
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.TeleportEnemy:
                case EObjectiveEffect.TeleportPlayer:
                    prop = property.FindPropertyRelative(OverridePosition);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Teleport position"));
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.AdvanceTime:
                    prop = property.FindPropertyRelative(Date);

                    EditorGUI.PropertyField(position, prop, true);
                    position.y += EditorGUI.GetPropertyHeight(prop);
                    break;
                case EObjectiveEffect.KillSurvivor:
                    prop = property.FindPropertyRelative(Power);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Survivor id"));
                    position.y += EditorGUI.GetPropertyHeight(prop);
                    break;
                case EObjectiveEffect.SetShowSpriteStrategyPanel:
                    prop = property.FindPropertyRelative(editor ? ObjectiveTransform : ObjectiveTarget);

                    EditorGUI.PropertyField(position, prop, new GUIContent("Objective"));
                    position.y += EditorGUI.GetPropertyHeight(prop);
                    break;
                case EObjectiveEffect.DestroyNotSentMissions:
                    prop = property.FindPropertyRelative(MissionType);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.DestroySection:
                    prop = property.FindPropertyRelative(Section);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(FirstSubsection);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(SecondSubsection);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    break;
                case EObjectiveEffect.ShowMagicSprite:
                    if (!effect.NotEffect)
                    {
                        prop = property.FindPropertyRelative(Power);

                        EditorGUI.PropertyField(position, prop, new GUIContent("Magic sprite index"));
                        position.y += lineHeight;
                    }
                    break;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private bool GetEffect(SerializedProperty prop, out SOTacticMap map, out ObjectiveEffectData effect)
    {
        bool result = ObjectiveData.GetObjectiveData(prop, out map, out ObjectiveData data);
        int index = prop.displayName.LastIndexOf(' ');
        index = int.Parse(prop.displayName.Substring(index + 1, prop.displayName.Length - index - 1));
        effect = data.Effects[index];
        return result;
    }

    private string[] GetMoviesTexts(SOTacticMap map)
    {
        var result = new string[map.Movies.Count];
        for (int i = 0; i < map.Movies.Count; i++)
        {
            result[i] = map.Movies[i].Clip.name;
        }
        return result;
    }

    private int Toggles(ref Rect position, float lineHeight, float width, int current, string text, int count)
    {
        if (SlotMasksTexts == null)
        {
            SlotMasksTexts = new List<string[]>();
        }
        while (SlotMasksTexts.Count <= count)
        {
            SlotMasksTexts.Add(new string[SlotMasksTexts.Count]);
        }
        if (string.IsNullOrWhiteSpace(SlotMasksTexts[count][0]))
        {
            for (int i = 0; i < count; i++)
            {
                SlotMasksTexts[count][i] = "Slot " + (i + 1).ToString("00");
            }
        }
        int result = EditorGUI.MaskField(position, text, current, SlotMasksTexts[count]);
        position.y += lineHeight;

        return result;
    }

    private void SetupHighlights()
    {
        try
        {
            var lines = Regex.Split(File.ReadAllText(HighlightsManager.Path), "\r|\n|\r\n");
            Highlights = new List<string[]>();
            HighlightsInts = new List<int[]>();
            var list = new List<string>();
            foreach (var line in lines)
            {
                if (line.StartsWith(HighlightsManager.CloseString))
                {
                    Assert.IsTrue(line == HighlightsManager.CloseString, "bad highlight name");
                    AddHighlightGroup(list);
                }
                else
                {
                    list.Add(line);
                }
            }
        }
        catch (Exception _)
        {
            Highlights = new List<string[]>();
        }
    }

    private void AddHighlightGroup(List<string> list)
    {
        Highlights.Add(list.ToArray());

        var group = new int[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            group[i] = i;
        }
        HighlightsInts.Add(group);

        list.Clear();
    }
}
