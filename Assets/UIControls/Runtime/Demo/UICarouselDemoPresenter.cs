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

            if (carousel != null)
            {
                carousel.OnPageChanged.RemoveListener(HandlePageChanged);
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
