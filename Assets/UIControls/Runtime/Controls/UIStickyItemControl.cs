using UnityEngine;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Marks a ScrollRect content-list item as sticky. Add to any direct child of the
    /// VerticalLayoutGroup content. The parent <see cref="UIStickyListControl"/> handles
    /// the actual positioning; this component carries the edge choice and per-item state.
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

        // Layout pose captured when sticking, restored on unstick
        private Vector2 _savedAnchorMin;
        private Vector2 _savedAnchorMax;
        private Vector2 _savedPivot;
        private Vector2 _savedSizeDelta;

        // Item height captured at stick time — the placeholder rect is not laid out yet
        // on the stick frame, so the list control reads the height from here.
        public float StuckHeight { get; private set; }

        public void SaveLayoutPose(RectTransform rect)
        {
            _savedAnchorMin = rect.anchorMin;
            _savedAnchorMax = rect.anchorMax;
            _savedPivot     = rect.pivot;
            _savedSizeDelta = rect.sizeDelta;
            StuckHeight     = rect.rect.height;
        }

        public void RestoreLayoutPose(RectTransform rect)
        {
            rect.anchorMin = _savedAnchorMin;
            rect.anchorMax = _savedAnchorMax;
            rect.pivot     = _savedPivot;
            rect.sizeDelta = _savedSizeDelta;
        }

        private void OnEnable()
        {
            _list = GetComponentInParent<UIStickyListControl>();
            if (_list != null)
                _list.Register(this);
        }

        private void OnDisable()
        {
            if (_list != null)
                _list.Unregister(this);
            _list = null;
        }
    }
}
