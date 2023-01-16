using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RoomDamageButton : MonoBehaviour
{
	public Image icon;
	public TooltipCaller ownTooltip;

	[HideInInspector]
	public SectionRoom room;


	public void ShowDamage()
	{
		CameraManager.Instance.SwitchMode(ECameraView.Sections);
		CameraManager.Instance.ZoomToSection(room);
	}
}