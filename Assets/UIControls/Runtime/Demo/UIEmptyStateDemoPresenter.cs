using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIEmptyStateDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIEmptyStateControl emptyState;
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private UIButtonControl clearButton;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (emptyState != null) emptyState.OnAction.AddListener(FillContent);
            if (clearButton != null) clearButton.OnClick.AddListener(ClearContent);
            ClearContent();
        }

        private void OnDisable()
        {
            if (emptyState != null) emptyState.OnAction.RemoveListener(FillContent);
            if (clearButton != null) clearButton.OnClick.RemoveListener(ClearContent);
        }

        private void FillContent()
        {
            if (contentRoot != null) contentRoot.SetActive(true);
            if (emptyState != null) emptyState.Hide();
            if (statusLabel != null) statusLabel.text = "Content added — 3 items.";
        }

        private void ClearContent()
        {
            if (contentRoot != null) contentRoot.SetActive(false);
            if (emptyState != null) emptyState.Show();
            if (statusLabel != null) statusLabel.text = "List is empty.";
        }
    }
}
