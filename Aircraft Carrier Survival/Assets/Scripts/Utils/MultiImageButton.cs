using UnityEngine;
using UnityEngine.UI;

public class MultiImageButton : Button
{
    [Header("MULTI IMAGE")]
    [SerializeField]
    private Image[] targetImages = new Image[0];
    [SerializeField]
    [Header("Include all states! (5x per targetImage)")]
    private Sprite[] targetImagesSprites = new Sprite[0];
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        for (int i = 0; i < targetImages.Length; ++i)
        {
            targetImages[i].sprite = targetImagesSprites[(int)state + i * 4];
        }
    }
}
