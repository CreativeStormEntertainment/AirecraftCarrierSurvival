using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Assertions;

public abstract class MapWaypoint : MonoBehaviour
{
    private const float SegmentLength = 30f;

    protected List<GameObject> trackList = new List<GameObject>();
    protected List<Image> trackImagesList = new List<Image>();
    protected List<GameObject> trackSegments = new List<GameObject>();
    protected List<GameObject> taskSegments = new List<GameObject>();

    private int segmentCount = 0;
    private float length = 0f;

    private RectTransform rectTransform = null;

    public RectTransform RectTransform => rectTransform ?? (rectTransform = transform as RectTransform);

    public List<PathNode> Path
    {
        get;
        set;
    }

    public int CurrentPathNode
    {
        get;
        set;
    }

    public bool SetNextPathNode()
    {
        CurrentPathNode++;
        return CurrentPathNode == Path.Count;
    }

    public void AddTrack(GameObject track)
    {
        trackList.Add(track);
        trackImagesList.Add(track.GetComponent<Image>());
    }

    public void SetTracksColor(Color color)
    {
        foreach (Image image in trackImagesList)
        {
            image.color = color;
        }
    }

    public void DestroyAllTracks()
    {
        foreach (GameObject t in trackList)
        {
            Destroy(t);
        }
        trackList.Clear();
        trackImagesList.Clear();
    }

    public void DestroyTaskSegments()
    {
        foreach (GameObject t in trackSegments)
        {
            Destroy(t);
        }
        trackSegments.Clear();
    }

    public void SetPath(List<PathNode> path)
    {
        Assert.IsNotNull(path);
        Path = path;
    }

    public void SpawnRoadTrack(List<PathNode> path, GameObject roadPrefab, RectTransform trackParent, Sprite sprite, Color color)
    {
        if (path.Count == 2)
        {
            Vector2 diff = path[0].Position - path[1].Position;
            float dist = diff.magnitude;
            int segmentsCount = (int)(dist / 30f);
            for (int i = 0; i <= segmentsCount; i++)
            {
                var roadTransform = Instantiate(roadPrefab, trackParent).transform as RectTransform;
                roadTransform.gameObject.SetActive(true);
                roadTransform.anchoredPosition = Vector2.Lerp(path[0].Position, path[1].Position, (float)i / segmentsCount);
                var image = roadTransform.GetComponent<Image>();
                image.color = color;
                AddTrack(roadTransform.gameObject);
            }
        }
        else
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (i % 3 != 0)
                {
                    continue;
                }
                var roadTransform = Instantiate(roadPrefab, trackParent).transform as RectTransform;
                roadTransform.gameObject.SetActive(true);
                roadTransform.anchoredPosition = path[i].Position;
                var image = roadTransform.GetComponent<Image>();
                image.sprite = sprite;
                image.color = color;
                AddTrack(roadTransform.gameObject);
            }
        }
    }

    public void RefreshRoadTrack(Vector2 pos)
    {
        if (Path.Count == 2)
        {
            Vector2 diff = pos - Path[0].Position;
            float dist = diff.magnitude;
            int segmentsCount = (int)(dist / 30f);
            for (int i = 0; i < trackList.Count; i++)
            {
                trackList[i].gameObject.SetActive(i > segmentsCount);
            }
        }
        else
        {
            for (int i = 0; i < trackList.Count; i++)
            {
                trackList[i].gameObject.SetActive(i > CurrentPathNode / 3);
            }
        }
    }

    public void SpawnTrack(Vector2 vFrom, Vector2 vTo, GameObject roadPrefab, RectTransform trackParent, Sprite sprite1, Sprite sprite2, Color color, bool isPlaneTrack)
    {
        length = Vector2.Distance(vFrom, vTo);
        segmentCount = (int)(length / SegmentLength);
        float xDelta = (vTo.x - vFrom.x) / segmentCount;
        float yDelta = (vTo.y - vFrom.y) / segmentCount;
        Vector3 destinatedDirection = (vTo - vFrom).normalized;

        var rot = Quaternion.Euler(0f, 0f, Vector3.Angle(Vector3.right, destinatedDirection) * Mathf.Sign(destinatedDirection.y));
        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 roadPos = new Vector2(vFrom.x + xDelta * (i + 1), vFrom.y + yDelta * (i + 1));

            var roadTransform = GameObject.Instantiate(roadPrefab, roadPos, Quaternion.identity, trackParent).transform as RectTransform;
            var image = roadTransform.GetComponent<Image>();
            image.sprite = i % 2 == 0 ? sprite1 : sprite2;
            image.color = color;
            roadTransform.anchoredPosition = roadPos;
            roadTransform.rotation = rot;
            roadTransform.localScale = Vector3.one;

            var size = image.sprite.rect.size;
            if (sprite1 != sprite2)
            {
                size *= .7f;
            }

            if (isPlaneTrack)
            {
                size.y /= 2f;
            }
            roadTransform.sizeDelta = size;

            AddTrack(roadTransform.gameObject);
        }
    }
}
