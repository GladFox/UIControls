using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIStepperDemoPresenter : MonoBehaviour
    {
        [Header("Quantity stepper (integer, hold-to-repeat)")]
        [SerializeField] private UIStepperControl quantityStepper;
        [SerializeField] private TMP_Text quantityStatusLabel;

        [Header("Volume stepper (fractional step)")]
        [SerializeField] private UIStepperControl volumeStepper;
        [SerializeField] private TMP_Text volumeStatusLabel;

        private void OnEnable()
        {
            if (quantityStepper != null)
            {
                quantityStepper.OnValueChanged.AddListener(HandleQuantityChanged);
                HandleQuantityChanged(quantityStepper.Value);
            }

            if (volumeStepper != null)
            {
                volumeStepper.OnValueChanged.AddListener(HandleVolumeChanged);
                HandleVolumeChanged(volumeStepper.Value);
            }
        }

        private void OnDisable()
        {
            if (quantityStepper != null)
            {
                quantityStepper.OnValueChanged.RemoveListener(HandleQuantityChanged);
            }

            if (volumeStepper != null)
            {
                volumeStepper.OnValueChanged.RemoveListener(HandleVolumeChanged);
            }
        }

        private void HandleQuantityChanged(float value)
        {
            if (quantityStatusLabel == null)
            {
                return;
            }

            quantityStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "Quantity: {0} item(s)",
                Mathf.RoundToInt(value));
        }

        private void HandleVolumeChanged(float value)
        {
            if (volumeStatusLabel == null)
            {
                return;
            }

            volumeStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "Volume: {0:P0}",
                value);
        }
    }
}
