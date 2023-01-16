using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.VFX;

public class ReportManager : MonoBehaviour, IPopupPanel
{
    public event System.Action AnimFinished = delegate { };

    public static ReportManager Instance;

    public EWindowType Type => EWindowType.Other;

    [SerializeField]
    private List<ReplayData> replayAnims = null;
    [SerializeField]
    private List<ReplayData> replayAnimsOutpost = null;
    [SerializeField]
    private List<GameObject> bombersI = null;
    [SerializeField]
    private List<GameObject> bombersII = null;
    [SerializeField]
    private List<GameObject> bombersIII = null;
    [SerializeField]
    private List<GameObject> fightersI = null;
    [SerializeField]
    private List<GameObject> fightersII = null;
    [SerializeField]
    private List<GameObject> fightersIII = null;
    [SerializeField]
    private List<GameObject> torpedoI = null;
    [SerializeField]
    private List<GameObject> torpedoII = null;
    [SerializeField]
    private List<GameObject> torpedoIII = null;

    [SerializeField]
    private List<GameObject> shipsSlot1 = null;
    [SerializeField]
    private List<GameObject> shipsSlot2 = null;
    [SerializeField]
    private List<GameObject> shipsSlot3 = null;
    [SerializeField]
    private List<GameObject> shipsSlot4 = null;

    private List<List<GameObject>> allShipsSlots;

    [SerializeField]
    private GameObject reportCamera = null;
    [SerializeField]
    private List<GameObject> canvases = null;
    [SerializeField]
    private GameObject prefab = null;
    [SerializeField]
    private List<GameObject> turnOffOnReportList = null;
    [SerializeField]
    private List<GameObject> disableAfterReportList = null;
    [SerializeField]
    private List<GameObject> fullHDBars = null;
    [SerializeField]
    private StudioEventEmitter music = null;

    private List<EReplayValue> replayList;
    private Dictionary<EReplayValue, ReplayData> replayDict;
    private Dictionary<EReplayValue, ReplayData> replayDictOutpost;
    private Dictionary<EReplayValue, ReplayData> currentDict;

    private int bomberLv;
    private int fighterLv;
    private int torpedoLv;

    private List<GameObject> currentSquadron;
    private List<GameObject> introSquadron;
    private List<GameObject> squadron;
    private int currentEnemyToShow;
    private int nextEnemyToShow;

    private List<List<GameObject>> bombers;
    private List<List<GameObject>> fighters;
    private List<List<GameObject>> torpedoes;

    private List<List<GameObject>> lossesSquadron;

    private bool hasAnim;
    private List<EnemyManeuverInstanceData> missedShips;
    private List<VisualEffect> visualEffects = new List<VisualEffect>();

    private ECameraView prevView;

    private void Awake()
    {
        Instance = this;

        replayDict = new Dictionary<EReplayValue, ReplayData>();
        foreach (var data in replayAnims)
        {
            replayDict[data.Type] = data;
        }
        replayDictOutpost = new Dictionary<EReplayValue, ReplayData>();
        foreach (var data in replayAnimsOutpost)
        {
            replayDictOutpost[data.Type] = data;
        }
        replayList = new List<EReplayValue>();

        allShipsSlots = new List<List<GameObject>> { shipsSlot1, shipsSlot2, shipsSlot3, shipsSlot4 };

        bombers = new List<List<GameObject>> { bombersI, bombersII, bombersIII };
        fighters = new List<List<GameObject>> { fightersI, fightersII, fightersIII };
        torpedoes = new List<List<GameObject>> { torpedoI, torpedoII, torpedoIII };

        lossesSquadron = new List<List<GameObject>>();

        missedShips = new List<EnemyManeuverInstanceData>();

        visualEffects.AddRange(prefab.GetComponentsInChildren<VisualEffect>(true));
    }

    private void Update()
    {
        foreach (var effect in visualEffects)
        {
            if (effect.gameObject.activeInHierarchy)
            {
                effect.Simulate(Time.unscaledDeltaTime);
            }
        }
    }

