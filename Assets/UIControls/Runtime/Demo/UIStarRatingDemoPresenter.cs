using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIStarRatingDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIStarRatingControl interactiveRating;
        [SerializeField] private UIStarRatingControl readOnlyRating;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (interactiveRating != null)
            {
                interactiveRating.OnRatingChanged.AddListener(HandleChanged);
                HandleChanged(interactiveRating.Value);
            }
        }

        private void OnDisable()
        {
            if (interactiveRating != null)
            {
                interactiveRating.OnRatingChanged.RemoveListener(HandleChanged);
            }
        }

        private void HandleChanged(float value)
        {
            if (statusLabel != null && interactiveRating != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture, "Your rating: {0:0.#} / {1}", value, interactiveRating.StarCount);
            }
        }
    }
}
