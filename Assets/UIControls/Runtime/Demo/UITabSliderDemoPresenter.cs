using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UITabSliderDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UITabSliderControl horizontalSlider;
        [SerializeField] private UITabSliderControl eventOnlySlider;
        [SerializeField] private TMP_Text horizontalStatusLabel;
        [SerializeField] private TMP_Text eventOnlyStatusLabel;

        private void OnEnable()
        {
            if (horizontalSlider != null)
            {
                horizontalSlider.OnTabChanged.AddListener(HandleHorizontalTabChanged);
                HandleHorizontalTabChanged(horizontalSlider.SelectedIndex);
            }

            if (eventOnlySlider != null)
            {
                eventOnlySlider.OnTabChanged.AddListener(HandleEventOnlyTabChanged);
                HandleEventOnlyTabChanged(eventOnlySlider.SelectedIndex);
            }
        }

        private void OnDisable()
        {
            if (horizontalSlider != null)
            {
                horizontalSlider.OnTabChanged.RemoveListener(HandleHorizontalTabChanged);
            }

            if (eventOnlySlider != null)
            {
                eventOnlySlider.OnTabChanged.RemoveListener(HandleEventOnlyTabChanged);
            }
        }

        private void HandleHorizontalTabChanged(int index)
        {
            if (horizontalStatusLabel == null)
            {
                return;
            }

            horizontalStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "Horizontal slider: tab {0} active",
                index + 1);
        }

        private void HandleEventOnlyTabChanged(int index)
        {
            if (eventOnlyStatusLabel == null)
            {
                return;
            }

            eventOnlyStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "Event-only slider: section index = {0}",
                index);
        }
    }
}
