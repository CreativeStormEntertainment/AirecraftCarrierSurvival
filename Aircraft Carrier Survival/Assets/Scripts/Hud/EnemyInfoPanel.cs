using FMODUnity;
using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoPanel : MonoBehaviour
{
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text defText = null;
    [SerializeField]
    private Text attText = null;

    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Sprite fleet = null;
    [SerializeField]
    private Sprite outpost = null;
    [SerializeField]
    private Sprite friendlyFleet = null;
    [SerializeField]
    private Sprite friendlyOutpost = null;
    [SerializeField]
    private Sprite specialOutpost = null;

    [SerializeField]
    private RectTransform panel = null;

    [SerializeField]
    private List<EnemyInfoPanelObject> enemyShips = null;

    [SerializeField]
    private StudioEventEmitter showSound = null;

    [SerializeField]
    private Text outdatedReportText = null;

    private RectTransform canvasRect;
    private bool init;

    private void Awake()
    {
        Init();
    }

    public void ShowPanel(TacticalEnemyShip enemyShip, int outdatedTicks)
    {
        Init();
        ResetEnemyShips();
        if (!enemyShip.Dead)
        {
            bool isFriendly = enemyShip.Side != ETacticalObjectSide.Enemy;
            if (enemyShip.Type == ETacticalObjectType.StrikeGroup)
            {
                image.sprite = isFriendly ? friendlyFleet : fleet;
            }
            else
            {
                if (enemyShip.Special)
                {
                    image.sprite = specialOutpost;
                }
                else if (isFriendly)
                {
                    image.sprite = friendlyOutpost;
                }
                else
                {
                    image.sprite = outpost;
                }
            }
            title.text = enemyShip.LocalizedName;

            for (int i = 0; i < enemyShip.Blocks.Count; i++)
            {
                enemyShips[i].Setup(enemyShip.Blocks[i], isFriendly);
            }
            CalculateEnemy(enemyShip, ECalculateType.TestMin, out var dataMin);
            CalculateEnemy(enemyShip, ECalculateType.TestMax, out var dataMax);
            defText.text = $"{dataMin.Defense} - {dataMax.Defense}";
            attText.text = $"{dataMin.Attack} - {dataMax.Attack}";

            gameObject.SetActive(true);
            showSound.Play();
            LayoutRebuilder.MarkLayoutForRebuild(panel);
            enemyShip.StartCoroutineActionAfterFrames(SetPanelPos, 1);

            if (outdatedTicks > 0)
            {
                outdatedReportText.gameObject.SetActive(true);
                var markers = TacticManager.Instance.Markers;
                outdatedReportText.text = $"{markers.LastReport} {outdatedTicks / TimeManager.Instance.TicksForHour} {markers.HoursAgo}";
            }
            else
            {
                outdatedReportText.gameObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Init()
    {
        if (!init)
        {
            init = true;
            var parent = panel.parent;
            while (parent.parent != null)
            {
                parent = parent.parent;
            }
            canvasRect = parent.GetComponent<RectTransform>();
        }
    }

    private void ResetEnemyShips()
    {
        foreach (var ship in enemyShips)
        {
            ship.gameObject.SetActive(false);
        }
    }

    private void SetPanelPos()
    {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, panel.sizeDelta.x / 2f, Screen.width - canvasRect.lossyScale.x * panel.sizeDelta.x / 2f),
            Mathf.Clamp(transform.position.y, panel.sizeDelta.y / 2f, Screen.height - canvasRect.lossyScale.y * panel.sizeDelta.y / 2f));
    }

    private void CalculateEnemy(TacticalEnemyShip enemy, ECalculateType type, out AttackParametersData data)
    {
        var modifiers = Parameters.Instance.DifficultyParams;
        ManeuverCalculator.CalculateEnemy(GetBlocks(enemy), GetDurability(enemy), out _, out data, type, modifiers.EnemyBlocksAttackModifier, modifiers.EnemyBlocksDefenseModifier);
    }

    private IEnumerable<EnemyManeuverData> GetBlocks(TacticalEnemyShip enemy)
    {
        foreach (var block in enemy.Blocks)
        {
            if (!block.Dead)
            {
                yield return block.Data;
            }
        }
    }

    private IEnumerable<int> GetDurability(TacticalEnemyShip enemy)
    {
        foreach (var block in enemy.Blocks)
        {
            if (!block.Dead)
            {
                yield return block.CurrentDurability;
            }
        }
    }
}
