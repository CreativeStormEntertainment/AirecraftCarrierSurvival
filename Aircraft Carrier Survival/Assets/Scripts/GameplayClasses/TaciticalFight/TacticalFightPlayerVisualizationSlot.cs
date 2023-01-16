using UnityEngine;
using System.Collections;
using Cinemachine;

public class TacticalFightPlayerVisualizationSlot : MonoBehaviour
{
    CinemachineVirtualCamera playerPlaneCamera;

    TacticalFightUnitVisualization bomberVisualization;
    TacticalFightUnitVisualization fighterVisualization;
    TacticalFightUnitVisualization torpedoVisualization;

    TacticalFightUnitVisualization currentlySelectedVisualization;

    private void Awake()
    {
        playerPlaneCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);
        bomberVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[0];
        fighterVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[1];
        torpedoVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[2];
    }


    public void ActivateVisualizationSlot(TacticalFightPlayerUnit playerToSetVisualization)
    {
        playerPlaneCamera.gameObject.SetActive(true);

        switch (playerToSetVisualization.GetPlaneType())
        {
            case (ETacticalFightPlayerPlaneType.Bomber):
                bomberVisualization.gameObject.SetActive(true);
                bomberVisualization.SetGameObjectActive(false);
                currentlySelectedVisualization = bomberVisualization;
                break;

            case (ETacticalFightPlayerPlaneType.Fighter):
                fighterVisualization.gameObject.SetActive(true);
                fighterVisualization.SetGameObjectActive(false);
                currentlySelectedVisualization = fighterVisualization;
                break;

            case (ETacticalFightPlayerPlaneType.Torpedo):
                torpedoVisualization.gameObject.SetActive(true);
                torpedoVisualization.SetGameObjectActive(false);
                currentlySelectedVisualization = torpedoVisualization;
                break;
        }
    }
   
    public void DeaactivateVisualizationSlot()
    {
        playerPlaneCamera.gameObject.SetActive(false);

        bomberVisualization.gameObject.SetActive(false);
        fighterVisualization.gameObject.SetActive(false);
        torpedoVisualization.gameObject.SetActive(false); 

    }

    public TacticalFightUnitVisualization GetCurrentlySelectedVisualization()
    {
        return currentlySelectedVisualization;
    }
}
