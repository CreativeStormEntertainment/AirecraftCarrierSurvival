using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReportPanel : MonoBehaviour, IPopupPanel
{
    public event Action ReportFinished = delegate { };

    public static ReportPanel Instance;

    public EWindowType Type => EWindowType.Report;

    public GameObject Container => container;
    public Sprite NotDead => notDead;
    public Sprite Dead => dead;
    public Sprite Damaged => damaged;
    public Sprite Detected => detected;
    public Sprite JapanFlag => japanFlag;
    public ReportData ReconStrings => reconReportData;
    public string NothingSpottedText => nothingReportData;
    public string AttackFoundNoEnemyText => attackFoundNoEnemy;
    public string StrikeGroupText => strikeGroupReportData;
    public string OutpostText => outpostReportData;
    public string WhalesText => whalesReportData;
    public string SurvivorsText => survivorsReportData;

    [SerializeField]
    private ReportMissionData airAttackReportData = null;
    [SerializeField]
    private ReportMissionData airAttackEnemyReportData = null;
    [SerializeField]
    private ReportData reconReportData = null;
    [SerializeField]
    private ReportData identifyTargetsReportData = null;
    [SerializeField]
    private ReportData scoutDefReportData = null;
    [SerializeField]
    private ReportData submarineDefReportData = null;
    [SerializeField]
    private ReportData capData = null;
    [SerializeField]
    private string attackFoundNoEnemy = null;
    [SerializeField]
    private string nothingReportData = null;
    [SerializeField]
    private string strikeGroupReportData = null;
    [SerializeField]
    private string outpostReportData = null;
    [SerializeField]
    private string whalesReportData = null;
    [SerializeField]
    private string survivorsReportData = null;
    [SerializeField]
    private string scoutingData = null;
    [SerializeField]
    private string capTitle = null;

    [SerializeField]
    private GameObject container = null;
    [SerializeField]
    private List<ReportObject> enemyShips = null;
    [SerializeField]
    private List<ReportObject> identifiedEnemyShips = null;
    [SerializeField]
    private List<ReportObject> playerShips = null;
    [SerializeField]
    private Sprite notDead = null;
    [SerializeField]
    private Sprite dead = null;
    [SerializeField]
    private Sprite damaged = null;
    [SerializeField]
    private Sprite detected = null;
    [SerializeField]
    private Sprite japanFlag = null;
    [SerializeField]
    private Sprite usaFlag = null;
    [SerializeField]
    private Text airAttackDescription = null;
    [SerializeField]
    private Text onlyDesc = null;
    [SerializeField]
    private Text identifyTargetDescription = null;
    [SerializeField]
    private GameObject airAttack = null;
    [SerializeField]
    private GameObject onlyDescription = null;
    [SerializeField]
    private GameObject identifyTarget = null;
    [SerializeField]
    private ReconReport recon = null;
    [SerializeField]
    private Button confirmButton = null;
    [SerializeField]
    private Button skipButton = null;
    [SerializeField]
    private Text missionName = null;
    [SerializeField]
    private Text date = null;
    [SerializeField]
    private Image missionTypeImage = null;

    [SerializeField]
    private string bomber = "Bomber";
    [SerializeField]
    private string fighter = "Fighter";
    [SerializeField]
    private string torpedoBomber = "TorpedoBomber";

    [SerializeField]
    private GameObject blocker = null;

    private CasualtiesData casualtiesData;
    private TacticalMission mission;
    private LocalizationManager locMan;
    private ETacticalObjectType enemyType;
    private RectTransform containerRect;
    private TacticManager tacMan;

    private int lastTimeIndex;

    private string durabilityText = "AirAttackEnemyDurabilityDamage";

    private void Awake()
    {
        Instance = this;

        locMan = LocalizationManager.Instance;

        bomber = locMan.GetText(bomber);
        fighter = locMan.GetText(fighter);
        torpedoBomber = locMan.GetText(torpedoBomber);
        airAttackReportData.Minor = locMan.GetText(airAttackReportData.Minor);
        airAttackReportData.Medium = locMan.GetText(airAttackReportData.Medium);
        airAttackReportData.Massive = locMan.GetText(airAttackReportData.Massive);
        airAttackEnemyReportData.Minor = locMan.GetText(airAttackEnemyReportData.Minor);
        airAttackEnemyReportData.Medium = locMan.GetText(airAttackEnemyReportData.Medium);
        airAttackEnemyReportData.Massive = locMan.GetText(airAttackEnemyReportData.Massive);
        reconReportData.BadText = locMan.GetText(reconReportData.BadText);
        reconReportData.GoodText = locMan.GetText(reconReportData.GoodText);
        identifyTargetsReportData.BadText = locMan.GetText(identifyTargetsReportData.BadText);
        identifyTargetsReportData.GoodText = locMan.GetText(identifyTargetsReportData.GoodText);
        scoutDefReportData.BadText = locMan.GetText(scoutDefReportData.BadText);
        scoutDefReportData.GoodText = locMan.GetText(scoutDefReportData.GoodText);
        submarineDefReportData.BadText = locMan.GetText(submarineDefReportData.BadText);
        submarineDefReportData.GoodText = locMan.GetText(submarineDefReportData.GoodText);
        attackFoundNoEnemy = locMan.GetText(attackFoundNoEnemy);
        nothingReportData = locMan.GetText(nothingReportData);
        strikeGroupReportData = locMan.GetText(strikeGroupReportData);
        outpostReportData = locMan.GetText(outpostReportData);
        whalesReportData = locMan.GetText(whalesReportData);
        capData.GoodText = locMan.GetText(capData.GoodText);
        capData.BadText = locMan.GetText(capData.BadText);
        scoutingData = locMan.GetText(scoutingData);
        capTitle = locMan.GetText(capTitle);
        survivorsReportData = locMan.GetText(survivorsReportData);

        skipButton.onClick.AddListener(Confirm);
        confirmButton.onClick.AddListener(StartAnimation);

        containerRect = container.GetComponent<RectTransform>();
        container.SetActive(false);

        durabilityText = locMan.GetText(durabilityText);
    }

    private void Start()
    {
        tacMan = TacticManager.Instance;
        ReportManager.Instance.AnimFinished += () => ReportFinished();
    }

    public void LoadLastSpeed(int speed)
    {
        lastTimeIndex = speed;
    }

    ///AirAttack
    public void Setup(TacticalEnemyShip enemyShip, CasualtiesData casData, TacticalMission mission, bool durabilityChanged)
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[mission.OrderType].MissionSprite;
        missionName.text = tacMan.MissionInfo[mission.OrderType].MissionName;
        string resultDescription = "";
        if (enemyShip)
        {
            this.mission = mission;
            confirmButton.gameObject.SetActive(true);
            enemyType = enemyShip.Type;
            casualtiesData = casData;
            int deadShips = 0;
            int allEnemyShips = enemyShip.Blocks.Count;
            for (int i = 0; i < enemyShip.Blocks.Count; i++)
            {
                var data = enemyShip.Blocks[i];
                if (data.Dead)
                {
                    enemyShips[i].Setup(dead, japanFlag, data.Data.LocalizedName, 0, data.Data.Durability);
                    deadShips++;
                }
                else if (data.Visible)
                {
                    enemyShips[i].Setup(notDead, japanFlag, data.Data.LocalizedName, data.CurrentDurability, data.Data.Durability);
                }
            }
            int index = 0;
            int survivedBombers = mission.Bombers - casualtiesData.SquadronsDestroyed[EPlaneType.Bomber] - casualtiesData.SquadronsBroken[EPlaneType.Bomber];
            int survivedFighters = mission.Fighters - casualtiesData.SquadronsDestroyed[EPlaneType.Fighter] - casualtiesData.SquadronsBroken[EPlaneType.Fighter];
            int survivedTorpedoes = mission.Torpedoes - casualtiesData.SquadronsDestroyed[EPlaneType.TorpedoBomber] - casualtiesData.SquadronsBroken[EPlaneType.TorpedoBomber];
            SetupPlayerPlanes(survivedBombers, bomber, notDead, ref index);
            SetupPlayerPlanes(survivedFighters, fighter, notDead, ref index);
            SetupPlayerPlanes(survivedTorpedoes, torpedoBomber, notDead, ref index);
            SetupDestroyedPlayerPlanes(EPlaneType.Bomber, bomber, dead, ref index);
            SetupDestroyedPlayerPlanes(EPlaneType.Fighter, fighter, dead, ref index);
            SetupDestroyedPlayerPlanes(EPlaneType.TorpedoBomber, torpedoBomber, dead, ref index);
            SetupBrokenPlayerPlanes(EPlaneType.Bomber, bomber, damaged, ref index);
            SetupBrokenPlayerPlanes(EPlaneType.Fighter, fighter, damaged, ref index);
            SetupBrokenPlayerPlanes(EPlaneType.TorpedoBomber, torpedoBomber, damaged, ref index);

            int allPlayerSquadrons = mission.SentSquadrons.Count;
            int survivedSquadrons = survivedBombers + survivedFighters + survivedTorpedoes;

            AddCheckResults(survivedSquadrons, allPlayerSquadrons, ref resultDescription, airAttackReportData);
            AddCheckResultsEnemy(allEnemyShips - deadShips, allEnemyShips - deadShips + casualtiesData.EnemyDestroyed.Count, ref resultDescription, airAttackEnemyReportData, durabilityChanged);

        }
        else
        {
            resultDescription += attackFoundNoEnemy;
        }
        airAttackDescription.text = resultDescription;
        airAttack.SetActive(true);
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    public void AttackFoundNoEnemySetup(EMissionOrderType orderType)
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[orderType].MissionSprite;
        missionName.text = tacMan.MissionInfo[orderType].MissionName;
        var resultDescription = "";
        resultDescription += attackFoundNoEnemy;
        onlyDesc.text = resultDescription;
        onlyDescription.SetActive(true);
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    ///SubmarineHunt
    public void SubmarineDefSetup()
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[EMissionOrderType.SubmarineHunt].MissionSprite;
        missionName.text = tacMan.MissionInfo[EMissionOrderType.SubmarineHunt].MissionName;
        var resultDescription = "";
        resultDescription += submarineDefReportData.GoodText;
        onlyDesc.text = resultDescription;
        onlyDescription.SetActive(true);
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    ///CounterHostileScouts
    public void ScoutDefSetup()
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[EMissionOrderType.CounterHostileScouts].MissionSprite;
        missionName.text = tacMan.MissionInfo[EMissionOrderType.CounterHostileScouts].MissionName;
        var resultDescription = "";
        resultDescription += scoutDefReportData.GoodText;
        onlyDesc.text = resultDescription;
        onlyDescription.SetActive(true);
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    ///Recon
    public void SetupRecon(EMissionOrderType type, List<ITacticalObjectHelper> objects)
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[type].MissionSprite;
        missionName.text = tacMan.MissionInfo[type].MissionName;
        recon.SetupRecon(objects, this);
        recon.gameObject.SetActive(true);
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    ///Identify target
    public void SetupIdentifyTarget(TacticalObject obj, EMissionOrderType type)
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[type].MissionSprite;
        missionName.text = tacMan.MissionInfo[type].MissionName;
        string resultDescription;
        if (obj is TacticalEnemyShip enemyShip && enemyShip.Side == ETacticalObjectSide.Enemy)
        {
            int index = 0;
            foreach (EnemyManeuverInstanceData data in enemyShip.Blocks)
            {
                if (!data.Dead && data.Visible)
                {
                    identifiedEnemyShips[index].Setup(data.WasDetected ? notDead : detected, japanFlag, data.Data.LocalizedName, data.CurrentDurability, data.Data.Durability);
                }
                else if (data.Dead)
                {
                    identifiedEnemyShips[index].Setup(dead, japanFlag, data.Data.LocalizedName, 0, data.Data.Durability);
                }
                index++;
            }
            resultDescription = identifyTargetsReportData.GoodText;
            identifyTargetDescription.text = resultDescription;
            identifyTarget.SetActive(true);
        }
        else
        {
            switch (obj.Type)
            {
                case ETacticalObjectType.Nothing:
                    resultDescription = nothingReportData;
                    break;
                case ETacticalObjectType.Outpost:
                    resultDescription = outpostReportData;
                    break;
                case ETacticalObjectType.StrikeGroup:
                    resultDescription = strikeGroupReportData;
                    break;
                case ETacticalObjectType.Whales:
                    resultDescription = whalesReportData;
                    break;
                case ETacticalObjectType.Survivors:
                    resultDescription = survivorsReportData;
                    break;
                default:
                    resultDescription = "";
                    break;
            }
            onlyDescription.SetActive(true);
            onlyDesc.text = resultDescription;
            //resultDescription += identifyTargetsReportData.BadText;
        }
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    public void IdentifyFoundNothingSetup(EMissionOrderType type)
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[type].MissionSprite;
        missionName.text = tacMan.MissionInfo[type].MissionName;
        onlyDescription.SetActive(true);
        onlyDesc.text = nothingReportData;
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    ///CAP
    public void SetupCAP(bool hadFight)
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[EMissionOrderType.CarriersCAP].MissionSprite;
        missionName.text = capTitle;
        onlyDescription.SetActive(true);
        onlyDesc.text = hadFight ? capData.BadText : capData.GoodText;
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    ///Scouting
    public void SetupScouting()
    {
        Reset();
        missionTypeImage.sprite = tacMan.MissionInfo[EMissionOrderType.Scouting].MissionSprite;
        missionName.text = tacMan.MissionInfo[EMissionOrderType.Scouting].MissionName;
        onlyDescription.SetActive(true);
        onlyDesc.text = scoutingData;
        RebuildPanel();
        container.SetActive(true);

        HudManager.Instance.PopupShown(this);
    }

    public void Confirm()
    {
        Hide();
        ReportFinished();
    }

    public void Hide()
    {
        container.SetActive(false);
        blocker.SetActive(false);

        var hudMan = HudManager.Instance;
        hudMan.ChangeTimeSpeed(lastTimeIndex);
        hudMan.PopupHidden(this);
        if (mission != null)
        {
            mission.CheckSquadronsLeft();
        }
    }

    private void AddCheckResults(int survived, int all, ref string resultDescription, ReportMissionData reportData)
    {
        resultDescription += " ";
        if (survived == all || all == 0)
        {
            resultDescription += reportData.Minor;
        }
        else if (survived > 0)
        {
            resultDescription += reportData.Medium;
        }
        else
        {
            resultDescription += reportData.Massive;
        }
    }

    private void AddCheckResultsEnemy(int survived, int all, ref string resultDescription, ReportMissionData reportData, bool durabilityChanged)
    {
        resultDescription += " ";
        if (survived == all)
        {
            resultDescription += durabilityChanged ? durabilityText : reportData.Minor;
        }
        else if (survived > 0)
        {
            resultDescription += reportData.Medium;
        }
        else
        {
            resultDescription += reportData.Massive;
        }
    }

    private void SetupPlayerPlanes(int i, string text, Sprite bg, ref int index)
    {
        int startIndex = index;
        for (; index < i + startIndex; index++)
        {
            playerShips[index].Setup(bg, usaFlag, text, 0, 0);
        }
    }

    private void SetupDestroyedPlayerPlanes(EPlaneType type, string text, Sprite bg, ref int index)
    {
        int startIndex = index;
        for (; index < casualtiesData.SquadronsDestroyed[type] + startIndex; index++)
        {
            playerShips[index].Setup(bg, usaFlag, text, 0, 0);
        }
    }

    private void SetupBrokenPlayerPlanes(EPlaneType type, string text, Sprite bg, ref int index)
    {
        int startIndex = index;
        for (; playerShips.Count > index && index < (casualtiesData.SquadronsBroken[type] + startIndex); index++)
        {
            playerShips[index].Setup(bg, usaFlag, text, 0, 0);
        }
    }

    private void StartAnimation()
    {
        Hide();
        if (mission != null && casualtiesData != null)
        {
            ReportManager.Instance.PlayReplay(mission, casualtiesData, enemyType == ETacticalObjectType.Outpost);
        }
    }

    private void RebuildPanel()
    {
        this.StartCoroutineActionAfterFrames(() => LayoutRebuilder.MarkLayoutForRebuild(containerRect), 1);
    }

    private void Reset()
    {
        blocker.SetActive(true);

        var hudMan = HudManager.Instance;
        lastTimeIndex = hudMan.TimeIndex;
        hudMan.OnPausePressed();

        confirmButton.gameObject.SetActive(false);
        mission = null;
        casualtiesData = null;
        onlyDescription.SetActive(false);
        airAttack.SetActive(false);
        identifyTarget.SetActive(false);
        recon.gameObject.SetActive(false);
        foreach (var obj in enemyShips)
        {
            obj.gameObject.SetActive(false);
        }
        foreach (var obj in playerShips)
        {
            obj.gameObject.SetActive(false);
        }
        foreach (var obj in identifiedEnemyShips)
        {
            obj.gameObject.SetActive(false);
        }
        var timeMan = TimeManager.Instance;
        date.text = timeMan.CurrentMonth.ToString("00") + "/" + timeMan.CurrentDay.ToString("00") + "/" + timeMan.CurrentYear + ", " + timeMan.CurrentHour + ":" + timeMan.CurrentMinute.ToString("00");
    }
}
