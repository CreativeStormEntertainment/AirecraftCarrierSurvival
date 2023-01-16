using System;
using UnityEngine;

public class DropSlot<T1, T2> : MonoBehaviour, IInventoryDropSlot<T1, T2>
    where T1 : InventoryDrag<T2, T1>
    where T2 : class, IInventoryDragData
{
    public event Action<T2> ItemChanged = delegate { };

    public T1 Item => item;

    public RectTransform RectTransform
    {
        get;
        private set;
    }

    [SerializeField]
    private T1 item = null;

    public void Setup(RectTransform root, Canvas canvas)
    {
        RectTransform = GetComponent<RectTransform>();

        item.Setup(root, canvas);
        item.Dropped -= OnDropped;
        item.Dropped += OnDropped;
    }

    public void Set(T2 data)
    {
        if (data == null)
        {
            item.Setup(item.Root, item.Canvas);
        }
        else
        {
            item.Set(data);
        }
    }

    private void OnDropped(T2 data)
    {
        if (data == null)
        {
            item.Setup(item.Root, item.Canvas);
            BackgroundAudio.Instance.PlayEvent(ECrewUIState.DragFail);
        }
        else
        {
            item.Set(data);
            BackgroundAudio.Instance.PlayEvent(ECrewUIState.DragSuccess);
        }
        ItemChanged(item.Data);
    }
}
