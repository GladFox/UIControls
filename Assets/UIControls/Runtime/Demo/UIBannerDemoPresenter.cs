using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIBannerDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIBannerControl banner;
        [SerializeField] private UIButtonControl infoButton;
        [SerializeField] private UIButtonControl successButton;
        [SerializeField] private UIButtonControl warningButton;
        [SerializeField] private UIButtonControl errorButton;

        private void OnEnable()
        {
            if (infoButton != null) infoButton.OnClick.AddListener(ShowInfo);
            if (successButton != null) successButton.OnClick.AddListener(ShowSuccess);
            if (warningButton != null) warningButton.OnClick.AddListener(ShowWarning);
            if (errorButton != null) errorButton.OnClick.AddListener(ShowError);
        }

        private void OnDisable()
        {
            if (infoButton != null) infoButton.OnClick.RemoveListener(ShowInfo);
            if (successButton != null) successButton.OnClick.RemoveListener(ShowSuccess);
            if (warningButton != null) warningButton.OnClick.RemoveListener(ShowWarning);
            if (errorButton != null) errorButton.OnClick.RemoveListener(ShowError);
        }

        private void ShowInfo() => banner?.Show(UIBannerControl.BannerType.Info, "Heads up — a new version of the editor is available.");

        private void ShowSuccess() => banner?.Show(UIBannerControl.BannerType.Success, "Your changes were saved successfully.");

        private void ShowWarning() => banner?.Show(UIBannerControl.BannerType.Warning, "Your trial expires in 3 days.");

        private void ShowError() => banner?.Show(UIBannerControl.BannerType.Error, "Couldn't connect to the server. Retrying…");
    }
}
