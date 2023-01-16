using System;
using System.Collections.Generic;

[Serializable]
public struct PlaneMovementSaveData
{
    public bool Movement;
    public EPlaneDirection MovementType;
    public EPlaneMoveStage MovementStage;
    public List<int> Params;
    public bool FinishMovement;
    public List<PlanePathSaveData> Planes;

    public PlaneMovementSaveData Duplicate()
    {
        var result = this;

        result.Params = new List<int>(Params);
        result.Planes = new List<PlanePathSaveData>();
        for (int i = 0; i < Planes.Count; i++)
        {
            result.Planes.Add(Planes[i].Duplicate());
        }

        return result;
    }
}
