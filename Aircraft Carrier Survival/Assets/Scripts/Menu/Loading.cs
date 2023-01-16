using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Image ProgressBar;
    public Image BG;
    public List<Sprite> BGList;

    private const float minTimeLoad = 3f;
    private float aspect = 0f;

    private Camera mainCamera;

    private void Start()
    {
        //load save game
        var saveMan = SaveManager.Instance;

        mainCamera = Camera.main;

        if (BGList.Count > 0)
        {
            BG.sprite = BGList[Random.Range(0, BGList.Count)];
        }
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        var asyncScene = LoadingManager.Instance.LoadAsync();
        asyncScene.allowSceneActivation = false;

        float time = 0f;
        do
        {
            ProgressBar.fillAmount = (asyncScene.progress + (Mathf.Min(time / minTimeLoad, .9f))) / 2f;
            yield return null;
            time += Time.unscaledDeltaTime;
        }
        while (time < minTimeLoad);

        asyncScene.allowSceneActivation = true;
        while (!asyncScene.isDone)
        {
            ProgressBar.fillAmount = asyncScene.progress;
            yield return null;
        }
    }

    private void Update()
    {
        var newAspect = mainCamera.aspect;
        if (aspect == newAspect)
        {
            return;
        }

        aspect = newAspect;
        if (aspect >= (21f / 9f))
        {
            BG.rectTransform.localScale = new Vector3(1.75f, 1.75f, 1f);
            BG.rectTransform.anchoredPosition = new Vector2(0f, -44f);
            return;
        }
        BG.rectTransform.anchoredPosition = Vector2.zero;
        if (aspect >= (16f / 9f))
        {
            BG.rectTransform.localScale = Vector3.one;
            return;
        }
        if (aspect >= (4f / 3f))
        {
            BG.rectTransform.localScale = new Vector3(1.43f, 1.43f, 1f);
            return;
        }
        if (aspect >= (5f / 4f))
        {
            BG.rectTransform.localScale = new Vector3(1.42f, 1.42f, 1f);
            return;
        }
    }
}