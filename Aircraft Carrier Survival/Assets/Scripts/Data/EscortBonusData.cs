using UnityEngine;
using UnityEngine.UI;

public class EscortBonusData : MonoBehaviour
{
    public EStrikeGroupPassiveSkill Category => category;
    public Text Text => text;
    public int CurrentValue;

    [SerializeField]
    private EStrikeGroupPassiveSkill category = EStrikeGroupPassiveSkill.MaxSupplies;

    [SerializeField]
    private Text text = null;
}
