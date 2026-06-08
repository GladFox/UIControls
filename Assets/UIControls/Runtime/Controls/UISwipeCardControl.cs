using System;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Tinder-style swipeable card. Drag the card horizontally; release past <see cref="threshold"/> to
    /// fling it off-screen and raise <see cref="OnSwipe"/> (−1 left, +1 right), otherwise it springs
    /// back to centre with a rubber-band ease. The card tilts with the drag and optional like/nope
    /// overlays fade in. Call <see cref="ResetCard"/> to recentre for the next item.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UISwipeCardControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Overlays (optional)")]
        [SerializeField] private CanvasGroup likeOverlay;
        [SerializeField] private CanvasGroup nopeOverlay;

        [Header("Behaviour")]
        [Tooltip("Horizontal distance (px) past which release flings the card away.")]
        [SerializeField] private float threshold = 220f;
        [Tooltip("Degrees of tilt per pixel of horizontal drag.")]
        [SerializeField] private float tiltPerPixel = 0.06f;
        [SerializeField] private float flingDuration = 0.3f;
        [SerializeField] private float springDuration = 0.35f;

        [Header("Events")]
        [SerializeField] private IntEvent onSwipe = new IntEvent();

        private RectTransform self;
        private RectTransform parentRect;
        private Vector2 home;
        private Vector2 grabOffset;
        private bool flung;

        public IntEvent OnSwipe => onSwipe;

        private void Awake()
        {
            self = transform as RectTransform;
            parentRect = self.parent as RectTransform;
        }

        private void OnEnable()
        {
            home = self != null ? self.anchoredPosition : Vector2.zero;
            UpdateOverlays(0f);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (flung || self == null || parentRect == null)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, eventData.position, eventData.pressEventCamera, out var local))
            {
                grabOffset = self.anchoredPosition - local;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (flung || self == null || parentRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, eventData.position, eventData.pressEventCamera, out var local))
            {
                return;
            }

            var pos = local + grabOffset;
            self.anchoredPosition = pos;
            var dx = pos.x - home.x;
            self.localEulerAngles = new Vector3(0f, 0f, -dx * tiltPerPixel);
            UpdateOverlays(dx);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (flung || self == null)
            {
                return;
            }

            var dx = self.anchoredPosition.x - home.x;
            if (Mathf.Abs(dx) >= threshold)
            {
                Fling(dx > 0f ? 1 : -1);
            }
            else
            {
                SpringBack();
            }
        }

        /// <summary>Programmatically swipe in a direction (−1 left, +1 right).</summary>
        public void Swipe(int direction)
        {
            Fling(direction >= 0 ? 1 : -1);
        }

        /// <summary>Recentre the card instantly so it can present the next item.</summary>
        public void ResetCard()
        {
            flung = false;
            if (self != null)
            {
                self.anchoredPosition = home;
                self.localEulerAngles = Vector3.zero;
                self.localScale = Vector3.one;
            }

            UpdateOverlays(0f);
        }

        private void Fling(int direction)
        {
            flung = true;
            var offX = (parentRect != null ? parentRect.rect.width : 1200f) + 400f;
            var target = new Vector2(home.x + direction * offX, self.anchoredPosition.y + 60f);

            UIDOTweenUtility.TweenAnchoredPosition(self, target, flingDuration).SetEase(Ease.InCubic).SetUpdate(true)
                .OnComplete(() => onSwipe.Invoke(direction));
            self.DORotate(new Vector3(0f, 0f, -direction * 25f), flingDuration).SetUpdate(true);
        }

        private void SpringBack()
        {
            UIDOTweenUtility.TweenAnchoredPosition(self, home, springDuration).SetEase(Ease.OutBack).SetUpdate(true);
            self.DORotate(Vector3.zero, springDuration).SetEase(Ease.OutBack).SetUpdate(true);
            UpdateOverlays(0f);
        }

        private void UpdateOverlays(float dx)
        {
            var t = threshold <= 0f ? 0f : Mathf.Clamp01(Mathf.Abs(dx) / threshold);
            if (likeOverlay != null)
            {
                likeOverlay.alpha = dx > 0f ? t : 0f;
            }

            if (nopeOverlay != null)
            {
                nopeOverlay.alpha = dx < 0f ? t : 0f;
            }
        }
    }
}
