using System;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A dual-thumb range slider: two handles (<see cref="lowValue"/> / <see cref="highValue"/>)
    /// over a shared track, with a fill drawn between them. Drag a handle to move it, or click the
    /// track to jump the nearer handle. The handles can't cross and keep at least
    /// <see cref="minDistance"/> apart. uGUI ships only a single-value <c>Slider</c>, so this fills
    /// the common "min/max filter" gap.
    /// </summary>
    public sealed class UIRangeSliderControl : MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IMoveHandler
    {
        [Serializable]
        public sealed class RangeChangedEvent : UnityEvent<float, float>
        {
        }

        [Header("Range")]
        [SerializeField] private float minLimit;
        [SerializeField] private float maxLimit = 100f;
        [SerializeField] private float lowValue = 20f;
        [SerializeField] private float highValue = 80f;
        [Tooltip("Smallest allowed gap between the two handles, in value units.")]
        [SerializeField] private float minDistance;
        [SerializeField] private bool wholeNumbers;

        [Header("Targets")]
        [Tooltip("The track RectTransform defining the draggable area. Handles and fill should be its children.")]
        [SerializeField] private RectTransform track;
        [SerializeField] private RectTransform lowHandle;
        [SerializeField] private RectTransform highHandle;
        [Tooltip("RectTransform stretched between the two handles. Anchored-centered; width is driven.")]
        [SerializeField] private RectTransform fill;

        [Header("Animation")]
        [Tooltip("Used for track-click jumps and programmatic SetValue. Dragging always tracks the pointer instantly.")]
        [SerializeField] private UITweenSettings moveTween = new UITweenSettings();

        [Header("Interaction")]
        [SerializeField] private bool interactable = true;
        [SerializeField] private CanvasGroup canvasGroup;
        [Range(0f, 1f)]
        [SerializeField] private float disabledAlpha = 0.55f;

        [Header("Events")]
        [SerializeField] private RangeChangedEvent onRangeChanged = new RangeChangedEvent();

        private bool draggingHigh;
        private int activeHandle = -1; // 0 = low, 1 = high (last grabbed, for keyboard)
        private Tween lowTween;
        private Tween highTween;
        private Tween fillTween;

        public RangeChangedEvent OnRangeChanged => onRangeChanged;
        public float LowValue => lowValue;
        public float HighValue => highValue;
        public float MinLimit => minLimit;
        public float MaxLimit => maxLimit;

        public bool Interactable
        {
            get => interactable;
            set => SetInteractable(value);
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            NormalizeLimits();
        }

        private void OnEnable()
        {
            ApplyInteractableState();
            ClampValues();
            RefreshPositions(true);
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public void SetRange(float low, float high, bool animate = true, bool notify = true)
        {
            lowValue = low;
            highValue = high;
            ClampValues();
            RefreshPositions(!animate);

            if (notify)
            {
                onRangeChanged?.Invoke(lowValue, highValue);
            }
        }

        public void SetLowValue(float low, bool animate = true, bool notify = true)
        {
            SetRange(low, highValue, animate, notify);
        }

        public void SetHighValue(float high, bool animate = true, bool notify = true)
        {
            SetRange(lowValue, high, animate, notify);
        }

        public void SetInteractable(bool value, bool instant = false)
        {
            if (interactable == value)
            {
                return;
            }

            interactable = value;
            ApplyInteractableState();

            if (instant)
            {
                RefreshPositions(true);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable || track == null || eventData == null ||
                eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!TryGetPointerValue(eventData, out var pointerValue))
            {
                return;
            }

            // Pick the handle nearer to the click; ties go to whichever side the click falls on.
            var distToLow = Mathf.Abs(pointerValue - lowValue);
            var distToHigh = Mathf.Abs(pointerValue - highValue);
            draggingHigh = distToHigh < distToLow || (Mathf.Approximately(distToHigh, distToLow) && pointerValue > lowValue);
            activeHandle = draggingHigh ? 1 : 0;

            ApplyHandleValue(draggingHigh, pointerValue, animate: true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!interactable || track == null || eventData == null)
            {
                return;
            }

            if (!TryGetPointerValue(eventData, out var pointerValue))
            {
                return;
            }

            ApplyHandleValue(draggingHigh, pointerValue, animate: false);
        }

        public void OnMove(AxisEventData eventData)
        {
            if (!interactable || eventData == null || activeHandle < 0)
            {
                return;
            }

            var stepUnit = wholeNumbers ? 1f : (maxLimit - minLimit) * 0.02f;
            var delta = 0f;
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                case MoveDirection.Up:
                    delta = stepUnit;
                    break;
                case MoveDirection.Left:
                case MoveDirection.Down:
                    delta = -stepUnit;
                    break;
            }

            if (Mathf.Approximately(delta, 0f))
            {
                return;
            }

            var isHigh = activeHandle == 1;
            var current = isHigh ? highValue : lowValue;
            ApplyHandleValue(isHigh, current + delta, animate: true);
        }

        private void ApplyHandleValue(bool isHigh, float rawValue, bool animate)
        {
            var resolved = ResolveValue(rawValue);

            if (isHigh)
            {
                resolved = Mathf.Max(resolved, lowValue + minDistance);
                resolved = Mathf.Min(resolved, maxLimit);
                if (Mathf.Approximately(resolved, highValue))
                {
                    return;
                }

                highValue = resolved;
            }
            else
            {
                resolved = Mathf.Min(resolved, highValue - minDistance);
                resolved = Mathf.Max(resolved, minLimit);
                if (Mathf.Approximately(resolved, lowValue))
                {
                    return;
                }

                lowValue = resolved;
            }

            RefreshPositions(!animate);
            onRangeChanged?.Invoke(lowValue, highValue);
        }

        private bool TryGetPointerValue(PointerEventData eventData, out float value)
        {
            value = 0f;
            if (track == null)
            {
                return false;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    track, eventData.position, eventData.pressEventCamera, out var local))
            {
                return false;
            }

            var width = track.rect.width;
            if (width <= Mathf.Epsilon)
            {
                return false;
            }

            // Track is anchored/pivoted at center → local.x in [-w/2, w/2].
            var normalized = Mathf.Clamp01((local.x + width * 0.5f) / width);
            value = Mathf.Lerp(minLimit, maxLimit, normalized);
            return true;
        }

        private float ResolveValue(float candidate)
        {
            var clamped = Mathf.Clamp(candidate, minLimit, maxLimit);
            return wholeNumbers ? Mathf.Round(clamped) : clamped;
        }

        private void ClampValues()
        {
            if (wholeNumbers)
            {
                lowValue = Mathf.Round(lowValue);
                highValue = Mathf.Round(highValue);
            }

            lowValue = Mathf.Clamp(lowValue, minLimit, maxLimit);
            highValue = Mathf.Clamp(highValue, minLimit, maxLimit);

            if (highValue < lowValue + minDistance)
            {
                highValue = Mathf.Min(maxLimit, lowValue + minDistance);
                lowValue = Mathf.Min(lowValue, highValue - minDistance);
                lowValue = Mathf.Max(minLimit, lowValue);
            }
        }

        private void NormalizeLimits()
        {
            if (maxLimit < minLimit)
            {
                (minLimit, maxLimit) = (maxLimit, minLimit);
            }

            minDistance = Mathf.Max(0f, Mathf.Min(minDistance, maxLimit - minLimit));
        }

        private float ValueToLocalX(float value)
        {
            if (track == null)
            {
                return 0f;
            }

            var range = maxLimit - minLimit;
            var normalized = range > Mathf.Epsilon ? (value - minLimit) / range : 0f;
            var width = track.rect.width;
            return (normalized - 0.5f) * width;
        }

        private void RefreshPositions(bool instant)
        {
            if (track == null)
            {
                return;
            }

            KillTweens();

            var lowX = ValueToLocalX(lowValue);
            var highX = ValueToLocalX(highValue);
            var centerX = (lowX + highX) * 0.5f;
            var fillWidth = Mathf.Max(0f, highX - lowX);

            var duration = moveTween != null ? Mathf.Max(0f, moveTween.Duration) : 0f;

            if (instant || duration <= Mathf.Epsilon)
            {
                SetHandleX(lowHandle, lowX);
                SetHandleX(highHandle, highX);
                SetFill(centerX, fillWidth);
                return;
            }

            if (lowHandle != null)
            {
                lowTween = UIDOTweenUtility.TweenAnchoredPosition(
                    lowHandle, new Vector2(lowX, lowHandle.anchoredPosition.y), duration);
                moveTween.Apply(lowTween);
            }

            if (highHandle != null)
            {
                highTween = UIDOTweenUtility.TweenAnchoredPosition(
                    highHandle, new Vector2(highX, highHandle.anchoredPosition.y), duration);
                moveTween.Apply(highTween);
            }

            if (fill != null)
            {
                fillTween = DOTween.To(
                    () => new Vector2(fill.anchoredPosition.x, fill.sizeDelta.x),
                    v =>
                    {
                        var pos = fill.anchoredPosition;
                        pos.x = v.x;
                        fill.anchoredPosition = pos;
                        var size = fill.sizeDelta;
                        size.x = v.y;
                        fill.sizeDelta = size;
                    },
                    new Vector2(centerX, fillWidth),
                    duration);
                moveTween.Apply(fillTween);
            }
        }

        private static void SetHandleX(RectTransform handle, float x)
        {
            if (handle == null)
            {
                return;
            }

            var pos = handle.anchoredPosition;
            pos.x = x;
            handle.anchoredPosition = pos;
        }

        private void SetFill(float centerX, float width)
        {
            if (fill == null)
            {
                return;
            }

            var pos = fill.anchoredPosition;
            pos.x = centerX;
            fill.anchoredPosition = pos;

            var size = fill.sizeDelta;
            size.x = width;
            fill.sizeDelta = size;
        }

        private void ApplyInteractableState()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = interactable ? 1f : disabledAlpha;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        private void KillTweens()
        {
            foreach (var t in new[] { lowTween, highTween, fillTween })
            {
                if (t != null && t.IsActive())
                {
                    t.Kill(false);
                }
            }

            lowTween = null;
            highTween = null;
            fillTween = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            NormalizeLimits();
            UnityEditor.EditorApplication.delayCall += SyncInEditor;
        }

        private void SyncInEditor()
        {
            UnityEditor.EditorApplication.delayCall -= SyncInEditor;
            if (this == null || Application.isPlaying)
            {
                return;
            }

            ClampValues();
            RefreshPositions(true);
        }
#endif
    }
}
