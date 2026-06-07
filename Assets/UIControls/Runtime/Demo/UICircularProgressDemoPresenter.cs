using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UICircularProgressDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UICircularProgressControl determinate;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private float fillSpeed = 0.25f;

        private float progress;

        private void Update()
        {
            if (determinate == null)
            {
                return;
            }

            progress = Mathf.Repeat(progress + fillSpeed * Time.unscaledDeltaTime, 1.0001f);
            determinate.SetValue(progress, false);

            if (statusLabel != null)
            {
                statusLabel.text = "Determinate auto-fills; the right ring is indeterminate (spinning).";
            }
        }
    }
}
