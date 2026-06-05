using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A search field: a text input with a clear (×) button that appears when there is text, a
    /// debounced <see cref="OnSearch"/> event (fires a short moment after the user stops typing),
    /// and a suggestions dropdown filtered from a provided source list. Choosing a suggestion fills
    /// the field and searches immediately.
    /// </summary>
    public sealed class UISearchFieldControl : MonoBehaviour
    {
        [Serializable]
        public sealed class SearchEvent : UnityEvent<string>
        {
        }

        [Header("Targets")]
        [SerializeField] private TMP_InputField inputField;
        [Tooltip("Shown only while the field has text; clicking it clears the field.")]
        [SerializeField] private UIButtonControl clearButton;
        [Tooltip("Dropdown container (expects a VerticalLayoutGroup + ContentSizeFitter). Hidden when empty.")]
        [SerializeField] private RectTransform suggestionsPanel;
        [Tooltip("Inactive suggestion-row prototype (a UIButtonControl with a TMP label), cloned into the pool.")]
        [SerializeField] private UIButtonControl suggestionTemplate;

        [Header("Behaviour")]
        [Min(0f)]
        [SerializeField] private float debounceSeconds = 0.3f;
        [SerializeField] private int maxSuggestions = 6;
        [SerializeField] private bool caseSensitive;

        [Header("Events")]
        [SerializeField] private SearchEvent onSearch = new SearchEvent();
        [SerializeField] private SearchEvent onSubmit = new SearchEvent();

        private readonly List<UIButtonControl> pool = new List<UIButtonControl>();
        private string[] source = Array.Empty<string>();
        private float debounceTimer;
        private bool debouncePending;

        public SearchEvent OnSearch => onSearch;
        public SearchEvent OnSubmit => onSubmit;
        public string Text => inputField != null ? inputField.text : string.Empty;

        private void Awake()
        {
            if (suggestionTemplate != null)
            {
                suggestionTemplate.gameObject.SetActive(false);
            }

            if (inputField != null)
            {
                inputField.onValueChanged.AddListener(HandleValueChanged);
                inputField.onSubmit.AddListener(HandleSubmit);
            }

            if (clearButton != null)
            {
                clearButton.OnClick.AddListener(Clear);
            }
        }

        private void OnDestroy()
        {
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveListener(HandleValueChanged);
                inputField.onSubmit.RemoveListener(HandleSubmit);
            }

            if (clearButton != null)
            {
                clearButton.OnClick.RemoveListener(Clear);
            }
        }

        private void OnEnable()
        {
            UpdateClearButton(Text);
            HideSuggestions();
        }

        private void Update()
        {
            if (!debouncePending)
            {
                return;
            }

            debounceTimer -= Time.unscaledDeltaTime;
            if (debounceTimer <= 0f)
            {
                debouncePending = false;
                onSearch?.Invoke(Text);
            }
        }

        public void SetSource(IEnumerable<string> items)
        {
            source = items != null ? new List<string>(items).ToArray() : Array.Empty<string>();
            RefreshSuggestions(Text);
        }

        public void Clear()
        {
            if (inputField == null)
            {
                return;
            }

            inputField.SetTextWithoutNotify(string.Empty);
            UpdateClearButton(string.Empty);
            HideSuggestions();
            debouncePending = false;
            onSearch?.Invoke(string.Empty);
        }

        public void Focus()
        {
            inputField?.ActivateInputField();
        }

        private void HandleValueChanged(string text)
        {
            UpdateClearButton(text);
            debouncePending = true;
            debounceTimer = debounceSeconds;
            RefreshSuggestions(text);
        }

        private void HandleSubmit(string text)
        {
            debouncePending = false;
            HideSuggestions();
            onSearch?.Invoke(text);
            onSubmit?.Invoke(text);
        }

        private void Choose(string suggestion)
        {
            if (inputField == null)
            {
                return;
            }

            inputField.SetTextWithoutNotify(suggestion);
            UpdateClearButton(suggestion);
            HideSuggestions();
            debouncePending = false;
            onSearch?.Invoke(suggestion);
        }

        private void UpdateClearButton(string text)
        {
            if (clearButton != null)
            {
                var show = !string.IsNullOrEmpty(text);
                if (clearButton.gameObject.activeSelf != show)
                {
                    clearButton.gameObject.SetActive(show);
                }
            }
        }

        private void RefreshSuggestions(string text)
        {
            if (suggestionsPanel == null || suggestionTemplate == null || string.IsNullOrEmpty(text) || source.Length == 0)
            {
                HideSuggestions();
                return;
            }

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var shown = 0;

            for (var i = 0; i < source.Length && shown < maxSuggestions; i++)
            {
                var candidate = source[i];
                if (string.IsNullOrEmpty(candidate) || candidate.IndexOf(text, comparison) < 0)
                {
                    continue;
                }

                var item = GetPooledItem(shown);
                var label = item.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = candidate;
                }

                var captured = candidate;
                item.OnClick.RemoveAllListeners();
                item.OnClick.AddListener(() => Choose(captured));
                item.gameObject.SetActive(true);
                shown++;
            }

            for (var i = shown; i < pool.Count; i++)
            {
                if (pool[i].gameObject.activeSelf)
                {
                    pool[i].gameObject.SetActive(false);
                }
            }

            suggestionsPanel.gameObject.SetActive(shown > 0);
        }

        private UIButtonControl GetPooledItem(int index)
        {
            while (pool.Count <= index)
            {
                var clone = Instantiate(suggestionTemplate, suggestionsPanel);
                clone.gameObject.SetActive(false);
                pool.Add(clone);
            }

            return pool[index];
        }

        private void HideSuggestions()
        {
            if (suggestionsPanel != null && suggestionsPanel.gameObject.activeSelf)
            {
                suggestionsPanel.gameObject.SetActive(false);
            }
        }
    }
}
