using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// iOS-style segmented control: a rounded container with N equal-weight segments and a
    /// sliding "thumb" that highlights the active segment. Unlike <see cref="UITabSliderControl"/>
    /// (a generic tab manager wired to external buttons/views), this control owns its segments
    /// directly, hit-tests pointer clicks itself, recolors the selected label, and insets the
    /// thumb with padding like a native segmented control.
    ///
    /// The thumb slides via <see cref="UITweenSettings"/>. With <see cref="rubberBand"/> on, the
    /// leading edge (in the direction of motion) starts first and the trailing edge catches up
    /// after a lag — driven through <see cref="Transform.localScale"/> so anything inside the
    /// thumb stretches with it. Segmented controls are horizontal, so the rubber-band acts on X.
    /// </summary>
    public sealed class UISegmentedControl : MonoBehaviour,
        IPointerClickHandler,
        ISubmitHandler,
        IMoveHandler
    {
        [Serializable]
        public sealed class Segment
        {
            [SerializeField] private RectTransform root;
            [SerializeField] private TMP_Text label;
            [SerializeField] private Graphic icon;
            [SerializeField] private UnityEvent onSelected = new UnityEvent();

            public RectTransform Root => root;
            public TMP_Text Label => label;
            public Graphic Icon => icon;
            public UnityEvent OnSelected => onSelected;
        }

        [Serializable]
        public sealed class SegmentChangedEvent : UnityEvent<int>
        {
        }

        [SerializeField] private List<Segment> segments = new List<Segment>();

        [Header("Thumb")]
        [Tooltip("RectTransform that slides under the selected segment. Should share a parent with the segment roots so anchored-position math lines up.")]
        [SerializeField] private RectTransform thumb;
        [Tooltip("Inset of the thumb relative to the segment bounds, in pixels per side (iOS leaves a small gap around the highlight).")]
        [SerializeField] private Vector2 thumbPadding = new Vector2(4f, 4f);

        [Header("Label Colors")]
        [SerializeField] private Color normalLabelColor = new Color(0.82f, 0.86f, 0.95f, 1f);
        [SerializeField] private Color selectedLabelColor = new Color(0.07f, 0.1f, 0.16f, 1f);
        [Tooltip("Tint applied to a segment's icon graphic. Mirrors the label colors.")]
        [SerializeField] private bool tintIcons = true;

        [Header("Initial")]
        [SerializeField] private int initialIndex;

        [Header("Animation")]
        [SerializeField] private UITweenSettings slideTween = new UITweenSettings();

        [Tooltip("Rubber-band slide: the leading edge starts first; the trailing edge catches up after a lag, so the thumb stretches mid-flight, then settles. Driven via localScale, so thumb contents stretch too. Requires the thumb to share a parent with the segments.")]
        [SerializeField] private bool rubberBand;
        [Range(0f, 0.9f)]
        [Tooltip("How long the trailing edge waits before catching up, as a fraction of the slide duration.")]
        [SerializeField] private float rubberBandLag = 0.35f;
        [SerializeField] private Ease rubberBandLeadEase = Ease.OutCubic;
        [SerializeField] private Ease rubberBandTrailEase = Ease.OutCubic;

        [Header("Interaction")]
        [SerializeField] private bool interactable = true;
        [SerializeField] private CanvasGroup canvasGroup;
        [Range(0f, 1f)]
        [SerializeField] private float disabledAlpha = 0.55f;

        [Header("Events")]
        [SerializeField] private SegmentChangedEvent onSelectionChanged = new SegmentChangedEvent();

        private int selectedIndex = -1;
        private Tween positionTween;
        private Tween sizeTween;
        private Sequence rubberBandSequence;
        private Sequence colorSequence;
        private float baseThumbWidth = 1f;
        private float baseThumbHeight = 1f;
        private float rubberMinX;
        private float rubberMaxX;

        public int SelectedIndex => selectedIndex;
        public int SegmentsCount => segments?.Count ?? 0;
        public SegmentChangedEvent OnSelectionChanged => onSelectionChanged;

        public bool Interactable
        {
            get => interactable;
            set => SetInteractable(value);
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            CaptureBaseThumbSize();
        }

        private void OnEnable()
        {
            ApplyInteractableState();

            if (SegmentsCount == 0)
            {
                return;
            }

            if (selectedIndex < 0)
            {
                ApplySelection(Mathf.Clamp(initialIndex, 0, SegmentsCount - 1), instant: true, notify: false);
            }
            else
            {
                ApplyLabelColors(true);
                MoveThumb(true);
            }
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public void Select(int index, bool animate = true, bool notify = true)
        {
            if (SegmentsCount == 0)
            {
                return;
            }

            var clamped = Mathf.Clamp(index, 0, SegmentsCount - 1);
            if (clamped == selectedIndex)
            {
                return;
            }

            ApplySelection(clamped, instant: !animate, notify: notify);
        }

        public void SelectNext(bool animate = true, bool notify = true)
        {
            if (SegmentsCount == 0)
            {
                return;
            }

            Select(Mathf.Min(selectedIndex + 1, SegmentsCount - 1), animate, notify);
        }

        public void SelectPrevious(bool animate = true, bool notify = true)
        {
            if (SegmentsCount == 0)
            {
                return;
            }

            Select(Mathf.Max(selectedIndex - 1, 0), animate, notify);
        }

        public void SetInteractable(bool value, bool instant = false)
        {
            if (interactable == value)
            {
                return;
            }

            interactable = value;
            ApplyInteractableState();

            if (instant)
            {
                MoveThumb(true);
                ApplyLabelColors(true);
            }
        }

        public void ForceSyncVisual()
        {
            if (selectedIndex < 0 || SegmentsCount == 0)
            {
                return;
            }

            ApplyLabelColors(true);
            MoveThumb(true);
        }

        public void RefreshThumbBaseSize()
        {
            CaptureBaseThumbSize();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable || eventData == null || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            var index = ResolveSegmentAtScreenPoint(eventData.position, eventData.pressEventCamera);
            if (index >= 0)
            {
                Select(index);
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            // Submit re-fires the active segment so listeners can treat it like a confirm.
            if (selectedIndex >= 0 && selectedIndex < segments.Count)
            {
                segments[selectedIndex]?.OnSelected?.Invoke();
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if (!interactable || eventData == null)
            {
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                    SelectNext();
                    break;
                case MoveDirection.Left:
                    SelectPrevious();
                    break;
            }
        }

        private int ResolveSegmentAtScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            for (var i = 0; i < segments.Count; i++)
            {
                var root = segments[i]?.Root;
                if (root == null)
                {
                    continue;
                }

                if (RectTransformUtility.RectangleContainsScreenPoint(root, screenPoint, eventCamera))
                {
                    return i;
                }
            }

            return -1;
        }

        private void ApplySelection(int index, bool instant, bool notify)
        {
            var previous = selectedIndex;
            selectedIndex = index;

            ApplyLabelColors(instant);
            MoveThumb(instant);

            segments[selectedIndex]?.OnSelected?.Invoke();

            if (notify && previous != selectedIndex)
            {
                onSelectionChanged?.Invoke(selectedIndex);
            }
        }

        private void ApplyInteractableState()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = interactable ? 1f : disabledAlpha;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        private void GetSegmentTarget(int index, out Vector2 center, out Vector2 size, out bool sameParent)
        {
            center = Vector2.zero;
            size = Vector2.one;
            sameParent = false;

            var root = segments[index]?.Root;
            if (root == null)
            {
                return;
            }

            sameParent = thumb != null && thumb.parent == root.parent;
            center = root.anchoredPosition;
            var segSize = root.rect.size;
            size = new Vector2(
                Mathf.Max(1f, segSize.x - thumbPadding.x * 2f),
                Mathf.Max(1f, segSize.y - thumbPadding.y * 2f));
        }

        private void MoveThumb(bool instant)
        {
            if (thumb == null || selectedIndex < 0 || selectedIndex >= segments.Count)
            {
                return;
            }

            KillThumbTweens();

            GetSegmentTarget(selectedIndex, out var targetCenter, out var targetSize, out var sameParent);
            var duration = slideTween != null ? Mathf.Max(0f, slideTween.Duration) : 0f;

            var useRubberBand = rubberBand
                && sameParent
                && baseThumbWidth > Mathf.Epsilon
                && baseThumbHeight > Mathf.Epsilon;

            if (instant || duration <= Mathf.Epsilon || !sameParent)
            {
                if (sameParent)
                {
                    thumb.anchoredPosition = targetCenter;
                }

                if (useRubberBand)
                {
                    thumb.sizeDelta = new Vector2(baseThumbWidth, baseThumbHeight);
                    var scale = thumb.localScale;
                    scale.x = targetSize.x / baseThumbWidth;
                    scale.y = targetSize.y / baseThumbHeight;
                    thumb.localScale = scale;
                }
                else
                {
                    thumb.localScale = Vector3.one;
                    thumb.sizeDelta = targetSize;
                }

                return;
            }

            if (useRubberBand)
            {
                StartRubberBandSequence(targetCenter, targetSize, duration);
                return;
            }

            positionTween = UIDOTweenUtility.TweenAnchoredPosition(thumb, targetCenter, duration);
            slideTween.Apply(positionTween);

            sizeTween = UIDOTweenUtility.TweenSizeDelta(thumb, targetSize, duration);
            slideTween.Apply(sizeTween);
        }

        private void StartRubberBandSequence(Vector2 toCenter, Vector2 toSize, float duration)
        {
            if (thumb.sizeDelta.x != baseThumbWidth || thumb.sizeDelta.y != baseThumbHeight)
            {
                thumb.sizeDelta = new Vector2(baseThumbWidth, baseThumbHeight);
            }

            // Y settles immediately to the target height — only X gets the rubber stretch.
            var fromCenter = thumb.anchoredPosition;
            var currentScale = thumb.localScale;
            var fromWidth = baseThumbWidth * currentScale.x;

            rubberMinX = fromCenter.x - fromWidth * 0.5f;
            rubberMaxX = fromCenter.x + fromWidth * 0.5f;

            var toMinX = toCenter.x - toSize.x * 0.5f;
            var toMaxX = toCenter.x + toSize.x * 0.5f;

            var heightScale = toSize.y / baseThumbHeight;
            var yScale = thumb.localScale;
            yScale.y = heightScale;
            thumb.localScale = yScale;

            var yPos = thumb.anchoredPosition;
            yPos.y = toCenter.y;
            thumb.anchoredPosition = yPos;

            var lagSeconds = duration * Mathf.Clamp(rubberBandLag, 0f, 0.9f);
            var movesPositive = toCenter.x >= fromCenter.x;

            var sequence = DOTween.Sequence();

            sequence.Insert(movesPositive ? 0f : lagSeconds,
                DOTween.To(() => rubberMaxX, v => { rubberMaxX = v; ApplyRubberEdges(); }, toMaxX, duration)
                    .SetEase(movesPositive ? rubberBandLeadEase : rubberBandTrailEase));
            sequence.Insert(movesPositive ? lagSeconds : 0f,
                DOTween.To(() => rubberMinX, v => { rubberMinX = v; ApplyRubberEdges(); }, toMinX, duration)
                    .SetEase(movesPositive ? rubberBandTrailEase : rubberBandLeadEase));

            slideTween.ApplyTimingOnly(sequence);
            rubberBandSequence = sequence;
        }

        private void ApplyRubberEdges()
        {
            if (thumb == null)
            {
                return;
            }

            var centerX = (rubberMinX + rubberMaxX) * 0.5f;
            var width = Mathf.Max(0f, rubberMaxX - rubberMinX);

            var pos = thumb.anchoredPosition;
            pos.x = centerX;
            thumb.anchoredPosition = pos;

            if (baseThumbWidth > Mathf.Epsilon)
            {
                var scale = thumb.localScale;
                scale.x = width / baseThumbWidth;
                thumb.localScale = scale;
            }
        }

        private void ApplyLabelColors(bool instant)
        {
            KillColorTween();

            var duration = slideTween != null ? Mathf.Max(0f, slideTween.Duration) : 0f;
            Sequence sequence = null;

            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                var target = i == selectedIndex ? selectedLabelColor : normalLabelColor;

                if (instant || duration <= Mathf.Epsilon)
                {
                    if (segment.Label != null)
                    {
                        segment.Label.color = target;
                    }

                    if (tintIcons && segment.Icon != null)
                    {
                        segment.Icon.color = target;
                    }

                    continue;
                }

                sequence ??= DOTween.Sequence();

                if (segment.Label != null)
                {
                    sequence.Join(UIDOTweenUtility.TweenGraphicColor(segment.Label, target, duration));
                }

                if (tintIcons && segment.Icon != null)
                {
                    sequence.Join(UIDOTweenUtility.TweenGraphicColor(segment.Icon, target, duration));
                }
            }

            if (sequence != null)
            {
                slideTween.ApplyTimingOnly(sequence);
                colorSequence = sequence;
            }
        }

        private void CaptureBaseThumbSize()
        {
            if (thumb == null)
            {
                baseThumbWidth = 1f;
                baseThumbHeight = 1f;
                return;
            }

            var size = thumb.rect.size;
            baseThumbWidth = size.x > Mathf.Epsilon ? size.x : 1f;
            baseThumbHeight = size.y > Mathf.Epsilon ? size.y : 1f;
        }

        private void KillThumbTweens()
        {
            if (positionTween != null && positionTween.IsActive())
            {
                positionTween.Kill(false);
            }

            if (sizeTween != null && sizeTween.IsActive())
            {
                sizeTween.Kill(false);
            }

            if (rubberBandSequence != null && rubberBandSequence.IsActive())
            {
                rubberBandSequence.Kill(false);
            }

            positionTween = null;
            sizeTween = null;
            rubberBandSequence = null;
        }

        private void KillColorTween()
        {
            if (colorSequence != null && colorSequence.IsActive())
            {
                colorSequence.Kill(false);
            }

            colorSequence = null;
        }

        private void KillTweens()
        {
            KillThumbTweens();
            KillColorTween();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += SyncInEditor;
        }

        private void SyncInEditor()
        {
            UnityEditor.EditorApplication.delayCall -= SyncInEditor;
            if (this == null || Application.isPlaying || SegmentsCount == 0)
            {
                return;
            }

            var index = Mathf.Clamp(initialIndex, 0, SegmentsCount - 1);

            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                var target = i == index ? selectedLabelColor : normalLabelColor;
                if (segment.Label != null)
                {
                    segment.Label.color = target;
                }

                if (tintIcons && segment.Icon != null)
                {
                    segment.Icon.color = target;
                }
            }

            if (thumb == null)
            {
                return;
            }

            var root = segments[index]?.Root;
            if (root == null)
            {
                return;
            }

            if (thumb.parent == root.parent)
            {
                thumb.anchoredPosition = root.anchoredPosition;
                var segSize = root.rect.size;
                thumb.localScale = Vector3.one;
                thumb.sizeDelta = new Vector2(
                    Mathf.Max(1f, segSize.x - thumbPadding.x * 2f),
                    Mathf.Max(1f, segSize.y - thumbPadding.y * 2f));
            }
        }
#endif
    }
}
