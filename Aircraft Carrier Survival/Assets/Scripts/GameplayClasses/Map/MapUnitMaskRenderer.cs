using GambitUtils;
using System.Collections.Generic;
using UnityEngine;

public class MapUnitMaskRenderer : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer circleMaskSpritePrefab = null;

    private List<SpriteRenderer> circlesPool = new List<SpriteRenderer>();
    private List<SpriteRenderer> circles = new List<SpriteRenderer>();

    private void Awake()
    {
        transform.DestroyAllChildren();
        AddCirclesToPool(5);
    }

    private void Update()
    {
        List<MapUnit> units = MapUnitMaskManager.Instance.Units;

        if (units.Count < circles.Count)
        {
            // return circles to pool
            while (circles.Count > units.Count)
            {
                var circle = circles.PickLastElement();
                circle.gameObject.SetActive(false);
                circlesPool.Add(circle);
            }
        }
        else
        {
            while (circles.Count < units.Count)
            {
                if (circlesPool.Count == 0)
                {
                    AddCirclesToPool(5);
                }
                var circle = circlesPool.PickLastElement();
                circle.gameObject.SetActive(true);
                circles.Add(circle);
            }
        }

        for (int i = 0; i < units.Count; i++)
        {
            MapUnit unit = units[i];
            SpriteRenderer circle = circles[i];
            circle.gameObject.SetActive(unit.obj);
            if (unit.obj)
            {
                Vector3 circlePosition = unit.obj.transform.position;
                circlePosition.z = circle.transform.position.z;
                circle.transform.position = circlePosition;
                circle.transform.localScale = Vector2.one * 0.2f * unit.radius;
            }
        }
    }

    private void AddCirclesToPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            circlesPool.Add(CreateCircleSprite());
        }
    }

    private SpriteRenderer CreateCircleSprite()
    {
        SpriteRenderer circleSprite = Instantiate(circleMaskSpritePrefab, transform, false);
        circleSprite.gameObject.SetActive(false);
        return circleSprite;
    }
}
