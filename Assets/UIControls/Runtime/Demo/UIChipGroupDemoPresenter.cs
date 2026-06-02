using System.Globalization;
using System.Text;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIChipGroupDemoPresenter : MonoBehaviour
    {
        [Header("Single-select (radio) group")]
        [SerializeField] private UIChipGroup radioGroup;
        [SerializeField] private string[] radioNames = System.Array.Empty<string>();
        [SerializeField] private TMP_Text radioStatusLabel;

        [Header("Multi-select (tags) group")]
        [SerializeField] private UIChipGroup tagsGroup;
        [SerializeField] private string[] tagNames = System.Array.Empty<string>();
        [SerializeField] private TMP_Text tagsStatusLabel;

        private void OnEnable()
        {
            if (radioGroup != null)
            {
                radioGroup.OnSelectionChanged.AddListener(HandleRadioChanged);
                UpdateRadioStatus();
            }

            if (tagsGroup != null)
            {
                tagsGroup.OnSelectionChanged.AddListener(HandleTagsChanged);
                UpdateTagsStatus();
            }
        }

        private void OnDisable()
        {
            if (radioGroup != null)
            {
                radioGroup.OnSelectionChanged.RemoveListener(HandleRadioChanged);
            }

            if (tagsGroup != null)
            {
                tagsGroup.OnSelectionChanged.RemoveListener(HandleTagsChanged);
            }
        }

        private void HandleRadioChanged(int index)
        {
            UpdateRadioStatus();
        }

        private void HandleTagsChanged(int index)
        {
            UpdateTagsStatus();
        }

        private void UpdateRadioStatus()
        {
            if (radioStatusLabel == null || radioGroup == null)
            {
                return;
            }

            var index = radioGroup.SelectedIndex;
            var name = index >= 0 && index < radioNames.Length ? radioNames[index] : "none";
            radioStatusLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "Single-select: {0} (index {1})",
                name,
                index);
        }

        private void UpdateTagsStatus()
        {
            if (tagsStatusLabel == null || tagsGroup == null)
            {
                return;
            }

            var builder = new StringBuilder("Multi-select: ");
            var any = false;
            for (var i = 0; i < tagNames.Length; i++)
            {
                if (tagsGroup.IsSelected(i))
                {
                    if (any)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(tagNames[i]);
                    any = true;
                }
            }

            if (!any)
            {
                builder.Append("(none)");
            }

            tagsStatusLabel.text = builder.ToString();
        }
    }
}
