using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PlayIntroMovie : MonoBehaviour
{
    [SerializeField]
    private List<VideoClip> clips = null;

    private int index;
    private VideoPlayer player;

    void Start()
    {
        player = GetComponent<VideoPlayer>();
        OnNextVideo(null);
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            OnNextVideo(null);
        }
    }

    private void OnNextVideo(VideoPlayer player2)
    {
        if (index >= clips.Count)
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            player.loopPointReached -= OnNextVideo;
            player.Stop();
            player.clip = clips[index];
            player.loopPointReached += OnNextVideo;
            player.Play();
            index++;
        }
    }
}
