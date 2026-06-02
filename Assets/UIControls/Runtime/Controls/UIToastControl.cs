using System;
using System.Collections;
using System.Collections.Generic;
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
    /// A single toast/snackbar slot driven by a FIFO queue. Call <see cref="Show"/> (or
    /// <see cref="ShowAction"/> for a snackbar with a button) as many times as you like; messages
    /// queue up and play one at a time: slide in + fade, hold for a duration, slide out, next.
    /// An optional action button fires a callback and dismisses early; the toast can also be
    /// swiped down to dismiss. Kinds (info / success / error) tint an accent strip.
    /// </summary>
    public sealed class UIToastControl : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public enum ToastKind
        {
            Info,
            Success,
            Error,
        }

        [Serializable]
        public sealed class ToastEvent : UnityEvent<string>
        {
        }

        private sealed class Request
        {
            public string Message;
            public float Duration;
            public ToastKind Kind;
            public string ActionLabel;
            public UnityAction Action;
        }

        [Header("Targets")]
        [Tooltip("The toast panel that slides in/out. Defaults to this object's RectTransform.")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private Graphic accent;
        [Tooltip("Optional snackbar action button. Hidden when a toast has no action.")]
        [SerializeField] private UIButtonControl actionButton;
        [SerializeField] private TMP_Text actionButtonLabel;

        [Header("Slide")]
        [Tooltip("anchoredPosition.y when hidden (off-screen).")]
        [SerializeField] private float hiddenY = -200f;
        [Tooltip("anchoredPosition.y when shown.")]
        [SerializeField] private float shownY = 60f;

        [Header("Timing")]
        [Min(0.1f)]
        [SerializeField] private float defaultDuration = 2.5f;
        [SerializeField] private UITweenSettings showTween = new UITweenSettings();
        [SerializeField] private UITweenSettings hideTween = new UITweenSettings();

        [Header("Dismiss")]
        [Tooltip("Downward drag distance (px) past which the toast dismisses.")]
        [SerializeField] private float swipeDismissDistance = 60f;

        [Header("Kind Accent Colors")]
        [SerializeField] private Color infoColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color successColor = new Color(0.2f, 0.65f, 0.42f, 1f);
        [SerializeField] private Color errorColor = new Color(0.86f, 0.34f, 0.38f, 1f);

        [Header("Queue")]
        [Tooltip("Maximum queued toasts; extra Show calls beyond this are dropped.")]
        [SerializeField] private int maxQueued = 8;

        [Header("Events")]
        [SerializeField] private ToastEvent onShown = new ToastEvent();
        [SerializeField] private UnityEvent onDismissed = new UnityEvent();

        private readonly Queue<Request> queue = new Queue<Request>();
        private Coroutine runner;
        private bool dismissRequested;
        private float dragStartY;
        private UnityAction currentAction;

        public ToastEvent OnShown => onShown;
        public UnityEvent OnDismissed => onDismissed;
        public bool IsShowing => runner != null;
        public int QueuedCount => queue.Count;

        private RectTransform Panel => panel != null ? panel : (panel = transform as RectTransform);

        private void Awake()
        {
            if (panel == null)
            {
                panel = transform as RectTransform;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            HideInstant();
            if (actionButton != null)
            {
                actionButton.OnClick.AddListener(HandleActionClicked);
            }
        }

        private void OnDisable()
        {
            if (actionButton != null)
            {
                actionButton.OnClick.RemoveListener(HandleActionClicked);
            }

            if (runner != null)
            {
                StopCoroutine(runner);
                runner = null;
            }

            queue.Clear();
            DOTween.Kill(Panel);
            if (canvasGroup != null)
            {
                DOTween.Kill(canvasGroup);
            }
        }

        public void Show(string message, float duration = -1f, ToastKind kind = ToastKind.Info)
        {
            Enqueue(new Request
            {
                Message = message,
                Duration = duration > 0f ? duration : defaultDuration,
                Kind = kind,
                ActionLabel = null,
                Action = null,
            });
        }

        public void ShowAction(string message, string actionLabel, UnityAction action,
            float duration = -1f, ToastKind kind = ToastKind.Info)
        {
            Enqueue(new Request
            {
                Message = message,
                Duration = duration > 0f ? duration : defaultDuration,
                Kind = kind,
                ActionLabel = actionLabel,
                Action = action,
            });
        }

        // Convenience hooks for UnityEvents / buttons in the inspector.
        public void ShowInfo(string message) => Show(message, -1f, ToastKind.Info);
        public void ShowSuccess(string message) => Show(message, -1f, ToastKind.Success);
        public void ShowError(string message) => Show(message, -1f, ToastKind.Error);

        private void Enqueue(Request request)
        {
            if (string.IsNullOrEmpty(request.Message) || queue.Count >= maxQueued)
            {
                return;
            }

            queue.Enqueue(request);
            if (runner == null && isActiveAndEnabled)
            {
                runner = StartCoroutine(RunQueue());
            }
        }

        private IEnumerator RunQueue()
        {
            while (queue.Count > 0)
            {
                var request = queue.Dequeue();
                yield return ShowOne(request);
            }

            runner = null;
        }

        private IEnumerator ShowOne(Request request)
        {
            dismissRequested = false;
            currentAction = request.Action;

            if (messageLabel != null)
            {
                messageLabel.text = request.Message;
            }

            if (accent != null)
            {
                accent.color = ColorFor(request.Kind);
            }

            var hasAction = !string.IsNullOrEmpty(request.ActionLabel);
            if (actionButton != null)
            {
                actionButton.gameObject.SetActive(hasAction);
                if (hasAction && actionButtonLabel != null)
                {
                    actionButtonLabel.text = request.ActionLabel;
                }
            }

            onShown?.Invoke(request.Message);

            // Slide + fade in.
            yield return Move(shownY, 1f, showTween);

            // Hold (interruptible by action click or swipe).
            var t = 0f;
            while (t < request.Duration && !dismissRequested)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Slide + fade out.
            yield return Move(hiddenY, 0f, hideTween);

            currentAction = null;
            onDismissed?.Invoke();
        }

        private IEnumerator Move(float targetY, float targetAlpha, UITweenSettings settings)
        {
            var duration = settings != null ? Mathf.Max(0f, settings.Duration) : 0f;
            if (duration <= Mathf.Epsilon || Panel == null)
            {
                SetY(targetY);
                SetAlpha(targetAlpha);
                yield break;
            }

            var sequence = DOTween.Sequence();
            sequence.Join(UIDOTweenUtility.TweenAnchoredPosition(
                Panel, new Vector2(Panel.anchoredPosition.x, targetY), duration));
            if (canvasGroup != null)
            {
                sequence.Join(UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, targetAlpha, duration));
            }

            settings.Apply(sequence);
            yield return sequence.WaitForCompletion();
        }

        private void HandleActionClicked()
        {
            currentAction?.Invoke();
            dismissRequested = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStartY = Panel != null ? Panel.anchoredPosition.y : 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Panel == null || eventData == null)
            {
                return;
            }

            // Allow dragging downward only (toward hidden); follow the pointer.
            var y = Mathf.Min(shownY, dragStartY + eventData.delta.y);
            // Accumulate via delta each frame.
            var pos = Panel.anchoredPosition;
            pos.y = Mathf.Min(shownY, pos.y + eventData.delta.y);
            Panel.anchoredPosition = pos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Panel == null)
            {
                return;
            }

            if (shownY - Panel.anchoredPosition.y >= swipeDismissDistance)
            {
                dismissRequested = true;
            }
            else
            {
                // Snap back to shown.
                SetY(shownY);
            }
        }

        private Color ColorFor(ToastKind kind)
        {
            switch (kind)
            {
                case ToastKind.Success: return successColor;
                case ToastKind.Error: return errorColor;
                default: return infoColor;
            }
        }

        private void HideInstant()
        {
            SetY(hiddenY);
            SetAlpha(0f);
            if (actionButton != null)
            {
                actionButton.gameObject.SetActive(false);
            }
        }

        private void SetY(float y)
        {
            if (Panel == null)
            {
                return;
            }

            var pos = Panel.anchoredPosition;
            pos.y = y;
            Panel.anchoredPosition = pos;
        }

        private void SetAlpha(float a)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = a;
            }
        }
    }
}
