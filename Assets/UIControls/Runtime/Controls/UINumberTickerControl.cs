using System;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Animated rolling number for scores, currency and counters. <see cref="SetValue"/> tweens the
    /// displayed value from its current amount to the target with easing, formatting with an optional
    /// thousands separator, prefix/suffix and fixed decimals. Pair with a pop-scale for "juice".
    /// </summary>
    public sealed class UINumberTickerControl : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private TMP_Text label;

        [Header("Value")]
        [SerializeField] private double value;
        [SerializeField] private double startValue;

        [Header("Format")]
        [SerializeField] private string prefix = string.Empty;
        [SerializeField] private string suffix = string.Empty;
        [Min(0)]
        [SerializeField] private int decimals;
        [SerializeField] private bool thousandsSeparator = true;

        [Header("Animation")]
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private Ease ease = Ease.OutCubic;
        [Tooltip("Pop the label scale when the value changes.")]
        [SerializeField] private bool popOnChange = true;
        [SerializeField] private float popScale = 1.15f;

        private Tween valueTween;
        private Tween popTween;
        private double displayed;

        public double Value => value;

        private void OnEnable()
        {
            displayed = startValue;
            value = startValue;
            Render(displayed);
        }

        private void OnDisable()
        {
            valueTween?.Kill();
            popTween?.Kill();
            valueTween = null;
            popTween = null;
        }

        /// <summary>Tween the displayed number to <paramref name="target"/>.</summary>
        public void SetValue(double target, bool animate = true)
        {
            value = target;
            valueTween?.Kill();

            if (!animate || duration <= 0f)
            {
                displayed = target;
                Render(displayed);
                Pop();
                return;
            }

            var from = displayed;
            valueTween = DOTween.To(() => from, v =>
                {
                    displayed = v;
                    Render(v);
                }, (float)target, duration)
                .SetEase(ease)
                .SetUpdate(true);

            Pop();
        }

        public void Add(double delta, bool animate = true)
        {
            SetValue(value + delta, animate);
        }

        private void Pop()
        {
            if (!popOnChange || label == null)
            {
                return;
            }

            popTween?.Kill();
            label.rectTransform.localScale = Vector3.one;
            popTween = DOTween.Sequence()
                .Append(label.rectTransform.DOScale(Vector3.one * popScale, duration * 0.2f).SetEase(Ease.OutQuad))
                .Append(label.rectTransform.DOScale(Vector3.one, duration * 0.3f).SetEase(Ease.OutBack))
                .SetUpdate(true);
        }

        private void Render(double v)
        {
            if (label == null)
            {
                return;
            }

            var rounded = Math.Round(v, decimals, MidpointRounding.AwayFromZero);
            var format = (thousandsSeparator ? "#,0" : "0");
            if (decimals > 0)
            {
                format += "." + new string('0', decimals);
            }

            label.text = prefix + rounded.ToString(format, CultureInfo.InvariantCulture) + suffix;
        }
    }
}
