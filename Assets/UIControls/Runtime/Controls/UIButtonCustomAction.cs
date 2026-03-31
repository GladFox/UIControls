using UnityEngine;

namespace UIControls.Runtime.Controls
{
    public abstract class UIButtonCustomAction : ScriptableObject
    {
        public virtual void OnPointerEnter(UIButtonControl button)
        {
        }

        public virtual void OnPointerExit(UIButtonControl button)
        {
        }

        public virtual void OnPointerDown(UIButtonControl button)
        {
        }

        public virtual void OnPointerUp(UIButtonControl button)
        {
        }

        public virtual void OnSubmit(UIButtonControl button)
        {
        }

        public virtual void OnStateChanged(UIButtonControl button, UIButtonControl.ButtonVisualState state)
        {
        }

        public virtual void OnClick(UIButtonControl button)
        {
        }
    }
}
