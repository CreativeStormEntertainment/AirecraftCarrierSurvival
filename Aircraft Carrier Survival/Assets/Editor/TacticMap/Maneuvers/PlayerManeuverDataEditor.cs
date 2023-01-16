using UnityEditor;

[CustomEditor(typeof(PlayerManeuverData))]
public class PlayerManeuverDataEditor : Editor
{
    private PlayerManeuverData data;

    public override void OnInspectorGUI()
    {
        if (data.Modifiers != null)
        {
            foreach (var modifier in data.Modifiers)
            {
                modifier.IsPlayer = true;
            }
        }
        DrawDefaultInspector();
    }

    private void OnEnable()
    {
        data = target as PlayerManeuverData;
    }
}
