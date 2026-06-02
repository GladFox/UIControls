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

        private void OnEnable()
        {
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
            if (sheet != null)
            {
                sheet.SnapTo(Mathf.Max(0, sheet.SnapCount - 1));
            }
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
        }
    }
}
