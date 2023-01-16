using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

public class KeyBinding : MonoBehaviour
{
    public InputAction Action => action;

    [SerializeField]
    private EKeyBinding binding = default;
    [SerializeField]
    private Toggle keyToggle = null;
    [SerializeField]
    private Text keyText = null;

    private ControlsManager controlsMan;

    private RebindingOperation rebindingOperation;
    private InputAction action;
    private int index;

    public void Setup(ControlsManager controlsManager)
    {
        controlsMan = controlsManager;
        controlsMan.GetInputAction(binding, out action, out index);
        keyToggle.onValueChanged.AddListener(Rebind);
        RefreshText();
    }

    public void RemoveBindingIfDuplicate(string newKey)
    {
        if (action.bindings[index].effectivePath == newKey)
        {
            action.ApplyBindingOverride(index, "");
            RefreshText();
        }
    }

    public void RefreshText()
    {
        var path = InputControlPath.ToHumanReadableString(action.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        var shortPath = InputControlPath.ToHumanReadableString(action.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames | InputControlPath.HumanReadableStringOptions.OmitDevice);
        if (shortPath.Length > 2)
        {
            shortPath.Remove(0);
            shortPath.Remove(shortPath.Length - 1);
        }
        var key = Keyboard.current.FindKeyOnCurrentKeyboardLayout(path);
        keyText.text = key != null ? key.name.ToUpper().Length > 1 ? ControlsManager.KeyToStringDictionary.ContainsKey(key.keyCode) ? ControlsManager.KeyToStringDictionary[key.keyCode] : key.name.ToUpper() : key.name.ToUpper() : shortPath;
    }

    public void RebindComplete(RebindingOperation o)
    {
        o.Dispose();
        RefreshText();
        keyToggle.SetIsOnWithoutNotify(false);
        action.Enable();
        controlsMan.BindingChanged();
        controlsMan.RemoveDuplicates(action.bindings[index].effectivePath, this);
    }

    public void RebindCanceled(RebindingOperation o)
    {
        keyToggle.SetIsOnWithoutNotify(false);
        action.Enable();
    }

    private void Rebind(bool value)
    {
        if (value)
        {
            action.Disable();
            rebindingOperation = action.PerformInteractiveRebinding(index)/*.WithControlsExcluding("Mouse")*/.OnMatchWaitForAnother(0.1f).OnComplete(RebindComplete).WithCancelingThrough("<Keyboard>/escape").Start();
            rebindingOperation.OnCancel(RebindCanceled);
        }
        else
        {

        }
    }
}
