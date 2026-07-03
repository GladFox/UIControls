using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Wraps a vertical <see cref="ScrollRect"/> and makes items marked with
    /// <see cref="UIStickyItemControl"/> stick to the top or bottom of the viewport.
    /// When the next sticky item scrolls into the pinned one it pushes it out —
    /// classic iOS section-header behaviour.
    ///
    /// Setup:
    ///   • Place this component on the same GameObject as the <see cref="ScrollRect"/>.
    ///   • Assign <see cref="stickyTopZone"/> / <see cref="stickyBottomZone"/> — overlay
    ///     RectTransforms inside the viewport, anchored to its top / bottom edge.
    ///   • Add <see cref="UIStickyItemControl"/> to any direct children of the content
    ///     VerticalLayoutGroup that should stick.
    ///
    /// While an item is stuck it is re-parented into the zone and a same-size
    /// placeholder keeps its slot in the layout, so nothing shifts. The placeholder
    /// also marks the item's "natural position" for the stick/unstick test.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UIStickyListControl : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private ScrollRect scrollRect;
        [Tooltip("Overlay container inside the viewport, anchored to its top edge.")]
        [SerializeField] private RectTransform stickyTopZone;
        [Tooltip("Overlay container inside the viewport, anchored to its bottom edge.")]
        [SerializeField] private RectTransform stickyBottomZone;

        // Registered by UIStickyItemControl.OnEnable, kept in content sibling order
        private readonly List<UIStickyItemControl> _topItems    = new List<UIStickyItemControl>();
        private readonly List<UIStickyItemControl> _bottomItems = new List<UIStickyItemControl>();

        private RectTransform Viewport
        {
            get
            {
                if (scrollRect == null) return null;
                if (scrollRect.viewport != null) return scrollRect.viewport;
                return (RectTransform)scrollRect.transform;
            }
        }

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>();
        }

        private void OnDisable()
        {
            // Return everything to the layout so the hierarchy stays sane
            for (var i = 0; i < _topItems.Count; i++) Unstick(_topItems[i]);
            for (var i = 0; i < _bottomItems.Count; i++) Unstick(_bottomItems[i]);
        }

        private void LateUpdate()
        {
            if (scrollRect == null || scrollRect.content == null || Viewport == null)
                return;

            EvaluateEdge(_topItems, stickyTopZone, top: true);
            EvaluateEdge(_bottomItems, stickyBottomZone, top: false);
        }

        // ── Registration (called by UIStickyItemControl) ─────────────────────────

        public void Register(UIStickyItemControl item)
        {
            var list = item.Edge == UIStickyItemControl.StickyEdge.Top ? _topItems : _bottomItems;
            if (!list.Contains(item))
            {
                list.Add(item);
                list.Sort(SiblingOrder);
            }
        }

        public void Unregister(UIStickyItemControl item)
        {
            Unstick(item);
            _topItems.Remove(item);
            _bottomItems.Remove(item);
        }

        // ── Evaluation ───────────────────────────────────────────────────────────

        // Top edge: an item sticks while its natural top edge is scrolled above the
        // viewport top (edge distance < 0). The next sticky item below acts as the
        // pusher: as its own edge distance shrinks under the pinned item's height, the
        // pinned item slides out by the difference. Bottom edge is fully mirrored.
        private void EvaluateEdge(List<UIStickyItemControl> items, RectTransform zone, bool top)
        {
            if (zone == null || items.Count == 0) return;

            var count = items.Count;
            for (var k = 0; k < count; k++)
            {
                var i    = top ? k : count - 1 - k;
                var item = items[i];
                if (!IsValid(item)) continue;

                var edgeDistance = EdgeDistance(item, top);
                // Small epsilon so an item resting exactly on the edge doesn't flicker
                var shouldStick  = edgeDistance < -0.05f;

                if (shouldStick && !item.IsStuck)
                    Stick(item, zone);
                else if (!shouldStick && item.IsStuck)
                    Unstick(item);

                if (!item.IsStuck) continue;

                var itemHeight = item.StuckHeight;

                // Push-out: the next sticky item toward the list interior shoves this
                // one off the edge as it approaches (0 = fully pinned, -height = gone)
                var pushOut = 0f;
                var pusher  = NextValid(items, i, top ? +1 : -1);
                if (pusher != null)
                {
                    var pusherDistance = EdgeDistance(pusher, top);
                    // Clamp at -height: fully hidden items park just past the edge
                    pushOut = Mathf.Clamp(pusherDistance - itemHeight, -itemHeight, 0f);
                }

                PositionInZone(item, pushOut, itemHeight, top);
            }
        }

        // ── Stick / Unstick ──────────────────────────────────────────────────────

        private void Stick(UIStickyItemControl item, RectTransform zone)
        {
            var source = (RectTransform)item.transform;

            // Placeholder keeps the layout slot and marks the natural position
            var ph = new GameObject($"[StickyPlaceholder] {item.name}",
                typeof(RectTransform), typeof(LayoutElement));
            var phRect = (RectTransform)ph.transform;
            phRect.SetParent(source.parent, false);
            phRect.SetSiblingIndex(source.GetSiblingIndex());

            var le = ph.GetComponent<LayoutElement>();
            le.preferredWidth  = source.rect.width;
            le.preferredHeight = source.rect.height;
            le.minHeight       = source.rect.height;
            le.flexibleHeight  = 0f;

            item.Placeholder = phRect;
            item.IsStuck     = true;
            item.SaveLayoutPose(source);

            source.SetParent(zone, false);
        }

        private void Unstick(UIStickyItemControl item)
        {
            if (item == null || !item.IsStuck) return;

            var source = (RectTransform)item.transform;

            if (item.Placeholder != null)
            {
                source.SetParent(item.Placeholder.parent, false);
                source.SetSiblingIndex(item.Placeholder.GetSiblingIndex());

                // Deactivate before the deferred Destroy so the layout doesn't count
                // both the item and its placeholder for one frame
                item.Placeholder.gameObject.SetActive(false);
                Destroy(item.Placeholder.gameObject);
                item.Placeholder = null;
            }

            item.RestoreLayoutPose(source);
            item.IsStuck = false;

            if (source.parent is RectTransform parentRect)
                LayoutRebuilder.MarkLayoutForRebuild(parentRect);
        }

        // ── Positioning ──────────────────────────────────────────────────────────

        private static void PositionInZone(UIStickyItemControl item, float pushOut,
            float itemHeight, bool top)
        {
            var rect    = (RectTransform)item.transform;
            var anchorY = top ? 1f : 0f;

            rect.anchorMin = new Vector2(0f, anchorY);
            rect.anchorMax = new Vector2(1f, anchorY);
            rect.pivot     = new Vector2(0.5f, anchorY);
            rect.sizeDelta = new Vector2(0f, itemHeight);
            // pushOut ≤ 0: top items slide up (+y), bottom items slide down (−y)
            rect.anchoredPosition = new Vector2(0f, top ? -pushOut : pushOut);
        }

        // ── Geometry ─────────────────────────────────────────────────────────────

        private static readonly Vector3[] CornersBuffer = new Vector3[4];

        // Distance from the viewport edge to the item's natural leading edge, measured
        // into the viewport: 0 when the edges coincide, positive while the item is
        // inside, negative once it has scrolled past the edge.
        private float EdgeDistance(UIStickyItemControl item, bool top)
        {
            var viewport = Viewport;
            var natural  = NaturalRect(item);
            natural.GetWorldCorners(CornersBuffer);
            // Corners: 0 = bottom-left, 1 = top-left
            var world = top ? CornersBuffer[1] : CornersBuffer[0];
            var local = viewport.InverseTransformPoint(world);
            return top
                ? viewport.rect.yMax - local.y
                : local.y - viewport.rect.yMin;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        // While stuck the item lives in the zone; its placeholder marks the natural rect.
        private static RectTransform NaturalRect(UIStickyItemControl item)
        {
            return item.IsStuck && item.Placeholder != null
                ? item.Placeholder
                : (RectTransform)item.transform;
        }

        private static UIStickyItemControl NextValid(List<UIStickyItemControl> items, int from, int step)
        {
            for (var i = from + step; i >= 0 && i < items.Count; i += step)
            {
                if (IsValid(items[i]))
                    return items[i];
            }
            return null;
        }

        private static bool IsValid(UIStickyItemControl item)
            => item != null && item.gameObject.activeInHierarchy;

        private static int SiblingOrder(UIStickyItemControl a, UIStickyItemControl b)
            => NaturalRect(a).GetSiblingIndex().CompareTo(NaturalRect(b).GetSiblingIndex());
    }
}
