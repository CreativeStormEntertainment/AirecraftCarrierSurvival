using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectiveData))]
public class ObjectiveDataPropertyDrawer : PropertyDrawer
{
    private const string Title = nameof(ObjectiveData.Title);
    private const string Description = nameof(ObjectiveData.Description);
    private const string ObjectiveTargets = nameof(ObjectiveData.ObjectiveTargets);
    private const string ObjectiveTargetVectors = nameof(ObjectiveData.ObjectiveTargetVectors);
    private const string ObjectiveTargetIDs = nameof(ObjectiveData.ObjectiveTargetIDs);
    private const string LoseType = nameof(ObjectiveData.LoseType);
    private const string InverseFinishStateInSummary = nameof(ObjectiveData.InverseFinishStateInSummary);
    private const string ObjectiveCategory = nameof(ObjectiveData.ObjectiveCategory);
    private const string Type = nameof(ObjectiveData.Type);
    private const string NotType = nameof(ObjectiveData.NotType);
    private const string TargetType = nameof(ObjectiveData.TargetType);
    private const string FailOnNegativeTarget = nameof(ObjectiveData.FailOnNegativeTarget);
    private const string Count = nameof(ObjectiveData.Count);
    private const string MissionType = nameof(ObjectiveData.MissionType);
    private const string Range = nameof(ObjectiveData.Range);
    private const string Simul = nameof(ObjectiveData.Simultaneously);
    private const string TargetBlock = nameof(ObjectiveData.TargetBlock);
    private const string TargetTranses = nameof(ObjectiveData.TargetTranses);
    private const string Targets = nameof(ObjectiveData.Targets);
    private const string SndTargetTrans = nameof(ObjectiveData.SecondaryTargetEnemy);
    private const string SndTarget = nameof(ObjectiveData.SecondaryTarget);
    private const string StrikeGroupActiveSkill = nameof(ObjectiveData.StrikeGroupActiveSkill);
    private const string CameraView = nameof(ObjectiveData.CameraView);
    private const string RoomType = nameof(ObjectiveData.RoomType);
    private const string WindowType = nameof(ObjectiveData.WindowType);
    private const string IndexValue = nameof(ObjectiveData.IndexValue);
    private const string SquadronType = nameof(ObjectiveData.SquadronType);
    private const string ManeuverType = nameof(ObjectiveData.ManeuverType);
    private const string Effects = nameof(ObjectiveData.Effects);
    private const string Active = nameof(ObjectiveData.Active);
    private const string Visible = nameof(ObjectiveData.Visible);
    private const string InverseComplete = nameof(ObjectiveData.InverseComplete);

