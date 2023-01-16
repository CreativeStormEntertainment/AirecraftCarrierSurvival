using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MovieManager : MonoBehaviour, IPopupPanel
{
    public event Action VideoFinished = delegate { };

    public static MovieManager Instance;

    public EWindowType Type => EWindowType.Other;

    public List<TutorialVideoData> TutorialVideos => tutorialVideos;

#if ALLOW_CHEATS
    public int MoviesCount => mapVideos.Count;
#endif

    [SerializeField]
    private List<GameObject> canvasToHide = null;
    [SerializeField]
    private VideoPlayer player = null;
    [SerializeField]
    private VideoPlayer tutorialPlayer = null;
    [SerializeField]
    private VideoClip missionOverallSuccessClip = null;
    [SerializeField]
    private GameObject blackScreen = null;
    [SerializeField]
    private StudioEventEmitter missionOverallSuccessMusic = null;
    [SerializeField]
    private StudioEventEmitter missionOverallSuccessSounds = null;
    [SerializeField]
    private List<CaptionsData> campaignFinishCaptions = null;
    [SerializeField]
    private Camera canvasCamera = null;
    [SerializeField]
    private MovieSounds sounds = null;

    [SerializeField]
    private GameObject tutorialVideoWindow = null;
    [SerializeField]
    private Text tutorialTitle = null;
    [SerializeField]
    private Text captions = null;
    [SerializeField]
    private List<TutorialVideoData> tutorialVideos = null;
    [SerializeField]
    private Slider tutorialTimeSlider = null;
    [SerializeField]
    private MovieButton buttonPrefab = null;
    [SerializeField]
    private RectTransform buttonParent = null;

    [SerializeField]
    private GameObject toHide = null;

    [SerializeField]
    private GameObject captionObject = null;
    [SerializeField]
    private Text captionText = null;

    [SerializeField]
    private Text pause = null;

    [SerializeField]
    private string pauseText = "MoviePause";
    [SerializeField]
    private string unpauseText = "MovieUnpause";

    [SerializeField]
    private StudioEventEmitter music = null;

    private bool skip = false;
    private bool waitForAnyKeyOnVideoEnd = false;

    private List<VideoData> mapVideos;

    private TutorialVideoData currentTutorialVideo;
    private Dictionary<ETutorialType, MovieButton> tutorialVideosDict;
    private Dictionary<MovieButton, TutorialVideoData> buttonTutorialVideosDict;

    private TutorialVideoData prevVideo;

    private List<CaptionsData> movieCaptions;

    private int captionIndex;

    private bool ignore;
    private bool finishedSeeking;
    private int time;

    private int currentSoundIndex = -1;
    private FMODEvent fmodEvent;
    private FMODEvent fmodEvent2;

    private void Awake()
    {
        Instance = this;

        player.loopPointReached += OnVideoEndReached;
        tutorialPlayer.loopPointReached += OnVideoEndReached;

        mapVideos = new List<VideoData>();

        tutorialVideosDict = new Dictionary<ETutorialType, MovieButton>();
        buttonTutorialVideosDict = new Dictionary<MovieButton, TutorialVideoData>();
        var locMan = LocalizationManager.Instance;
        foreach (var data in tutorialVideos)
        {
            foreach (var caption in data.Captions)
            {
                caption.Text = null;
            }
            data.Button = Instantiate(buttonPrefab, buttonParent);
            data.Title = locMan.GetText(data.TitleID);
            data.Button.GetComponentInChildren<Text>().text = data.Title;
            tutorialVideosDict[data.Type] = data.Button;
            buttonTutorialVideosDict[data.Button] = data;
        }

        pauseText = locMan.GetText(pauseText);
        unpauseText = locMan.GetText(unpauseText);
    }

    private void LateUpdate()
    {
        if (ignore)
        {
            if (Input.GetMouseButton(0) || !finishedSeeking || (player.isPlaying && !player.isPrepared) || ((tutorialPlayer.isPlaying || tutorialPlayer.isPaused) && !tutorialPlayer.isPrepared))
            {
                return;
            }
            ignore = false;

            if (!player.isPlaying && !tutorialPlayer.isPlaying && !tutorialPlayer.isPaused)
            {
                if (++time > 3)
                {
                    OnVideoEndReached(null);
                }
            }
            captions.enabled = false;
        }

        if (currentTutorialVideo != null)
        {
            if (!tutorialPlayer.isPlaying && !tutorialPlayer.isPaused)
            {
                currentSoundIndex = -1;
                fmodEvent?.Release();
                fmodEvent = null;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (tutorialPlayer.isPaused)
                {
                    fmodEvent?.SetPause(false);

                    tutorialPlayer.Play();
                    SetPauseText(true);
                }
                else
                {
                    fmodEvent?.SetPause(true);

                    tutorialPlayer.Pause();
                    SetPauseText(false);
                }
            }

            RemoveSliderListener();
            if (!Input.GetMouseButton(0))
            {
                tutorialTimeSlider.value = (float)tutorialPlayer.time;
            }
            AddSliderListener();

            SetCaptions(currentTutorialVideo.Captions, tutorialPlayer, captions);
            return;
        }

        if (movieCaptions != null && player.isPlaying)
        {
            SetCaptions(movieCaptions, player, captionText);
        }

        if (skip)
        {
            skip = false;
            if (player.isPlaying)
            {
                OnVideoEndReached(player);
            }
        }

        if (player.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                skip = true;
                if (!blackScreen.activeSelf)
                {
                    waitForAnyKeyOnVideoEnd = false;
                }
            }
        }
        else
        {
            if (waitForAnyKeyOnVideoEnd && Input.anyKeyDown)
            {
                waitForAnyKeyOnVideoEnd = false;
                OnVideoEndReached(player);
            }
        }
    }

    private void OnDestroy()
    {
        fmodEvent?.Release();
    }

    public void Setup(SOTacticMap map)
    {
        mapVideos.Clear();
        mapVideos.AddRange(map.Movies);
        sounds.Setup(map.Movies);
    }

    public void Play(int movieIndex)
    {
        VideoStart();

        PlayVideo(player, mapVideos[movieIndex].Clip, true);
        sounds.Play(movieIndex);
        music.Play();
        player.SetDirectAudioMute(0, true);
        HudManager.Instance.SetCinematic(true);

        captionIndex = 0;
        captionText.enabled = false;
        movieCaptions = mapVideos[movieIndex].Captions;
        captionObject.SetActive(movieCaptions.Count > 0);
    }

    public void Play(ETutorialType type)
    {
        tutorialVideoWindow.gameObject.SetActive(true);
        toHide.SetActive(false);

        ChangeTutorialMovie(tutorialVideosDict[type]);

        var hudMan = HudManager.Instance;
        hudMan.SetCinematic(true);
        hudMan.PopupShown(this);
    }

    public void ChangeTutorialMovie(MovieButton button)
    {
        VideoStart();

        captionIndex = 0;
        if (prevVideo != null)
        {
            prevVideo.Button.SetSelected(false);
        }

        currentSoundIndex = -1;
        fmodEvent?.Release();
        fmodEvent = null;

        currentTutorialVideo = buttonTutorialVideosDict[button];
        currentTutorialVideo.Button.SetSelected(true);
        prevVideo = currentTutorialVideo;

        tutorialTitle.text = currentTutorialVideo.Title;
        captions.enabled = false;

        pause.gameObject.SetActive(true);
        SetPauseText(true);

        var clip = currentTutorialVideo.Clip;

        tutorialPlayer.time = 0f;
        PlayVideo(tutorialPlayer, clip, false);
        tutorialPlayer.SetDirectAudioVolume(0, currentTutorialVideo.Volume);

        RemoveSliderListener();
        tutorialTimeSlider.maxValue = (float)(clip.frameCount / clip.frameRate);
        AddSliderListener();
    }

    public void PlayMissionResultVideo()
    {
        blackScreen.SetActive(true);
        captionIndex = 0;
        movieCaptions = campaignFinishCaptions;
        captionObject.SetActive(movieCaptions.Count > 0);
        VideoStart();
        PlayVideo(player, missionOverallSuccessClip, true);
        HudManager.Instance.SetCinematic(true);
        HudManager.Instance.PopupShown(this);
        missionOverallSuccessMusic.Play();
        missionOverallSuccessSounds.Play();
    }

    public void CloseVideo()
    {
        missionOverallSuccessMusic.Stop();
        missionOverallSuccessSounds.Stop();
        sounds.Stop();
        music.Stop();
        player.Stop();
        tutorialPlayer.Stop();

        var hudMan = HudManager.Instance;
        hudMan.SetCinematic(false);
        hudMan.PopupHidden(this);

        SetShow(false);
        RemoveSliderListener();
        currentTutorialVideo = null;

        currentSoundIndex = -1;
        fmodEvent?.Release();
        fmodEvent = null;

        VideoFinished();

        tutorialVideoWindow.SetActive(false);
        toHide.SetActive(true);

        movieCaptions = null;
        captionObject.SetActive(false);
    }

    public void Hide()
    {
        CloseVideo();
    }

    private void OnVideoEndReached(VideoPlayer _)
    {
        if (waitForAnyKeyOnVideoEnd)
        {
            captionObject.SetActive(true);
            captionText.enabled = true;
            captionText.text = LocalizationManager.Instance.GetText("PressAnyButton");
        }
        else if (!ignore)
        {
            //sounds.Stop();

            player.SetDirectAudioVolume(0, 1f);
            player.SetDirectAudioMute(0, false);

            //player.Stop();
            //tutorialPlayer.Pause();

            if (currentTutorialVideo == null)
            {
                CloseVideo();
            }
            else
            {
                pause.gameObject.SetActive(false);
            }
        }
    }

    private void PlayVideo(VideoPlayer player, VideoClip clip, bool waitForAnyKeyOnVideoEnd)
    {
        this.waitForAnyKeyOnVideoEnd = waitForAnyKeyOnVideoEnd;
        SetShow(true);

        player.isLooping = false;
        player.clip = clip;
        player.Play();
    }

    private void SetShow(bool show)
    {
        var hudMan = HudManager.Instance;
        if (show)
        {
            hudMan.OnPausePressed();
        }
        else
        {
            hudMan.OnPlayPressed();
        }
        if (currentTutorialVideo == null)
        {
            player.gameObject.SetActive(show);
            foreach (var canvas in canvasToHide)
            {
                canvas.SetActive(!show);
            }
            canvasCamera.gameObject.SetActive(!show);
        }
    }

    private void SetCaptions(List<CaptionsData> captions, VideoPlayer player, Text text)
    {
        while (captions.Count > captionIndex)
        {
            var data = captions[captionIndex];
            if (data.Timer > player.time)
            {
                break;
            }
            captionIndex++;
            text.enabled = true;
            data.Text = LocalizationManager.Instance.GetText(data.TextID);
            text.text = data.Text;
        }
        int index = captionIndex - 1;
        if (index != currentSoundIndex)
        {
            currentSoundIndex = index;

            fmodEvent?.Release();

            var data = captions[currentSoundIndex];
            if (string.IsNullOrWhiteSpace(data.FmodEvent))
            {
                fmodEvent = null;
            }
            else
            {
                if (fmodEvent2 == null)
                {
                    fmodEvent2 = new FMODEvent(data.FmodEvent);
                }
                else
                {
                    fmodEvent2.ReplaceEvent(data.FmodEvent);
                }
                fmodEvent = fmodEvent2;
                fmodEvent.Play();
                if (player.isPaused)
                {
                    fmodEvent.SetPause(true);
                }

                fmodEvent.SetTimelinePosition((int)((player.time - data.Timer) * 1000d));
            }
        }
    }

    private void AddSliderListener()
    {
        RemoveSliderListener();
        tutorialTimeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void RemoveSliderListener()
    {
        tutorialTimeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        bool paused = tutorialPlayer.isPaused;

        if (!tutorialPlayer.isPlaying && !paused)
        {
            var clip = currentTutorialVideo.Clip;
            PlayVideo(tutorialPlayer, clip, false);
        }

        currentSoundIndex = -1;
        fmodEvent?.Release();
        fmodEvent = null;

        captionIndex = 0;
        tutorialPlayer.time = value;

        ignore = true;
        finishedSeeking = false;
        tutorialPlayer.seekCompleted -= OnSeekCompleted;
        tutorialPlayer.seekCompleted += OnSeekCompleted;

        if (paused)
        {
            tutorialPlayer.Pause();
        }
    }

    private void VideoStart()
    {
        time = 0;
        ignore = true;
        OnSeekCompleted(null);
    }

    private void OnSeekCompleted(VideoPlayer _)
    {
        tutorialPlayer.seekCompleted -= OnSeekCompleted;
        finishedSeeking = true;
    }

    private void SetPauseText(bool pause)
    {
        this.pause.text = pause ? pauseText : unpauseText;
    }
}
