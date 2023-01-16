using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TacticalFightEnemyUnitButton : MonoBehaviour
{
    TacticalFightEnemyUnit enemyAcquiredToButton;
    Image buttonImage;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnTacticalFightEnemyUnitButtonClick);
        buttonImage = GetComponent<Image>();
    }

    private void Start()
    {

    }

    private void OnTacticalFightEnemyUnitButtonClick()
    {
    }

    public void SetEnemyUnitButton(TacticalFightEnemyUnit enemyUnit)
    {
        enemyAcquiredToButton = enemyUnit;
        GetComponentInChildren<Text>().text = enemyAcquiredToButton.gameObject.name;
    }

    public TacticalFightEnemyUnit GetEnemyAcquiredToButton()
    {
        return enemyAcquiredToButton;
    }

    public void OnEnemyUnitMark()
    {
        buttonImage.color = new Color(255, 165, 0);
    }

    public void OnEnemyUnitUnMark()
    {
        buttonImage.color = Color.white;
    }
}
