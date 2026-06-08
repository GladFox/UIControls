using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIKnobDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIKnobControl knob;
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField] private string suffix = "%";
        [SerializeField] private float scale = 100f;

        private void OnEnable()
        {
            if (knob != null) knob.OnValueChanged.AddListener(HandleValue);
            HandleValue(knob != null ? knob.Value : 0f);
        }

        private void OnDisable()
        {
            if (knob != null) knob.OnValueChanged.RemoveListener(HandleValue);
        }

        private void HandleValue(float value)
        {
            if (valueLabel != null)
            {
                valueLabel.text = (value * scale).ToString("0", CultureInfo.InvariantCulture) + suffix;
            }
        }
    }
}
