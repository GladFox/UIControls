using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UIControls.Runtime.Controls.Actions
{
    [CreateAssetMenu(menuName = "UIControls/ProgressBar Actions/Scale Pulse", fileName = "UIProgressBarScalePulse")]
    public sealed class UIProgressBarScalePulseAction : UIProgressBarCustomAction
    {
        [Min(1f)]
        [SerializeField] private float scaleMultiplier = 1.05f;

        [Min(0f)]
        [SerializeField] private float duration = 0.2f;

        [SerializeField] private Ease ease = Ease.OutQuad;
        [SerializeField] private bool independentUpdate;

        private readonly Dictionary<int, Sequence> activeSequences = new Dictionary<int, Sequence>();

        public override void OnSegmentCompleted(UIProgressBarControl progressBar, int segmentIndex)
        {
            if (progressBar == null)
            {
                return;
            }

            var target = progressBar.transform as RectTransform;
            if (target == null)
            {
                return;
            }

            var key = progressBar.GetInstanceID();

            if (activeSequences.TryGetValue(key, out var existing) && existing != null && existing.IsActive())
            {
                existing.Kill(false);
            }

            var dur = Mathf.Max(0f, duration);
            var baseScale = Vector3.one;

            if (dur <= Mathf.Epsilon)
            {
                target.localScale = baseScale;
                activeSequences.Remove(key);
                return;
            }

            var half = dur * 0.5f;
            var expanded = baseScale * Mathf.Max(1f, scaleMultiplier);
            var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, independentUpdate);

            sequence.Append(target.DOScale(expanded, half).SetEase(ease));
            sequence.Append(target.DOScale(baseScale, half).SetEase(ease));
            sequence.OnKill(() =>
            {
                target.localScale = baseScale;
                if (activeSequences.TryGetValue(key, out var current) && current == sequence)
                {
                    activeSequences.Remove(key);
                }
            });

            activeSequences[key] = sequence;
        }
    }
}
