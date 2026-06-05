using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// One-time-code (OTP) input: a row of single-character cells. A single hidden
    /// <see cref="TMP_InputField"/> captures the keyboard — so paste, backspace and auto-advance
    /// all work for free — while the cells just render each character and highlight the active one.
    /// Fires <see cref="OnChanged"/> on every edit and <see cref="OnCompleted"/> when all cells fill.
    /// </summary>
    public sealed class UIOTPInputControl : MonoBehaviour, IPointerClickHandler
    {
        [Serializable]
        public sealed class CodeEvent : UnityEvent<string>
        {
        }

        [Header("Targets")]
        [Tooltip("Hidden input field that captures typing/paste. Its own text/caret should be invisible.")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Image[] cellBackgrounds = Array.Empty<Image>();
        [SerializeField] private TMP_Text[] cellLabels = Array.Empty<TMP_Text>();

        [Header("Behaviour")]
        [SerializeField] private bool digitsOnly = true;
        [Tooltip("Mask the displayed characters (show a dot instead of the value).")]
        [SerializeField] private bool mask;
        [SerializeField] private string maskChar = "•";

        [Header("Cell Colors")]
        [SerializeField] private Color emptyColor = new Color(0.18f, 0.22f, 0.31f, 1f);
        [SerializeField] private Color filledColor = new Color(0.22f, 0.28f, 0.4f, 1f);
        [SerializeField] private Color activeColor = new Color(0.24f, 0.55f, 0.95f, 1f);

        [Header("Events")]
        [SerializeField] private CodeEvent onChanged = new CodeEvent();
        [SerializeField] private CodeEvent onCompleted = new CodeEvent();

        public CodeEvent OnChanged => onChanged;
        public CodeEvent OnCompleted => onCompleted;
        public int Length => cellLabels != null ? cellLabels.Length : 0;
        public string Code => inputField != null ? inputField.text : string.Empty;
        public bool IsComplete => Code.Length >= Length;

        private void Awake()
        {
            if (inputField != null)
            {
                inputField.characterLimit = Length;
                if (digitsOnly)
                {
                    inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                }

                inputField.onValueChanged.AddListener(HandleValueChanged);
                inputField.onSelect.AddListener(_ => RefreshCells());
                inputField.onDeselect.AddListener(_ => RefreshCells());
            }
        }

        private void OnDestroy()
        {
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveListener(HandleValueChanged);
            }
        }

        private void OnEnable()
        {
            RefreshCells();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Focus();
        }

        public void Focus()
        {
            if (inputField != null)
            {
                inputField.ActivateInputField();
                inputField.caretPosition = inputField.text.Length;
            }
        }

        public void Clear(bool notify = true)
        {
            if (inputField == null)
            {
                return;
            }

            inputField.SetTextWithoutNotify(string.Empty);
            RefreshCells();

            if (notify)
            {
                onChanged?.Invoke(string.Empty);
            }
        }

        public void SetCode(string code, bool notify = true)
        {
            if (inputField == null)
            {
                return;
            }

            var clipped = code ?? string.Empty;
            if (clipped.Length > Length)
            {
                clipped = clipped.Substring(0, Length);
            }

            inputField.SetTextWithoutNotify(clipped);
            RefreshCells();

            if (notify)
            {
                onChanged?.Invoke(clipped);
                if (clipped.Length >= Length && Length > 0)
                {
                    onCompleted?.Invoke(clipped);
                }
            }
        }

        private void HandleValueChanged(string text)
        {
            RefreshCells();
            onChanged?.Invoke(text);

            if (text.Length >= Length && Length > 0)
            {
                onCompleted?.Invoke(text);
            }
        }

        private void RefreshCells()
        {
            var text = inputField != null ? inputField.text : string.Empty;
            var focused = inputField != null && inputField.isFocused;
            var activeIndex = Mathf.Min(text.Length, Length - 1);

            for (var i = 0; i < Length; i++)
            {
                if (cellLabels != null && i < cellLabels.Length && cellLabels[i] != null)
                {
                    if (i < text.Length)
                    {
                        cellLabels[i].text = mask ? maskChar : text[i].ToString();
                    }
                    else
                    {
                        cellLabels[i].text = string.Empty;
                    }
                }

                if (cellBackgrounds != null && i < cellBackgrounds.Length && cellBackgrounds[i] != null)
                {
                    Color color;
                    if (focused && i == activeIndex && text.Length < Length)
                    {
                        color = activeColor;
                    }
                    else if (i < text.Length)
                    {
                        color = filledColor;
                    }
                    else
                    {
                        color = emptyColor;
                    }

                    cellBackgrounds[i].color = color;
                }
            }
        }
    }
}
