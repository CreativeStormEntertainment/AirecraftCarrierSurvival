using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BonusData))]
public class BonusDataPropertyDrawer : PropertyDrawer
{
    private const string ModifierType = nameof(BonusData.ModifierType);
    private const string Data = nameof(BonusData.Data);
    private const string ThisSlot = nameof(BonusData.ThisSlot);
    private const string Squadron = nameof(BonusData.SquadronType);
    private const string Requirement = nameof(BonusData.Requirement);
    private const string RequirementData = nameof(BonusData.RequirementData);
    private const string Benefitor = nameof(BonusData.Benefitor);
    private const string Effect = nameof(BonusData.Effect);
    private const string EffectData = nameof(BonusData.EffectData);
    private const string IsPlayer = nameof(BonusData.IsPlayer);

    private const string Negate = nameof(BonusExtendData.Negate);
    private const string Slot = nameof(BonusExtendData.Slot);
    private const string Various = nameof(BonusExtendData.Various);
    private const string Maneuver = nameof(BonusExtendData.ManeuverType);

    private const string Value1 = nameof(BonusValueData.Value1);
    private const string Value2 = nameof(BonusValueData.Value2);
    private const string ValueSquadronType = nameof(BonusValueData.SquadronType);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0f;// base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        try
        {
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                var modifierTypeProp = property.FindPropertyRelative(ModifierType);
                bool isPlayer = property.FindPropertyRelative(IsPlayer).boolValue;
                var dataProp = property.FindPropertyRelative(Data);

                if (isPlayer)
                {
                    EditorGUILayout.PropertyField(modifierTypeProp);
                }
                else
                {
                    var bonusType = (EEnemyBonusType)modifierTypeProp.enumValueIndex;
                    switch (bonusType)
                    {
                        case EEnemyBonusType.Always:
                        case EEnemyBonusType.SpecificPiece:
                        case EEnemyBonusType.NumberOfNext:
                        case EEnemyBonusType.AllOfManeuverType:
                        case EEnemyBonusType.AllOfSquadronType:
                            break;
                        default:
                            bonusType = EEnemyBonusType.Always;
                            break;
                    }
                    bonusType = (EEnemyBonusType)EditorGUILayout.EnumPopup(ModifierType, bonusType);
                    modifierTypeProp.enumValueIndex = (int)bonusType;
                }
                var modifierType = (EBonusType)modifierTypeProp.enumValueIndex;
                var reqProp = property.FindPropertyRelative(Requirement);
                EditorGUI.indentLevel++;
                switch (modifierType)
                {
                    case EBonusType.Always:
                        break;
                    case EBonusType.Next:
                        EditorGUILayout.PropertyField(dataProp.FindPropertyRelative(Negate), new GUIContent("Previous"));
                        break;
                    case EBonusType.SpecificPiece:
                        var slotProp = dataProp.FindPropertyRelative(Slot);
                        var varProp = dataProp.FindPropertyRelative(Various);
                        TwoSlots(slotProp, varProp, "Include second slot", "Second slot");
                        break;
                    case EBonusType.NumberOfNext:
                        EditorGUILayout.PropertyField(dataProp.FindPropertyRelative(Negate), new GUIContent("Number of previous"));
                        var thisSlotProp = property.FindPropertyRelative(ThisSlot);
                        if (isPlayer)
                        {
                            EditorGUILayout.PropertyField(thisSlotProp);
                        }
                        else
                        {
                            thisSlotProp.boolValue = false;
                        }
                        if (!thisSlotProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(dataProp.FindPropertyRelative(Slot));
                        }
                        Count(dataProp.FindPropertyRelative(Various));
                        break;
                    case EBonusType.AllOfManeuverType:
                        EditorGUILayout.PropertyField(dataProp.FindPropertyRelative(Negate), new GUIContent("Not of the maneuver type"));
                        EditorGUILayout.PropertyField(dataProp.FindPropertyRelative(Maneuver));
                        break;
                    case EBonusType.AllOfSquadronType:
                        EditorGUILayout.PropertyField(dataProp.FindPropertyRelative(Negate), new GUIContent("Not of the squadron type"));
                        EditorGUILayout.PropertyField(property.FindPropertyRelative(Squadron));
                        break;
                    case EBonusType.SpecificPlacement:
                        var negativeProp = dataProp.FindPropertyRelative(Negate);
                        EditorGUILayout.PropertyField(negativeProp, new GUIContent("Not specific placement"));
                        string toggleText;
                        string slotText;
                        if (negativeProp.boolValue)
                        {
                            toggleText = "Add second AND not in slot";
                            slotText = "Second not in slot";
                        }
                        else
                        {
                            toggleText = "Add second OR slot";
                            slotText = "Or in slot";
                        }
                        TwoSlots(dataProp.FindPropertyRelative(Slot), dataProp.FindPropertyRelative(Various), toggleText, slotText);
                        break;
                    case EBonusType.AfterSpecificPlacement:
                        EditorGUILayout.PropertyField(dataProp.FindPropertyRelative(Negate), new GUIContent("Before specific placement"));
                        EditorGUILayout.PropertyField(dataProp.FindPropertyRelative(Slot));
                        break;
                    case EBonusType.CustomRequirements:
                        EditorGUILayout.PropertyField(reqProp);
                        var reqDataProp = property.FindPropertyRelative(RequirementData);
                        var reqManeuverProp = reqDataProp.FindPropertyRelative(Maneuver);
                        var reqSquadronProp = reqDataProp.FindPropertyRelative(Squadron);
                        switch ((EBonusRequirement)reqProp.enumValueIndex)
                        {
                            case EBonusRequirement.BetweenSquadrons:
                                EditorGUILayout.PropertyField(reqSquadronProp);
                                break;
                            case EBonusRequirement.BetweenManeuvers:
                                NotAnyManeuverType(reqManeuverProp);
                                break;
                            case EBonusRequirement.OnlyManeuver:
                                EditorGUILayout.PropertyField(reqDataProp.FindPropertyRelative(Negate), new GUIContent("No maneuvers of type"));
                                NotAnyManeuverType(reqManeuverProp);
                                break;
                            case EBonusRequirement.SpecificOfManeuver:
                                EditorGUILayout.PropertyField(reqDataProp.FindPropertyRelative(Negate), new GUIContent("Not of maneuver of type"));
                                NotAnyManeuverType(reqManeuverProp);
                                TwoSlots(reqDataProp.FindPropertyRelative(Slot), reqDataProp.FindPropertyRelative(Various), "Include second slot", "Second slot");
                                break;
                            case EBonusRequirement.SpecificOfSquadron:
                                EditorGUILayout.PropertyField(reqDataProp.FindPropertyRelative(Negate), new GUIContent("Not of squadron type"));
                                EditorGUILayout.PropertyField(reqSquadronProp);
                                TwoSlots(reqDataProp.FindPropertyRelative(Slot), reqDataProp.FindPropertyRelative(Various), "Include second slot", "Second slot");
                                break;
                            case EBonusRequirement.GroupCountOfManeuver:
                                var negateProp = reqDataProp.FindPropertyRelative(Negate);
                                EditorGUILayout.PropertyField(negateProp, new GUIContent("No maneuvers of type, next to each other"));
                                EditorGUILayout.PropertyField(reqManeuverProp);
                                if (!negateProp.boolValue)
                                {
                                    Count(reqDataProp.FindPropertyRelative(Various));
                                }
                                break;
                            case EBonusRequirement.AllManeuverGroups:
                                break;
                            case EBonusRequirement.AllManeuversOfType:
                                EditorGUILayout.PropertyField(reqManeuverProp);
                                break;
                            case EBonusRequirement.NextManeuver:
                                EditorGUILayout.PropertyField(reqDataProp.FindPropertyRelative(Negate), new GUIContent("Previous maneuver"));
                                EditorGUILayout.PropertyField(reqManeuverProp);
                                break;
                            case EBonusRequirement.NextSquadron:
                                EditorGUILayout.PropertyField(reqDataProp.FindPropertyRelative(Negate), new GUIContent("Previous of squadron type"));
                                EditorGUILayout.PropertyField(reqSquadronProp);
                                break;
                        }
                        break;
                }
                EditorGUI.indentLevel--;
                var benefitorProp = property.FindPropertyRelative(Benefitor);
                if (benefitorProp.intValue == -10)
                {
                    benefitorProp.intValue = 0;
                }
                switch (modifierType)
                {
                    case EBonusType.Always:
                    case EBonusType.NumberOfNext:
                    case EBonusType.AllOfManeuverType:
                        var benefitor = (EBaseBenefitor)benefitorProp.enumValueIndex;
                        if (!isPlayer || benefitor != EBaseBenefitor.BaseAndThis)
                        {
                            benefitor = EBaseBenefitor.BaseWithoutThis;
                        }
                        bool enabled = GUI.enabled;
                        GUI.enabled = GUI.enabled && isPlayer;
                        benefitor = (EBaseBenefitor)EditorGUILayout.EnumPopup(Benefitor, benefitor);
                        GUI.enabled = enabled;
                        benefitorProp.enumValueIndex = (int)benefitor;
                        break;
                    case EBonusType.AllOfSquadronType:
                        if (isPlayer)
                        {
                            var benefitor2 = (EBaseExtendedBenefitor)benefitorProp.enumValueIndex;
                            switch (benefitor2)
                            {
                                case EBaseExtendedBenefitor.BaseAndThis:
                                case EBaseExtendedBenefitor.BaseWithoutThis:
                                case EBaseExtendedBenefitor.BaseAllPrev:
                                case EBaseExtendedBenefitor.BaseAllNext:
                                    break;
                                default:
                                    benefitor2 = EBaseExtendedBenefitor.BaseWithoutThis;
                                    break;
                            }
                            benefitorProp.enumValueIndex = (int)(EBaseExtendedBenefitor)EditorGUILayout.EnumPopup(Benefitor, benefitor2);
                        }
                        else
                        {
                            bool enabled2 = GUI.enabled;
                            GUI.enabled = false;
                            benefitorProp.enumValueIndex = (int)EBaseBenefitor.BaseWithoutThis;
                            benefitorProp.enumValueIndex = (int)(EBaseBenefitor)EditorGUILayout.EnumPopup(Benefitor, (EBaseBenefitor)benefitorProp.enumValueIndex);
                            GUI.enabled = enabled2;
                        }
                        break;
                    case EBonusType.CustomRequirements:
                        if (reqProp.enumValueIndex == (int)EBonusRequirement.AllManeuverGroups)
                        {
                            benefitorProp.enumValueIndex = (int)EBonusBenefitor.BaseAndThis;
                            bool enabled2 = GUI.enabled;
                            GUI.enabled = false;
                            EditorGUILayout.EnumPopup(Benefitor, EBonusBenefitor.BaseAndThis);
                            GUI.enabled = enabled2;
                        }
                        else
                        {
                            var bonusRequirement = (EBonusRequirement)property.FindPropertyRelative(Requirement).enumValueIndex;
                            var reqDataProp = property.FindPropertyRelative(RequirementData);
                            bool specialCase = bonusRequirement == EBonusRequirement.GroupCountOfManeuver &&
                                reqDataProp.FindPropertyRelative(Negate).boolValue &&
                                (EManeuverType)reqDataProp.FindPropertyRelative(Maneuver).enumValueIndex == EManeuverType.Any;
                            bool includePrevNext = bonusRequirement == EBonusRequirement.SpecificOfManeuver || bonusRequirement == EBonusRequirement.SpecificOfSquadron;

                            if (isPlayer)
                            {
                                if (specialCase)
                                {
                                    var benefitor2 = (ESpecialBenefitor)benefitorProp.enumValueIndex;
                                    if (benefitor2 != ESpecialBenefitor.Everyone)
                                    {
                                        benefitor2 = ESpecialBenefitor.JustThis;
                                    }
                                    benefitor2 = (ESpecialBenefitor)EditorGUILayout.EnumPopup(Benefitor, benefitor2);
                                    benefitorProp.enumValueIndex = (int)benefitor2;
                                }
                                else if (!includePrevNext)
                                {
                                    var benefitor2 = (EExtendedBenefitor)benefitorProp.enumValueIndex;
                                    switch (benefitor2)
                                    {
                                        case EExtendedBenefitor.JustThis:
                                        case EExtendedBenefitor.BaseAndThis:
                                        case EExtendedBenefitor.BaseWithoutThis:
                                        case EExtendedBenefitor.Everyone:
                                            break;
                                        default:
                                            benefitor2 = EExtendedBenefitor.JustThis;
                                            break;

                                    }
                                    benefitor2 = (EExtendedBenefitor)EditorGUILayout.EnumPopup(Benefitor, benefitor2);
                                    benefitorProp.enumValueIndex = (int)benefitor2;
                                }
                                else
                                {
                                    EditorGUILayout.PropertyField(benefitorProp);
                                }
                            }
                            else
                            {
                                var benefitor2 = (EEnemyBenefitor)benefitorProp.enumValueIndex;
                                if (specialCase)
                                {
                                    benefitor2 = EEnemyBenefitor.Everyone;
                                }
                                else if (benefitor2 != EEnemyBenefitor.Everyone)
                                {
                                    benefitor2 = EEnemyBenefitor.BaseWithoutThis;
                                }
                                bool enabled2 = GUI.enabled;
                                GUI.enabled = GUI.enabled && !specialCase;
                                benefitor2 = (EEnemyBenefitor)EditorGUILayout.EnumPopup(Benefitor, benefitor2);
                                GUI.enabled = enabled2;
                                benefitorProp.enumValueIndex = (int)benefitor2;
                            }
                        }
                        break;
                    default:
                        if (modifierType != EBonusType.SpecificPiece || isPlayer)
                        {
                            benefitorProp.intValue = -10;
                        }
                        break;
                }
                var effectProp = property.FindPropertyRelative(Effect);
                var effectDataProp = property.FindPropertyRelative(EffectData);

                if (isPlayer)
                {
                    var playerEffect = (EPlayerBonusValue)effectProp.enumValueIndex;
                    switch (playerEffect)
                    {
                        case EPlayerBonusValue.FlatAddAttackParameters:
                        case EPlayerBonusValue.MultiplyAttackParameters:
                        case EPlayerBonusValue.FlatAddSquadronType:
                        case EPlayerBonusValue.FlatValueSquadronType:
                            break;
                        default:
                            playerEffect = EPlayerBonusValue.FlatAddAttackParameters;
                            break;
                    }
                    playerEffect = (EPlayerBonusValue)EditorGUILayout.EnumPopup(Effect, playerEffect);
                    effectProp.enumValueIndex = (int)playerEffect;
                }
                else
                {
                    var enemyEffect = (EEnemyBonusValue)effectProp.enumValueIndex;
                    switch (enemyEffect)
                    {
                        case EEnemyBonusValue.FlatAddAttackParameters:
                        case EEnemyBonusValue.MultiplyAttackParameters:
                        case EEnemyBonusValue.DisableModifier:
                        case EEnemyBonusValue.DisableAll:
                            break;
                        default:
                            enemyEffect = EEnemyBonusValue.FlatAddAttackParameters;
                            break;
                    }
                    enemyEffect = (EEnemyBonusValue)EditorGUILayout.EnumPopup(Effect, enemyEffect);
                    effectProp.enumValueIndex = (int)enemyEffect;
                }

                var effect = (EBonusValue)effectProp.enumValueIndex;
                EditorGUI.indentLevel++;
                switch (effect)
                {
                    case EBonusValue.FlatAddAttackParameters:
                        IntField(effectDataProp.FindPropertyRelative(Value1), "Attack", false);
                        IntField(effectDataProp.FindPropertyRelative(Value2), "Defense", false);
                        break;
                    case EBonusValue.MultiplyAttackParameters:
                        var value1Prop = effectDataProp.FindPropertyRelative(Value1);
                        var value2Prop = effectDataProp.FindPropertyRelative(Value2);
                        EditorGUILayout.PropertyField(value1Prop, new GUIContent("Attack multiplier"));
                        EditorGUILayout.PropertyField(value2Prop, new GUIContent("Defense multiplier"));
                        FloatCheck(value1Prop);
                        FloatCheck(value2Prop);
                        break;
                    case EBonusValue.FlatAddSquadronType:
                        IntField(effectDataProp.FindPropertyRelative(Value1), "Squadron count", false);
                        IntField(effectDataProp.FindPropertyRelative(Value2), "Min value of squadrons to modify its value", false);
                        EditorGUILayout.PropertyField(effectDataProp.FindPropertyRelative(ValueSquadronType), new GUIContent("Squadron type"));
                        break;
                    case EBonusValue.FlatValueSquadronType:
                        IntField(effectDataProp.FindPropertyRelative(Value1), "Squadron count", true);
                        EditorGUILayout.PropertyField(effectDataProp.FindPropertyRelative(ValueSquadronType), new GUIContent("Squadron type"));
                        break;
                    case EBonusValue.DisableModifier:
                    case EBonusValue.DisableAll:
                        break;
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }
        catch (Exception ex)
        {
            if (ex is ExitGUIException)
            {
                throw;
            }
            if (!(ex is ArgumentException && ex.Message.StartsWith("Getting control ")))
            {
                Debug.LogException(ex);
            }
        }
    }

