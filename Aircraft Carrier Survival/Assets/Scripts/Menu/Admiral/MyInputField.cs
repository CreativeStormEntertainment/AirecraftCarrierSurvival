using GambitUtils;
using UnityEngine;

public class MyInputField : CustomInputField
{
    [SerializeField]
    private bool allowDigit = false;

    protected override void EnableInput()
    {
        BasicInput.Instance.Enable();
    }

    protected override bool DisableInput()
    {
        var input = BasicInput.Instance;
        if (input.Enabled)
        {
            input.Disable();
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override bool CharacterAllowed(string text, int charIndex, char addedChar)
    {
        return base.CharacterAllowed(text, charIndex, addedChar) || (allowDigit && char.IsDigit(addedChar));
    }
}
