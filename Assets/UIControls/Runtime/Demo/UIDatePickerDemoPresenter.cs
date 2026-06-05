using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIDatePickerDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIDatePickerControl picker;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (picker != null)
            {
                picker.OnDateChanged.AddListener(HandleChanged);
                HandleChanged();
            }
        }

        private void OnDisable()
        {
            if (picker != null)
            {
                picker.OnDateChanged.RemoveListener(HandleChanged);
            }
        }

        private void HandleChanged()
        {
            if (statusLabel != null && picker != null)
            {
                statusLabel.text = "Selected: " +
                    picker.SelectedDate.ToString("dddd, dd MMMM yyyy", CultureInfo.InvariantCulture);
            }
        }
    }
}
