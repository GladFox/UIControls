using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UIControls.Runtime.Controls.Actions
{
    [CreateAssetMenu(menuName = "UIControls/Actions/Button/Anchored Offset", fileName = "UIButtonAnchoredOffsetAction")]
    public sealed class UIButtonAnchoredOffsetAction : UIButtonCustomAction
    {
        [Header("Press/Release Triggers")]
        [SerializeField] private UIButtonActionTriggerFlags pressTriggers = UIButtonActionTriggerFlags.PointerDown;
        [SerializeField] private UIButtonActionTriggerFlags releaseTriggers = UIButtonActionTriggerFlags.PointerUp | UIButtonActionTriggerFlags.PointerExit;
        [SerializeField] private bool releaseWhenDisabled = true;

        [Header("Offset")]
        [SerializeField] private Vector2 pressedOffset = new Vector2(0f, -10f);

        [Min(0f)]
        [SerializeField]
        private float pressDuration = 0.2f;

        [Min(0f)]
        [SerializeField]
        private float releaseDuration = 0.1f;

        [SerializeField] private Ease pressEase = Ease.OutQuad;
        [SerializeField] private Ease releaseEase = Ease.OutQuad;
        [SerializeField] private bool independentUpdate;

        private readonly Dictionary<int, Tween> activeTweens = new Dictionary<int, Tween>();
        private readonly Dictionary<int, Vector2> basePositions = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, bool> isPressedMap = new Dictionary<int, bool>();

        public override void OnPointerEnter(UIButtonControl button)
        {
            TryPress(button, UIButtonActionTriggerFlags.PointerEnter);
            TryRelease(button, UIButtonActionTriggerFlags.PointerEnter);
        }

        public override void OnPointerExit(UIButtonControl button)
        {
            TryPress(button, UIButtonActionTriggerFlags.PointerExit);
            TryRelease(button, UIButtonActionTriggerFlags.PointerExit);
        }

        public override void OnPointerDown(UIButtonControl button)
        {
            TryPress(button, UIButtonActionTriggerFlags.PointerDown);
            TryRelease(button, UIButtonActionTriggerFlags.PointerDown);
        }

        public override void OnPointerUp(UIButtonControl button)
        {
            TryPress(button, UIButtonActionTriggerFlags.PointerUp);
            TryRelease(button, UIButtonActionTriggerFlags.PointerUp);
        }

        public override void OnSubmit(UIButtonControl button)
        {
            TryPress(button, UIButtonActionTriggerFlags.Submit);
            TryRelease(button, UIButtonActionTriggerFlags.Submit);
        }

        public override void OnClick(UIButtonControl button)
        {
            TryPress(button, UIButtonActionTriggerFlags.Click);
            TryRelease(button, UIButtonActionTriggerFlags.Click);
        }

        public override void OnStateChanged(UIButtonControl button, UIButtonControl.ButtonVisualState state)
        {
            if (button == null)
            {
                return;
            }

            if (state == UIButtonControl.ButtonVisualState.Pressed)
            {
                TryPress(button, UIButtonActionTriggerFlags.StateChanged);
                return;
            }

            if (releaseWhenDisabled && state == UIButtonControl.ButtonVisualState.Disabled)
            {
                Animate(button, false);
                return;
            }

            TryRelease(button, UIButtonActionTriggerFlags.StateChanged);
        }

        private void TryPress(UIButtonControl button, UIButtonActionTriggerFlags trigger)
        {
            if (button == null || !HasFlag(pressTriggers, trigger))
            {
                return;
            }

            Animate(button, true);
        }

        private void TryRelease(UIButtonControl button, UIButtonActionTriggerFlags trigger)
        {
            if (button == null || !HasFlag(releaseTriggers, trigger))
            {
                return;
            }

            Animate(button, false);
        }

        private void Animate(UIButtonControl button, bool pressed)
        {
            var target = button.transform as RectTransform;
            if (target == null)
            {
                return;
            }

            var key = button.GetInstanceID();

            if (pressed)
            {
                if (!isPressedMap.TryGetValue(key, out var isPressed) || !isPressed)
                {
                    basePositions[key] = target.anchoredPosition;
                    isPressedMap[key] = true;
                }
            }
            else
            {
                isPressedMap[key] = false;
            }

            if (!basePositions.TryGetValue(key, out var basePosition))
            {
                basePosition = target.anchoredPosition;
                basePositions[key] = basePosition;
            }

            var duration = pressed ? pressDuration : releaseDuration;
            var ease = pressed ? pressEase : releaseEase;
            var destination = pressed ? basePosition + pressedOffset : basePosition;

            KillTween(key);

            if (duration <= Mathf.Epsilon)
            {
                target.anchoredPosition = destination;
                return;
            }

            var tween = target.DOAnchorPos(destination, duration)
                .SetEase(ease)
                .SetUpdate(UpdateType.Normal, independentUpdate);

            activeTweens[key] = tween;
            tween.OnKill(() =>
            {
                if (activeTweens.TryGetValue(key, out var active) && active == tween)
                {
                    activeTweens.Remove(key);
                }
            });
        }

        private void KillTween(int key)
        {
            if (!activeTweens.TryGetValue(key, out var tween))
            {
                return;
            }

            if (tween != null && tween.IsActive())
            {
                tween.Kill(false);
            }

            activeTweens.Remove(key);
        }

        private static bool HasFlag(UIButtonActionTriggerFlags source, UIButtonActionTriggerFlags flag)
        {
            return (source & flag) != 0;
        }
    }
}
