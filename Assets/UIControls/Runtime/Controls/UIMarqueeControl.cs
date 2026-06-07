using TMPro;
using UnityEngine;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Scrolling marquee for text that is too wide for its viewport. In <see cref="Mode.Loop"/> it
    /// runs like a news ticker (text exits left, re-enters right); in <see cref="Mode.PingPong"/> it
    /// slides to the end and back. By default it only scrolls when the text actually overflows.
    /// Put a RectMask2D on the viewport so the text clips at the edges.
    /// </summary>
    public sealed class UIMarqueeControl : MonoBehaviour
    {
        public enum Mode
        {
            Loop,
            PingPong,
        }

        [Header("Targets")]
        [SerializeField] private RectTransform viewport;
        [Tooltip("Label to scroll. Should be left-anchored with a left pivot.")]
        [SerializeField] private TMP_Text label;

        [Header("Behaviour")]
        [SerializeField] private Mode mode = Mode.Loop;
        [Tooltip("Scroll speed in pixels per second.")]
        [SerializeField] private float speed = 80f;
        [Tooltip("Only scroll when the text is wider than the viewport.")]
        [SerializeField] private bool onlyWhenOverflowing = true;

        private float loopX;
        private float pingTime;
        private bool initialized;

        public string Text
        {
            get => label != null ? label.text : string.Empty;
            set
            {
                if (label != null)
                {
                    label.text = value;
                }

                Reset();
            }
        }

        private void OnEnable()
        {
            Reset();
        }

        private void Reset()
        {
            loopX = viewport != null ? viewport.rect.width : 0f;
            pingTime = 0f;
            initialized = false;
            SetX(0f);
        }

        private void Update()
        {
            if (viewport == null || label == null)
            {
                return;
            }

            var textWidth = label.preferredWidth;
            var viewWidth = viewport.rect.width;

            if (onlyWhenOverflowing && textWidth <= viewWidth)
            {
                SetX(0f);
                return;
            }

            var dt = Time.unscaledDeltaTime;

            if (mode == Mode.Loop)
            {
                if (!initialized)
                {
                    loopX = viewWidth;
                    initialized = true;
                }

                loopX -= speed * dt;
                if (loopX <= -textWidth)
                {
                    loopX = viewWidth;
                }

                SetX(loopX);
            }
            else
            {
                var range = Mathf.Max(0f, textWidth - viewWidth);
                pingTime += speed * dt;
                SetX(-Mathf.PingPong(pingTime, range));
            }
        }

        private void SetX(float x)
        {
            if (label == null)
            {
                return;
            }

            var pos = label.rectTransform.anchoredPosition;
            pos.x = x;
            label.rectTransform.anchoredPosition = pos;
        }
    }
}
