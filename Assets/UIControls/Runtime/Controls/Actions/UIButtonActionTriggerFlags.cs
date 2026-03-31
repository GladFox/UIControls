using System;

namespace UIControls.Runtime.Controls.Actions
{
    [Flags]
    public enum UIButtonActionTriggerFlags
    {
        None = 0,
        PointerEnter = 1 << 0,
        PointerExit = 1 << 1,
        PointerDown = 1 << 2,
        PointerUp = 1 << 3,
        Click = 1 << 4,
        Submit = 1 << 5,
        StateChanged = 1 << 6
    }
}
