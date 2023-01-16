using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EnemyStrategyPanel : MonoBehaviour
{
    public bool AllEnemyShipBlocksVisible
    {
        get;
        private set;
    }

    public List<EnemyManeuverData> EnemyShipDatas => enemyShipDatas;

    [SerializeField]
    private List<EnemyStrategyObject> strategyObjects = null;
    [SerializeField]
    private Text magicSpriteText = null;

    private List<EnemyStrategyObject> enemyStrategyObjects = new List<EnemyStrategyObject>();
    private int selectedObject;

    private List<EnemyManeuverData> enemyShipDatas = new List<EnemyManeuverData>();
    private List<EnemyManeuverData> alternativeShipDatas = new List<EnemyManeuverData>();
    private List<int> durabilities = new List<int>();

    private bool shouldShowSprite;
    private StrategySelectionPanel strategySelectionPanel;
    private Dictionary<int, List<int>> strategyTargets;

    public void Init()
    {
        strategyTargets = new Dictionary<int, List<int>>();
        foreach (var obj in strategyObjects)
        {
            obj.Init();
        }
    }

    public void Setup(TacticalEnemyShip enemyShip, StrategySelectionPanel strategyPanel)
    {
        Clear();

        strategySelectionPanel = strategyPanel;
        selectedObject = -1;

        ObjectiveObject objective = null;
        if (shouldShowSprite && enemyShip.Blocks[0].Visible && !enemyShip.Blocks[0].Dead)
        {
            foreach (var pair in strategyTargets)
            {
                if (pair.Value.Contains(enemyShip.Id))
                {
                    objective = ObjectivesManager.Instance.GetObjectiveObject(pair.Key);
                    break;
                }
            }
        }
        if (objective == null)
        {
            magicSpriteText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            magicSpriteText.transform.parent.gameObject.SetActive(true);
            magicSpriteText.text = objective.ObjectiveNumberText.text;
        }

        if (enemyShip == null)
        {
            for (int i = 0; i < strategyObjects.Count; i++)
            {
                strategyObjects[i].Setup(null, i, this, i);
            }
            AllEnemyShipBlocksVisible = false;
        }
        else
        {
            int visibleBlocksCount = 0;
            int aliveBlocks = 0;
            for (int i = 0; i < enemyShip.Blocks.Count; i++)
            {
                var obj = strategyObjects[i];
                var block = enemyShip.Blocks[i];
                enemyStrategyObjects.Add(obj);
                if (block.Visible)
                {
                    visibleBlocksCount++;
                }
                if (block.Dead)
                {
                    obj.Setup(block, i, this, -1);
                }
                else
                {
                    int index = aliveBlocks;
                    aliveBlocks++;
                    if (selectedObject == -1)
                    {
                        selectedObject = i;
                        TacticManager.Instance.SelectedObjectIndex = i;
                        obj.SetSelected(true);
                    }

                    enemyShipDatas.Add(block.Data);
                    durabilities.Add(block.CurrentDurability);
                    alternativeShipDatas.Add(null);

                    if (block.Visible)
                    {
                        obj.Setup(block, i, this, index);
                    }
                    else
                    {
                        if (block.Data == block.Alternative || block.Alternative == null)
                        {
                            Debug.LogError(block.Data.name + " --- error3");
                        }

                        alternativeShipDatas[index] = block.Alternative;
                        bool alternativeLeft = Random.value > .5f;
                        if (alternativeLeft)
                        {
                            SwitchTypeInner(index);
                        }

                        obj.Setup(block, block.Alternative, alternativeLeft, i, this, index);
                    }
                }
            }
            AllEnemyShipBlocksVisible = visibleBlocksCount == enemyShip.Blocks.Count;
        }
        if (selectedObject != -1)
        {
            enemyStrategyObjects[selectedObject].SetSelected(true);
        }
    }

    public void SetShowHighlight(bool show, int enemyIndex)
    {
        if (enemyIndex != -1)
        {
            strategyObjects[enemyIndex].Outline.SetActive(show);
        }
    }

    public void SetShowSprite(bool show, IEnumerable<int> targets, int objective)
    {
        if (show)
        {
            Assert.IsTrue(strategyTargets.Count < 4);
            if (strategyTargets.TryGetValue(objective, out var list))
            {
                Assert.IsTrue(false);
                list.Clear();
                list.AddRange(targets);
            }
            else
            {
                strategyTargets[objective] = new List<int>(targets);
            }
        }
        else
        {
            strategyTargets.Remove(objective);
        }
        shouldShowSprite = show;
    }

    public void SaveStrategySprites(ref StrategyVisualsSaveData data)
    {
        data.Objectives.Clear();
        data.Sizes.Clear();
        data.Targets.Clear();
        foreach (var pair in strategyTargets)
        {
            data.Objectives.Add(pair.Key);
            data.Sizes.Add(pair.Value.Count);
            data.Targets.AddRange(pair.Value);
        }
    }

    public void LoadStrategySprites(ref StrategyVisualsSaveData data)
    {
        strategyTargets.Clear();

        for (int i = 0; i < data.Objectives.Count; i++)
        {
            SetShowSprite(true, GetTargets(data.Targets, data.Sizes, i), data.Objectives[i]);
        }
    }

    public void GetSelectedShip(out int index, out List<EnemyManeuverData> blocks, out List<int> blocksDurabilities)
    {
        index = selectedObject;
        for (int i = 0; i < selectedObject; i++)
        {
            if (enemyStrategyObjects[i].Data.Dead)
            {
                index--;
            }
        }

        blocks = enemyShipDatas;
        blocksDurabilities = durabilities;
    }

    public void SetSelectedObject(int index)
    {
        if (selectedObject != -1 && selectedObject < enemyStrategyObjects.Count)
        {
            enemyStrategyObjects[selectedObject].SetSelected(false);
        }
        selectedObject = index;
        enemyStrategyObjects[selectedObject].SetSelected(true);

        TacticManager.Instance.SelectedObjectIndex = index;
        strategySelectionPanel.RecalculateValues();
    }

    public void SwitchType(int index)
    {
        SwitchTypeInner(index);
        strategySelectionPanel.Restart();
    }

    private void SwitchTypeInner(int index)
    {
        (enemyShipDatas[index], alternativeShipDatas[index]) = (alternativeShipDatas[index], enemyShipDatas[index]);
        durabilities[index] = enemyShipDatas[index].Durability;
    }

    private void Clear()
    {
        foreach (EnemyStrategyObject obj in strategyObjects)
        {
            obj.transform.parent.gameObject.SetActive(false);
        }
        enemyStrategyObjects.Clear();
        enemyShipDatas.Clear();
        alternativeShipDatas.Clear();
        durabilities.Clear();
    }

    private IEnumerable<int> GetTargets(List<int> targets, List<int> sizes, int index)
    {
        int index2 = 0;
        for (int i = 0; i < index; i++)
        {
            index2 += sizes[i];
        }
        for (int i = 0; i < sizes[index]; i++)
        {
            yield return targets[index2 + i];
        }
    }
}
