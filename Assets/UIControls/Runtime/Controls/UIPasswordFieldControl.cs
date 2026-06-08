using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Password input with a show/hide (eye) toggle and a live strength meter. Wraps a
    /// <see cref="TMP_InputField"/>: the toggle flips its content type between password and visible
    /// text; each edit recomputes a 0–1 strength (length + character classes) that drives the meter
    /// fill width, colour and label. Raises <see cref="OnStrengthChanged"/>.
    /// </summary>
    public sealed class UIPasswordFieldControl : MonoBehaviour
    {
        [Serializable]
        public sealed class FloatEvent : UnityEvent<float>
        {
        }

        [Header("Targets")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private UIButtonControl toggleButton;
        [SerializeField] private TMP_Text toggleLabel;
        [SerializeField] private RectTransform strengthFill;
        [SerializeField] private TMP_Text strengthLabel;

        [Header("Style")]
        [SerializeField] private Color weakColor = new Color(0.88f, 0.34f, 0.34f, 1f);
        [SerializeField] private Color fairColor = new Color(0.9f, 0.66f, 0.3f, 1f);
        [SerializeField] private Color strongColor = new Color(0.3f, 0.75f, 0.45f, 1f);

        [Header("Events")]
        [SerializeField] private FloatEvent onStrengthChanged = new FloatEvent();

        private bool revealed;

        public FloatEvent OnStrengthChanged => onStrengthChanged;
        public string Password => inputField != null ? inputField.text : string.Empty;

        private void Awake()
        {
            if (toggleButton != null) toggleButton.OnClick.AddListener(ToggleReveal);
            if (inputField != null) inputField.onValueChanged.AddListener(_ => Refresh());
        }

        private void OnEnable()
        {
            ApplyReveal();
            Refresh();
        }

        public void ToggleReveal()
        {
            revealed = !revealed;
            ApplyReveal();
        }

        private void ApplyReveal()
        {
            if (inputField != null)
            {
                inputField.contentType = revealed ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
                inputField.ForceLabelUpdate();
            }

            if (toggleLabel != null)
            {
                toggleLabel.text = revealed ? "Hide" : "Show";
            }
        }

        private void Refresh()
        {
            var strength = Evaluate(Password);

            if (strengthFill != null)
            {
                var parent = strengthFill.parent as RectTransform;
                var fullWidth = parent != null ? parent.rect.width : 300f;
                strengthFill.sizeDelta = new Vector2(fullWidth * strength, strengthFill.sizeDelta.y);

                var img = strengthFill.GetComponent<Image>();
                if (img != null) img.color = ColorFor(strength);
            }

            if (strengthLabel != null)
            {
                strengthLabel.text = Password.Length == 0 ? "" : LabelFor(strength);
                strengthLabel.color = ColorFor(strength);
            }

            onStrengthChanged.Invoke(strength);
        }

        private static float Evaluate(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return 0f;
            }

            var score = 0;
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;

            var hasLower = false; var hasUpper = false; var hasDigit = false; var hasSymbol = false;
            foreach (var c in password)
            {
                if (char.IsLower(c)) hasLower = true;
                else if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSymbol = true;
            }

            var classes = (hasLower ? 1 : 0) + (hasUpper ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSymbol ? 1 : 0);
            score += classes;

            return Mathf.Clamp01(score / 6f);
        }

        private Color ColorFor(float strength) => strength < 0.34f ? weakColor : strength < 0.67f ? fairColor : strongColor;

        private static string LabelFor(float strength) => strength < 0.34f ? "Weak" : strength < 0.67f ? "Fair" : "Strong";
    }
}
