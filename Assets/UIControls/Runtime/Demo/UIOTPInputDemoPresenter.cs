using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIOTPInputDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIOTPInputControl otp;
        [SerializeField] private UIButtonControl clearButton;
        [SerializeField] private TMP_Text statusLabel;

        private static readonly Color NeutralColor = new Color(0.82f, 0.88f, 1f, 1f);
        private static readonly Color OkColor = new Color(0.45f, 0.85f, 0.55f, 1f);

        private void OnEnable()
        {
            if (otp != null)
            {
                otp.OnChanged.AddListener(HandleChanged);
                otp.OnCompleted.AddListener(HandleCompleted);
                HandleChanged(otp.Code);
            }

            if (clearButton != null)
            {
                clearButton.OnClick.AddListener(HandleClear);
            }
        }

        private void OnDisable()
        {
            if (otp != null)
            {
                otp.OnChanged.RemoveListener(HandleChanged);
                otp.OnCompleted.RemoveListener(HandleCompleted);
            }

            if (clearButton != null)
            {
                clearButton.OnClick.RemoveListener(HandleClear);
            }
        }

        private void HandleChanged(string code)
        {
            if (statusLabel == null || otp == null)
            {
                return;
            }

            statusLabel.color = NeutralColor;
            statusLabel.text = string.IsNullOrEmpty(code)
                ? "Type or paste the code — focus auto-advances, backspace goes back."
                : string.Format(CultureInfo.InvariantCulture, "Entered {0} of {1}", code.Length, otp.Length);
        }

        private void HandleCompleted(string code)
        {
            if (statusLabel != null)
            {
                statusLabel.color = OkColor;
                statusLabel.text = string.Format(CultureInfo.InvariantCulture, "Code {0} — verified ✓", code);
            }
        }

        private void HandleClear()
        {
            otp?.Clear();
            otp?.Focus();
        }
    }
}
