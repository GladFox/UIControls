using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIRangeSliderDemoPresenter : MonoBehaviour
    {
        [Header("Price range (whole numbers)")]
        [SerializeField] private UIRangeSliderControl priceSlider;
        [SerializeField] private TMP_Text priceStatusLabel;

        [Header("Time range (hours)")]
        [SerializeField] private UIRangeSliderControl timeSlider;
        [SerializeField] private TMP_Text timeStatusLabel;

        private void OnEnable()
        {
            if (priceSlider != null)
            {
                priceSlider.OnRangeChanged.AddListener(HandlePriceChanged);
                HandlePriceChanged(priceSlider.LowValue, priceSlider.HighValue);
            }

            if (timeSlider != null)
            {
                timeSlider.OnRangeChanged.AddListener(HandleTimeChanged);
                HandleTimeChanged(timeSlider.LowValue, timeSlider.HighValue);
            }
        }

        private void OnDisable()
        {
            if (priceSlider != null)
            {
                priceSlider.OnRangeChanged.RemoveListener(HandlePriceChanged);
            }

            if (timeSlider != null)
            {
                timeSlider.OnRangeChanged.RemoveListener(HandleTimeChanged);
            }
        }

        private void HandlePriceChanged(float low, float high)
        {
            if (priceStatusLabel == null)
            {
                return;
            }

            priceStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "Price: ${0} — ${1}",
                Mathf.RoundToInt(low),
                Mathf.RoundToInt(high));
        }

        private void HandleTimeChanged(float low, float high)
        {
            if (timeStatusLabel == null)
            {
                return;
            }

            timeStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "Time: {0} — {1}",
                FormatHour(low),
                FormatHour(high));
        }

        private static string FormatHour(float hours)
        {
            var h = Mathf.Clamp(Mathf.RoundToInt(hours), 0, 24);
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:00", h);
        }
    }
}
