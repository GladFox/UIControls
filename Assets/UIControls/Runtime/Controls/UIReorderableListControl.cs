using System;
using System.Collections.Generic;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Vertical drag-to-reorder list. Each child of <see cref="content"/> that carries a
    /// <see cref="UIReorderableItem"/> can be dragged; the others slide to make room and the order is
    /// committed (sibling indices) on drop. Items are laid out manually at a fixed row height, so no
    /// layout group is needed. Raises <see cref="OnReordered"/> with the from/to indices.
    /// </summary>
    public sealed class UIReorderableListControl : MonoBehaviour
    {
        [Serializable]
        public sealed class ReorderEvent : UnityEvent<int, int>
        {
        }

        [Header("Targets")]
        [SerializeField] private RectTransform content;

        [Header("Layout")]
        [SerializeField] private float rowHeight = 72f;
        [SerializeField] private float spacing = 10f;

        [Header("Animation")]
        [SerializeField] private float settleDuration = 0.18f;

        [Header("Events")]
        [SerializeField] private ReorderEvent onReordered = new ReorderEvent();

        private readonly List<RectTransform> order = new List<RectTransform>();
        private RectTransform dragged;
        private int dragFromIndex = -1;

        public ReorderEvent OnReordered => onReordered;
        public int Count => order.Count;

        private float Step => rowHeight + spacing;

        private void OnEnable()
        {
            Rebuild();
        }

        /// <summary>Re-reads the children of <see cref="content"/> and lays them out.</summary>
        public void Rebuild()
        {
            order.Clear();
            if (content == null)
            {
                return;
            }

            for (var i = 0; i < content.childCount; i++)
            {
                if (content.GetChild(i) is RectTransform rect)
                {
                    ConfigureRow(rect);
                    order.Add(rect);
                }
            }

            LayoutAll(false);
        }

        public int IndexOf(RectTransform item) => order.IndexOf(item);

        internal void BeginDrag(UIReorderableItem item)
        {
            if (item == null)
            {
                return;
            }

            dragged = item.transform as RectTransform;
            dragFromIndex = order.IndexOf(dragged);
            if (dragged != null)
            {
                dragged.SetAsLastSibling();
            }
        }

        internal void Drag(UIReorderableItem item, PointerEventData eventData)
        {
            if (dragged == null || content == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    content, eventData.position, eventData.pressEventCamera, out var local))
            {
                return;
            }

            var maxY = 0f;
            var minY = -(order.Count - 1) * Step;
            var y = Mathf.Clamp(local.y, minY, maxY);
            dragged.anchoredPosition = new Vector2(0f, y);

            var currentIndex = order.IndexOf(dragged);
            var targetIndex = Mathf.Clamp(Mathf.RoundToInt(-y / Step), 0, order.Count - 1);
            if (targetIndex != currentIndex)
            {
                order.RemoveAt(currentIndex);
                order.Insert(targetIndex, dragged);
                LayoutAll(true, dragged);
            }
        }

        internal void EndDrag(UIReorderableItem item)
        {
            if (dragged == null)
            {
                return;
            }

            var toIndex = order.IndexOf(dragged);
            UIDOTweenUtility.TweenAnchoredPosition(dragged, SlotPosition(toIndex), settleDuration)
                .SetEase(Ease.OutCubic).SetUpdate(true);

            // Commit sibling order so the hierarchy matches the logical order.
            for (var i = 0; i < order.Count; i++)
            {
                order[i].SetSiblingIndex(i);
            }

            if (dragFromIndex != toIndex && dragFromIndex >= 0)
            {
                onReordered.Invoke(dragFromIndex, toIndex);
            }

            dragged = null;
            dragFromIndex = -1;
        }

        private void ConfigureRow(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rowHeight);

            if (rect.GetComponent<UIReorderableItem>() == null)
            {
                rect.gameObject.AddComponent<UIReorderableItem>();
            }
        }

        private void LayoutAll(bool animate, RectTransform skip = null)
        {
            for (var i = 0; i < order.Count; i++)
            {
                var rect = order[i];
                if (rect == skip)
                {
                    continue;
                }

                var pos = SlotPosition(i);
                if (animate)
                {
                    UIDOTweenUtility.TweenAnchoredPosition(rect, pos, settleDuration).SetEase(Ease.OutCubic).SetUpdate(true);
                }
                else
                {
                    rect.anchoredPosition = pos;
                }
            }
        }

        private Vector2 SlotPosition(int index) => new Vector2(0f, -index * Step);
    }
}
