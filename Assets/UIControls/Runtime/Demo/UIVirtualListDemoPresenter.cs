using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Demo
{
    public sealed class UIVirtualListDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIVirtualListControl list;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private int itemCount = 10000;

        private static readonly Color EvenColor = new Color(0.18f, 0.22f, 0.31f, 1f);
        private static readonly Color OddColor = new Color(0.15f, 0.18f, 0.26f, 1f);

        private void OnEnable()
        {
            if (list != null)
            {
                list.SetItems(itemCount, Bind);
            }
        }

        private void Bind(int index, RectTransform cell)
        {
            var label = cell.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = string.Format(CultureInfo.InvariantCulture, "Item #{0:n0}", index);
            }

            var image = cell.GetComponent<Image>();
            if (image != null)
            {
                image.color = (index & 1) == 0 ? EvenColor : OddColor;
            }
        }

        private void Update()
        {
            if (statusLabel != null && list != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:n0} items · only {1} cells instantiated (recycled while scrolling)",
                    list.ItemCount,
                    list.ActiveCellCount);
            }
        }
    }
}
