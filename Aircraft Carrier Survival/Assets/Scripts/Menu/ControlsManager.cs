using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour
{
    public static Dictionary<Key, string> KeyToStringDictionary = new Dictionary<Key, string>()
    {
        { Key.Backquote, "`" },
        { Key.Backslash, "\\" },
        { Key.Comma, "," },
        { Key.Equals, "=" },
        { Key.Minus, "-" },
        { Key.NumpadDivide, "/" },
        { Key.NumpadEquals, "=" },
        { Key.NumpadMinus, "-" },
        { Key.NumpadMultiply, "*" },
        { Key.NumpadPeriod, "," },
        { Key.NumpadPlus, "+" },
        { Key.Period, "." },
        { Key.Quote,  "\""},
        { Key.Slash, "/" },
        { Key.Semicolon, ";" },
        { Key.LeftBracket, "[" },
        { Key.RightBracket, "]" },
        { Key.LeftArrow, "<" },
        { Key.RightArrow, ">" },
    };

    [SerializeField]
    private List<KeyBinding> keyBindingObjects = null;
    [SerializeField]
    private Button resetButton = null;
    [SerializeField]
    private Button saveButton = null;
    [SerializeField]
    private StartPersistentData basicPersistantData = null;
    [SerializeField]
    private ScrollRect scrollRect = null;

    private BasicInput input;

    private void Awake()
    {
        input = BasicInput.Instance;
        foreach (var obj in keyBindingObjects)
        {
            obj.Setup(this);
        }
        resetButton.onClick.AddListener(ResetBindings);
        saveButton.interactable = false;
        saveButton.onClick.AddListener(SaveBindings);
    }

    public void OnEnable()
    {
        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void BindingChanged()
    {
        saveButton.interactable = true;
    }

    public void GetInputAction(EKeyBinding key, out InputAction action, out int index)
    {
        action = null;
        index = -1;
        switch (key)
        {
            case EKeyBinding.MovementUp:
                action = input.Camera.VerticalMovement;
                index = 1;
                break;
            case EKeyBinding.MovementDown:
                action = input.Camera.VerticalMovement;
                index = 2;
                break;
            case EKeyBinding.MovementLeft:
                action = input.Camera.HorizontalMovement;
                index = 1;
                break;
            case EKeyBinding.MovementRight:
                action = input.Camera.HorizontalMovement;
                index = 2;
                break;
            case EKeyBinding.AltMovementUp:
                action = input.Camera.VerticalMovement;
                index = 4;
                break;
            case EKeyBinding.AltMovementDown:
                action = input.Camera.VerticalMovement;
                index = 5;
                break;
            case EKeyBinding.AltMovementLeft:
                action = input.Camera.HorizontalMovement;
                index = 4;
                break;
            case EKeyBinding.AltMovementRight:
                action = input.Camera.HorizontalMovement;
                index = 5;
                break;
            case EKeyBinding.CameraRotationLeft:
                action = input.Camera.CameraRotation;
                index = 1;
                break;
            case EKeyBinding.CameraRotationRight:
                action = input.Camera.CameraRotation;
                index = 2;
                break;
            case EKeyBinding.AltCameraRotationLeft:
                action = input.Camera.CameraRotation;
                index = 4;
                break;
            case EKeyBinding.AltCameraRotationRight:
                action = input.Camera.CameraRotation;
                index = 5;
                break;
            case EKeyBinding.DeckView:
                action = input.Camera.Deck;
                index = 0;
                break;
            case EKeyBinding.SectionsView:
                action = input.Camera.Sections;
                index = 0;
                break;
            case EKeyBinding.IslandView:
                action = input.Camera.Island;
                index = 0;
                break;
            case EKeyBinding.FreeView:
                action = input.Camera.FreeView;
                index = 0;
                break;
            case EKeyBinding.AltDeckView:
                action = input.Camera.Deck;
                index = 1;
                break;
            case EKeyBinding.AltSectionsView:
                action = input.Camera.Sections;
                index = 1;
                break;
            case EKeyBinding.AltIslandView:
                action = input.Camera.Island;
                index = 1;
                break;
            case EKeyBinding.AltFreeView:
                action = input.Camera.FreeView;
                index = 1;
                break;
            case EKeyBinding.Malfunction:
                action = input.DamageControl.MalfunctionButton;
                index = 0;
                break;
            case EKeyBinding.Deflood:
                action = input.DamageControl.DefloodButton;
                index = 0;
                break;
            case EKeyBinding.AltMalfunction:
                action = input.DamageControl.MalfunctionButton;
                index = 1;
                break;
            case EKeyBinding.AltDeflood:
                action = input.DamageControl.DefloodButton;
                index = 1;
                break;
            case EKeyBinding.PrepareAirstrike:
                action = input.TacticMissions.PrepareAirstrike;
                index = 0;
                break;
            case EKeyBinding.PrepareIdentify:
                action = input.TacticMissions.PrepareIdentify;
                index = 0;
                break;
            case EKeyBinding.PrepareRecon:
                action = input.TacticMissions.PrepareRecon;
                index = 0;
                break;
            case EKeyBinding.AltPrepareAirstrike:
                action = input.TacticMissions.PrepareAirstrike;
                index = 1;
                break;
            case EKeyBinding.AltPrepareIdentify:
                action = input.TacticMissions.PrepareIdentify;
                index = 1;
                break;
            case EKeyBinding.AltPrepareRecon:
                action = input.TacticMissions.PrepareRecon;
                index = 1;
                break;
            case EKeyBinding.Map:
                action = input.UI.Map;
                index = 0;
                break;
            case EKeyBinding.CrewPanel:
                action = input.UI.CrewPanel;
                index = 0;
                break;
            case EKeyBinding.Orders:
                action = input.UI.Orders;
                index = 0;
                break;
            case EKeyBinding.ChangeDeck:
                action = input.UI.ChangeDeck;
                index = 0;
                break;
            case EKeyBinding.TimeSpeedUp:
                action = input.UI.TimeSpeedUp;
                index = 0;
                break;
            case EKeyBinding.TimeSpeedDown:
                action = input.UI.TimeSpeedDown;
                index = 0;
                break;
            case EKeyBinding.AltMap:
                action = input.UI.Map;
                index = 1;
                break;
            case EKeyBinding.AltCrewPanel:
                action = input.UI.CrewPanel;
                index = 1;
                break;
            case EKeyBinding.AltOrders:
                action = input.UI.Orders;
                index = 1;
                break;
            case EKeyBinding.AltChangeDeck:
                action = input.UI.ChangeDeck;
                index = 1;
                break;
            case EKeyBinding.AltTimeSpeedUp:
                action = input.UI.TimeSpeedUp;
                index = 1;
                break;
            case EKeyBinding.AltTimeSpeedDown:
                action = input.UI.TimeSpeedDown;
                index = 1;
                break;
            case EKeyBinding.SetToMalfunction:
                action = input.SetupDC.SetToMalfunction;
                index = 0;
                break;
            case EKeyBinding.SetToFlood:
                action = input.SetupDC.SetToFlood;
                index = 0;
                break;
            case EKeyBinding.SetToMedic:
                action = input.SetupDC.SetToMedic;
                index = 0;
                break;
            case EKeyBinding.SetToFire:
                action = input.SetupDC.SetToFire;
                index = 0;
                break;
            case EKeyBinding.AltSetToMalfunction:
                action = input.SetupDC.SetToMalfunction;
                index = 1;
                break;
            case EKeyBinding.AltSetToFlood:
                action = input.SetupDC.SetToFlood;
                index = 1;
                break;
            case EKeyBinding.AltSetToMedic:
                action = input.SetupDC.SetToMedic;
                index = 1;
                break;
            case EKeyBinding.AltSetToFire:
                action = input.SetupDC.SetToFire;
                index = 1;
                break;
            case EKeyBinding.PrepareFighter:
                action = input.PreparePlanes.PrepareFighter;
                index = 0;
                break;
            case EKeyBinding.PrepareBomber:
                action = input.PreparePlanes.PrepareBomber;
                index = 0;
                break;
            case EKeyBinding.PrepareTorpedo:
                action = input.PreparePlanes.PrepareTorpedo;
                index = 0;
                break;
            case EKeyBinding.AltPrepareFighter:
                action = input.PreparePlanes.PrepareFighter;
                index = 1;
                break;
            case EKeyBinding.AltPrepareBomber:
                action = input.PreparePlanes.PrepareBomber;
                index = 1;
                break;
            case EKeyBinding.AltPrepareTorpedo:
                action = input.PreparePlanes.PrepareTorpedo;
                index = 1;
                break;
            case EKeyBinding.DeadSlow:
                action = input.CarrierSpeed.DeadSlow;
                index = 0;
                break;
            case EKeyBinding.Slow:
                action = input.CarrierSpeed.Slow;
                index = 0;
                break;
            case EKeyBinding.Half:
                action = input.CarrierSpeed.Half;
                index = 0;
                break;
            case EKeyBinding.Full:
                action = input.CarrierSpeed.Full;
                index = 0;
                break;
            case EKeyBinding.Stop:
                action = input.CarrierSpeed.Stop;
                index = 0;
                break;
            case EKeyBinding.AltDeadSlow:
                action = input.CarrierSpeed.DeadSlow;
                index = 1;
                break;
            case EKeyBinding.AltSlow:
                action = input.CarrierSpeed.Slow;
                index = 1;
                break;
            case EKeyBinding.AltHalf:
                action = input.CarrierSpeed.Half;
                index = 1;
                break;
            case EKeyBinding.AltFull:
                action = input.CarrierSpeed.Full;
                index = 1;
                break;
            case EKeyBinding.AltStop:
                action = input.CarrierSpeed.Stop;
                index = 1;
                break;
            case EKeyBinding.Save:
                action = input.QuickSave.QuickSave;
                index = 0;
                break;
            case EKeyBinding.Load:
                action = input.QuickSave.QuickLoad;
                index = 0;
                break;
            case EKeyBinding.AltSave:
                action = input.QuickSave.QuickSave;
                index = 1;
                break;
            case EKeyBinding.AltLoad:
                action = input.QuickSave.QuickLoad;
                index = 1;
                break;
        }
        Assert.IsTrue(action != null && index != -1);
    }

    public void RemoveDuplicates(string newKey, KeyBinding newBinding)
    {
        foreach (var binding in keyBindingObjects)
        {
            if (binding != newBinding)
            {
                binding.RemoveBindingIfDuplicate(newKey);
            }
        }
    }

    private void ResetBindings()
    {
        BindingChanged();
        input.Load(ref basicPersistantData.Data.InputData);
        foreach (var obj in keyBindingObjects)
        {
            obj.RefreshText();
        }
    }

    private void SaveBindings()
    {
        input.Save(ref SaveManager.Instance.PersistentData.InputData);
        saveButton.interactable = false;
    }
}
