using System;
using DG.Tweening;
using TMPro;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Persistent inline banner / alert (info, success, warning, error). Unlike a toast it stays until
    /// dismissed: <see cref="Show"/> sets the type colour, icon and message and slides/fades it in;
    /// <see cref="Dismiss"/> animates it out and raises <see cref="OnDismissed"/>.
    /// </summary>
    public sealed class UIBannerControl : MonoBehaviour
    {
        public enum BannerType
        {
            Info,
            Success,
            Warning,
            Error,
        }

        [Header("Targets")]
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image background;
        [SerializeField] private Image accentBar;
        [SerializeField] private TMP_Text iconLabel;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private UIButtonControl dismissButton;

        [Header("Animation")]
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private float slideFrom = 40f;

        [Header("Events")]
        [SerializeField] private UnityEvent onDismissed = new UnityEvent();

        public UnityEvent OnDismissed => onDismissed;
        public bool IsVisible => root != null && root.gameObject.activeSelf;

        private Vector2 shownPos;

        private void Awake()
        {
            if (dismissButton != null) dismissButton.OnClick.AddListener(Dismiss);
            if (root != null) shownPos = root.anchoredPosition;
        }

        private void OnEnable()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (root != null) root.gameObject.SetActive(false);
        }

        public void Show(BannerType type, string message)
        {
            if (root == null)
            {
                return;
            }

            var color = ColorFor(type);
            if (background != null) background.color = new Color(color.r, color.g, color.b, 0.18f);
            if (accentBar != null) accentBar.color = color;
            if (iconLabel != null) { iconLabel.text = IconFor(type); iconLabel.color = color; }
            if (messageLabel != null) messageLabel.text = message;

            root.gameObject.SetActive(true);
            root.anchoredPosition = shownPos + new Vector2(0f, slideFrom);
            UIDOTweenUtility.TweenAnchoredPosition(root, shownPos, duration).SetEase(Ease.OutCubic).SetUpdate(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, 1f, duration).SetUpdate(true);
            }
        }

        public void Dismiss()
        {
            if (root == null || !root.gameObject.activeSelf)
            {
                return;
            }

            UIDOTweenUtility.TweenAnchoredPosition(root, shownPos + new Vector2(0f, slideFrom), duration).SetEase(Ease.InCubic).SetUpdate(true);
            if (canvasGroup != null)
            {
                UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, 0f, duration).SetUpdate(true)
                    .OnComplete(Finish);
            }
            else
            {
                Finish();
            }
        }

        private void Finish()
        {
            if (root != null) root.gameObject.SetActive(false);
            onDismissed.Invoke();
        }

        private static Color ColorFor(BannerType type)
        {
            switch (type)
            {
                case BannerType.Success: return new Color(0.3f, 0.75f, 0.45f, 1f);
                case BannerType.Warning: return new Color(0.92f, 0.7f, 0.3f, 1f);
                case BannerType.Error: return new Color(0.9f, 0.36f, 0.36f, 1f);
                default: return new Color(0.34f, 0.6f, 0.95f, 1f);
            }
        }

        private static string IconFor(BannerType type)
        {
            switch (type)
            {
                case BannerType.Success: return "✓";
                case BannerType.Warning: return "⚠";
                case BannerType.Error: return "✕";
                default: return "ℹ";
            }
        }
    }
}
