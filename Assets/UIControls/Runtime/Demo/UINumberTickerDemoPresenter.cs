using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UINumberTickerDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UINumberTickerControl ticker;
        [SerializeField] private UIButtonControl add100Button;
        [SerializeField] private UIButtonControl add2500Button;
        [SerializeField] private UIButtonControl randomButton;
        [SerializeField] private UIButtonControl resetButton;

        private void OnEnable()
        {
            if (add100Button != null) add100Button.OnClick.AddListener(Add100);
            if (add2500Button != null) add2500Button.OnClick.AddListener(Add2500);
            if (randomButton != null) randomButton.OnClick.AddListener(Randomize);
            if (resetButton != null) resetButton.OnClick.AddListener(ResetValue);
        }

        private void OnDisable()
        {
            if (add100Button != null) add100Button.OnClick.RemoveListener(Add100);
            if (add2500Button != null) add2500Button.OnClick.RemoveListener(Add2500);
            if (randomButton != null) randomButton.OnClick.RemoveListener(Randomize);
            if (resetButton != null) resetButton.OnClick.RemoveListener(ResetValue);
        }

        private void Add100() => ticker?.Add(100);

        private void Add2500() => ticker?.Add(2500);

        private void Randomize() => ticker?.SetValue(Random.Range(10000, 9999999));

        private void ResetValue() => ticker?.SetValue(0);
    }
}
