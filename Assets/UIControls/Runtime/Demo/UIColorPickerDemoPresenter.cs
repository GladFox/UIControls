using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIColorPickerDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIColorPickerControl picker;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (picker != null)
            {
                picker.OnColorChanged.AddListener(HandleColor);
                HandleColor(picker.Color);
            }
        }

        private void OnDisable()
        {
            if (picker != null)
            {
                picker.OnColorChanged.RemoveListener(HandleColor);
            }
        }

        private void HandleColor(Color color)
        {
            if (statusLabel != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "RGB ({0}, {1}, {2})   #{3}",
                    Mathf.RoundToInt(color.r * 255f),
                    Mathf.RoundToInt(color.g * 255f),
                    Mathf.RoundToInt(color.b * 255f),
                    ColorUtility.ToHtmlStringRGB(color));
            }
        }
    }
}
