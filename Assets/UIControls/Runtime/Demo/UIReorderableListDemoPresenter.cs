using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIReorderableListDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIReorderableListControl list;
        [SerializeField] private TMP_Text statusLabel;

        private void OnEnable()
        {
            if (list != null) list.OnReordered.AddListener(HandleReordered);
        }

        private void OnDisable()
        {
            if (list != null) list.OnReordered.RemoveListener(HandleReordered);
        }

        private void HandleReordered(int from, int to)
        {
            if (statusLabel != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture, "Moved row {0} -> {1}", from + 1, to + 1);
            }
        }
    }
}
