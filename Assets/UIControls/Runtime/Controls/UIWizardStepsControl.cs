using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Horizontal step indicator for wizards / multi-step flows: ① — ② — ③ with done / active /
    /// upcoming states. Completed steps show a check and the connector behind them fills with the
    /// accent colour. Drive with <see cref="CurrentStep"/>, <see cref="Next"/> and
    /// <see cref="Previous"/>; raises <see cref="OnStepChanged"/>.
    /// </summary>
    public sealed class UIWizardStepsControl : MonoBehaviour
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Per-step (parallel arrays)")]
        [SerializeField] private Graphic[] stepCircles;
        [SerializeField] private TMP_Text[] stepNumbers;
        [SerializeField] private TMP_Text[] stepTitles;
        [Tooltip("Connector between step i and i+1 (one fewer than steps).")]
        [SerializeField] private Graphic[] connectors;

        [Header("State")]
        [SerializeField] private int currentStep;

        [Header("Style")]
        [SerializeField] private Color doneColor = new Color(0.3f, 0.7f, 0.45f, 1f);
        [SerializeField] private Color activeColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color upcomingColor = new Color(0.22f, 0.28f, 0.4f, 1f);
        [SerializeField] private Color titleActiveColor = new Color(0.97f, 0.98f, 1f, 1f);
        [SerializeField] private Color titleMutedColor = new Color(0.6f, 0.66f, 0.78f, 1f);

        [Header("Events")]
        [SerializeField] private IntEvent onStepChanged = new IntEvent();

        public IntEvent OnStepChanged => onStepChanged;
        public int CurrentStep => currentStep;
        public int StepCount => stepCircles != null ? stepCircles.Length : 0;

        public int CurrentStepIndex
        {
            get => currentStep;
            set => GoTo(value);
        }

        private void OnEnable()
        {
            Render();
        }

        public void GoTo(int step, bool notify = true)
        {
            var clamped = Mathf.Clamp(step, 0, Mathf.Max(0, StepCount - 1));
            if (clamped == currentStep)
            {
                Render();
                return;
            }

            currentStep = clamped;
            Render();
            if (notify)
            {
                onStepChanged.Invoke(currentStep);
            }
        }

        public void Next() => GoTo(currentStep + 1);

        public void Previous() => GoTo(currentStep - 1);

        private void Render()
        {
            if (stepCircles == null)
            {
                return;
            }

            for (var i = 0; i < stepCircles.Length; i++)
            {
                var done = i < currentStep;
                var active = i == currentStep;

                if (stepCircles[i] != null)
                {
                    stepCircles[i].color = done ? doneColor : active ? activeColor : upcomingColor;
                }

                if (stepNumbers != null && i < stepNumbers.Length && stepNumbers[i] != null)
                {
                    stepNumbers[i].text = done ? "✓" : (i + 1).ToString();
                }

                if (stepTitles != null && i < stepTitles.Length && stepTitles[i] != null)
                {
                    stepTitles[i].color = (done || active) ? titleActiveColor : titleMutedColor;
                }
            }

            if (connectors != null)
            {
                for (var i = 0; i < connectors.Length; i++)
                {
                    if (connectors[i] != null)
                    {
                        connectors[i].color = i < currentStep ? doneColor : upcomingColor;
                    }
                }
            }
        }
    }
}
