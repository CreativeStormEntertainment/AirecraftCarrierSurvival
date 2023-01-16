using UnityEngine;
using UnityEngine.UI;

public class IconRotate : MonoBehaviour
{
    [SerializeField]
    private Button button = null;

    [SerializeField]
    private float time = 0.5f;

    private float from;
    private float to;
    private float current;

    private void Awake()
    {
        button.onClick.AddListener(Animate);
        from = -180f;
        to = 0f;

        enabled = false;
    }

    private void Update()
    {
        current += Time.unscaledDeltaTime / time;
        if (current >= 1f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, to);
            enabled = false;
            return;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(from, to, (2f - current) * current));
    }

    public void Animate()
    {
        if (enabled)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, to);
        }
        (from, to) = (-to, -from);
        current = 0f;

        enabled = true;
    }
}
