using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Empty-state placeholder: a large icon glyph, a title, a message and an optional call-to-action
    /// button (e.g. "Nothing here yet — Add the first item"). Toggle with <see cref="Show"/> /
    /// <see cref="Hide"/>; the action button raises <see cref="OnAction"/>.
    /// </summary>
    public sealed class UIEmptyStateControl : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text iconLabel;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private UIButtonControl actionButton;

        [Header("Events")]
        [SerializeField] private UnityEvent onAction = new UnityEvent();

        public UnityEvent OnAction => onAction;
        public bool IsVisible => root != null && root.activeSelf;

        private void Awake()
        {
            if (actionButton != null) actionButton.OnClick.AddListener(() => onAction.Invoke());
        }

        public void Show()
        {
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (visible) Show(); else Hide();
        }

        public void SetContent(string icon, string title, string message)
        {
            if (iconLabel != null) iconLabel.text = icon;
            if (titleLabel != null) titleLabel.text = title;
            if (messageLabel != null) messageLabel.text = message;
        }
    }
}
