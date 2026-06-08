using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIPaginationDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIPaginationControl pagination;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (pagination != null) pagination.OnPageChanged.AddListener(HandlePage);
            HandlePage(pagination != null ? pagination.CurrentPage : 1);
        }

        private void OnDisable()
        {
            if (pagination != null) pagination.OnPageChanged.RemoveListener(HandlePage);
        }

        private void HandlePage(int page)
        {
            if (statusLabel == null) return;
            statusLabel.text = string.Format(
                CultureInfo.InvariantCulture, "Page {0} of {1}", page, pagination != null ? pagination.PageCount : page);
        }
    }
}
