using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIControls.Runtime.Demo
{
    /// <summary>
    /// Populates the UIStickyList demo scene with labelled rows and section headers.
    /// </summary>
    public sealed class UIStickyListDemoPresenter : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject sectionHeaderTemplate;
        [SerializeField] private GameObject rowTemplate;

        private static readonly string[][] Sections =
        {
            new[] { "Alpha",   "Apple",    "Avocado",   "Almond",     "Apricot"  },
            new[] { "Beta",    "Banana",   "Blueberry", "Breadfruit", "Boysenberry" },
            new[] { "Gamma",   "Grape",    "Guava",     "Gooseberry", "Grapefruit" },
            new[] { "Delta",   "Date",     "Durian",    "Damson",     "Dewberry"  },
            new[] { "Epsilon", "Eggplant", "Elderberry","Endive",     "Emblica"   },
        };

        private void Start()
        {
            if (content == null) return;

            for (var s = 0; s < Sections.Length; s++)
            {
                var section = Sections[s];
                SpawnHeader(section[0]);
                for (var r = 1; r < section.Length; r++)
                    SpawnRow(section[r]);
            }

            // Templates are only needed as prefabs — hide them
            if (sectionHeaderTemplate != null) sectionHeaderTemplate.SetActive(false);
            if (rowTemplate != null)           rowTemplate.SetActive(false);
        }

        private void SpawnHeader(string label)
        {
            if (sectionHeaderTemplate == null) return;
            var go = Instantiate(sectionHeaderTemplate, content, false);
            go.SetActive(true);
            var tmp = go.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = label;
        }

        private void SpawnRow(string label)
        {
            if (rowTemplate == null) return;
            var go = Instantiate(rowTemplate, content, false);
            go.SetActive(true);
            var tmp = go.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = label;
        }
    }
}
