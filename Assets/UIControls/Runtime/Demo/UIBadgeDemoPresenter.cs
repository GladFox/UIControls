using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIBadgeDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIBadgeControl badge;
        [SerializeField] private UIButtonControl plusButton;
        [SerializeField] private UIButtonControl minusButton;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (plusButton != null) plusButton.OnClick.AddListener(Plus);
            if (minusButton != null) minusButton.OnClick.AddListener(Minus);
            if (badge != null) badge.OnCountChanged.AddListener(HandleCount);
            HandleCount(badge != null ? badge.Count : 0);
        }

        private void OnDisable()
        {
            if (plusButton != null) plusButton.OnClick.RemoveListener(Plus);
            if (minusButton != null) minusButton.OnClick.RemoveListener(Minus);
            if (badge != null) badge.OnCountChanged.RemoveListener(HandleCount);
        }

        private void Plus() => badge?.Increment();

        private void Minus() => badge?.Decrement();

        private void HandleCount(int count)
        {
            if (statusLabel != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Count: {0} — badge hides at 0 and shows \"99+\" past the cap.",
                    count);
            }
        }
    }
}
