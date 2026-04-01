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
        [SerializeField] private Text energyLabel;

        [Header("Health (HitBar)")]
        [Range(0f, 1f)]
        [SerializeField] private float startValue = 1f;

        [Range(0.01f, 1f)]
        [SerializeField] private float damageStep = 0.12f;

        [Range(0.01f, 1f)]
        [SerializeField] private float heavyDamageStep = 0.35f;

        [Range(0.01f, 1f)]
        [SerializeField] private float healStep = 0.08f;

        [Header("Energy (Auto Recharge)")]
        [Min(1)]
        [SerializeField] private int energySegments = 3;

        [Min(0.1f)]
        [SerializeField] private float energyFillDuration = 6f;

        [Min(0.01f)]
        [SerializeField] private float energyVisualUpdateInterval = 0.08f;

        [SerializeField] private bool loopEnergyCharge;

        private float healthValue;
        private float energyValueNormalized;
        private float energyElapsed;
        private float energyUpdateTimer;

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

            if (segmentFillProgressBarControl != null)
            {
                segmentFillProgressBarControl.OnSegmentCompleted.AddListener(HandleEnergySegmentCompleted);
            }

            ConfigureLegacyScenePresentation();
            SetLegacyToggleActive(autoDamageToggle, false);
            SetLegacyToggleActive(autoHealToggle, false);

            InitializeDemoState();
            SetStatus("HitBar: damage has echo, heal updates HP immediately. Energy starts auto-recharge.");
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

            if (segmentFillProgressBarControl != null)
            {
                segmentFillProgressBarControl.OnSegmentCompleted.RemoveListener(HandleEnergySegmentCompleted);
            }
        }

        private void Update()
        {
            if (segmentFillProgressBarControl == null || energyFillDuration <= Mathf.Epsilon)
            {
                return;
            }

            var duration = Mathf.Max(0.01f, energyFillDuration);
            energyElapsed += Time.unscaledDeltaTime;
            energyUpdateTimer += Time.unscaledDeltaTime;

            var targetValue = loopEnergyCharge
                ? Mathf.Repeat(energyElapsed / duration, 1f)
                : Mathf.Clamp01(energyElapsed / duration);

            var needsFlush = energyUpdateTimer >= energyVisualUpdateInterval ||
                             targetValue >= 1f - Mathf.Epsilon ||
                             targetValue <= Mathf.Epsilon;
            if (!needsFlush)
            {
                return;
            }

            energyUpdateTimer = 0f;
            if (Mathf.Approximately(targetValue, energyValueNormalized))
            {
                return;
            }

            var wasFull = energyValueNormalized >= 1f - Mathf.Epsilon;
            energyValueNormalized = targetValue;
            segmentFillProgressBarControl.SetValue(energyValueNormalized, true, false);
            UpdateEnergyLabel();

            if (!loopEnergyCharge && !wasFull && energyValueNormalized >= 1f - Mathf.Epsilon)
            {
                SetStatus($"Energy full: {energySegments}/{energySegments}");
            }
        }

        private void HandleDamageClick()
        {
            ApplyHealthDelta(-damageStep, $"Damage -{damageStep:P0}", true);
        }

        private void HandleHeavyDamageClick()
        {
            ApplyHealthDelta(-heavyDamageStep, $"Heavy -{heavyDamageStep:P0}", true);
        }

        private void HandleHealClick()
        {
            ApplyHealthDelta(healStep, $"Heal +{healStep:P0}", false);
        }

        private void HandleResetClick()
        {
            InitializeDemoState();
            SetStatus($"Reset: HP 100%, energy 0/{Mathf.Max(1, energySegments)}");
        }

        private void HandleEnergySegmentCompleted(int segmentIndex)
        {
            var total = Mathf.Max(1, energySegments);
            var completed = Mathf.Clamp(segmentIndex + 1, 1, total);
            SetStatus($"Energy segment ready: {completed}/{total}");
        }

        private void InitializeDemoState()
        {
            healthValue = Mathf.Clamp01(startValue);
            if (progressBarControl != null)
            {
                progressBarControl.SetValue(healthValue, false, true);
            }

            energyElapsed = 0f;
            energyUpdateTimer = 0f;
            energyValueNormalized = 0f;
            if (segmentFillProgressBarControl != null)
            {
                segmentFillProgressBarControl.SetUseHitBar(true, false);
                segmentFillProgressBarControl.SetSegmentsCount(Mathf.Max(1, energySegments), true);
                segmentFillProgressBarControl.SetValue(0f, false, false);
            }

            UpdateEnergyLabel();
        }

        private void ApplyHealthDelta(float delta, string reason, bool animate)
        {
            if (progressBarControl == null)
            {
                return;
            }

            healthValue = Mathf.Clamp01(healthValue + delta);
            progressBarControl.SetValue(healthValue, animate, true);
            SetStatus($"{reason} -> HP {healthValue:P0}");
        }

        private void SetStatus(string text)
        {
            if (statusLabel == null)
            {
                return;
            }

            statusLabel.text = text;
        }

        private void UpdateEnergyLabel()
        {
            if (energyLabel == null)
            {
                return;
            }

            var total = Mathf.Max(1, energySegments);
            var energyValue = energyValueNormalized * total;
            energyLabel.text = $"Energy {energyValue:0.00}/{total}";
        }

        private static void SetLegacyToggleActive(UIToggleControl toggle, bool active)
        {
            if (toggle == null)
            {
                return;
            }

            var root = toggle.transform.parent != null
                ? toggle.transform.parent.gameObject
                : toggle.gameObject;
            root.SetActive(active);
        }

        private void ConfigureLegacyScenePresentation()
        {
            SetLegacyObjectActive("AutoDamageLabel", false);
            SetLegacyObjectActive("AutoHealLabel", false);
            SetLegacyText("SegmentModeLabel", "Energy recharge: 0 -> 3 in 6s, each completed segment locks to main color.");
            SetLegacyText("Hint", "Top bar: damage -> echo rollback, heal -> instant HP update. Bottom bar: energy auto-fills for super hit.");
        }

        private void SetLegacyObjectActive(string childName, bool active)
        {
            if (string.IsNullOrEmpty(childName))
            {
                return;
            }

            var child = transform.Find(childName);
            if (child == null)
            {
                return;
            }

            child.gameObject.SetActive(active);
        }

        private void SetLegacyText(string childName, string value)
        {
            if (string.IsNullOrEmpty(childName))
            {
                return;
            }

            var child = transform.Find(childName);
            if (child == null)
            {
                return;
            }

            var text = child.GetComponent<Text>();
            if (text == null)
            {
                return;
            }

            text.text = value;
        }
    }
}
