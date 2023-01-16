using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SandboxMapTexturesCreator : MonoBehaviour
{
    [SerializeField]
    private Camera renderCamera = null;
    [SerializeField]
    private RenderTexture renderTexture = null;
    [SerializeField]
    private RenderTexture renderTextureMask = null;
    [SerializeField]
    private RenderTexture renderTextureLandMask = null;
    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private GameObject mapImages = null;
    [SerializeField]
    private GameObject mapMasks = null;
    [SerializeField]
    private GameObject landMasks = null;
    [SerializeField]
    private Texture2D landMasksTexture = null;
    [SerializeField]
    private Texture2D whiteTexture = null;

    public void ClearTexture()
    {
        Graphics.CopyTexture(whiteTexture, landMasksTexture);
    }

    public IEnumerator SetMapTextures(Vector2 offset, Action callback)
    {
        gameObject.SetActive(true);
        renderCamera.gameObject.SetActive(true);
        renderCamera.Render();
        rectTransform.anchoredPosition = -offset;
        mapMasks.SetActive(false);
        landMasks.SetActive(false);
        renderCamera.targetTexture = renderTexture;
        mapImages.SetActive(true);
        yield return null;
        yield return null;
        yield return null;
        renderCamera.targetTexture = renderTextureMask;
        mapMasks.SetActive(true);
        yield return null;
        renderCamera.targetTexture = renderTextureLandMask;
        landMasks.SetActive(true);
        yield return null;
        RenderTexture.active = renderTextureLandMask;
        landMasksTexture.ReadPixels(new Rect(0f, 0f, renderTextureLandMask.width, renderTextureLandMask.height), 0, 0);
        landMasksTexture.Apply();
        gameObject.SetActive(false);
        renderCamera.gameObject.SetActive(false);

        callback();
    }
}
