using System;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    public sealed class UIToggleControl : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        [Serializable]
        public sealed class ToggleValueChangedEvent : UnityEvent<bool>
        {
        }

        [SerializeField] private bool isOn;
        [SerializeField] private bool interactable = true;

        [Header("Targets")]
        [SerializeField] private RectTransform handle;
        [SerializeField] private Graphic backgroundGraphic;
        [SerializeField] private Graphic handleGraphic;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Layout")]
        [SerializeField] private Vector2 offHandlePosition = new Vector2(-24f, 0f);
        [SerializeField] private Vector2 onHandlePosition = new Vector2(24f, 0f);

        [Header("Colors")]
        [SerializeField] private Color offBackgroundColor = new Color(0.27f, 0.27f, 0.3f, 1f);
        [SerializeField] private Color onBackgroundColor = new Color(0.17f, 0.6f, 0.35f, 1f);
        [SerializeField] private Color offHandleColor = Color.white;
        [SerializeField] private Color onHandleColor = Color.white;

        [Header("Animation")]
        [SerializeField] private UITweenSettings toggleTween = new UITweenSettings();
        [SerializeField] private float disabledAlpha = 0.55f;

        [Header("Events")]
        [SerializeField] private ToggleValueChangedEvent onValueChanged = new ToggleValueChangedEvent();

        private Sequence activeSequence;

        public bool IsOn
        {
            get => isOn;
            set => SetIsOn(value);
        }

        public bool Interactable
        {
            get => interactable;
            set => SetInteractable(value);
        }

        public ToggleValueChangedEvent OnValueChanged => onValueChanged;

        private void Awake()
        {
            if (backgroundGraphic == null)
            {
                backgroundGraphic = GetComponent<Graphic>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (handleGraphic == null && handle != null)
            {
                handleGraphic = handle.GetComponent<Graphic>();
            }

            if (handle == null)
            {
                var child = transform.Find("Handle");
                if (child != null)
                {
                    handle = child as RectTransform;
                    if (handleGraphic == null)
                    {
                        handleGraphic = handle.GetComponent<Graphic>();
                    }
                }
            }
        }

        private void OnEnable()
        {
            ApplyInteractableState();
            ApplyVisual(true);
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public void Toggle()
        {
            SetIsOn(!isOn);
        }

        public void SetIsOn(bool value, bool animate = true, bool notify = true)
        {
            if (isOn == value)
            {
                if (!animate)
                {
                    ApplyVisual(true);
                }

                return;
            }

            isOn = value;
            ApplyVisual(!animate);

            if (notify)
            {
                onValueChanged?.Invoke(isOn);
            }
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
                ApplyVisual(true);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            Toggle();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            Toggle();
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

        private void ApplyVisual(bool instant)
        {
            KillTweens();

            var targetPosition = isOn ? onHandlePosition : offHandlePosition;
            var targetBackgroundColor = isOn ? onBackgroundColor : offBackgroundColor;
            var targetHandleColor = isOn ? onHandleColor : offHandleColor;
            var duration = toggleTween != null ? Mathf.Max(0f, toggleTween.Duration) : 0f;

            if (instant || duration <= Mathf.Epsilon)
            {
                if (handle != null)
                {
                    handle.anchoredPosition = targetPosition;
                }

                if (backgroundGraphic != null)
                {
                    backgroundGraphic.color = targetBackgroundColor;
                }

                if (handleGraphic != null)
                {
                    handleGraphic.color = targetHandleColor;
                }

                return;
            }

            var sequence = DOTween.Sequence();
            var hasTweens = false;

            if (handle != null)
            {
                sequence.Join(handle.DOAnchorPos(targetPosition, duration));
                hasTweens = true;
            }

            if (backgroundGraphic != null)
            {
                sequence.Join(backgroundGraphic.DOColor(targetBackgroundColor, duration));
                hasTweens = true;
            }

            if (handleGraphic != null)
            {
                sequence.Join(handleGraphic.DOColor(targetHandleColor, duration));
                hasTweens = true;
            }

            if (!hasTweens)
            {
                sequence.Kill();
                return;
            }

            toggleTween?.Apply(sequence);
            activeSequence = sequence;
        }

        private void KillTweens()
        {
            if (activeSequence != null && activeSequence.IsActive())
            {
                activeSequence.Kill();
            }

            activeSequence = null;
        }
    }
}