    private const string MapCornerData = nameof(ObjectiveData.MapCornerData);
    private const string CornerOrientation = nameof(global::MapCornerData.CornerOrientation);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        bool editor = ObjectiveData.GetObjectiveData(property, out _, out ObjectiveData data);
        if (editor || data.Expanded)
        {
            float height = EditorGUIUtility.singleLineHeight;

            float lineHeight = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            height += 9 * lineHeight;
            SerializedProperty prop;
            if (editor)
            {
                height += 3 * lineHeight;

                prop = property.FindPropertyRelative(ObjectiveTargets);
                height += EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                prop = property.FindPropertyRelative(ObjectiveTargetIDs);
                height += EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                height += EditorGUIUtility.standardVerticalSpacing;
                prop = property.FindPropertyRelative(ObjectiveTargetVectors);
                height += EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            bool show = false;
            switch (data.Type)
            {
                case EObjectiveType.CompleteObjective:
                case EObjectiveType.ReachMiles:
                case EObjectiveType.Time:
                case EObjectiveType.FinishCustomMission:
                case EObjectiveType.FinishMissions:
                case EObjectiveType.RescueSurvivors:
                case EObjectiveType.ZoomCamera:
                case EObjectiveType.OpenWindow:
                case EObjectiveType.UseBuffOrder:
                case EObjectiveType.ChangeDeckState:
                case EObjectiveType.DcReachMaintenance:
                case EObjectiveType.EnemyProximity:
                    show = true;
                    break;
            }
            if (show)
            {
                height += lineHeight;
            }
            else if (data.Type == EObjectiveType.ReachMapCorner)
            {
                height += 3 * lineHeight;
            }

            if (editor)
            {
                prop = property.FindPropertyRelative(TargetTranses);
                switch (data.Type)
                {
                    case EObjectiveType.CompleteObjective:
                    case EObjectiveType.Destroy:
                    case EObjectiveType.DestroyBlock:
                    case EObjectiveType.Reveal:
                    case EObjectiveType.FinishMissions:
                    case EObjectiveType.SendAirstrikeWithoutLosses:
                    case EObjectiveType.Reach:
                    case EObjectiveType.RevealBlocks:
                    case EObjectiveType.EnemyProximity:
                    case EObjectiveType.SetSpecificCourse:
                        height += EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                        height += EditorGUIUtility.standardVerticalSpacing;
                        break;
                }
                if (data.Type == EObjectiveType.Reach)
                {
                    height += 2 * lineHeight;
                }
            }
            else
            {
                height += lineHeight;
                prop = property.FindPropertyRelative(Targets);
                height += EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            show = false;
            switch (data.Type)
            {
                case EObjectiveType.CompleteObjective:
                case EObjectiveType.Destroy:
                case EObjectiveType.DestroyBlock:
                case EObjectiveType.Reveal:
                case EObjectiveType.FinishMissions:
                case EObjectiveType.RevealBlocks:
                    show = true;
                    break;
                case EObjectiveType.Reach:
                    if (!data.Simultaneously)
                    {
                        show = true;
                    }
                    break;
            }
            if (show)
            {
                height += lineHeight;
            }
            if (data.Type == EObjectiveType.Reach && data.SecondaryTargetEnemy == null)
            {
                height += 2 * lineHeight;
            }
            if ((show && data.TargetType == EObjectiveTarget.Number) ||
                data.Type == EObjectiveType.ReachMiles || data.Type == EObjectiveType.Time || data.Type == EObjectiveType.FinishMissions || data.Type == EObjectiveType.RescueSurvivors ||
                data.Type == EObjectiveType.DeploySquadrons || data.Type == EObjectiveType.DestroyBlockCount || data.Type == EObjectiveType.RevealUO)
            {
                height += lineHeight;
            }

            switch (data.Type)
            {
                case EObjectiveType.CompleteObjective:
                    height += 2 * lineHeight;
                    break;
                case EObjectiveType.DestroyBlock:
                case EObjectiveType.Reveal:
                case EObjectiveType.FinishMissions:
                case EObjectiveType.MissionAfterAction:
                case EObjectiveType.RescueSurvivors:
                case EObjectiveType.UseStrikeGroupActiveSkill:
                case EObjectiveType.CameraView:
                case EObjectiveType.OfficerReachedRoom:
                case EObjectiveType.OpenWindow:
                case EObjectiveType.SetCarrierSpeed:
                case EObjectiveType.DragCrew:
                case EObjectiveType.DeploySquadrons:
                case EObjectiveType.PlanMission:
                case EObjectiveType.SendMission:
                case EObjectiveType.HoverMission:
                case EObjectiveType.StartPlanningMission:
                case EObjectiveType.ManeuverCategory:
                case EObjectiveType.RevealBlocks:
                case EObjectiveType.EnemyProximity:
                case EObjectiveType.DestroyBlockCount:
                case EObjectiveType.OrderSendMission:
                case EObjectiveType.RescueSpecificSurvivor:
                    height += lineHeight;
                    break;
                case EObjectiveType.Reach:
                    if (data.SecondaryTargetEnemy == null)
                    {
                        height += lineHeight;
                    }
                    break;
            }
            prop = property.FindPropertyRelative(Effects);
            height += EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
            return height;
        }
        else
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool test = position.height <= 2f;

        bool end = false;

        try
        {
            var wholePosition = position;
            if (!test)
            {
                position.height = EditorGUIUtility.singleLineHeight;
            }
            float lineHeight = test ? 0f : EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            bool editor = ObjectiveData.GetObjectiveData(property, out _, out ObjectiveData data);
            bool expanded;
            if (editor)
            {
                expanded = true;
                EditorGUI.BeginProperty(wholePosition, label, property);
                end = true;
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);
                end = true;

                data.Expanded = EditorGUI.Foldout(position, data.Expanded, property.displayName);

                EditorGUI.EndProperty();
                end = false;

                position.y += lineHeight;
                expanded = data.Expanded;
                EditorGUI.indentLevel++;
            }

            if (expanded)
            {
                var prop = property.FindPropertyRelative(Title);
                EditorGUI.PropertyField(position, prop, new GUIContent("Title loc ID"));
                position.y += lineHeight;

                prop = property.FindPropertyRelative(Description);
                EditorGUI.PropertyField(position, prop, new GUIContent("Description loc ID"));
                position.y += lineHeight;

                if (editor)
                {
                    EditorGUI.LabelField(position, "Leave none if objective shouldn't show on tactical map");
                    position.y += lineHeight;
                    EditorGUI.LabelField(position, "Drag enemy here for sprite follow him");
                    position.y += lineHeight;
                    EditorGUI.LabelField(position, "Anything else will show immovable sprite");
                    position.y += lineHeight;

                    prop = property.FindPropertyRelative(ObjectiveTargets);

                    if (!test)
                    {
                        position.height = EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                    }
                    EditorGUI.PropertyField(position, prop, new GUIContent("Objective sprite position"), true);
                    if (!test)
                    {
                        position.y += position.height;
                        position.y += EditorGUIUtility.standardVerticalSpacing;
                        position.height = EditorGUIUtility.singleLineHeight;
                    }
                }
                else
                {
                    prop = property.FindPropertyRelative(ObjectiveTargetIDs);
                    if (!test)
                    {
                        position.height = EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                    }
                    EditorGUI.PropertyField(position, prop, new GUIContent("Objective sprite on enemies"), true);
                    if (!test)
                    {
                        position.y += position.height;
                        position.y += EditorGUIUtility.standardVerticalSpacing;
                        position.height = EditorGUIUtility.singleLineHeight;
                    }

                    prop = property.FindPropertyRelative(ObjectiveTargetVectors);
                    if (!test)
                    {
                        position.height = EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                    }
                    EditorGUI.PropertyField(position, prop, new GUIContent("Objective sprite on map"), true);
                    if (!test)
                    {
                        position.y += position.height;
                        position.y += EditorGUIUtility.standardVerticalSpacing;
                        position.height = EditorGUIUtility.singleLineHeight;
                    }
                }

                prop = property.FindPropertyRelative(LoseType);
                EditorGUI.PropertyField(position, prop);
                position.y += lineHeight;

                prop = property.FindPropertyRelative(ObjectiveCategory);
                EditorGUI.PropertyField(position, prop);
                position.y += lineHeight;
                prop = property.FindPropertyRelative(InverseFinishStateInSummary);
                EditorGUI.PropertyField(position, prop);
                position.y += lineHeight;
                prop = property.FindPropertyRelative(Type);
                EditorGUI.PropertyField(position, prop);
                position.y += lineHeight;

                bool show = false;
                switch (data.Type)
                {
                    case EObjectiveType.CompleteObjective:
                    case EObjectiveType.ReachMiles:
                    case EObjectiveType.Time:
                    case EObjectiveType.FinishCustomMission:
                    case EObjectiveType.FinishMissions:
                    case EObjectiveType.RescueSurvivors:
                    case EObjectiveType.ZoomCamera:
                    case EObjectiveType.OpenWindow:
                    case EObjectiveType.UseBuffOrder:
                    case EObjectiveType.ChangeDeckState:
                    case EObjectiveType.DcReachMaintenance:
                    case EObjectiveType.EnemyProximity:
                        show = true;
                        break;
                }
                if (show)
                {
                    prop = property.FindPropertyRelative(NotType);
                    EditorGUI.PropertyField(position, prop, new GUIContent("Inverse objective"));
                    position.y += lineHeight;
                }
                else if (data.Type == EObjectiveType.ReachMapCorner)
                {
                    if (data.MapCornerData == null)
                    {
                        data.MapCornerData = new MapCornerData();
                    }
                    prop = property.FindPropertyRelative(MapCornerData);
                    prop = prop.FindPropertyRelative(CornerOrientation);

                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    var orientation = (EOrientation)prop.enumValueIndex;

                    Vector2 pos = data.MapCornerData.CornerPositional;
                    pos.x += 960f;
                    pos.x = EditorGUI.Slider(position, "Horizontal limit", pos.x, 0, 1920f);
                    position.y += lineHeight;

                    pos.y += 540f;
                    pos.y = EditorGUI.Slider(position, "Vertical limit", pos.y, 0f, 1080f);
                    position.y += lineHeight;

                    pos.x -= 960f;
                    pos.y -= 540f;
                    data.MapCornerData.CornerPositional = pos;

                    if (editor)
                    {
                        var horizontal = MapEditorWindow.MapCornerPointer.anchoredPosition;
                        var vertical = MapEditorWindow.MapCornerPointer2.anchoredPosition;
                        horizontal.x = pos.x;
                        vertical.y = pos.y;
                        switch (orientation)
                        {
                            case EOrientation.NE:
                                horizontal.y = Mathf.Abs(horizontal.y);
                                vertical.x = Mathf.Abs(vertical.x);
                                break;
                            case EOrientation.SE:
                                horizontal.y = -Mathf.Abs(horizontal.y);
                                vertical.x = Mathf.Abs(vertical.x);
                                break;
                            case EOrientation.NW:
                                horizontal.y = Mathf.Abs(horizontal.y);
                                vertical.x = -Mathf.Abs(vertical.x);
                                break;
                            case EOrientation.SW:
                                horizontal.y = -Mathf.Abs(horizontal.y);
                                vertical.x = -Mathf.Abs(vertical.x);
                                break;
                        }

                        MapEditorWindow.MapCornerPointer.anchoredPosition = horizontal;
                        MapEditorWindow.MapCornerPointer2.anchoredPosition = vertical;
                    }
                }

                int count = 0;
                if (editor)
                {
                    prop = property.FindPropertyRelative(TargetTranses);
                    if (!test)
                    {
                        position.height = EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                    }
                    bool moved = true;
                    switch (data.Type)
                    {
                        case EObjectiveType.CompleteObjective:
                            EditorGUI.PropertyField(position, prop, new GUIContent("Target objectives"), prop.isExpanded);
                            break;
                        case EObjectiveType.Destroy:
                        case EObjectiveType.DestroyBlock:
                        case EObjectiveType.Reveal:
                        case EObjectiveType.FinishMissions:
                        case EObjectiveType.SendAirstrikeWithoutLosses:
                        case EObjectiveType.RevealBlocks:
                        case EObjectiveType.EnemyProximity:
                            EditorGUI.PropertyField(position, prop, new GUIContent("Target enemies"), prop.isExpanded);
                            break;
                        case EObjectiveType.Reach:
                        case EObjectiveType.SetSpecificCourse:
                            EditorGUI.PropertyField(position, prop, new GUIContent("Target nodes"), prop.isExpanded);
                            break;
                        default:
                            moved = false;
                            break;
                    }
                    if (!test)
                    {
                        if (moved)
                        {
                            position.y += EditorGUIUtility.standardVerticalSpacing;
                            position.y += position.height;
                        }
                        position.height = EditorGUIUtility.singleLineHeight;
                    }
                    count = prop.arraySize;

                    if (data.Type == EObjectiveType.Reach)
                    {
                        EditorGUI.LabelField(position, "Null if player should reach");
                        position.y += lineHeight;

                        prop = property.FindPropertyRelative(SndTargetTrans);
                        EditorGUI.PropertyField(position, prop, new GUIContent("Enemy to reach"));
                        position.y += lineHeight;
                    }
                }
                else
                {
                    prop = property.FindPropertyRelative(Targets);

                    if (!test)
                    {
                        position.height = EditorGUI.GetPropertyHeight(prop, prop.isExpanded);
                    }
                    EditorGUI.PropertyField(position, prop, prop.isExpanded);
                    if (!test)
                    {
                        position.y += position.height;
                        position.height = EditorGUIUtility.singleLineHeight;
                    }

                    count = prop.arraySize;
                    prop = property.FindPropertyRelative(SndTarget);
                    EditorGUI.PropertyField(position, prop, new GUIContent("Enemy to reach"));
                    position.y += lineHeight;
                }

                show = false;
                switch (data.Type)
                {
                    case EObjectiveType.CompleteObjective:
                    case EObjectiveType.Destroy:
                    case EObjectiveType.DestroyBlock:
                    case EObjectiveType.Reveal:
                    case EObjectiveType.FinishMissions:
                    case EObjectiveType.RevealBlocks:
                        show = true;
                        break;
                    case EObjectiveType.Reach:
                        if (!data.Simultaneously)
                        {
                            show = true;
                        }
                        break;
                }

                if (show)
                {
                    prop = property.FindPropertyRelative(TargetType);
                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                }

                RectTransform t = null;
                if (data.Type == EObjectiveType.Reach && data.SecondaryTargetEnemy == null)
                {
                    prop = property.FindPropertyRelative(Range);

                    EditorGUI.LabelField(position, "(Only player)Zero or less if default value");
                    position.y += lineHeight;
                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;

                    foreach (var trans in data.TargetTranses)
                    {
                        if (trans is RectTransform t2)
                        {
                            t = t2;
                            break;
                        }
                    }

                    if (data.Range <= 0f)
                    {
                        t = null;
                    }
                }
                else if (data.Type == EObjectiveType.EnemyProximity)
                {
                    foreach (var trans in data.TargetTranses)
                    {
                        if (trans is RectTransform t2)
                        {
                            t = t2;
                            break;
                        }
                    }
                    if (data.Range <= 0f)
                    {
                        t = null;
                    }
                }
                var range = MapEditorWindow.Range;
                if (editor)
                {
                    if (t == null)
                    {
                        range.gameObject.SetActive(false);
                    }
                    else
                    {
                        range.gameObject.SetActive(true);
                        range.anchoredPosition = t.anchoredPosition;
                        range.sizeDelta = new Vector2(data.Range * 2f, data.Range * 2f);
                    }
                }
                bool show2;
                switch (data.Type)
                {
                    case EObjectiveType.ReachMiles:
                    case EObjectiveType.Time:
                    case EObjectiveType.FinishMissions:
                    case EObjectiveType.RescueSurvivors:
                    case EObjectiveType.DeploySquadrons:
                    case EObjectiveType.DestroyBlockCount:
                    case EObjectiveType.RevealUO:
                        show2 = true;
                        break;
                    default:
                        show2 = show && data.TargetType == EObjectiveTarget.Number;
                        break;
                }

                if (show2)
                {
                    data.TargetType = EObjectiveTarget.Number;
                    prop = property.FindPropertyRelative(Count);
                    EditorGUI.PropertyField(position, prop);
                    position.y += lineHeight;
                    if (!show)
                    {
                        count = 100000;
                    }
                    prop.intValue = Mathf.Min(Mathf.Max(prop.intValue, 1), count);
                }

                switch (data.Type)
                {
                    case EObjectiveType.CompleteObjective:
                        prop = property.FindPropertyRelative(FailOnNegativeTarget);
                        bool prev = EditorStyles.label.wordWrap;

                        if (!test)
                        {
                            position.height = EditorGUIUtility.singleLineHeight;
                        }
                        EditorStyles.label.wordWrap = true;
                        EditorGUI.LabelField(position, "Should fail if not enough objectives left");
                        EditorStyles.label.wordWrap = prev;

                        if (!test)
                        {
                            position.y += lineHeight;
                            position.height = EditorGUIUtility.singleLineHeight;
                        }

                        EditorGUI.PropertyField(position, prop, new GUIContent(" "));
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.DestroyBlock:
                        prop = property.FindPropertyRelative(TargetBlock);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.Reach:
                    case EObjectiveType.Reveal:
                    case EObjectiveType.RevealBlocks:
                        if (data.Type != EObjectiveType.Reach || data.SecondaryTargetEnemy == null)
                        {
                            prop = property.FindPropertyRelative(Simul);
                            EditorGUI.PropertyField(position, prop, new GUIContent(data.Type == EObjectiveType.Reach ? "In order" : Simul));
                            position.y += lineHeight;
                        }
                        break;
                    case EObjectiveType.FinishMissions:
                    case EObjectiveType.MissionAfterAction:
                        prop = property.FindPropertyRelative(MissionType);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.RescueSurvivors:
                        prop = property.FindPropertyRelative(FailOnNegativeTarget);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.UseStrikeGroupActiveSkill:
                        prop = property.FindPropertyRelative(StrikeGroupActiveSkill);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.CameraView:
                        prop = property.FindPropertyRelative(CameraView);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.OfficerReachedRoom:
                        prop = property.FindPropertyRelative(RoomType);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.OpenWindow:
                        prop = property.FindPropertyRelative(WindowType);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.SetCarrierSpeed:
                        prop = property.FindPropertyRelative(IndexValue);
                        EditorGUI.PropertyField(position, prop, new GUIContent("Speed index"));
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.DragCrew:
                        prop = property.FindPropertyRelative(IndexValue);
                        EditorGUI.PropertyField(position, prop, new GUIContent("Crew index"));
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.DeploySquadrons:
                        prop = property.FindPropertyRelative(SquadronType);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.PlanMission:
                    case EObjectiveType.SendMission:
                    case EObjectiveType.HoverMission:
                    case EObjectiveType.StartPlanningMission:
                    case EObjectiveType.OrderSendMission:
                        prop = property.FindPropertyRelative(MissionType);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.ManeuverCategory:
                        prop = property.FindPropertyRelative(ManeuverType);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.EnemyProximity:
                        prop = property.FindPropertyRelative(Range);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.DestroyBlockCount:
                        prop = property.FindPropertyRelative(TargetBlock);
                        EditorGUI.PropertyField(position, prop);
                        position.y += lineHeight;
                        break;
                    case EObjectiveType.RescueSpecificSurvivor:
                        prop = property.FindPropertyRelative(IndexValue);
                        EditorGUI.PropertyField(position, prop, new GUIContent("Survivor index"));
                        position.y += lineHeight;
                        break;
                }

                if (!test)
                {
                    position.height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative(Effects));
                }

                prop = property.FindPropertyRelative(Effects);
                EditorGUI.PropertyField(position, prop, true);

                if (!test)
                {
                    position.y += position.height;
                    position.y += EditorGUIUtility.standardVerticalSpacing;
                    position.height = EditorGUIUtility.singleLineHeight;
                }

                if (editor)
                {
                    t = null;
                    range = MapEditorWindow.Range2;
                    if (data.Effects != null)
                    {
                        foreach (var effect in data.Effects)
                        {
                            if (effect.EffectType == EObjectiveEffect.MakeDamageRange && !effect.NotEffect && effect.TargetTranses.Count > 0 && effect.TargetTranses[0] is RectTransform trans)
                            {
                                t = trans;
                                range.anchoredPosition = trans.anchoredPosition;
                                range.sizeDelta = new Vector2(effect.Range * 2f, effect.Range * 2f);
                            }
                        }
                    }
                    range.gameObject.SetActive(t != null);
                }

                prop = property.FindPropertyRelative(Active);
                EditorGUI.PropertyField(position, prop);
                position.y += lineHeight;

                prop = property.FindPropertyRelative(Visible);
                EditorGUI.PropertyField(position, prop);
                position.y += lineHeight;
                
                prop = property.FindPropertyRelative(InverseComplete);
                EditorGUI.PropertyField(position, prop);
                position.y += lineHeight;

                if (!editor)
                {
                    EditorGUI.indentLevel--;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is ExitGUIException)
            {
                throw;
            }
            if (!ex.Message.Contains("Getting control "))
            {
                Debug.LogException(ex);
            }
        }

        if (end)
        {
            EditorGUI.EndProperty();
        }
    }
}
