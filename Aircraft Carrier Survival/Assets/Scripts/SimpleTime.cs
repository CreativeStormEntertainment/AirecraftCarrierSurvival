using System;
using UnityEngine;
using UnityEngine.AzureSky;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadonlyAttribute))]
public class ReadonlyPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        try
        {
            bool enabled = GUI.enabled;
            try
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(property);
            }
            catch (Exception ex)
            {
                if (ex is ExitGUIException)
                {
                    throw;
                }
            }
            GUI.enabled = enabled;
        }
        catch(Exception ex)
        {
            if (ex is ExitGUIException)
            {
                throw;
            }
        }
    }
}
#endif

public class ReadonlyAttribute : PropertyAttribute
{

}

public class SimpleTime : MonoBehaviour
{
    [SerializeField]
    private int startHours = 8;

    [SerializeField]
    private int minutePerSecond = 10;

    [SerializeField]
    private AzureTimeController controller = null;

    [SerializeField]
    [Readonly]
    private int _hours;
    [SerializeField]
    [Readonly]
    private int _minutes;

    private double hours;

    private void Awake()
    {
        hours = startHours;
    }

    private void Update()
    {
        hours += ((double)Time.deltaTime) * minutePerSecond * 1d / 60d;
        while (hours >= 24d)
        {
            hours -= 24d;
        }
        _hours = (int)hours;
        _minutes = (int)((hours - _hours) * 60d);
        controller.SetTimeline((float)hours);
    }
}
