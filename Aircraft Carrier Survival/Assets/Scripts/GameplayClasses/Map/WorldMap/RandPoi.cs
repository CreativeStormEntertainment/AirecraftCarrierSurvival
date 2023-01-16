using UnityEngine;

public class RandPoi : MonoBehaviour
{
    private RectTransform trans;

    public bool Setup(float posX, float posY, float sectorSize)
    {
        trans = GetComponent<RectTransform>();

        trans.anchoredPosition = new Vector2(posX, posY);
        return CheckLand(sectorSize);
    }
    private bool CheckLand(float sectorSize)
    {
        #region Calculation of the diagonal of the sector
        float sectorHalfSize = sectorSize / 2;

        Vector2 LD = new Vector2(
            (trans.anchoredPosition.x > 0 ? trans.anchoredPosition.x - sectorHalfSize : trans.anchoredPosition.x + sectorHalfSize),
            (trans.anchoredPosition.y > 0 ? trans.anchoredPosition.y - sectorHalfSize : trans.anchoredPosition.y + sectorHalfSize));

        Vector2 RU = new Vector2(
            (trans.anchoredPosition.x > 0 ? trans.anchoredPosition.x + sectorHalfSize : trans.anchoredPosition.x - sectorHalfSize),
            (trans.anchoredPosition.y > 0 ? trans.anchoredPosition.y + sectorHalfSize : trans.anchoredPosition.y - sectorHalfSize));
        #endregion

        return WorldMap.Instance.IsLandOnTheWay(LD, RU);
    }
}
