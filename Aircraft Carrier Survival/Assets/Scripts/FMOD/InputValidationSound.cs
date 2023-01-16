using UnityEngine;

[RequireComponent(typeof(MyInputField))]
public class InputValidationSound : ParameterEventBase<EInputFieldState>
{
    protected override void Awake()
    {
        base.Awake();

        GetComponent<MyInputField>().StateChanged += PlayEvent;
    }
}
