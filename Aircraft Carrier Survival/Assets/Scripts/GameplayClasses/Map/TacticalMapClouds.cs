using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AzureSky;
using UnityEngine.UI;

public class TacticalMapClouds : MonoBehaviour
{
    public static TacticalMapClouds Instance;

    public AzureEffectsController AzureEffectsController
    {
        get;
        private set;
    }

    [SerializeField]
    private RectTransform trans = null;
    [SerializeField]
    private AzureSkyController skyController = null;

    private Vector2 direction;
    private float speed;

    private GameObject clouds;
    private GameObject additionalClouds;
    private HashSet<Collider2D> colliders;
    private Image[] images;
    private Image[] additionalImages;

    private Dictionary<RectTransform, RectTransform> colliderMovement;

    private bool showClouds;

    private void Awake()
    {
        Instance = this;

        colliders = new HashSet<Collider2D>();
        colliderMovement = new Dictionary<RectTransform, RectTransform>();
        AzureEffectsController = skyController.GetComponent<AzureEffectsController>();
    }

    private void Update()
    {
        trans.anchoredPosition += direction * speed * Time.deltaTime;

        foreach (var pair in colliderMovement)
        {
            pair.Key.position = pair.Value.position;
        }
    }

    public void Setup(SOTacticMap map)
    {
        if (clouds != null)
        {
            Destroy(clouds.gameObject);
        }
        if (additionalClouds != null)
        {
            Destroy(additionalClouds.gameObject);
        }
        foreach (var collider in colliderMovement.Keys)
        {
            Destroy(collider.gameObject);
        }
        colliderMovement.Clear();

        colliders.Clear();
        if (map.CloudsPrefab == null)
        {
            images = null;
            enabled = false;
        }
        else
        {
            enabled = true;
            trans.anchoredPosition = Vector2.zero;
            clouds = SpawnClouds(map.CloudsPrefab);
            images = clouds.GetComponentsInChildren<Image>(true);
            if (SaveManager.Instance.Data.MissionRewards.ActiveBuff == ETemporaryBuff.CloudyWeather && map.AdditionalCloudsPrefab != null)
            {
                additionalClouds = SpawnClouds(map.AdditionalCloudsPrefab);
                additionalImages = additionalClouds.GetComponentsInChildren<Image>(true);
            }
            else
            {
                additionalClouds = null;
                additionalImages = null;
            }

            direction = map.CloudDirection.normalized;
            speed = map.CloudSpeed;

            SetShowClouds(showClouds);
        }
    }

    public void SetShowClouds(bool show)
    {
        showClouds = show;
        if (images != null)
        {
            foreach (var image in images)
            {
                image.gameObject.SetActive(show);
            }
        }
        if (additionalImages != null)
        {
            foreach (var image in additionalImages)
            {
                image.gameObject.SetActive(show);
            }
        }
    }

    public bool InClouds(RectTransform obj)
    {
        var collider = Physics2D.OverlapCircle(new Vector2(obj.position.x, obj.position.y), 5f, 1 << 9);
        return colliders.Contains(collider);
    }

    private GameObject SpawnClouds(GameObject prefab)
    {
        var result = Instantiate(prefab).GetComponent<RectTransform>();
        result.SetParent(trans);
        result.anchoredPosition = Vector2.zero;
        result.rotation = Quaternion.identity;

        foreach (var collider in result.GetComponentsInChildren<Collider2D>(true))
        {
            colliders.Add(collider);

            var colliderTrans = collider.GetComponent<RectTransform>();
            colliderMovement[colliderTrans] = colliderTrans.parent.GetComponent<RectTransform>();
            colliderTrans.SetParent(null);
        }
        return result.gameObject;
    }

    public void SetWeather(bool rain)
    {
        skyController.SetNewWeatherProfile(rain ? 0 : -1);
    }
}

