using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Breadcrumb trail: Home › Section › Page, drawn into a fixed pool of crumb slots with separators
    /// between them. The last crumb is the current location (highlighted, non-clickable); earlier
    /// crumbs raise <see cref="OnCrumbSelected"/> when clicked. Call <see cref="SetPath"/> to update.
    /// </summary>
    public sealed class UIBreadcrumbsControl : MonoBehaviour
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Slots")]
        [SerializeField] private UIButtonControl[] crumbButtons;
        [SerializeField] private TMP_Text[] crumbLabels;
        [Tooltip("Separator objects shown between crumbs (one fewer than crumbs).")]
        [SerializeField] private GameObject[] separators;

        [Header("Style")]
        [SerializeField] private Color normalColor = new Color(0.62f, 0.7f, 0.85f, 1f);
        [SerializeField] private Color currentColor = new Color(0.97f, 0.98f, 1f, 1f);

        [Header("Content")]
        [SerializeField] private string[] path = { "Home", "Library", "Controls" };

        [Header("Events")]
        [SerializeField] private IntEvent onCrumbSelected = new IntEvent();

        public IntEvent OnCrumbSelected => onCrumbSelected;

        private void Awake()
        {
            if (crumbButtons != null)
            {
                for (var i = 0; i < crumbButtons.Length; i++)
                {
                    var index = i;
                    if (crumbButtons[i] != null)
                    {
                        crumbButtons[i].OnClick.AddListener(() => onCrumbSelected.Invoke(index));
                    }
                }
            }
        }

        private void OnEnable()
        {
            Render();
        }

        public void SetPath(params string[] crumbs)
        {
            path = crumbs;
            Render();
        }

        private void Render()
        {
            if (crumbButtons == null)
            {
                return;
            }

            var count = path != null ? path.Length : 0;

            for (var i = 0; i < crumbButtons.Length; i++)
            {
                var visible = i < count;
                if (crumbButtons[i] != null)
                {
                    crumbButtons[i].gameObject.SetActive(visible);
                }

                if (visible && crumbLabels != null && i < crumbLabels.Length && crumbLabels[i] != null)
                {
                    crumbLabels[i].text = path[i];
                    crumbLabels[i].color = i == count - 1 ? currentColor : normalColor;
                    crumbLabels[i].fontStyle = i == count - 1 ? FontStyles.Bold : FontStyles.Normal;
                }

                if (separators != null && i < separators.Length && separators[i] != null)
                {
                    // Separator i sits after crumb i, shown only when crumb i+1 exists.
                    separators[i].SetActive(i < count - 1);
                }
            }
        }
    }
}
