using UnityEngine;

public class ToggleObjectSounds : MonoBehaviour
{
    [SerializeField]
    private EMainSceneUI onEnableSound = EMainSceneUI.ShowDepartaments;
    [SerializeField]
    private EMainSceneUI onDisableSound = EMainSceneUI.HideDepartaments;

    private void OnEnable()
    {
        BackgroundAudio.Instance.PlayEvent(onEnableSound);
    }

    private void OnDisable()
    {
        BackgroundAudio.Instance.PlayEvent(onDisableSound);
    }
}
