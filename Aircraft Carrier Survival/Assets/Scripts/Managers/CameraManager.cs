using Cinemachine;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.AzureSky;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour, IEnableable
{
    public event Action<ECameraView> ViewChanged = delegate { };
    public event Action<ECameraView> CameraTargetChanged = delegate { };
    public event Action DeckCameraChanged = delegate { };
    public event Action CameraMoved = delegate { };
    public event Action<bool> CameraZoomed = delegate { };

    private const float FieldHoverMouse = 100.0f;
    private const int SecondaryCameraLayer = 16;

    public static CameraManager Instance;

    public float DistanceToSea => Mathf.Clamp01((MainCamera.transform.position.y - 50f) / (FreeView.StartOrbits[0].m_Height - 50f));

    public ECameraView CurrentCameraView => currentCameraView;

    public bool IsFreeMode
    {
        get => isFreeMode;
        private set
        {
            isFreeMode = value;
            if (currentCameraView == ECameraView.PreviewCamera)
            {
                ResetSecondaryCamera();
            }
            else
            {
                lastView = currentCameraView;
            }
            currentCameraView = ECameraView.Free;
            TacticalMapClouds.Instance.AzureEffectsController.followTarget = MainCamera.transform;

            bool blending = isBlending > 0;
            CameraTargetChanged(currentCameraView);

            isDeckShown = false;
            isIslandShown = false;

            currentView = null;
            currentFreeView = FreeView;

            if (!loading)
            {
                FreeView.SetLockRanges(true);
            }
            FreeView.FreeCamera.gameObject.SetActive(true);
            DeckView.Camera.gameObject.SetActive(false);
            AlternativeDeckView.Camera.gameObject.SetActive(false);
            SectionView.Camera.gameObject.SetActive(false);
            IslandView.FreeCamera.gameObject.SetActive(false);
            SetBlend(true);

            SetAnims();
        }
    }

    public bool IsDeckShown
    {
        get => isDeckShown;
        private set
        {
            isDeckShown = value;
            if (currentCameraView == ECameraView.PreviewCamera)
            {
                ResetSecondaryCamera();
            }
            else
            {
                lastView = currentCameraView;
            }
            currentCameraView = value ? ECameraView.Deck : ECameraView.Sections;
            TacticalMapClouds.Instance.AzureEffectsController.followTarget = MainCamera.transform;

            bool blending = isBlending > 0;
            CameraTargetChanged(currentCameraView);

            AlternativeDeckView.Camera.gameObject.SetActive(deckDefaultView ? false : value);
            DeckView.Camera.gameObject.SetActive(deckDefaultView ? value : false);
            SectionView.Camera.gameObject.SetActive(!value);
            IslandView.FreeCamera.gameObject.SetActive(false);
            FreeView.FreeCamera.gameObject.SetActive(false);
            isIslandShown = false;
            isFreeMode = false;
            if (value)
            {
                currentDeckView = deckDefaultView ? DeckView : AlternativeDeckView;
                currentView = null;
            }
            else
            {
                currentView = SectionView;
            }
            currentFreeView = null;
            SetBlend(true);

            if (lastView == ECameraView.Free && !blending)
            {
                FreeLookBlend();
            }
            else
            {
                SetAnims();
            }
        }
    }

    public bool IsIslandShown
    {
        get => isIslandShown;
        private set
        {
            isIslandShown = value;
            if (currentCameraView == ECameraView.PreviewCamera)
            {
                ResetSecondaryCamera();
            }
            else
            {
                lastView = currentCameraView;
            }
            currentCameraView = value ? ECameraView.Island : ECameraView.Sections;
            TacticalMapClouds.Instance.AzureEffectsController.followTarget = MainCamera.transform;

            bool blending = isBlending > 0;

            CameraTargetChanged(currentCameraView);

            DeckView.Camera.gameObject.SetActive(false);
            AlternativeDeckView.Camera.gameObject.SetActive(false);
            isDeckShown = false;
            SectionView.Camera.gameObject.SetActive(!value);
            if (!loading && value)
            {
                IslandView.SetLockRanges(true);
            }
            IslandView.FreeCamera.gameObject.SetActive(value);
            FreeView.FreeCamera.gameObject.SetActive(false);
            isFreeMode = false;
            currentView = value ? null : SectionView;
            currentFreeView = value ? IslandView : null;
            SetBlend(true);

            if (lastView == ECameraView.Free && !blending)
            {
                FreeLookBlend();
            }
            else
            {
                SetAnims();
            }
        }
    }

    public ECameraInputType CameraInput
    {
        get;
        set;
    }

    public bool FixAttackCamera
    {
        get;
        set;
    }

    [Header("Particular view params")]
    public DeckViewData DeckView;
    public DeckViewData AlternativeDeckView;
    public ViewData SectionView;
    public FreeViewData IslandView;
    public FreeViewData FreeView;
    [Header("Below don't touch")]

    public CinemachineBrain Brain;
    public Camera MainCamera;
    public PostProcessVolume PostProcessVolume;

    public float ScrollThresholdToFreeCam = .15f;

    public List<GameObject> SectionsSideWater;

    public CinemachineVirtualCamera PreviewAttackCamera;

    [SerializeField]
    private StudioEventEmitter changeViewSound = null;

    [SerializeField]
    private CameraSwitchPoint islandSwitch = null;
    [SerializeField]
    private CameraSwitchPoint deckSwitch = null;
    [SerializeField]
    private CameraSwitchPoint sectionsSwitch = null;

    [SerializeField]
    private CarrierWallAnim sectionSideWallAnim = null;

    [SerializeField]
    private CarrierWallAnim islandSideWallAnim = null;

    [SerializeField]
    private float maxSafeDistance = 16000f;

    [SerializeField]
    private CinemachineFreeLook freeLookHelper = null;

    [SerializeField]
    private Scrollbar cameraScrollbar = null;

    [SerializeField]
    private Text nextCameraViewText = null;

    [SerializeField]
    private List<string> scrollLabelIDs = null;

    [SerializeField]
    private CameraButton cameraButton = null;

    [Header("All views params(can touch)")]
    [Tooltip("How long you have to scroll to change view")]
    [SerializeField]
    private float scrollInOutThreshold = .375f;

    [SerializeField]
    [Tooltip("How long breaks you can have between scrolls to change view")]
    private float timeBetweenScrollsThreshold = .3f;

    [SerializeField]
    [FormerlySerializedAs("verticalMovementFreeCamFactor")]
    [Tooltip("Change mouse vertical \"power\" of input to mitigate lower than wider screen")]
    private float verticalMovementMouseFactor = 1f;

    [SerializeField]
    [Tooltip("Change arrows/wasd vertical \"power\" of input to mitigate lower than wider screen")]
    private float verticalMovementKeyboardFactor = 1f;

    [SerializeField]
    private float mouseMinSpeed = .5f;
    [SerializeField]
    private float mouseMaxSpeed = 10f;

    [SerializeField]
    [Tooltip("\"Power\" of arrows and wasd")]
    private float keyboardInputPower = 1f;

    [SerializeField]
    [Tooltip("How much time you have to press arrows or wasd to have highest inpact on camera movement")]
    private float keyboardInputTimeToFullPower = 1f;

    [SerializeField]
    [Tooltip("Max inertia power when mouse has that delta distance in frame")]
    private float inertiaMaxPowerRMBDistance = 100f;

    [SerializeField]
    [Tooltip("How much to include former frame mouse delta distance to camera initial inertia")]
    private float previousRMBDeltaPowerFactor = .1f;

    [SerializeField]
    [Tooltip("How long inertia lasts")]
    private float inertiaTime = 1f;

    [SerializeField]
    [Tooltip("How much mouse has influence on inertia")]
    private float inertiaFactorRMB = 1f;

    [SerializeField]
    [Tooltip("How much wasd or arrows have influence on inertia")]
    private float inertiaFactorKeyboard = 1f;

    [SerializeField]
    private Camera secondaryCamera = null;
    [SerializeField]
    private Camera cameraWater = null;
    [SerializeField]
    private CinemachineVirtualCamera dummy = null;

    [SerializeField]
    private PostProcessLayer postProcess = null;

    [SerializeField]
    private PostProcessProfile animationReportProfile = null;
    [SerializeField]
    private PostProcessProfile defaultProfile = null;

    [SerializeField]
    private AzureFogScattering secondaryFog = null;

    private ECameraView currentCameraView = ECameraView.Deck;

    private bool isDeckShown;
    private bool isIslandShown;
    private bool isFreeMode;

    private int isBlending = 3;

    private ViewData currentView;
    private DeckViewData currentDeckView;
    private FreeViewData currentFreeView;
    private float sectionViewMiddleX;

    private Vector3 preverMousePosition;
    private Vector3 prevMousePosition;
    private bool isPanning;

    private bool deckDefaultView = true;
    private bool worldMapMode = false;

    private float scrollInOutTime = 0f;

    private ECameraView lastView = ECameraView.Deck;

    private float timeBetweenScrolls;
    private int helperBlendIndex;
    private CinemachineBlendDefinition helperBlend;

    private ECameraView animLastView;
    private ECameraView animView;

    private Vector2 keyboardPowerTimer;
    private float inertiaTimer;
    private bool mouseInertia;
    private bool newValuesRMB;
    private bool newValuesKeyboard;
    private Vector2 inertiaFactor;

    private bool loading;
    private bool disabled;

    private List<Transform> secondaryCameraTargetsList;
    private List<SecondaryCameraParamsData> secondaryCameraChanges;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        ViewChanged += OnViewChanged;
        CameraTargetChanged += ViewTargetChanged;

        keyboardPowerTimer = Vector2.zero;
        CameraInput = (ECameraInputType)(-1);

        secondaryCameraTargetsList = new List<Transform>() { MainCamera.transform };

        secondaryCameraChanges = new List<SecondaryCameraParamsData>();
    }

    private void Start()
    {
        WorldMap.Instance.Toggled += OnWorldMapToggled;
        DeckView.Setup();
        AlternativeDeckView.Setup();

        SectionView.Setup();
        IslandView.Setup();
        FreeView.Setup();
        freeLookHelper.m_XAxis.Value = FreeView.FreeCamera.m_XAxis.Value;

        sectionViewMiddleX = (SectionView.BottomLeft.position.x + SectionView.BottomRight.position.x) / 2f;

        helperBlendIndex = -1;
        var blends = Brain.m_CustomBlends.m_CustomBlends;
        for (int i = 0; i < blends.Length; i++)
        {
            if (blends[i].m_From == FreeView.FreeCamera.name && blends[i].m_To == freeLookHelper.name)
            {
                helperBlend = blends[i].m_Blend;
                helperBlendIndex = i;
                break;
            }
        }
        if (helperBlendIndex == -1)
        {
            Debug.LogError("No helper blend!");
            helperBlendIndex = 0;
        }

        inertiaTimer = 1e9f;
    }

    private void Update()
    {
        bool broken = false;
        foreach (var data in secondaryCameraChanges)
        {
            var targetTrans = data.Transform;
            if (data.Set)
            {
                dummy.gameObject.SetActive(true);
                secondaryCameraTargetsList.Add(targetTrans);
            }
            else
            {
                int index = secondaryCameraTargetsList.IndexOf(targetTrans);
                if (index < 0 || index >= secondaryCameraTargetsList.Count)
                {
                    broken = true;
                    break;
                }
                secondaryCameraTargetsList.RemoveAt(index);
                if (secondaryCameraTargetsList.Count == index)
                {
                    targetTrans = secondaryCameraTargetsList[secondaryCameraTargetsList.Count - 1];
                }
                else
                {
                    break;
                }
            }
            var trans = secondaryCamera.transform;
            trans.SetParent(targetTrans);
            TacticalMapClouds.Instance.AzureEffectsController.followTarget = secondaryCamera.transform;
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
            int mask = 1 << SecondaryCameraLayer | 1 << 17;
            if (secondaryCameraTargetsList.Count == 1)
            {
                dummy.gameObject.SetActive(false);
                secondaryCamera.cullingMask = mask;
                cameraWater.depth = -2;
                secondaryFog.enabled = false;
            }
            else
            {
                //secondaryCamera.cullingMask = (mask | MainCamera.cullingMask) &(~(1));
                cameraWater.depth = 12;
                secondaryFog.enabled = true;
            }
        }
        secondaryCameraChanges.Clear();
        if (broken)
        {
            var targetTrans = MainCamera.transform;
            foreach (var obj in secondaryCameraTargetsList)
            {
                if (obj != null && obj != targetTrans)
                {
                    obj.gameObject.SetActive(false);
                }
            }
            secondaryCameraTargetsList.Clear();
            secondaryCameraTargetsList.Add(targetTrans);

            var trans = secondaryCamera.transform;
            trans.SetParent(targetTrans);
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
            dummy.gameObject.SetActive(false);
            secondaryCamera.cullingMask = 1 << SecondaryCameraLayer | 1 << 17;
            cameraWater.depth = -2;
        }
        if (!Brain.IsBlending && --isBlending == 0)
        {
            SetBlend(false);
        }

        bool clearKeyboardPowerX = true;
        bool clearKeyboardPowerY = true;

        var hudMan = HudManager.Instance;
        if (hudMan.AcceptInput && !hudMan.IsSettingsOpened)
        {
            if (hudMan.HasNo(ETutorialMode.DisableCameraControls) && !Brain.IsBlending)
            {
                float scroll = disabled ? 0f : Input.GetAxis("Mouse ScrollWheel");

                if (!disabled && Input.GetMouseButtonDown(1) && InputAvailable(ECameraInputType.Mouse))
                {
                    prevMousePosition = Input.mousePosition;
                    isPanning = true;
                    mouseInertia = true;
                    newValuesRMB = true;
                }
                else if (isPanning && !DragPlanesManager.Instance.IsDraggingSquadron())
                {
                    preverMousePosition = prevMousePosition;
                    isPanning = !disabled && InputAvailable(ECameraInputType.Mouse) && Input.GetMouseButton(1);
                    Vector3 diff = (Input.mousePosition - prevMousePosition);
                    float len = diff.magnitude;
                    if (len != 0f)
                    {
                        diff /= len;
                        len = Mathf.Clamp(len, mouseMinSpeed, mouseMaxSpeed);
                        diff *= len;
                        Move(diff.x * Time.unscaledDeltaTime, verticalMovementMouseFactor * diff.y * Time.unscaledDeltaTime);
                    }
                    prevMousePosition = Input.mousePosition;

                    newValuesRMB = true;
                }
                else
                {
                    isPanning = false;

                    int horizontal = 0;
                    int vertical = 0;
                    bool hasHorizontalInput = false;
                    bool hasVerticalInput = false;
                    if (!disabled && InputAvailable(ECameraInputType.Keyboard))
                    {
                        var camInput = BasicInput.Instance.Camera;
                        horizontal += (int)camInput.VerticalMovement.ReadValue<float>();
                        vertical -= (int)camInput.HorizontalMovement.ReadValue<float>();
                        hasHorizontalInput = horizontal != 0;
                        hasVerticalInput = vertical != 0;
                    }

                    if (currentFreeView != null)
                    {
                        currentFreeView.Rotate(BasicInput.Instance.Camera.CameraRotation.ReadValue<float>());
                    }

                    if (hasHorizontalInput || hasVerticalInput)
                    {
                        if (hasHorizontalInput)
                        {
                            clearKeyboardPowerX = false;
                            KeyboardInput(horizontal, ref keyboardPowerTimer.x);
                        }
                        if (hasVerticalInput)
                        {
                            clearKeyboardPowerY = false;
                            KeyboardInput(horizontal, ref keyboardPowerTimer.y);
                        }
                        Vector2 factor = keyboardPowerTimer / keyboardInputTimeToFullPower * keyboardInputPower;
                        Move(horizontal * Time.unscaledDeltaTime * Mathf.Abs(factor.x), vertical * Time.unscaledDeltaTime * Mathf.Abs(factor.y) * verticalMovementKeyboardFactor);

                        mouseInertia = false;
                        newValuesKeyboard = true;
                    }
                    else
                    {
                        if (newValuesRMB)
                        {
                            inertiaTimer = 0f;
                            newValuesRMB = false;

                            var m2 = prevMousePosition - preverMousePosition;
                            float len2 = m2.magnitude;
                            if (len2 > 0f)
                            {
                                m2 /= len2;
                                m2 *= Mathf.Min(len2 / inertiaMaxPowerRMBDistance, 1f);
                            }

                            var m1 = Input.mousePosition - prevMousePosition;
                            float len1 = m1.magnitude;
                            if (len1 > 0f)
                            {
                                m1 /= len1;
                                m1 *= Mathf.Min(len1 / inertiaMaxPowerRMBDistance, 1f);
                            }

                            inertiaFactor = m2 * previousRMBDeltaPowerFactor + m1 * (1f - previousRMBDeltaPowerFactor);

                            //Debug.LogWarning("factor: " + inertiaFactor.ToString("F3"));
                            //Debug.LogWarning(";prever: " + preverMousePosition.ToString("F3") + ";prev: " + prevMousePosition.ToString("F3") + ";m2: " + (prevMousePosition - preverMousePosition).ToString("F3") + 
                            //    ";len2: " + len2.ToString("F3") + ";m2now: " + m2.ToString("F3") + ";mousePosition: " + Input.mousePosition.ToString("F3") + ";m1: " +
                            //    (Input.mousePosition - prevMousePosition).ToString("F3") + ";len1: " + len1.ToString("F3") + ";m1now: " + m1.ToString("F3"));
                        }
                        if (newValuesKeyboard)
                        {
                            inertiaTimer = 0f;
                            newValuesKeyboard = false;
                            inertiaFactor = keyboardPowerTimer / keyboardInputTimeToFullPower * keyboardInputPower;
                        }

                        inertiaTimer += Time.unscaledDeltaTime;
                        var boost = (mouseInertia ? inertiaFactorRMB : inertiaFactorKeyboard) * inertiaFactor;
                        Vector2 factor = new Vector2(inertiaTime, inertiaTime);
                        if (inertiaTimer >= factor.x)
                        {
                            factor.x = 0f;
                        }
                        else
                        {
                            factor.x = boost.x * (1f - (inertiaTimer / factor.x));
                            if (Mathf.Abs(factor.x) < .5f)
                            {
                                factor.x = 0f;
                            }
                        }
                        if (inertiaTimer >= factor.y)
                        {
                            factor.y = 0f;
                        }
                        else
                        {
                            //factor.y = (inertiaFactor.y < 0f ? -1f : 1f);// * (1f - (inertiaTimer / factor.y));
                            factor.y = boost.y * (1f - (inertiaTimer / factor.y));
                            if (Mathf.Abs(factor.y) < .5f)
                            {
                                factor.y = 0f;
                            }
                        }
                        if (factor.x != 0f || factor.y != 0f)
                        {
                            Move(factor.x * Time.unscaledDeltaTime, factor.y * Time.unscaledDeltaTime);
                        }
                    }
                }
                if (Scroll(scroll) && InputAvailable(ECameraInputType.ScrollSwitch) && CurrentCameraView != ECameraView.PreviewCamera)
                {
                    cameraScrollbar.gameObject.SetActive(true);
                    timeBetweenScrolls = 0f;
                    scrollInOutTime += Time.unscaledDeltaTime;
                    cameraScrollbar.value = scrollInOutTime / scrollInOutThreshold;

                    int index = 0;
                    switch (IsFreeMode ? lastView : ECameraView.Free)
                    {
                        case ECameraView.Island:
                            index = 0;
                            break;
                        case ECameraView.Free:
                            index = 1;
                            break;
                        case ECameraView.Deck:
                            index = 2;
                            break;
                        case ECameraView.Sections:
                            index = 3;
                            break;
                    }
                    nextCameraViewText.text = LocalizationManager.Instance.GetText(scrollLabelIDs[index]);
                    if (scrollInOutTime >= scrollInOutThreshold)
                    {
                        SwitchMode(IsFreeMode ? lastView : ECameraView.Free);
                    }
                }
                else
                {
                    timeBetweenScrolls += Time.unscaledDeltaTime;
                    if (timeBetweenScrolls > timeBetweenScrollsThreshold)
                    {
                        ResetScroll();
                    }
                }
            }
        }

        if (clearKeyboardPowerX)
        {
            keyboardPowerTimer.x = 0f;
        }
        if (clearKeyboardPowerY)
        {
            keyboardPowerTimer.y = 0f;
        }
    }

    private void OnDisable()
    {
        Brain.m_CustomBlends.m_CustomBlends[helperBlendIndex].m_Blend = helperBlend;
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
#endif
        var cameraInput = BasicInput.Instance.Camera;
        cameraInput.Deck.performed -= CameraSwitchInputCallbackDeck;
        cameraInput.Sections.performed -= CameraSwitchInputCallbackSections;
        cameraInput.Island.performed -= CameraSwitchInputCallbackIsland;
        cameraInput.FreeView.performed -= CameraSwitchInputCallbackFree;
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
        deckSwitch.SetEnable(enable);
        islandSwitch.SetEnable(enable);
        sectionsSwitch.SetEnable(enable);

        cameraButton.SetSelectedEnable(enable);
    }

    public void Setup(SOTacticMap map)
    {
        var cameraInput = BasicInput.Instance.Camera;
        cameraInput.Deck.performed += CameraSwitchInputCallbackDeck;
        cameraInput.Sections.performed += CameraSwitchInputCallbackSections;
        cameraInput.Island.performed += CameraSwitchInputCallbackIsland;
        cameraInput.FreeView.performed += CameraSwitchInputCallbackFree;

        if (map.Overrides.View == null)
        {
            IsDeckShown = true;
        }
        else
        {
            SwitchMode(map.Overrides.View.Value, true);
        }
        SetBlend(false);
        BackgroundAudio.Instance.StopMainSceneSound(false);
        postProcess.antialiasingMode = (PostProcessLayer.Antialiasing)SaveManager.Instance.PersistentData.AntiAliasing;
    }

    public void LoadData(ref CameraSaveData data)
    {
        if (data.CurrentView == ECameraView.Island)
        {
            return;
        }
        loading = true;

        Brain.gameObject.SetActive(false);
        SwitchMode(data.CurrentView);

        Transform virCam;
        if (currentFreeView != null)
        {
            currentFreeView.LoadData(ref data);
            virCam = currentFreeView.FreeCamera.transform;
        }
        else if (IsDeckShown)
        {
            currentDeckView.LoadData(ref data);
            virCam = currentDeckView.Camera.transform;
        }
        else
        {
            currentView.LoadData(ref data);
            virCam = currentView.Camera.transform;
        }
        SetBlend(false);
        if (isBlending > 0)
        {
            SetBlend(false);
        }
        CameraPositionFixer.Position = Brain.transform.position = virCam.position;
        CameraPositionFixer.Rotation = Brain.transform.rotation = data.MainCameraRotation;
        Brain.gameObject.SetActive(true);
        loading = false;
    }

    public void SaveData(ref CameraSaveData data)
    {
        data.CurrentView = CurrentCameraView == ECameraView.PreviewCamera ? lastView : CurrentCameraView;
        switch (data.CurrentView)
        {
            case ECameraView.Free:
                FreeView.SaveData(ref data);
                break;
            case ECameraView.Island:
                IslandView.SaveData(ref data);
                break;
            case ECameraView.Sections:
                SectionView.SaveData(ref data);
                break;
            case ECameraView.Deck:
                currentDeckView.SaveData(ref data);
                break;
        }
        data.MainCameraRotation = (CurrentCameraView == ECameraView.PreviewCamera || Brain.IsBlending) ? data.Rotation : (MyVector4)Brain.transform.rotation;
    }

    public void ForceSwitchCamera(ECameraView view)
    {
        if (currentCameraView != view)
        {
            currentCameraView = view;
            TacticalMapClouds.Instance.AzureEffectsController.followTarget = view == ECameraView.PreviewCamera ? secondaryCamera.transform : MainCamera.transform;
            SetBlend(false);
        }
    }

    public void SwitchPostprocessVolume(bool animationReport)
    {
        PostProcessVolume.profile = animationReport ? animationReportProfile : defaultProfile;
        if (!animationReport)
        {
            SetClippingPlanes(false);
        }
    }

    public void SetClippingPlanes(bool outpost)
    {
        secondaryCamera.farClipPlane = outpost ? 50000f : 5000f;
    }

    public void OpenTutorial()
    {
        var movieMan = MovieManager.Instance;
        switch (currentCameraView)
        {
            case ECameraView.Sections:
                movieMan.Play(ETutorialType.Dc);
                break;
            case ECameraView.Island:
                movieMan.Play(ETutorialType.Island);
                break;
            case ECameraView.Free:
                movieMan.Play(ETutorialType.GettingStarted);
                break;
            default:
                movieMan.Play(ETutorialType.Deck);
                break;
        }
    }

    public void ZoomToSection(SectionRoom room)
    {
        if (!IsDeckShown)
        {
            SectionView.SetPos(new Vector3(SectionView.BottomLeft.transform.position.x, SectionView.BottomLeft.transform.position.y, room.ZCenter));
        }
    }

    public void ZoomToSectionSegment(SectionSegment segment)
    {
        if (!IsDeckShown)
        {
            var pos = new Vector3(SectionView.BottomLeft.transform.position.x, SectionView.BottomLeft.transform.position.y, segment.Center.z);
            SectionView.SetPos(pos);
        }
    }

    public void SetDeckView(bool defaultView, bool force)
    {
        if (deckDefaultView == defaultView)
        {
            return;
        }
        DragPlanesManager.Instance.ForceEndDrag();
        if (!force)
        {
            changeViewSound.Play();
        }
        deckDefaultView = defaultView;
        if (currentCameraView == ECameraView.Deck)
        {
            AlternativeDeckView.Camera.gameObject.SetActive(!deckDefaultView);
            DeckView.Camera.gameObject.SetActive(deckDefaultView);
            currentDeckView = deckDefaultView ? DeckView : AlternativeDeckView;
            DeckCameraChanged();
        }
        (defaultView ? AlternativeDeckView : DeckView).ResetPos();
    }

    public void SwitchMode(ECameraView mode, bool force = false)
    {
        if (force || (currentCameraView != mode && HudManager.Instance.AcceptInput))
        {
            switch (mode)
            {
                case ECameraView.Island:
                    IsIslandShown = true;
                    break;
                case ECameraView.Deck:
                    IsDeckShown = true;
                    break;
                case ECameraView.Sections:
                    IsIslandShown = false;
                    break;
                case ECameraView.Free:
                    IsFreeMode = true;
                    break;
            }
        }
    }

    public bool InputAvailable(ECameraInputType type)
    {
        return (CameraInput & type) == type;
    }

    public void SetSecondaryCameraPosition(in SecondaryCameraParamsData data)
    {
        //Debug.Log($"{nameof(SetSecondaryCameraPosition)}: {data.Set}, {data.Transform.name}/{data.Transform.root.name}");
        secondaryCameraChanges.Add(data);
    }

    public Transform GetCurrentCamera()
    {
        return secondaryCameraTargetsList[secondaryCameraTargetsList.Count - 1];
    }

    private void ResetSecondaryCamera()
    {
        int count = secondaryCameraTargetsList.Count;
        while (count-- > 1)
        {
            secondaryCameraTargetsList[count].gameObject.SetActive(false);
        }
    }

    private float GetEdgePanning(float mousePos, float maxMousePos)
    {
        float value = 0;
        if (!isPanning)
        {
            if (mousePos > (maxMousePos - FieldHoverMouse) && mousePos < maxMousePos)
            {
                value++;
            }
            else if (mousePos >= 0f && mousePos < FieldHoverMouse)
            {
                value--;
            }
        }
        return Mathf.Clamp(value, -1f, 1f) * Time.unscaledDeltaTime;
    }

    private void Move(float horizontal, float vertical)
    {
        if (currentCameraView == ECameraView.PreviewCamera)
        {
            return;
        }
        var data = SaveManager.Instance.PersistentData;
        if (data.InverseX)
        {
            horizontal = -horizontal;
        }
        if (data.InverseY)
        {
            vertical = -vertical;
        }
        if (horizontal != 0f || vertical != 0f)
        {
            CameraMoved();
        }
        if (currentFreeView != null)
        {
            currentFreeView.Move(horizontal, vertical);
        }
        else if (IsDeckShown)
        {
            currentDeckView.Move(horizontal);
        }
        else
        {
            currentView.Move(horizontal);
        }
    }

    private bool Scroll(float value)
    {
        if (currentCameraView == ECameraView.PreviewCamera)
        {
            return false;
        }
        float sign = Math.Sign(value);
        if (sign != 0f)
        {
            CameraZoomed(sign < 0f);
            if (currentFreeView != null)
            {
                return currentFreeView.Scroll(sign);
            }
            else if (currentView != null)
            {
                return currentView.Scroll(sign * Time.unscaledDeltaTime * MainCamera.ScreenPointToRay(Input.mousePosition).direction, sign);
            }
            else if (currentDeckView != null)
            {
                return currentDeckView.Scroll(sign);
            }
        }
        return false;
    }

    private void SetShowViewSpecificObjects(bool island, bool deck, bool section)
    {
        foreach (var side in SectionsSideWater)
        {
            side.SetActive(section);
        }
    }

    private void SetBlend(bool blend)
    {
        ResetScroll();

        StopAllCoroutines();

        if (freeLookHelper.gameObject.activeSelf)
        {
            blend = true;

            SetAnims();
        }

        freeLookHelper.gameObject.SetActive(false);
        isBlending = blend ? 3 : -1;

        ViewChanged(blend ? ECameraView.Blend : currentCameraView);
        BackgroundAudio.Instance.OnCameraViewChanged(blend ? ECameraView.Blend : currentCameraView);
        if (currentCameraView != ECameraView.Blend)
        {
            if (CurrentCameraView == ECameraView.Sections)
            {
                MainCamera.cullingMask = MainCamera.cullingMask | 1 << 18;
            }
            else
            {
                MainCamera.cullingMask = MainCamera.cullingMask & ~(1 << 18);
            }
        }
        if (!blend)
        {
            if (!loading)
            {
                IslandView.SetLockRanges(false);
                FreeView.SetLockRanges(false);
                FreeView.FreeCamera.m_YAxis.Value = .5f;
            }

            if (IsFreeMode)
            {
                Move(.001f, .001f);
            }
        }
        FixAttackCamera = false;
    }

    private void OnViewChanged(ECameraView view)
    {
        SetShowViewSpecificObjects(view == ECameraView.Island, view == ECameraView.Deck, CurrentCameraView == ECameraView.Sections);
    }

    private void CameraSwitchInputCallbackDeck(InputAction.CallbackContext _)
    {
        CameraSwitchInput(ECameraView.Deck);
    }

    private void CameraSwitchInputCallbackSections(InputAction.CallbackContext _)
    {
        CameraSwitchInput(ECameraView.Sections);
    }

    private void CameraSwitchInputCallbackIsland(InputAction.CallbackContext _)
    {
        CameraSwitchInput(ECameraView.Island);
    }

    private void CameraSwitchInputCallbackFree(InputAction.CallbackContext _)
    {
        CameraSwitchInput(ECameraView.Free);
    }

    private void CameraSwitchInput(ECameraView view)
    {
        var hudMan = HudManager.Instance;
        if (!hudMan.AcceptInput || hudMan.IsSettingsOpened || GameStateManager.Instance.AlreadyShown || disabled || !InputAvailable(ECameraInputType.InputSwitch) || !hudMan.HasNo(ETutorialMode.DisableViewSwitch))
        {
            return;
        }
        switch (view)
        {
            case ECameraView.Deck:
                if (hudMan.HasNo(ETutorialMode.DisableDeckViewSwitch) && currentCameraView != ECameraView.Deck && InputAvailable(ECameraInputType.DeckView))
                {
                    IsDeckShown = true;
                }
                break;
            case ECameraView.Sections:
                if (hudMan.HasNo(ETutorialMode.DisableSectionsViewSwitch) && currentCameraView != ECameraView.Sections && InputAvailable(ECameraInputType.SectionsView) && Input.GetKeyDown(KeyCode.Alpha2))
                {
                    IsIslandShown = false;
                }
                break;
            case ECameraView.Island:
                if (hudMan.HasNo(ETutorialMode.DisableIslandViewSwitch) && currentCameraView != ECameraView.Island && InputAvailable(ECameraInputType.IslandView) && Input.GetKeyDown(KeyCode.Alpha3))
                {
                    IsIslandShown = true;
                }
                break;
            case ECameraView.Free:
                if (hudMan.HasNo(ETutorialMode.DisableFreeViewSwitch) && currentCameraView != ECameraView.Free && InputAvailable(ECameraInputType.FreeView) && Input.GetKeyDown(KeyCode.Alpha4))
                {
                    IsFreeMode = true;
                }
                break;
        }
    }

    private void ViewTargetChanged(ECameraView view)
    {
        HudManager.Instance.UnsetBlockSpeed();

        animLastView = lastView;
        animView = view;
    }

    private void SetAnims()
    {
        sectionSideWallAnim.SetAnim(animView == ECameraView.Sections);
        islandSideWallAnim.SetAnim(animView == ECameraView.Island);
        if (animView == ECameraView.Island && animLastView == ECameraView.Sections)
        {
            sectionSideWallAnim.EndAnim();
        }
    }

    private void OnWorldMapToggled(bool state)
    {
        worldMapMode = state;
    }

    private void KeyboardInput(int power, ref float value)
    {
        float input = power < 0 ? -1f : 1f;
        if (Mathf.Sign(value) != Mathf.Sign(input) || power == 0)
        {
            value = 0f;
        }
        value = Mathf.Clamp(value + input * Time.unscaledDeltaTime, -keyboardInputTimeToFullPower, keyboardInputTimeToFullPower);
    }

    private void FreeLookBlend()
    {
        float rotation = FreeView.FreeCamera.m_XAxis.Value;
        while (rotation > 360f)
        {
            rotation -= 360f;
        }
        while (rotation <= -360f)
        {
            rotation += 360f;
        }

        float delta1 = Mathf.Abs(FreeView.StartAngle - rotation);
        float delta2 = Mathf.Abs((FreeView.StartAngle < 0f ? 1f : -1f) * 360f + FreeView.StartAngle - rotation);

        float factor = Mathf.Max((((delta1 < delta2) ? delta1 : delta2) / 180f), Mathf.Abs(FreeView.FreeCamera.m_YAxis.Value - .5f), Mathf.Abs(FreeView.GetScroll() - 1f));

        var blends = Brain.m_CustomBlends.m_CustomBlends;
        var style = (factor < .1f) ? CinemachineBlendDefinition.Style.Cut : helperBlend.m_Style;
        factor *= helperBlend.m_Time;
        blends[helperBlendIndex].m_Blend = new CinemachineBlendDefinition(style, factor);

        FreeView.SetLockRanges(true, freeLookHelper);
        freeLookHelper.gameObject.SetActive(true);
    }

    private void ResetScroll()
    {
        scrollInOutTime = 0f;
        cameraScrollbar.gameObject.SetActive(false);
        cameraScrollbar.value = 0f;
    }
}
