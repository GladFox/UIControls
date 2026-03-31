using UIControls.Runtime.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Demo
{
    public sealed class UIProgressBarDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIProgressBarControl progressBarControl;
        [SerializeField] private UIProgressBarControl segmentFillProgressBarControl;
        [SerializeField] private UIButtonControl damageButton;
        [SerializeField] private UIButtonControl heavyDamageButton;
        [SerializeField] private UIButtonControl healButton;
        [SerializeField] private UIButtonControl resetButton;
        [SerializeField] private UIToggleControl autoDamageToggle;
        [SerializeField] private UIToggleControl autoHealToggle;
        [SerializeField] private Text statusLabel;

        [Header("Demo Values")]
        [Range(0f, 1f)]
        [SerializeField] private float startValue = 1f;

        [Range(0.01f, 1f)]
        [SerializeField] private float damageStep = 0.12f;

        [Range(0.01f, 1f)]
        [SerializeField] private float heavyDamageStep = 0.35f;

        [Range(0.01f, 1f)]
        [SerializeField] private float healStep = 0.08f;

        [Header("Auto Damage")]
        [Range(0.1f, 10f)]
        [SerializeField] private float autoDamageInterval = 1.2f;

        [Range(0.01f, 1f)]
        [SerializeField] private float autoHealStep = 0.06f;

        [SerializeField] private bool randomizeAutoDamage = true;

        private float autoActionTimer;
        private bool suppressToggleCallbacks;

        private void OnEnable()
        {
            if (damageButton != null)
            {
                damageButton.OnClick.AddListener(HandleDamageClick);
            }

            if (heavyDamageButton != null)
            {
                heavyDamageButton.OnClick.AddListener(HandleHeavyDamageClick);
            }

            if (healButton != null)
            {
                healButton.OnClick.AddListener(HandleHealClick);
            }

            if (resetButton != null)
            {
                resetButton.OnClick.AddListener(HandleResetClick);
            }

            if (autoDamageToggle != null)
            {
                autoDamageToggle.OnValueChanged.AddListener(HandleAutoDamageToggled);
                HandleAutoDamageToggled(autoDamageToggle.IsOn);
            }

            if (autoHealToggle != null)
            {
                autoHealToggle.OnValueChanged.AddListener(HandleAutoHealToggled);
                HandleAutoHealToggled(autoHealToggle.IsOn);
            }

            if (progressBarControl != null)
            {
                progressBarControl.OnValueChanged.AddListener(HandleProgressValueChanged);
                progressBarControl.OnSegmentCompleted.AddListener(HandleSegmentCompleted);
                progressBarControl.OnEchoStarted.AddListener(HandleEchoStarted);
                progressBarControl.OnEchoCompleted.AddListener(HandleEchoCompleted);
            }

            if (ResolveValueSource() != null)
            {
                ApplyValueToProgressBars(startValue, false, true);
            }
        }

        private void OnDisable()
        {
            if (damageButton != null)
            {
                damageButton.OnClick.RemoveListener(HandleDamageClick);
            }

            if (heavyDamageButton != null)
            {
                heavyDamageButton.OnClick.RemoveListener(HandleHeavyDamageClick);
            }

            if (healButton != null)
            {
                healButton.OnClick.RemoveListener(HandleHealClick);
            }

            if (resetButton != null)
            {
                resetButton.OnClick.RemoveListener(HandleResetClick);
            }

            if (autoDamageToggle != null)
            {
                autoDamageToggle.OnValueChanged.RemoveListener(HandleAutoDamageToggled);
            }

            if (autoHealToggle != null)
            {
                autoHealToggle.OnValueChanged.RemoveListener(HandleAutoHealToggled);
            }

            if (progressBarControl != null)
            {
                progressBarControl.OnValueChanged.RemoveListener(HandleProgressValueChanged);
                progressBarControl.OnSegmentCompleted.RemoveListener(HandleSegmentCompleted);
                progressBarControl.OnEchoStarted.RemoveListener(HandleEchoStarted);
                progressBarControl.OnEchoCompleted.RemoveListener(HandleEchoCompleted);
            }
        }

        private void Update()
        {
            if (ResolveValueSource() == null)
            {
                return;
            }

            var autoDamageEnabled = autoDamageToggle != null && autoDamageToggle.IsOn;
            var autoHealEnabled = autoHealToggle != null && autoHealToggle.IsOn;
            if (!autoDamageEnabled && !autoHealEnabled)
            {
                return;
            }

            autoActionTimer += Time.unscaledDeltaTime;
            if (autoActionTimer < autoDamageInterval)
            {
                return;
            }

            autoActionTimer = 0f;
            if (autoDamageEnabled)
            {
                var step = randomizeAutoDamage && Random.value > 0.5f ? heavyDamageStep : damageStep;
                ApplyDelta(-step, $"Auto Damage -{step:P0}");
                return;
            }

            ApplyDelta(autoHealStep, $"Auto Heal +{autoHealStep:P0}");
        }

        private void HandleDamageClick()
        {
            ApplyDelta(-damageStep, $"Damage -{damageStep:P0}");
        }

        private void HandleHeavyDamageClick()
        {
            ApplyDelta(-heavyDamageStep, $"Heavy Hit -{heavyDamageStep:P0}");
        }

        private void HandleHealClick()
        {
            ApplyDelta(healStep, $"Heal +{healStep:P0}");
        }

        private void HandleResetClick()
        {
            if (ResolveValueSource() == null)
            {
                return;
            }

            ApplyValueToProgressBars(startValue, true, true);
            SetStatus($"Reset to {startValue:P0}");
        }

        private void HandleAutoDamageToggled(bool isOn)
        {
            if (suppressToggleCallbacks)
            {
                return;
            }

            if (isOn && autoHealToggle != null && autoHealToggle.IsOn)
            {
                SetToggleWithoutCallback(autoHealToggle, false);
            }

            autoActionTimer = 0f;
            SetStatus(isOn ? "Auto damage enabled" : "Auto damage disabled");
        }

        private void HandleAutoHealToggled(bool isOn)
        {
            if (suppressToggleCallbacks)
            {
                return;
            }

            if (isOn && autoDamageToggle != null && autoDamageToggle.IsOn)
            {
                SetToggleWithoutCallback(autoDamageToggle, false);
            }

            autoActionTimer = 0f;
            SetStatus(isOn ? "Auto heal enabled" : "Auto heal disabled");
        }

        private void HandleProgressValueChanged(float currentValue)
        {
            SetStatus($"Value changed: {currentValue:P0}");
        }

        private void HandleSegmentCompleted(int segmentIndex)
        {
            SetStatus($"Segment completed: {segmentIndex + 1}");
        }

        private void HandleEchoStarted(float fromValue, float toValue)
        {
            SetStatus($"Echo started: {fromValue:P0} -> {toValue:P0}");
        }

        private void HandleEchoCompleted(float currentValue)
        {
            SetStatus($"Echo completed: {currentValue:P0}");
        }

        private void ApplyDelta(float delta, string reason)
        {
            var source = ResolveValueSource();
            if (source == null)
            {
                return;
            }

            var nextValue = Mathf.Clamp01(source.Value + delta);
            ApplyValueToProgressBars(nextValue, true, true);
            SetStatus($"{reason} -> {nextValue:P0}");
        }

        private UIProgressBarControl ResolveValueSource()
        {
            if (progressBarControl != null)
            {
                return progressBarControl;
            }

            return segmentFillProgressBarControl;
        }

        private void ApplyValueToProgressBars(float targetValue, bool animate, bool notify)
        {
            if (progressBarControl != null)
            {
                progressBarControl.SetValue(targetValue, animate, notify);
            }

            if (segmentFillProgressBarControl != null && segmentFillProgressBarControl != progressBarControl)
            {
                segmentFillProgressBarControl.SetValue(targetValue, animate, notify);
            }
        }

        private void SetStatus(string text)
        {
            if (statusLabel == null)
            {
                return;
            }

            statusLabel.text = text;
        }

        private void SetToggleWithoutCallback(UIToggleControl toggle, bool value)
        {
            if (toggle == null)
            {
                return;
            }

            suppressToggleCallbacks = true;
            toggle.SetIsOn(value, true, false);
            suppressToggleCallbacks = false;
        }
    }
}
