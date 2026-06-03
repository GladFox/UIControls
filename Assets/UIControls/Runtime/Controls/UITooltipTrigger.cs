using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Attach to a hoverable element. On pointer enter (after a delay) it asks the shared
    /// <see cref="UITooltipControl"/> to show <see cref="text"/> next to this element; pointer exit
    /// hides it. Optional long-press shows it on touch.
    ///
    /// NOTE: this lives in its own file (matching the class name) so Unity can serialize it as a
    /// component — a MonoBehaviour in a file named after a different class shows up as a missing
    /// script in the inspector.
    /// </summary>
    public sealed class UITooltipTrigger : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField] private UITooltipControl tooltip;
        [TextArea]
        [SerializeField] private string text = "Tooltip";
        [SerializeField] private UITooltipControl.Placement placement = UITooltipControl.Placement.Top;
        [Min(0f)]
        [SerializeField] private float hoverDelay = 0.35f;
        [Tooltip("Also show on touch long-press.")]
        [SerializeField] private bool longPress = true;
        [Min(0.1f)]
        [SerializeField] private float longPressTime = 0.5f;

        private Coroutine pending;

        public void SetTooltip(UITooltipControl control) => tooltip = control;
        public void SetText(string value) => text = value;

        public void OnPointerEnter(PointerEventData eventData)
        {
            ScheduleShow(hoverDelay);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CancelAndHide();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (longPress)
            {
                ScheduleShow(longPressTime);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (longPress)
            {
                CancelAndHide();
            }
        }

        private void OnDisable()
        {
            CancelAndHide();
        }

        private void ScheduleShow(float delay)
        {
            if (tooltip == null)
            {
                return;
            }

            if (pending != null)
            {
                StopCoroutine(pending);
            }

            pending = StartCoroutine(ShowAfter(delay));
        }

        private IEnumerator ShowAfter(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSecondsRealtime(delay);
            }

            pending = null;
            tooltip.Show(transform as RectTransform, text, placement);
        }

        private void CancelAndHide()
        {
            if (pending != null)
            {
                StopCoroutine(pending);
                pending = null;
            }

            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }
    }
}
