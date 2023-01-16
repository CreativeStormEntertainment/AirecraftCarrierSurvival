using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionSummaryObjective : MonoBehaviour
{
    [SerializeField]
    private Text objectiveName = null;
    [SerializeField]
    private Image objectiveStateImage = null;
    [SerializeField]
    private Sprite objectiveDefaultSprite = null;
    [SerializeField]
    private Sprite objectiveCompleteSprite = null;
    [SerializeField]
    private Sprite objectiveFailedSprite = null;
    [SerializeField]
    private Color objectiveCompleteColor = Color.green;
    [SerializeField]
    private Color objectiveFailedColor = Color.red;

    public void Setup(Objective objective)
    {
        var title = ObjectivesManager.Instance.GetTitle(objective.Index);
        if (string.IsNullOrWhiteSpace(title))
        {
            Debug.LogError($"{objective.Data.Name}, {objective.Index}, {objective.Data.Type}, {objective.Data.Title}, {objective.Data.Description}");
        }
        else if (title.StartsWith("BAD_"))
        {
            Debug.LogError($"{objective.Data.Name}, {objective.Index}, {objective.Data.Type}, {objective.Data.Title}, {objective.Data.Description}");
        }
        objectiveName.text = title;
        if (objective.ObjectiveValidState && objective.Success != objective.Data.InverseFinishStateInSummary)
        {
            objectiveStateImage.sprite = objectiveCompleteSprite;
            objectiveName.color = objectiveCompleteColor;
        }
        else
        {
            objectiveStateImage.sprite = objectiveFailedSprite;
            objectiveName.color = objectiveFailedColor;
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
