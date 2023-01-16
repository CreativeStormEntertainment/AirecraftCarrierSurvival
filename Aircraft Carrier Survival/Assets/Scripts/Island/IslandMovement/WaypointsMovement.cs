using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsMovement : MonoBehaviour
{
    public IslandNode CurrentNode
    {
        get;
        private set;
    }


    private List<IslandNode> path;
    private Transform currTransform;
    [SerializeField]
    private float baseMoveSpeed = 5f;
    private float moveSpeed;
    [SerializeField]
    private float maxRotationSpeed = 20f;

    private Animator animator = null;

    private Quaternion desiredRotation;

    private void Awake()
    {
        currTransform = transform;
        path = new List<IslandNode>();
        enabled = false;
        moveSpeed = baseMoveSpeed;
    }

    private void Update()
    {
        int nameHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (path.Count != 0)
        {
            currTransform.position = Vector3.MoveTowards(currTransform.position, path[0].Position, moveSpeed * Time.deltaTime);
            currTransform.rotation = Quaternion.RotateTowards(currTransform.rotation, desiredRotation, maxRotationSpeed * Time.deltaTime);
            if (Mathf.Approximately((currTransform.position - path[0].Position).sqrMagnitude, 0f))
            {
                path.RemoveAt(0);
                if (path.Count == 0)
                {
                    currTransform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    CurrentNode = path[0];
                    Vector3 forward = currTransform.position - CurrentNode.Position;
                    forward.y = 0;
                    desiredRotation = Quaternion.LookRotation(forward);
                }

            }
        }
    }

    public void StartMovement(IslandNode destination)
    {
        if (path.Count != 0)
        {
            IslandNode node = path[0];
            path.Clear();
            path.Add(node);
        }
        enabled = true;
        path.AddRange(IslandNode.FindPath(CurrentNode, destination));
        CurrentNode = path[0];
        Vector3 forward = currTransform.position - CurrentNode.Position;
        forward.y = 0;
        desiredRotation = Quaternion.LookRotation(forward);
    }

    public void Setup(IslandNode startNode)
    {
        CurrentNode = startNode;
        animator = GetComponentInChildren<Animator>();
    }

    public void SetTiredMovementChange(bool tired)
    {
        moveSpeed = baseMoveSpeed * (tired ? 0.8f : 1);
    }
}
