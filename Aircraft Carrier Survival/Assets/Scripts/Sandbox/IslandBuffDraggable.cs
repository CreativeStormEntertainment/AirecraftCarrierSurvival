using UnityEngine;
using UnityEngine.UI;

public class IslandBuffDraggable : InventoryDrag<IslandBuffDragData, IslandBuffDraggable>
{
    public override bool CanBeDragged => base.CanBeDragged && Data != null && Data.CanBeDragged;

    [SerializeField]
    private Text airText = null;
    [SerializeField]
    private Text navyText = null;
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Image image = null;

    public override void Setup(IslandBuffDragData data)
    {
        base.Setup(data);
        airText.text = data.Buff.Air.ToString();
        navyText.text = data.Buff.Ship.ToString();
        title.text = LocalizationManager.Instance.GetText(data.Buff.IslandBuffType.ToString() + "Title");
        image.sprite = data.Buff.Icon;
    }
}
