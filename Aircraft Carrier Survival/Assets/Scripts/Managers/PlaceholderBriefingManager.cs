using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PlaceholderBriefingManager : MonoBehaviour
{
    [SerializeField]
    private Image map = null;
    [SerializeField]
    private Text text = null;

    private void Start()
    {
        var mapData = SaveManager.Instance.TransientData.FabularTacticMap;
        map.sprite = mapData.Map;
        text.text = LocalizationManager.Instance.GetText(mapData.PlaceholderBriefingID);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                return;
            }
#endif
            LoadingManager.Instance.CurrentScene = ESceneType.Game;
        }
    }
}
