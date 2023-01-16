using UnityEngine;

public class ColliderData
{
    public Waypoint Parent01;
    public Waypoint Parent02;
    public BoxCollider Connector;

    public ColliderData(Waypoint parent01, Waypoint parent02, BoxCollider connector)
    {
        Parent01 = parent01;
        Parent02 = parent02;
        Connector = connector;
    }

    public void CalculatePosition()
    {
        var diff = Parent02.transform.position - Parent01.transform.position;
        float angle = Vector3.Angle(diff, new Vector3(diff.x, 0f, diff.z));
        var right = new Vector3(diff.z, 0f, -diff.x);
        Connector.gameObject.transform.position = Parent01.transform.position + diff / 2f;
        Connector.gameObject.transform.rotation = Quaternion.LookRotation(right) * Quaternion.Euler(0f, 0f, -angle);
        Connector.size = new Vector3(diff.magnitude, .125f, .125f);
    }
}
