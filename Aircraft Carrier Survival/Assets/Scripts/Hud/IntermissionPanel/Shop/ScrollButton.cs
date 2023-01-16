using UnityEngine;
using UnityEngine.UI;

public class ScrollButton : MonoBehaviour
{
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private float power = 1f;
    [SerializeField]
    private Scrollbar bar = null;

    private void Awake()
    {
        button.onClick.AddListener(Scroll);
    }

    private void Update()
    {
        button.interactable = bar.gameObject.activeInHierarchy && bar.value != (power > 0f ? 1f : 0f);
    }

    private void Scroll()
    {
        bar.value += power * Time.unscaledDeltaTime / bar.size;
        bar.value = Mathf.Clamp(bar.value, 0f, 1f);
    }
}
