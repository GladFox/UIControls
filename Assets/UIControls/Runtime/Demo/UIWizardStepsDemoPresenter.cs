using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIWizardStepsDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIWizardStepsControl wizard;
        [SerializeField] private UIButtonControl backButton;
        [SerializeField] private UIButtonControl nextButton;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string[] stepTitles;

        private void OnEnable()
        {
            if (backButton != null) backButton.OnClick.AddListener(Back);
            if (nextButton != null) nextButton.OnClick.AddListener(Next);
            if (wizard != null) wizard.OnStepChanged.AddListener(HandleStep);
            HandleStep(wizard != null ? wizard.CurrentStep : 0);
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.OnClick.RemoveListener(Back);
            if (nextButton != null) nextButton.OnClick.RemoveListener(Next);
            if (wizard != null) wizard.OnStepChanged.RemoveListener(HandleStep);
        }

        private void Back() => wizard?.Previous();

        private void Next() => wizard?.Next();

        private void HandleStep(int step)
        {
            if (statusLabel == null || wizard == null) return;
            var title = stepTitles != null && step >= 0 && step < stepTitles.Length ? stepTitles[step] : string.Empty;
            statusLabel.text = string.Format(
                CultureInfo.InvariantCulture, "Step {0} of {1}: {2}", step + 1, wizard.StepCount, title);
        }
    }
}
