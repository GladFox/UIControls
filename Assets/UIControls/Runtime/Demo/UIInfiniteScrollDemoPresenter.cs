using System.Collections;
using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIInfiniteScrollDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIInfiniteScrollControl list;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject rowTemplate;
        [SerializeField] private RectTransform footer;
        [SerializeField] private TMP_Text statusLabel;

        [SerializeField] private int initialItems = 14;
        [SerializeField] private int batchSize = 10;
        [SerializeField] private int maxItems = 80;
        [Min(0f)]
        [SerializeField] private float loadSeconds = 1f;

        private int loaded;

        private void OnEnable()
        {
            if (list != null)
            {
                list.OnLoadMore.AddListener(HandleLoadMore);
            }
        }

        private void OnDisable()
        {
            if (list != null)
            {
                list.OnLoadMore.RemoveListener(HandleLoadMore);
            }
        }

        private void Start()
        {
            AddRows(initialItems);
            if (list != null)
            {
                list.HasMore = loaded < maxItems;
            }

            UpdateStatus();
        }

        private void HandleLoadMore()
        {
            StartCoroutine(LoadBatch());
        }

        private IEnumerator LoadBatch()
        {
            yield return new WaitForSecondsRealtime(loadSeconds);

            var add = Mathf.Min(batchSize, maxItems - loaded);
            AddRows(add);

            if (list != null)
            {
                list.EndLoadMore(loaded < maxItems);
            }

            UpdateStatus();
        }

        private void AddRows(int count)
        {
            if (content == null || rowTemplate == null)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                loaded++;
                var row = Instantiate(rowTemplate, content);
                row.SetActive(true);
                var label = row.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = string.Format(CultureInfo.InvariantCulture, "Item #{0}", loaded);
                }
            }

            if (footer != null)
            {
                footer.SetAsLastSibling();
            }
        }

        private void UpdateStatus()
        {
            if (statusLabel != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Loaded {0} of {1} — scroll to the bottom to load more",
                    loaded, maxItems);
            }
        }
    }
}
