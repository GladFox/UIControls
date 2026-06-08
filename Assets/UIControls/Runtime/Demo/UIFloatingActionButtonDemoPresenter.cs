using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIFloatingActionButtonDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIFloatingActionButtonControl fab;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] actionNames;

        private void OnEnable()
        {
            if (fab != null) fab.OnActionSelected.AddListener(HandleAction);
        }

        private void OnDisable()
        {
            if (fab != null) fab.OnActionSelected.RemoveListener(HandleAction);
        }

        private void HandleAction(int index)
        {
            if (statusLabel == null) return;
            var name = actionNames != null && index >= 0 && index < actionNames.Length ? actionNames[index] : "#" + index;
            statusLabel.text = "Action: " + name;
        }
    }
}
