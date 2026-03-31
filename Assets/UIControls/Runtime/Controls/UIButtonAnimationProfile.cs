using System;
using UIControls.Runtime.Core;
using UnityEngine;

namespace UIControls.Runtime.Controls
{
    [CreateAssetMenu(menuName = "UIControls/Animations/UI/Button Profile", fileName = "UIButtonAnimationProfile")]
    public sealed class UIButtonAnimationProfile : ScriptableObject
    {
        [Header("State Visual Presets")]
        [SerializeField] private UIStateVisualAsset normal;
        [SerializeField] private UIStateVisualAsset hover;
        [SerializeField] private UIStateVisualAsset pressed;
        [SerializeField] private UIStateVisualAsset disabled;

        [Header("Custom Actions")]
        [SerializeField] private UIButtonCustomAction[] customActions = Array.Empty<UIButtonCustomAction>();

        public UIButtonCustomAction[] CustomActions => customActions;

        public UIStateVisualAsset GetStateAsset(UIButtonControl.ButtonVisualState state)
        {
            switch (state)
            {
                case UIButtonControl.ButtonVisualState.Hover:
                    return hover;
                case UIButtonControl.ButtonVisualState.Pressed:
                    return pressed;
                case UIButtonControl.ButtonVisualState.Disabled:
                    return disabled;
                default:
                    return normal;
            }
        }
    }
}
