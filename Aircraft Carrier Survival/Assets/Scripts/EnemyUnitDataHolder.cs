using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class EnemyUnitDataHolder : MonoBehaviour
{
    public EnemyUnitData Data;
    public TacticalMapGrid MapGrid = null;

#if UNITY_EDITOR
    [NonSerialized]
    private bool update = true;
    [NonSerialized]
    private List<List<float>> nodesValues = null;
    [NonSerialized]
    private GUIStyle textStyle = null;

    private void Awake()
    {
        textStyle = new GUIStyle();
        textStyle.normal.textColor = Color.black;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.fontSize = 16;
    }

    private void Update()
    {
        if (update)
        {
            update = false;
            try
            {
                var secondaryRangeRect = transform.GetChild(1).GetComponent<RectTransform>();

                float range = Data.AttackRange;
                var rangeColor = Color.white;
                var shipColor = Color.red;
                if (Data.IsAlly)
                {
                    range = Data.RevealRange;
                    rangeColor = Color.blue;
                    shipColor = Color.green;
                    secondaryRangeRect.gameObject.SetActive(false);
                }
                else
                {
                    secondaryRangeRect.sizeDelta = Vector2.one * 2f * Data.DetectRange;
                    secondaryRangeRect.gameObject.SetActive(true);
                }

                var rangeRect = transform.GetChild(0).GetComponent<RectTransform>();
                rangeRect.sizeDelta = Vector2.one * 2f * range;
                rangeRect.GetComponent<Image>().color = rangeColor;
                GetComponent<Image>().color = shipColor;
            }
            catch (Exception ex)
            {
                if (ex is ExitGUIException)
                {
                    throw ex;
                }
            }
        }
    }

    private void OnValidate()
    {
        update = true;
        if (Data.MaxOffset < 0f)
        {
            Data.MaxOffset = 0f;
        }
        if (Data.AttackRange < 0f)
        {
            Data.AttackRange = 0f;
        }
        if (Data.RevealRange < 0f)
        {
            Data.RevealRange = 0f;
        }
        if (Data.DetectRange < 0f)
        {
            Data.DetectRange = 0f;
        }
        Data.OverrideChanceToReveal = Mathf.Clamp(Data.OverrideChanceToReveal, -1f, 1f);
        RecalculatePath();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var dict = new Dictionary<RectTransform, List<RectTransform>>();
        if (Data.Patrols == null || Data.Patrols.Count == 0)
        {
            return;
        }
        int patrolCount = 0;
        foreach (var patrol in Data.Patrols)
        {
            dict.Clear();
            if (patrol.Nodes.Count < 2)
            {
                continue;
            }
            int j = 1;
            int i = 0;
            while (i < patrol.Nodes.Count && j < patrol.Nodes.Count)
            {
                var trans1 = patrol.Nodes[i];
                if (trans1 != null)
                {
                    var trans2 = patrol.Nodes[j];
                    if (trans2 == null)
                    {
                        j++;
                        continue;
                    }
                    bool second = false;
                    if (!dict.TryGetValue(trans1, out var list) && ((second = true) && !dict.TryGetValue(trans2, out list)))
                    {
                        second = false;
                        list = new List<RectTransform>();
                        dict[trans1] = list;
                    }
                    var anotherLineOffset = Vector3.zero;
                    var transu = second ? trans1 : trans2;
                    int count = 0;
                    foreach (var trans in list)
                    {
                        if (trans == transu)
                        {
                            count++;
                        }
                    }
                    list.Add(transu);
                    var toSelfOffset = Vector3.zero;
                    if (trans1 == trans2)
                    {
                        toSelfOffset = new Vector3(15f, 15f);
                    }
                    else
                    {
                        if (!dict.TryGetValue(transu, out list))
                        {
                            list = new List<RectTransform>();
                            dict[transu] = list;
                        }
                        list.Add(second ? trans2 : trans1);
                    }
                    if (count > 0)
                    {
                        anotherLineOffset = trans2.position + toSelfOffset - trans1.position;
                        anotherLineOffset.z = 0f;
                        anotherLineOffset.Normalize();
                        (anotherLineOffset.x, anotherLineOffset.y) = (anotherLineOffset.y, -anotherLineOffset.x);

                        anotherLineOffset *= (count * 3f);
                    }
                    Gizmos.DrawLine(trans1.position + anotherLineOffset, trans2.position + anotherLineOffset + toSelfOffset);
                    if (nodesValues != null)
                    {
                        UnityEditor.Handles.Label(trans2.position, Mathf.RoundToInt(nodesValues[patrolCount][i]).ToString(), textStyle);
                    }
                }
                i++;
                j++;
            }
            if (nodesValues != null)
            {
                try
                {
                    UnityEditor.Handles.Label(patrol.Nodes[0].position, Mathf.RoundToInt(nodesValues[patrolCount][i]).ToString(), textStyle);
                }
                catch(Exception ex)
                {
                    if (patrol == null)
                    {
                        Debug.LogError("patrol is empty");
                    }
                    else if (patrol.Nodes == null)
                    {
                        Debug.LogError("nodes are empty");
                    }
                    else if (patrol.Nodes[0] == null)
                    {
                        Debug.LogError("first node is empty");
                    }
                    Debug.LogError(nodesValues.Count + ";" + patrolCount);
                    if (nodesValues.Count > patrolCount)
                    {
                        if (nodesValues[patrolCount] == null)
                        {
                            Debug.LogError("nodesValues[patrolCount] is empty!!!");
                        }
                        else
                        {
                            Debug.LogError(nodesValues[patrolCount].Count + ";" + i);
                        }
                    }
                    throw ex;
                }
            }
            patrolCount++;
        }
    }

    public void RecalculatePath()
    {
        if (MapGrid != null)
        {
            nodesValues = new List<List<float>>();
            if (Data.Patrols == null || Data.Patrols.Count == 0)
            {
                return;
            }
            int p = 0;
            foreach (var patrol in Data.Patrols)
            {
                nodesValues.Add(new List<float>());
                float totalSwimCount = 0f;
                for (int i = 1; i < patrol.Nodes.Count; i++)
                {
                    if (Data.AlternativePathfinding)
                    {
                        nodesValues[p].Add(GetSwimTime(MapGrid.Find(patrol.Nodes[i - 1].anchoredPosition), MapGrid.Find(patrol.Nodes[i].anchoredPosition)));
                    }
                    else
                    {
                        nodesValues[p].Add(GetSwimTime(MapGrid.FindPath(patrol.Nodes[i - 1].anchoredPosition, patrol.Nodes[i].anchoredPosition)));
                    }
                    totalSwimCount += nodesValues[p][i - 1];
                }
                nodesValues[p].Add(totalSwimCount);
                p++;
            }
        }
    }

    private float GetSwimTime(params PathNode[] nodes)
    {
        return GetSwimTime((IEnumerable<PathNode>)nodes);
    }

    private float GetSwimTime(IEnumerable<PathNode> nodes)
    {
        float distance = 0f;
        PathNode prevNode = null;
        foreach (var node in nodes)
        {
            if (prevNode == null)
            {
                prevNode = node;
                continue;
            }
            distance += Vector3.Magnitude(prevNode.Position - node.Position);
            prevNode = node;
        }
        return distance / Data.Speed;
    }
#endif
}
