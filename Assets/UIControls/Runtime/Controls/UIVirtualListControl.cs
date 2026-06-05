using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A vertically scrolling list that only instantiates enough cells to fill the viewport (plus
    /// a small buffer) and recycles them as you scroll — so a list of 10 000 items costs the same
    /// as a list of 20. Fixed row height. Provide the item count and a bind callback; the control
    /// positions and rebinds the pooled cells to whatever indices are on screen.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UIVirtualListControl : MonoBehaviour
    {
        [Serializable]
        public sealed class BindEvent : UnityEvent<int, RectTransform>
        {
        }

        [Header("Targets")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [Tooltip("Prototype cell cloned into the recycle pool. Kept inactive.")]
        [SerializeField] private RectTransform cellTemplate;

        [Header("Layout")]
        [SerializeField] private float cellHeight = 64f;
        [SerializeField] private float spacing = 8f;
        [SerializeField] private float paddingTop = 8f;
        [SerializeField] private float paddingBottom = 8f;
        [Tooltip("Extra cells kept above/below the visible range to avoid pop-in while scrolling.")]
        [SerializeField] private int bufferCells = 2;

        [Header("Events")]
        [Tooltip("Invoked to bind a recycled cell to a data index. Set a code binder via SetItems for more control.")]
        [SerializeField] private BindEvent onBindCell = new BindEvent();

        private readonly List<RectTransform> pool = new List<RectTransform>();
        private int itemCount;
        private int lastFirstIndex = -1;
        private Action<int, RectTransform> binder;
        private bool started;

        public BindEvent OnBindCell => onBindCell;
        public int ItemCount => itemCount;
        public int ActiveCellCount => pool.Count;

        private float Step => cellHeight + spacing;

        private void Awake()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }

            if (viewport == null && scrollRect != null)
            {
                viewport = scrollRect.viewport;
            }

            if (content == null && scrollRect != null)
            {
                content = scrollRect.content;
            }

            if (cellTemplate != null)
            {
                cellTemplate.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.AddListener(OnScrolled);
            }
        }

        private void OnDisable()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrolled);
            }
        }

        private void Start()
        {
            started = true;
            Rebuild();
        }

        /// <summary>Set how many rows there are and how to bind a recycled cell to a data index.</summary>
        public void SetItems(int count, Action<int, RectTransform> bindCallback)
        {
            binder = bindCallback;
            SetItemCount(count);
        }

        public void SetItemCount(int count)
        {
            itemCount = Mathf.Max(0, count);
            if (started)
            {
                Rebuild();
            }
        }

        /// <summary>Scroll so that <paramref name="index"/> is at the top of the viewport.</summary>
        public void ScrollToIndex(int index)
        {
            if (content == null || itemCount == 0)
            {
                return;
            }

            var clamped = Mathf.Clamp(index, 0, itemCount - 1);
            var y = paddingTop + clamped * Step;
            var pos = content.anchoredPosition;
            pos.y = y;
            content.anchoredPosition = pos;
            RefreshVisible(true);
        }

        /// <summary>Re-run the bind callback for every on-screen cell (e.g. after the data changed).</summary>
        public void RefreshActiveCells()
        {
            RefreshVisible(true);
        }

        private void Rebuild()
        {
            ResizeContent();
            EnsurePool(RequiredPoolSize());
            RefreshVisible(true);
        }

        private void ResizeContent()
        {
            if (content == null)
            {
                return;
            }

            var total = paddingTop + paddingBottom + itemCount * cellHeight +
                        Mathf.Max(0, itemCount - 1) * spacing;
            var size = content.sizeDelta;
            size.y = Mathf.Max(0f, total);
            content.sizeDelta = size;
        }

        private int RequiredPoolSize()
        {
            var viewportHeight = viewport != null ? viewport.rect.height : 0f;
            if (viewportHeight <= 1f)
            {
                viewportHeight = 600f; // fallback before first layout
            }

            var visible = Mathf.CeilToInt(viewportHeight / Mathf.Max(1f, Step)) + 1;
            var needed = visible + Mathf.Max(0, bufferCells) * 2;
            return Mathf.Min(Mathf.Max(needed, 1), Mathf.Max(itemCount, 1));
        }

        private void EnsurePool(int size)
        {
            if (cellTemplate == null || content == null)
            {
                return;
            }

            while (pool.Count < size)
            {
                var cell = Instantiate(cellTemplate, content);
                cell.gameObject.SetActive(false);
                cell.anchorMin = new Vector2(0f, 1f);
                cell.anchorMax = new Vector2(1f, 1f);
                cell.pivot = new Vector2(0.5f, 1f);
                pool.Add(cell);
            }
        }

        private void OnScrolled(Vector2 _)
        {
            RefreshVisible(false);
        }

        private void RefreshVisible(bool force)
        {
            if (content == null || cellTemplate == null)
            {
                return;
            }

            EnsurePool(RequiredPoolSize());

            // content.anchoredPosition.y grows as you scroll down (top-anchored content).
            var scroll = Mathf.Max(0f, content.anchoredPosition.y);
            var firstIndex = Mathf.FloorToInt((scroll - paddingTop) / Step) - Mathf.Max(0, bufferCells);
            firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, itemCount - 1));

            if (!force && firstIndex == lastFirstIndex)
            {
                return;
            }

            lastFirstIndex = firstIndex;

            for (var i = 0; i < pool.Count; i++)
            {
                var cell = pool[i];
                var dataIndex = firstIndex + i;

                if (dataIndex >= itemCount)
                {
                    if (cell.gameObject.activeSelf)
                    {
                        cell.gameObject.SetActive(false);
                    }

                    continue;
                }

                if (!cell.gameObject.activeSelf)
                {
                    cell.gameObject.SetActive(true);
                }

                cell.offsetMin = new Vector2(0f, cell.offsetMin.y);
                cell.offsetMax = new Vector2(0f, cell.offsetMax.y);
                cell.sizeDelta = new Vector2(0f, cellHeight);
                cell.anchoredPosition = new Vector2(0f, -(paddingTop + dataIndex * Step));

                binder?.Invoke(dataIndex, cell);
                onBindCell?.Invoke(dataIndex, cell);
            }
        }
    }
}
