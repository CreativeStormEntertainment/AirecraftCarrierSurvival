using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManeuversList", menuName = "Datas/PlayerManeuversList", order = 2)]
public class ManeuversList : ScriptableObject
{
    public List<PlayerManeuverData> Maneuvers;
    public PlayerManeuverData MidwayCustomManeuver;
    public PlayerManeuverData MagicCustomManeuver;
    public int AdmiralManeuversCount;
}
