using UnityEngine;

public abstract class TutorialBlinker : MonoBehaviour
{
    private float time;
    private float endTime;
    private AnimationCurve curve;

    private void Update()
    {
        time += Time.unscaledDeltaTime;
        while (time > endTime)
        {
            time -= endTime;
        }
        SetColor(curve.Evaluate(time / endTime));
    }

    public void Setup(float startTime, float endTime, AnimationCurve curve)
    {
        time = startTime;
        this.endTime = endTime;
        this.curve = curve;

        gameObject.SetActive(true);
    }

    protected abstract void SetColor(float power);
}
