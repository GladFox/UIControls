using System.Collections.Generic;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIBreadcrumbsDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIBreadcrumbsControl breadcrumbs;
        [SerializeField] private UIButtonControl resetButton;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] fullPath = { "Home", "Library", "Controls", "Navigation" };

        private void OnEnable()
        {
            if (breadcrumbs != null)
            {
                breadcrumbs.OnCrumbSelected.AddListener(HandleCrumb);
                breadcrumbs.SetPath(fullPath);
            }

            if (resetButton != null) resetButton.OnClick.AddListener(ResetPath);
        }

        private void OnDisable()
        {
            if (breadcrumbs != null) breadcrumbs.OnCrumbSelected.RemoveListener(HandleCrumb);
            if (resetButton != null) resetButton.OnClick.RemoveListener(ResetPath);
        }

        private void ResetPath()
        {
            if (breadcrumbs != null) breadcrumbs.SetPath(fullPath);
            if (statusLabel != null) statusLabel.text = "Path: " + string.Join(" / ", fullPath);
        }

        private void HandleCrumb(int index)
        {
            // Navigate to the clicked crumb: truncate the trail there.
            var trail = new List<string>();
            for (var i = 0; i <= index && i < fullPath.Length; i++)
            {
                trail.Add(fullPath[i]);
            }

            if (breadcrumbs != null) breadcrumbs.SetPath(trail.ToArray());
            if (statusLabel != null) statusLabel.text = "Navigated to: " + string.Join(" / ", trail);
        }
    }
}
