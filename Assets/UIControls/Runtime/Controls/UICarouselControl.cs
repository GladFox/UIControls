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
    /// A horizontal paged carousel built on a <see cref="ScrollRect"/>. Pages are laid out
    /// left-to-right at <see cref="pageWidth"/>; dragging snaps to the nearest page on release.
    /// Dot indicators highlight the current page, and optional autoplay advances pages on a timer
    /// (paused while the user is dragging). Lives on the same GameObject as the ScrollRect.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UICarouselControl : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler
    {
        public enum AutoplayMode
        {
            /// <summary>Advance forward; wrap from the last page back to the first.</summary>
            Loop,
            /// <summary>Advance forward to the end, then step back to the start, bouncing back and forth.</summary>
            PingPong,
        }

        [Serializable]
        public sealed class PageChangedEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [Tooltip("Width of a single page (usually the viewport width).")]
        [SerializeField] private float pageWidth = 660f;

        [Header("Dots")]
        [Tooltip("Optional dot graphics, one per page, highlighted to show the current page.")]
        [SerializeField] private Graphic[] dots = Array.Empty<Graphic>();
        [SerializeField] private Color activeDotColor = new Color(0.95f, 0.97f, 1f, 1f);
        [SerializeField] private Color inactiveDotColor = new Color(0.45f, 0.5f, 0.62f, 1f);
        [SerializeField] private float activeDotScale = 1.3f;

        [Header("Behaviour")]
        [SerializeField] private int initialPage;
        [Tooltip("Manual Next/Previous wrap around the ends.")]
        [SerializeField] private bool wrap = true;

        [Header("Swipe")]
        [Tooltip("A quick horizontal flick advances one page in its direction even if the drag was short. Slower drags just snap to the nearest page.")]
        [SerializeField] private bool swipeGesture = true;
        [Tooltip("Minimum horizontal drag distance (screen px) to count as a swipe.")]
        [SerializeField] private float swipeMinDistance = 50f;
        [Tooltip("Minimum horizontal flick speed (screen px/s) to count as a swipe.")]
        [SerializeField] private float swipeMinSpeed = 500f;

        [Header("Autoplay")]
        [SerializeField] private bool autoplay;
        [Min(0.5f)]
        [SerializeField] private float autoplayInterval = 3f;
        [SerializeField] private AutoplayMode autoplayMode = AutoplayMode.PingPong;

        [Header("Animation")]
        [SerializeField] private UITweenSettings snapTween = new UITweenSettings();

        [Header("Events")]
        [SerializeField] private PageChangedEvent onPageChanged = new PageChangedEvent();

        private int currentPage;
        private bool dragging;
        private float autoplayTimer;
        private int autoplayDir = 1;
        private float dragStartTime;
        private Tween snap;

        public PageChangedEvent OnPageChanged => onPageChanged;
        public int CurrentPage => currentPage;
        public int PageCount => content != null ? content.childCount : 0;

        /// <summary>Toggle autoplay at runtime. Setting it (re)starts the interval timer from zero.</summary>
        public bool Autoplay
        {
            get => autoplay;
            set
            {
                autoplay = value;
                autoplayTimer = 0f;
            }
        }

        /// <summary>Autoplay direction behaviour: Loop (wrap) or PingPong (bounce back and forth).</summary>
        public AutoplayMode Mode
        {
            get => autoplayMode;
            set => autoplayMode = value;
        }

        private void Awake()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }

            if (content == null && scrollRect != null)
            {
                content = scrollRect.content;
            }
        }

        private void OnEnable()
        {
            currentPage = Mathf.Clamp(initialPage, 0, Mathf.Max(0, PageCount - 1));
            SnapInstant(currentPage);
            UpdateDots();
        }

        private void OnDisable()
        {
            KillSnap();
        }

        private void Update()
        {
            if (!autoplay || dragging || PageCount <= 1)
            {
                return;
            }

            autoplayTimer += Time.unscaledDeltaTime;
            if (autoplayTimer >= autoplayInterval)
            {
                autoplayTimer = 0f;
                AutoplayStep();
            }
        }

        private void AutoplayStep()
        {
            if (PageCount <= 1)
            {
                return;
            }

            if (autoplayMode == AutoplayMode.PingPong)
            {
                var next = currentPage + autoplayDir;
                if (next >= PageCount || next < 0)
                {
                    autoplayDir = -autoplayDir;
                    next = Mathf.Clamp(currentPage + autoplayDir, 0, PageCount - 1);
                }

                GoTo(next);
            }
            else
            {
                var next = currentPage + 1;
                GoTo(next >= PageCount ? 0 : next);
            }
        }

        public void GoTo(int index, bool animate = true, bool notify = true)
        {
            if (PageCount == 0 || content == null)
            {
                return;
            }

            var clamped = Mathf.Clamp(index, 0, PageCount - 1);
            currentPage = clamped;
            autoplayTimer = 0f;

            // Keep ping-pong bouncing off the ends even after manual / swipe navigation.
            if (currentPage <= 0)
            {
                autoplayDir = 1;
            }
            else if (currentPage >= PageCount - 1)
            {
                autoplayDir = -1;
            }

            var targetX = -clamped * pageWidth;
            KillSnap();

            var duration = snapTween != null ? Mathf.Max(0f, snapTween.Duration) : 0f;
            if (!animate || duration <= Mathf.Epsilon)
            {
                SetContentX(targetX);
            }
            else
            {
                snap = UIDOTweenUtility.TweenAnchoredPosition(
                    content, new Vector2(targetX, content.anchoredPosition.y), duration);
                snapTween.Apply(snap);
            }

            UpdateDots();

            if (notify)
            {
                onPageChanged?.Invoke(currentPage);
            }
        }

        public void Next()
        {
            if (PageCount == 0)
            {
                return;
            }

            var next = currentPage + 1;
            if (next >= PageCount)
            {
                next = wrap ? 0 : PageCount - 1;
            }

            GoTo(next);
        }

        public void Previous()
        {
            if (PageCount == 0)
            {
                return;
            }

            var prev = currentPage - 1;
            if (prev < 0)
            {
                prev = wrap ? PageCount - 1 : 0;
            }

            GoTo(prev);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
            dragStartTime = Time.unscaledTime;
            autoplayTimer = 0f; // user took over — don't let autoplay fire right after release
            KillSnap();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
            autoplayTimer = 0f;

            if (PageCount == 0 || pageWidth <= Mathf.Epsilon || content == null)
            {
                return;
            }

            // Quick horizontal flick → advance one page in the swipe direction.
            if (swipeGesture && eventData != null)
            {
                var dx = eventData.position.x - eventData.pressPosition.x;
                var dt = Mathf.Max(0.0001f, Time.unscaledTime - dragStartTime);
                var speed = Mathf.Abs(dx) / dt;
                if (Mathf.Abs(dx) >= swipeMinDistance && speed >= swipeMinSpeed)
                {
                    GoTo(currentPage + (dx < 0f ? 1 : -1));
                    return;
                }
            }

            // Otherwise snap to whichever page is nearest after the drag.
            var nearest = Mathf.RoundToInt(-content.anchoredPosition.x / pageWidth);
            GoTo(nearest);
        }

        private void SnapInstant(int index)
        {
            KillSnap();
            SetContentX(-index * pageWidth);
        }

        private void SetContentX(float x)
        {
            if (content == null)
            {
                return;
            }

            var pos = content.anchoredPosition;
            pos.x = x;
            content.anchoredPosition = pos;
        }

        private void UpdateDots()
        {
            if (dots == null)
            {
                return;
            }

            for (var i = 0; i < dots.Length; i++)
            {
                var dot = dots[i];
                if (dot == null)
                {
                    continue;
                }

                var active = i == currentPage;
                dot.color = active ? activeDotColor : inactiveDotColor;
                dot.rectTransform.localScale = Vector3.one * (active ? activeDotScale : 1f);
            }
        }

        private void KillSnap()
        {
            if (snap != null && snap.IsActive())
            {
                snap.Kill(false);
            }

            snap = null;
        }
    }
}
