using System;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Slide-in side menu / drawer. The panel slides in from the configured <see cref="Side"/> (left or
    /// right) over a dimming backdrop; once it arrives, the menu items fly in one after another from the
    /// same direction the panel came from (with an optional fade). Items that carry a
    /// <see cref="UIButtonControl"/> are auto-wired to raise <see cref="OnItemSelected"/>; clicking the
    /// backdrop (or an item, if <see cref="closeOnSelect"/>) closes the menu.
    /// </summary>
    public sealed class UISideMenuControl : MonoBehaviour
    {
        public enum MenuSide
        {
            Left,
            Right,
        }

        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup backdrop;
        [SerializeField] private UIButtonControl backdropButton;
        [SerializeField] private UIButtonControl toggleButton;
        [SerializeField] private RectTransform itemsContainer;

        [Header("Configuration")]
        [SerializeField] private MenuSide side = MenuSide.Left;
        [SerializeField] private bool closeOnSelect = true;

        [Header("Panel animation")]
        [SerializeField] private float panelDuration = 0.32f;
        [SerializeField] private Ease panelEase = Ease.OutCubic;

        [Header("Item animation")]
        [Tooltip("Distance (px) each item flies in from, along the open direction.")]
        [SerializeField] private float itemSlideDistance = 240f;
        [SerializeField] private float itemDuration = 0.28f;
        [SerializeField] private float itemStagger = 0.06f;
        [Tooltip("Delay before the first item starts, as a fraction of the panel slide.")]
        [SerializeField] private float itemStartDelayFactor = 0.35f;

        [Header("Events")]
        [SerializeField] private IntEvent onItemSelected = new IntEvent();

        private RectTransform[] items;
        private CanvasGroup[] itemGroups;
        private Vector2[] itemRest;
        private Vector2 shownPos;
        private bool isOpen;
        private bool initialized;

        public IntEvent OnItemSelected => onItemSelected;
        public bool IsOpen => isOpen;

        private float Direction => side == MenuSide.Left ? -1f : 1f;

        private void Awake()
        {
            if (toggleButton != null) toggleButton.OnClick.AddListener(Toggle);
            if (backdropButton != null) backdropButton.OnClick.AddListener(Close);
            CollectItems();
            Initialize();
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            ApplyAnchors();
            CloseInstant();
        }

        private void CollectItems()
        {
            if (itemsContainer == null)
            {
                items = Array.Empty<RectTransform>();
                itemGroups = Array.Empty<CanvasGroup>();
                itemRest = Array.Empty<Vector2>();
                return;
            }

            var count = itemsContainer.childCount;
            items = new RectTransform[count];
            itemGroups = new CanvasGroup[count];
            itemRest = new Vector2[count];

            for (var i = 0; i < count; i++)
            {
                var child = itemsContainer.GetChild(i) as RectTransform;
                items[i] = child;
                itemRest[i] = child != null ? child.anchoredPosition : Vector2.zero;
                itemGroups[i] = child != null ? child.GetComponent<CanvasGroup>() : null;

                var index = i;
                var button = child != null ? child.GetComponent<UIButtonControl>() : null;
                if (button != null)
                {
                    button.OnClick.AddListener(() => Select(index));
                }
            }
        }

        private void ApplyAnchors()
        {
            if (panel == null)
            {
                return;
            }

            var x = side == MenuSide.Left ? 0f : 1f;
            panel.anchorMin = new Vector2(x, 0f);
            panel.anchorMax = new Vector2(x, 1f);
            panel.pivot = new Vector2(x, 0.5f);
            // Flush to the edge, full height.
            panel.anchoredPosition = new Vector2(0f, 0f);
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, 0f);
            shownPos = panel.anchoredPosition;
        }

        public void SetSide(MenuSide newSide)
        {
            side = newSide;
            ApplyAnchors();
            // Re-read item rest positions in case layout differs, then re-close.
            for (var i = 0; items != null && i < items.Length; i++)
            {
                if (items[i] != null) items[i].anchoredPosition = itemRest[i];
            }
            CloseInstant();
        }

        public void Toggle()
        {
            if (isOpen) Close(); else Open();
        }

        public void Open()
        {
            Initialize();
            isOpen = true;

            if (panel != null)
            {
                panel.DOKill();
                UIDOTweenUtility.TweenAnchoredPosition(panel, shownPos, panelDuration).SetEase(panelEase).SetUpdate(true);
            }

            if (backdrop != null)
            {
                backdrop.DOKill();
                backdrop.blocksRaycasts = true;
                UIDOTweenUtility.TweenCanvasGroupAlpha(backdrop, 1f, panelDuration).SetUpdate(true);
            }

            AnimateItemsIn();
        }

        public void Close()
        {
            isOpen = false;

            if (panel != null)
            {
                panel.DOKill();
                UIDOTweenUtility.TweenAnchoredPosition(panel, HiddenPos(), panelDuration).SetEase(Ease.InCubic).SetUpdate(true);
            }

            if (backdrop != null)
            {
                backdrop.DOKill();
                backdrop.blocksRaycasts = false;
                UIDOTweenUtility.TweenCanvasGroupAlpha(backdrop, 0f, panelDuration).SetUpdate(true);
            }
        }

        public void Select(int index)
        {
            onItemSelected.Invoke(index);
            if (closeOnSelect)
            {
                Close();
            }
        }

        private void AnimateItemsIn()
        {
            if (items == null)
            {
                return;
            }

            var startDelay = panelDuration * Mathf.Clamp01(itemStartDelayFactor);
            var offset = Direction * itemSlideDistance;

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                item.DOKill();
                item.anchoredPosition = itemRest[i] + new Vector2(offset, 0f);

                var group = itemGroups[i];
                if (group != null)
                {
                    group.DOKill();
                    group.alpha = 0f;
                }

                var delay = startDelay + i * itemStagger;
                UIDOTweenUtility.TweenAnchoredPosition(item, itemRest[i], itemDuration)
                    .SetEase(Ease.OutCubic).SetDelay(delay).SetUpdate(true);

                if (group != null)
                {
                    UIDOTweenUtility.TweenCanvasGroupAlpha(group, 1f, itemDuration).SetDelay(delay).SetUpdate(true);
                }
            }
        }

        private Vector2 HiddenPos()
        {
            var width = panel != null ? panel.rect.width : itemSlideDistance;
            if (width <= 1f)
            {
                width = panel != null ? panel.sizeDelta.x : itemSlideDistance;
            }

            return shownPos + new Vector2(Direction * (width + 8f), 0f);
        }

        private void CloseInstant()
        {
            isOpen = false;

            if (panel != null)
            {
                panel.anchoredPosition = HiddenPos();
            }

            if (backdrop != null)
            {
                backdrop.alpha = 0f;
                backdrop.blocksRaycasts = false;
            }
        }
    }
}
