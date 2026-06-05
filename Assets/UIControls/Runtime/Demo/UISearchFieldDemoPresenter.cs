using System;
using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UISearchFieldDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UISearchFieldControl search;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] source = Array.Empty<string>();

        private void OnEnable()
        {
            if (search != null)
            {
                search.SetSource(source);
                search.OnSearch.AddListener(HandleSearch);
                HandleSearch(search.Text);
            }
        }

        private void OnDisable()
        {
            if (search != null)
            {
                search.OnSearch.RemoveListener(HandleSearch);
            }
        }

        private void HandleSearch(string query)
        {
            if (statusLabel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(query))
            {
                statusLabel.text = "Type to search — suggestions appear after a short pause.";
                return;
            }

            var matches = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!string.IsNullOrEmpty(source[i]) &&
                    source[i].IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matches++;
                }
            }

            statusLabel.text = string.Format(
                CultureInfo.InvariantCulture, "{0} result(s) for \"{1}\"", matches, query);
        }
    }
}
