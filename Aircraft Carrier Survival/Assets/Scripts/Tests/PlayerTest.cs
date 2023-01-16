using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class PlayerTest : MonoBehaviour
{
    public PlayableDirector dir;
    public PlayableDirector dir2;
    public PlayableDirector dirCameras;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartPlayback();
        }
    }

    void StartPlayback()
    {
        dir.Play();
        dir2.Play();
        dirCameras.Play();
    }
}