    private void LateUpdate()
    {
        if (hasAnim && Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void Setup(int bomberLv, int fighterLv, int torpedoLv)
    {
        for (int i = 0; i < bombers.Count; i++)
        {
            SetShowObjects(bombers[i], false);
            SetShowObjects(fighters[i], false);
            SetShowObjects(torpedoes[i], false);
        }

        this.bomberLv = bomberLv;
        this.fighterLv = fighterLv;
        this.torpedoLv = torpedoLv;
    }

    public void PlayReplay(TacticalMission mission, CasualtiesData data, bool outpost)
    {
        var camMan = CameraManager.Instance;
        prevView = camMan.CurrentCameraView;
        camMan.ForceSwitchCamera(ECameraView.PreviewCamera);
        camMan.SwitchPostprocessVolume(true);
        camMan.SetClippingPlanes(outpost);

        foreach (var list in replayAnims)
        {
            foreach (var obj in list.AnimationList)
            {
                obj.gameObject.SetActive(false);
            }
        }
        foreach (var list in replayAnimsOutpost)
        {
            foreach (var obj in list.AnimationList)
            {
                obj.gameObject.SetActive(false);
            }
        }

        hasAnim = true;
        music.Play();

        var hudMan = HudManager.Instance;
        hudMan.OnPausePressed();
        hudMan.SetAcceptInput(false);
        hudMan.SetReportPlaying(true);
        hudMan.PopupShown(this);

        prefab.SetActive(true);
        foreach (var go in turnOffOnReportList)
        {
            go.SetActive(false);
        }

        foreach (var canvas in canvases)
        {
            canvas.SetActive(false);
        }

        foreach (var bar in fullHDBars)
        {
            bar.SetActive(true);
        }
        StopAllCoroutines();

        int slot = 0;
        foreach (var block in mission.EnemyShip.Blocks)
        {
            if (!block.Dead)
            {
                ShowEnemy(block.Data.ShipTypeIndex, slot++);
                if (slot >= allShipsSlots.Count)
                {
                    break;
                }
            }
        }
        for (; slot < allShipsSlots.Count; slot++)
        {
            ShowEnemy(-1, slot);
        }

        int torpedoesCount;
        if (outpost)
        {
            torpedoesCount = 0;
            currentDict = replayDictOutpost;
        }
        else
        {
            torpedoesCount = mission.Torpedoes;
            currentDict = replayDict;
        }

        replayList.Clear();
        lossesSquadron.Clear();
        bool hasBombers = mission.Bombers > 0;
        bool hasTorpedoes = torpedoesCount > 0;
        if (hasBombers || hasTorpedoes)
        {
            bool chooseBombers = mission.Bombers > torpedoesCount;
            if (mission.Bombers == torpedoesCount)
            {
                chooseBombers = Random.value > .5f;
            }

            int count;
            if (chooseBombers)
            {
                squadron = this.bombers[bomberLv];
                count = mission.Bombers;
            }
            else
            {
                squadron = this.torpedoes[torpedoLv];
                count = mission.Torpedoes;
            }

            float chance = ((hasBombers && hasTorpedoes) ? .66f : .5f);
            if (mission.Fighters > count || (mission.Fighters == count && (Random.value > chance)))
            {
                introSquadron = this.fighters[fighterLv];
            }
            else
            {
                introSquadron = squadron;
            }

            EReplayValue hit;
            EReplayValue miss;
            if (chooseBombers)
            {
                hit = EReplayValue.EnemyHitBomber;
                miss = EReplayValue.EnemyMissBomber;
            }
            else
            {
                hit = EReplayValue.EnemyHitTorpedo;
                miss = EReplayValue.EnemyMissTorpedo;
            }

            int index = Mathf.Clamp(mission.SelectedObjectIndex, 0, mission.EnemyShip.Blocks.Count - 1);
            var enemyBlock = mission.EnemyShip.Blocks[index];
            if (!data.EnemyDestroyed.Contains(enemyBlock.Data) && data.EnemyDestroyed.Count > 0)
            {
                foreach (var enemy2 in mission.EnemyShip.Blocks)
                {
                    if (enemy2.Data == data.EnemyDestroyed[0])
                    {
                        enemyBlock = enemy2;
                    }
                }
            }

            currentEnemyToShow = enemyBlock.Data.ShipTypeIndex;
            nextEnemyToShow = currentEnemyToShow;

            if (data.EnemyDestroyedIndices.Count == 0)
            {
                replayList.Add(miss);
                replayList.Add(miss);
            }
            else if (mission.EnemyShip.Dead)
            {
                replayList.Add(hit);
                if (data.EnemyDestroyedIndices.Count > 1)
                {
                    replayList.Add(hit);
                    data.EnemyDestroyed.Remove(enemyBlock.Data);
                    nextEnemyToShow = RandomUtils.GetRandom(data.EnemyDestroyed).ShipTypeIndex;
                }
            }
            else
            {
                replayList.Add(hit);
                replayList.Add(miss);

                missedShips.Clear();
                foreach (var block in mission.EnemyShip.Blocks)
                {
                    if (!block.Dead)
                    {
                        missedShips.Add(block);
                    }
                }
                if (missedShips.Count > 0)
                {
                    nextEnemyToShow = RandomUtils.GetRandom(missedShips).Data.ShipTypeIndex;
                }
            }
        }
        else
        {
            squadron = null;
            introSquadron = this.fighters[fighterLv];
        }

        int bombers = data.SquadronsBroken[EPlaneType.Bomber] + data.SquadronsDestroyed[EPlaneType.Bomber];
        int fighters = data.SquadronsBroken[EPlaneType.Fighter] + data.SquadronsDestroyed[EPlaneType.Fighter];
        int torpedoes = data.SquadronsBroken[EPlaneType.TorpedoBomber] + data.SquadronsDestroyed[EPlaneType.TorpedoBomber];
        int sumLosses = bombers + fighters + torpedoes;
        if (sumLosses > 0)
        {
            int sum = mission.GetPlanesCount();
            replayList.Add(EReplayValue.LostSquadrons);
            Rand(sumLosses, bombers, fighters);
            if (sum == sumLosses && sumLosses > 1)
            {
                replayList.Add(EReplayValue.LostSquadrons);
                Rand(sumLosses, bombers, fighters);
            }
        }

        currentSquadron = introSquadron;
        SetupEnemies(mission.EnemyShip, data);
        var anim = GetAnim(EReplayValue.Intro, null);
        StartCoroutine(NextAnim(anim, 0));
    }

    public void Hide()
    {
        if (hasAnim)
        {
            FinishAnim();
        }
        CameraManager.Instance.SwitchPostprocessVolume(false);
    }

    private void FinishAnim()
    {
        hasAnim = false;
        music.Stop();

        var hudMan = HudManager.Instance;
        hudMan.SetAcceptInput(true);
        hudMan.SetReportPlaying(false);
        hudMan.PlayLastSpeed();
        hudMan.PopupHidden(this);

        var camMan = CameraManager.Instance;
        if (prevView != ECameraView.PreviewCamera)
        {
            camMan.SwitchMode(prevView);
        }

        prefab.SetActive(false);
        foreach (var go in turnOffOnReportList)
        {
            go.SetActive(true);
        }

        foreach (var go in disableAfterReportList)
        {
            go.SetActive(false);
        }

        reportCamera.SetActive(false);
        foreach (var canvas in canvases)
        {
            canvas.SetActive(true);
        }

        if (camMan.CurrentCameraView != ECameraView.PreviewCamera)
        {
            foreach (var bar in fullHDBars)
            {
                bar.SetActive(false);
            }
        }

        StopAllCoroutines();
        AnimFinished();
        CameraManager.Instance.SwitchPostprocessVolume(false);
    }

    private void SetShowObjects(List<GameObject> objs, bool show)
    {
        foreach (var obj in objs)
        {
            obj.SetActive(show);
        }
    }

    private PlayableDirector GetAnim(EReplayValue type, PlayableDirector prevAnim)
    {
        if (currentDict.TryGetValue(type, out ReplayData data))
        {
            if (data.AnimationList.Count > 0)
            {
                if (type != EReplayValue.Intro)
                {
                    ShowEnemy(currentEnemyToShow);
                }
                currentEnemyToShow = nextEnemyToShow;

                if (type == EReplayValue.LostSquadrons)
                {
                    if (lossesSquadron.Count > 0)
                    {
                        currentSquadron = lossesSquadron[0];
                        lossesSquadron.RemoveAt(0);
                    }
                    else
                    {
                        return null;
                    }
                }

                SetShowObjects(bombers[bomberLv], false);
                SetShowObjects(fighters[fighterLv], false);
                SetShowObjects(torpedoes[torpedoLv], false);
                SetShowObjects(currentSquadron, true);
                currentSquadron = squadron;

                int animIndex = data.AnimationList.IndexOf(prevAnim);
                if (animIndex != -1)
                {
                    data.AnimationList.RemoveAt(animIndex);
                }

                var anim = data.AnimationList[Random.Range(0, data.AnimationList.Count)];
                anim.gameObject.SetActive(false);
                anim.gameObject.SetActive(true);

                if (animIndex != -1)
                {
                    data.AnimationList.Add(prevAnim);
                }

                return anim;
            }
        }
        return null;
    }

    private void ShowEnemy(int index)
    {
        foreach (var slot in allShipsSlots)
        {
            for (int i = 0; i < slot.Count; i++)
            {
                slot[i].SetActive(i == index);
            }
        }
    }

    private void ShowEnemy(int index, int slot)
    {
        var ships = allShipsSlots[slot];
        for (int i = 0; i < ships.Count; i++)
        {
            ships[i].SetActive(i == index);
        }
    }

    private void SetupEnemies(TacticalEnemyShip ship, CasualtiesData data)
    {
        var blocks = ship.Blocks;
        int slotIndex = 0;
        int shift = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (slotIndex < 4)
            {
                if (blocks[i].Dead && !data.EnemyDestroyedIndices.Contains(i))
                {
                    if ((blocks.Count - shift) < 5)
                    {
                        foreach (var slot in allShipsSlots[slotIndex])
                        {
                            slot.SetActive(false);
                        }
                        slotIndex++;
                    }
                    else
                    {
                        shift++;
                    }
                }
                else
                {
                    ShowEnemyWithType(slotIndex, blocks[i].Data.ShipTypeIndex);
                    slotIndex++;
                }
            }
        }
    }

    private void ShowEnemyWithType(int slotIndex, int shipTypeIndex)
    {
        var slots = allShipsSlots[slotIndex];
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetActive(i == shipTypeIndex);
        }
    }

    private void Rand(int sumLosses, int bombers, int fighters)
    {
        int rand = Random.Range(0, sumLosses);
        rand -= bombers;
        if (rand < 0)
        {
            lossesSquadron.Add(this.bombers[bomberLv]);
        }
        rand -= fighters;
        if (rand < 0)
        {
            lossesSquadron.Add(this.fighters[fighterLv]);
        }
        else
        {
            lossesSquadron.Add(torpedoes[torpedoLv]);
        }
    }

    private IEnumerator NextAnim(PlayableDirector prevAnim, int index)
    {
        reportCamera.SetActive(true);
        if (prevAnim != null)
        {
            yield return new WaitForSecondsRealtime((float)prevAnim.duration);
        }

        if (replayList.Count > index)
        {
            var newAnim = GetAnim(replayList[index], prevAnim);
            StartCoroutine(NextAnim(newAnim, index + 1));
            yield break;
        }
        FinishAnim();
    }
}
