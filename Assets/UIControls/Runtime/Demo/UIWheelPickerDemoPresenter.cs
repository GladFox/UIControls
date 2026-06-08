using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIWheelPickerDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIWheelPickerControl wheel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] optionNames;

        private void OnEnable()
        {
            if (wheel != null) wheel.OnValueChanged.AddListener(HandleValue);
            HandleValue(wheel != null ? wheel.SelectedIndex : 0);
        }

        private void OnDisable()
        {
            if (wheel != null) wheel.OnValueChanged.RemoveListener(HandleValue);
        }

        private void HandleValue(int index)
        {
            if (statusLabel == null) return;
            var name = optionNames != null && index >= 0 && index < optionNames.Length ? optionNames[index] : "#" + index;
            statusLabel.text = "Selected: " + name;
        }
    }
}
