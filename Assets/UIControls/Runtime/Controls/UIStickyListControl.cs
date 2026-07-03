using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Sticky rows for a vertical <see cref="ScrollRect"/> list. Any direct child of the
    /// content <c>VerticalLayoutGroup</c> marked with <see cref="UIStickyItemControl"/>
    /// pins to the top or bottom of the viewport once its natural position scrolls past
    /// that edge. When the next sticky row reaches a pinned one it pushes it off the
    /// edge (iOS section-header behaviour). Works with any number of marked rows and
    /// with rows added or removed at runtime — the control re-scans the content every
    /// frame, no registration required.
    ///
    /// Pinning is implemented by moving the row into an overlay layer above the list and
    /// parking a placeholder of the same size in its slot, so the layout group never
    /// notices. The overlay layers are created automatically under the viewport (or
    /// assign your own via <see cref="topLayer"/> / <see cref="bottomLayer"/>).
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    [DisallowMultipleComponent]
    public sealed class UIStickyListControl : MonoBehaviour
    {
        [Header("Overlay layers (auto-created when empty)")]
        [Tooltip("Full-viewport overlay that hosts rows pinned to the top edge.")]
        [SerializeField] private RectTransform topLayer;
        [Tooltip("Full-viewport overlay that hosts rows pinned to the bottom edge.")]
        [SerializeField] private RectTransform bottomLayer;

        // An item starts pinning only once it is measurably past the edge, so a row
        // resting exactly on the boundary doesn't oscillate between states.
        private const float EdgeEpsilon = 0.1f;

        private ScrollRect _scrollRect;

        // Persistent per-item state, keyed by the marker component
        private readonly Dictionary<UIStickyItemControl, PinState> _states =
            new Dictionary<UIStickyItemControl, PinState>();

        // Scratch lists rebuilt every frame in content order (no allocations after warm-up)
        private readonly List<UIStickyItemControl> _topRows    = new List<UIStickyItemControl>(8);
        private readonly List<UIStickyItemControl> _bottomRows = new List<UIStickyItemControl>(8);

        private static readonly Vector3[] Corners = new Vector3[4];

        /// <summary>Everything the control needs to pin a row and undo it later.</summary>
        private sealed class PinState
        {
            public bool Pinned;
            public RectTransform Placeholder; // pooled: created once, toggled active
            public float Height;              // row height captured at pin time
            public Vector2 AnchorMin, AnchorMax, Pivot, SizeDelta; // pose to restore
        }

        /// <summary>Marks pooled placeholder objects so the content scan can skip them.</summary>
        private sealed class PlaceholderTag : MonoBehaviour
        {
            public UIStickyItemControl Owner;
        }

        private RectTransform ViewportRect =>
            _scrollRect.viewport != null ? _scrollRect.viewport : (RectTransform)_scrollRect.transform;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        private void OnDisable()
        {
            // Give every pinned row back to the layout before we stop driving them
            foreach (var pair in _states)
            {
                if (pair.Value.Pinned)
                    Unpin(pair.Key, pair.Value);
            }
        }

        private void LateUpdate()
        {
            var content = _scrollRect.content;
            if (content == null)
                return;

            if (content != _watchedContent)
                WatchContent(content);
            if (_contentDirty)
                ScanContent(content);

            EvaluateEdge(_topRows, atTop: true);
            EvaluateEdge(_bottomRows, atTop: false);
        }

        // ── Content scan ──────────────────────────────────────────────────────────

        /// <summary>
        /// Forces a re-scan of the content children on the next frame. Called
        /// automatically when children are added/removed/reordered; call it manually
        /// only after changes the watcher can't see (e.g. toggling a row's active
        /// state while it is pinned from another script).
        /// </summary>
        public void Refresh() => _contentDirty = true;

        private RectTransform _watchedContent;
        private bool _contentDirty = true;

        // Attaches a watcher to the content so the scan runs only when the child list
        // actually changes, instead of every frame.
        private void WatchContent(RectTransform content)
        {
            if (!content.TryGetComponent<ContentWatcher>(out var watcher))
                watcher = content.gameObject.AddComponent<ContentWatcher>();
            watcher.Owner = this;
            _watchedContent = content;
            _contentDirty = true;
        }

        /// <summary>Invalidates the scan when the content's child list changes.</summary>
        private sealed class ContentWatcher : MonoBehaviour
        {
            public UIStickyListControl Owner;

            private void OnTransformChildrenChanged()
            {
                if (Owner != null)
                    Owner._contentDirty = true;
            }
        }

        // Collects sticky rows in visual (sibling) order. A pinned row lives in the
        // overlay, so its (active) placeholder represents it in the content order.
        // Rows currently inactive are still collected — activity is checked during
        // evaluation, so a SetActive toggle doesn't need a re-scan.
        private void ScanContent(RectTransform content)
        {
            _contentDirty = false;
            _topRows.Clear();
            _bottomRows.Clear();

            for (var i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);

                UIStickyItemControl row = null;
                if (child.TryGetComponent<PlaceholderTag>(out var tag))
                {
                    // Inactive placeholder = pooled leftover of an unpinned row; skip it,
                    // the row itself is elsewhere in the child list
                    if (child.gameObject.activeSelf)
                        row = tag.Owner;
                }
                else
                {
                    child.TryGetComponent(out row);
                }

                if (row == null)
                    continue;

                (row.Edge == UIStickyItemControl.StickyEdge.Top ? _topRows : _bottomRows).Add(row);
            }
        }

        // ── Pin evaluation ────────────────────────────────────────────────────────

        // Rows are visited in the order they pin: top rows first-to-last, bottom rows
        // last-to-first. `Protrusion` measures how far a row's natural position pokes
        // past the viewport edge (> 0 = it has scrolled out and must pin). The
        // neighbouring sticky row toward the list interior acts as the pusher: once it
        // comes closer to the edge than the pinned row's height, the pinned row slides
        // out by the difference.
        private void EvaluateEdge(List<UIStickyItemControl> rows, bool atTop)
        {
            for (var n = 0; n < rows.Count; n++)
            {
                var i     = atTop ? n : rows.Count - 1 - n;
                var row   = rows[i];
                if (row == null)
                    continue;

                var state = GetState(row);

                if (!row.gameObject.activeSelf)
                {
                    if (state.Pinned)
                        Unpin(row, state);
                    continue;
                }

                var protrusion = Protrusion(NaturalRect(row, state), atTop);

                if (protrusion > EdgeEpsilon && !state.Pinned)
                    Pin(row, state, atTop);
                else if (protrusion <= EdgeEpsilon && state.Pinned)
                    Unpin(row, state);

                if (!state.Pinned)
                    continue;

                var slideOut = 0f;
                var pusher = NextActiveRow(rows, i, atTop ? +1 : -1);
                if (pusher != null)
                {
                    var gap = -Protrusion(NaturalRect(pusher, GetState(pusher)), atTop);
                    // gap = distance left until the pusher reaches the edge
                    if (gap < state.Height)
                        slideOut = Mathf.Min(state.Height - gap, state.Height);
                }

                Place(row, state, atTop, slideOut);
            }
        }

        // Closest active sticky row toward the list interior — the potential pusher.
        private static UIStickyItemControl NextActiveRow(
            List<UIStickyItemControl> rows, int from, int step)
        {
            for (var i = from + step; i >= 0 && i < rows.Count; i += step)
            {
                var row = rows[i];
                if (row != null && row.gameObject.activeSelf)
                    return row;
            }

            return null;
        }

        // ── Pin / Unpin ───────────────────────────────────────────────────────────

        private void Pin(UIStickyItemControl row, PinState state, bool atTop)
        {
            var rect  = (RectTransform)row.transform;
            var layer = EnsureLayer(atTop);
            if (layer == null)
                return;

            state.Height    = rect.rect.height;
            state.AnchorMin = rect.anchorMin;
            state.AnchorMax = rect.anchorMax;
            state.Pivot     = rect.pivot;
            state.SizeDelta = rect.sizeDelta;

            var ph = EnsurePlaceholder(row, state);
            ph.SetParent(rect.parent, false);
            ph.SetSiblingIndex(rect.GetSiblingIndex());
            var element = ph.GetComponent<LayoutElement>();
            element.preferredWidth  = rect.rect.width;
            element.preferredHeight = state.Height;
            element.minHeight       = state.Height;
            ph.gameObject.SetActive(true);

            rect.SetParent(layer, false);
            state.Pinned = true;
        }

        private void Unpin(UIStickyItemControl row, PinState state)
        {
            var rect = (RectTransform)row.transform;
            var ph   = state.Placeholder;

            if (ph != null)
            {
                rect.SetParent(ph.parent, false);
                rect.SetSiblingIndex(ph.GetSiblingIndex());
                ph.gameObject.SetActive(false); // pooled, reused on the next pin
            }

            rect.anchorMin = state.AnchorMin;
            rect.anchorMax = state.AnchorMax;
            rect.pivot     = state.Pivot;
            rect.sizeDelta = state.SizeDelta;
            state.Pinned   = false;

            if (rect.parent is RectTransform parentRect)
                LayoutRebuilder.MarkLayoutForRebuild(parentRect);
        }

        private void Place(UIStickyItemControl row, PinState state, bool atTop, float slideOut)
        {
            var rect = (RectTransform)row.transform;
            var y    = atTop ? 1f : 0f;

            rect.anchorMin = new Vector2(0f, y);
            rect.anchorMax = new Vector2(1f, y);
            rect.pivot     = new Vector2(0.5f, y);
            rect.sizeDelta = new Vector2(0f, state.Height);
            // slideOut ≥ 0 pushes the row past its edge (up for top, down for bottom)
            rect.anchoredPosition = new Vector2(0f, atTop ? slideOut : -slideOut);
        }

        // ── Geometry ──────────────────────────────────────────────────────────────

        // How far the rect's leading edge pokes past the viewport edge, in viewport
        // units: 0 when flush with the edge, positive once scrolled out of view,
        // negative while still inside the viewport.
        private float Protrusion(RectTransform rect, bool atTop)
        {
            var viewport = ViewportRect;
            rect.GetWorldCorners(Corners);
            // GetWorldCorners order: [0] bottom-left, [1] top-left, [2] top-right, [3] bottom-right
            var edgeWorld = atTop ? Corners[1] : Corners[0];
            var edgeLocal = viewport.InverseTransformPoint(edgeWorld).y;
            return atTop
                ? edgeLocal - viewport.rect.yMax
                : viewport.rect.yMin - edgeLocal;
        }

        // ── Plumbing ──────────────────────────────────────────────────────────────

        private PinState GetState(UIStickyItemControl row)
        {
            if (!_states.TryGetValue(row, out var state))
            {
                state = new PinState();
                _states.Add(row, state);
            }

            return state;
        }

        // The rect representing the row's natural place in the list: the row itself
        // while it sits in the layout, its placeholder while it is pinned.
        private static RectTransform NaturalRect(UIStickyItemControl row, PinState state)
        {
            return state.Pinned && state.Placeholder != null
                ? state.Placeholder
                : (RectTransform)row.transform;
        }

        private RectTransform EnsurePlaceholder(UIStickyItemControl row, PinState state)
        {
            if (state.Placeholder == null)
            {
                var go = new GameObject($"{row.name} (sticky slot)",
                    typeof(RectTransform), typeof(LayoutElement), typeof(PlaceholderTag));
                go.GetComponent<PlaceholderTag>().Owner = row;
                state.Placeholder = (RectTransform)go.transform;
            }

            return state.Placeholder;
        }

        private RectTransform EnsureLayer(bool atTop)
        {
            var layer = atTop ? topLayer : bottomLayer;
            if (layer != null)
                return layer;

            var viewport = ViewportRect;
            var go = new GameObject(atTop ? "StickyLayer_Top" : "StickyLayer_Bottom",
                typeof(RectTransform));
            layer = (RectTransform)go.transform;
            layer.SetParent(viewport, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = Vector2.zero;
            layer.offsetMax = Vector2.zero;
            layer.SetAsLastSibling(); // draw over the scrolled rows

            if (atTop) topLayer = layer;
            else bottomLayer = layer;
            return layer;
        }
    }
}
