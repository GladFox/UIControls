using System;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Slide-in menu / drawer that can dock to any of the four screen edges. The panel slides in from the
    /// configured <see cref="MenuSide"/> over a dimming backdrop; once it arrives, the menu items fly in
    /// one after another from the same direction the panel came from (with a scale pop and fade). For
    /// Left/Right the items are stacked in a column; for Top/Bottom they are laid out in a row — the
    /// control owns the layout so it adapts when the side changes. Items that carry a
    /// <see cref="UIButtonControl"/> are auto-wired to raise <see cref="OnItemSelected"/>; clicking the
    /// backdrop (or an item, if <see cref="closeOnSelect"/>) closes the menu.
    /// </summary>
    public sealed class UISideMenuControl : MonoBehaviour
    {
        public enum MenuSide
        {
            Left,
            Right,
            Top,
            Bottom,
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
        [Tooltip("Drawer thickness along the slide axis (width for Left/Right, height for Top/Bottom).")]
        [SerializeField] private float menuDepth = 240f;
        [Tooltip("Gap between items along the cross axis.")]
        [SerializeField] private float itemSpacing = 16f;
        [SerializeField] private bool closeOnSelect = true;

        [Header("Panel animation")]
        [SerializeField] private float panelDuration = 0.26f;
        [SerializeField] private Ease panelEase = Ease.OutCubic;

        [Header("Item animation (Brawl-Stars-style pop)")]
        [Tooltip("Distance (px) each item flies in from, along the open direction.")]
        [SerializeField] private float itemSlideDistance = 170f;
        [SerializeField] private float itemDuration = 0.34f;
        [SerializeField] private float itemStagger = 0.05f;
        [Tooltip("Delay before the first item starts, as a fraction of the panel slide.")]
        [SerializeField] private float itemStartDelayFactor = 0.22f;
        [Tooltip("Easing for each item's fly-in; OutBack gives the bouncy overshoot.")]
        [SerializeField] private Ease itemEase = Ease.OutBack;
        [Tooltip("Scale each item pops in from (0 disables the scale pop).")]
        [SerializeField] private float itemStartScale = 0.72f;

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
        public MenuSide Side => side;

        private bool IsHorizontalSlide => side == MenuSide.Left || side == MenuSide.Right;

        private Vector2 SlideDirection
        {
            get
            {
                switch (side)
                {
                    case MenuSide.Right: return new Vector2(1f, 0f);
                    case MenuSide.Top: return new Vector2(0f, 1f);
                    case MenuSide.Bottom: return new Vector2(0f, -1f);
                    default: return new Vector2(-1f, 0f); // Left
                }
            }
        }

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
            LayoutItems();
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
                itemGroups[i] = child != null ? child.GetComponent<CanvasGroup>() : null;

                if (child != null)
                {
                    child.anchorMin = new Vector2(0.5f, 0.5f);
                    child.anchorMax = new Vector2(0.5f, 0.5f);
                    child.pivot = new Vector2(0.5f, 0.5f);
                }

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

            switch (side)
            {
                case MenuSide.Right:
                    panel.anchorMin = new Vector2(1f, 0f);
                    panel.anchorMax = new Vector2(1f, 1f);
                    panel.pivot = new Vector2(1f, 0.5f);
                    panel.sizeDelta = new Vector2(menuDepth, 0f);
                    break;
                case MenuSide.Top:
                    panel.anchorMin = new Vector2(0f, 1f);
                    panel.anchorMax = new Vector2(1f, 1f);
                    panel.pivot = new Vector2(0.5f, 1f);
                    panel.sizeDelta = new Vector2(0f, menuDepth);
                    break;
                case MenuSide.Bottom:
                    panel.anchorMin = new Vector2(0f, 0f);
                    panel.anchorMax = new Vector2(1f, 0f);
                    panel.pivot = new Vector2(0.5f, 0f);
                    panel.sizeDelta = new Vector2(0f, menuDepth);
                    break;
                default: // Left
                    panel.anchorMin = new Vector2(0f, 0f);
                    panel.anchorMax = new Vector2(0f, 1f);
                    panel.pivot = new Vector2(0f, 0.5f);
                    panel.sizeDelta = new Vector2(menuDepth, 0f);
                    break;
            }

            panel.anchoredPosition = Vector2.zero;
            shownPos = panel.anchoredPosition;
        }

        /// <summary>
        /// Lays the items out centred along the cross axis: a column for Left/Right, a row for Top/Bottom.
        /// </summary>
        private void LayoutItems()
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            var vertical = IsHorizontalSlide; // L/R → stack vertically; T/B → row horizontally
            var n = items.Length;

            var sizes = new float[n];
            var total = 0f;
            for (var i = 0; i < n; i++)
            {
                var size = items[i] != null ? items[i].sizeDelta : Vector2.zero;
                sizes[i] = vertical ? size.y : size.x;
                total += sizes[i];
            }

            total += itemSpacing * Mathf.Max(0, n - 1);

            if (vertical)
            {
                // Top to bottom: item 0 at the top.
                var cursor = total * 0.5f;
                for (var i = 0; i < n; i++)
                {
                    var c = cursor - sizes[i] * 0.5f;
                    itemRest[i] = new Vector2(0f, c);
                    cursor -= sizes[i] + itemSpacing;
                }
            }
            else
            {
                // Left to right: item 0 at the left.
                var cursor = -total * 0.5f;
                for (var i = 0; i < n; i++)
                {
                    var c = cursor + sizes[i] * 0.5f;
                    itemRest[i] = new Vector2(c, 0f);
                    cursor += sizes[i] + itemSpacing;
                }
            }

            for (var i = 0; i < n; i++)
            {
                if (items[i] != null)
                {
                    items[i].anchoredPosition = itemRest[i];
                }
            }
        }

        public void SetSide(MenuSide newSide)
        {
            side = newSide;
            ApplyAnchors();
            LayoutItems();
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
                backdrop.gameObject.SetActive(true);
                backdrop.DOKill();
                backdrop.alpha = 0f;
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
                UIDOTweenUtility.TweenCanvasGroupAlpha(backdrop, 0f, panelDuration).SetUpdate(true)
                    .OnComplete(() => { if (backdrop != null) backdrop.gameObject.SetActive(false); });
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
            var offset = SlideDirection * itemSlideDistance;

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                item.DOKill();
                item.anchoredPosition = itemRest[i] + offset;
                if (itemStartScale > 0f)
                {
                    item.localScale = Vector3.one * itemStartScale;
                }

                var group = itemGroups[i];
                if (group != null)
                {
                    group.DOKill();
                    group.alpha = 0f;
                }

                var delay = startDelay + i * itemStagger;
                UIDOTweenUtility.TweenAnchoredPosition(item, itemRest[i], itemDuration)
                    .SetEase(itemEase).SetDelay(delay).SetUpdate(true);

                if (itemStartScale > 0f)
                {
                    item.DOScale(1f, itemDuration).SetEase(itemEase).SetDelay(delay).SetUpdate(true);
                }

                if (group != null)
                {
                    // Fade a touch faster than the slide so items are solid before they settle.
                    UIDOTweenUtility.TweenCanvasGroupAlpha(group, 1f, itemDuration * 0.6f).SetDelay(delay).SetUpdate(true);
                }
            }
        }

        private Vector2 HiddenPos()
        {
            var size = IsHorizontalSlide
                ? (panel != null && panel.rect.width > 1f ? panel.rect.width : menuDepth)
                : (panel != null && panel.rect.height > 1f ? panel.rect.height : menuDepth);
            return shownPos + SlideDirection * (size + 8f);
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
                backdrop.DOKill();
                backdrop.alpha = 0f;
                backdrop.blocksRaycasts = false;
                // Fully deactivate so it can never overlap or eat clicks on the trigger while closed.
                backdrop.gameObject.SetActive(false);
            }
        }
    }
}
