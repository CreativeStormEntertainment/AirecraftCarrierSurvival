using System;

[Serializable]
public struct TacticalObjectData
{
    public MyVector2 Position;
    public MyVector2 Direction;
    public ETacticalObjectType Type;
    public float MovementSpeed;

    public bool Revealed;
    public MyVector2 MarkerPosition;
    public float MarkerDirectionZ;
    public int TooltipTimer;
    public float UpdateTimer;
    public int HideTimer;
}
