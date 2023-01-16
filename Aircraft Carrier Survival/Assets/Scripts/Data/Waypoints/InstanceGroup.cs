using System.Collections.Generic;

public class InstanceGroup
{
    public EWaypointTaskType TaskType;
    public EWorkerActionType ActionType;
    public List<InstanceData> Instances;
    //public Subcrewman ConnectedSubcrewman;

    public InstanceGroup(EWaypointTaskType taskType)
    {
        TaskType = taskType;
        ActionType = EWorkerActionType.None;
        Instances = new List<InstanceData>();
    }

    public void SetSelected(bool selected)
    {
        foreach (var workerInstance in Instances)
        {
            workerInstance.SetSelect(workerInstance.InUse && selected);
        }
    }
}
