using System;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    public sealed class UIButtonControl : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        ISubmitHandler
    {
        public enum ButtonVisualState
        {
            Normal,
            Hover,
            Pressed,
            Disabled
        }

        [SerializeField] private bool interactable = true;
        [SerializeField] private UIStateAnimator stateAnimator = new UIStateAnimator();

        [Header("Animation Profile")]
        [SerializeField] private UIButtonAnimationProfile animationProfile;

        [Header("State Visual Presets")]
        [SerializeField] private UIStateVisualAsset normal;
        [SerializeField] private UIStateVisualAsset hover;
        [SerializeField] private UIStateVisualAsset pressed;
        [SerializeField] private UIStateVisualAsset disabled;

        [Header("Events")]
        [SerializeField] private UnityEvent onClick = new UnityEvent();

        [Header("Custom Actions")]
        [SerializeField] private UIButtonCustomAction[] customActions = Array.Empty<UIButtonCustomAction>();

        private static readonly UIStateVisual DefaultNormal = new UIStateVisual();
        private static readonly UIStateVisual DefaultHover = new UIStateVisual { scale = 1.04f };
        private static readonly UIStateVisual DefaultPressed = new UIStateVisual { scale = 0.96f };
        private static readonly UIStateVisual DefaultDisabled = new UIStateVisual { alpha = 0.5f };

        private bool pointerInside;
        private bool pointerDown;
        private bool hasState;
        private ButtonVisualState visualState;

        public bool Interactable
        {
            get => interactable;
            set => SetInteractable(value);
        }

        public ButtonVisualState VisualState => visualState;
        public UnityEvent OnClick => onClick;

        public UIButtonAnimationProfile AnimationProfile => animationProfile;

        public ButtonVisualState CurrentState => visualState;

        private void Awake()
        {
            stateAnimator.AutoAssign(this);
        }

        private void OnEnable()
        {
            RefreshVisual(true);
        }

        private void OnDisable()
        {
            pointerInside = false;
            pointerDown = false;
            hasState = false;
            stateAnimator.Kill();
        }

        public void SetInteractable(bool value, bool instant = false)
        {
            if (interactable == value)
            {
                if (instant)
                {
                    RefreshVisual(true);
                }

                return;
            }

            interactable = value;
            RefreshVisual(instant);
        }

        public void SetAnimationProfile(UIButtonAnimationProfile profile, bool instant = false)
        {
            if (animationProfile == profile)
            {
                if (instant)
                {
                    RefreshVisual(true);
                }

                return;
            }

            animationProfile = profile;
            RefreshVisual(instant);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            pointerInside = true;
            RefreshVisual();
            InvokeCustomActions(action => action.OnPointerEnter(this));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            pointerDown = false;
            RefreshVisual();
            InvokeCustomActions(action => action.OnPointerExit(this));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            pointerDown = true;
            RefreshVisual();
            InvokeCustomActions(action => action.OnPointerDown(this));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            pointerDown = false;
            RefreshVisual();
            InvokeCustomActions(action => action.OnPointerUp(this));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            HandleClick();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            InvokeCustomActions(action => action.OnSubmit(this));
            HandleClick();
        }

        private void HandleClick()
        {
            onClick?.Invoke();
            InvokeCustomActions(action => action.OnClick(this));
        }

        private void RefreshVisual(bool instant = false)
        {
            var nextState = ResolveState();
            var visual = ResolveVisual(nextState);

            if (instant)
            {
                stateAnimator.ApplyInstant(visual);
            }
            else
            {
                stateAnimator.Animate(visual);
            }

            if (!hasState || visualState != nextState)
            {
                visualState = nextState;
                hasState = true;
                InvokeCustomActions(action => action.OnStateChanged(this, visualState));
            }
        }

        private ButtonVisualState ResolveState()
        {
            if (!interactable)
            {
                return ButtonVisualState.Disabled;
            }

            if (pointerDown)
            {
                return ButtonVisualState.Pressed;
            }

            if (pointerInside)
            {
                return ButtonVisualState.Hover;
            }

            return ButtonVisualState.Normal;
        }

        private UIStateVisual ResolveVisual(ButtonVisualState state)
        {
            var profileState = animationProfile != null ? animationProfile.GetStateAsset(state) : null;
            if (profileState != null)
            {
                return profileState.State;
            }

            switch (state)
            {
                case ButtonVisualState.Hover:
                    return hover != null ? hover.State : DefaultHover;
                case ButtonVisualState.Pressed:
                    return pressed != null ? pressed.State : DefaultPressed;
                case ButtonVisualState.Disabled:
                    return disabled != null ? disabled.State : DefaultDisabled;
                default:
                    return normal != null ? normal.State : DefaultNormal;
            }
        }

        private void InvokeCustomActions(Action<UIButtonCustomAction> invocation)
        {
            if (invocation == null)
            {
                return;
            }

            InvokeActionCollection(animationProfile != null ? animationProfile.CustomActions : null, invocation);
            InvokeActionCollection(customActions, invocation);
        }

        private void InvokeActionCollection(UIButtonCustomAction[] actions, Action<UIButtonCustomAction> invocation)
        {
            if (actions == null || invocation == null)
            {
                return;
            }

            for (var i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                if (action == null)
                {
                    continue;
                }

                try
                {
                    invocation(action);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }
    }
}
