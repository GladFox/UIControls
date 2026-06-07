using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// An HSV color picker: a saturation/value square plus a hue bar, with a live preview swatch and
    /// hex label. The square and bar textures are generated at runtime (no sprites/shaders needed).
    /// The control sits on a root that catches pointer input and routes it to whichever area the
    /// pointer is over. <see cref="OnColorChanged"/> fires as the color changes.
    /// </summary>
    public sealed class UIColorPickerControl : MonoBehaviour,
        IPointerDownHandler,
        IDragHandler
    {
        [Serializable]
        public sealed class ColorEvent : UnityEvent<Color>
        {
        }

        [Header("Saturation / Value square")]
        [SerializeField] private RawImage svImage;
        [SerializeField] private RectTransform svRect;
        [SerializeField] private RectTransform svCursor;

        [Header("Hue bar")]
        [SerializeField] private RawImage hueImage;
        [SerializeField] private RectTransform hueRect;
        [SerializeField] private RectTransform hueHandle;

        [Header("Output")]
        [SerializeField] private Image preview;
        [SerializeField] private TMP_Text hexLabel;

        [Header("Initial")]
        [SerializeField] private Color initialColor = new Color(0.24f, 0.55f, 0.95f, 1f);

        [Header("Events")]
        [SerializeField] private ColorEvent onColorChanged = new ColorEvent();

        private const int SvSize = 128;
        private const int HueSize = 256;

        private Texture2D svTexture;
        private Texture2D hueTexture;
        private float hue, sat, val;
        private bool built;

        public ColorEvent OnColorChanged => onColorChanged;
        public Color Color => UnityEngine.Color.HSVToRGB(hue, sat, val);

        private void OnEnable()
        {
            UnityEngine.Color.RGBToHSV(initialColor, out hue, out sat, out val);
            Build();
            RebuildSvTexture();
            ApplyVisuals(false);
        }

        private void OnDisable()
        {
            if (svTexture != null)
            {
                Destroy(svTexture);
                svTexture = null;
            }

            if (hueTexture != null)
            {
                Destroy(hueTexture);
                hueTexture = null;
            }

            built = false;
        }

        public void SetColor(Color color, bool notify = true)
        {
            UnityEngine.Color.RGBToHSV(color, out hue, out sat, out val);
            if (!built)
            {
                Build();
            }

            RebuildSvTexture();
            ApplyVisuals(notify);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Route(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Route(eventData);
        }

        private void Route(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            if (svRect != null &&
                RectTransformUtility.RectangleContainsScreenPoint(svRect, eventData.position, eventData.pressEventCamera))
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        svRect, eventData.position, eventData.pressEventCamera, out var local))
                {
                    // local is relative to the rect's pivot; normalize via the rect bounds so this
                    // works regardless of the pivot.
                    var r = svRect.rect;
                    sat = r.width > Mathf.Epsilon ? Mathf.Clamp01((local.x - r.xMin) / r.width) : 0f;
                    val = r.height > Mathf.Epsilon ? Mathf.Clamp01((local.y - r.yMin) / r.height) : 0f;
                    ApplyVisuals(true);
                }

                return;
            }

            if (hueRect != null &&
                RectTransformUtility.RectangleContainsScreenPoint(hueRect, eventData.position, eventData.pressEventCamera))
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        hueRect, eventData.position, eventData.pressEventCamera, out var local))
                {
                    var r = hueRect.rect;
                    hue = r.width > Mathf.Epsilon ? Mathf.Clamp01((local.x - r.xMin) / r.width) : 0f;
                    RebuildSvTexture();
                    ApplyVisuals(true);
                }
            }
        }

        private void Build()
        {
            if (built)
            {
                return;
            }

            hueTexture = new Texture2D(HueSize, 1, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Clamp };
            for (var x = 0; x < HueSize; x++)
            {
                hueTexture.SetPixel(x, 0, UnityEngine.Color.HSVToRGB((float)x / (HueSize - 1), 1f, 1f));
            }

            hueTexture.Apply();
            if (hueImage != null)
            {
                hueImage.texture = hueTexture;
            }

            svTexture = new Texture2D(SvSize, SvSize, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Clamp };
            if (svImage != null)
            {
                svImage.texture = svTexture;
            }

            built = true;
        }

        private void RebuildSvTexture()
        {
            if (svTexture == null)
            {
                return;
            }

            for (var y = 0; y < SvSize; y++)
            {
                var v = (float)y / (SvSize - 1);
                for (var x = 0; x < SvSize; x++)
                {
                    var s = (float)x / (SvSize - 1);
                    svTexture.SetPixel(x, y, UnityEngine.Color.HSVToRGB(hue, s, v));
                }
            }

            svTexture.Apply();
        }

        private void ApplyVisuals(bool notify)
        {
            var color = Color;

            if (preview != null)
            {
                preview.color = color;
            }

            if (hexLabel != null)
            {
                hexLabel.text = "#" + ColorUtility.ToHtmlStringRGB(color);
            }

            if (svCursor != null && svRect != null)
            {
                svCursor.anchoredPosition = new Vector2(
                    (sat - 0.5f) * svRect.rect.width,
                    (val - 0.5f) * svRect.rect.height);
            }

            if (hueHandle != null && hueRect != null)
            {
                var pos = hueHandle.anchoredPosition;
                pos.x = (hue - 0.5f) * hueRect.rect.width;
                hueHandle.anchoredPosition = pos;
            }

            if (notify)
            {
                onColorChanged?.Invoke(color);
            }
        }
    }
}
