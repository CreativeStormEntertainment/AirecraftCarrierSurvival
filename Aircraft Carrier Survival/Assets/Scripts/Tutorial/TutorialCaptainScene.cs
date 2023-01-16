using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCaptainScene : TutorialPart
{
    private void Start()
    {
        SetupStep(0);
    }

    private void Update()
    {
        if (steps[currentStep].IsPressAnyMode && Input.anyKeyDown)
        {
            switch (currentStep)
            {
                case 0:
                    Hide();
                    break;
            }
        }
    }
}
