using UnityEngine;

public class IntermissionMap : MonoBehaviour
{
    public IntermissionPanel Panel => panel;

    [SerializeField]
    private IntermissionPanel panel = null;
    [SerializeField]
    private IntermissionMapResolutionResizer resizer = null;
    [SerializeField]
    private MissionDescription description = null;
    [SerializeField]
    private Transform descriptionAnchor = null;
    [SerializeField]
    private Vector2 offsetWithDescription = new Vector2(194f, -37f);

    private RectTransform trans;

    private void Awake()
    {
        trans = transform as RectTransform;
    }

    public MissionDescription ShowGetMissionDescription()
    {
        if (!description.gameObject.activeSelf)
        {
            trans.anchoredPosition = offsetWithDescription / resizer.Scale;
            description.gameObject.SetActive(true);
            description.transform.position = new Vector3(descriptionAnchor.position.x, description.transform.position.y, description.transform.position.z);
        }
        return description;
    }
}
