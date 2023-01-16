using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class OfficerDescription : MonoBehaviour
{
    [SerializeField]
    private GameObject airFrame = null;

    [SerializeField]
    private GameObject navyFrame = null;

    public static OfficerDescription Instance;

    private OfficerSetup officer;

    public OfficerSetup Officer
    {
        get
        {
            return officer;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void FillOfficerDescription(OfficerSetup officer)
    {
        this.officer = officer;

        if (!officer.HasAir)
        {
            airFrame.SetActive(false);
        }

        if (!officer.HasNavy)
        {
            navyFrame.SetActive(false);
        }
    }

}
