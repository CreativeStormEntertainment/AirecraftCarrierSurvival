using UnityEngine;

public class TimeSpeed : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f / 3f;

    private void Update()
    {
        if (Time.timeScale != speed)
        {
            Time.timeScale = speed;
        }
    }
}
