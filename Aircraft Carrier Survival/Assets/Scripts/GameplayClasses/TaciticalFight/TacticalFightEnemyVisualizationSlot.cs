using UnityEngine;
using System.Collections;
using Cinemachine;

public class TacticalFightEnemyVisualizationSlot : MonoBehaviour
{
    TacticalFightEnemyUnit enemyAcquired;
    CinemachineVirtualCamera enemyCamera;
    TacticalFightUnitVisualization kagaVisualization;
    TacticalFightUnitVisualization kumaVisualization;
    TacticalFightUnitVisualization wakatakeVisualization;
    TacticalFightUnitVisualization yamatoVisualization;
    TacticalFightUnitVisualization zeroVisualization;
    TacticalFightUnitVisualization tankerVisualization;
    TacticalFightUnitVisualization currentlySelectedVisualization;


    private void Awake()
    {
        enemyCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);
        kagaVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[0];
        kumaVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[1];
        wakatakeVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[2];
        yamatoVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[3];
        zeroVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[4];
        tankerVisualization = GetComponentsInChildren<TacticalFightUnitVisualization>(true)[5];
    }

    public void SetEnemyForSlot(TacticalFightEnemyUnit enemy)
    {
        enemyAcquired = enemy;
    }

    public TacticalFightEnemyUnit GetEnemyAcquiredForSlot()
    {
        return enemyAcquired;
    }

    public void ActivateVisualizationSlot()
    {
        enemyCamera.gameObject.SetActive(true);

        switch (enemyAcquired.GetEnemyType())
        {
            case (ETacticalFightEnemyType.Kuma):
                kumaVisualization.gameObject.SetActive(true);
                currentlySelectedVisualization = kumaVisualization;
                kumaVisualization.SetGameObjectActive(enemyAcquired.GetIsHeavyDamaged());
                break;

            case (ETacticalFightEnemyType.Kaga):
                kagaVisualization.gameObject.SetActive(true);
                currentlySelectedVisualization = kagaVisualization;
                kagaVisualization.SetGameObjectActive(enemyAcquired.GetIsHeavyDamaged());
                break;

            case (ETacticalFightEnemyType.Yamato):
                yamatoVisualization.gameObject.SetActive(true);
                currentlySelectedVisualization = yamatoVisualization;
                yamatoVisualization.SetGameObjectActive(enemyAcquired.GetIsHeavyDamaged());
                break;

            case (ETacticalFightEnemyType.Wakatake):
                wakatakeVisualization.gameObject.SetActive(true);
                currentlySelectedVisualization = wakatakeVisualization;
                wakatakeVisualization.SetGameObjectActive(enemyAcquired.GetIsHeavyDamaged());
                break;

            case (ETacticalFightEnemyType.ZeroPlane):
                zeroVisualization.gameObject.SetActive(true);
                currentlySelectedVisualization = zeroVisualization;
                zeroVisualization.SetGameObjectActive(false);
                break;

            case (ETacticalFightEnemyType.Tanker):
                tankerVisualization.gameObject.SetActive(true);
                currentlySelectedVisualization = tankerVisualization;
                tankerVisualization.SetGameObjectActive(enemyAcquired.GetIsHeavyDamaged());
                break;
        }
    }

    public void DeaactivateVisualizationSlot()
    {
        enemyCamera.gameObject.SetActive(false);
    //    kagaVisualization.gameObject.SetActive(false);
    //    kumaVisualization.gameObject.SetActive(false);
    //    wakatakeVisualization.gameObject.SetActive(false);
    //    yamatoVisualization.gameObject.SetActive(false);
    //    zeroVisualization.gameObject.SetActive(false);
    }

    public TacticalFightUnitVisualization GetCurrentlySelectedVisualization()
    {
        return currentlySelectedVisualization;
    }
}
