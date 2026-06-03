using System;
using System.Collections.Generic;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A stack of collapsible sections. Each section has a clickable header and a content area
    /// whose height animates open/closed; a chevron rotates to match. In single-open mode it
    /// behaves like a classic accordion (opening one closes the others); in multi-open mode any
    /// number can be expanded at once.
    ///
    /// Sections are laid out manually top-to-bottom (no LayoutGroup), so heights animate smoothly
    /// and the sections below shift in lock-step. Content is expected to sit inside a masked
    /// viewport (e.g. a RectMask2D) so it clips cleanly while collapsing.
    /// </summary>
    public sealed class UIAccordionControl : MonoBehaviour
    {
        [Serializable]
        public sealed class Section
        {
            [SerializeField] private RectTransform root;
            [SerializeField] private UIButtonControl header;
            [SerializeField] private RectTransform chevron;
            [SerializeField] private RectTransform contentViewport;
            [SerializeField] private float headerHeight = 72f;
            [SerializeField] private float contentHeight = 200f;
            [SerializeField] private bool expanded;

            public RectTransform Root => root;
            public UIButtonControl Header => header;
            public RectTransform Chevron => chevron;
            public RectTransform ContentViewport => contentViewport;
            public float HeaderHeight => headerHeight;
            public float ContentHeight => contentHeight;
            public bool Expanded => expanded;
        }

        [Serializable]
        public sealed class SectionToggledEvent : UnityEvent<int, bool>
        {
        }

        [SerializeField] private List<Section> sections = new List<Section>();

        [Header("Layout")]
        [Tooltip("Container the sections are positioned within. Defaults to this object's RectTransform.")]
        [SerializeField] private RectTransform container;
        [SerializeField] private float spacing = 8f;

        [Header("Behaviour")]
        [Tooltip("Off = classic accordion (opening one closes the rest). On = any number open at once.")]
        [SerializeField] private bool allowMultipleOpen;

        [Header("Chevron")]
        [SerializeField] private float collapsedChevronZ = 0f;
        [SerializeField] private float expandedChevronZ = -90f;

        [Header("Animation")]
        [SerializeField] private UITweenSettings tween = new UITweenSettings();

        [Header("Events")]
        [SerializeField] private SectionToggledEvent onSectionToggled = new SectionToggledEvent();

        private float[] heights;
        private bool[] open;
        private Tweener[] tweens;
        private readonly List<UnityAction> headerHandlers = new List<UnityAction>();

        public SectionToggledEvent OnSectionToggled => onSectionToggled;
        public int SectionCount => sections?.Count ?? 0;

        private RectTransform Container => container != null ? container : (container = transform as RectTransform);

        public bool IsExpanded(int index)
        {
            return index >= 0 && open != null && index < open.Length && open[index];
        }

        private void Awake()
        {
            if (container == null)
            {
                container = transform as RectTransform;
            }

            EnsureState();
            BindHeaders();
        }

        private void OnEnable()
        {
            EnsureState();
            Relayout();
        }

        private void OnDisable()
        {
            KillTweens();
        }

        private void OnDestroy()
        {
            UnbindHeaders();
        }

        public void Toggle(int index)
        {
            if (index < 0 || index >= SectionCount)
            {
                return;
            }

            SetExpanded(index, !open[index]);
        }

        public void Expand(int index, bool animate = true, bool notify = true)
        {
            SetExpanded(index, true, animate, notify);
        }

        public void Collapse(int index, bool animate = true, bool notify = true)
        {
            SetExpanded(index, false, animate, notify);
        }

        public void SetExpanded(int index, bool value, bool animate = true, bool notify = true)
        {
            if (index < 0 || index >= SectionCount)
            {
                return;
            }

            EnsureState();

            if (open[index] == value)
            {
                return;
            }

            // Single-open mode: collapse the others first.
            if (value && !allowMultipleOpen)
            {
                for (var i = 0; i < SectionCount; i++)
                {
                    if (i != index && open[i])
                    {
                        ApplySection(i, false, animate, notify);
                    }
                }
            }

            ApplySection(index, value, animate, notify);
        }

        private void ApplySection(int index, bool value, bool animate, bool notify)
        {
            open[index] = value;
            var section = sections[index];
            var target = value ? Mathf.Max(0f, section.ContentHeight) : 0f;

            KillTween(index);

            var duration = tween != null ? Mathf.Max(0f, tween.Duration) : 0f;
            if (!animate || duration <= Mathf.Epsilon)
            {
                heights[index] = target;
                Relayout();
            }
            else
            {
                var captured = index;
                var tweener = DOTween.To(
                    () => heights[captured],
                    v => { heights[captured] = v; Relayout(); },
                    target,
                    duration);
                tween.Apply(tweener);
                tweens[index] = tweener;
            }

            if (notify)
            {
                onSectionToggled?.Invoke(index, value);
            }
        }

        private void Relayout()
        {
            if (Container == null || sections == null)
            {
                return;
            }

            var cursor = 0f;
            for (var i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                if (section?.Root == null)
                {
                    continue;
                }

                var contentH = heights != null && i < heights.Length ? heights[i] : 0f;
                var sectionH = section.HeaderHeight + contentH;

                var root = section.Root;
                root.anchorMin = new Vector2(0f, 1f);
                root.anchorMax = new Vector2(1f, 1f);
                root.pivot = new Vector2(0.5f, 1f);
                root.offsetMin = new Vector2(0f, root.offsetMin.y);
                root.anchoredPosition = new Vector2(0f, -cursor);
                var size = root.sizeDelta;
                size.y = sectionH;
                root.sizeDelta = size;

                if (section.ContentViewport != null)
                {
                    var vp = section.ContentViewport.sizeDelta;
                    vp.y = contentH;
                    section.ContentViewport.sizeDelta = vp;
                }

                if (section.Chevron != null)
                {
                    var ratio = section.ContentHeight > Mathf.Epsilon
                        ? Mathf.Clamp01(contentH / section.ContentHeight)
                        : (open != null && i < open.Length && open[i] ? 1f : 0f);
                    var z = Mathf.Lerp(collapsedChevronZ, expandedChevronZ, ratio);
                    section.Chevron.localEulerAngles = new Vector3(0f, 0f, z);
                }

                cursor += sectionH + spacing;
            }

            // Size the container to fit all sections (useful inside scroll views / fitters).
            var containerSize = Container.sizeDelta;
            containerSize.y = Mathf.Max(0f, cursor - spacing);
            Container.sizeDelta = containerSize;
        }

        private void EnsureState()
        {
            var count = SectionCount;
            if (heights == null || heights.Length != count)
            {
                heights = new float[count];
                open = new bool[count];
                tweens = new Tweener[count];
                for (var i = 0; i < count; i++)
                {
                    open[i] = sections[i] != null && sections[i].Expanded;
                    heights[i] = open[i] ? Mathf.Max(0f, sections[i].ContentHeight) : 0f;
                }
            }
        }

        private void BindHeaders()
        {
            UnbindHeaders();
            for (var i = 0; i < sections.Count; i++)
            {
                var header = sections[i]?.Header;
                if (header == null)
                {
                    headerHandlers.Add(null);
                    continue;
                }

                var captured = i;
                UnityAction handler = () => Toggle(captured);
                header.OnClick.AddListener(handler);
                headerHandlers.Add(handler);
            }
        }

        private void UnbindHeaders()
        {
            for (var i = 0; i < headerHandlers.Count && i < sections.Count; i++)
            {
                var handler = headerHandlers[i];
                if (handler != null && sections[i]?.Header != null)
                {
                    sections[i].Header.OnClick.RemoveListener(handler);
                }
            }

            headerHandlers.Clear();
        }

        private void KillTween(int index)
        {
            if (tweens != null && index >= 0 && index < tweens.Length)
            {
                var t = tweens[index];
                if (t != null && t.IsActive())
                {
                    t.Kill(false);
                }

                tweens[index] = null;
            }
        }

        private void KillTweens()
        {
            if (tweens == null)
            {
                return;
            }

            for (var i = 0; i < tweens.Length; i++)
            {
                KillTween(i);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += SyncInEditor;
        }

        private void SyncInEditor()
        {
            UnityEditor.EditorApplication.delayCall -= SyncInEditor;
            if (this == null || Application.isPlaying || SectionCount == 0)
            {
                return;
            }

            if (container == null)
            {
                container = transform as RectTransform;
            }

            heights = null;
            EnsureState();
            Relayout();
        }
#endif
    }
}
