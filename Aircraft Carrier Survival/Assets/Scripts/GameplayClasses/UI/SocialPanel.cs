using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SocialPanel : MonoBehaviour
{
    public void Facebook()
    {
        Application.OpenURL("https://www.facebook.com/creativeforgegames/");
    }

    public void Discord()
    {
        Application.OpenURL("https://discord.gg/6NbkCnq");

    }

    public void Steam()
    {
        Application.OpenURL("https://store.steampowered.com/app/1021100/Aircraft_Carrier_Survival/");
    }
}
