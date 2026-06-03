using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UITooltipDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UITooltipControl tooltip;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (tooltip != null)
            {
                tooltip.OnShown.AddListener(HandleShown);
                tooltip.OnHidden.AddListener(HandleHidden);
            }
        }

        private void OnDisable()
        {
            if (tooltip != null)
            {
                tooltip.OnShown.RemoveListener(HandleShown);
                tooltip.OnHidden.RemoveListener(HandleHidden);
            }
        }

        private void HandleShown(string text)
        {
            if (statusLabel != null)
            {
                statusLabel.text = "Tooltip: " + text.Replace("\n", " ");
            }
        }

        private void HandleHidden()
        {
            if (statusLabel != null)
            {
                statusLabel.text = "Hover a marker to see its tooltip.";
            }
        }
    }
}
