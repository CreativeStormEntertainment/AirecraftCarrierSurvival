using UnityEngine;

public class InjuredIcon : SingleIcon
{
    public Color ToInjureColor = Color.yellow;
    public Color ToDeadColor = Color.red;

    public void SetStatus(bool toInjure)
    {
        IconRenderer.material.SetColor("_Color", toInjure ? ToInjureColor : ToDeadColor);
    }
}
