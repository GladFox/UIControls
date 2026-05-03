using System;
using System.Collections.Generic;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    public sealed class UITabSliderControl : MonoBehaviour
    {
        [Serializable]
        public sealed class Tab
        {
            [SerializeField] private UIButtonControl button;
            [SerializeField] private GameObject view;
            [SerializeField] private UnityEvent onActivated = new UnityEvent();
            [SerializeField] private UnityEvent onDeactivated = new UnityEvent();

            public UIButtonControl Button => button;
            public GameObject View => view;
            public UnityEvent OnActivated => onActivated;
            public UnityEvent OnDeactivated => onDeactivated;
        }

        [Serializable]
        public sealed class TabChangedEvent : UnityEvent<int>
        {
        }

        [SerializeField] private List<Tab> tabs = new List<Tab>();

        [Header("Highlight")]
        [SerializeField] private RectTransform highlight;
        [SerializeField] private bool matchHighlightSize = true;

        [Header("Initial")]
        [SerializeField] private int initialIndex;
        [SerializeField] private bool autoToggleViews = true;

        [Header("Animation")]
        [SerializeField] private UITweenSettings slideTween = new UITweenSettings();

        [Header("Events")]
        [SerializeField] private TabChangedEvent onTabChanged = new TabChangedEvent();

        [Header("Custom Actions")]
        [SerializeField] private UITabSliderCustomAction[] customActions = Array.Empty<UITabSliderCustomAction>();

        private int selectedIndex = -1;
        private Tween positionTween;
        private Tween sizeTween;
        private readonly List<UnityAction> tabClickHandlers = new List<UnityAction>();

        public int SelectedIndex => selectedIndex;
        public int TabsCount => tabs?.Count ?? 0;
        public TabChangedEvent OnTabChanged => onTabChanged;

        private void Awake()
        {
            BindButtons();
        }

        private void OnEnable()
        {
            if (TabsCount == 0)
            {
                return;
            }

            if (selectedIndex < 0)
            {
                var startIndex = Mathf.Clamp(initialIndex, 0, TabsCount - 1);
                ApplyTab(startIndex, instant: true, notify: false);
            }
            else
            {
                AnimateHighlight(true);
            }
        }

        private void OnDisable()
        {
            KillTweens();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += SyncHighlightInEditor;
        }

        private void SyncHighlightInEditor()
        {
            UnityEditor.EditorApplication.delayCall -= SyncHighlightInEditor;
            if (this == null || Application.isPlaying)
            {
                return;
            }

            if (highlight == null || TabsCount == 0)
            {
                return;
            }

            var index = Mathf.Clamp(initialIndex, 0, TabsCount - 1);
            var tab = tabs[index];
            if (tab?.Button == null)
            {
                return;
            }

            var targetRect = tab.Button.transform as RectTransform;
            if (targetRect == null)
            {
                return;
            }

            if (highlight.parent == targetRect.parent)
            {
                highlight.anchoredPosition = targetRect.anchoredPosition;
            }
            else
            {
                highlight.position = targetRect.position;
            }

            if (matchHighlightSize)
            {
                highlight.sizeDelta = targetRect.rect.size;
            }
        }
#endif

        public void SelectTab(int index, bool animate = true, bool notify = true)
        {
            if (TabsCount == 0)
            {
                return;
            }

            var clamped = Mathf.Clamp(index, 0, TabsCount - 1);
            if (clamped == selectedIndex)
            {
                return;
            }

            ApplyTab(clamped, instant: !animate, notify: notify);
        }

        public void ForceSyncVisual()
        {
            if (selectedIndex < 0 || TabsCount == 0)
            {
                return;
            }

            UpdateViews(selectedIndex);
            AnimateHighlight(true);
        }

        private void ApplyTab(int index, bool instant, bool notify)
        {
            var previous = selectedIndex;
            selectedIndex = index;

            UpdateViews(previous);
            AnimateHighlight(instant);

            var tab = tabs[selectedIndex];
            tab?.OnActivated?.Invoke();

            if (notify)
            {
                onTabChanged?.Invoke(selectedIndex);
                InvokeCustomActions(action => action.OnTabChanged(this, previous, selectedIndex));
            }
        }

        private void UpdateViews(int previousIndex)
        {
            if (!autoToggleViews)
            {
                return;
            }

            for (var i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                if (tab?.View == null)
                {
                    continue;
                }

                var shouldBeActive = i == selectedIndex;
                if (tab.View.activeSelf != shouldBeActive)
                {
                    tab.View.SetActive(shouldBeActive);
                }
            }

            if (previousIndex >= 0 && previousIndex < tabs.Count && previousIndex != selectedIndex)
            {
                tabs[previousIndex]?.OnDeactivated?.Invoke();
            }
        }

        private void AnimateHighlight(bool instant)
        {
            if (highlight == null)
            {
                return;
            }

            if (selectedIndex < 0 || selectedIndex >= tabs.Count)
            {
                return;
            }

            var tab = tabs[selectedIndex];
            if (tab?.Button == null)
            {
                return;
            }

            var targetRect = tab.Button.transform as RectTransform;
            if (targetRect == null)
            {
                return;
            }

            KillTweens();

            var sameParent = highlight.parent == targetRect.parent;
            var targetSize = targetRect.rect.size;
            var duration = slideTween != null ? Mathf.Max(0f, slideTween.Duration) : 0f;

            if (instant || duration <= Mathf.Epsilon)
            {
                if (sameParent)
                {
                    highlight.anchoredPosition = targetRect.anchoredPosition;
                }
                else
                {
                    highlight.position = targetRect.position;
                }

                if (matchHighlightSize)
                {
                    highlight.sizeDelta = targetSize;
                }

                return;
            }

            if (sameParent)
            {
                positionTween = highlight.DOAnchorPos(targetRect.anchoredPosition, duration);
            }
            else
            {
                positionTween = highlight.DOMove(targetRect.position, duration);
            }

            slideTween.Apply(positionTween);

            if (matchHighlightSize)
            {
                sizeTween = highlight.DOSizeDelta(targetSize, duration);
                slideTween.Apply(sizeTween);
            }
        }

        private void BindButtons()
        {
            UnbindButtons();

            for (var i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                if (tab?.Button == null)
                {
                    tabClickHandlers.Add(null);
                    continue;
                }

                var capturedIndex = i;
                UnityAction handler = () => SelectTab(capturedIndex);
                tab.Button.OnClick.AddListener(handler);
                tabClickHandlers.Add(handler);
            }
        }

        private void UnbindButtons()
        {
            for (var i = 0; i < tabClickHandlers.Count && i < tabs.Count; i++)
            {
                var handler = tabClickHandlers[i];
                if (handler == null)
                {
                    continue;
                }

                var tab = tabs[i];
                if (tab?.Button == null)
                {
                    continue;
                }

                tab.Button.OnClick.RemoveListener(handler);
            }

            tabClickHandlers.Clear();
        }

        private void KillTweens()
        {
            if (positionTween != null && positionTween.IsActive())
            {
                positionTween.Kill(false);
            }

            if (sizeTween != null && sizeTween.IsActive())
            {
                sizeTween.Kill(false);
            }

            positionTween = null;
            sizeTween = null;
        }

        private void InvokeCustomActions(Action<UITabSliderCustomAction> invocation)
        {
            if (customActions == null || invocation == null)
            {
                return;
            }

            for (var i = 0; i < customActions.Length; i++)
            {
                var action = customActions[i];
                if (action == null)
                {
                    continue;
                }

                try
                {
                    invocation(action);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }
    }
}
