using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Numeric pager: ‹ 1 2 … 9 › with a fixed pool of slots. Slots are relabelled to page numbers or
    /// an ellipsis as the current page moves, the active page is tinted, and prev/next step by one.
    /// Raises <see cref="OnPageChanged"/>. Wire <see cref="slotButtons"/> in order; the control decides
    /// which page (if any) each slot represents.
    /// </summary>
    public sealed class UIPaginationControl : MonoBehaviour
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Slots (page-number buttons)")]
        [SerializeField] private UIButtonControl[] slotButtons;
        [SerializeField] private TMP_Text[] slotLabels;
        [Tooltip("Graphic per slot tinted to mark the current page.")]
        [SerializeField] private Graphic[] slotBackgrounds;

        [Header("Navigation")]
        [SerializeField] private UIButtonControl prevButton;
        [SerializeField] private UIButtonControl nextButton;

        [Header("State")]
        [SerializeField] private int pageCount = 9;
        [SerializeField] private int currentPage = 1;

        [Header("Style")]
        [SerializeField] private Color normalColor = new Color(0.18f, 0.24f, 0.36f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.24f, 0.55f, 0.95f, 1f);

        [Header("Events")]
        [SerializeField] private IntEvent onPageChanged = new IntEvent();

        private int[] slotPages;

        public IntEvent OnPageChanged => onPageChanged;
        public int CurrentPage => currentPage;
        public int PageCount => pageCount;

        private void Awake()
        {
            if (slotButtons != null)
            {
                slotPages = new int[slotButtons.Length];
                for (var i = 0; i < slotButtons.Length; i++)
                {
                    var slot = i;
                    if (slotButtons[i] != null)
                    {
                        slotButtons[i].OnClick.AddListener(() => OnSlotClicked(slot));
                    }
                }
            }

            if (prevButton != null) prevButton.OnClick.AddListener(Previous);
            if (nextButton != null) nextButton.OnClick.AddListener(Next);
        }

        private void OnEnable()
        {
            Render();
        }

        public void SetPageCount(int count)
        {
            pageCount = Mathf.Max(1, count);
            currentPage = Mathf.Clamp(currentPage, 1, pageCount);
            Render();
        }

        public void GoTo(int page, bool notify = true)
        {
            var clamped = Mathf.Clamp(page, 1, pageCount);
            if (clamped == currentPage)
            {
                return;
            }

            currentPage = clamped;
            Render();
            if (notify)
            {
                onPageChanged.Invoke(currentPage);
            }
        }

        public void Next() => GoTo(currentPage + 1);

        public void Previous() => GoTo(currentPage - 1);

        private void OnSlotClicked(int slot)
        {
            if (slotPages != null && slot < slotPages.Length && slotPages[slot] > 0)
            {
                GoTo(slotPages[slot]);
            }
        }

        private void Render()
        {
            if (slotButtons == null)
            {
                return;
            }

            var tokens = BuildTokens(slotButtons.Length);

            for (var i = 0; i < slotButtons.Length; i++)
            {
                var hasToken = i < tokens.Count;
                var page = hasToken ? tokens[i] : 0;
                slotPages[i] = page > 0 ? page : 0;

                if (slotButtons[i] != null)
                {
                    slotButtons[i].gameObject.SetActive(hasToken);
                }

                if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
                {
                    slotLabels[i].text = page > 0 ? page.ToString() : (hasToken ? "…" : string.Empty);
                }

                if (slotBackgrounds != null && i < slotBackgrounds.Length && slotBackgrounds[i] != null)
                {
                    slotBackgrounds[i].color = page == currentPage ? selectedColor : normalColor;
                }
            }

            if (prevButton != null) prevButton.gameObject.SetActive(currentPage > 1);
            if (nextButton != null) nextButton.gameObject.SetActive(currentPage < pageCount);
        }

        // Returns a list of page numbers (>0) and ellipses (0) that fits within <paramref name="slots"/>.
        private System.Collections.Generic.List<int> BuildTokens(int slots)
        {
            var list = new System.Collections.Generic.List<int>();
            if (pageCount <= slots)
            {
                for (var p = 1; p <= pageCount; p++) list.Add(p);
                return list;
            }

            // First, last, current and a neighbour window; ellipses fill the gaps.
            var pages = new System.Collections.Generic.SortedSet<int> { 1, pageCount, currentPage };
            if (currentPage - 1 > 1) pages.Add(currentPage - 1);
            if (currentPage + 1 < pageCount) pages.Add(currentPage + 1);

            var ordered = new System.Collections.Generic.List<int>(pages);
            var prev = 0;
            foreach (var p in ordered)
            {
                if (prev != 0 && p - prev > 1)
                {
                    list.Add(0); // ellipsis
                }

                list.Add(p);
                prev = p;
            }

            return list;
        }
    }
}
