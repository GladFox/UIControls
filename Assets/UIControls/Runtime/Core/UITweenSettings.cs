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
    }
}
