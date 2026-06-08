using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UITabBarDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UITabBarControl tabBar;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] tabNames;

        private void OnEnable()
        {
            if (tabBar != null) tabBar.OnTabChanged.AddListener(HandleTab);
            HandleTab(tabBar != null ? tabBar.SelectedIndex : 0);
        }

        private void OnDisable()
        {
            if (tabBar != null) tabBar.OnTabChanged.RemoveListener(HandleTab);
        }

        private void HandleTab(int index)
        {
            if (statusLabel == null) return;
            var name = tabNames != null && index >= 0 && index < tabNames.Length ? tabNames[index] : "#" + index;
            statusLabel.text = "Active tab: " + name;
        }
    }
}
