using System;
using DG.Tweening;
using TMPro;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Single-value slider with a value bubble that follows the handle and appears while dragging.
    /// Drag anywhere on the track to set the value (with optional stepping); the fill and handle track
    /// it and the bubble shows the formatted number. Raises <see cref="OnValueChanged"/>. Distinct from
    /// the dual-handle range slider.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIValueSliderControl : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Serializable]
        public sealed class FloatEvent : UnityEvent<float>
        {
        }

        [Header("Targets")]
        [SerializeField] private RectTransform track;
        [SerializeField] private RectTransform fill;
        [SerializeField] private RectTransform handle;
        [SerializeField] private CanvasGroup bubbleGroup;
        [SerializeField] private RectTransform bubble;
        [SerializeField] private TMP_Text bubbleLabel;

        [Header("Range")]
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 100f;
        [Tooltip("Snap step (0 = continuous).")]
        [SerializeField] private float step = 1f;
        [SerializeField] private float value = 50f;
        [SerializeField] private string format = "0";

        [Header("Events")]
        [SerializeField] private FloatEvent onValueChanged = new FloatEvent();

        public FloatEvent OnValueChanged => onValueChanged;

        public float Value
        {
            get => value;
            set => SetValue(value);
        }

        private void OnEnable()
        {
            ApplyVisual();
            if (bubbleGroup != null) bubbleGroup.alpha = 0f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ShowBubble(true);
            UpdateFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ShowBubble(false);
        }

        public void SetValue(float newValue, bool notify = true)
        {
            var clamped = Mathf.Clamp(newValue, minValue, maxValue);
            if (step > 0f)
            {
                clamped = minValue + Mathf.Round((clamped - minValue) / step) * step;
                clamped = Mathf.Clamp(clamped, minValue, maxValue);
            }

            value = clamped;
            ApplyVisual();
            if (notify)
            {
                onValueChanged.Invoke(value);
            }
        }

        private void UpdateFromPointer(PointerEventData eventData)
        {
            if (track == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    track, eventData.position, eventData.pressEventCamera, out var local))
            {
                return;
            }

            var width = track.rect.width;
            var t = width <= 0f ? 0f : Mathf.Clamp01((local.x - track.rect.xMin) / width);
            SetValue(Mathf.Lerp(minValue, maxValue, t));
        }

        private void ApplyVisual()
        {
            var range = Mathf.Approximately(maxValue, minValue) ? 1f : maxValue - minValue;
            var t = Mathf.Clamp01((value - minValue) / range);

            if (track != null)
            {
                var width = track.rect.width;
                var x = track.rect.xMin + t * width;

                if (handle != null)
                {
                    handle.anchoredPosition = new Vector2(x, handle.anchoredPosition.y);
                }

                if (fill != null)
                {
                    fill.anchoredPosition = new Vector2(track.rect.xMin, fill.anchoredPosition.y);
                    fill.sizeDelta = new Vector2(t * width, fill.sizeDelta.y);
                }

                if (bubble != null)
                {
                    bubble.anchoredPosition = new Vector2(x, bubble.anchoredPosition.y);
                }
            }

            if (bubbleLabel != null)
            {
                bubbleLabel.text = value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void ShowBubble(bool show)
        {
            if (bubbleGroup == null)
            {
                return;
            }

            UIDOTweenUtility.TweenCanvasGroupAlpha(bubbleGroup, show ? 1f : 0f, 0.15f).SetUpdate(true);
        }
    }
}
