using System;
using System.Collections.Generic;

[Serializable]
public class TutorialStep
{
    public bool IsPressAnyMode = false;
    public bool PressAnyTextVisible = true;
    public string TextID = "";
    public EVoiceOver VO;
    public List<HighlightTask> tasks = new List<HighlightTask>();
    public List<TutorialBlinker> Highlights;

    public ArrowPositions arrowPositions;
    public EHelperArrow HelperArrow;
}
