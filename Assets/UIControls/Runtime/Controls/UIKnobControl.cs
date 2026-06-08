using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Rotary knob / dial: drag around the centre to set a 0–1 <see cref="Value"/> mapped onto an
    /// angular sweep (default −135°…+135°, with the dead gap at the bottom). The knob visual rotates to
    /// match and an optional fill <see cref="UnityEngine.UI.Image"/> tracks the value. Great for volume,
    /// sensitivity and tuning dials.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIKnobControl : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        [Serializable]
        public sealed class FloatEvent : UnityEvent<float>
        {
        }

        [Header("Targets")]
        [Tooltip("Visual that rotates with the value (e.g. the knob cap with an indicator notch).")]
        [SerializeField] private RectTransform knobVisual;
        [Tooltip("Optional radial-fill Image driven by the value (fillAmount).")]
        [SerializeField] private UnityEngine.UI.Image fill;

        [Header("Range")]
        [Tooltip("Angle (deg, signed from straight up) at value 0.")]
        [SerializeField] private float minAngle = -135f;
        [Tooltip("Angle (deg, signed from straight up) at value 1.")]
        [SerializeField] private float maxAngle = 135f;
        [Range(0f, 1f)]
        [SerializeField] private float value = 0.5f;
        [Tooltip("Snap to this many discrete steps (0 = continuous).")]
        [SerializeField] private int steps;

        [Header("Events")]
        [SerializeField] private FloatEvent onValueChanged = new FloatEvent();

        private RectTransform self;

        public FloatEvent OnValueChanged => onValueChanged;

        public float Value
        {
            get => value;
            set => SetValue(value);
        }

        private void Awake()
        {
            self = transform as RectTransform;
        }

        private void OnEnable()
        {
            ApplyVisual();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
        }

        public void SetValue(float newValue, bool notify = true)
        {
            var clamped = Mathf.Clamp01(newValue);
            if (steps > 1)
            {
                clamped = Mathf.Round(clamped * (steps - 1)) / (steps - 1);
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
            if (self == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    self, eventData.position, eventData.pressEventCamera, out var local))
            {
                return;
            }

            // Angle clockwise from straight up, in degrees (−180..180).
            var angle = Mathf.Atan2(local.x, local.y) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            SetValue(Mathf.InverseLerp(minAngle, maxAngle, angle));
        }

        private void ApplyVisual()
        {
            var angle = Mathf.Lerp(minAngle, maxAngle, value);
            if (knobVisual != null)
            {
                knobVisual.localEulerAngles = new Vector3(0f, 0f, -angle);
            }

            if (fill != null)
            {
                fill.fillAmount = value;
            }
        }

        private void OnValidate()
        {
            ApplyVisual();
        }
    }
}
