using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GambitUtils
{
    [RequireComponent(typeof(InputField))]
    public class CustomInputField : MonoBehaviour, IPointerClickHandler, ISelectHandler, IDeselectHandler
    {
        public event Action TextChanged = delegate { };
        public event Action<EInputFieldState> StateChanged = delegate { };

        public string Text
        {
            get;
            private set;
        }

        private InputField field;
        private Text innerText;
        private TextGenerationSettings textSettings;
        private RectTransform innerTextRect;
        private string draftText;

        private bool ignore;

        private bool inputActive;

        private void Start()
        {
            field.onValidateInput = OnValidateInput;
            field.onValueChanged.AddListener(OnValueChanged);
            field.onEndEdit.AddListener(OnEndEdit);

            innerText = field.textComponent;
            textSettings = innerText.GetGenerationSettings(Vector2.zero);
            textSettings.horizontalOverflow = HorizontalWrapMode.Overflow;
            innerTextRect = innerText.rectTransform;
        }

        public void Setup(string value)
        {
            field = GetComponent<InputField>();

            ignore = true;
            field.text = value;
            ignore = false;

            Text = value;
            draftText = value;
        }

        public void OnSelect(BaseEventData eventData)
        {
            Assert.IsNotNull(field, "Setup first");
            inputActive = DisableInput();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Assert.IsNotNull(field, "Setup first");
            if (inputActive)
            {
                EnableInput();
                inputActive = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StateChanged(EInputFieldState.Finished);
        }

        protected virtual void EnableInput()
        {

        }

        // should return true, if input was enabled
        protected virtual bool DisableInput()
        {
            return false;
        }

        protected virtual bool CharacterAllowed(string text, int charIndex, char addedChar)
        {
            return char.IsLetter(addedChar) || addedChar == ' ';
        }

        private char OnValidateInput(string text, int charIndex, char addedChar)
        {
            if (ignore)
            {
                return addedChar;
            }
            Assert.IsNotNull(field, "Setup first");

            if (CharacterAllowed(text, charIndex, addedChar) &&
                (innerTextRect.rect.width > (innerText.cachedTextGeneratorForLayout.GetPreferredWidth(text + addedChar, textSettings) / innerText.pixelsPerUnit)))
            {
                StateChanged(EInputFieldState.AllowedInput);
                return addedChar;
            }
            else
            {
                StateChanged(EInputFieldState.DisallowedInput);
                return '\0';
            }
        }

        private void OnValueChanged(string text)
        {
            if (text.Length < draftText.Length)
            {
                StateChanged(EInputFieldState.RemovedInput);
            }
            draftText = text;
            TextChanged();
        }

        private void OnEndEdit(string _)
        {
            if (string.IsNullOrWhiteSpace(field.text))
            {
                field.text = Text;
            }
            else
            {
                draftText = Text;
                Text = field.text;
            }
            StateChanged(EInputFieldState.Finished);
            TextChanged();
        }
    }
}
