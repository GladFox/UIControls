using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIBottomSheetDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIBottomSheetControl sheet;
        [SerializeField] private UIButtonControl openButton;
        [SerializeField] private UIButtonControl expandButton;
        [SerializeField] private UIButtonControl closeButton;
        [SerializeField] private TMP_Text statusLabel;

        private TMP_Text expandLabel;

        private void OnEnable()
        {
            if (expandButton != null && expandLabel == null)
            {
                expandLabel = expandButton.GetComponentInChildren<TMP_Text>(true);
            }

            if (openButton != null)
            {
                openButton.OnClick.AddListener(HandleOpen);
            }

            if (expandButton != null)
            {
                expandButton.OnClick.AddListener(HandleExpand);
            }

            if (closeButton != null)
            {
                closeButton.OnClick.AddListener(HandleClose);
            }

            if (sheet != null)
            {
                sheet.OnStateChanged.AddListener(HandleStateChanged);
                HandleStateChanged(sheet.CurrentSnapIndex);
            }
        }

        private void OnDisable()
        {
            if (openButton != null)
            {
                openButton.OnClick.RemoveListener(HandleOpen);
            }

            if (expandButton != null)
            {
                expandButton.OnClick.RemoveListener(HandleExpand);
            }

            if (closeButton != null)
            {
                closeButton.OnClick.RemoveListener(HandleClose);
            }

            if (sheet != null)
            {
                sheet.OnStateChanged.RemoveListener(HandleStateChanged);
            }
        }

        private void HandleOpen()
        {
            sheet?.Open(0);
        }

        private void HandleExpand()
        {
            if (sheet == null || sheet.SnapCount == 0)
            {
                return;
            }

            var top = sheet.SnapCount - 1;
            // Toggle: if already expanded, collapse back; otherwise expand.
            sheet.SnapTo(sheet.CurrentSnapIndex >= top ? 0 : top);
        }

        private void HandleClose()
        {
            sheet?.Close();
        }

        private void HandleStateChanged(int index)
        {
            if (statusLabel == null)
            {
                return;
            }

            statusLabel.text = index < 0
                ? "Sheet: closed"
                : string.Format(CultureInfo.InvariantCulture, "Sheet: open at snap {0}", index);

            // Flip the toggle button between Expand / Collapse based on whether the sheet is
            // already at its top snap point.
            if (expandLabel != null && sheet != null && sheet.SnapCount > 0)
            {
                var expanded = index >= sheet.SnapCount - 1;
                expandLabel.text = expanded ? "Collapse" : "Expand";
            }
        }
    }
}
