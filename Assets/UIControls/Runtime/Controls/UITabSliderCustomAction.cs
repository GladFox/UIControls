using UnityEngine;

namespace UIControls.Runtime.Controls
{
    public abstract class UITabSliderCustomAction : ScriptableObject
    {
        public virtual void OnTabChanged(UITabSliderControl tabSlider, int previousIndex, int newIndex)
        {
        }
    }
}