    private void TwoSlots(SerializedProperty firstSlot, SerializedProperty secondSlot, string toggleText, string slotText)
    {
        EditorGUILayout.PropertyField(firstSlot);
        if (EditorGUILayout.Toggle(toggleText, secondSlot.intValue > -1))
        {
            if (secondSlot.intValue == -1)
            {
                secondSlot.intValue = 0;
            }
            EditorGUILayout.PropertyField(secondSlot, new GUIContent(slotText));
        }
        else
        {
            secondSlot.intValue = -1;
        }
        //EditorGUILayout.EndToggleGroup();
    }

    private void Count(SerializedProperty property)
    {
        if (property.intValue < 0)
        {
            property.intValue = 0;
        }
        EditorGUILayout.PropertyField(property, new GUIContent("How many of"));
    }

    private void NotAnyManeuverType(SerializedProperty property)
    {
        var maneuverType = (EManeuverType)property.enumValueIndex;
        if (maneuverType == EManeuverType.Any)
        {
            maneuverType = EManeuverType.Aggressive;
        }
        maneuverType = (EManeuverType)EditorGUILayout.EnumPopup("Maneuver type", maneuverType);
        if (maneuverType == EManeuverType.Any)
        {
            maneuverType = EManeuverType.Aggressive;
        }
        property.enumValueIndex = (int)maneuverType;
    }

    private void FloatCheck(SerializedProperty property)
    {
        if (property.floatValue < 0f)
        {
            property.floatValue = 0f;
        }
    }

    private void IntField(SerializedProperty property, string text, bool positive)
    {
        int value = EditorGUILayout.IntField(text, (int)property.floatValue);
        if (positive)
        {
            value = Mathf.Abs(value);
        }
        property.floatValue = value;
    }
}
