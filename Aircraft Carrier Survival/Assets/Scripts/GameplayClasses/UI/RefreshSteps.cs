
using UnityEngine;
using UnityEngine.UI;

public class RefreshSteps : MonoBehaviour
{
    [SerializeField]
    RectTransform[] toRefresh = null;

    private void OnEnable()
    {
        if (toRefresh != null)
        {
            //toRefresh.GetComponent<ObjectiveObject>().UpdateRectSize();
            foreach (RectTransform rt in toRefresh)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }
    }
}
