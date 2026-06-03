using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A shared tooltip popup. Targets ask it to <see cref="Show"/> a bubble next to them at a
    /// preferred side; the bubble auto-flips to the opposite side if it would run off screen and
    /// clamps along the cross axis so it always stays within the canvas. Sizes itself to the text.
    /// Pair with <see cref="UITooltipTrigger"/> on hoverable elements.
    /// </summary>
    public sealed class UITooltipControl : MonoBehaviour
    {
        public enum Placement
        {
            Top,
            Bottom,
            Left,
            Right,
        }

        [Serializable]
        public sealed class ShownEvent : UnityEvent<string>
        {
        }

        [Header("Targets")]
        [Tooltip("The bubble that moves and shows the text. Should be a child of the canvas (or a high-level RectTransform).")]
        [SerializeField] private RectTransform bubble;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text label;
        [Tooltip("RectTransform whose rect defines the screen bounds for flipping/clamping. Defaults to the root canvas.")]
        [SerializeField] private RectTransform bounds;

        [Header("Layout")]
        [SerializeField] private float maxWidth = 320f;
        [SerializeField] private float padding = 16f;
        [Tooltip("Gap between the target and the bubble.")]
        [SerializeField] private float gap = 12f;
        [Tooltip("Keep the bubble at least this far from the canvas edges.")]
        [SerializeField] private float edgePadding = 16f;

        [Header("Animation")]
        [SerializeField] private UITweenSettings showTween = new UITweenSettings();
        [SerializeField] private UITweenSettings hideTween = new UITweenSettings();

        [Header("Events")]
        [SerializeField] private ShownEvent onShown = new ShownEvent();
        [SerializeField] private UnityEvent onHidden = new UnityEvent();

        private Tween fadeTween;

        public ShownEvent OnShown => onShown;
        public UnityEvent OnHidden => onHidden;

        private RectTransform Bounds => bounds != null ? bounds : (bounds = ResolveCanvasRect());

        private void Awake()
        {
            if (bubble == null)
            {
                bubble = transform as RectTransform;
            }

            if (canvasGroup == null && bubble != null)
            {
                canvasGroup = bubble.GetComponent<CanvasGroup>();
            }

            if (bounds == null)
            {
                bounds = ResolveCanvasRect();
            }
        }

        private void OnEnable()
        {
            HideInstant();
        }

        private void OnDisable()
        {
            KillFade();
        }

        public void Show(RectTransform target, string text, Placement placement = Placement.Top)
        {
            if (bubble == null || target == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            ApplyText(text);
            PositionFor(target, placement);

            // Always render on top — the bubble may have been created before other UI, so force it
            // to the front of its parent whenever it is shown.
            bubble.SetAsLastSibling();

            KillFade();
            bubble.localScale = Vector3.one;

            var duration = showTween != null ? Mathf.Max(0f, showTween.Duration) : 0f;
            if (canvasGroup != null)
            {
                if (duration <= Mathf.Epsilon)
                {
                    canvasGroup.alpha = 1f;
                }
                else
                {
                    fadeTween = UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, 1f, duration);
                    showTween.Apply(fadeTween);
                }

                canvasGroup.blocksRaycasts = false;
            }

            onShown?.Invoke(text);
        }

        public void Hide()
        {
            KillFade();

            var duration = hideTween != null ? Mathf.Max(0f, hideTween.Duration) : 0f;
            if (canvasGroup == null)
            {
                onHidden?.Invoke();
                return;
            }

            if (duration <= Mathf.Epsilon)
            {
                canvasGroup.alpha = 0f;
            }
            else
            {
                fadeTween = UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, 0f, duration);
                hideTween.Apply(fadeTween);
            }

            onHidden?.Invoke();
        }

        private void ApplyText(string text)
        {
            if (label == null)
            {
                return;
            }

            var inner = Mathf.Max(20f, maxWidth - padding * 2f);
            label.rectTransform.sizeDelta = new Vector2(inner, label.rectTransform.sizeDelta.y);
            label.text = text;
            label.ForceMeshUpdate();

            var textHeight = label.preferredHeight;
            var textWidth = Mathf.Min(inner, label.preferredWidth);

            label.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
            bubble.sizeDelta = new Vector2(textWidth + padding * 2f, textHeight + padding * 2f);
        }

        private void PositionFor(RectTransform target, Placement placement)
        {
            var area = Bounds;
            if (area == null)
            {
                return;
            }

            var b = RectTransformUtility.CalculateRelativeRectTransformBounds(area, target);
            var targetCenter = (Vector2)b.center;
            var targetExtents = (Vector2)b.extents;
            var half = bubble.rect.size * 0.5f;

            var resolved = ResolvePlacement(placement, targetCenter, targetExtents, half, area.rect);
            var pos = PositionForPlacement(resolved, targetCenter, targetExtents, half);

            // Clamp on the cross axis so the bubble never leaves the canvas.
            var canvasHalf = area.rect.size * 0.5f;
            var minX = -canvasHalf.x + edgePadding + half.x;
            var maxX = canvasHalf.x - edgePadding - half.x;
            var minY = -canvasHalf.y + edgePadding + half.y;
            var maxY = canvasHalf.y - edgePadding - half.y;
            if (minX <= maxX) pos.x = Mathf.Clamp(pos.x, minX, maxX);
            if (minY <= maxY) pos.y = Mathf.Clamp(pos.y, minY, maxY);

            bubble.anchorMin = new Vector2(0.5f, 0.5f);
            bubble.anchorMax = new Vector2(0.5f, 0.5f);
            bubble.pivot = new Vector2(0.5f, 0.5f);
            bubble.anchoredPosition = pos;
        }

        private Placement ResolvePlacement(Placement preferred, Vector2 targetCenter, Vector2 targetExtents, Vector2 half, Rect canvasRect)
        {
            var canvasHalf = canvasRect.size * 0.5f;

            bool FitsTop() => targetCenter.y + targetExtents.y + gap + half.y * 2f <= canvasHalf.y - edgePadding;
            bool FitsBottom() => targetCenter.y - targetExtents.y - gap - half.y * 2f >= -canvasHalf.y + edgePadding;
            bool FitsLeft() => targetCenter.x - targetExtents.x - gap - half.x * 2f >= -canvasHalf.x + edgePadding;
            bool FitsRight() => targetCenter.x + targetExtents.x + gap + half.x * 2f <= canvasHalf.x - edgePadding;

            switch (preferred)
            {
                case Placement.Top: return FitsTop() ? Placement.Top : (FitsBottom() ? Placement.Bottom : Placement.Top);
                case Placement.Bottom: return FitsBottom() ? Placement.Bottom : (FitsTop() ? Placement.Top : Placement.Bottom);
                case Placement.Left: return FitsLeft() ? Placement.Left : (FitsRight() ? Placement.Right : Placement.Left);
                default: return FitsRight() ? Placement.Right : (FitsLeft() ? Placement.Left : Placement.Right);
            }
        }

        private Vector2 PositionForPlacement(Placement placement, Vector2 targetCenter, Vector2 targetExtents, Vector2 half)
        {
            switch (placement)
            {
                case Placement.Top:
                    return new Vector2(targetCenter.x, targetCenter.y + targetExtents.y + gap + half.y);
                case Placement.Bottom:
                    return new Vector2(targetCenter.x, targetCenter.y - targetExtents.y - gap - half.y);
                case Placement.Left:
                    return new Vector2(targetCenter.x - targetExtents.x - gap - half.x, targetCenter.y);
                default:
                    return new Vector2(targetCenter.x + targetExtents.x + gap + half.x, targetCenter.y);
            }
        }

        private void HideInstant()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void KillFade()
        {
            if (fadeTween != null && fadeTween.IsActive())
            {
                fadeTween.Kill(false);
            }

            fadeTween = null;
        }

        private RectTransform ResolveCanvasRect()
        {
            var canvas = GetComponentInParent<Canvas>();
            return canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
        }
    }

    /// <summary>
    /// Attach to a hoverable element. On pointer enter (after a delay) it asks the shared
    /// <see cref="UITooltipControl"/> to show <see cref="text"/> next to this element; pointer exit
    /// hides it. Optional long-press shows it on touch.
    /// </summary>
    public sealed class UITooltipTrigger : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField] private UITooltipControl tooltip;
        [TextArea]
        [SerializeField] private string text = "Tooltip";
        [SerializeField] private UITooltipControl.Placement placement = UITooltipControl.Placement.Top;
        [Min(0f)]
        [SerializeField] private float hoverDelay = 0.35f;
        [Tooltip("Also show on touch long-press.")]
        [SerializeField] private bool longPress = true;
        [Min(0.1f)]
        [SerializeField] private float longPressTime = 0.5f;

        private Coroutine pending;

        public void SetTooltip(UITooltipControl control) => tooltip = control;
        public void SetText(string value) => text = value;

        public void OnPointerEnter(PointerEventData eventData)
        {
            ScheduleShow(hoverDelay);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CancelAndHide();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (longPress)
            {
                ScheduleShow(longPressTime);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (longPress)
            {
                CancelAndHide();
            }
        }

        private void OnDisable()
        {
            CancelAndHide();
        }

        private void ScheduleShow(float delay)
        {
            if (tooltip == null)
            {
                return;
            }

            if (pending != null)
            {
                StopCoroutine(pending);
            }

            pending = StartCoroutine(ShowAfter(delay));
        }

        private IEnumerator ShowAfter(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSecondsRealtime(delay);
            }

            pending = null;
            tooltip.Show(transform as RectTransform, text, placement);
        }

        private void CancelAndHide()
        {
            if (pending != null)
            {
                StopCoroutine(pending);
                pending = null;
            }

            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }
    }
}
