using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Infinite (load-more) scroll for a vertical <see cref="ScrollRect"/>. When the user scrolls
    /// within <see cref="bottomThreshold"/> pixels of the bottom, <see cref="OnLoadMore"/> fires
    /// (once, while <see cref="HasMore"/> is true and not already loading). A footer shows a spinner
    /// while loading and an "all caught up" message when there is nothing left. The consumer appends
    /// rows and calls <see cref="EndLoadMore"/> to report whether more pages remain.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UIInfiniteScrollControl : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [Tooltip("Footer element (kept as the last child of content) that shows the loader / end message.")]
        [SerializeField] private RectTransform footer;
        [SerializeField] private RectTransform footerSpinner;
        [SerializeField] private TMP_Text footerLabel;

        [Header("Trigger")]
        [Tooltip("Fire OnLoadMore when the content bottom comes within this many pixels of the viewport bottom.")]
        [SerializeField] private float bottomThreshold = 220f;

        [Header("Spinner")]
        [SerializeField] private float spinSpeed = 320f;

        [Header("Text")]
        [SerializeField] private string loadingText = "Loading more…";
        [SerializeField] private string doneText = "You're all caught up";

        [Header("State")]
        [SerializeField] private bool hasMore = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onLoadMore = new UnityEvent();

        private bool loading;
        private float spinAngle;

        public UnityEvent OnLoadMore => onLoadMore;
        public bool IsLoading => loading;

        public bool HasMore
        {
            get => hasMore;
            set
            {
                hasMore = value;
                UpdateFooter();
            }
        }

        private void Awake()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }

            if (viewport == null && scrollRect != null)
            {
                viewport = scrollRect.viewport;
            }

            if (content == null && scrollRect != null)
            {
                content = scrollRect.content;
            }
        }

        private void OnEnable()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.AddListener(OnScrolled);
            }

            UpdateFooter();
        }

        private void OnDisable()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrolled);
            }
        }

        private void Update()
        {
            if (loading && footerSpinner != null)
            {
                spinAngle -= spinSpeed * Time.unscaledDeltaTime;
                footerSpinner.localEulerAngles = new Vector3(0f, 0f, spinAngle);
            }
        }

        /// <summary>Call when a load completes: appended rows + whether more pages remain.</summary>
        public void EndLoadMore(bool moreRemaining)
        {
            loading = false;
            hasMore = moreRemaining;

            if (footer != null)
            {
                footer.SetAsLastSibling(); // keep the footer below any newly appended rows
            }

            UpdateFooter();
        }

        /// <summary>Manually trigger a load (if possible).</summary>
        public void RequestLoadMore()
        {
            TryBeginLoad();
        }

        private void OnScrolled(Vector2 _)
        {
            if (DistanceToBottom() <= bottomThreshold)
            {
                TryBeginLoad();
            }
        }

        private void TryBeginLoad()
        {
            if (loading || !hasMore || content == null)
            {
                return;
            }

            loading = true;
            UpdateFooter();
            onLoadMore?.Invoke();
        }

        private float DistanceToBottom()
        {
            if (content == null || viewport == null)
            {
                return float.MaxValue;
            }

            var maxScroll = Mathf.Max(0f, content.rect.height - viewport.rect.height);
            var scroll = Mathf.Clamp(content.anchoredPosition.y, 0f, maxScroll);
            return maxScroll - scroll;
        }

        private void UpdateFooter()
        {
            if (footer == null)
            {
                return;
            }

            var show = loading || !hasMore;
            if (footer.gameObject.activeSelf != show)
            {
                footer.gameObject.SetActive(show);
            }

            if (footerSpinner != null && footerSpinner.gameObject.activeSelf != loading)
            {
                footerSpinner.gameObject.SetActive(loading);
            }

            if (footerLabel != null)
            {
                footerLabel.text = loading ? loadingText : doneText;
            }
        }
    }
}
