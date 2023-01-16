using System;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    public Button PlusButton => plusButton;
    public Button MinusButton => minusButton;

    public int Value
    {
        get => int.Parse(counterInput.text);
        set
        {
            counterInput.onValidateInput = null;
            counterInput.text = value.ToString();
            counterInput.onValidateInput = OnValidateInput;
        }
    }

    [SerializeField]
    private Button plusButton = null;
    [SerializeField]
    private Button minusButton = null;
    [SerializeField]
    private InputField counterInput = null;

    private Action<int> counterValueChanged;

    public void Init(Action<int> counterValueChanged)
    {
        this.counterValueChanged = counterValueChanged;

        plusButton.onClick.AddListener(() => OnCounterClicked(true));
        minusButton.onClick.AddListener(() => OnCounterClicked(false));

        counterInput.text = "1";

        counterInput.onValidateInput = OnValidateInput;
        counterInput.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string text)
    {
        if (int.TryParse(text, out int value))
        {
            counterValueChanged(value);
        }
        else
        {
            counterValueChanged(0);
        }
    }

    private void OnCounterClicked(bool plus)
    {
        int newValue = Value + (plus ? 1 : -1);
        Value = newValue;
        counterValueChanged(newValue);
    }

    private char OnValidateInput(string input, int length, char addedChar)
    {
        if (char.IsDigit(addedChar) || (addedChar == '-' && length == 0))
        {
            return addedChar;
        }
        else
        {
            return '\0';
        }
    }
}
