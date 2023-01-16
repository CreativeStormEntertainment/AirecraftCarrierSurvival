using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MissionResultPopup : MonoBehaviour
{
	[SerializeField]
	private float topOffset = 100f;
	[SerializeField]
	private float bottomOffset = 40f;
	[SerializeField]
	private float baseContainerOffset = 40f;

	[SerializeField]
	private RectTransform enemyLossesList = null;
	[SerializeField]
	private RectTransform playerLossesList = null;
	[SerializeField]
	private RectTransform baseContainer = null;
	[SerializeField]
	private RectTransform descriptionTransform = null;

	[SerializeField]
	private RectTransform ownRectTransform = null;

	public void RecalculatePopupSize()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(enemyLossesList);
		LayoutRebuilder.ForceRebuildLayoutImmediate(playerLossesList);
		LayoutRebuilder.ForceRebuildLayoutImmediate(baseContainer);
		////LayoutRebuilder.ForceRebuildLayoutImmediate(ownRectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionTransform);

		float baseHeight = Mathf.Max(enemyLossesList.sizeDelta.y, playerLossesList.sizeDelta.y);

		baseHeight += baseContainerOffset;
		float height = baseHeight + topOffset + bottomOffset + descriptionTransform.sizeDelta.y;

		baseContainer.sizeDelta = new Vector2(baseContainer.sizeDelta.x, baseHeight);

		Vector2 newSize = new Vector2(ownRectTransform.sizeDelta.x, height);
		ownRectTransform.sizeDelta = newSize;
		
		////LayoutRebuilder.ForceRebuildLayoutImmediate(enemyLossesList);
		////LayoutRebuilder.ForceRebuildLayoutImmediate(playerLossesList);
		////LayoutRebuilder.ForceRebuildLayoutImmediate(baseContainer);
		////LayoutRebuilder.ForceRebuildLayoutImmediate(ownRectTransform);
		////LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionTransform);
	}


	private void OnDisable()
	{
		while (enemyLossesList.childCount > 0)
		{
			enemyLossesList.GetChild(0).SetParent(null);
		}

		while (playerLossesList.childCount > 0)
		{
			playerLossesList.GetChild(0).SetParent(null);
		}

		RecalculatePopupSize();
	}
}