using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance;

    public List<SectionSegment> segments;

    private HashSet<ActionData> datas;
    private HashSet<ActionData> toLaunch;

    private void Awake()
    {
        Instance = this;
        datas = new HashSet<ActionData>();
        toLaunch = new HashSet<ActionData>();
    }

    private System.Collections.IEnumerator Coroutino()
    {
        yield return new WaitForSeconds(10f);
        //ObjectivesManager.Instance.FinishObjective(0, true);
        //GameStateManager.Instance.ShowMissionSummary(true, ESummaryPopup.OverallSuccess);
    }

    private void Update()
    {
        toLaunch.Clear();
        foreach (var data in datas)
        {
            data.Time -= Time.deltaTime;
            if (data.Time < Mathf.Epsilon)
            {
                toLaunch.Add(data);
            }
        }

        foreach (var data in toLaunch)
        {
            switch (data.Action)
            {
                case EActionType.Attack:
                    Debug.Log("enemy attack");
                    var eneAttMan = EnemyAttacksManager.Instance;
                    //eneAttMan.CreateEnemyAttack(eneAttMan.ScriptedAttackScenarios[data.PatternID].enemyAttackDatas[data.AttackInPattern], 0);
                    break;
                case EActionType.DynamicEvent:
                    Debug.Log("section event");
                    switch (data.EventType)
                    {
                        case EDynamicEventType.Damage:
                            data.Segment.MakeDamage();
                            break;
                        case EDynamicEventType.DoorLeak:
                            //foreach (var neighbourData in data.Segment.Neighbours)
                            //{
                            //    if (neighbourData.HasDoor() && !neighbourData.Door.HasLeak(data.Segment))
                            //    {
                            //        neighbourData.Door.MakeLeak();
                            //        break;
                            //    }
                            //}
                            break;
                        case EDynamicEventType.Fire:
                            data.Segment.MakeFire();
                            break;
                        case EDynamicEventType.Injured:
                            data.Segment.MakeInjured(EWaypointTaskType.Rescue);
                            break;
                        case EDynamicEventType.SectionDamage:
                            data.Segment.Parent.IsBroken = true;
                            break;
                        case EDynamicEventType.Water:
                            data.Segment.MakeFlood(false);
                            break;
                    }
                    break;
                case EActionType.FleetSpotted:
                    Debug.Log("fleet spotted");
                    TacticManager.Instance.RevealFleet(data.FleetID);
                    break;
                case EActionType.Mission:
                    Debug.Log("custom mission");
                    //TacticManager.Instance.CreateMission(data.MissionName, 0, data.MissionType, data.Squadrons, data.Stage, data.AttackPos, data.RetrievalPos, new List<Strategy>());
                    break;
                case EActionType.Event:
                    //Debug.Log("custom event");
                    //EventManager.Instance.AddOtherEvent(data.Title, data.Description);
                    break;
                case EActionType.Trigger:
                    Debug.Log("custom trigger");
                    if (data.Trigger())
                    {
                        data.OnSuccess();
                    }
                    else
                    {
                        data.OnFailure();
                    }
                    break;
                case EActionType.Cuustom:
                    data.Fun();
                    break;
                default:
                    break;
            }
            datas.Remove(data);
        }
    }

    public void DelayDynamicEvent(float time, EDynamicEventType action, SectionSegment segment)
    {
        if (segment == null)
        {
            segment = segments[0];
            segments.RemoveAt(0);
        }
        datas.Add(new ActionData(time, action, segment));
    }

    public void DelayAttack(float time, int patternID, int attackInPattern)
    {
        datas.Add(new ActionData(time, patternID, attackInPattern));
    }

    public void DelayRevealFleet(float time, int fleetID)
    {
        datas.Add(new ActionData(time, fleetID));
    }

    public void DelayMission(float time, string missionName, EMissionOrderType missionType, List<EPlaneType> squadrons, EMissionStage stage, Vector2 attackPos, Vector2 retrievalPos)
    {
        datas.Add(new ActionData(time, missionName, missionType, squadrons, stage, attackPos, retrievalPos));
    }

    public void CustomEvent(float time, string title, string description)
    {
        datas.Add(new ActionData(time, title, description));
    }

    public void CustomTrigger(float time, Func<bool> trigger, Action onSuccess, Action onFailure)
    {
        datas.Add(new ActionData(time, trigger, onSuccess, onFailure));
    }

    public void CustomStuff(float time, Action action)
    {
        datas.Add(new ActionData(time, action));
    }
}
