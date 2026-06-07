using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Material-style ripple: on pointer-down a soft circle spawns at the click point and expands
    /// while fading out, clipped to this element's bounds (put a RectMask2D on the same object). The
    /// circle sprite is generated at runtime. Drop this on any clickable surface for tactile feedback.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIRippleEffectControl : MonoBehaviour, IPointerDownHandler
    {
        [Header("Ripple")]
        [SerializeField] private Color rippleColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private float duration = 0.5f;
        [Tooltip("Ripple end diameter as a multiple of the larger element dimension.")]
        [SerializeField] private float sizeMultiplier = 1.6f;

        private RectTransform rectTransform;
        private Sprite circleSprite;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            BuildCircleSprite();
        }

        private void OnDisable()
        {
            // Live ripple tweens own their own targets and self-destruct; nothing to track here.
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null || circleSprite == null || rectTransform == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, eventData.position, eventData.pressEventCamera, out var local))
            {
                return;
            }

            SpawnRipple(local);
        }

        private void SpawnRipple(Vector2 localPoint)
        {
            var go = new GameObject("Ripple", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(rectTransform, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = localPoint;
            rect.sizeDelta = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = circleSprite;
            image.color = rippleColor;
            image.raycastTarget = false;

            var maxSize = Mathf.Max(rectTransform.rect.width, rectTransform.rect.height) * sizeMultiplier;
            var endColor = new Color(rippleColor.r, rippleColor.g, rippleColor.b, 0f);

            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Join(UIDOTweenUtility.TweenSizeDelta(rect, new Vector2(maxSize, maxSize), duration).SetEase(Ease.OutQuad));
            seq.Join(UIDOTweenUtility.TweenGraphicColor(image, endColor, duration).SetEase(Ease.OutQuad));
            seq.OnComplete(() => Destroy(go));
        }

        private void BuildCircleSprite()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var center = (size - 1) * 0.5f;
            var radius = size * 0.5f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    var a = Mathf.Clamp01(radius - d); // 1px antialiased edge
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }

            tex.Apply();
            circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
