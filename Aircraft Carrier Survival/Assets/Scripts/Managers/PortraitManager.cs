using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PortraitManager : MonoBehaviour, IEnableable
{
    public static PortraitManager Instance;

    public int OfficersEnabled
    {
        get => officersEnabled;
        set
        {
            officersEnabled = value;
            for (int i = 0; i < officersPortrait.Count; i++)
            {
                officersPortrait[i].SetSelectedEnable((value & (1 << i)) != 0);
            }
        }
    }

    public Portrait PortraitPrefab;

    public RectTransform PortraitsRect;
    public float MaxDistBetweenPortraits = 32f;
    public Canvas Canvas;

    private List<Portrait> officersPortrait = new List<Portrait>();
    private int officersEnabled;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        officersEnabled = -1;
    }

    public void SetEnable(bool enable)
    {
        foreach (var portrait in officersPortrait)
        {
            portrait.SetEnable(enable);
        }
    }

    public void Setup(IslandsAndOfficersManager islandOfficerMan)
    {
        Portrait portrait;
        using (var enumer = IslandsAndOfficersManager.Instance.OfficersEnumerator)
        {
            while (enumer.MoveNext())
            {
                portrait = Instantiate(PortraitPrefab, Vector3.zero, Quaternion.identity, PortraitsRect);
                portrait.Init(enumer.Current);
                officersPortrait.Add(portrait);
                islandOfficerMan.IslandRooms[enumer.Current.CurrentIslandRoom.RoomType].AddOfficer(enumer.Current, true);
            }
        }
    }

    public void SelectOfficer(Officer officer)
    {
        var islandMan = IslandsAndOfficersManager.Instance;
        islandMan.PlayEvent(EIslandUIState.Click);
        if (islandMan.AssignOfficersMode)
        {
            if (officer.Cooldown == 0)
            {
                if (!officer.Assigned)
                {
                    islandMan.AssignOfficer(officer);
                }
                else if (!islandMan.DisableOfficerDeallocation)
                {
                    islandMan.RemoveOfficer(officer);
                }
            }
        }
        else if (!islandMan.BuffsPanelOpen)
        {
            CameraManager.Instance.SwitchMode(ECameraView.Island);
            islandMan.SelectedOfficer = officer;
        }
    }
}
