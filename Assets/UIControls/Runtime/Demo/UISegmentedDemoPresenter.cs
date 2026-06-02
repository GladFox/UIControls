using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UISegmentedDemoPresenter : MonoBehaviour
    {
        [Header("View-switching segmented control")]
        [SerializeField] private UISegmentedControl viewSegmented;
        [SerializeField] private GameObject[] views = System.Array.Empty<GameObject>();
        [SerializeField] private TMP_Text viewStatusLabel;

        [Header("Event-only segmented control")]
        [SerializeField] private UISegmentedControl eventSegmented;
        [SerializeField] private TMP_Text eventStatusLabel;

        private void OnEnable()
        {
            if (viewSegmented != null)
            {
                viewSegmented.OnSelectionChanged.AddListener(HandleViewChanged);
                HandleViewChanged(viewSegmented.SelectedIndex);
            }

            if (eventSegmented != null)
            {
                eventSegmented.OnSelectionChanged.AddListener(HandleEventChanged);
                HandleEventChanged(eventSegmented.SelectedIndex);
            }
        }

        private void OnDisable()
        {
            if (viewSegmented != null)
            {
                viewSegmented.OnSelectionChanged.RemoveListener(HandleViewChanged);
            }

            if (eventSegmented != null)
            {
                eventSegmented.OnSelectionChanged.RemoveListener(HandleEventChanged);
            }
        }

        private void HandleViewChanged(int index)
        {
            for (var i = 0; i < views.Length; i++)
            {
                if (views[i] != null && views[i].activeSelf != (i == index))
                {
                    views[i].SetActive(i == index);
                }
            }

            if (viewStatusLabel != null)
            {
                viewStatusLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "View segmented: showing view {0}",
                    index + 1);
            }
        }

        private void HandleEventChanged(int index)
        {
            if (eventStatusLabel == null)
            {
                return;
            }

            eventStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "Event-only segmented: index = {0}",
                index);
        }
    }
}
