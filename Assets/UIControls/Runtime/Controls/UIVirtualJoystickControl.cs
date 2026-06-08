using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// On-screen analog joystick for touch input. The handle is clamped to a radius around the
    /// background; <see cref="Direction"/> is the normalized stick vector and <see cref="Magnitude"/>
    /// its 0–1 length (after dead zone). In <see cref="floating"/> mode the background jumps to the
    /// touch point on press. Put this on a full touch area (raycast target) with background + handle
    /// children.
    /// </summary>
    public sealed class UIVirtualJoystickControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Serializable]
        public sealed class Vector2Event : UnityEvent<Vector2>
        {
        }

        [Header("Targets")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;

        [Header("Behaviour")]
        [Tooltip("Max handle travel from the centre, in background-local pixels.")]
        [SerializeField] private float handleRange = 75f;
        [Tooltip("Inputs below this fraction are treated as zero.")]
        [Range(0f, 0.9f)]
        [SerializeField] private float deadZone = 0.1f;
        [Tooltip("Background jumps to the press point and hides until pressed.")]
        [SerializeField] private bool floating;

        [Header("Events")]
        [SerializeField] private Vector2Event onValueChanged = new Vector2Event();

        private RectTransform self;
        private Vector2 input;

        public Vector2Event OnValueChanged => onValueChanged;
        public Vector2 Direction => input.sqrMagnitude > 0f ? input.normalized : Vector2.zero;
        public float Magnitude => Mathf.Clamp01(input.magnitude);
        public float Horizontal => input.x;
        public float Vertical => input.y;

        private void Awake()
        {
            self = transform as RectTransform;
        }

        private void OnEnable()
        {
            if (floating && background != null)
            {
                background.gameObject.SetActive(false);
            }

            ResetStick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (background == null)
            {
                return;
            }

            if (floating)
            {
                background.gameObject.SetActive(true);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        self, eventData.position, eventData.pressEventCamera, out var local))
                {
                    background.anchoredPosition = local;
                }
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, eventData.pressEventCamera, out var local))
            {
                return;
            }

            var radius = handleRange <= 0f ? 1f : handleRange;
            var clamped = Vector2.ClampMagnitude(local, radius);
            handle.anchoredPosition = clamped;

            var raw = clamped / radius;
            input = raw.magnitude < deadZone ? Vector2.zero : raw;
            onValueChanged.Invoke(input);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetStick();

            if (floating && background != null)
            {
                background.gameObject.SetActive(false);
            }
        }

        private void ResetStick()
        {
            input = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }

            onValueChanged.Invoke(Vector2.zero);
        }
    }
}
