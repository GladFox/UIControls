using System;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Radial (pie) menu: item children fan out from the centre on <see cref="Open"/> and collapse on
    /// <see cref="Close"/>, with a staggered scale/move animation. Each item that carries a
    /// <see cref="UIButtonControl"/> is wired automatically to raise <see cref="OnItemSelected"/> (by
    /// sibling index) and close the menu. Items live under <see cref="itemsContainer"/>.
    /// </summary>
    public sealed class UIRadialMenuControl : MonoBehaviour
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Layout")]
        [Tooltip("Distance of each item from the centre when open.")]
        [SerializeField] private float radius = 170f;
        [Tooltip("Angle of the first item, in degrees (0 = up, clockwise).")]
        [SerializeField] private float startAngle;
        [Tooltip("Sweep across which items are distributed. 360 = full circle.")]
        [SerializeField] private float sweep = 360f;

        [Header("Animation")]
        [SerializeField] private float duration = 0.28f;
        [SerializeField] private float stagger = 0.04f;

        [Header("Events")]
        [SerializeField] private IntEvent onItemSelected = new IntEvent();

        private RectTransform[] items;
        private bool isOpen;

        public IntEvent OnItemSelected => onItemSelected;
        public bool IsOpen => isOpen;

        private void Awake()
        {
            CollectItems();
        }

        private void OnEnable()
        {
            ApplyClosedInstant();
        }

        private void CollectItems()
        {
            if (itemsContainer == null)
            {
                items = Array.Empty<RectTransform>();
                return;
            }

            var count = itemsContainer.childCount;
            items = new RectTransform[count];
            for (var i = 0; i < count; i++)
            {
                var child = itemsContainer.GetChild(i) as RectTransform;
                items[i] = child;

                var index = i;
                var button = child != null ? child.GetComponent<UIButtonControl>() : null;
                if (button != null)
                {
                    button.OnClick.AddListener(() => Select(index));
                }
            }
        }

        public void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (items == null)
            {
                CollectItems();
            }

            isOpen = true;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, 1f, duration).SetUpdate(true);
            }

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                item.gameObject.SetActive(true);
                item.localScale = Vector3.one * 0.4f;
                var target = PositionFor(i);
                var delay = stagger * i;

                UIDOTweenUtility.TweenAnchoredPosition(item, target, duration)
                    .SetEase(Ease.OutBack).SetDelay(delay).SetUpdate(true);
                item.DOScale(Vector3.one, duration).SetEase(Ease.OutBack).SetDelay(delay).SetUpdate(true);
            }
        }

        public void Close()
        {
            isOpen = false;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                UIDOTweenUtility.TweenCanvasGroupAlpha(canvasGroup, 0f, duration).SetUpdate(true);
            }

            if (items == null)
            {
                return;
            }

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                UIDOTweenUtility.TweenAnchoredPosition(item, Vector2.zero, duration).SetEase(Ease.InBack).SetUpdate(true);
                item.DOScale(Vector3.one * 0.4f, duration).SetEase(Ease.InBack).SetUpdate(true);
            }
        }

        public void Select(int index)
        {
            onItemSelected.Invoke(index);
            Close();
        }

        private Vector2 PositionFor(int index)
        {
            var count = Mathf.Max(1, items.Length);
            var step = Mathf.Approximately(sweep, 360f) ? sweep / count : sweep / Mathf.Max(1, count - 1);
            var angleDeg = startAngle + step * index;
            var rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * radius;
        }

        private void ApplyClosedInstant()
        {
            isOpen = false;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                item.anchoredPosition = Vector2.zero;
                item.localScale = Vector3.one * 0.4f;
            }
        }
    }
}
