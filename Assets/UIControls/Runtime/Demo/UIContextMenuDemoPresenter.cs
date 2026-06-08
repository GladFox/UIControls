using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIContextMenuDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIContextMenuControl contextMenu;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] itemNames;

        private void OnEnable()
        {
            if (contextMenu != null) contextMenu.OnItemSelected.AddListener(HandleItem);
        }

        private void OnDisable()
        {
            if (contextMenu != null) contextMenu.OnItemSelected.RemoveListener(HandleItem);
        }

        private void HandleItem(int index)
        {
            if (statusLabel == null) return;
            var name = itemNames != null && index >= 0 && index < itemNames.Length ? itemNames[index] : "#" + index;
            statusLabel.text = "Chose: " + name;
        }
    }
}
