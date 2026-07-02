using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Wraps a vertical <see cref="ScrollRect"/> and makes items marked with
    /// <see cref="UIStickyItemControl"/> stick to the top or bottom of the viewport,
    /// pushing each other when they collide (classic iOS section-header behaviour).
    ///
    /// Setup:
    ///   • Place this component on the same GameObject as the <see cref="ScrollRect"/>.
    ///   • Assign <see cref="stickyTopZone"/> and <see cref="stickyBottomZone"/> — two
    ///     overlay RectTransforms anchored to the top and bottom edges of the viewport.
    ///   • Add <see cref="UIStickyItemControl"/> to any direct children of the content
    ///     VerticalLayoutGroup that should stick.
    ///
    /// The component re-parents a stuck item into the zone and leaves a same-size
    /// placeholder in its original position so the layout does not collapse.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UIStickyListControl : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private ScrollRect scrollRect;
        [Tooltip("Overlay container anchored to the top of the viewport.")]
        [SerializeField] private RectTransform stickyTopZone;
        [Tooltip("Overlay container anchored to the bottom of the viewport.")]
        [SerializeField] private RectTransform stickyBottomZone;

        // Items registered by UIStickyItemControl.OnEnable, in content sibling order
        private readonly List<UIStickyItemControl> _topItems    = new List<UIStickyItemControl>();
        private readonly List<UIStickyItemControl> _bottomItems = new List<UIStickyItemControl>();

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>();
        }

        private void LateUpdate()
        {
            if (scrollRect == null || scrollRect.content == null) return;

            EvaluateTopItems();
            EvaluateBottomItems();
        }

        // ── Registration ─────────────────────────────────────────────────────────

        public void Register(UIStickyItemControl item)
        {
            var list = item.Edge == UIStickyItemControl.StickyEdge.Top ? _topItems : _bottomItems;
            if (!list.Contains(item))
                list.Add(item);

            // Keep lists in top-to-bottom sibling order for push logic
            _topItems.Sort(SiblingOrder);
            _bottomItems.Sort(SiblingOrder);
        }

        public void Unregister(UIStickyItemControl item)
        {
            Unstick(item);
            _topItems.Remove(item);
            _bottomItems.Remove(item);
        }

        // ── Evaluation ───────────────────────────────────────────────────────────

        private void EvaluateTopItems()
        {
            if (stickyTopZone == null) return;

            // Accumulated height of already-stuck top items (push offset for the next one)
            float stackedHeight = 0f;

            for (var i = 0; i < _topItems.Count; i++)
            {
                var item = _topItems[i];
                if (!IsValid(item)) continue;

                var source     = GetSourceRect(item);
                var itemHeight = source.rect.height;

                // World-space top-Y of the item's natural position
                var naturalTopY = GetContentTopY(source);

                // The item should stick when its natural top would scroll above the zone top
                var zoneTopY = GetZoneTopY();
                var threshold = zoneTopY - stackedHeight;

                bool shouldStick = naturalTopY > threshold;

                if (shouldStick && !item.IsStuck)
                    Stick(item, source);
                else if (!shouldStick && item.IsStuck)
                    Unstick(item);

                if (item.IsStuck)
                {
                    // Position within the top zone, stacked below previous stuck items
                    PositionInZone(item, stickyTopZone, stackedHeight, itemHeight, isTop: true);
                    stackedHeight += itemHeight;
                }
            }
        }

        private void EvaluateBottomItems()
        {
            if (stickyBottomZone == null) return;

            float stackedHeight = 0f;

            // Evaluate in reverse order (lowest item first for bottom sticking)
            for (var i = _bottomItems.Count - 1; i >= 0; i--)
            {
                var item = _bottomItems[i];
                if (!IsValid(item)) continue;

                var source     = GetSourceRect(item);
                var itemHeight = source.rect.height;

                var naturalBottomY = GetContentBottomY(source);
                var zoneBottomY    = GetZoneBottomY();
                var threshold      = zoneBottomY + stackedHeight;

                bool shouldStick = naturalBottomY < threshold;

                if (shouldStick && !item.IsStuck)
                    Stick(item, source);
                else if (!shouldStick && item.IsStuck)
                    Unstick(item);

                if (item.IsStuck)
                {
                    PositionInZone(item, stickyBottomZone, stackedHeight, itemHeight, isTop: false);
                    stackedHeight += itemHeight;
                }
            }
        }

        // ── Stick / Unstick ──────────────────────────────────────────────────────

        private void Stick(UIStickyItemControl item, RectTransform source)
        {
            var zone = item.Edge == UIStickyItemControl.StickyEdge.Top ? stickyTopZone : stickyBottomZone;
            if (zone == null) return;

            // Create placeholder with the same size so the layout doesn't shift
            var ph = new GameObject($"[StickyPlaceholder_{item.name}]", typeof(RectTransform), typeof(LayoutElement));
            var phRect = ph.GetComponent<RectTransform>();
            phRect.SetParent(source.parent, false);
            phRect.SetSiblingIndex(source.GetSiblingIndex());

            var le = ph.GetComponent<LayoutElement>();
            le.preferredWidth  = source.rect.width;
            le.preferredHeight = source.rect.height;
            le.minHeight       = source.rect.height;
            le.flexibleHeight  = 0f;

            item.Placeholder = phRect;
            item.IsStuck     = true;

            // Re-parent into the zone; layout group no longer owns this transform
            source.SetParent(zone, true);
        }

        private void Unstick(UIStickyItemControl item)
        {
            if (!item.IsStuck) return;

            var source = (RectTransform)item.transform;

            if (item.Placeholder != null)
            {
                // Return item to its original position in the content list
                source.SetParent(item.Placeholder.parent, true);
                source.SetSiblingIndex(item.Placeholder.GetSiblingIndex());

                Object.Destroy(item.Placeholder.gameObject);
                item.Placeholder = null;
            }

            item.IsStuck = false;
        }

        // ── Positioning ──────────────────────────────────────────────────────────

        private static void PositionInZone(UIStickyItemControl item, RectTransform zone,
            float stackOffset, float itemHeight, bool isTop)
        {
            var rect = (RectTransform)item.transform;

            // Stretch width to fill the zone
            rect.anchorMin = new Vector2(0f, isTop ? 1f : 0f);
            rect.anchorMax = new Vector2(1f, isTop ? 1f : 0f);
            rect.pivot     = new Vector2(0.5f, isTop ? 1f : 0f);
            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(0f, itemHeight);

            if (isTop)
                rect.anchoredPosition = new Vector2(0f, -stackOffset);
            else
                rect.anchoredPosition = new Vector2(0f, stackOffset);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        // Returns the rect that represents the item's "natural" position in the content.
        // While stuck, the placeholder occupies that natural position.
        private RectTransform GetSourceRect(UIStickyItemControl item)
        {
            // While stuck the item lives in the zone; its placeholder is the natural rect
            if (item.IsStuck && item.Placeholder != null)
                return item.Placeholder;
            return (RectTransform)item.transform;
        }

        // Top edge of the source rect in viewport local-space Y (up = positive).
        private float GetContentTopY(RectTransform source)
        {
            var viewport   = scrollRect.viewport ?? scrollRect.GetComponentInChildren<RectMask2D>()?.rectTransform;
            if (viewport == null) return 0f;

            // Convert top-left of source to viewport local space
            var worldTopLeft = source.TransformPoint(new Vector3(-source.rect.width * source.pivot.x,
                source.rect.height * (1f - source.pivot.y), 0f));
            var local = viewport.InverseTransformPoint(worldTopLeft);

            // In uGUI viewport local space, positive Y is up and the top is at +height/2
            // We want distance from the top edge: positive when BELOW the top, negative when above
            return viewport.rect.height * 0.5f - local.y;
        }

        // Bottom edge Y relative to viewport (positive = below viewport top)
        private float GetContentBottomY(RectTransform source)
        {
            var viewport = scrollRect.viewport ?? scrollRect.GetComponentInChildren<RectMask2D>()?.rectTransform;
            if (viewport == null) return 0f;

            var worldBottomLeft = source.TransformPoint(new Vector3(-source.rect.width * source.pivot.x,
                -source.rect.height * source.pivot.y, 0f));
            var local = viewport.InverseTransformPoint(worldBottomLeft);

            return viewport.rect.height * 0.5f - local.y;
        }

        // Distance from top of viewport to bottom of stickyTopZone (= height of zone)
        private float GetZoneTopY()
        {
            return stickyTopZone != null ? stickyTopZone.rect.height : 0f;
        }

        // Distance from top of viewport to top of stickyBottomZone
        private float GetZoneBottomY()
        {
            var viewport = scrollRect.viewport;
            if (viewport == null || stickyBottomZone == null) return 0f;
            return viewport.rect.height - stickyBottomZone.rect.height;
        }

        private static bool IsValid(UIStickyItemControl item)
            => item != null && item.gameObject.activeSelf;

        private static int SiblingOrder(UIStickyItemControl a, UIStickyItemControl b)
        {
            // Compare by placeholder when stuck, otherwise by transform
            var ra = a.IsStuck && a.Placeholder != null ? a.Placeholder : (RectTransform)a.transform;
            var rb = b.IsStuck && b.Placeholder != null ? b.Placeholder : (RectTransform)b.transform;
            return ra.GetSiblingIndex().CompareTo(rb.GetSiblingIndex());
        }
    }
}
