using UnityEngine;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Marker for a sticky row inside a <see cref="UIStickyListControl"/> content list.
    /// Purely declarative — the list control scans the content children and drives all
    /// pinning itself, so this component carries nothing but the edge choice.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIStickyItemControl : MonoBehaviour
    {
        public enum StickyEdge
        {
            Top,
            Bottom
        }

        [Tooltip("Viewport edge this item pins to while its natural position is scrolled past it.")]
        [SerializeField] private StickyEdge edge = StickyEdge.Top;

        public StickyEdge Edge
        {
            get => edge;
            set => edge = value;
        }
    }
}
