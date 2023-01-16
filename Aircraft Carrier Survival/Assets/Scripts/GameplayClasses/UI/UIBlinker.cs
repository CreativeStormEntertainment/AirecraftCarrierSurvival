using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UIBlinker : MonoBehaviour
{
	public float blinkingTime = 1f;
	[Range(0f, 1f)]
	public float alphaValue = 0.8f;


	private Image ownImage;


	private Image OwnImage
	{
		get
		{
			if (ownImage == null)
			{
				ownImage = GetComponent<Image>();
			}

			return ownImage;
		}
	}


	private void OnEnable()
	{
		StartCoroutine(FadeCoroutine());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}


	private IEnumerator FadeCoroutine()
	{
		////WaitForSeconds wait = new WaitForSeconds(blinkingTime);

		while (true)
		{
			OwnImage.CrossFadeAlpha(alphaValue, blinkingTime, false);
			yield return new WaitForSecondsRealtime(blinkingTime);
			OwnImage.CrossFadeAlpha(1f, blinkingTime, false);
			yield return new WaitForSecondsRealtime(blinkingTime);
		}
	}
}