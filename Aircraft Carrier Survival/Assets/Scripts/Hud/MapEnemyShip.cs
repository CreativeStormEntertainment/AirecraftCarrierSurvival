using System;
using UnityEngine;
using UnityEngine.Assertions;

public class MapEnemyShip : MonoBehaviour
{
    [NonSerialized]
    public bool IsDetected;

    [SerializeField]
    private float moveSpeed = 0f;
    [SerializeField]
    private float detectionRange = 10f;
    [SerializeField]
    private float timeScale;

    private MapNode rootNode = null;

    private MapNode currentNode;

    public TacticalEnemyMapButton Fleet
    {
        get
        {
            if (fleet == null)
            {
                fleet = GetComponent<TacticalEnemyMapButton>();
                return fleet;
            }
            else
            {
                return fleet;
            }
        }
    }
    public RectTransform ObjectTransform
    {
        get
        {
            if (objectTransform == null)
            {
                objectTransform = GetComponent<RectTransform>();
                return objectTransform;
            }
            else
            {
                return objectTransform;
            }
        }
    }

    private TacticalEnemyMapButton fleet = null;
    private RectTransform objectTransform = null;

    private float shipSpeed = 1f;
    //private Vector3 destinatedDirection;

    public bool CannotAttack;
    public bool IsDead;
    public bool Critical;

    private bool ticking;

    private bool isInRange;

    public void Setup(MapNode startNode, float timeScale)
    {
        rootNode = startNode;
        shipSpeed = moveSpeed * timeScale;

        if (Fleet == null)
        {
            Debug.LogError("Fleet is empty", this);
        }
        detectionRange *= detectionRange;
        Hide();
    }

    public void InitMovement()
    {
        currentNode = currentNode.NextNode;

        if (currentNode == null)
        {
            return;
        }

        //destinatedDirection = (currentNode.Position - objectTransform.anchoredPosition).normalized;
        RotationUpdate();
    }

    public void SetupNextMove()
    {
        if (IsDetected)
        {
            Assert.IsNotNull(currentNode.ExitNode);
            currentNode = currentNode.ExitNode;
        }
        else
        {
            currentNode = currentNode.NextNode;
        }
        if (currentNode == null)
        {
            return;
        }
        RotationUpdate();
    }

    public void Show()
    {
        Fleet.gameObject.SetActive(true);

        currentNode = rootNode;
        ObjectTransform.anchoredPosition = currentNode.Position;
        InitMovement();

        IsDead = false;

#warning todo reusing(restart fleet ships)
        //Assert.IsFalse(once);
        IsDetected = false;

        if (!ticking)
        {
            ticking = true;
            TimeManager.Instance.Invoke(MoveTick, 1);
        }
    }

    public void Hide()
    {
        IsDead = true;
        Fleet.gameObject.SetActive(false);
    }

    private void MoveTick()
    {

    }

    private bool IsWithinRange(float sqrRange, Vector2 targetPos)
    {
        float distToMarkSqr = Vector2.SqrMagnitude(targetPos - objectTransform.anchoredPosition);
        return distToMarkSqr < sqrRange;
    }

    public void PositionUpdate()
    {
        if (currentNode == null) return;
        objectTransform.anchoredPosition = Vector2.MoveTowards(objectTransform.anchoredPosition, currentNode.Position, shipSpeed * Time.deltaTime);
        if (Mathf.Approximately((objectTransform.anchoredPosition - currentNode.Position).sqrMagnitude, 0f))
        {
            SetupNextMove();
        }
    }

    private void RotationUpdate()
    {
        var diff = currentNode.Position - objectTransform.anchoredPosition;
        if (diff != Vector2.zero)
        {
            Quaternion rotationDir = Quaternion.LookRotation(diff, Vector3.forward);
            rotationDir.x = 0;
            rotationDir.y = 0;
            transform.rotation = rotationDir;
        }
    }
}
