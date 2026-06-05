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
        [SerializeField] private bool autoplay;
        [Min(0.5f)]
        [SerializeField] private float autoplayInterval = 3f;
        [Tooltip("Autoplay wraps from the last page back to the first.")]
        [SerializeField] private bool autoplayLoops = true;

        [Header("Animation")]
        [SerializeField] private UITweenSettings snapTween = new UITweenSettings();

        [Header("Events")]
        [SerializeField] private PageChangedEvent onPageChanged = new PageChangedEvent();

        private int currentPage;
        private bool dragging;
        private float autoplayTimer;
        private Tween snap;

        public PageChangedEvent OnPageChanged => onPageChanged;
        public int CurrentPage => currentPage;
        public int PageCount => content != null ? content.childCount : 0;

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
                Next();
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
                next = autoplayLoops ? 0 : PageCount - 1;
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
                prev = autoplayLoops ? PageCount - 1 : 0;
            }

            GoTo(prev);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
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
