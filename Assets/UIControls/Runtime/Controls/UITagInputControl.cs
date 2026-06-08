using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Token / tag input: type a word and press Enter to turn it into a removable chip. Each chip has
    /// an × button; the running list is exposed via <see cref="Tags"/> and changes raise
    /// <see cref="OnTagsChanged"/>. Chips are cloned from <see cref="chipTemplate"/> (an inactive
    /// prototype under <see cref="chipsContainer"/>). Unlike a fixed ChipGroup, tags are created on the
    /// fly.
    /// </summary>
    public sealed class UITagInputControl : MonoBehaviour
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private RectTransform chipsContainer;
        [Tooltip("Inactive prototype chip: a root with a 'Label' TMP child and a 'Remove' UIButtonControl child.")]
        [SerializeField] private GameObject chipTemplate;

        [Header("Behaviour")]
        [SerializeField] private bool allowDuplicates;
        [SerializeField] private int maxTags = 12;

        [Header("Events")]
        [SerializeField] private IntEvent onTagsChanged = new IntEvent();

        private readonly List<string> tags = new List<string>();
        private readonly List<GameObject> chips = new List<GameObject>();

        public IntEvent OnTagsChanged => onTagsChanged;
        public IReadOnlyList<string> Tags => tags;

        private void Awake()
        {
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(OnSubmit);
            }

            if (chipTemplate != null)
            {
                chipTemplate.SetActive(false);
            }
        }

        private void OnSubmit(string text)
        {
            AddTag(text);
            if (inputField != null)
            {
                inputField.text = string.Empty;
                inputField.ActivateInputField();
            }
        }

        public void AddTag(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || chipTemplate == null || chipsContainer == null)
            {
                return;
            }

            var tag = raw.Trim();
            if (tags.Count >= maxTags)
            {
                return;
            }

            if (!allowDuplicates && tags.Contains(tag))
            {
                return;
            }

            tags.Add(tag);

            var chip = Instantiate(chipTemplate, chipsContainer);
            chip.SetActive(true);
            chip.name = "Chip_" + tag;

            var label = chip.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (label != null) label.text = tag;

            var removeButton = chip.transform.Find("Remove")?.GetComponent<UIButtonControl>();
            if (removeButton != null)
            {
                var captured = chip;
                var capturedTag = tag;
                removeButton.OnClick.AddListener(() => RemoveChip(captured, capturedTag));
            }

            chips.Add(chip);
            onTagsChanged.Invoke(tags.Count);
        }

        private void RemoveChip(GameObject chip, string tag)
        {
            tags.Remove(tag);
            chips.Remove(chip);
            Destroy(chip);
            onTagsChanged.Invoke(tags.Count);
        }

        public void Clear()
        {
            foreach (var chip in chips)
            {
                if (chip != null) Destroy(chip);
            }

            chips.Clear();
            tags.Clear();
            onTagsChanged.Invoke(0);
        }
    }
}
