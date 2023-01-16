using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;

public class TacticalFightUnitVisualization : MonoBehaviour
{
    public PlayableDirector BomberAbillityHit;
    public PlayableDirector BomberAbillityNotHit;
    public PlayableDirector FighterAbillityHit;
    public PlayableDirector FighterAbillityNotHit;
    public PlayableDirector TorpedoAbillityHit;
    public PlayableDirector TorpedoAbillityNotHit;

    public PlayableDirector BomberSecondAbillityHit;
    public PlayableDirector BomberSecondAbillityNotHit;
    public PlayableDirector FighterSecondAbillityHit;
    public PlayableDirector FighterSecondAbillityNotHit;
    public PlayableDirector TorpedoSecondAbillityHit;
    public PlayableDirector TorpedoSecondAbillityNotHit;

    public GameObject NormalVersion;
    public GameObject DamagedVersion;

    public void SetGameObjectActive(bool isHeavyDamaged)
    {
        if (isHeavyDamaged)
        {
            if(NormalVersion != null)
                NormalVersion.gameObject.SetActive(false);

            if(DamagedVersion != null) 
                DamagedVersion.gameObject.SetActive(true);
            else if (NormalVersion != null)
                NormalVersion.gameObject.SetActive(true);
        }
        else
        {
            if (NormalVersion != null)
                NormalVersion.gameObject.SetActive(true);
            else if (DamagedVersion != null)
                DamagedVersion.gameObject.SetActive(true);

            if (DamagedVersion != null)
                DamagedVersion.gameObject.SetActive(false);
        }
    }
}