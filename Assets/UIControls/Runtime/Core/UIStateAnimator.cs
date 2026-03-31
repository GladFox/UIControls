using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Core
{
    [Serializable]
    public sealed class UIStateAnimator
    {
        [SerializeField] private RectTransform scaleTarget;
        [SerializeField] private CanvasGroup alphaTarget;
        [SerializeField] private Graphic colorTarget;

        private Sequence activeSequence;

        public void AutoAssign(Component owner)
        {
            if (owner == null)
            {
                return;
            }

            if (scaleTarget == null)
            {
                scaleTarget = owner.transform as RectTransform;
            }

            if (alphaTarget == null)
            {
                alphaTarget = owner.GetComponent<CanvasGroup>();
            }

            if (colorTarget == null)
            {
                colorTarget = owner.GetComponent<Graphic>();
            }
        }

        public void ApplyInstant(UIStateVisual state)
        {
            if (state == null)
            {
                return;
            }

            Kill();

            if (scaleTarget != null)
            {
                scaleTarget.localScale = Vector3.one * state.scale;
            }

            if (alphaTarget != null)
            {
                alphaTarget.alpha = state.alpha;
            }

            if (colorTarget != null)
            {
                colorTarget.color = state.color;
            }
        }

        public void Animate(UIStateVisual state)
        {
            if (state == null)
            {
                return;
            }

            Kill();

            var tweenSettings = state.tween;
            var duration = tweenSettings != null ? Mathf.Max(0f, tweenSettings.Duration) : 0f;

            if (duration <= Mathf.Epsilon)
            {
                ApplyInstant(state);
                return;
            }

            var sequence = DOTween.Sequence();
            var hasTweens = false;

            if (scaleTarget != null)
            {
                sequence.Join(scaleTarget.DOScale(state.scale, duration));
                hasTweens = true;
            }

            if (alphaTarget != null)
            {
                sequence.Join(alphaTarget.DOFade(state.alpha, duration));
                hasTweens = true;
            }

            if (colorTarget != null)
            {
                sequence.Join(colorTarget.DOColor(state.color, duration));
                hasTweens = true;
            }

            if (!hasTweens)
            {
                sequence.Kill();
                return;
            }

            tweenSettings?.Apply(sequence);
            activeSequence = sequence;
        }

        public void Kill()
        {
            if (activeSequence != null && activeSequence.IsActive())
            {
                activeSequence.Kill();
            }

            activeSequence = null;
        }
    }
}
