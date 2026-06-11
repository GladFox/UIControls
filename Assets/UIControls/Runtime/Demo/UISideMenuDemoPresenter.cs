using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UISideMenuDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UISideMenuControl sideMenu;
        [SerializeField] private UIButtonControl switchSideButton;
        [SerializeField] private TMP_Text switchSideLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] itemNames;
        [SerializeField] private UISideMenuControl.MenuSide side = UISideMenuControl.MenuSide.Left;

        private void OnEnable()
        {
            if (sideMenu != null) sideMenu.OnItemSelected.AddListener(HandleItem);
            if (switchSideButton != null) switchSideButton.OnClick.AddListener(SwitchSide);
            UpdateSideLabel();
        }

        private void OnDisable()
        {
            if (sideMenu != null) sideMenu.OnItemSelected.RemoveListener(HandleItem);
            if (switchSideButton != null) switchSideButton.OnClick.RemoveListener(SwitchSide);
        }

        private void SwitchSide()
        {
            // Cycle around the four edges: Left -> Top -> Right -> Bottom -> Left.
            switch (side)
            {
                case UISideMenuControl.MenuSide.Left: side = UISideMenuControl.MenuSide.Top; break;
                case UISideMenuControl.MenuSide.Top: side = UISideMenuControl.MenuSide.Right; break;
                case UISideMenuControl.MenuSide.Right: side = UISideMenuControl.MenuSide.Bottom; break;
                default: side = UISideMenuControl.MenuSide.Left; break;
            }

            if (sideMenu != null) sideMenu.SetSide(side);
            UpdateSideLabel();
        }

        private void UpdateSideLabel()
        {
            if (switchSideLabel != null)
            {
                switchSideLabel.text = "Side: " + side;
            }
        }

        private void HandleItem(int index)
        {
            if (statusLabel == null) return;
            var name = itemNames != null && index >= 0 && index < itemNames.Length ? itemNames[index] : "#" + index;
            statusLabel.text = "Selected: " + name;
        }
    }
}
