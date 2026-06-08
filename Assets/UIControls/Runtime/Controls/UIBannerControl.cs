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
    /// <see cref="Dismiss"/> animates it out and raises <see cref="OnDismissed"/>. Visibility is driven
    /// by the <see cref="CanvasGroup"/> (alpha / blocksRaycasts), so the control's own GameObject stays
    /// active and never fights its enable callbacks.
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

        private Vector2 shownPos;
        private bool initialized;
        private bool visible;

        public UnityEvent OnDismissed => onDismissed;
        public bool IsVisible => visible;

        private void Awake()
        {
            if (dismissButton != null) dismissButton.OnClick.AddListener(Dismiss);
            Initialize();
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            if (root != null) shownPos = root.anchoredPosition;
            HideInstant();
        }

        public void Show(BannerType type, string message)
        {
            Initialize();

            var color = ColorFor(type);
            if (background != null) background.color = new Color(color.r, color.g, color.b, 0.18f);
            if (accentBar != null) accentBar.color = color;
            if (iconLabel != null) { iconLabel.text = IconFor(type); iconLabel.color = color; }
            if (messageLabel != null) messageLabel.text = message;

            visible = true;

            if (root != null)
            {
                root.DOKill();
                root.anchoredPosition = shownPos + new Vector2(0f, slideFrom);
                UIDOTweenUtility.TweenAnchoredPosition(root, shownPos, duration).SetEase(Ease.OutCubic).SetUpdate(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.blocksRaycasts = true;
                UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, 1f, duration).SetUpdate(true);
            }
        }

        public void Dismiss()
        {
            if (!visible)
            {
                return;
            }

            visible = false;

            if (root != null)
            {
                root.DOKill();
                UIDOTweenUtility.TweenAnchoredPosition(root, shownPos + new Vector2(0f, slideFrom), duration).SetEase(Ease.InCubic).SetUpdate(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.blocksRaycasts = false;
                UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, 0f, duration).SetUpdate(true)
                    .OnComplete(() => onDismissed.Invoke());
            }
            else
            {
                onDismissed.Invoke();
            }
        }

        private void HideInstant()
        {
            visible = false;
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }

            if (root != null)
            {
                root.anchoredPosition = shownPos + new Vector2(0f, slideFrom);
            }
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
