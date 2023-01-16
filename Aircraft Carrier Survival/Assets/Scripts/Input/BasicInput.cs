using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BasicInput : IDisposable
{
    public static BasicInput Instance => _Instance ?? (_Instance = new BasicInput());
    private static BasicInput _Instance;

    public struct CameraActions
    {
        public InputAction HorizontalMovement => wrapper.camera_HorizontalMovement;
        public InputAction VerticalMovement => wrapper.camera_VerticalMovement;
        public InputAction @CameraRotation => wrapper.camera_CameraRotation;
        public InputAction Deck => wrapper.camera_Deck;
        public InputAction Sections => wrapper.camera_Sections;
        public InputAction Island => wrapper.camera_Island;
        public InputAction FreeView => wrapper.camera_FreeView;

        private BasicInput wrapper;

        public CameraActions(BasicInput wrapper)
        {
            this.wrapper = wrapper;
        }

        public void Enable()
        {
            Get().Enable();
        }

        public void Disable()
        {
            Get().Disable();
        }

        public InputActionMap Get()
        {
            return wrapper.camera;
        }
    }
    public CameraActions Camera => new CameraActions(this);

    public struct DamageControlActions
    {
        public InputAction MalfunctionButton => wrapper.damageControl_MalfunctionButton;
        public InputAction DefloodButton => wrapper.damageControl_DefloodButton;

        private BasicInput wrapper;

        public DamageControlActions(BasicInput wrapper)
        {
            this.wrapper = wrapper;
        }

        public InputActionMap Get()
        {
            return wrapper.damageControl;
        }

        public void Enable()
        {
            Get().Enable();
        }

        public void Disable()
        {
            Get().Disable();
        }
    }
    public DamageControlActions DamageControl => new DamageControlActions(this);

    public struct TacticMissionsActions
    {
        public InputAction PrepareAirstrike => wrapper.tacticMissions_PrepareAirstrike;
        public InputAction PrepareIdentify => wrapper.tacticMissions_PrepareIdentify;
        public InputAction PrepareRecon => wrapper.tacticMissions_PrepareRecon;

        private BasicInput wrapper;

        public TacticMissionsActions(BasicInput wrapper)
        {
            this.wrapper = wrapper;
        }

        public InputActionMap Get()
        {
            return wrapper.tacticMissions;
        }

        public void Enable()
        {
            Get().Enable();
        }

        public void Disable()
        {
            Get().Disable();
        }
    }
    public TacticMissionsActions TacticMissions => new TacticMissionsActions(this);

    public struct UIActions
    {
        public InputAction Map => wrapper.ui_Map;
        public InputAction CrewPanel => wrapper.ui_CrewPanel;
        public InputAction Orders => wrapper.ui_Orders;
        public InputAction ChangeDeck => wrapper.ui_ChangeDeck;
        public InputAction TimeSpeedUp => wrapper.ui_TimeSpeedUp;
        public InputAction TimeSpeedDown => wrapper.ui_TimeSpeedDown;

        private BasicInput wrapper;

        public UIActions(BasicInput wrapper)
        {
            this.wrapper = wrapper;
        }

        public InputActionMap Get()
        {
            return wrapper.ui;
        }

        public void Enable()
        {
            Get().Enable();
        }

        public void Disable()
        {
            Get().Disable();
        }
    }
    public UIActions UI => new UIActions(this);

    public struct SetupDCActions
    {
        public InputAction SetToFire => wrapper.setupDC_SetToFire;
        public InputAction SetToMalfunction => wrapper.setupDC_SetToMalfunction;
        public InputAction SetToFlood => wrapper.setupDC_SetToFlood;
        public InputAction SetToMedic => wrapper.setupDC_SetToMedic;
        
        private @BasicInput wrapper;

        public SetupDCActions(BasicInput wrapper)
        {
            this.wrapper = wrapper;
        }


        public InputActionMap Get()
        {
            return wrapper.setupDC;
        }

        public void Enable()
        {
            Get().Enable();
        }

        public void Disable()
        {
            Get().Disable();
        }
    }
    public SetupDCActions SetupDC => new SetupDCActions(this);

    public struct PreparePlanesActions
    {
        public InputAction PrepareFighter => wrapper.preparePlanes_PrepareFighter;
        public InputAction PrepareBomber => wrapper.preparePlanes_PrepareBomber;
        public InputAction PrepareTorpedo => wrapper.preparePlanes_PrepareTorpedo;

        private @BasicInput wrapper;

        public PreparePlanesActions(BasicInput wrapper)
        {
            this.wrapper = wrapper;
        }

        public InputActionMap Get()
        {
            return wrapper.preparePlanes;
        }

        public void Enable()
        {
            Get().Enable();
        }

        public void Disable()
        {
            Get().Disable();
        }
    }
    public PreparePlanesActions PreparePlanes => new PreparePlanesActions(this);

    public struct CarrierSpeedActions
    {
        public InputAction DeadSlow => wrapper.carrierSpeed_DeadSlow;
        public InputAction Slow => wrapper.carrierSpeed_Slow;
        public InputAction Half => wrapper.carrierSpeed_Half;
        public InputAction Full => wrapper.carrierSpeed_Full;
        public InputAction Stop => wrapper.carrierSpeed_Stop;

        private @BasicInput wrapper;

        public CarrierSpeedActions(BasicInput wrapper)
        {
            this.wrapper = wrapper;
        }

        public InputActionMap Get()
        {
            return wrapper.carrierSpeed;
        }

        public void Enable()
        {
            Get().Enable();
        }

        public void Disable()
        {
            Get().Disable();
        }
    }
    public CarrierSpeedActions CarrierSpeed => new CarrierSpeedActions(this);
    
    public struct SaveActions
    {
        public InputAction QuickSave => wrapper.save_QuickSave;
        public InputAction QuickLoad => wrapper.save_QuickLoad;

        private BasicInput wrapper;

        public SaveActions(BasicInput wrapper)
        {
            this.wrapper = wrapper;
        }

        public InputActionMap Get()
        {
            return wrapper.save;
        }

        public void Enable()
        {
            Get().Enable();
        }

        public void Disable()
        {
            Get().Disable();
        }
    }
    public SaveActions QuickSave => new SaveActions(this);

    public bool Enabled => asset.enabled;

    public bool WasLoaded
    {
        get;
        private set;
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public void Save(ref InputSaveData data)
    {
        data.MovementLeft = GetBindingPath(camera_HorizontalMovement, 1);
        data.MovementRight = GetBindingPath(camera_HorizontalMovement, 2);

        data.AltMovementLeft = GetBindingPath(camera_HorizontalMovement, 4);
        data.AltMovementRight = GetBindingPath(camera_HorizontalMovement, 5);

        data.MovementUp = GetBindingPath(camera_VerticalMovement, 1);
        data.MovementDown = GetBindingPath(camera_VerticalMovement, 2);

        data.AltMovementUp = GetBindingPath(camera_VerticalMovement, 4);
        data.AltMovementDown = GetBindingPath(camera_VerticalMovement, 5);

        data.CameraRotationLeft = GetBindingPath(camera_CameraRotation, 1);
        data.CameraRotationRight = GetBindingPath(camera_CameraRotation, 2);

        data.AltCameraRotationLeft = GetBindingPath(camera_CameraRotation, 4);
        data.AltCameraRotationRight = GetBindingPath(camera_CameraRotation, 5);

        data.DeckView = GetBindingPath(camera_Deck, 0);
        data.SectionsView = GetBindingPath(camera_Sections, 0);
        data.IslandView = GetBindingPath(camera_Island, 0);
        data.FreeView = GetBindingPath(camera_FreeView, 0);

        data.AltDeckView = GetBindingPath(camera_Deck, 1);
        data.AltSectionsView = GetBindingPath(camera_Sections, 1);
        data.AltIslandView = GetBindingPath(camera_Island, 1);
        data.AltFreeView = GetBindingPath(camera_FreeView, 1);

        data.Malfunction = GetBindingPath(damageControl_MalfunctionButton, 0);
        data.Deflood = GetBindingPath(damageControl_DefloodButton, 0);

        data.AltMalfunction = GetBindingPath(damageControl_MalfunctionButton, 1);
        data.AltDeflood = GetBindingPath(damageControl_DefloodButton, 1);

        data.PrepareAirstrike = GetBindingPath(tacticMissions_PrepareAirstrike, 0);
        data.PrepareIdentify = GetBindingPath(tacticMissions_PrepareIdentify, 0);
        data.PrepareRecon = GetBindingPath(tacticMissions_PrepareRecon, 0);

        data.AltPrepareAirstrike = GetBindingPath(tacticMissions_PrepareAirstrike, 1);
        data.AltPrepareIdentify = GetBindingPath(tacticMissions_PrepareIdentify, 1);
        data.AltPrepareRecon = GetBindingPath(tacticMissions_PrepareRecon, 1);

        data.Map = GetBindingPath(ui_Map, 0);
        data.CrewPanel = GetBindingPath(ui_CrewPanel, 0);
        data.Orders = GetBindingPath(ui_Orders, 0);
        data.ChangeDeck = GetBindingPath(ui_ChangeDeck, 0);
        data.TimeSpeedUp = GetBindingPath(ui_TimeSpeedUp, 0);
        data.TimeSpeedDown = GetBindingPath(ui_TimeSpeedDown, 0);

        data.AltMap = GetBindingPath(ui_Map, 1);
        data.AltCrewPanel = GetBindingPath(ui_CrewPanel, 1);
        data.AltOrders = GetBindingPath(ui_Orders, 1);
        data.AltChangeDeck = GetBindingPath(ui_ChangeDeck, 1);
        data.AltTimeSpeedUp = GetBindingPath(ui_TimeSpeedUp, 1);
        data.AltTimeSpeedDown = GetBindingPath(ui_TimeSpeedDown, 1);

        data.SetToFire = GetBindingPath(setupDC_SetToFire, 0);
        data.SetToMalfunction = GetBindingPath(setupDC_SetToMalfunction, 0);
        data.SetToFlood = GetBindingPath(setupDC_SetToFlood, 0);
        data.SetToMedic = GetBindingPath(setupDC_SetToMedic, 0);

        data.AltSetToFire = GetBindingPath(setupDC_SetToFire, 1);
        data.AltSetToMalfunction = GetBindingPath(setupDC_SetToMalfunction, 1);
        data.AltSetToFlood = GetBindingPath(setupDC_SetToFlood, 1);
        data.AltSetToMedic = GetBindingPath(setupDC_SetToMedic, 1);

        data.PrepareFighter = GetBindingPath(preparePlanes_PrepareFighter, 0);
        data.PrepareBomber = GetBindingPath(preparePlanes_PrepareBomber, 0);
        data.PrepareTorpedo = GetBindingPath(preparePlanes_PrepareTorpedo, 0);

        data.AltPrepareFighter = GetBindingPath(preparePlanes_PrepareFighter, 1);
        data.AltPrepareBomber = GetBindingPath(preparePlanes_PrepareBomber, 1);
        data.AltPrepareTorpedo = GetBindingPath(preparePlanes_PrepareTorpedo, 1);

        data.DeadSlow = GetBindingPath(carrierSpeed_DeadSlow, 0);
        data.Slow = GetBindingPath(carrierSpeed_Slow, 0);
        data.Half = GetBindingPath(carrierSpeed_Half, 0);
        data.Full = GetBindingPath(carrierSpeed_Full, 0);
        data.Stop = GetBindingPath(carrierSpeed_Stop, 0);

        data.AltDeadSlow = GetBindingPath(carrierSpeed_DeadSlow, 1);
        data.AltSlow = GetBindingPath(carrierSpeed_Slow, 1);
        data.AltHalf = GetBindingPath(carrierSpeed_Half, 1);
        data.AltFull = GetBindingPath(carrierSpeed_Full, 1);
        data.AltStop = GetBindingPath(carrierSpeed_Stop, 1);

        data.Save = GetBindingPath(save_QuickSave, 0);
        data.Load = GetBindingPath(save_QuickLoad, 0);

        data.AltSave = GetBindingPath(save_QuickSave, 1);
        data.AltLoad = GetBindingPath(save_QuickLoad, 1);
    }

    public void Load(ref InputSaveData data)
    {
        WasLoaded = true;

        Camera.Disable();

        ApplyBindingOverride(camera_HorizontalMovement, 1, data.MovementLeft);
        ApplyBindingOverride(camera_HorizontalMovement, 2, data.MovementRight);

        ApplyBindingOverride(camera_HorizontalMovement, 4, data.AltMovementLeft);
        ApplyBindingOverride(camera_HorizontalMovement, 5, data.AltMovementRight);

        ApplyBindingOverride(camera_VerticalMovement, 1, data.MovementUp);
        ApplyBindingOverride(camera_VerticalMovement, 2, data.MovementDown);

        ApplyBindingOverride(camera_VerticalMovement, 4, data.AltMovementUp);
        ApplyBindingOverride(camera_VerticalMovement, 5, data.AltMovementDown);

        ApplyBindingOverride(camera_Deck, 0, data.DeckView);
        ApplyBindingOverride(camera_Sections, 0, data.SectionsView);
        ApplyBindingOverride(camera_Island, 0, data.IslandView);
        ApplyBindingOverride(camera_FreeView, 0, data.FreeView);

        ApplyBindingOverride(camera_Deck, 1, data.AltDeckView);
        ApplyBindingOverride(camera_Sections, 1, data.AltSectionsView);
        ApplyBindingOverride(camera_Island, 1, data.AltIslandView);
        ApplyBindingOverride(camera_FreeView, 1, data.AltFreeView);

        Camera.Enable();

        DamageControl.Disable();

        ApplyBindingOverride(damageControl_MalfunctionButton, 0, data.Malfunction);
        ApplyBindingOverride(damageControl_DefloodButton, 0, data.Deflood);

        ApplyBindingOverride(damageControl_MalfunctionButton, 1, data.AltMalfunction);
        ApplyBindingOverride(damageControl_DefloodButton, 1, data.AltDeflood);

        DamageControl.Enable();

        TacticMissions.Disable();

        ApplyBindingOverride(tacticMissions_PrepareAirstrike, 0, data.PrepareAirstrike);
        ApplyBindingOverride(tacticMissions_PrepareIdentify, 0, data.PrepareIdentify);
        ApplyBindingOverride(tacticMissions_PrepareRecon, 0, data.PrepareRecon);

        ApplyBindingOverride(tacticMissions_PrepareAirstrike, 1, data.AltPrepareAirstrike);
        ApplyBindingOverride(tacticMissions_PrepareIdentify, 1, data.AltPrepareIdentify);
        ApplyBindingOverride(tacticMissions_PrepareRecon, 1, data.AltPrepareRecon);

        TacticMissions.Enable();

        UI.Disable();

        ApplyBindingOverride(ui_Map, 0, data.Map);
        ApplyBindingOverride(ui_CrewPanel, 0, data.CrewPanel);
        ApplyBindingOverride(ui_Orders, 0, data.Orders);
        ApplyBindingOverride(ui_ChangeDeck, 0, data.ChangeDeck);
        ApplyBindingOverride(ui_TimeSpeedUp, 0, data.TimeSpeedUp);
        ApplyBindingOverride(ui_TimeSpeedDown, 0, data.TimeSpeedDown);

        ApplyBindingOverride(ui_Map, 1, data.AltMap);
        ApplyBindingOverride(ui_CrewPanel, 1, data.AltCrewPanel);
        ApplyBindingOverride(ui_Orders, 1, data.AltOrders);
        ApplyBindingOverride(ui_ChangeDeck, 1, data.AltChangeDeck);
        ApplyBindingOverride(ui_TimeSpeedUp, 1, data.AltTimeSpeedUp);
        ApplyBindingOverride(ui_TimeSpeedDown, 1, data.AltTimeSpeedDown);

        UI.Enable();

        SetupDC.Disable();

        ApplyBindingOverride(setupDC_SetToFire, 0, data.SetToFire);
        ApplyBindingOverride(setupDC_SetToMalfunction, 0, data.SetToMalfunction);
        ApplyBindingOverride(setupDC_SetToFlood, 0, data.SetToFlood);
        ApplyBindingOverride(setupDC_SetToMedic, 0, data.SetToMedic);

        ApplyBindingOverride(setupDC_SetToFire, 1, data.AltSetToFire);
        ApplyBindingOverride(setupDC_SetToMalfunction, 1, data.AltSetToMalfunction);
        ApplyBindingOverride(setupDC_SetToFlood, 1, data.AltSetToFlood);
        ApplyBindingOverride(setupDC_SetToMedic, 1, data.AltSetToMedic);

        SetupDC.Enable();

        PreparePlanes.Disable();

        ApplyBindingOverride(preparePlanes_PrepareFighter, 0, data.PrepareFighter);
        ApplyBindingOverride(preparePlanes_PrepareBomber, 0, data.PrepareBomber);
        ApplyBindingOverride(preparePlanes_PrepareTorpedo, 0, data.PrepareTorpedo);

        ApplyBindingOverride(preparePlanes_PrepareFighter, 1, data.AltPrepareFighter);
        ApplyBindingOverride(preparePlanes_PrepareBomber, 1, data.AltPrepareBomber);
        ApplyBindingOverride(preparePlanes_PrepareTorpedo, 1, data.AltPrepareTorpedo);

        PreparePlanes.Enable();

        CarrierSpeed.Disable();

        ApplyBindingOverride(carrierSpeed_DeadSlow, 0, data.DeadSlow);
        ApplyBindingOverride(carrierSpeed_Slow, 0, data.Slow);
        ApplyBindingOverride(carrierSpeed_Half, 0, data.Half);
        ApplyBindingOverride(carrierSpeed_Full, 0, data.Full);
        ApplyBindingOverride(carrierSpeed_Stop, 0, data.Stop);

        ApplyBindingOverride(carrierSpeed_DeadSlow, 1, data.AltDeadSlow);
        ApplyBindingOverride(carrierSpeed_Slow, 1, data.AltSlow);
        ApplyBindingOverride(carrierSpeed_Half, 1, data.AltHalf);
        ApplyBindingOverride(carrierSpeed_Full, 1, data.AltFull);
        ApplyBindingOverride(carrierSpeed_Stop, 1, data.AltStop);

        CarrierSpeed.Enable();

        QuickSave.Disable();

        ApplyBindingOverride(save_QuickSave, 0, data.Save);
        ApplyBindingOverride(save_QuickLoad, 0, data.Load);

        ApplyBindingOverride(save_QuickSave, 1, data.AltSave);
        ApplyBindingOverride(save_QuickLoad, 1, data.AltLoad);

        QuickSave.Enable();
    }

    public void GetControlsWithout(List<string> controls, InputAction action, int index)
    {
        controls.Add(GetBindingPath(camera_HorizontalMovement, 1));
        controls.Add(GetBindingPath(camera_HorizontalMovement, 2));

        controls.Add(GetBindingPath(camera_HorizontalMovement, 4));
        controls.Add(GetBindingPath(camera_HorizontalMovement, 5));

        controls.Add(GetBindingPath(camera_VerticalMovement, 1));
        controls.Add(GetBindingPath(camera_VerticalMovement, 2));

        controls.Add(GetBindingPath(camera_VerticalMovement, 4));
        controls.Add(GetBindingPath(camera_VerticalMovement, 5));

        controls.Add(GetBindingPath(camera_Deck, 0));
        controls.Add(GetBindingPath(camera_Sections, 0));
        controls.Add(GetBindingPath(camera_Island, 0));
        controls.Add(GetBindingPath(camera_FreeView, 0));

        controls.Add(GetBindingPath(camera_Deck, 1));
        controls.Add(GetBindingPath(camera_Sections, 1));
        controls.Add(GetBindingPath(camera_Island,1));
        controls.Add(GetBindingPath(camera_FreeView, 1));

        controls.Add(GetBindingPath(damageControl_MalfunctionButton, 0));
        controls.Add(GetBindingPath(damageControl_DefloodButton, 0));

        controls.Add(GetBindingPath(damageControl_MalfunctionButton, 1));
        controls.Add(GetBindingPath(damageControl_DefloodButton, 1));

        controls.Add(GetBindingPath(tacticMissions_PrepareAirstrike, 0));
        controls.Add(GetBindingPath(tacticMissions_PrepareIdentify, 0));
        controls.Add(GetBindingPath(tacticMissions_PrepareRecon, 0));

        controls.Add(GetBindingPath(tacticMissions_PrepareAirstrike, 1));
        controls.Add(GetBindingPath(tacticMissions_PrepareIdentify, 1));
        controls.Add(GetBindingPath(tacticMissions_PrepareRecon, 1));

        controls.Add(GetBindingPath(ui_Map, 0));
        controls.Add(GetBindingPath(ui_CrewPanel, 0));
        controls.Add(GetBindingPath(ui_Orders, 0));
        controls.Add(GetBindingPath(ui_ChangeDeck, 0));
        controls.Add(GetBindingPath(ui_TimeSpeedUp, 0));
        controls.Add(GetBindingPath(ui_TimeSpeedDown, 0));

        controls.Add(GetBindingPath(ui_Map, 1));
        controls.Add(GetBindingPath(ui_CrewPanel, 1));
        controls.Add(GetBindingPath(ui_Orders, 1));
        controls.Add(GetBindingPath(ui_ChangeDeck, 1));
        controls.Add(GetBindingPath(ui_TimeSpeedUp, 1));
        controls.Add(GetBindingPath(ui_TimeSpeedDown, 1));

        controls.Add(GetBindingPath(setupDC_SetToFire, 0));
        controls.Add(GetBindingPath(setupDC_SetToMalfunction, 0));
        controls.Add(GetBindingPath(setupDC_SetToFlood, 0));
        controls.Add(GetBindingPath(setupDC_SetToMedic, 0));

        controls.Add(GetBindingPath(setupDC_SetToFire, 1));
        controls.Add(GetBindingPath(setupDC_SetToMalfunction, 1));
        controls.Add(GetBindingPath(setupDC_SetToFlood, 1));
        controls.Add(GetBindingPath(setupDC_SetToMedic, 1));

        controls.Add(GetBindingPath(preparePlanes_PrepareFighter, 0));
        controls.Add(GetBindingPath(preparePlanes_PrepareBomber, 0));
        controls.Add(GetBindingPath(preparePlanes_PrepareTorpedo, 0));

        controls.Add(GetBindingPath(preparePlanes_PrepareFighter, 1));
        controls.Add(GetBindingPath(preparePlanes_PrepareBomber, 1));
        controls.Add(GetBindingPath(preparePlanes_PrepareTorpedo, 1));

        controls.Add(GetBindingPath(carrierSpeed_DeadSlow, 0));
        controls.Add(GetBindingPath(carrierSpeed_Slow, 0));
        controls.Add(GetBindingPath(carrierSpeed_Half, 0));
        controls.Add(GetBindingPath(carrierSpeed_Full, 0));
        controls.Add(GetBindingPath(carrierSpeed_Stop, 0));

        controls.Add(GetBindingPath(carrierSpeed_DeadSlow, 1));
        controls.Add(GetBindingPath(carrierSpeed_Slow, 1));
        controls.Add(GetBindingPath(carrierSpeed_Half, 1));
        controls.Add(GetBindingPath(carrierSpeed_Full, 1));
        controls.Add(GetBindingPath(carrierSpeed_Stop, 1));

        controls.Add(GetBindingPath(save_QuickSave, 0));
        controls.Add(GetBindingPath(save_QuickLoad, 0));

        controls.Add(GetBindingPath(save_QuickSave, 1));
        controls.Add(GetBindingPath(save_QuickLoad, 1));

        index = controls.IndexOf(GetBindingPath(action, index));
        controls[index] = null;
    }

    private BasicInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""BasicInput"",
    ""maps"": [
        {
            ""name"": ""Camera"",
            ""id"": ""a680adda-3a50-4685-82a2-aae7042fbbbf"",
            ""actions"": [
                {
                    ""name"": ""HorizontalMovement"",
                    ""type"": ""Button"",
                    ""id"": ""26663ad3-06fe-4733-84b5-15954cba4260"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""VerticalMovement"",
                    ""type"": ""Button"",
                    ""id"": ""7cddeab4-6108-430b-813a-0172bb45e849"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Deck"",
                    ""type"": ""Button"",
                    ""id"": ""a229b244-3747-494c-9975-74f1b52164dd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Sections"",
                    ""type"": ""Button"",
                    ""id"": ""a55a0886-9a10-4bd1-9d3a-d25c363c6c7c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Island"",
                    ""type"": ""Button"",
                    ""id"": ""9ee8cdd8-d7b3-40ae-8992-4c61b0955841"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""FreeView"",
                    ""type"": ""Button"",
                    ""id"": ""de2c29a6-97cb-4b67-bb13-f566578745cb"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CameraRotation"",
                    ""type"": ""Button"",
                    ""id"": ""ea49204a-ef04-4ce1-a5ab-a6e506d24002"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2290dcec-cec8-4e0b-8a3c-361ac31ca55f"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Deck"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fcb98d4a-42a2-4a5f-a790-561a6fc73946"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Deck"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c1dc6b6-c82d-404a-a263-466774a16578"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Sections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""515ffaa0-6635-43b0-b522-52af6758cf7f"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sections"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""43f02579-1d93-4602-8e27-f02aeac50012"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Island"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d113d188-3fca-47a7-9361-cd432ad41acf"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Island"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e9b5a7de-b0ab-4083-b761-290b8a777020"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""FreeView"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""82dcaae8-0088-40ee-9611-c24048f0fce9"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FreeView"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""062b3368-53bc-45fa-8fee-f668bcc9d3bf"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""97e2eecb-172e-41b4-aab0-c437aa436ef1"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""2699b6f8-736f-4155-9553-6e7b22d1d990"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""f296560e-0494-49c0-aa3f-a70132f76a07"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""727c695f-69b2-496b-8fbe-a62311e1fd9b"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""da215802-d478-414a-894a-da9f558d52df"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""e7c9aa12-fbde-45e2-afd5-5462165f711d"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""1aeab901-0082-465e-b7b3-99ec70138616"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""e3c77dee-ee6e-463c-8b49-be967eb9842d"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""2e1d7c6e-51b9-4799-80c9-f4bb71938db6"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""0011ae46-60cd-4562-b35b-5c25f2d43dda"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""9315dfd9-540c-462d-ad74-0e2322fa852c"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""fb3fb932-ef08-48c1-a2ec-a1b45ac00e54"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""df145496-5db0-4798-add1-6f25081c9c5b"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""0bb9c075-1108-4a0f-9018-382dd2f5e173"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""9232554f-cfae-4a18-9464-1db523353f8e"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""318cf5ea-e896-4de4-8a38-b41acaccc48d"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""6ac0232d-2cd3-45c6-8f47-cc398b057e02"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""VerticalMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""DamageControl"",
            ""id"": ""264f673d-d705-4ac0-81fb-09725f4d365a"",
            ""actions"": [
                {
                    ""name"": ""MalfunctionButton"",
                    ""type"": ""Button"",
                    ""id"": ""5936dc26-e231-425a-8723-c3902cd43980"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""DefloodButton"",
                    ""type"": ""Button"",
                    ""id"": ""10bb5854-d31a-4cdb-aba8-90d1cbec39cd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""41ad888f-55aa-41ec-bed0-1d14af5627a0"",
                    ""path"": ""<Keyboard>/h"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""DefloodButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a891a05e-8c19-4614-aaa7-22751af6994e"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DefloodButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""108320be-99ff-4889-b54b-ce4769c381b9"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""MalfunctionButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""99ea1a94-26c0-4bce-83cd-30d4c09baa53"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MalfunctionButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""TacticMissions"",
            ""id"": ""4bcac144-6594-4102-b65f-2a7fbc720b23"",
            ""actions"": [
                {
                    ""name"": ""PrepareAirstrike"",
                    ""type"": ""Button"",
                    ""id"": ""d7d6d444-ff63-414d-81df-9b819ae83dd2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PrepareIdentify"",
                    ""type"": ""Button"",
                    ""id"": ""389ada10-df6d-471a-92dc-cd60e0b7da46"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PrepareRecon"",
                    ""type"": ""Button"",
                    ""id"": ""066b81e1-276b-427d-a6b7-7ab997235d44"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c5af8ddf-25af-45f7-940d-c5c3a1af13a8"",
                    ""path"": ""<Keyboard>/p"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""PrepareAirstrike"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a9f251fe-a1f5-465f-b354-0a4f798ea726"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareAirstrike"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8d3e326b-d5d4-4dbe-bbaa-ba83b34483e4"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""PrepareRecon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e59106d6-05ba-4998-88a4-17ce037b7968"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareRecon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4dc11687-a791-4c23-8115-fb237b1a0e7d"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""PrepareIdentify"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""28f07ace-1e77-41c0-bbdf-42eed1cb5948"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareIdentify"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""e53bfc3d-dc77-48c0-ba52-d3228234eaad"",
            ""actions"": [
                {
                    ""name"": ""Map"",
                    ""type"": ""Button"",
                    ""id"": ""fa9dcfd0-17d4-48c4-846b-cf8441ce86cf"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CrewPanel"",
                    ""type"": ""Button"",
                    ""id"": ""06c757cb-3c0f-4959-ab9c-ce448f0e0638"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Orders"",
                    ""type"": ""Button"",
                    ""id"": ""60b4621c-d867-42b7-9d20-d25351f4a2fd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ChangeDeck"",
                    ""type"": ""Button"",
                    ""id"": ""2aba9052-9788-4319-a5fc-06c73dcfcaf0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TimeSpeedUp"",
                    ""type"": ""Button"",
                    ""id"": ""e2170064-4441-4c4a-bc2a-144ef906e2be"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TimeSpeedDown"",
                    ""type"": ""Button"",
                    ""id"": ""9462201d-b8b3-40e3-b9de-011b109965e1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""db68d7ab-cbce-4702-952d-0cc1d943a19f"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Map"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d367170e-30b6-46a4-abb8-a4befec7a74a"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Map"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""29224d77-1c32-46fb-92d5-89f9de143ed8"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""CrewPanel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cc6603e0-dc39-4aaa-91ef-a00450f84618"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CrewPanel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""78841b5e-94b9-49b0-a1e3-b3e8533cf2fb"",
                    ""path"": ""<Keyboard>/o"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Orders"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2ac73d35-0fea-44dd-9a11-928e381a5cf2"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Orders"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7ac46405-3bef-4a00-9e5e-395001d52c39"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""ChangeDeck"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c5b0544-12b0-4f47-b5c6-b07939258055"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeDeck"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cbdee104-07af-4936-bcfc-9de40c3e535d"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""TimeSpeedUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dc6c29bf-9f68-4c68-850a-71892b36b89d"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TimeSpeedUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e5b6431b-0a6b-422b-94cf-c03effa8ab69"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""TimeSpeedDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1e8ee5b3-f00d-42b9-8c86-ea3b4009a26b"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TimeSpeedDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""SetupDC"",
            ""id"": ""a5ce60ee-0306-48e3-82a4-b93114ea7ec9"",
            ""actions"": [
                {
                    ""name"": ""SetToFire"",
                    ""type"": ""Button"",
                    ""id"": ""81bb28c1-8e00-4b7a-9534-03f86afa28a0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SetToMalfunction"",
                    ""type"": ""Button"",
                    ""id"": ""d1874fbf-1486-4076-80b9-a96fe0d134d3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SetToFlood"",
                    ""type"": ""Button"",
                    ""id"": ""740c2eb8-ba15-4a09-91ff-24fa21942c8c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SetToMedic"",
                    ""type"": ""Button"",
                    ""id"": ""4b3ad741-1bc0-4292-87d8-b64432a8267a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""33646b87-d3c9-447c-af65-e44c352b7340"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetToFire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""66ca9901-99e0-4ae8-9825-36b7c6bbd463"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetToFire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2b836272-4423-49f0-8cd0-c59e30647493"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetToFlood"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b3d38976-9c69-42e0-be3e-321e7f5881e3"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetToFlood"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""896db099-3cd8-4377-8009-1b2578e750f2"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetToMedic"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cfd25884-bfce-4cd2-979a-248a270adda6"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetToMedic"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7ab3dbfe-2ac7-44b1-973f-6ef152a3bbaf"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetToMalfunction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8ac1b494-ee43-4dd2-b4b1-12c87c24257a"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetToMalfunction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""PreparePlanes"",
            ""id"": ""ef05e8b0-a4d0-400f-8d71-9a306158ae74"",
            ""actions"": [
                {
                    ""name"": ""PrepareFighter"",
                    ""type"": ""Button"",
                    ""id"": ""b6cc2a68-cb4d-42f2-9b94-a988feeecf83"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PrepareBomber"",
                    ""type"": ""Button"",
                    ""id"": ""0ce90a5a-a4c7-45cd-b7d9-a17ad3b6c15d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PrepareTorpedo"",
                    ""type"": ""Button"",
                    ""id"": ""d3a055cb-57b0-4c7f-a08b-ab037ebf99a4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a001fd21-fcb2-4cd4-9358-a24dc7b29331"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareFighter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4542da73-5876-417f-a5b2-5bf658498134"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareFighter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a2cd1522-b6a8-4a82-a2b2-c8f751b4bff2"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareBomber"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""afdb8472-ac1d-4a5d-a3c6-004af7396c06"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareBomber"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""78f8394e-c5b1-435f-8909-f503795b9883"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareTorpedo"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e65bc06f-bef7-4d43-931d-8bbe5a17671a"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrepareTorpedo"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""CarrierSpeed"",
            ""id"": ""cfa42fa4-b4b4-4103-9b3c-d40c4ac238a2"",
            ""actions"": [
                {
                    ""name"": ""DeadSlow"",
                    ""type"": ""Button"",
                    ""id"": ""44b9b9a9-7355-4ea7-b402-88929684384b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Slow"",
                    ""type"": ""Button"",
                    ""id"": ""2b90aa0c-4876-4a1d-9a12-80b6e02f5215"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Half"",
                    ""type"": ""Button"",
                    ""id"": ""4c8d7af0-c357-430a-a0ac-81e55bf4c7b1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Full"",
                    ""type"": ""Button"",
                    ""id"": ""82186dae-46b1-4cf1-8ac1-e2055c922cc4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Stop"",
                    ""type"": ""Button"",
                    ""id"": ""d710147e-49c3-478d-bbd4-d1b819a71f3a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""e508482c-b66a-4926-9715-9b2699573558"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DeadSlow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""470a633f-d3ac-416b-b86a-1580ed64d0a6"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DeadSlow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e75853bd-4ed4-4dd3-a93c-8fa16d93e788"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Slow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a0a2723b-2856-4d4c-a8fb-f90dca7730fd"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Slow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f3958713-aa6d-4ffb-8f33-6eaceb6ac63d"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Half"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0d6552b5-f110-44f8-90b8-52945681b295"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Half"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e5a17139-7804-493f-a7e9-302fb7fbdf54"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Full"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""24c4f1d1-d2f9-445d-ac8d-0dcfe7f01734"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Full"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be744ce1-db4f-4369-b010-f57ff7c47925"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Stop"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2b7e1fb0-74e8-4698-b008-027dd4c4508e"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Stop"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Save"",
            ""id"": ""d53cd238-38c2-45e3-aeb7-bc560a3e5b1d"",
            ""actions"": [
                {
                    ""name"": ""QuickSave"",
                    ""type"": ""Button"",
                    ""id"": ""58027081-3055-4a07-8913-979830e85705"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""QuickLoad"",
                    ""type"": ""Button"",
                    ""id"": ""12acbb0f-f3e7-49a4-9347-8dc1c4b0bcb3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""43b007e1-ac28-48db-a539-bb831d749473"",
                    ""path"": ""<Keyboard>/f5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""QuickSave"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7d2227fc-1ca7-4014-b6c0-8ad31a41d32e"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""QuickSave"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""04e3922c-d153-48ba-a83b-beaf512bdd10"",
                    ""path"": ""<Keyboard>/f9"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""QuickLoad"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be7102d9-90f8-4097-955b-c9c7ab621607"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""QuickLoad"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": []
        }
    ]
}");

        // Camera
        camera = asset.FindActionMap("Camera", throwIfNotFound: true);
        camera_HorizontalMovement = camera.FindAction("HorizontalMovement", throwIfNotFound: true);
        camera_VerticalMovement = camera.FindAction("VerticalMovement", throwIfNotFound: true);
        camera_CameraRotation = camera.FindAction("CameraRotation", throwIfNotFound: true);
        camera_Deck = camera.FindAction("Deck", throwIfNotFound: true);
        camera_Sections = camera.FindAction("Sections", throwIfNotFound: true);
        camera_Island = camera.FindAction("Island", throwIfNotFound: true);
        camera_FreeView = camera.FindAction("FreeView", throwIfNotFound: true);
        // DamageControl
        damageControl = asset.FindActionMap("DamageControl", throwIfNotFound: true);
        damageControl_MalfunctionButton = damageControl.FindAction("MalfunctionButton", throwIfNotFound: true);
        damageControl_DefloodButton = damageControl.FindAction("DefloodButton", throwIfNotFound: true);
        // TacticMissions
        tacticMissions = asset.FindActionMap("TacticMissions", throwIfNotFound: true);
        tacticMissions_PrepareAirstrike = tacticMissions.FindAction("PrepareAirstrike", throwIfNotFound: true);
        tacticMissions_PrepareIdentify = tacticMissions.FindAction("PrepareIdentify", throwIfNotFound: true);
        tacticMissions_PrepareRecon = tacticMissions.FindAction("PrepareRecon", throwIfNotFound: true);
        // UI
        ui = asset.FindActionMap("UI", throwIfNotFound: true);
        ui_Map = ui.FindAction("Map", throwIfNotFound: true);
        ui_CrewPanel = ui.FindAction("CrewPanel", throwIfNotFound: true);
        ui_Orders = ui.FindAction("Orders", throwIfNotFound: true);
        ui_ChangeDeck = ui.FindAction("ChangeDeck", throwIfNotFound: true);
        ui_TimeSpeedUp = ui.FindAction("TimeSpeedUp", throwIfNotFound: true);
        ui_TimeSpeedDown = ui.FindAction("TimeSpeedDown", throwIfNotFound: true);
        // SetupDC
        setupDC = asset.FindActionMap("SetupDC", throwIfNotFound: true);
        setupDC_SetToFire = setupDC.FindAction("SetToFire", throwIfNotFound: true);
        setupDC_SetToMalfunction = setupDC.FindAction("SetToMalfunction", throwIfNotFound: true);
        setupDC_SetToFlood = setupDC.FindAction("SetToFlood", throwIfNotFound: true);
        setupDC_SetToMedic = setupDC.FindAction("SetToMedic", throwIfNotFound: true);
        // PreparePlanes
        preparePlanes = asset.FindActionMap("PreparePlanes", throwIfNotFound: true);
        preparePlanes_PrepareFighter = preparePlanes.FindAction("PrepareFighter", throwIfNotFound: true);
        preparePlanes_PrepareBomber = preparePlanes.FindAction("PrepareBomber", throwIfNotFound: true);
        preparePlanes_PrepareTorpedo = preparePlanes.FindAction("PrepareTorpedo", throwIfNotFound: true);
        // CarrierSpeed
        carrierSpeed = asset.FindActionMap("CarrierSpeed", throwIfNotFound: true);
        carrierSpeed_DeadSlow = carrierSpeed.FindAction("DeadSlow", throwIfNotFound: true);
        carrierSpeed_Slow = carrierSpeed.FindAction("Slow", throwIfNotFound: true);
        carrierSpeed_Half = carrierSpeed.FindAction("Half", throwIfNotFound: true);
        carrierSpeed_Full = carrierSpeed.FindAction("Full", throwIfNotFound: true);
        carrierSpeed_Stop = carrierSpeed.FindAction("Stop", throwIfNotFound: true);
        // Save
        save = asset.FindActionMap("Save", throwIfNotFound: true);
        save_QuickSave = save.FindAction("QuickSave", throwIfNotFound: true);
        save_QuickLoad = save.FindAction("QuickLoad", throwIfNotFound: true);
    }

    private string GetBindingPath(InputAction action, int index)
    {
        var binding = action.bindings[index];
        return (binding.overridePath ?? binding.path) ?? "";
    }

    private void ApplyBindingOverride(InputAction action, int index, string path)
    {
        action.ApplyBindingOverride(index, string.IsNullOrWhiteSpace(path) ? "" : path);
    }

    private readonly InputActionAsset asset;

    // Camera
    private readonly InputActionMap camera;
    private readonly InputAction camera_HorizontalMovement;
    private readonly InputAction camera_VerticalMovement;
    private readonly InputAction camera_CameraRotation;
    private readonly InputAction camera_Deck;
    private readonly InputAction camera_Sections;
    private readonly InputAction camera_Island;
    private readonly InputAction camera_FreeView;

    // DamageControl
    private readonly InputActionMap damageControl;
    private readonly InputAction damageControl_MalfunctionButton;
    private readonly InputAction damageControl_DefloodButton;

    // TacticMissions
    private readonly InputActionMap tacticMissions;
    private readonly InputAction tacticMissions_PrepareAirstrike;
    private readonly InputAction tacticMissions_PrepareIdentify;
    private readonly InputAction tacticMissions_PrepareRecon;

    // UI
    private readonly InputActionMap ui;
    private readonly InputAction ui_Map;
    private readonly InputAction ui_CrewPanel;
    private readonly InputAction ui_Orders;
    private readonly InputAction ui_ChangeDeck;
    private readonly InputAction ui_TimeSpeedUp;
    private readonly InputAction ui_TimeSpeedDown;

    // SetupDC
    private readonly InputActionMap setupDC;
    private readonly InputAction setupDC_SetToFire;
    private readonly InputAction setupDC_SetToMalfunction;
    private readonly InputAction setupDC_SetToFlood;
    private readonly InputAction setupDC_SetToMedic;

    // PreparePlanes
    private readonly InputActionMap preparePlanes;
    private readonly InputAction preparePlanes_PrepareFighter;
    private readonly InputAction preparePlanes_PrepareBomber;
    private readonly InputAction preparePlanes_PrepareTorpedo;

    // CarrierSpeed
    private readonly InputActionMap carrierSpeed;
    private readonly InputAction carrierSpeed_DeadSlow;
    private readonly InputAction carrierSpeed_Slow;
    private readonly InputAction carrierSpeed_Half;
    private readonly InputAction carrierSpeed_Full;
    private readonly InputAction carrierSpeed_Stop;

    // Save
    private readonly InputActionMap save;
    private readonly InputAction save_QuickSave;
    private readonly InputAction save_QuickLoad;
}
