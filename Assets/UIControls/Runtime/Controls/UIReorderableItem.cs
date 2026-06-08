using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Drag handle for a row inside a <see cref="UIReorderableListControl"/>. Forwards begin/drag/end
    /// pointer events to the parent list. Added automatically by the list when it rebuilds; you can
    /// also add it manually to restrict dragging to a specific handle graphic.
    /// </summary>
    public sealed class UIReorderableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private UIReorderableListControl list;

        private void Awake()
        {
            list = GetComponentInParent<UIReorderableListControl>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (list == null)
            {
                list = GetComponentInParent<UIReorderableListControl>();
            }

            list?.BeginDrag(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            list?.Drag(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            list?.EndDrag(this);
        }
    }
}
