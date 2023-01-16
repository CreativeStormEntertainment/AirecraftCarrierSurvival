using FMODUnity;
using GambitUtils;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonMission : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite MissionSprite => missionIcon.sprite;
    public RectTransform Rect => rect;
    public RectTransform ConfirmButtonParent => confirmButtonParent;
    public Text Timer => timer;
    public Text MinutesTimer => minutesTimer;
    public Text HoursTimer => hoursTimer;
    public Text RecoveryTimer => recoveryTimer;
    public Text RecoveryMinutesTimer => recoveryMinutesTimer;
    public Text RecoveryHoursTimer => recoveryHoursTimer;
    public Text NameText => nameText;
    public Image FillImage => fillImage;
    public Image RecoveryFillImage => recoveryFillImage;
    public TacticalMission Mission => mission;

    public bool Enabled => ((int)TacticManager.Instance.EnabledMissions & (1 << (int)Mission.OrderType)) != 0 || (Mission.MissionStage > EMissionStage.Launching && Mission.MissionStage < EMissionStage.ReadyToRetrieve);

    [SerializeField]
    private RectTransform tooltipParent = null;
    [SerializeField]
    private RectTransform rect = null;
    [SerializeField]
    private RectTransform confirmButtonParent = null;
    [SerializeField]
    private Text timer = null;
    [SerializeField]
    private Text minutesTimer = null;
    [SerializeField]
    private Text hoursTimer = null;
    [SerializeField]
    private Text recoveryTimer = null;
    [SerializeField]
    private Text recoveryMinutesTimer = null;
    [SerializeField]
    private Text recoveryHoursTimer = null;
    [SerializeField]
    private Text nameText = null;
    [SerializeField]
    private Image fillImage = null;
    [SerializeField]
    private Image recoveryFillImage = null;
    [SerializeField]
    private Image missionIcon = null;
    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private Sprite red = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private GameObject animatorObject = null;
    [SerializeField]
    private GameObject strikeGroupBlink = null;
    [SerializeField]
    private StudioEventEmitter disabledClick = null;
    [SerializeField]
    private GameObject activeAnim = null;


    private TacticalMission mission;
    private MissionPanel missionPanel;

    private bool canDehighlight = true;

    private bool isClicked;
    protected Coroutine doubleClickCoroutine = null;

    private MarkerData friendData;

    private bool hovered;

    //private void Update()
    //{
    //    time += Time.deltaTime;
    //    fillImage.fillAmount = 1f - (tickTime + time) / timeToFinish;
    //}

    private void OnDisable()
    {
        TacticManager.Instance.CurrentMissionChanged -= OnCurrentMissionChanged;
    }

    public void Setup(TacticalMission m, MissionPanel panel)
    {
        missionPanel = panel;
        mission = m;
        missionIcon.sprite = mission.MissionIcon;
        TacticManager.Instance.CurrentMissionChanged += OnCurrentMissionChanged;
        m.MissionRemoved += OnMissionRemoved;
        if (mission.MissionStage == EMissionStage.Available)
        {
            if (TacticalMission.MissionsWithSetup.Contains(mission.OrderType))
            {
                nameText.gameObject.SetActive(true);
                nameText.text = mission.MissionName;
            }
            else
            {
                nameText.gameObject.SetActive(false);
            }
        }
        if (mission.OrderType == EMissionOrderType.CounterHostileScouts || mission.OrderType == EMissionOrderType.SubmarineHunt)
        {
            image.sprite = red;
        }
        if (mission.OrderType == EMissionOrderType.FriendlyFleetCAP || mission.OrderType == EMissionOrderType.FriendlyCAPMidway)
        {
            var tacticMan = TacticManager.Instance;
            var friend = tacticMan.GetShip(mission.EnemyAttackFriend.FriendID);
            foreach (var pair in tacticMan.Markers.Outposts)
            {
                if (pair.Key == friend)
                {
                    friendData = pair.Value;
                    break;
                }
            }
            foreach (var pair in tacticMan.Markers.StrikeGroups)
            {
                if (pair.Key == friend)
                {
                    friendData = pair.Value;
                    break;
                }
            }
            if (friendData != null)
            {
                tacticMan.ObjectDestroyed += OnFriendDied;
            }
        }
        Refresh();
    }

    public void Refresh()
    {
        activeAnim.SetActive(mission.MissionStage > EMissionStage.ReadyToLaunch);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Enabled)
        {
            hovered = true;
            mission.ButtonHovered = true;
            mission.SetEscortRecoverRange();
            highlight.SetActive(true);
            missionPanel.MissionDetails.SetupTooltip(mission, this);
            missionPanel.MissionDetails.ShowTooltip(tooltipParent, false);
            //mission.ShowWaypoints();
            if (mission.MissionWaypoints != null)
            {
                mission.MissionWaypoints.gameObject.SetActive(true);
                //fadeCoroutine = StartCoroutine(WaypointsFade());
            }
            if (mission.CustomMission)
            {
                mission.CustomRetrievalPoint.gameObject.SetActive(true);
            }
            if ((mission.OrderType == EMissionOrderType.FriendlyFleetCAP || mission.OrderType == EMissionOrderType.FriendlyCAPMidway) && friendData != null && friendData.Highlight != null)
            {
                //friendData.Highlight.Highlight(true);
            }
            Blink(false);
            //BackgroundAudio.Instance.PlayEvent(EMainSceneUI.)
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit();
    }

    public void PointerExit()
    {
        if (Enabled || hovered)
        {
            hovered = false;
            if (canDehighlight)
            {
                highlight.SetActive(false);
            }
            mission.ButtonHovered = false;
            missionPanel.MissionDetails.HideTooltip();
            if (mission.MissionWaypoints != null)
            {
                // StopCoroutine(fadeCoroutine);
                mission.MissionWaypoints.gameObject.SetActive(false);
            }
            if (mission.CustomMission)
            {
                mission.CustomRetrievalPoint.gameObject.SetActive(false);
            }
            if ((mission.OrderType == EMissionOrderType.FriendlyFleetCAP || mission.OrderType == EMissionOrderType.FriendlyCAPMidway) && friendData != null)
            {
                //friendData.Highlight.Highlight(false);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        if (tooltipParent.childCount > 0)
        {
            if (missionPanel.MissionDetails.Available)
            {
                if (isClicked)
                {
                    isClicked = false;
                    if (doubleClickCoroutine != null)
                    {
                        StopCoroutine(doubleClickCoroutine);
                        doubleClickCoroutine = null;
                    }
                    DoubleClick();
                }
                else
                {
                    isClicked = true;
                    doubleClickCoroutine = this.StartCoroutineActionAfterRealtime(() =>
                    {
                        isClicked = false;
                        doubleClickCoroutine = null;
                    }, .5f);
                    Click();
                }
            }
            else
            {
                disabledClick.Play();
            }
        }
    }

    private void Click()
    {
        if (Enabled)
        {
        }
    }

    private void DoubleClick()
    {
        if (Enabled && mission.MissionStage == EMissionStage.Available && TacticalMission.MissionsWithSetup.Contains(mission.OrderType))
        {
            TacticManager.Instance.StartMissionSetupMode(mission);
            missionPanel.MissionDetails.HideTooltip();
        }
    }

    //public void Tick()
    //{
    //    tickTime++;
    //    time = 0f;
    //    if (tickTime < timeToFinish)
    //    {
    //        savedMinute = timeManager.CurrentMinute;
    //        SetMinuteTimer();
    //        SetTimeText();
    //    }
    //}

    public void SetAsParent(RectTransform rect)
    {
        rect.SetParent(confirmButtonParent);
        rect.anchoredPosition = Vector2.zero;
    }

    public void Blink(bool value)
    {
        animatorObject.SetActive(value);
    }

    public void StrikeGroupBlink(bool show, EMissionOrderType missionOrderType)
    {
        if (missionOrderType == EMissionOrderType.None || missionOrderType == mission.OrderType)
        {
            var deckMan = AircraftCarrierDeckManager.Instance;
            if (deckMan.FreeSquadrons > 0)
            {
                show = mission.GetPlanesCount() <= deckMan.FreeSquadrons;
            }
            else if (deckMan.EscortRetrievingSquadrons)
            {
                show = mission.CanBeRetrievedByEscort();
            }
            strikeGroupBlink.SetActive(show);
        }
    }

    private void OnCurrentMissionChanged(TacticalMission mission)
    {
        if (mission == this.mission)
        {
            highlight.SetActive(true);
            canDehighlight = false;
        }
        else
        {
            highlight.SetActive(false);
            canDehighlight = true;
        }
    }

    private void OnMissionRemoved()
    {
        TacticManager.Instance.StartCoroutineActionAfterFrames(() =>
        {
            missionPanel.RemoveMissionButton(mission);
            missionPanel.RebuildPanel(mission.MissionList);
        }, 1);
    }

    private void OnDestroy()
    {
        TacticManager.Instance.CurrentMissionChanged -= OnCurrentMissionChanged;
        if (missionPanel.MissionDetails.Button == this)
        {
            missionPanel.MissionDetails.HideTooltip();
        }
        if (confirmButtonParent.childCount > 0)
        {
            var child = confirmButtonParent.GetChild(0);
            if (child != null)
            {
                child.GetComponent<RectTransform>().SetParent(null);
            }
        }
        //missionPanel.RemoveMissionButton(mission);
    }

    private IEnumerator WaypointsFade()
    {
        Color fadeColor = Color.white;
        bool fadeOff = true;
        while (true)
        {
            fadeColor.a += (fadeOff ? -Time.unscaledDeltaTime : Time.unscaledDeltaTime);
            foreach (Image i in mission.WaypointsImages)
            {
                i.color = fadeColor;
            }
            if (fadeOff && fadeColor.a < 0.3f)
            {
                fadeOff = false;
            }
            if (!fadeOff && fadeColor.a > 1f)
            {
                fadeOff = true;
            }
            yield return null;
        }
    }

    private void OnFriendDied(int friendID, bool _)
    {
        if (mission.EnemyAttackFriend.FriendID == friendID && mission.MissionStage < EMissionStage.Launching)
        {
            mission.RemoveMission(true);
        }
    }
}
