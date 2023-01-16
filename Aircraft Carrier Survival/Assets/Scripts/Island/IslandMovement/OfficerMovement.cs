using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficerMovement : MonoBehaviour
{
    public event Action RoomReached = delegate { };

    public IslandNode CurrentNode
    {
        get;
        private set;
    }


    public List<IslandNode> Path
    {
        get;
        private set;
    }
    private Transform currTransform;
    [SerializeField]
    private float baseMoveSpeed = 5f;
    private float moveSpeed;
    [SerializeField]
    private float maxRotationSpeed = 20f;

    private Animator animator = null;

    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int idleHash = Animator.StringToHash("Idle02");
    private readonly int walkHash = Animator.StringToHash("Walk");
    private readonly int actionHash = Animator.StringToHash("ActionNumber");
    private readonly int startActionHash = Animator.StringToHash("StartAction");
    private Quaternion desiredRotation;
    private Officer officer;

    private void Awake()
    {
        currTransform = transform;
        Path = new List<IslandNode>();
        officer = GetComponent<Officer>();
        enabled = false;
        moveSpeed = baseMoveSpeed;
    }

    private void Update()
    {
        int nameHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (Path.Count != 0 && (nameHash == idleHash || nameHash == walkHash))
        {
            currTransform.position = Vector3.MoveTowards(currTransform.position, Path[0].Position, moveSpeed * Time.deltaTime);
            currTransform.rotation = Quaternion.RotateTowards(currTransform.rotation, desiredRotation, maxRotationSpeed * Time.deltaTime);
            if (Mathf.Approximately((currTransform.position - Path[0].Position).sqrMagnitude, 0f))
            {
                Path.RemoveAt(0);
                if (Path.Count == 0)
                {
                    animator.SetBool(isWalkingHash, false);
                    RoomReached();
                    IslandsAndOfficersManager.Instance.SetShowPath(officer, false);
                    currTransform.rotation = Quaternion.Euler(0, 0, 0);
                    animator.SetInteger(actionHash, officer.CurrentIslandRoom.GetOfficerAnimationNumber());
                    animator.SetTrigger(startActionHash);
                }
                else
                {
                    CurrentNode = Path[0];
                    Vector3 forward = currTransform.position - CurrentNode.Position;
                    forward.y = 0f;
                    if (forward.sqrMagnitude > .01f)
                    {
                        desiredRotation = Quaternion.LookRotation(forward);
                    }
                }
            }
        }
    }

    public void StartMovement(IslandNode destination)
    {
        if (Path.Count != 0)
        {
            IslandNode node = Path[0];
            Path.Clear();
            Path.Add(node);
        }
        enabled = true;
        animator.SetBool(isWalkingHash, true);
        Path.AddRange(IslandNode.FindPath(CurrentNode, destination));
        CurrentNode = Path[0];
        Vector3 forward = currTransform.position - CurrentNode.Position;
        forward.y = 0;
        if (forward != Vector3.zero)
        {
            desiredRotation = Quaternion.LookRotation(forward);
        }
    }

    public void ForceIdle()
    {
        animator.Play(idleHash);
    }

    public void Setup(IslandNode startNode)
    {
        CurrentNode = startNode;
        animator = officer.Model.GetComponent<Animator>();
        animator.SetBool(isWalkingHash, false);
    }

    public void SetTiredMovementChange(bool tired)
    {
        moveSpeed = baseMoveSpeed * (tired ? 0.8f : 1);
    }
}
