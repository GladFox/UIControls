using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIRadialMenuDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIRadialMenuControl menu;
        [SerializeField] private UIButtonControl toggleButton;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] itemNames;

        private void OnEnable()
        {
            if (toggleButton != null) toggleButton.OnClick.AddListener(Toggle);
            if (menu != null) menu.OnItemSelected.AddListener(HandleSelected);
        }

        private void OnDisable()
        {
            if (toggleButton != null) toggleButton.OnClick.RemoveListener(Toggle);
            if (menu != null) menu.OnItemSelected.RemoveListener(HandleSelected);
        }

        private void Toggle() => menu?.Toggle();

        private void HandleSelected(int index)
        {
            if (statusLabel == null)
            {
                return;
            }

            var name = itemNames != null && index >= 0 && index < itemNames.Length
                ? itemNames[index]
                : "#" + index;
            statusLabel.text = "Selected: " + name;
        }
    }
}
