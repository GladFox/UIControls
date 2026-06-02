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
    /// A sheet that slides up from the bottom and settles at one of several snap points
    /// (e.g. collapsed / half / expanded). Drag it to move between snaps; flick down or drag
    /// below the dismiss threshold to close. A backdrop fades in proportionally to how open the
    /// sheet is and dismisses on click. Snaps use an overshoot ease for a springy, rubber-band feel.
    ///
    /// The sheet is anchored to the bottom of its parent (pivot Y = 0); its
    /// <see cref="RectTransform.anchoredPosition"/>.y is what moves — <see cref="closedY"/> is fully
    /// hidden, larger Y is more open.
    /// </summary>
    public sealed class UIBottomSheetControl : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Serializable]
        public sealed class StateChangedEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [Tooltip("The sheet RectTransform that slides. Defaults to this object's RectTransform.")]
        [SerializeField] private RectTransform sheet;
        [Tooltip("Backdrop dimmer behind the sheet. Its alpha tracks how open the sheet is; clicking it closes the sheet.")]
        [SerializeField] private CanvasGroup backdrop;

        [Header("Snap Points (anchoredPosition.y)")]
        [Tooltip("Y when fully closed/hidden (sheet off-screen below).")]
        [SerializeField] private float closedY = -900f;
        [Tooltip("Open snap positions in ascending order (collapsed -> expanded), as anchoredPosition.y.")]
        [SerializeField] private float[] openSnapPoints = { -450f, 0f };
        [Tooltip("Snap index shown on enable. -1 = start closed.")]
        [SerializeField] private int initialSnapIndex = -1;

        [Header("Dismiss")]
        [Tooltip("If released with the sheet below this Y, it closes instead of snapping.")]
        [SerializeField] private float dismissBelowY = -650f;
        [Tooltip("Downward drag speed (px/s, screen space) that triggers a flick-to-dismiss.")]
        [SerializeField] private float flickDismissSpeed = 1300f;

        [Header("Backdrop")]
        [Range(0f, 1f)]
        [SerializeField] private float backdropMaxAlpha = 0.6f;
        [SerializeField] private bool closeOnBackdropClick = true;

        [Header("Animation")]
        [SerializeField] private UITweenSettings snapTween = new UITweenSettings();

        [Header("Interaction")]
        [SerializeField] private bool interactable = true;

        [Header("Events")]
        [Tooltip("Fires with the new snap index, or -1 when closed.")]
        [SerializeField] private StateChangedEvent onStateChanged = new StateChangedEvent();

        private int currentIndex = -1;
        private bool dragging;
        private float grabOffsetY;
        private float lastDragDeltaY;
        private Sequence animSequence;

        public StateChangedEvent OnStateChanged => onStateChanged;
        public int CurrentSnapIndex => currentIndex;
        public bool IsOpen => currentIndex >= 0;
        public int SnapCount => openSnapPoints?.Length ?? 0;

        public bool Interactable
        {
            get => interactable;
            set => interactable = value;
        }

        private RectTransform Sheet => sheet != null ? sheet : (sheet = transform as RectTransform);
        private float MaxOpenY => openSnapPoints != null && openSnapPoints.Length > 0
            ? openSnapPoints[openSnapPoints.Length - 1]
            : 0f;

        private void Awake()
        {
            if (sheet == null)
            {
                sheet = transform as RectTransform;
            }

            if (closeOnBackdropClick && backdrop != null &&
                backdrop.GetComponent<UIBottomSheetBackdrop>() == null)
            {
                backdrop.gameObject.AddComponent<UIBottomSheetBackdrop>().Bind(this);
            }
        }

        private void OnEnable()
        {
            currentIndex = Mathf.Clamp(initialSnapIndex, -1, SnapCount - 1);
            ApplyInstant(currentIndex);
        }

        private void OnDisable()
        {
            KillTween();
        }

        public void Open(int index = 0, bool animate = true, bool notify = true)
        {
            SnapTo(Mathf.Clamp(index, 0, Mathf.Max(0, SnapCount - 1)), animate, notify);
        }

        public void Close(bool animate = true, bool notify = true)
        {
            SetState(-1, animate, notify);
        }

        /// <summary>Backdrop relay entry point.</summary>
        public void RequestClose()
        {
            if (interactable)
            {
                Close();
            }
        }

        public void SnapTo(int index, bool animate = true, bool notify = true)
        {
            if (SnapCount == 0)
            {
                return;
            }

            SetState(Mathf.Clamp(index, 0, SnapCount - 1), animate, notify);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!interactable || eventData == null || Sheet == null)
            {
                return;
            }

            KillTween();
            dragging = true;
            lastDragDeltaY = 0f;

            if (TryGetParentLocalY(eventData, out var localY))
            {
                grabOffsetY = Sheet.anchoredPosition.y - localY;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging || !interactable || eventData == null || Sheet == null)
            {
                return;
            }

            if (!TryGetParentLocalY(eventData, out var localY))
            {
                return;
            }

            lastDragDeltaY = eventData.delta.y;
            var y = Mathf.Clamp(localY + grabOffsetY, closedY, MaxOpenY);
            SetSheetY(y);
            UpdateBackdropFromY(y);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;

            var y = Sheet.anchoredPosition.y;
            var flickDown = eventData != null && eventData.delta.y < 0f &&
                            Mathf.Abs(lastDragDeltaY) / Mathf.Max(Time.unscaledDeltaTime, 0.0001f) > flickDismissSpeed;

            if (flickDown || y < dismissBelowY)
            {
                SetState(-1, animate: true, notify: true);
                return;
            }

            SetState(NearestSnapIndex(y), animate: true, notify: true);
        }

        private int NearestSnapIndex(float y)
        {
            var best = 0;
            var bestDist = float.MaxValue;
            for (var i = 0; i < openSnapPoints.Length; i++)
            {
                var d = Mathf.Abs(openSnapPoints[i] - y);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }

        private void SetState(int index, bool animate, bool notify)
        {
            currentIndex = index;
            var targetY = index < 0 ? closedY : openSnapPoints[index];
            var targetAlpha = index < 0 ? 0f : BackdropAlphaForY(targetY);

            KillTween();

            if (backdrop != null)
            {
                backdrop.blocksRaycasts = index >= 0;
            }

            var duration = snapTween != null ? Mathf.Max(0f, snapTween.Duration) : 0f;
            if (duration <= Mathf.Epsilon)
            {
                SetSheetY(targetY);
                SetBackdropAlpha(targetAlpha);
                if (notify)
                {
                    onStateChanged?.Invoke(currentIndex);
                }

                return;
            }

            var sequence = DOTween.Sequence();
            sequence.Join(UIDOTweenUtility.TweenAnchoredPosition(
                Sheet, new Vector2(Sheet.anchoredPosition.x, targetY), duration));
            if (backdrop != null)
            {
                sequence.Join(UIDOTweenUtility.TweenCanvasGroupAlpha(backdrop, targetAlpha, duration));
            }

            snapTween.Apply(sequence);
            if (notify)
            {
                sequence.OnComplete(() => onStateChanged?.Invoke(currentIndex));
            }

            animSequence = sequence;
        }

        private void ApplyInstant(int index)
        {
            KillTween();
            var targetY = index < 0 ? closedY : openSnapPoints[Mathf.Clamp(index, 0, SnapCount - 1)];
            SetSheetY(targetY);
            SetBackdropAlpha(index < 0 ? 0f : BackdropAlphaForY(targetY));
            if (backdrop != null)
            {
                backdrop.blocksRaycasts = index >= 0;
            }
        }

        private bool TryGetParentLocalY(PointerEventData eventData, out float localY)
        {
            localY = 0f;
            if (Sheet == null || !(Sheet.parent is RectTransform parentRect))
            {
                return false;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, eventData.position, eventData.pressEventCamera, out var local))
            {
                return false;
            }

            localY = local.y;
            return true;
        }

        private void SetSheetY(float y)
        {
            if (Sheet == null)
            {
                return;
            }

            var pos = Sheet.anchoredPosition;
            pos.y = y;
            Sheet.anchoredPosition = pos;
        }

        private float BackdropAlphaForY(float y)
        {
            var range = MaxOpenY - closedY;
            var t = range > Mathf.Epsilon ? Mathf.Clamp01((y - closedY) / range) : 0f;
            return t * backdropMaxAlpha;
        }

        private void UpdateBackdropFromY(float y)
        {
            SetBackdropAlpha(BackdropAlphaForY(y));
            if (backdrop != null)
            {
                backdrop.blocksRaycasts = y > closedY + 1f;
            }
        }

        private void SetBackdropAlpha(float alpha)
        {
            if (backdrop != null)
            {
                backdrop.alpha = alpha;
            }
        }

        private void KillTween()
        {
            if (animSequence != null && animSequence.IsActive())
            {
                animSequence.Kill(false);
            }

            animSequence = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += SyncInEditor;
        }

        private void SyncInEditor()
        {
            UnityEditor.EditorApplication.delayCall -= SyncInEditor;
            if (this == null || Application.isPlaying)
            {
                return;
            }

            if (sheet == null)
            {
                sheet = transform as RectTransform;
            }

            ApplyInstant(Mathf.Clamp(initialSnapIndex, -1, SnapCount - 1));
        }
#endif
    }

    /// <summary>
    /// Tiny relay added to the backdrop so a click closes the owning sheet, without the sheet
    /// needing to be the raycast target of the full-screen dimmer.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIBottomSheetBackdrop : MonoBehaviour, IPointerClickHandler
    {
        private UIBottomSheetControl owner;

        public void Bind(UIBottomSheetControl sheet)
        {
            owner = sheet;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (owner != null && eventData != null && eventData.button == PointerEventData.InputButton.Left)
            {
                owner.RequestClose();
            }
        }
    }
}
