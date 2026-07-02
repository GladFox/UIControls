using UnityEngine;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Marks a ScrollRect content-list item as sticky. Add to any direct child of the
    /// VerticalLayoutGroup content. The parent <see cref="UIStickyListControl"/> handles
    /// the actual positioning; this component is a data-carrier only.
    /// </summary>
    public sealed class UIStickyItemControl : MonoBehaviour
    {
        public enum StickyEdge { Top, Bottom }

        [SerializeField] private StickyEdge edge = StickyEdge.Top;

        public StickyEdge Edge => edge;

        // Runtime state managed by UIStickyListControl
        [System.NonSerialized] public RectTransform Placeholder;
        [System.NonSerialized] public bool IsStuck;

        private UIStickyListControl _list;

        private void OnEnable()
        {
            _list = GetComponentInParent<UIStickyListControl>();
            _list?.Register(this);
        }

        private void OnDisable()
        {
            _list?.Unregister(this);
            _list = null;
        }
    }
}
