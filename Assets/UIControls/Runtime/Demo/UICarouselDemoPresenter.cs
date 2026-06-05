using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UICarouselDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UICarouselControl carousel;
        [SerializeField] private UIButtonControl prevButton;
        [SerializeField] private UIButtonControl nextButton;
        [SerializeField] private UIToggleControl autoplayToggle;
        [SerializeField] private UIToggleControl pingPongToggle;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (prevButton != null)
            {
                prevButton.OnClick.AddListener(HandlePrev);
            }

            if (nextButton != null)
            {
                nextButton.OnClick.AddListener(HandleNext);
            }

            if (autoplayToggle != null)
            {
                autoplayToggle.OnValueChanged.AddListener(HandleAutoplayToggled);
                HandleAutoplayToggled(autoplayToggle.IsOn);
            }

            if (pingPongToggle != null)
            {
                pingPongToggle.OnValueChanged.AddListener(HandlePingPongToggled);
                HandlePingPongToggled(pingPongToggle.IsOn);
            }

            if (carousel != null)
            {
                carousel.OnPageChanged.AddListener(HandlePageChanged);
                HandlePageChanged(carousel.CurrentPage);
            }
        }

        private void OnDisable()
        {
            if (prevButton != null)
            {
                prevButton.OnClick.RemoveListener(HandlePrev);
            }

            if (nextButton != null)
            {
                nextButton.OnClick.RemoveListener(HandleNext);
            }

            if (autoplayToggle != null)
            {
                autoplayToggle.OnValueChanged.RemoveListener(HandleAutoplayToggled);
            }

            if (pingPongToggle != null)
            {
                pingPongToggle.OnValueChanged.RemoveListener(HandlePingPongToggled);
            }

            if (carousel != null)
            {
                carousel.OnPageChanged.RemoveListener(HandlePageChanged);
            }
        }

        private void HandleAutoplayToggled(bool on)
        {
            if (carousel != null)
            {
                carousel.Autoplay = on;
            }
        }

        private void HandlePingPongToggled(bool pingPong)
        {
            if (carousel != null)
            {
                carousel.Mode = pingPong
                    ? UICarouselControl.AutoplayMode.PingPong
                    : UICarouselControl.AutoplayMode.Loop;
            }
        }

        private void HandlePrev()
        {
            carousel?.Previous();
        }

        private void HandleNext()
        {
            carousel?.Next();
        }

        private void HandlePageChanged(int index)
        {
            if (statusLabel != null && carousel != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture, "Page {0} of {1}", index + 1, carousel.PageCount);
            }
        }
    }
}
