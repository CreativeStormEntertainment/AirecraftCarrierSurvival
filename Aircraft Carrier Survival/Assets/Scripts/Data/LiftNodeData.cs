using UnityEngine;

public class LiftNodeData : MonoBehaviour
{
    public int Lift;
    public bool Up;
    public LiftNodeData WaitForNodeData;

    public PlaneNode WaitForNode
    {
        get;
        set;
    }
}
