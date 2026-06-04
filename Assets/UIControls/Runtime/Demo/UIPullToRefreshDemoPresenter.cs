using System.Collections;
using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIPullToRefreshDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIPullToRefreshControl pull;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject rowTemplate;
        [SerializeField] private TMP_Text statusLabel;
        [Min(0f)]
        [SerializeField] private float fakeWorkSeconds = 1.4f;

        private int refreshCount;
        private int nextItemId = 100;

        private void OnEnable()
        {
            if (pull != null)
            {
                pull.OnRefresh.AddListener(HandleRefresh);
            }

            UpdateStatus();
        }

        private void OnDisable()
        {
            if (pull != null)
            {
                pull.OnRefresh.RemoveListener(HandleRefresh);
            }
        }

        private void HandleRefresh()
        {
            StartCoroutine(DoRefresh());
        }

        private IEnumerator DoRefresh()
        {
            yield return new WaitForSecondsRealtime(fakeWorkSeconds);

            if (content != null && rowTemplate != null)
            {
                var row = Instantiate(rowTemplate, content);
                row.SetActive(true);
                row.transform.SetAsFirstSibling();
                var label = row.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = string.Format(CultureInfo.InvariantCulture, "Fresh item #{0}", nextItemId++);
                }
            }

            refreshCount++;
            UpdateStatus();

            if (pull != null)
            {
                pull.EndRefreshing();
            }
        }

        private void UpdateStatus()
        {
            if (statusLabel != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture, "Pull the list down to refresh.  Refreshed {0}x", refreshCount);
            }
        }
    }
}
