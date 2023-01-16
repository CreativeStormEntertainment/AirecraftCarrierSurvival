using UnityEngine;
using UnityEngine.UI;

public class ReconMissionResult : MonoBehaviour
{
	[SerializeField]
	private float topOffset = 100f;
	[SerializeField]
	private float bottomOffset = 40f;

	[SerializeField]
	private RectTransform fleetsList = null;
	[SerializeField]
	private RectTransform descriptionTransform = null;

	[SerializeField]
	private RectTransform ownRectTransform = null;

	public void RecalculatePopupSize()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(fleetsList);
		LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionTransform);
		////LayoutRebuilder.ForceRebuildLayoutImmediate(ownRectTransform);

		float height = topOffset + bottomOffset;
		height += fleetsList.sizeDelta.y + descriptionTransform.sizeDelta.y;

		ownRectTransform.sizeDelta = new Vector2(ownRectTransform.sizeDelta.x, height);
	}
}