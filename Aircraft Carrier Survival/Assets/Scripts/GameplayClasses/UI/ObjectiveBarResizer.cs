using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveBarResizer : MonoBehaviour
{
    public static ObjectiveBarResizer Instance;

    [SerializeField] private RectTransform objectives = null;
    [SerializeField] private RectTransform objectivesBar = null;

    private void Awake()
    {
        Instance = this;
    }

    public void ResizeBar()
    {
        float newSize = 0;

        for (int i = 0; i < objectives.transform.childCount; ++i)
        {
            var childSize = objectives.rect.width;
            if (newSize < childSize)
            {
                newSize = childSize;
            }
        }
        objectivesBar.sizeDelta = new Vector2(newSize, objectivesBar.sizeDelta.y);
    }

    public void RebuildContent(List<ObjectiveObject> objectivesObjects)
    {
        float newSize = 0.0f;

        foreach (ObjectiveObject objective in objectivesObjects)
        {
            float contentSize = objective.ContentRect.sizeDelta.x;
            if (newSize < contentSize)
            {
                newSize = contentSize;
            }
        }

        foreach (ObjectiveObject objective in objectivesObjects)
        {
            objective.ContentRect.sizeDelta = new Vector2(newSize, objective.ContentRect.sizeDelta.y);
            LayoutRebuilder.ForceRebuildLayoutImmediate(objective.ContentRect);
        }
    }

    public void RebuildBackground(List<ObjectiveObject> objectivesObjects)
    {
        float newSize = 0.0f;

        foreach (ObjectiveObject objective in objectivesObjects)
        {
            if (objective.isActiveAndEnabled)
            {
                float backgroundSize = objective.mainRectDeltaSize;
                if (newSize < backgroundSize)
                {
                    newSize = backgroundSize;
                }
            }
        }

        foreach (ObjectiveObject objective in objectivesObjects)
        {
            objective.MainTextRect.sizeDelta = new Vector2(newSize, objective.MainTextRect.sizeDelta.y);
            objective.MainRect.sizeDelta = new Vector2(newSize, objective.MainRect.sizeDelta.y);
        }

        foreach (ObjectiveObject objective in objectivesObjects)
        {
            foreach (ObjectiveStepObject stepObject in objective.StepObjects)
            {
                stepObject.RebuildText();
            }
        }

        RebuildContent(objectivesObjects);

    }

}
