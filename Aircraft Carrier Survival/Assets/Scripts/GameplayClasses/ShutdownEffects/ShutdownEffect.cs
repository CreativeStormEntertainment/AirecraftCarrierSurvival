using UnityEngine;

[RequireComponent(typeof(SectionRoom))]
public abstract class ShutdownEffect : MonoBehaviour
{
    protected SectionRoom room;

    private void Awake()
    {
        room = GetComponent<SectionRoom>();
    }

    private void Start()
    {
        room.SectionWorkingChanged += OnSectionWorkingChanged;
    }

    protected abstract void OnSectionWorkingChanged(bool __);
}
