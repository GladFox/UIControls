using System;
using DG.Tweening;
using UnityEngine;

namespace UIControls.Runtime.Core
{
    [Serializable]
    public sealed class UITweenSettings
    {
        [Min(0f)]
        [SerializeField] private float duration = 0.2f;

        [SerializeField] private Ease ease = Ease.OutQuad;

        [Min(0f)]
        [SerializeField] private float delay;

        [SerializeField] private bool independentUpdate;

        public float Duration => duration;
        public Ease Ease => ease;
        public float Delay => delay;
        public bool IndependentUpdate => independentUpdate;

        public Tween Apply(Tween tween)
        {
            if (tween == null)
            {
                return null;
            }

            return tween
                .SetDelay(delay)
                .SetEase(ease)
                .SetUpdate(UpdateType.Normal, independentUpdate);
        }

        /// <summary>
        /// Applies delay and independentUpdate — leaves ease untouched. Useful for sequences
        /// where each child tween already has its own ease, and a sequence-level ease would
        /// reshape the playhead unexpectedly.
        /// </summary>
        public Tween ApplyTimingOnly(Tween tween)
        {
            if (tween == null)
            {
                return null;
            }

            return tween
                .SetDelay(delay)
                .SetUpdate(UpdateType.Normal, independentUpdate);
        }
    }
}
