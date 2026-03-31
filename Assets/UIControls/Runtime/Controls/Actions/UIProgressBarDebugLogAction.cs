using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Controls.Actions
{
    [CreateAssetMenu(menuName = "UIControls/ProgressBar Actions/Debug Log")]
    public sealed class UIProgressBarDebugLogAction : UIProgressBarCustomAction
    {
        [SerializeField] private string logPrefix = "[UIProgressBar]";
        [SerializeField] private bool logValueChanged = true;
        [SerializeField] private bool logSegmentCompleted = true;
        [SerializeField] private bool logEchoStarted = true;
        [SerializeField] private bool logEchoCompleted = true;

        public override void OnValueChanged(UIProgressBarControl progressBar, float value)
        {
            if (!logValueChanged)
            {
                return;
            }

            Debug.Log($"{logPrefix} Value changed to {value:P0}", progressBar);
        }

        public override void OnSegmentCompleted(UIProgressBarControl progressBar, int segmentIndex)
        {
            if (!logSegmentCompleted)
            {
                return;
            }

            Debug.Log($"{logPrefix} Segment completed: {segmentIndex + 1}", progressBar);
        }

        public override void OnEchoStarted(UIProgressBarControl progressBar, float from, float to)
        {
            if (!logEchoStarted)
            {
                return;
            }

            Debug.Log($"{logPrefix} Echo started: {from:P0} -> {to:P0}", progressBar);
        }

        public override void OnEchoCompleted(UIProgressBarControl progressBar, float value)
        {
            if (!logEchoCompleted)
            {
                return;
            }

            Debug.Log($"{logPrefix} Echo completed at {value:P0}", progressBar);
        }
    }
}
