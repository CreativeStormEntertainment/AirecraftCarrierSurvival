using UnityEngine;

public class WorldMapMovement : MonoBehaviour
{
    public float CurrentZoom
    {
        get;
        private set;
    } = 1f;

    public RectTransform MapRect => mapRect;

    [SerializeField]
    private RectTransform mapRect = null;
    [SerializeField]
    private float mapSpeed = 100f;
    [SerializeField]
    private float moveMapSpeedModifier = 10f;
    [SerializeField]
    private float zoomSpeed = 10f;
    [SerializeField]
    private float maxZoom = 1.5f;
    [SerializeField]
    private float minZoom = 1f;
    [SerializeField]
    private RectTransform minPos = null;
    [SerializeField]
    private RectTransform maxPos = null;
    [SerializeField]
    private RectTransform canvasRect = null;
    [SerializeField]
    private WorldMapShip ship = null;

    private Vector2 middle = Vector3.zero;

    private float mapImageWidth = 1920f;
    private float mapImageHeight = 1080f;

    private float leftBorder;
    private float rightBorder;
    private float topBorder;
    private float bottomBorder;

    private float leftShipPos;
    private float bottomShipPos;
    private float topShipPos;
    private float rightShipPos;

    private void Start()
    {
        ship.ShipPositionChanged += OnShipPositionChanged;
        mapImageWidth = mapRect.rect.width;
        mapImageHeight = mapRect.rect.height;
        CurrentZoom = mapRect.localScale.x;
        middle = new Vector2(canvasRect.rect.width / 2f, canvasRect.rect.height / 2f);
        float scale = (mapImageWidth / canvasRect.rect.width);
        float sizeDelta = (canvasRect.rect.width - mapImageWidth);
        if (canvasRect.rect.width > mapImageWidth)
        {
            minPos.anchoredPosition = new Vector3(sizeDelta / 2f, 0f, 0f);
            maxPos.anchoredPosition = new Vector3(-sizeDelta / 2f, 0f, 0f);
        }
        else
        {
            minPos.anchoredPosition = new Vector3(0f, (canvasRect.rect.height - mapImageHeight / scale) / 2f, 0f);
            maxPos.anchoredPosition = new Vector3(0f, -(canvasRect.rect.height - mapImageHeight / scale) / 2f, 0f);
        }
    }

    void Update()
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float clamp = Mathf.Clamp(mapRect.localScale.x + zoomSpeed * scroll * Time.unscaledDeltaTime, minZoom, maxZoom);
            mapRect.localScale = new Vector3(clamp, clamp, 0f);
            CurrentZoom = mapRect.localScale.x;

            leftBorder = canvasRect.rect.width / 2f - ((mapRect.rect.width / 2f) - minPos.anchoredPosition.x) * CurrentZoom;
            rightBorder = -leftBorder;
            bottomBorder = canvasRect.rect.height / 2f - ((mapRect.rect.height / 2f) - minPos.anchoredPosition.y) * CurrentZoom;
            topBorder = -bottomBorder;


            if (leftBorder > rightBorder)
            {
                leftBorder = rightBorder = 0f;
            }
            if (bottomBorder > topBorder)
            {
                bottomBorder = topBorder = 0f;
            }

            if (CurrentZoom <= maxZoom && CurrentZoom >= minZoom)
            {
                OnShipPositionChanged();
                Vector2 mousePos = Input.mousePosition / canvasRect.localScale.x;
                Vector2 newMousePos = (mousePos - middle);
                Vector2 deltaPos = (1f - (1f + zoomSpeed * scroll * Time.unscaledDeltaTime)) * newMousePos;
                MoveMap(deltaPos.x, deltaPos.y);
            }
        }

        if (CurrentZoom > 1f && Input.GetMouseButton(2))
        {
            float horizontal = Input.GetAxis("Mouse X") * moveMapSpeedModifier;
            float vertical = Input.GetAxis("Mouse Y") * moveMapSpeedModifier;
            MoveMap(horizontal, vertical);
        }
    }

    private void MoveMap(float horizontal, float vertical)
    {
        var left = -leftShipPos > leftBorder ? -leftShipPos : leftBorder;
        var right = rightShipPos < rightBorder ? rightShipPos : rightBorder;
        var bot = -bottomShipPos > bottomBorder ? -bottomShipPos : bottomBorder;
        var top = topShipPos < topBorder ? topShipPos : topBorder;
        mapRect.offsetMin = new Vector2(Mathf.Clamp(mapRect.offsetMin.x + horizontal * mapSpeed * Time.unscaledDeltaTime, left, right),
            Mathf.Clamp(mapRect.offsetMin.y + vertical * mapSpeed * Time.unscaledDeltaTime, bot, top));
        mapRect.offsetMax = mapRect.offsetMin;
    }

    private void OnShipPositionChanged()
    {
        leftShipPos = canvasRect.rect.width / 2f - ((mapRect.rect.width / 2f) - (ship.Rect.anchoredPosition.x + canvasRect.rect.width / 2f)) * CurrentZoom;
        bottomShipPos = canvasRect.rect.height / 2f - ((mapRect.rect.height / 2f) - (ship.Rect.anchoredPosition.y + canvasRect.rect.height / 2f)) * CurrentZoom;
        topShipPos = canvasRect.rect.height - bottomShipPos;
        rightShipPos = canvasRect.rect.width - leftShipPos;

        if (-leftShipPos > rightShipPos)
        {
            leftShipPos = rightShipPos = 0f;
        }
        if (-bottomShipPos > topShipPos)
        {
            bottomShipPos = topShipPos = 0f;
        }
        MoveMap(0f, 0f);
    }
}
