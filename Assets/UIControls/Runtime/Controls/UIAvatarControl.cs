using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Circular avatar that renders initials over a colour deterministically derived from the name,
    /// with an optional status dot. Use several together (overlapped) plus a "+N" avatar for an avatar
    /// group. Call <see cref="SetName"/> / <see cref="SetStatus"/> to update at runtime.
    /// </summary>
    public sealed class UIAvatarControl : MonoBehaviour
    {
        public enum Status
        {
            None,
            Online,
            Away,
            Busy,
            Offline,
        }

        [Header("Targets")]
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text initialsLabel;
        [SerializeField] private Image statusDot;

        [Header("Content")]
        [SerializeField] private string displayName = "Ada Lovelace";
        [SerializeField] private Status status = Status.Online;
        [Tooltip("If set, shown instead of the derived initials (e.g. \"+3\" for an overflow avatar).")]
        [SerializeField] private string overrideLabel = string.Empty;
        [Tooltip("Override the derived colour with a fixed one (e.g. for a '+N' overflow avatar).")]
        [SerializeField] private bool useCustomColor;
        [SerializeField] private Color customColor = new Color(0.3f, 0.36f, 0.5f, 1f);

        private static readonly Color[] Palette =
        {
            new Color(0.86f, 0.36f, 0.4f, 1f),
            new Color(0.35f, 0.55f, 0.85f, 1f),
            new Color(0.4f, 0.68f, 0.45f, 1f),
            new Color(0.7f, 0.5f, 0.85f, 1f),
            new Color(0.88f, 0.62f, 0.3f, 1f),
            new Color(0.3f, 0.7f, 0.7f, 1f),
        };

        public string DisplayName => displayName;

        private void OnEnable()
        {
            Render();
        }

        public void SetName(string name)
        {
            displayName = name;
            Render();
        }

        public void SetStatus(Status value)
        {
            status = value;
            Render();
        }

        /// <summary>Set a literal label (e.g. "+3") and a fixed colour for an overflow avatar.</summary>
        public void SetOverflow(string label, Color color)
        {
            useCustomColor = true;
            customColor = color;
            if (initialsLabel != null) initialsLabel.text = label;
            if (background != null) background.color = color;
            if (statusDot != null) statusDot.gameObject.SetActive(false);
        }

        private void Render()
        {
            if (initialsLabel != null)
            {
                initialsLabel.text = string.IsNullOrEmpty(overrideLabel) ? Initials(displayName) : overrideLabel;
            }

            if (background != null)
            {
                background.color = useCustomColor ? customColor : ColorFor(displayName);
            }

            if (statusDot != null)
            {
                var show = status != Status.None;
                statusDot.gameObject.SetActive(show);
                if (show)
                {
                    statusDot.color = StatusColor(status);
                }
            }
        }

        private static string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "?";
            }

            var parts = name.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return parts[0].Substring(0, 1).ToUpperInvariant();
            }

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static Color ColorFor(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Palette[0];
            }

            var hash = 0;
            foreach (var c in name)
            {
                hash = (hash * 31 + c) & 0x7fffffff;
            }

            return Palette[hash % Palette.Length];
        }

        private static Color StatusColor(Status status)
        {
            switch (status)
            {
                case Status.Online: return new Color(0.3f, 0.78f, 0.45f, 1f);
                case Status.Away: return new Color(0.9f, 0.7f, 0.3f, 1f);
                case Status.Busy: return new Color(0.88f, 0.34f, 0.34f, 1f);
                case Status.Offline: return new Color(0.5f, 0.55f, 0.62f, 1f);
                default: return Color.clear;
            }
        }
    }
}
