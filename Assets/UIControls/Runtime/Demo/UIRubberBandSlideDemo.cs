using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Demo
{
    /// <summary>
    /// Prototype of a rubber-band slide animation. The leading edge of the moving rect
    /// (the one in the direction of motion) starts first; the trailing edge catches up
    /// after a short lag. The whole thing stretches mid-flight, then settles.
    ///
    /// Implementation note: we drive <see cref="Transform.localScale"/> instead of
    /// <see cref="RectTransform.sizeDelta"/>. That way every vertex inside — image,
    /// text, child UI — stretches uniformly with the indicator. No custom shader, no
    /// vertex hacks, no per-element bookkeeping.
    /// </summary>
    public sealed class UIRubberBandSlideDemo : MonoBehaviour
    {
        [Header("Rig")]
        [SerializeField] private RectTransform rubberRect;
        [SerializeField] private List<RectTransform> stations = new List<RectTransform>();
        [SerializeField] private List<Button> stationButtons = new List<Button>();

        [Header("Animation")]
        [Min(0.05f)]
        [SerializeField] private float duration = 0.55f;
        [Range(0f, 0.9f)]
        [SerializeField] private float lag = 0.4f;
        [SerializeField] private Ease leadEase = Ease.OutCubic;
        [SerializeField] private Ease trailEase = Ease.OutCubic;
        [SerializeField] private int initialIndex;

        [Header("UI")]
        [SerializeField] private TMP_Text statusLabel;

        private float baseWidth;
        private float currentLeftX;
        private float currentRightX;
        private Sequence activeSequence;
        private int selectedIndex = -1;

        private void Awake()
        {
            if (rubberRect == null || stations == null || stations.Count == 0)
            {
                return;
            }

            baseWidth = rubberRect.rect.width;
            if (baseWidth <= Mathf.Epsilon)
            {
                baseWidth = 1f;
            }

            for (var i = 0; i < stationButtons.Count; i++)
            {
                var index = i;
                var button = stationButtons[i];
                if (button == null)
                {
                    continue;
                }

                button.onClick.AddListener(() => MoveTo(index));
            }
        }

        private void Start()
        {
            if (rubberRect == null || stations.Count == 0)
            {
                return;
            }

            var startIndex = Mathf.Clamp(initialIndex, 0, stations.Count - 1);
            SnapTo(startIndex);
            UpdateStatus();
        }

        private void OnDisable()
        {
            KillSequence();
        }

        public void MoveTo(int index)
        {
            if (rubberRect == null || stations.Count == 0)
            {
                return;
            }

            var clamped = Mathf.Clamp(index, 0, stations.Count - 1);
            if (clamped == selectedIndex)
            {
                return;
            }

            var target = stations[clamped];
            if (target == null)
            {
                return;
            }

            selectedIndex = clamped;

            var targetCenter = target.anchoredPosition.x;
            var targetWidth = target.rect.width;
            var targetLeft = targetCenter - targetWidth * 0.5f;
            var targetRight = targetCenter + targetWidth * 0.5f;

            StartRubberBand(targetLeft, targetRight);
            UpdateStatus();
        }

        private void SnapTo(int index)
        {
            selectedIndex = index;

            var target = stations[index];
            if (target == null)
            {
                return;
            }

            var targetCenter = target.anchoredPosition.x;
            var targetWidth = target.rect.width;
            currentLeftX = targetCenter - targetWidth * 0.5f;
            currentRightX = targetCenter + targetWidth * 0.5f;
            ApplyEdges();
        }

        private void StartRubberBand(float toLeft, float toRight)
        {
            KillSequence();

            var moveRight = (toLeft + toRight) * 0.5f >= (currentLeftX + currentRightX) * 0.5f;
            var lagSeconds = duration * Mathf.Clamp(lag, 0f, 0.9f);

            var sequence = DOTween.Sequence();

            if (moveRight)
            {
                // Lead = right edge runs ahead; trail = left edge catches up.
                sequence.Insert(0f,
                    DOTween.To(() => currentRightX, v => { currentRightX = v; ApplyEdges(); }, toRight, duration)
                        .SetEase(leadEase));
                sequence.Insert(lagSeconds,
                    DOTween.To(() => currentLeftX, v => { currentLeftX = v; ApplyEdges(); }, toLeft, duration)
                        .SetEase(trailEase));
            }
            else
            {
                // Lead = left edge runs ahead; trail = right edge catches up.
                sequence.Insert(0f,
                    DOTween.To(() => currentLeftX, v => { currentLeftX = v; ApplyEdges(); }, toLeft, duration)
                        .SetEase(leadEase));
                sequence.Insert(lagSeconds,
                    DOTween.To(() => currentRightX, v => { currentRightX = v; ApplyEdges(); }, toRight, duration)
                        .SetEase(trailEase));
            }

            activeSequence = sequence;
        }

        private void ApplyEdges()
        {
            if (rubberRect == null)
            {
                return;
            }

            var center = (currentLeftX + currentRightX) * 0.5f;
            var width = Mathf.Max(0f, currentRightX - currentLeftX);
            var scaleX = baseWidth > Mathf.Epsilon ? width / baseWidth : 1f;

            var pos = rubberRect.anchoredPosition;
            pos.x = center;
            rubberRect.anchoredPosition = pos;

            var scale = rubberRect.localScale;
            scale.x = scaleX;
            rubberRect.localScale = scale;
        }

        private void UpdateStatus()
        {
            if (statusLabel == null)
            {
                return;
            }

            statusLabel.text = $"Selected station: {selectedIndex}    duration={duration:0.00}s    lag={lag:0.00}";
        }

        private void KillSequence()
        {
            if (activeSequence != null && activeSequence.IsActive())
            {
                activeSequence.Kill();
            }

            activeSequence = null;
        }
    }
}
