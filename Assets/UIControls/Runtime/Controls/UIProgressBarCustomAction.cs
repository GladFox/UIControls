using UnityEngine;

namespace UIControls.Runtime.Controls
{
    public abstract class UIProgressBarCustomAction : ScriptableObject
    {
        public virtual void OnValueChanged(UIProgressBarControl progressBar, float value)
        {
        }

        public virtual void OnSegmentCompleted(UIProgressBarControl progressBar, int segmentIndex)
        {
        }

        public virtual void OnEchoStarted(UIProgressBarControl progressBar, float from, float to)
        {
        }

        public virtual void OnEchoCompleted(UIProgressBarControl progressBar, float value)
        {
        }
    }
}
