using TMPro;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    /// <summary>
    /// Fills the UIStickyList demo with grouped rows: five sections with sticky-top
    /// headers and one sticky-bottom total row at the end of the list.
    /// </summary>
    public sealed class UIStickyListDemoPresenter : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject headerTemplate; // has UIStickyItemControl (Top)
        [SerializeField] private GameObject rowTemplate;
        [SerializeField] private GameObject footerTemplate; // has UIStickyItemControl (Bottom)

        private static readonly (string header, string[] rows)[] Sections =
        {
            ("Weapons",   new[] { "Longsword", "Warhammer", "Recurve Bow", "Dagger" }),
            ("Armor",     new[] { "Iron Helm", "Chainmail", "Tower Shield", "Greaves" }),
            ("Potions",   new[] { "Health Potion", "Mana Potion", "Antidote", "Elixir" }),
            ("Materials", new[] { "Iron Ore", "Leather", "Oak Wood", "Crystal Shard" }),
            ("Quest",     new[] { "Ancient Key", "Sealed Letter", "Dragon Scale", "Torn Map" }),
        };

        private void Start()
        {
            if (content == null)
                return;

            var total = 0;
            foreach (var (header, rows) in Sections)
            {
                Spawn(headerTemplate, header);
                foreach (var row in rows)
                {
                    Spawn(rowTemplate, row);
                    total++;
                }
            }

            Spawn(footerTemplate, $"Total items: {total}");
        }

        private void Spawn(GameObject template, string label)
        {
            if (template == null)
                return;

            var clone = Instantiate(template, content, false);
            clone.SetActive(true);
            var text = clone.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = label;
        }
    }
}
