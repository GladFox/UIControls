using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIControlsDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIButtonControl buttonControl;
        [SerializeField] private UIToggleControl toggleControl;
        [SerializeField] private UIProgressBarControl progressBarControl;

        [Header("Demo Values")]
        [Range(0f, 1f)]
        [SerializeField] private float progressWhenToggleOn = 0.85f;

        [Range(0f, 1f)]
        [SerializeField] private float progressWhenToggleOff = 0.25f;

        [Range(0.01f, 1f)]
        [SerializeField] private float clickStep = 0.1f;

        private void OnEnable()
        {
            if (buttonControl != null)
            {
                buttonControl.OnClick.AddListener(HandleButtonClick);
            }

            if (toggleControl != null)
            {
                toggleControl.OnValueChanged.AddListener(HandleToggleChanged);
                HandleToggleChanged(toggleControl.IsOn);
            }
        }

        private void OnDisable()
        {
            if (buttonControl != null)
            {
                buttonControl.OnClick.RemoveListener(HandleButtonClick);
            }

            if (toggleControl != null)
            {
                toggleControl.OnValueChanged.RemoveListener(HandleToggleChanged);
            }
        }

        private void HandleButtonClick()
        {
            if (progressBarControl == null)
            {
                return;
            }

            var nextValue = progressBarControl.Value + clickStep;
            if (nextValue > 1f)
            {
                nextValue -= 1f;
            }

            progressBarControl.SetValue(nextValue);
        }

        private void HandleToggleChanged(bool isOn)
        {
            if (progressBarControl == null)
            {
                return;
            }

            progressBarControl.SetValue(isOn ? progressWhenToggleOn : progressWhenToggleOff);
        }
    }
}
