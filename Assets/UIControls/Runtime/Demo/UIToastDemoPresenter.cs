using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIToastDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIToastControl toast;
        [SerializeField] private UIButtonControl infoButton;
        [SerializeField] private UIButtonControl successButton;
        [SerializeField] private UIButtonControl errorButton;
        [SerializeField] private UIButtonControl snackbarButton;
        [SerializeField] private TMP_Text statusLabel;

        private int counter;

        private void OnEnable()
        {
            if (infoButton != null) infoButton.OnClick.AddListener(ShowInfo);
            if (successButton != null) successButton.OnClick.AddListener(ShowSuccess);
            if (errorButton != null) errorButton.OnClick.AddListener(ShowError);
            if (snackbarButton != null) snackbarButton.OnClick.AddListener(ShowSnackbar);
        }

        private void OnDisable()
        {
            if (infoButton != null) infoButton.OnClick.RemoveListener(ShowInfo);
            if (successButton != null) successButton.OnClick.RemoveListener(ShowSuccess);
            if (errorButton != null) errorButton.OnClick.RemoveListener(ShowError);
            if (snackbarButton != null) snackbarButton.OnClick.RemoveListener(ShowSnackbar);
        }

        private void ShowInfo()
        {
            counter++;
            toast?.Show(
                string.Format(CultureInfo.InvariantCulture, "Heads up — info message #{0}", counter),
                -1f, UIToastControl.ToastKind.Info);
        }

        private void ShowSuccess()
        {
            toast?.Show("Saved successfully.", -1f, UIToastControl.ToastKind.Success);
        }

        private void ShowError()
        {
            toast?.Show("Something went wrong.", -1f, UIToastControl.ToastKind.Error);
        }

        private void ShowSnackbar()
        {
            toast?.ShowAction("Item deleted.", "UNDO", HandleUndo, 4f, UIToastControl.ToastKind.Info);
            SetStatus("Item deleted — UNDO available");
        }

        private void HandleUndo()
        {
            SetStatus("Undo pressed — item restored");
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null)
            {
                statusLabel.text = text;
            }
        }
    }
}
