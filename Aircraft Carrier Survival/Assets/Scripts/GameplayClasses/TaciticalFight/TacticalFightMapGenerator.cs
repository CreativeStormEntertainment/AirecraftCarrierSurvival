using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalFightMapGenerator : MonoBehaviour
{
    [SerializeField]
    TacticalFightMapField fieldPrefab = null;
    [SerializeField]
    int gridWidth = 8;
    [SerializeField]
    int gridHeigth = 8;
    [SerializeField]
    float gap = 2f;
    [SerializeField]
    Vector2 startPosition = Vector2.zero;

    // check saving to array on generation

    public void GenerateMap()
    {
        float fieldWidth = fieldPrefab.GetComponent<RectTransform>().sizeDelta.x + gap;
        float fieldHeigth = fieldPrefab.GetComponent<RectTransform>().sizeDelta.y + gap;

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeigth; j++)
            {
                TacticalFightMapField instatiedField = Instantiate(fieldPrefab, transform);
                Vector2 fieldPosition = Vector2.zero;

                fieldPosition.x = startPosition.x + (i * (fieldWidth * 0.75f));

                if (i % 2 != 0)
                {
                    fieldPosition.y = startPosition.y + ((j * fieldHeigth) - (fieldHeigth / 2));
                }
                else
                {
                    fieldPosition.y = startPosition.y + (j * fieldHeigth);
                }
                
                instatiedField.GetComponent<RectTransform>().anchoredPosition = fieldPosition;

                instatiedField.SetPositionOfElement(i, j);
            }
        }
    }

    public int GetArrayWidth()
    {
        return gridWidth;
    }

    public int GetArrayHeigth()
    {
        return gridHeigth;
    }
}
