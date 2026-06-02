using System;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A numeric stepper: <c>[ - ] value [ + ]</c>. Tapping a button nudges the value by
    /// <see cref="step"/>; holding it starts an accelerating auto-repeat after a short delay.
    /// The value label pops on change and the arrows dim when a bound is reached
    /// (unless <see cref="wrapAround"/> is on).
    /// </summary>
    public sealed class UIStepperControl : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerExitHandler,
        IMoveHandler
    {
        [Serializable]
        public sealed class ValueChangedEvent : UnityEvent<float>
        {
        }

        [Header("Value")]
        [SerializeField] private float value;
        [SerializeField] private float min;
        [SerializeField] private float max = 10f;
        [SerializeField] private float step = 1f;
        [Tooltip("When the value passes a bound, wrap to the other end instead of clamping.")]
        [SerializeField] private bool wrapAround;
        [Tooltip("C#/.NET numeric format string for the label, e.g. \"0\", \"0.0\", \"0.##\".")]
        [SerializeField] private string valueFormat = "0";

        [Header("Targets")]
        [SerializeField] private RectTransform decrementButton;
        [SerializeField] private RectTransform incrementButton;
        [SerializeField] private Graphic decrementGraphic;
        [SerializeField] private Graphic incrementGraphic;
        [SerializeField] private TMP_Text valueLabel;

        [Header("Hold To Repeat")]
        [Min(0f)]
        [Tooltip("Delay after press before auto-repeat starts.")]
        [SerializeField] private float holdDelay = 0.4f;
        [Min(0.01f)]
        [Tooltip("Interval between the first few auto-repeats.")]
        [SerializeField] private float repeatInterval = 0.15f;
        [Range(0.5f, 1f)]
        [Tooltip("Each repeat multiplies the interval by this (acceleration). 1 = constant speed.")]
        [SerializeField] private float repeatAcceleration = 0.85f;
        [Min(0.01f)]
        [SerializeField] private float minRepeatInterval = 0.04f;

        [Header("Animation")]
        [SerializeField] private UITweenSettings labelTween = new UITweenSettings();
        [SerializeField] private bool popOnChange = true;
        [SerializeField] private float popScale = 1.18f;

        [Header("Arrow Colors")]
        [SerializeField] private Color arrowEnabledColor = new Color(0.92f, 0.95f, 1f, 1f);
        [SerializeField] private Color arrowDisabledColor = new Color(0.92f, 0.95f, 1f, 0.28f);

        [Header("Interaction")]
        [SerializeField] private bool interactable = true;
        [SerializeField] private CanvasGroup canvasGroup;
        [Range(0f, 1f)]
        [SerializeField] private float disabledAlpha = 0.55f;

        [Header("Events")]
        [SerializeField] private ValueChangedEvent onValueChanged = new ValueChangedEvent();

        private int heldDirection;
        private float heldTime;
        private bool repeating;
        private float repeatTimer;
        private float currentInterval;
        private Tween labelTweenInstance;

        public ValueChangedEvent OnValueChanged => onValueChanged;

        public float Value
        {
            get => value;
            set => SetValue(value);
        }

        public float Min
        {
            get => min;
            set { min = value; ClampValueToBounds(); RefreshVisual(true); }
        }

        public float Max
        {
            get => max;
            set { max = value; ClampValueToBounds(); RefreshVisual(true); }
        }

        public float Step
        {
            get => step;
            set => step = value;
        }

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

            if (max < min)
            {
                (min, max) = (max, min);
            }
        }

        private void OnEnable()
        {
            ApplyInteractableState();
            ClampValueToBounds();
            RefreshVisual(true);
        }

        private void OnDisable()
        {
            StopRepeat();
            KillLabelTween();
        }

        private void Update()
        {
            if (heldDirection == 0 || !interactable)
            {
                return;
            }

            heldTime += Time.unscaledDeltaTime;

            if (!repeating)
            {
                if (heldTime >= holdDelay)
                {
                    repeating = true;
                    repeatTimer = 0f;
                    currentInterval = repeatInterval;
                }

                return;
            }

            repeatTimer += Time.unscaledDeltaTime;
            if (repeatTimer >= currentInterval)
            {
                repeatTimer = 0f;
                if (!StepBy(heldDirection))
                {
                    // Hit a bound with wrap off — stop hammering.
                    StopRepeat();
                    return;
                }

                currentInterval = Mathf.Max(minRepeatInterval, currentInterval * repeatAcceleration);
            }
        }

        public void SetValue(float newValue, bool animate = true, bool notify = true)
        {
            var resolved = ResolveValue(newValue);
            if (Mathf.Approximately(resolved, value))
            {
                RefreshVisual(true);
                return;
            }

            value = resolved;
            RefreshVisual(!animate);

            if (notify)
            {
                onValueChanged?.Invoke(value);
            }
        }

        public void Increment()
        {
            StepBy(1);
        }

        public void Decrement()
        {
            StepBy(-1);
        }

        public void SetInteractable(bool isInteractable, bool instant = false)
        {
            if (interactable == isInteractable)
            {
                return;
            }

            interactable = isInteractable;
            if (!interactable)
            {
                StopRepeat();
            }

            ApplyInteractableState();

            if (instant)
            {
                RefreshVisual(true);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable || eventData == null || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            var dir = ResolveDirectionAtScreenPoint(eventData.position, eventData.pressEventCamera);
            if (dir == 0)
            {
                return;
            }

            // Immediate first step, then arm the hold-to-repeat timer.
            StepBy(dir);
            heldDirection = dir;
            heldTime = 0f;
            repeating = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopRepeat();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopRepeat();
        }

        public void OnMove(AxisEventData eventData)
        {
            if (!interactable || eventData == null)
            {
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                case MoveDirection.Up:
                    StepBy(1);
                    break;
                case MoveDirection.Left:
                case MoveDirection.Down:
                    StepBy(-1);
                    break;
            }
        }

        private int ResolveDirectionAtScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            if (incrementButton != null &&
                RectTransformUtility.RectangleContainsScreenPoint(incrementButton, screenPoint, eventCamera))
            {
                return 1;
            }

            if (decrementButton != null &&
                RectTransformUtility.RectangleContainsScreenPoint(decrementButton, screenPoint, eventCamera))
            {
                return -1;
            }

            return 0;
        }

        private bool StepBy(int direction)
        {
            if (direction == 0)
            {
                return false;
            }

            var candidate = value + direction * step;
            var resolved = ResolveValue(candidate);
            if (Mathf.Approximately(resolved, value))
            {
                return false;
            }

            value = resolved;
            RefreshVisual(false);
            onValueChanged?.Invoke(value);
            return true;
        }

        private float ResolveValue(float candidate)
        {
            if (!wrapAround)
            {
                return Mathf.Clamp(candidate, min, max);
            }

            var range = max - min;
            if (range <= Mathf.Epsilon)
            {
                return min;
            }

            if (candidate > max)
            {
                return min;
            }

            if (candidate < min)
            {
                return max;
            }

            return candidate;
        }

        private void ClampValueToBounds()
        {
            if (!wrapAround)
            {
                value = Mathf.Clamp(value, min, max);
            }
        }

        private void RefreshVisual(bool instant)
        {
            if (valueLabel != null)
            {
                valueLabel.text = value.ToString(valueFormat, CultureInfo.InvariantCulture);
            }

            RefreshArrowColors();

            if (instant || valueLabel == null || !popOnChange)
            {
                if (valueLabel != null)
                {
                    valueLabel.rectTransform.localScale = Vector3.one;
                }

                return;
            }

            var duration = labelTween != null ? Mathf.Max(0f, labelTween.Duration) : 0f;
            if (duration <= Mathf.Epsilon)
            {
                return;
            }

            KillLabelTween();

            var rect = valueLabel.rectTransform;
            var sequence = DOTween.Sequence();
            sequence.Append(rect.DOScale(Vector3.one * popScale, duration * 0.4f).SetEase(Ease.OutQuad));
            sequence.Append(rect.DOScale(Vector3.one, duration * 0.6f).SetEase(Ease.OutBack));
            labelTween.ApplyTimingOnly(sequence);
            labelTweenInstance = sequence;
        }

        private void RefreshArrowColors()
        {
            var canDecrement = wrapAround || value > min + Mathf.Epsilon;
            var canIncrement = wrapAround || value < max - Mathf.Epsilon;

            if (decrementGraphic != null)
            {
                decrementGraphic.color = canDecrement ? arrowEnabledColor : arrowDisabledColor;
            }

            if (incrementGraphic != null)
            {
                incrementGraphic.color = canIncrement ? arrowEnabledColor : arrowDisabledColor;
            }
        }

        private void StopRepeat()
        {
            heldDirection = 0;
            heldTime = 0f;
            repeating = false;
            repeatTimer = 0f;
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

        private void KillLabelTween()
        {
            if (labelTweenInstance != null && labelTweenInstance.IsActive())
            {
                labelTweenInstance.Kill(false);
            }

            labelTweenInstance = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (max < min)
            {
                max = min;
            }

            UnityEditor.EditorApplication.delayCall += SyncInEditor;
        }

        private void SyncInEditor()
        {
            UnityEditor.EditorApplication.delayCall -= SyncInEditor;
            if (this == null || Application.isPlaying)
            {
                return;
            }

            if (!wrapAround)
            {
                value = Mathf.Clamp(value, min, max);
            }

            if (valueLabel != null)
            {
                valueLabel.text = value.ToString(valueFormat, CultureInfo.InvariantCulture);
                valueLabel.rectTransform.localScale = Vector3.one;
            }

            RefreshArrowColors();
        }
#endif
    }
}
