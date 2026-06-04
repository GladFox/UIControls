using System;
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
    /// Pull-to-refresh wrapper for a vertical <see cref="ScrollRect"/>. Lives on the same
    /// GameObject as the ScrollRect. When the user overscrolls past the top and releases beyond
    /// <see cref="pullThreshold"/>, <see cref="OnRefresh"/> fires; call <see cref="EndRefreshing"/>
    /// when the work is done and the indicator springs back.
    ///
    /// An overlay indicator (with a spinner) at the top of the viewport reveals as you pull and
    /// rotates while refreshing. Assumes the ScrollRect content is top-anchored (the uGUI default),
    /// so overscrolling at the top drives <c>content.anchoredPosition.y</c> negative.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UIPullToRefreshControl : MonoBehaviour, IEndDragHandler
    {
        [Header("Targets")]
        [SerializeField] private ScrollRect scrollRect;
        [Tooltip("Overlay shown at the top of the viewport. Anchored top-center; its anchoredPosition.y is driven between hiddenY and restY.")]
        [SerializeField] private RectTransform indicator;
        [Tooltip("Graphic rotated to show progress/spinning.")]
        [SerializeField] private RectTransform spinner;
        [SerializeField] private TMP_Text label;

        [Header("Pull")]
        [Tooltip("Overscroll distance (px) past which a release triggers a refresh.")]
        [SerializeField] private float pullThreshold = 120f;
        [SerializeField] private float hiddenY = 70f;
        [SerializeField] private float restY = -60f;

        [Header("Spinner")]
        [Tooltip("Degrees the spinner rotates per pixel pulled (pre-refresh).")]
        [SerializeField] private float degreesPerPixel = 2.2f;
        [Tooltip("Continuous spin speed while refreshing (deg/s).")]
        [SerializeField] private float spinSpeed = 320f;

        [Header("Text")]
        [SerializeField] private string pullText = "Pull to refresh";
        [SerializeField] private string releaseText = "Release to refresh";
        [SerializeField] private string refreshingText = "Refreshing…";

        [Header("Animation")]
        [SerializeField] private UITweenSettings returnTween = new UITweenSettings();

        [Header("Events")]
        [SerializeField] private UnityEvent onRefresh = new UnityEvent();

        private bool refreshing;
        private float spinAngle;
        private Tween indicatorTween;

        public UnityEvent OnRefresh => onRefresh;
        public bool IsRefreshing => refreshing;

        private void Awake()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }
        }

        private void OnEnable()
        {
            SetIndicatorY(hiddenY);
            UpdateLabel(pullText);
        }

        private void OnDisable()
        {
            KillIndicatorTween();
        }

        private void Update()
        {
            if (refreshing)
            {
                spinAngle -= spinSpeed * Time.unscaledDeltaTime;
                ApplySpin();
                return;
            }

            var pull = CurrentPull();
            var t = pullThreshold > Mathf.Epsilon ? Mathf.Clamp01(pull / pullThreshold) : 0f;
            SetIndicatorY(Mathf.Lerp(hiddenY, restY, t));

            spinAngle = -pull * degreesPerPixel;
            ApplySpin();

            UpdateLabel(pull >= pullThreshold ? releaseText : pullText);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (refreshing)
            {
                return;
            }

            if (CurrentPull() >= pullThreshold)
            {
                BeginRefreshing();
            }
        }

        /// <summary>Trigger the refresh state programmatically (also usable to show the spinner up front).</summary>
        public void BeginRefreshing(bool notify = true)
        {
            if (refreshing)
            {
                return;
            }

            refreshing = true;
            UpdateLabel(refreshingText);
            AnimateIndicatorTo(restY);

            if (notify)
            {
                onRefresh?.Invoke();
            }
        }

        public void EndRefreshing()
        {
            if (!refreshing)
            {
                return;
            }

            refreshing = false;
            UpdateLabel(pullText);
            AnimateIndicatorTo(hiddenY);
        }

        private float CurrentPull()
        {
            if (scrollRect == null || scrollRect.content == null)
            {
                return 0f;
            }

            // Top-anchored content: overscrolling at the top pushes anchoredPosition.y negative.
            return Mathf.Max(0f, -scrollRect.content.anchoredPosition.y);
        }

        private void AnimateIndicatorTo(float y)
        {
            KillIndicatorTween();

            if (indicator == null)
            {
                return;
            }

            var duration = returnTween != null ? Mathf.Max(0f, returnTween.Duration) : 0f;
            if (duration <= Mathf.Epsilon)
            {
                SetIndicatorY(y);
                return;
            }

            indicatorTween = UIDOTweenUtility.TweenAnchoredPosition(
                indicator, new Vector2(indicator.anchoredPosition.x, y), duration);
            returnTween.Apply(indicatorTween);
        }

        private void SetIndicatorY(float y)
        {
            if (indicator == null)
            {
                return;
            }

            var pos = indicator.anchoredPosition;
            pos.y = y;
            indicator.anchoredPosition = pos;
        }

        private void ApplySpin()
        {
            if (spinner != null)
            {
                spinner.localEulerAngles = new Vector3(0f, 0f, spinAngle);
            }
        }

        private void UpdateLabel(string text)
        {
            if (label != null && label.text != text)
            {
                label.text = text;
            }
        }

        private void KillIndicatorTween()
        {
            if (indicatorTween != null && indicatorTween.IsActive())
            {
                indicatorTween.Kill(false);
            }

            indicatorTween = null;
        }
    }
}
