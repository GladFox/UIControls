using System;
using DG.Tweening;
using UIControls.Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// App-style bottom navigation bar. Each tab is a <see cref="UIButtonControl"/>; selecting one
    /// tints its graphic to the selected colour, slides an optional indicator under it, shows the
    /// matching page (if provided) and raises <see cref="OnTabChanged"/>. Distinct from the sliding
    /// <c>UITabSliderControl</c> — this is the persistent navigation bar of an app.
    /// </summary>
    public sealed class UITabBarControl : MonoBehaviour
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Tabs")]
        [SerializeField] private RectTransform[] tabs;
        [SerializeField] private UIButtonControl[] tabButtons;
        [Tooltip("Graphics tinted between normal/selected (e.g. icon + label per tab).")]
        [SerializeField] private Graphic[] tabGraphics;

        [Header("Indicator (optional)")]
        [SerializeField] private RectTransform indicator;

        [Header("Pages (optional, parallel to tabs)")]
        [SerializeField] private GameObject[] pages;

        [Header("Style")]
        [SerializeField] private Color normalColor = new Color(0.6f, 0.66f, 0.78f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        [SerializeField] private int selectedIndex;
        [SerializeField] private float indicatorDuration = 0.25f;

        [Header("Events")]
        [SerializeField] private IntEvent onTabChanged = new IntEvent();

        public IntEvent OnTabChanged => onTabChanged;
        public int SelectedIndex => selectedIndex;

        private void Awake()
        {
            if (tabButtons != null)
            {
                for (var i = 0; i < tabButtons.Length; i++)
                {
                    var index = i;
                    if (tabButtons[i] != null)
                    {
                        tabButtons[i].OnClick.AddListener(() => Select(index));
                    }
                }
            }
        }

        private void OnEnable()
        {
            Apply(selectedIndex, false, false);
        }

        public void Select(int index)
        {
            Apply(index, true, true);
        }

        private void Apply(int index, bool animate, bool notify)
        {
            var count = tabButtons != null ? tabButtons.Length : 0;
            if (count == 0)
            {
                return;
            }

            selectedIndex = Mathf.Clamp(index, 0, count - 1);

            if (tabGraphics != null)
            {
                for (var i = 0; i < tabGraphics.Length; i++)
                {
                    if (tabGraphics[i] != null)
                    {
                        tabGraphics[i].color = i == selectedIndex ? selectedColor : normalColor;
                    }
                }
            }

            if (pages != null)
            {
                for (var i = 0; i < pages.Length; i++)
                {
                    if (pages[i] != null)
                    {
                        pages[i].SetActive(i == selectedIndex);
                    }
                }
            }

            MoveIndicator(animate);

            if (notify)
            {
                onTabChanged.Invoke(selectedIndex);
            }
        }

        private void MoveIndicator(bool animate)
        {
            if (indicator == null || tabs == null || selectedIndex >= tabs.Length || tabs[selectedIndex] == null)
            {
                return;
            }

            var target = new Vector2(tabs[selectedIndex].anchoredPosition.x, indicator.anchoredPosition.y);
            if (animate)
            {
                UIDOTweenUtility.TweenAnchoredPosition(indicator, target, indicatorDuration).SetEase(Ease.OutCubic).SetUpdate(true);
            }
            else
            {
                indicator.anchoredPosition = target;
            }
        }
    }
}
