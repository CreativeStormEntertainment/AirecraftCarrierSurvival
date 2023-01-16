using FMODUnity;
using UnityEngine;

public class DirectorSoundRef : MonoBehaviour
{
    public StudioEventEmitter Emitter;

    private void Awake()
    {
        if (Emitter == null)
        {
            Debug.LogError("Empty emitter", this);
        }
    }
}
