using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;

public class ShipWaypoint : MapWaypoint, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    public RectTransform RangeCircle = null;
    protected WaypointMap map;
    private MapIndicator linkedIndicator;
    private float distanceToPrev;
    private float reachTime;
    private EventSystem es;
    protected bool isDragged;

    public Image Image;

    [SerializeField]
    private Sprite normal = null;
    [SerializeField]
    private Sprite highlight = null;
    [SerializeField]
    private StudioEventEmitter hoverSound = null;
    [SerializeField]
    private StudioEventEmitter dragSound = null;
    [SerializeField]
    private StudioEventEmitter dropSound = null;
    [SerializeField]
    private StudioEventEmitter disabledDropSound = null;
    [SerializeField]
    private StudioEventEmitter deleteSound = null;

    protected Vector2 originPosition;

    public float DistanceToPrev { get => distanceToPrev; set { distanceToPrev = value; } }
    public float ReachTime { get => reachTime; set { reachTime = value; } }

    public WaypointMap Map { get => map; }

    public List<GameObject> TracksList => trackList;

    public bool Hovered
    {
        get;
        private set;
    }

    protected bool ignoreRedraw;

    private Vector3 newPosition;

    private bool firstHover = false;

    private PointerEventData ped = new PointerEventData(null);
    List<RaycastResult> results = new List<RaycastResult>();

    private void Awake()
    {
        if (dropSound != null)
        {
            dropSound.Play();
        }
    }

    private void Update()
    {
        if (isDragged && !Input.GetMouseButton(0))
        {
            OnEndDrag(null);
        }
    }

    public virtual void SetMap(WaypointMap map)
    {
        this.map = map;
    }

    public void OnClick()
    {
        //RemoveWaypoint();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hovered = true;
        Image.sprite = highlight;
        if (firstHover)
        {
            if (hoverSound != null && !map.WaypointDraging)
            {
                hoverSound.Play();
            }
        }
        else
        {
            firstHover = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hovered = false;
        Image.sprite = normal;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (dragSound != null && eventData.button == PointerEventData.InputButton.Left)
        {
            dragSound.Play();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragged)
        {
            map.OnPointerClick(eventData);
        }
    }

    public void SetObjective(MapIndicator _linkedIndicator)
    {
        linkedIndicator = _linkedIndicator;
    }

    public void TriggerMapIndicator()
    {
        if (linkedIndicator == null)
            return;
        linkedIndicator.FireShipArrivedEvent();
    }

    public void DestroyAllTracksDots()
    {
        foreach (GameObject t in trackSegments)
        {
            GameObject.Destroy(t);
        }
        trackSegments.Clear();
    }

    public void DestroyAllTaskSegments()
    {
        foreach (GameObject t in taskSegments)
        {
            GameObject.Destroy(t);
        }
        taskSegments.Clear();
    }

    public void DestroyGameObjectWithAllTracks()
    {
        if (deleteSound != null && gameObject.activeInHierarchy)
        {
            deleteSound.Play();
        }
        DestroyAllTracks();
        DestroyAllTaskSegments();
        DestroyAllTracksDots();
        GameObject.Destroy(gameObject);
    }

    public void GetAllTracksPoints(Transform parent)
    {
        foreach (GameObject t in trackList)
        {
            t.transform.SetParent(parent);
        }
    }

    public void DrawRadius(float range)
    {
        RangeCircle.sizeDelta = new Vector2(2f * range, 2f * range);
        RangeCircle.gameObject.SetActive(true);
    }

    public void SetInteracteble()
    {
        //RangeCircle
    }

    //public void SetTaskPoint(Vector3 A, Vector3 B, GameObject dotPrefab)
    //{
    //    GameObject dot = GameObject.Instantiate(dotPrefab, transform);
    //    dot.transform.position = LerpByPercentage(A, B, .5f);
    //    taskSegments.Add(dot);
    //}

    public void OnDrop(PointerEventData eventData)
    {
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || GameStateManager.Instance.Tutorial)
        {
            return;
        }
        Image.raycastTarget = false;
        isDragged = true;
        originPosition = RectTransform.anchoredPosition;
        map.WaypointDraging = true;
        HudManager.QuickSaved += OnQuickSaved;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || GameStateManager.Instance.Tutorial)
        {
            return;
        }
        Drag(eventData.position);
    }

    public void Drag(Vector3 pos)
    {
        if (isDragged)
        {
            ////Debug.Log ("OnDrag");
            transform.position = pos;
            //bool ok = map.CanBeDropped(this, RectTransform.anchoredPosition);
            map.RedrawRoadTracks(this);
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left || GameStateManager.Instance.Tutorial)
        {
            return;
        }
        DragEnd();
    }

    private void DragEnd()
    {
        ped.position = Input.mousePosition;
        results.Clear();
        UIManager.Instance.GraphicRaycaster.Raycast(ped, results);

        bool mapParent = false;
        if (results.Count > 0)
        {
            var obj = results[0].gameObject.transform;
            if (obj == map.transform)
            {
                mapParent = true;
            }
            while (obj.parent != null && !mapParent)
            {
                obj = obj.parent;
                if (obj == map.transform)
                {
                    mapParent = true;
                    break;
                }
            }
        }

        bool ok = (ignoreRedraw || map.RedrawRoadTracks(this)) && !map.LandOnTheWay && mapParent;
        if (ok && (!ignoreRedraw && !SectionRoomManager.Instance.Helm.IsWorking))
        {
            ok = false;
            EventManager.Instance.WaypointPopup();
        }

        if (ok)
        {
            if (dropSound != null && isDragged)
            {
                dropSound.Play();
            }
        }
        else
        {
            if (disabledDropSound != null)
            {
                disabledDropSound.Play();
            }
            RectTransform.anchoredPosition = originPosition;
            map.RedrawRoadTracks(this);
        }
        isDragged = false;
        map.WaypointDraging = false;
        Image.raycastTarget = true;
        HudManager.QuickSaved -= OnQuickSaved;
    }

    private void OnQuickSaved()
    {
        DragEnd();
    }

    //private void Update()
    //{
    //    if (HudManager.Instance.IsSettingsOpened && isDragged)
    //    {
    //        transform.position = originPosition;
    //        map.RedrawPotentialTrack(this, false, true, false);
    //        map.MapShip.SetDestinationWaypoint();
    //        isDragged = false;
    //    }
    //}
}
