using UnityEngine;
using UnityEngine.UI;

public class PulseAnim : MonoBehaviour
{
    [SerializeField]
    private Image image = null;

    [SerializeField]
    private float time = .5f;

    private float current;
    private Color color;

    private void Awake()
    {
        color = image.color;
    }

    private void OnEnable()
    {
        current = 0f;
    }

    private void Update()
    {
        if (Time.timeScale < .1f)
        {
            return;
        }
        current += Time.unscaledDeltaTime / time;
        while (current > 1f)
        {
            current--;
        }
        color.a = 4f * current * (current - 1f) + 1f;
        image.color = color;
    }

    public void SetShow(bool show)
    {
        enabled = show;
        image.gameObject.SetActive(show);
    }
}
