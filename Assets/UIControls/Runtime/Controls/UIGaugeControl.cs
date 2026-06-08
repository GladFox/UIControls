using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Arc gauge with a sweeping needle (speedometer, boss health, signal strength). <see cref="SetValue"/>
    /// maps a value within [min,max] onto a needle angle across the arc and tweens to it; an optional
    /// radial-fill arc and a numeric label track the value too.
    /// </summary>
    public sealed class UIGaugeControl : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private RectTransform needle;
        [SerializeField] private Image arcFill;
        [SerializeField] private TMP_Text valueLabel;

        [Header("Range")]
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float value;
        [Tooltip("Needle angle (deg, signed from up) at min value.")]
        [SerializeField] private float minAngle = 120f;
        [Tooltip("Needle angle (deg, signed from up) at max value.")]
        [SerializeField] private float maxAngle = -120f;

        [Header("Format")]
        [SerializeField] private string suffix = "";
        [SerializeField] private string format = "0";

        [Header("Animation")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private Tween needleTween;

        public float Value => value;

        private void OnEnable()
        {
            ApplyInstant(value);
        }

        private void OnDisable()
        {
            needleTween?.Kill();
            needleTween = null;
        }

        public void SetValue(float target, bool animate = true)
        {
            value = Mathf.Clamp(target, minValue, maxValue);
            var t = Mathf.InverseLerp(minValue, maxValue, value);

            needleTween?.Kill();

            if (animate && duration > 0f)
            {
                var fromAngle = needle != null ? needle.localEulerAngles.z : 0f;
                var toAngle = Mathf.Lerp(minAngle, maxAngle, t);
                needleTween = DOTween.To(() => fromAngle, a =>
                {
                    if (needle != null) needle.localEulerAngles = new Vector3(0f, 0f, a);
                }, toAngle, duration).SetEase(ease).SetUpdate(true);

                if (arcFill != null)
                {
                    DOTween.To(() => arcFill.fillAmount, v => arcFill.fillAmount = v, t, duration).SetEase(ease).SetUpdate(true);
                }

                RenderLabel();
            }
            else
            {
                ApplyInstant(value);
            }
        }

        private void ApplyInstant(float v)
        {
            var t = Mathf.InverseLerp(minValue, maxValue, v);
            if (needle != null) needle.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(minAngle, maxAngle, t));
            if (arcFill != null) arcFill.fillAmount = t;
            RenderLabel();
        }

        private void RenderLabel()
        {
            if (valueLabel != null)
            {
                valueLabel.text = value.ToString(format, System.Globalization.CultureInfo.InvariantCulture) + suffix;
            }
        }
    }
}
