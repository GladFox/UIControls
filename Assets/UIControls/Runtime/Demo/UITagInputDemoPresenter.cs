using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UITagInputDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UITagInputControl tagInput;
        [SerializeField] private UIButtonControl clearButton;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (tagInput != null) tagInput.OnTagsChanged.AddListener(HandleCount);
            if (clearButton != null) clearButton.OnClick.AddListener(ClearTags);
            HandleCount(tagInput != null ? tagInput.Tags.Count : 0);
        }

        private void OnDisable()
        {
            if (tagInput != null) tagInput.OnTagsChanged.RemoveListener(HandleCount);
            if (clearButton != null) clearButton.OnClick.RemoveListener(ClearTags);
        }

        private void ClearTags() => tagInput?.Clear();

        private void HandleCount(int count)
        {
            if (statusLabel == null || tagInput == null) return;
            statusLabel.text = count == 0
                ? "No tags yet — type and press Enter."
                : string.Format(CultureInfo.InvariantCulture, "{0} tag(s): {1}", count, string.Join(", ", tagInput.Tags));
        }
    }
}
