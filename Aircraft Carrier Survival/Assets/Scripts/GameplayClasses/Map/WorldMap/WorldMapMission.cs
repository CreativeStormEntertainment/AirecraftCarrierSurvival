using UnityEngine;

public class WorldMapMission : MonoBehaviour
{
    [SerializeField] private SOTacticMap tacticMap = null;
    private int id = -1;

    public int ID
    {
        get
        {
            if (id == -1)
            {
                id = int.Parse(gameObject.name.Split('#')[1]) - 1;
                return id;
            }
            else
            {
                return id;
            }
        }
    }
    public SOTacticMap TacticMap { get => tacticMap; }
    public bool IsEnable { get => gameObject.activeSelf; }
    public Vector2 AnchoredPosition { get => GetComponent<RectTransform>().anchoredPosition; }

    public void SetEnable(bool isEnable)
    {
        gameObject.SetActive(isEnable);
    }
}
