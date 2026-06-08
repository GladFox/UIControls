using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIGaugeDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIGaugeControl gauge;
        [SerializeField] private UIButtonControl lowButton;
        [SerializeField] private UIButtonControl midButton;
        [SerializeField] private UIButtonControl highButton;
        [SerializeField] private UIButtonControl randomButton;

        private void OnEnable()
        {
            if (lowButton != null) lowButton.OnClick.AddListener(Low);
            if (midButton != null) midButton.OnClick.AddListener(Mid);
            if (highButton != null) highButton.OnClick.AddListener(High);
            if (randomButton != null) randomButton.OnClick.AddListener(Randomize);
        }

        private void OnDisable()
        {
            if (lowButton != null) lowButton.OnClick.RemoveListener(Low);
            if (midButton != null) midButton.OnClick.RemoveListener(Mid);
            if (highButton != null) highButton.OnClick.RemoveListener(High);
            if (randomButton != null) randomButton.OnClick.RemoveListener(Randomize);
        }

        private void Low() => gauge?.SetValue(15f);

        private void Mid() => gauge?.SetValue(60f);

        private void High() => gauge?.SetValue(95f);

        private void Randomize() => gauge?.SetValue(Random.Range(0f, 100f));
    }
}
