using System;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

public class UnidentifiedObject : TacticalObject, ITickable
{
    public event Action TypeChanged = delegate { };
    public event Action Destroying = delegate { };

    public bool Dead => !active;

    private TacticalMap tacticalMap;
    private Vector2 direction;
    private Vector2 borders;

    private bool active;
    private UnidentifiedObject obj;

    public void Setup(TacticalMap tacticalMap, float carrierOffset, float borderOffset, bool visible)
    {
        Init();
        var bX = borders.x - borderOffset;
        var bY = borders.y - borderOffset;

        Vector2 point;
        int tries = 0;
        do
        {
            point.x = UnityRandom.Range(-bX, bX);
            point.y = UnityRandom.Range(-bY, bY);
            if (++tries > 1000)
            {
                break;
            }
        }
        while (Vector2.SqrMagnitude(tacticalMap.MapShip.Position - point) < carrierOffset);

        RectTransform.anchoredPosition = point;

        if (tacticalMap.IsOnLand(RectTransform.anchoredPosition, TacticManager.Instance.MapNodes.MaskScale))
        {
            var index = UnityRandom.Range(0, 2);
            if (index == 0)
            {
                ChangeType(ETacticalObjectType.Nothing);
            }
            else
            {
                ChangeType(ETacticalObjectType.Outpost);
            }
        }
        else
        {
            var index = UnityRandom.Range(0, 3);
            if (index == 0)
            {
                ChangeType(ETacticalObjectType.Nothing);
            }
            else if (index == 1)
            {
                ChangeType(ETacticalObjectType.Whales);
            }
            else
            {
                ChangeType(ETacticalObjectType.StrikeGroup);
            }
        }
        if (Type != ETacticalObjectType.Outpost)
        {
            var dir = UnityRandom.insideUnitCircle.normalized;
            if (Type != ETacticalObjectType.Nothing && Type != ETacticalObjectType.Whales)
            {
                RectTransform.anchoredPosition += direction * MovementSpeed;
                direction = dir;
            }
            RectTransform.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, dir));
        }
        InstantUpdate = true;
        gameObject.SetActive(visible);
    }

    public void Setup(ETacticalObjectType type, TacticalEnemyShip ship)
    {
        ChangeType(type);
        InstantUpdate = true;
        direction = UnityRandom.insideUnitCircle.normalized;
        MovementSpeed = 0f;
        RectTransform.anchoredPosition = ship.RectTransform.anchoredPosition;

        if (Type != ETacticalObjectType.Outpost)
        {
            RectTransform.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, direction));
        }
        Init();
    }

    public void LoadData(TacticalObjectData data)
    {
        RectTransform.anchoredPosition = data.Position;

        direction = data.Direction;
        MovementSpeed = data.MovementSpeed;

        ChangeType(data.Type);
        if (Type != ETacticalObjectType.Outpost)
        {
            RectTransform.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, direction));
        }
        Init();
    }

    public TacticalObjectData SaveData()
    {
        var result = new TacticalObjectData();

        result.Position = RectTransform.anchoredPosition;
        result.Direction = direction;
        result.Type = Type;
        result.MovementSpeed = MovementSpeed;

        return result;
    }

    public void Tick()
    {
        if (!active || obj == null || RectTransform == null)
        {
            return;
        }
        if (Type != ETacticalObjectType.Outpost && Type != ETacticalObjectType.Nothing)
        {
            RectTransform.anchoredPosition += direction * MovementSpeed;
            //RectTransform.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
        }

        if (Type != ETacticalObjectType.Outpost && Type != ETacticalObjectType.Nothing && tacticalMap.IsOnLand(RectTransform.anchoredPosition, TacticManager.Instance.MapNodes.MaskScale))
        {
            ChangeType(ETacticalObjectType.Nothing);
            TypeChanged();
        }

        if (Mathf.Abs(RectTransform.anchoredPosition.x) > borders.x ||
            Mathf.Abs(RectTransform.anchoredPosition.y) > borders.y)
        {
            Destroy();
        }
    }

    public void Destroy()
    {
        active = false;
        TimeManager.Instance.RemoveTickable(this);
        Destroying();
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        obj = this;
        tacticalMap = TacticalMap.Instance;
        active = true;

        var mapNodes = TacticManager.Instance.MapNodes;
        borders.x = (mapNodes.Width / 2f) - 20f;
        borders.y = mapNodes.Height / 2f;
        
        TimeManager.Instance.AddTickable(this);
    }
}
