using FMODUnity;
using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BackgroundAudio : ParameterEventBase<EButtonState>
{
    public static BackgroundAudio Instance;

    [SerializeField]
    private StudioEventEmitter hoverEmitter = null;
    [SerializeField]
    private StudioEventEmitter shipClickEmitter = null;
    [SerializeField]
    private StudioEventEmitter tooltipSound = null;
    [SerializeField]
    private List<StudioEventEmitter> islandEmitters = null;
    [SerializeField]
    private List<StudioEventEmitter> sectionsEmitters = null;

    private IntermissionClickSound intermissionSound;
    private CrewManSounds crewManSounds;
    private MainSceneSound mainSceneSound;
    private MegaphoneSound megaphoneSound;
    private int silence;

    protected override void Awake()
    {
        base.Awake();

        Assert.IsNull(Instance);
        Instance = this;

        TryGetComponent(out intermissionSound);
        TryGetComponent(out crewManSounds);
        TryGetComponent(out mainSceneSound);
        TryGetComponent(out megaphoneSound);
    }

    public new void PlayEvent(EButtonState paramValue)
    {
        if (paramValue != EButtonState.Hover || silence < 1)
        {
            if (paramValue != EButtonState.Hover)
            {
                silence++;
                this.StartCoroutineActionAfterFrames(() => silence--, 3);
            }
            base.PlayEvent(paramValue);
        }
    }

    public void PlayEvent(EIntermissionClick state)
    {
        intermissionSound.PlayEvent(state);
    }

    public void PlayEvent(EMainSceneUI state)
    {
        mainSceneSound.PlayEvent(state);
    }

    public void PlayEvent(EMegaphoneVoice state)
    {
        megaphoneSound.PlayEvent(state);
    }

    public void PlayEvent(ECrewUIState state)
    {
        crewManSounds.PlayEvent(state);
    }

    public void PlayTooltip()
    {
        tooltipSound.Play();
    }

    public void StopMainSceneSound(bool allowFadeout)
    {
        mainSceneSound.Stop(allowFadeout);
    }
    public void StopMegaphoneSound(bool allowFadeout)
    {
        megaphoneSound.Stop(allowFadeout);
    }

    public void SpecialHover()
    {
        if (hoverEmitter != null)
        {
            hoverEmitter.Play();
        }
    }

    public void ShipClick()
    {
        shipClickEmitter.Play();
    }

    public void OnCameraViewChanged(ECameraView view)
    {
        bool playIsland = false;
        bool playSections = false;
        switch (view)
        {
            case ECameraView.Island:
                playIsland = true;
                break;
            case ECameraView.Sections:
                playSections = true;
                break;
        }
        foreach (var emitter in islandEmitters)
        {
            emitter.gameObject.SetActive(playIsland);
        }
        foreach (var emitter in sectionsEmitters)
        {
            emitter.gameObject.SetActive(playSections);
        }
    }
}
