using FMODUnity;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class DcCategoryButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public Sprite Sprite => sprite;
    public Button Button => button;

    [SerializeField]
    private Button button = null;
    [SerializeField]
    private Sprite sprite = null;
    [SerializeField]
    private StudioEventEmitter clickSound = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.BuffHoverChanged);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickSound.Play();
    }
}
