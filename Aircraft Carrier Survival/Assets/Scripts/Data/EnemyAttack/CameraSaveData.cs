using System;

[Serializable]
public struct CameraSaveData
{
    public ECameraView CurrentView;
    public MyVector3 CurrentCameraPos;
    public MyVector2 Axes;
    public float Scroll;
    public MyVector3 Position;
    public MyVector4 Rotation;
    public MyVector4 MainCameraRotation;

    public MyVector3 TargetPosition;
}
