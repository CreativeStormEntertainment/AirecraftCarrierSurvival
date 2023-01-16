using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffBaskets", menuName = "ACS/BuffBaskets")]
public class BuffBaskets : ScriptableObject
{
    public List<EIslandBuff> GreenBasket;
    public List<EIslandBuff> OrangeBasket;
    public List<EIslandBuff> RedBasket;
}
