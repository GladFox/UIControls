using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Skeleton placeholder with a sweeping shimmer. While <see cref="IsLoading"/> is true the
    /// skeleton (a set of grey "bone" rects) is shown with a highlight band sliding across it; when
    /// loading finishes the skeleton is hidden and the real content is revealed. The shimmer gradient
    /// is generated at runtime — no sprites or shaders needed.
    /// </summary>
    public sealed class UISkeletonLoaderControl : MonoBehaviour
    {
        [Header("Roots")]
        [SerializeField] private GameObject skeletonRoot;
        [SerializeField] private GameObject contentRoot;

        [Header("Shimmer")]
        [Tooltip("Moving highlight band (a RawImage). Travels horizontally across the skeleton.")]
        [SerializeField] private RawImage shimmer;
        [Tooltip("Rect that defines the horizontal travel range (usually the skeleton card).")]
        [SerializeField] private RectTransform travelArea;
        [Tooltip("Sweeps per second.")]
        [SerializeField] private float speed = 0.8f;

        [Header("State")]
        [SerializeField] private bool isLoading = true;

        private Texture2D shimmerTexture;
        private float t;

        public bool IsLoading => isLoading;

        private void Awake()
        {
            BuildShimmerTexture();
        }

        private void OnEnable()
        {
            ApplyState();
        }

        private void OnDisable()
        {
            if (shimmerTexture != null)
            {
                Destroy(shimmerTexture);
                shimmerTexture = null;
            }
        }

        private void Update()
        {
            if (!isLoading || shimmer == null || travelArea == null)
            {
                return;
            }

            t += Time.unscaledDeltaTime * speed;
            t = Mathf.Repeat(t, 1f);

            var width = travelArea.rect.width;
            var shimmerWidth = shimmer.rectTransform.rect.width;
            var from = -width * 0.5f - shimmerWidth * 0.5f;
            var to = width * 0.5f + shimmerWidth * 0.5f;

            var pos = shimmer.rectTransform.anchoredPosition;
            pos.x = Mathf.Lerp(from, to, t);
            shimmer.rectTransform.anchoredPosition = pos;
        }

        public void SetLoading(bool loading)
        {
            isLoading = loading;
            ApplyState();
        }

        private void ApplyState()
        {
            if (skeletonRoot != null)
            {
                skeletonRoot.SetActive(isLoading);
            }

            if (contentRoot != null)
            {
                contentRoot.SetActive(!isLoading);
            }
        }

        private void BuildShimmerTexture()
        {
            if (shimmer == null)
            {
                return;
            }

            const int width = 64;
            shimmerTexture = new Texture2D(width, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (var x = 0; x < width; x++)
            {
                var u = x / (float)(width - 1);
                // Soft band: alpha peaks in the middle, fades to the edges.
                var a = Mathf.Clamp01(1f - Mathf.Abs(u - 0.5f) * 2f);
                a = a * a * 0.35f;
                shimmerTexture.SetPixel(x, 0, new Color(1f, 1f, 1f, a));
            }

            shimmerTexture.Apply();
            shimmer.texture = shimmerTexture;
            shimmer.color = Color.white;
        }
    }
}
