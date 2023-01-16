using UnityEngine;

public class Fill : MonoBehaviour
{
    public SpriteMask Mask;
    public Transform Transform;
    public Transform Sprite;
    public float Size = .2f;
    [SerializeField]
    private float fillState = 1f;

    public float FillState
    {
        get => fillState;
        set
        {
            fillState = value;
            if (Mathf.Approximately(FillState, 0f))
            {
                Mask.enabled = false;
            }
            else
            {
                Mask.enabled = true;
                Vector3 vec = Transform.localScale;
                vec.y = FillState;
                Transform.localScale = vec;

                vec = Sprite.localScale;
                vec.y = vec.x / FillState;
                Sprite.localScale = vec;

                vec = Transform.localPosition;
                vec.y = -Size * (1f - FillState);
                Transform.localPosition = vec;

                vec = Sprite.localPosition;
                vec.y = Size / FillState - Size;
                Sprite.localPosition = vec;
            }
        }
    }
}
