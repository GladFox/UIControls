using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Barrel / wheel value picker (time, units, difficulty…). Option labels are stacked vertically;
    /// drag to spin and release to snap the nearest option to the centre line, where it is highlighted
    /// and scaled up while neighbours fade. Selection raises <see cref="OnValueChanged"/>. Put a
    /// RectMask2D on the viewport so only a few rows show.
    /// </summary>
    public sealed class UIWheelPickerControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [SerializeField] private RectTransform content;
        [SerializeField] private TMP_Text[] itemLabels;

        [Header("Layout")]
        [SerializeField] private float itemSpacing = 64f;
        [Tooltip("Scale of the centred item; neighbours interpolate down to minScale.")]
        [SerializeField] private float maxScale = 1.25f;
        [SerializeField] private float minScale = 0.7f;

        [Header("State")]
        [SerializeField] private int selectedIndex;
        [SerializeField] private float snapDuration = 0.22f;

        [Header("Events")]
        [SerializeField] private IntEvent onValueChanged = new IntEvent();

        private int count;
        private Tween snapTween;

        public IntEvent OnValueChanged => onValueChanged;
        public int SelectedIndex => selectedIndex;

        private void OnEnable()
        {
            count = itemLabels != null ? itemLabels.Length : 0;
            LayoutItems();
            SetContentY(selectedIndex * itemSpacing, false);
        }

        private void LayoutItems()
        {
            if (itemLabels == null)
            {
                return;
            }

            for (var i = 0; i < itemLabels.Length; i++)
            {
                if (itemLabels[i] == null) continue;
                var rt = itemLabels[i].rectTransform;
                rt.anchoredPosition = new Vector2(0f, -i * itemSpacing);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            snapTween?.Kill();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (content == null) return;
            var y = content.anchoredPosition.y + eventData.delta.y / GetScale(eventData);
            SetContentY(y, false);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (content == null || count == 0) return;
            var index = Mathf.Clamp(Mathf.RoundToInt(content.anchoredPosition.y / itemSpacing), 0, count - 1);
            SnapTo(index, true);
        }

        public void SnapTo(int index, bool notify)
        {
            index = Mathf.Clamp(index, 0, Mathf.Max(0, count - 1));
            var changed = index != selectedIndex;
            selectedIndex = index;

            snapTween?.Kill();
            var from = content.anchoredPosition.y;
            var to = index * itemSpacing;
            snapTween = DOTween.To(() => from, v => SetContentY(v, false), to, snapDuration).SetEase(Ease.OutCubic).SetUpdate(true);

            if (notify && changed)
            {
                onValueChanged.Invoke(selectedIndex);
            }
        }

        private void SetContentY(float y, bool _)
        {
            if (content == null) return;
            var maxY = (count - 1) * itemSpacing;
            y = Mathf.Clamp(y, 0f, Mathf.Max(0f, maxY));
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
            ApplyItemStyles(y);
        }

        private void ApplyItemStyles(float contentY)
        {
            if (itemLabels == null) return;
            for (var i = 0; i < itemLabels.Length; i++)
            {
                if (itemLabels[i] == null) continue;
                // Distance of item i from the centre line, in item units.
                var dist = Mathf.Abs((contentY - i * itemSpacing) / itemSpacing);
                var t = Mathf.Clamp01(dist);
                var scale = Mathf.Lerp(maxScale, minScale, t);
                itemLabels[i].rectTransform.localScale = Vector3.one * scale;
                var c = itemLabels[i].color;
                c.a = Mathf.Lerp(1f, 0.35f, t);
                itemLabels[i].color = c;
            }
        }

        private static float GetScale(PointerEventData eventData)
        {
            var canvas = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponentInParent<Canvas>() : null;
            return canvas != null ? canvas.scaleFactor : 1f;
        }
    }
}
