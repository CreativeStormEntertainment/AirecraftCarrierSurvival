using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

[Serializable]
public class ScenarioData
{
#if UNITY_EDITOR
    static class DataOrder
    {
        public const int CrewMan = 0;
        public const int StartX = 1;
        public const int StartY = 2;
        public const int ObjectiveX = 3;
        public const int ObjectiveY = 4;
        public const int EndX = 5;
        public const int EndY = 6;
        public const int ViewRange = 7;
        public const int ConstEvents = 8;
        public const int StartRandomEvents = 9;
        public const int LaterRandomEvents = 10;
        public const int Description = 11;
        public const int Summary = 12;
        public const int Count = 13;
    }

    public ScenarioData(EventManager evMan, List<string[]> data)
    {
        Assert.IsTrue(data[0].Length == DataOrder.Count);
        StartPointX = int.Parse(data[0][DataOrder.StartX]);
        StartPointY = int.Parse(data[0][DataOrder.StartY]);
        ObjectivePointX = int.Parse(data[0][DataOrder.ObjectiveX]);
        ObjectivePointY = int.Parse(data[0][DataOrder.ObjectiveY]);
        EndPointX = int.Parse(data[0][DataOrder.EndX]);
        EndPointY = int.Parse(data[0][DataOrder.EndY]);
        ViewRange = float.Parse(data[0][DataOrder.ViewRange]);
        Description = data[0][DataOrder.Description];
        Summary = data[0][DataOrder.Summary];

        //ConstEventIndices = new List<int>();
        //RandomEventIndicesOnStart = new List<int>();
        //RandomEventIndicesAfterObjective = new List<int>();
        //for (int i = 0; i < data.Count; i++)
        //{
        //    Assert.IsTrue(data[i].Length == DataOrder.Count);
        //    Assert.IsTrue(i == 0 || data[i][DataOrder.StartX].Length == 0);
        //    Assert.IsTrue(i == 0 || data[i][DataOrder.StartY].Length == 0);
        //    Assert.IsTrue(i == 0 || data[i][DataOrder.ObjectiveX].Length == 0);
        //    Assert.IsTrue(i == 0 || data[i][DataOrder.ObjectiveY].Length == 0);
        //    Assert.IsTrue(i == 0 || data[i][DataOrder.EndX].Length == 0);
        //    Assert.IsTrue(i == 0 || data[i][DataOrder.EndY].Length == 0);
        //    Assert.IsTrue(i == 0 || data[i][DataOrder.Description].Length == 0);
        //    Assert.IsTrue(i == 0 || data[i][DataOrder.Summary].Length == 0);

        //    var eventName = data[i][DataOrder.ConstEvents];
        //    bool found = false;
        //    if (eventName.Length != 0)
        //    {
        //        ConstEventIndices.Add(evMan.GetConstEventIndex(eventName));
        //        found = true;
        //    }
        //    eventName = data[i][DataOrder.StartRandomEvents];
        //    if (eventName.Length != 0)
        //    {
        //        RandomEventIndicesOnStart.Add(evMan.GetRandomEventIndex(eventName));
        //        found = true;
        //    }
        //    eventName = data[i][DataOrder.LaterRandomEvents];
        //    if (eventName.Length != 0)
        //    {
        //        RandomEventIndicesAfterObjective.Add(evMan.GetRandomEventIndex(eventName));
        //        found = true;
        //    }
        //    Assert.IsTrue(found);
        //}
    }
#endif

    private ScenarioData() { }

    //public void GameplayInit(EventManager evMan)
    //{
    //    ConstEvents = new List<ConstEventData>();
    //    foreach(var index in ConstEventIndices)
    //    {
    //        ConstEvents.Add(evMan.ConstEvents[index]);
    //    }

    //    GameplayInit(evMan, ref RandomEventsOnStart, RandomEventIndicesOnStart);
    //    GameplayInit(evMan, ref RandomEventsAfterObjective, RandomEventIndicesAfterObjective);
    //}

    //private void GameplayInit(EventManager evMan, ref List<RandomEventData> events, List<int> indices)
    //{
    //    events = new List<RandomEventData>();
    //    foreach (var index in indices)
    //    {
    //        events.Add(evMan.RandomEvents[index]);
    //    }
    //}

    public int StartPointX;
    public int StartPointY;
    public int ObjectivePointX;
    public int ObjectivePointY;
    public int EndPointX;
    public int EndPointY;
    public float ViewRange;
    public List<int> ConstEventIndices;
    public List<int> RandomEventIndicesOnStart;
    public List<int> RandomEventIndicesAfterObjective;
    public string Description;
    public string Summary;

    //[NonSerialized]
    //public List<ConstEventData> ConstEvents;
    //[NonSerialized]
    //public List<RandomEventData> RandomEventsOnStart;
    //[NonSerialized]
    //public List<RandomEventData> RandomEventsAfterObjective;
}
