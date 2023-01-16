using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SectionDamageNotifierManager : MonoBehaviour
{
    public static SectionDamageNotifierManager Instance;

    [SerializeField]
    private float notifyTime = 30f;

    [SerializeField]
    private AudioQueue audioQueue = null;

    private Dictionary<EIssue, HashSet<SectionSegment>> container;
    private HashSet<EAudio> randEvents;
    private HashSet<EAudio> queued;

    private float timer;
    private bool sectionView;

    private void Awake()
    {
        Instance = this;

        container = new Dictionary<EIssue, HashSet<SectionSegment>>();
        for (EIssue i = default; i < EIssue.Any; i++)
        {
            container.Add(i, new HashSet<SectionSegment>());
        }
        randEvents = new HashSet<EAudio>();
        queued = new HashSet<EAudio>();
    }

    private void Start()
    {
        var camMan = CameraManager.Instance;
        camMan.ViewChanged += OnViewChanged;
        OnViewChanged(camMan.CurrentCameraView);

        WorldMap.Instance.Toggled += OnWorldMapToggled;
    }

    private void Update()
    {
        if (Time.timeScale > 0f && !sectionView)
        {
            if (HasAny())
            {
                timer -= Time.unscaledDeltaTime;
                if (timer < 0f)
                {
                    timer = notifyTime;

                    randEvents.Clear();
                    if (container[EIssue.Fire].Count > 0)
                    {
                        randEvents.Add(EAudio.Fire);
                    }
                    if (container[EIssue.Flood].Count > 0)
                    {
                        randEvents.Add(EAudio.Flood);
                    }
                    if (container[EIssue.Fault].Count > 0)
                    {
                        randEvents.Add(EAudio.Fault);
                    }
                    if (container[EIssue.Injured].Count > 0)
                    {
                        randEvents.Add(EAudio.Injured);
                    }
                    EAudio audio = EAudio.Generate;
                    do
                    {
                        queued.Remove(audio);
                        audio = RandomUtils.GetRandom(randEvents);
                    }
                    while (queued.Contains(audio) && randEvents.Count > 1);
                    if (queued.Contains(audio))
                    {
                        timer = 1f;
                        return;
                    }
                    queued.Add(audio);
                    audioQueue.Queue(audio, () => queued.Remove(audio));
                }
            }
        }
    }

    public void SetSegmentIssue(SectionSegment segment, EIssue issue)
    {
        bool exist;
        switch (issue)
        {
            case EIssue.Fire:
                exist = segment.Fire.Exists;
                break;
            case EIssue.Flood:
                exist = segment.IsFlooding();
                break;
            case EIssue.Fault:
                exist = segment.Damage.Exists;
                break;
            case EIssue.Injured:
                exist = segment.Injured();
                break;
            default:
                Assert.IsTrue(false);
                return;
        }

        if (exist)
        {
            if (!HasAny())
            {
                timer = 0f;
            }
            container[issue].Add(segment);
        }
        else
        {
            container[issue].Remove(segment);
        }
    }

    private bool HasAny()
    {
        foreach (var set in container.Values)
        {
            if (set.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    private void OnViewChanged(ECameraView view)
    {
        sectionView = view == ECameraView.Sections;
    }

    private void OnWorldMapToggled(bool toggled)
    {
        foreach (var set in container.Values)
        {
            set.Clear();
        }
    }
}
