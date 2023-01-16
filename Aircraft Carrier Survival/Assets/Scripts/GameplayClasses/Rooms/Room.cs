using UnityEngine;

public abstract class Room : MonoBehaviour
{
    public string RoomName;
    [SerializeField]
    protected bool isActive;
    //public virtual bool IsActive { get => isActive; set => isActive = value; }
}
