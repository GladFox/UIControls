using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A notification badge that overlays a count (e.g. on an icon). Hides at zero (unless
    /// <see cref="showZero"/>), clamps large counts to "N+", supports a dot-only mode, and pops its
    /// scale when the count changes.
    /// </summary>
    public sealed class UIBadgeControl : MonoBehaviour
    {
        [Serializable]
        public sealed class CountEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [Tooltip("The badge bubble RectTransform (shown/hidden and popped).")]
        [SerializeField] private RectTransform bubble;
        [SerializeField] private TMP_Text countLabel;

        [Header("Value")]
        [SerializeField] private int count;
        [SerializeField] private int maxDisplay = 99;
        [SerializeField] private bool showZero;
        [Tooltip("Show a small dot instead of the number.")]
        [SerializeField] private bool dotMode;

        [Header("Animation")]
        [SerializeField] private float popScale = 1.35f;
        [SerializeField] private float popDuration = 0.25f;

        [Header("Events")]
        [SerializeField] private CountEvent onCountChanged = new CountEvent();

        private Tween popTween;

        public CountEvent OnCountChanged => onCountChanged;
        public int Count => count;

        private void OnEnable()
        {
            Refresh(false);
        }

        private void OnDisable()
        {
            if (popTween != null && popTween.IsActive())
            {
                popTween.Kill();
            }

            popTween = null;
        }

        public void SetCount(int value, bool animate = true, bool notify = true)
        {
            var clamped = Mathf.Max(0, value);
            var changed = clamped != count;
            count = clamped;
            Refresh(animate && changed);

            if (notify && changed)
            {
                onCountChanged?.Invoke(count);
            }
        }

        public void Increment()
        {
            SetCount(count + 1);
        }

        public void Decrement()
        {
            SetCount(count - 1);
        }

        private void Refresh(bool pop)
        {
            var visible = dotMode ? (count > 0 || showZero) : (count > 0 || showZero);

            if (bubble != null && bubble.gameObject.activeSelf != visible)
            {
                bubble.gameObject.SetActive(visible);
            }

            if (countLabel != null)
            {
                if (dotMode)
                {
                    countLabel.text = string.Empty;
                }
                else
                {
                    countLabel.text = count > maxDisplay ? maxDisplay + "+" : count.ToString();
                }
            }

            if (pop && visible && bubble != null)
            {
                if (popTween != null && popTween.IsActive())
                {
                    popTween.Kill();
                }

                bubble.localScale = Vector3.one;
                popTween = DOTween.Sequence()
                    .Append(bubble.DOScale(Vector3.one * popScale, popDuration * 0.45f).SetEase(Ease.OutQuad))
                    .Append(bubble.DOScale(Vector3.one, popDuration * 0.55f).SetEase(Ease.OutBack))
                    .SetUpdate(true);
            }
        }
    }
}
