using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIValueSliderDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIValueSliderControl slider;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string suffix = "%";

        private void OnEnable()
        {
            if (slider != null) slider.OnValueChanged.AddListener(HandleValue);
            HandleValue(slider != null ? slider.Value : 0f);
        }

        private void OnDisable()
        {
            if (slider != null) slider.OnValueChanged.RemoveListener(HandleValue);
        }

        private void HandleValue(float value)
        {
            if (statusLabel != null)
            {
                statusLabel.text = "Value: " + value.ToString("0", CultureInfo.InvariantCulture) + suffix;
            }
        }
    }
}
