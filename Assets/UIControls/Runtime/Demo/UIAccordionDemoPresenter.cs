using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIAccordionDemoPresenter : MonoBehaviour
    {
        [Header("Single-open (FAQ) accordion")]
        [SerializeField] private UIAccordionControl faqAccordion;
        [SerializeField] private TMP_Text faqStatusLabel;

        [Header("Multi-open (settings) accordion")]
        [SerializeField] private UIAccordionControl settingsAccordion;
        [SerializeField] private TMP_Text settingsStatusLabel;

        private void OnEnable()
        {
            if (faqAccordion != null)
            {
                faqAccordion.OnSectionToggled.AddListener(HandleFaqToggled);
                UpdateFaqStatus();
            }

            if (settingsAccordion != null)
            {
                settingsAccordion.OnSectionToggled.AddListener(HandleSettingsToggled);
                UpdateSettingsStatus();
            }
        }

        private void OnDisable()
        {
            if (faqAccordion != null)
            {
                faqAccordion.OnSectionToggled.RemoveListener(HandleFaqToggled);
            }

            if (settingsAccordion != null)
            {
                settingsAccordion.OnSectionToggled.RemoveListener(HandleSettingsToggled);
            }
        }

        private void HandleFaqToggled(int index, bool expanded)
        {
            UpdateFaqStatus();
        }

        private void HandleSettingsToggled(int index, bool expanded)
        {
            UpdateSettingsStatus();
        }

        private void UpdateFaqStatus()
        {
            if (faqStatusLabel == null || faqAccordion == null)
            {
                return;
            }

            var openIndex = -1;
            for (var i = 0; i < faqAccordion.SectionCount; i++)
            {
                if (faqAccordion.IsExpanded(i))
                {
                    openIndex = i;
                    break;
                }
            }

            faqStatusLabel.text = openIndex < 0
                ? "FAQ: all collapsed"
                : string.Format(CultureInfo.InvariantCulture, "FAQ: question {0} open", openIndex + 1);
        }

        private void UpdateSettingsStatus()
        {
            if (settingsStatusLabel == null || settingsAccordion == null)
            {
                return;
            }

            var count = 0;
            for (var i = 0; i < settingsAccordion.SectionCount; i++)
            {
                if (settingsAccordion.IsExpanded(i))
                {
                    count++;
                }
            }

            settingsStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture, "Settings: {0} section(s) open", count);
        }
    }
}
