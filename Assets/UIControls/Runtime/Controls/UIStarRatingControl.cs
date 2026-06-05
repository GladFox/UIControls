using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A star rating: a row of stars the user clicks (or drags across) to set a value. Supports
    /// half-star precision, a hover preview, and a read-only mode for displaying ratings. Each star
    /// is a pair of glyphs — a dim "empty" background and a bright "filled" overlay whose fill amount
    /// is driven by clipping the overlay width.
    /// </summary>
    public sealed class UIStarRatingControl : MonoBehaviour,
        IPointerMoveHandler,
        IPointerClickHandler,
        IPointerExitHandler
    {
        [Serializable]
        public sealed class RatingEvent : UnityEvent<float>
        {
        }

        [Header("Stars")]
        [Tooltip("Per-star fill viewports (left-anchored, with a RectMask2D). Their width is set to fraction × star width to clip the bright star — so half-stars show the left half at full size.")]
        [SerializeField] private RectTransform[] starFills = Array.Empty<RectTransform>();
        [Tooltip("Per-star root rects, used to read star width and to hit-test the pointer.")]
        [SerializeField] private RectTransform[] starRoots = Array.Empty<RectTransform>();

        [Header("Value")]
        [SerializeField] private float value;
        [SerializeField] private bool allowHalf = true;
        [SerializeField] private bool readOnly;

        [Header("Events")]
        [SerializeField] private RatingEvent onRatingChanged = new RatingEvent();

        public RatingEvent OnRatingChanged => onRatingChanged;
        public int StarCount => starFills != null ? starFills.Length : 0;

        public float Value
        {
            get => value;
            set => SetValue(value);
        }

        public bool ReadOnly
        {
            get => readOnly;
            set => readOnly = value;
        }

        private void OnEnable()
        {
            ApplyFill(value);
        }

        public void SetValue(float rating, bool notify = true)
        {
            var clamped = Clamp(rating);
            if (Mathf.Approximately(clamped, value))
            {
                ApplyFill(clamped);
                return;
            }

            value = clamped;
            ApplyFill(value);

            if (notify)
            {
                onRatingChanged?.Invoke(value);
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (readOnly || eventData == null)
            {
                return;
            }

            ApplyFill(RatingAt(eventData)); // hover preview, doesn't commit
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!readOnly)
            {
                ApplyFill(value); // restore committed value
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (readOnly || eventData == null || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            SetValue(RatingAt(eventData));
        }

        private float RatingAt(PointerEventData eventData)
        {
            // Use each star's rect to figure out how many stars (and the fraction of the hovered one)
            // are to the left of the pointer.
            for (var i = 0; i < StarCount; i++)
            {
                var star = starRoots != null && i < starRoots.Length ? starRoots[i] : null;
                if (star == null)
                {
                    continue;
                }

                if (RectTransformUtility.RectangleContainsScreenPoint(star, eventData.position, eventData.pressEventCamera))
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            star, eventData.position, eventData.pressEventCamera, out var local))
                    {
                        var width = star.rect.width;
                        var frac = width > Mathf.Epsilon ? Mathf.Clamp01((local.x - star.rect.xMin) / width) : 1f;
                        var starValue = allowHalf ? (frac <= 0.5f ? 0.5f : 1f) : 1f;
                        return i + starValue;
                    }
                }
            }

            // Pointer past the last star → full; before the first → keep current logic via clamp.
            return Clamp(value);
        }

        private void ApplyFill(float rating)
        {
            for (var i = 0; i < StarCount; i++)
            {
                var fill = starFills[i];
                if (fill == null)
                {
                    continue;
                }

                var amount = Mathf.Clamp01(rating - i);
                var root = starRoots != null && i < starRoots.Length ? starRoots[i] : null;
                var width = root != null ? root.rect.width : fill.rect.width;

                var size = fill.sizeDelta;
                size.x = amount * width;
                fill.sizeDelta = size;
            }
        }

        private float Clamp(float rating)
        {
            rating = Mathf.Clamp(rating, 0f, StarCount);
            if (allowHalf)
            {
                return Mathf.Round(rating * 2f) / 2f;
            }

            return Mathf.Round(rating);
        }
    }
}
