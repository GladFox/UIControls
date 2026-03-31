using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UIControls.Runtime.Controls.Actions
{
    [CreateAssetMenu(menuName = "UIControls/Actions/Button/Scale Pulse", fileName = "UIButtonScalePulseAction")]
    public sealed class UIButtonScalePulseAction : UIButtonCustomAction
    {
        [SerializeField]
        private UIButtonActionTriggerFlags triggerFlags = UIButtonActionTriggerFlags.Click | UIButtonActionTriggerFlags.Submit;

        [Min(0f)]
        [SerializeField]
        private float scaleMultiplier = 1.1f;

        [Min(0f)]
        [SerializeField]
        private float scaleUpDuration = 0.1f;

        [Min(0f)]
        [SerializeField]
        private float scaleDownDuration = 0.1f;

        [SerializeField] private Ease scaleUpEase = Ease.Linear;
        [SerializeField] private Ease scaleDownEase = Ease.Linear;
        [SerializeField] private bool independentUpdate;

        private readonly Dictionary<int, Sequence> activeSequences = new Dictionary<int, Sequence>();

        public override void OnPointerEnter(UIButtonControl button)
        {
            TryPlay(button, UIButtonActionTriggerFlags.PointerEnter);
        }

        public override void OnPointerExit(UIButtonControl button)
        {
            TryPlay(button, UIButtonActionTriggerFlags.PointerExit);
        }

        public override void OnPointerDown(UIButtonControl button)
        {
            TryPlay(button, UIButtonActionTriggerFlags.PointerDown);
        }

        public override void OnPointerUp(UIButtonControl button)
        {
            TryPlay(button, UIButtonActionTriggerFlags.PointerUp);
        }

        public override void OnSubmit(UIButtonControl button)
        {
            TryPlay(button, UIButtonActionTriggerFlags.Submit);
        }

        public override void OnClick(UIButtonControl button)
        {
            TryPlay(button, UIButtonActionTriggerFlags.Click);
        }

        public override void OnStateChanged(UIButtonControl button, UIButtonControl.ButtonVisualState state)
        {
            TryPlay(button, UIButtonActionTriggerFlags.StateChanged);
        }

        private void TryPlay(UIButtonControl button, UIButtonActionTriggerFlags trigger)
        {
            if (button == null || !HasFlag(triggerFlags, trigger))
            {
                return;
            }

            var target = button.transform as RectTransform;
            if (target == null)
            {
                return;
            }

            var key = button.GetInstanceID();
            KillSequence(key);

            var baseScale = target.localScale;
            var targetScale = baseScale * scaleMultiplier;

            if (scaleUpDuration <= Mathf.Epsilon && scaleDownDuration <= Mathf.Epsilon)
            {
                target.localScale = baseScale;
                return;
            }

            var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, independentUpdate);

            if (scaleUpDuration > Mathf.Epsilon)
            {
                sequence.Append(target.DOScale(targetScale, scaleUpDuration).SetEase(scaleUpEase));
            }
            else
            {
                target.localScale = targetScale;
            }

            if (scaleDownDuration > Mathf.Epsilon)
            {
                sequence.Append(target.DOScale(baseScale, scaleDownDuration).SetEase(scaleDownEase));
            }
            else
            {
                target.localScale = baseScale;
            }

            activeSequences[key] = sequence;
            sequence.OnKill(() =>
            {
                if (activeSequences.TryGetValue(key, out var active) && active == sequence)
                {
                    activeSequences.Remove(key);
                }
            });
        }

        private void KillSequence(int key)
        {
            if (!activeSequences.TryGetValue(key, out var sequence))
            {
                return;
            }

            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill(false);
            }

            activeSequences.Remove(key);
        }

        private static bool HasFlag(UIButtonActionTriggerFlags source, UIButtonActionTriggerFlags flag)
        {
            return (source & flag) != 0;
        }
    }
}
