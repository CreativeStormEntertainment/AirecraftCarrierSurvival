using FMODUnity;
using UnityEngine;
using UnityEngine.Assertions;

public class Lift : MonoBehaviour
{
    public float ElevatorState
    {
        get => elevatorState;
        set
        {
            if (elevatorState != value)
            {
                if (value > .5f)
                {
                    if (liftUpSound != null)
                    {
                        liftUpSound.Play();
                    }
                }
                else
                {
                    if (liftDownSound != null)
                    {
                        liftDownSound.Play();
                    }
                }
                elevatorState = value;
                Assert.IsTrue(elevatorTimer >= 0f);
                Assert.IsTrue(elevatorTimer <= elevatorTime);
                elevatorTimer = elevatorTime - elevatorTimer;
            }
        }
    }

    [SerializeField]
    private SkinnedMeshRenderer elevator = null;
    [SerializeField]
    private float elevatorTime = 9.5f;
    [SerializeField]
    private StudioEventEmitter liftUpSound = null;
    [SerializeField]
    private StudioEventEmitter liftDownSound = null;

    private float elevatorState = 1f;
    private float elevatorTimer;
    private float elevatorBlendingValue;

    public void Setup()
    {
        elevatorTimer = elevatorTime;
    }

    public void UpdateLift(float deltaTime)
    {
        float start = 0f;
        float finish = 100f;
        if (ElevatorState == 1f)
        {
            start = 100f;
            finish = 0f;
        }
        elevatorTimer = Mathf.Clamp(elevatorTimer + deltaTime, 0f, elevatorTime);
        elevatorBlendingValue = Mathf.Lerp(start, finish, elevatorTimer / elevatorTime);
        elevator.SetBlendShapeWeight(0, elevatorBlendingValue);
    }
}
