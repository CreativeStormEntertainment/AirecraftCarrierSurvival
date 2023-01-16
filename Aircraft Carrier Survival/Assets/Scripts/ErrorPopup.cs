using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ErrorPopup : MonoBehaviour
{
    public static ErrorPopup Instance;

    [SerializeField]
    private GameObject container = null;

    [SerializeField]
    private Text text = null;

    [SerializeField]
    private Button button = null;

    [SerializeField]
    private List<GameObject> objectsToHide = null;
    [SerializeField]
    private List<VideoPlayer> videosToStop = null;

    private void Awake()
    {
        Instance = this;

        button.onClick.AddListener(() => Application.Quit());
    }

    public void Show(string text)
    {
        var hudMan = HudManager.Instance;
        hudMan.AcceptInput = false;
        hudMan.BlockTooltips = false;
        hudMan.OnPausePressed();

        container.SetActive(true);
        this.text.text = text;

        GameStateManager.Instance.StartCoroutineActionAfterFrames(() =>
            {
                this.transform.parent.gameObject.SetActive(true);

                foreach (var obj in objectsToHide)
                {
                    obj.SetActive(false);
                }

                foreach (var video in videosToStop)
                {
                    video.Stop();
                }
            }, 1);
    }
}
