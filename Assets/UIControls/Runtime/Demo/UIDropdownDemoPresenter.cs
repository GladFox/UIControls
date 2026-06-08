using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIDropdownDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIDropdownControl dropdown;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (dropdown != null) dropdown.OnValueChanged.AddListener(HandleValue);
        }

        private void OnDisable()
        {
            if (dropdown != null) dropdown.OnValueChanged.RemoveListener(HandleValue);
        }

        private void HandleValue(int index)
        {
            if (statusLabel != null && dropdown != null)
            {
                statusLabel.text = "Selected: " + dropdown.SelectedValue;
            }
        }
    }
}
