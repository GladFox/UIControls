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
    /// A group of chips that toggle their own selected state. Unlike
    /// <see cref="UITabSliderControl"/> / <see cref="UISegmentedControl"/> (one-of-N with a single
    /// sliding highlight), there is no moving indicator here: every chip carries its own
    /// background/label/checkmark state and animates independently.
    ///
    /// In <see cref="SelectionMode.Single"/> it behaves like a radio group (selecting one clears the
    /// rest); in <see cref="SelectionMode.Multi"/> each chip toggles freely, so any number can be on
    /// at once — something the slider-style controls can't express.
    /// </summary>
    public sealed class UIChipGroup : MonoBehaviour,
        IPointerClickHandler,
        ISubmitHandler,
        IMoveHandler,
        ISelectHandler,
        IDeselectHandler
    {
        public enum SelectionMode
        {
            Single,
            Multi,
        }

        [Serializable]
        public sealed class Chip
        {
            [SerializeField] private RectTransform root;
            [SerializeField] private Graphic background;
            [SerializeField] private TMP_Text label;
            [SerializeField] private GameObject checkmark;
            [SerializeField] private ChipToggledEvent onToggled = new ChipToggledEvent();

            public RectTransform Root => root;
            public Graphic Background => background;
            public TMP_Text Label => label;
            public GameObject Checkmark => checkmark;
            public ChipToggledEvent OnToggled => onToggled;
        }

        [Serializable]
        public sealed class ChipToggledEvent : UnityEvent<bool>
        {
        }

        [Serializable]
        public sealed class SelectionChangedEvent : UnityEvent<int>
        {
        }

        [SerializeField] private List<Chip> chips = new List<Chip>();

        [Header("Mode")]
        [SerializeField] private SelectionMode mode = SelectionMode.Single;
        [Tooltip("Single mode only: allow tapping the active chip to clear it (no chip selected). Off = classic radio that always keeps one selected.")]
        [SerializeField] private bool allowEmptyInSingle;

        [Header("Initial")]
        [Tooltip("Single mode: index selected on start (clamped). Multi mode: ignored — use the per-chip state set in the scene.")]
        [SerializeField] private int initialIndex;

        [Header("Colors")]
        [SerializeField] private Color normalBackgroundColor = new Color(0.18f, 0.22f, 0.31f, 1f);
        [SerializeField] private Color selectedBackgroundColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color normalLabelColor = new Color(0.82f, 0.86f, 0.95f, 1f);
        [SerializeField] private Color selectedLabelColor = Color.white;

        [Header("Animation")]
        [SerializeField] private UITweenSettings toggleTween = new UITweenSettings();
        [Tooltip("Pop the chip's scale when its state changes.")]
        [SerializeField] private bool popOnToggle = true;
        [SerializeField] private float popScale = 1.08f;

        [Header("Keyboard")]
        [Tooltip("Subtle scale applied to the keyboard-focused chip while the group is selected in the EventSystem.")]
        [SerializeField] private float focusScale = 1.05f;

        [Header("Interaction")]
        [SerializeField] private bool interactable = true;
        [SerializeField] private CanvasGroup canvasGroup;
        [Range(0f, 1f)]
        [SerializeField] private float disabledAlpha = 0.55f;

        [Header("Events")]
        [Tooltip("Fires with the chip index whose state changed (both on and off).")]
        [SerializeField] private SelectionChangedEvent onSelectionChanged = new SelectionChangedEvent();

        private readonly HashSet<int> selected = new HashSet<int>();
        private readonly List<Tween> activeTweens = new List<Tween>();
        private bool initialized;
        private int focusIndex = -1;
        private bool focused;

        public SelectionMode Mode => mode;
        public int ChipsCount => chips?.Count ?? 0;
        public SelectionChangedEvent OnSelectionChanged => onSelectionChanged;

        public bool Interactable
        {
            get => interactable;
            set => SetInteractable(value);
        }

        /// <summary>Selected index in single mode, or -1 if none. In multi mode returns the lowest selected index, or -1.</summary>
        public int SelectedIndex
        {
            get
            {
                var min = -1;
                foreach (var i in selected)
                {
                    if (min < 0 || i < min)
                    {
                        min = i;
                    }
                }

                return min;
            }
        }

        public IReadOnlyCollection<int> SelectedIndices => selected;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            ApplyInteractableState();
            InitializeSelection();
            RefreshAllVisuals(true);
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public bool IsSelected(int index)
        {
            return selected.Contains(index);
        }

        public void SetSelected(int index, bool value, bool animate = true, bool notify = true)
        {
            if (index < 0 || index >= ChipsCount)
            {
                return;
            }

            EnsureInitialized();

            if (value)
            {
                if (mode == SelectionMode.Single)
                {
                    SelectOnly(index, animate, notify);
                    return;
                }

                if (!selected.Add(index))
                {
                    return;
                }
            }
            else
            {
                if (mode == SelectionMode.Single && !allowEmptyInSingle && selected.Contains(index))
                {
                    // Radio: can't clear the only selected chip by deselecting it.
                    return;
                }

                if (!selected.Remove(index))
                {
                    return;
                }
            }

            RefreshChipVisual(index, !animate, pop: animate);

            if (notify)
            {
                chips[index]?.OnToggled?.Invoke(value);
                onSelectionChanged?.Invoke(index);
            }
        }

        public void Toggle(int index, bool animate = true, bool notify = true)
        {
            SetSelected(index, !IsSelected(index), animate, notify);
        }

        public void SelectOnly(int index, bool animate = true, bool notify = true)
        {
            if (index < 0 || index >= ChipsCount)
            {
                return;
            }

            EnsureInitialized();

            if (selected.Count == 1 && selected.Contains(index))
            {
                return;
            }

            var changed = new List<int>();
            foreach (var i in selected)
            {
                if (i != index)
                {
                    changed.Add(i);
                }
            }

            var wasSelected = selected.Contains(index);
            selected.Clear();
            selected.Add(index);
            if (!wasSelected)
            {
                changed.Add(index);
            }

            for (var i = 0; i < changed.Count; i++)
            {
                RefreshChipVisual(changed[i], !animate, pop: animate && changed[i] == index);
            }

            if (notify)
            {
                for (var i = 0; i < changed.Count; i++)
                {
                    var idx = changed[i];
                    chips[idx]?.OnToggled?.Invoke(selected.Contains(idx));
                    onSelectionChanged?.Invoke(idx);
                }
            }
        }

        public void ClearAll(bool animate = true, bool notify = true)
        {
            if (selected.Count == 0)
            {
                return;
            }

            var cleared = new List<int>(selected);
            selected.Clear();

            for (var i = 0; i < cleared.Count; i++)
            {
                RefreshChipVisual(cleared[i], !animate, pop: false);
            }

            if (notify)
            {
                for (var i = 0; i < cleared.Count; i++)
                {
                    chips[cleared[i]]?.OnToggled?.Invoke(false);
                    onSelectionChanged?.Invoke(cleared[i]);
                }
            }
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
                RefreshAllVisuals(true);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable || eventData == null || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            var index = ResolveChipAtScreenPoint(eventData.position, eventData.pressEventCamera);
            if (index >= 0)
            {
                focusIndex = index;
                Toggle(index);
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!interactable || focusIndex < 0)
            {
                return;
            }

            Toggle(focusIndex);
        }

        public void OnSelect(BaseEventData eventData)
        {
            focused = true;
            if (focusIndex < 0)
            {
                focusIndex = SelectedIndex < 0 ? 0 : SelectedIndex;
            }

            RefreshFocusVisuals();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            focused = false;
            RefreshFocusVisuals();
        }

        public void OnMove(AxisEventData eventData)
        {
            if (!interactable || eventData == null || ChipsCount == 0)
            {
                return;
            }

            var delta = 0;
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                case MoveDirection.Down:
                    delta = 1;
                    break;
                case MoveDirection.Left:
                case MoveDirection.Up:
                    delta = -1;
                    break;
            }

            if (delta == 0)
            {
                return;
            }

            var start = focusIndex < 0 ? (SelectedIndex < 0 ? 0 : SelectedIndex) : focusIndex;
            var next = Mathf.Clamp(start + delta, 0, ChipsCount - 1);
            if (next == focusIndex)
            {
                return;
            }

            focusIndex = next;

            // In single mode, moving focus also moves the selection (radio convention). In multi
            // mode focus just moves; Submit toggles. Either way refresh the focus highlight.
            if (mode == SelectionMode.Single)
            {
                SelectOnly(focusIndex);
            }
            else
            {
                RefreshFocusVisuals();
            }
        }

        private int ResolveChipAtScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            for (var i = 0; i < chips.Count; i++)
            {
                var root = chips[i]?.Root;
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

        private void InitializeSelection()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            if (mode == SelectionMode.Single && selected.Count == 0 && !allowEmptyInSingle && ChipsCount > 0)
            {
                selected.Add(Mathf.Clamp(initialIndex, 0, ChipsCount - 1));
            }
        }

        private void EnsureInitialized()
        {
            if (!initialized)
            {
                InitializeSelection();
            }
        }

        private void RefreshAllVisuals(bool instant)
        {
            for (var i = 0; i < chips.Count; i++)
            {
                RefreshChipVisual(i, instant, pop: false);
            }
        }

        private void RefreshChipVisual(int index, bool instant, bool pop)
        {
            if (index < 0 || index >= chips.Count)
            {
                return;
            }

            var chip = chips[index];
            if (chip == null)
            {
                return;
            }

            var on = selected.Contains(index);
            var targetBg = on ? selectedBackgroundColor : normalBackgroundColor;
            var targetLabel = on ? selectedLabelColor : normalLabelColor;

            if (chip.Checkmark != null && chip.Checkmark.activeSelf != on)
            {
                chip.Checkmark.SetActive(on);
            }

            var duration = toggleTween != null ? Mathf.Max(0f, toggleTween.Duration) : 0f;
            var targetScale = ComputeChipScale(index);

            if (instant || duration <= Mathf.Epsilon)
            {
                if (chip.Background != null)
                {
                    chip.Background.color = targetBg;
                }

                if (chip.Label != null)
                {
                    chip.Label.color = targetLabel;
                }

                if (chip.Root != null)
                {
                    chip.Root.localScale = Vector3.one * targetScale;
                }

                return;
            }

            var sequence = DOTween.Sequence();

            if (chip.Background != null)
            {
                sequence.Join(UIDOTweenUtility.TweenGraphicColor(chip.Background, targetBg, duration));
            }

            if (chip.Label != null)
            {
                sequence.Join(UIDOTweenUtility.TweenGraphicColor(chip.Label, targetLabel, duration));
            }

            if (chip.Root != null)
            {
                if (pop && popOnToggle)
                {
                    // Quick pop: overshoot to popScale, settle to the resting scale.
                    sequence.Join(chip.Root
                        .DOScale(Vector3.one * (targetScale * popScale), duration * 0.45f)
                        .SetEase(Ease.OutQuad));
                    sequence.Insert(duration * 0.45f, chip.Root
                        .DOScale(Vector3.one * targetScale, duration * 0.55f)
                        .SetEase(Ease.OutBack));
                }
                else
                {
                    sequence.Join(chip.Root.DOScale(Vector3.one * targetScale, duration));
                }
            }

            toggleTween.ApplyTimingOnly(sequence);
            TrackTween(sequence);
        }

        private void RefreshFocusVisuals()
        {
            for (var i = 0; i < chips.Count; i++)
            {
                RefreshChipVisual(i, true, pop: false);
            }
        }

        private float ComputeChipScale(int index)
        {
            return focused && index == focusIndex ? focusScale : 1f;
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

        private void TrackTween(Tween tween)
        {
            if (tween == null)
            {
                return;
            }

            activeTweens.Add(tween);
            tween.OnKill(() => activeTweens.Remove(tween));
        }

        private void KillTweens()
        {
            for (var i = activeTweens.Count - 1; i >= 0; i--)
            {
                var tween = activeTweens[i];
                if (tween != null && tween.IsActive())
                {
                    tween.Kill(false);
                }
            }

            activeTweens.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += SyncInEditor;
        }

        private void SyncInEditor()
        {
            UnityEditor.EditorApplication.delayCall -= SyncInEditor;
            if (this == null || Application.isPlaying || ChipsCount == 0)
            {
                return;
            }

            // Editor preview: in single mode show initialIndex as the selected chip; in multi mode
            // just paint everything in the normal state (designer toggles per chip at runtime).
            var previewSelected = mode == SelectionMode.Single && !allowEmptyInSingle
                ? Mathf.Clamp(initialIndex, 0, ChipsCount - 1)
                : -1;

            for (var i = 0; i < chips.Count; i++)
            {
                var chip = chips[i];
                if (chip == null)
                {
                    continue;
                }

                var on = i == previewSelected;
                if (chip.Background != null)
                {
                    chip.Background.color = on ? selectedBackgroundColor : normalBackgroundColor;
                }

                if (chip.Label != null)
                {
                    chip.Label.color = on ? selectedLabelColor : normalLabelColor;
                }

                if (chip.Checkmark != null && chip.Checkmark.activeSelf != on)
                {
                    chip.Checkmark.SetActive(on);
                }

                if (chip.Root != null)
                {
                    chip.Root.localScale = Vector3.one;
                }
            }
        }
#endif
    }
}
