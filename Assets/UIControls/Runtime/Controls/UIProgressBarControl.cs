using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    public sealed class UIProgressBarControl : MonoBehaviour
    {
        [Serializable]
        public sealed class ValueChangedEvent : UnityEvent<float>
        {
        }

        [Serializable]
        public sealed class SegmentCompletedEvent : UnityEvent<int>
        {
        }

        [Serializable]
        public sealed class EchoStartedEvent : UnityEvent<float, float>
        {
        }

        [Serializable]
        public sealed class EchoCompletedEvent : UnityEvent<float>
        {
        }

        public enum HitBarIncreaseMode
        {
            SyncBoth,
            InstantEchoToPrimary
        }

        public enum SegmentVisualMode
        {
            FillBlocks,
            DividersOnly
        }

        [Serializable]
        private sealed class SegmentPulseSettings
        {
            [Min(1f)]
            public float scaleMultiplier = 1.08f;

            [Min(0f)]
            public float duration = 0.14f;

            public Ease ease = Ease.OutQuad;
            public bool independentUpdate;
        }

        [Header("Base")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Text valueLabel;
        [SerializeField] private string valueFormat = "{0:0%}";

        [Range(0f, 1f)]
        [SerializeField] private float value = 1f;

        [SerializeField] private bool animateLabel = true;
        [SerializeField] private UITweenSettings tween = new UITweenSettings();

        [Header("Segments")]
        [SerializeField] private bool useSegments;
        [Min(1)]
        [SerializeField] private int segmentsCount = 10;
        [SerializeField] private bool autoGenerateSegments = true;
        [SerializeField] private SegmentVisualMode segmentVisualMode = SegmentVisualMode.DividersOnly;
        [SerializeField] private RectTransform generatedSegmentsRoot;
        [Min(0f)]
        [SerializeField] private float segmentGap = 4f;
        [Min(0f)]
        [SerializeField] private float dividerWidth = 2f;
        [SerializeField] private Color dividerColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] private Sprite segmentFillSprite;
        [SerializeField] private Sprite segmentDividerSprite;
        [SerializeField] private Image[] segmentFills = Array.Empty<Image>();
        [SerializeField] private Color fillingColor = new Color(0.28f, 0.9f, 0.42f, 1f);
        [SerializeField] private Color filledColor = new Color(0.06f, 0.72f, 0.32f, 1f);
        [SerializeField] private bool triggerControlStateOnSegmentCompleted;
        [SerializeField] private UIStateAnimator controlSegmentStateAnimator = new UIStateAnimator();
        [SerializeField] private UIStateVisualAsset controlSegmentCompletedState;
        [SerializeField] private bool triggerSegmentStateOnSegmentCompleted;
        [SerializeField] private SegmentPulseSettings segmentPulse = new SegmentPulseSettings();

        [Header("HitBar")]
        [SerializeField] private bool useHitBar;
        [SerializeField] private Image primaryFillImage;
        [SerializeField] private Image echoFillImage;
        [Min(0f)]
        [SerializeField] private float primaryDropDuration = 0.05f;
        [Min(0f)]
        [SerializeField] private float echoDelay = 0.12f;
        [Min(0f)]
        [SerializeField] private float echoDuration = 0.35f;
        [SerializeField] private Ease echoEase = Ease.OutQuad;
        [SerializeField] private HitBarIncreaseMode increaseMode = HitBarIncreaseMode.SyncBoth;
        [SerializeField] private bool hideEchoOnIncrease = true;

        [Header("Events")]
        [SerializeField] private ValueChangedEvent onValueChanged = new ValueChangedEvent();
        [SerializeField] private SegmentCompletedEvent onSegmentCompleted = new SegmentCompletedEvent();
        [SerializeField] private EchoStartedEvent onEchoStarted = new EchoStartedEvent();
        [SerializeField] private EchoCompletedEvent onEchoCompleted = new EchoCompletedEvent();

        [Header("Custom Actions")]
        [SerializeField] private UIProgressBarCustomAction[] customActions = Array.Empty<UIProgressBarCustomAction>();

        private Tween primaryTween;
        private Tween echoTween;
        private Tween labelTween;
        private readonly List<Image> generatedSegmentImages = new List<Image>();
        private readonly List<Graphic> generatedDividers = new List<Graphic>();
        private readonly Dictionary<int, Tween> segmentPulseTweens = new Dictionary<int, Tween>();
        private RectTransform generatedSegmentsContainer;

        private float displayedPrimaryValue;
        private float displayedEchoValue;
        private float displayedLabelValue;
        private bool[] completedSegments = Array.Empty<bool>();

        private const string AutoSegmentsRootName = "AutoSegments";
        private const string AutoSegmentPrefix = "AutoSegment_";
        private const string AutoDividerPrefix = "AutoDivider_";
        private static Sprite fallbackFilledSprite;
        private static Texture2D fallbackFilledTexture;

        public float Value => value;
        public ValueChangedEvent OnValueChanged => onValueChanged;
        public SegmentCompletedEvent OnSegmentCompleted => onSegmentCompleted;
        public EchoStartedEvent OnEchoStarted => onEchoStarted;
        public EchoCompletedEvent OnEchoCompleted => onEchoCompleted;
        public bool UseSegments => useSegments;
        public bool UseHitBar => useHitBar;

        private void Awake()
        {
            ResolveImageReferences();
            EnsureFilledImageSprite(fillImage);
            EnsureFilledImageSprite(primaryFillImage);
            EnsureFilledImageSprite(echoFillImage);
            EnsureSegmentVisuals();

            if (triggerControlStateOnSegmentCompleted)
            {
                controlSegmentStateAnimator.AutoAssign(this);
            }

            value = Mathf.Clamp01(value);
            displayedPrimaryValue = value;
            displayedEchoValue = value;
            displayedLabelValue = value;

            ResetSegmentTracking();
            ApplyVisualImmediate(value, false);
        }

        private void OnValidate()
        {
            value = Mathf.Clamp01(value);
            segmentsCount = Mathf.Max(1, segmentsCount);

            ResolveImageReferences();
            EnsureFilledImageSprite(fillImage);
            EnsureFilledImageSprite(primaryFillImage);
            EnsureFilledImageSprite(echoFillImage);
            EnsureSegmentVisuals();
        }

        private void OnDisable()
        {
            KillAllTweens();
            controlSegmentStateAnimator.Kill();
        }

        public void SetUseSegments(bool enabled, bool syncVisual = true)
        {
            if (useSegments == enabled)
            {
                if (syncVisual)
                {
                    ForceSyncVisual();
                }

                return;
            }

            useSegments = enabled;
            EnsureSegmentVisuals();
            ResetSegmentTracking();

            if (syncVisual)
            {
                ForceSyncVisual();
            }
        }

        public void SetUseHitBar(bool enabled, bool syncVisual = true)
        {
            if (useHitBar == enabled)
            {
                if (syncVisual)
                {
                    ForceSyncVisual();
                }

                return;
            }

            useHitBar = enabled;
            ResolveImageReferences();
            EnsureFilledImageSprite(primaryFillImage);
            EnsureFilledImageSprite(echoFillImage);

            if (syncVisual)
            {
                ForceSyncVisual();
            }
        }

        public void SetSegmentsCount(int count, bool rebuild = true)
        {
            var normalized = Mathf.Max(1, count);
            if (segmentsCount == normalized)
            {
                if (rebuild)
                {
                    ForceSyncVisual();
                }

                return;
            }

            segmentsCount = normalized;
            EnsureSegmentVisuals();
            ResetSegmentTracking();

            if (rebuild)
            {
                ForceSyncVisual();
            }
        }

        public void SetValue(float newValue, bool animate = true, bool notify = true)
        {
            var normalizedValue = Mathf.Clamp01(newValue);
            var previousValue = value;
            var changed = !Mathf.Approximately(previousValue, normalizedValue);

            value = normalizedValue;

            if (changed && notify)
            {
                onValueChanged?.Invoke(value);
                InvokeCustomActions(action => action.OnValueChanged(this, value));
            }

            if (!changed)
            {
                if (!animate)
                {
                    ApplyVisualImmediate(value, false);
                }

                return;
            }

            if (!animate)
            {
                ApplyVisualImmediate(value, true);
                return;
            }

            ApplyVisualAnimated(previousValue, value);
        }

        public void ForceSyncVisual()
        {
            EnsureSegmentVisuals();
            ResetSegmentTracking();
            ApplyVisualImmediate(value, false);
        }

        private void ResolveImageReferences()
        {
            if (fillImage == null)
            {
                fillImage = GetComponent<Image>();
            }

            if (primaryFillImage == null)
            {
                primaryFillImage = fillImage;
            }
        }

        private void ApplyVisualImmediate(float targetValue, bool raiseSegmentEvents)
        {
            KillValueTweens();

            displayedPrimaryValue = targetValue;
            displayedEchoValue = targetValue;
            displayedLabelValue = targetValue;

            UpdatePrimaryVisual(displayedPrimaryValue, raiseSegmentEvents);
            UpdateEchoVisual(displayedEchoValue);
            UpdateLabel(displayedLabelValue);
        }

        private void ApplyVisualAnimated(float previousValue, float targetValue)
        {
            KillValueTweens();

            if (!useHitBar)
            {
                AnimatePrimaryTo(targetValue, ResolveDuration(tween), true);
                StartLabelTween(targetValue, ResolveDuration(tween));
                return;
            }

            var isDecrease = targetValue < displayedPrimaryValue - Mathf.Epsilon;
            if (isDecrease)
            {
                AnimatePrimaryTo(targetValue, Mathf.Max(0f, primaryDropDuration), true);
                StartEchoTween(displayedEchoValue, targetValue, echoDelay, echoDuration);
                StartLabelTween(targetValue, Mathf.Max(0f, primaryDropDuration));
                return;
            }

            var increaseDuration = ResolveDuration(tween);
            AnimatePrimaryTo(targetValue, increaseDuration, true);

            if (hideEchoOnIncrease)
            {
                displayedEchoValue = targetValue;
                UpdateEchoVisual(displayedEchoValue);
            }
            else if (increaseMode == HitBarIncreaseMode.SyncBoth)
            {
                StartEchoTween(displayedEchoValue, targetValue, 0f, increaseDuration);
            }
            else
            {
                displayedEchoValue = targetValue;
                UpdateEchoVisual(displayedEchoValue);
            }

            StartLabelTween(targetValue, increaseDuration);
        }

        private void AnimatePrimaryTo(float targetValue, float duration, bool raiseSegmentEvents)
        {
            if (primaryFillImage == null)
            {
                UpdateSegments(targetValue, raiseSegmentEvents);
                return;
            }

            if (duration <= Mathf.Epsilon)
            {
                displayedPrimaryValue = targetValue;
                UpdatePrimaryVisual(displayedPrimaryValue, raiseSegmentEvents);
                return;
            }

            primaryTween = DOTween.To(() => displayedPrimaryValue, x =>
            {
                displayedPrimaryValue = x;
                UpdatePrimaryVisual(displayedPrimaryValue, raiseSegmentEvents);
            }, targetValue, duration);

            tween?.Apply(primaryTween);
        }

        private void StartEchoTween(float fromValue, float toValue, float delay, float duration)
        {
            if (echoFillImage == null)
            {
                displayedEchoValue = toValue;
                return;
            }

            if (Mathf.Approximately(fromValue, toValue))
            {
                displayedEchoValue = toValue;
                UpdateEchoVisual(displayedEchoValue);
                return;
            }

            if (delay <= Mathf.Epsilon && duration <= Mathf.Epsilon)
            {
                displayedEchoValue = toValue;
                UpdateEchoVisual(displayedEchoValue);
                onEchoStarted?.Invoke(fromValue, toValue);
                onEchoCompleted?.Invoke(toValue);
                InvokeCustomActions(action =>
                {
                    action.OnEchoStarted(this, fromValue, toValue);
                    action.OnEchoCompleted(this, toValue);
                });
                return;
            }

            onEchoStarted?.Invoke(fromValue, toValue);
            InvokeCustomActions(action => action.OnEchoStarted(this, fromValue, toValue));

            var sequence = DOTween.Sequence();
            if (delay > Mathf.Epsilon)
            {
                sequence.AppendInterval(delay);
            }

            var moveDuration = Mathf.Max(0f, duration);
            if (moveDuration > Mathf.Epsilon)
            {
                sequence.Append(DOTween.To(() => displayedEchoValue, x =>
                {
                    displayedEchoValue = x;
                    UpdateEchoVisual(displayedEchoValue);
                }, toValue, moveDuration).SetEase(echoEase));
            }
            else
            {
                displayedEchoValue = toValue;
                UpdateEchoVisual(displayedEchoValue);
            }

            sequence.OnComplete(() =>
            {
                onEchoCompleted?.Invoke(toValue);
                InvokeCustomActions(action => action.OnEchoCompleted(this, toValue));
            });

            echoTween = sequence;
        }

        private void StartLabelTween(float targetValue, float duration)
        {
            if (valueLabel == null)
            {
                return;
            }

            if (!animateLabel || duration <= Mathf.Epsilon)
            {
                displayedLabelValue = targetValue;
                UpdateLabel(displayedLabelValue);
                return;
            }

            labelTween = DOTween.To(() => displayedLabelValue, x =>
            {
                displayedLabelValue = x;
                UpdateLabel(displayedLabelValue);
            }, targetValue, duration);

            tween?.Apply(labelTween);
        }

        private void UpdatePrimaryVisual(float currentValue, bool raiseSegmentEvents)
        {
            if (primaryFillImage != null)
            {
                primaryFillImage.fillAmount = currentValue;
            }

            if (!useHitBar && fillImage != null && fillImage != primaryFillImage)
            {
                fillImage.fillAmount = currentValue;
            }

            UpdateSegments(currentValue, raiseSegmentEvents);
        }

        private void UpdateEchoVisual(float currentValue)
        {
            if (echoFillImage == null)
            {
                return;
            }

            echoFillImage.fillAmount = currentValue;
        }

        private void UpdateSegments(float progressValue, bool raiseEvents)
        {
            if (!useSegments)
            {
                return;
            }

            var count = Mathf.Max(1, segmentsCount);
            EnsureSegmentStateBuffer(count);
            var segmentSize = 1f / count;

            for (var i = 0; i < count; i++)
            {
                var min = i * segmentSize;
                var localFill = Mathf.Clamp01((progressValue - min) / segmentSize);
                var nowCompleted = localFill >= 1f - 0.0001f;

                if (segmentVisualMode == SegmentVisualMode.FillBlocks &&
                    i < segmentFills.Length &&
                    segmentFills[i] != null)
                {
                    var segmentFill = segmentFills[i];
                    EnsureFilledImageSprite(segmentFill);
                    segmentFill.fillAmount = localFill;
                    segmentFill.color = nowCompleted ? filledColor : fillingColor;
                }

                var wasCompleted = completedSegments[i];
                if (wasCompleted == nowCompleted)
                {
                    continue;
                }

                completedSegments[i] = nowCompleted;
                if (!raiseEvents || !nowCompleted)
                {
                    continue;
                }

                HandleSegmentCompleted(i);
            }
        }

        private void HandleSegmentCompleted(int segmentIndex)
        {
            onSegmentCompleted?.Invoke(segmentIndex);
            InvokeCustomActions(action => action.OnSegmentCompleted(this, segmentIndex));

            if (triggerControlStateOnSegmentCompleted && controlSegmentCompletedState != null)
            {
                controlSegmentStateAnimator.Animate(controlSegmentCompletedState.State);
            }

            if (!triggerSegmentStateOnSegmentCompleted)
            {
                return;
            }

            var pulseTarget = ResolveSegmentPulseTarget(segmentIndex);
            if (pulseTarget == null)
            {
                return;
            }

            PlaySegmentPulse(segmentIndex, pulseTarget);
        }

        private void PlaySegmentPulse(int segmentIndex, RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            if (segmentPulseTweens.TryGetValue(segmentIndex, out var existing) && existing != null && existing.IsActive())
            {
                existing.Kill(false);
            }

            var duration = Mathf.Max(0f, segmentPulse.duration);
            var baseScale = Vector3.one;

            if (duration <= Mathf.Epsilon)
            {
                target.localScale = baseScale;
                segmentPulseTweens.Remove(segmentIndex);
                return;
            }

            var half = duration * 0.5f;
            var expanded = baseScale * Mathf.Max(1f, segmentPulse.scaleMultiplier);
            var sequence = DOTween.Sequence()
                .SetUpdate(UpdateType.Normal, segmentPulse.independentUpdate);

            sequence.Append(target.DOScale(expanded, half).SetEase(segmentPulse.ease));
            sequence.Append(target.DOScale(baseScale, half).SetEase(segmentPulse.ease));
            sequence.OnKill(() =>
            {
                target.localScale = baseScale;
                if (segmentPulseTweens.TryGetValue(segmentIndex, out var current) && current == sequence)
                {
                    segmentPulseTweens.Remove(segmentIndex);
                }
            });

            segmentPulseTweens[segmentIndex] = sequence;
        }

        private void UpdateLabel(float currentValue)
        {
            if (valueLabel == null)
            {
                return;
            }

            valueLabel.text = string.Format(CultureInfo.InvariantCulture, valueFormat, currentValue);
        }

        private void ResetSegmentTracking()
        {
            EnsureSegmentStateBuffer(Mathf.Max(1, segmentsCount));
            for (var i = 0; i < completedSegments.Length; i++)
            {
                completedSegments[i] = false;
            }
        }

        private void EnsureSegmentStateBuffer(int count)
        {
            if (count <= 0)
            {
                count = 1;
            }

            if (completedSegments != null && completedSegments.Length == count)
            {
                return;
            }

            completedSegments = new bool[count];
        }

        private void EnsureSegmentVisuals()
        {
            if (!useSegments)
            {
                if (autoGenerateSegments)
                {
                    ClearGeneratedVisuals();
                }

                return;
            }

            if (!autoGenerateSegments)
            {
                EnsureFilledImageSpriteArray(segmentFills);
                return;
            }

            BuildGeneratedSegments(Mathf.Max(1, segmentsCount));
        }

        private void BuildGeneratedSegments(int count)
        {
            var root = ResolveGeneratedSegmentsRoot();
            if (root == null)
            {
                return;
            }

            ClearGeneratedVisuals(root);

            if (segmentVisualMode == SegmentVisualMode.FillBlocks)
            {
                segmentFills = BuildGeneratedSegmentFillImages(root, count);
            }
            else
            {
                segmentFills = Array.Empty<Image>();
            }

            BuildGeneratedDividers(root, count);
        }

        private Image[] BuildGeneratedSegmentFillImages(RectTransform root, int count)
        {
            var images = new Image[count];
            var halfGap = Mathf.Max(0f, segmentGap) * 0.5f;
            var sprite = segmentFillSprite != null ? segmentFillSprite : ResolveFilledSpriteCandidate();

            for (var i = 0; i < count; i++)
            {
                var segmentGo = new GameObject($"{AutoSegmentPrefix}{i + 1}", typeof(RectTransform), typeof(Image));
                var segmentRect = segmentGo.GetComponent<RectTransform>();
                segmentRect.SetParent(root, false);

                var minX = i / (float)count;
                var maxX = (i + 1f) / count;
                segmentRect.anchorMin = new Vector2(minX, 0f);
                segmentRect.anchorMax = new Vector2(maxX, 1f);
                segmentRect.pivot = new Vector2(0.5f, 0.5f);
                segmentRect.anchoredPosition = Vector2.zero;
                segmentRect.offsetMin = new Vector2(i == 0 ? 0f : halfGap, 0f);
                segmentRect.offsetMax = new Vector2(i == count - 1 ? 0f : -halfGap, 0f);

                var segmentImage = segmentGo.GetComponent<Image>();
                segmentImage.raycastTarget = false;
                segmentImage.type = Image.Type.Filled;
                segmentImage.fillMethod = Image.FillMethod.Horizontal;
                segmentImage.fillOrigin = 0;
                segmentImage.fillAmount = 0f;
                segmentImage.color = fillingColor;
                if (segmentImage.sprite == null && sprite != null)
                {
                    segmentImage.sprite = sprite;
                }

                EnsureFilledImageSprite(segmentImage);
                images[i] = segmentImage;
                generatedSegmentImages.Add(segmentImage);
            }

            return images;
        }

        private void BuildGeneratedDividers(RectTransform root, int count)
        {
            if (count <= 1 || dividerWidth <= Mathf.Epsilon)
            {
                return;
            }

            var sprite = segmentDividerSprite != null ? segmentDividerSprite : ResolveFilledSpriteCandidate();
            for (var i = 1; i < count; i++)
            {
                var dividerGo = new GameObject($"{AutoDividerPrefix}{i}", typeof(RectTransform), typeof(Image));
                var dividerRect = dividerGo.GetComponent<RectTransform>();
                dividerRect.SetParent(root, false);

                var x = i / (float)count;
                dividerRect.anchorMin = new Vector2(x, 0f);
                dividerRect.anchorMax = new Vector2(x, 1f);
                dividerRect.pivot = new Vector2(0.5f, 0.5f);
                dividerRect.anchoredPosition = Vector2.zero;
                dividerRect.sizeDelta = new Vector2(dividerWidth, 0f);

                var dividerImage = dividerGo.GetComponent<Image>();
                dividerImage.raycastTarget = false;
                dividerImage.type = Image.Type.Simple;
                dividerImage.color = dividerColor;
                if (dividerImage.sprite == null && sprite != null)
                {
                    dividerImage.sprite = sprite;
                }

                generatedDividers.Add(dividerImage);
            }
        }

        private RectTransform ResolveGeneratedSegmentsRoot()
        {
            if (generatedSegmentsContainer != null)
            {
                return generatedSegmentsContainer;
            }

            var anchor = generatedSegmentsRoot != null
                ? generatedSegmentsRoot
                : primaryFillImage != null
                    ? primaryFillImage.rectTransform
                    : fillImage != null
                        ? fillImage.rectTransform
                        : transform as RectTransform;

            if (anchor == null)
            {
                return null;
            }

            if (string.Equals(anchor.name, AutoSegmentsRootName, StringComparison.Ordinal))
            {
                generatedSegmentsContainer = anchor;
                return generatedSegmentsContainer;
            }

            var existing = anchor.Find(AutoSegmentsRootName) as RectTransform;
            if (existing != null)
            {
                generatedSegmentsContainer = existing;
                return generatedSegmentsContainer;
            }

            var rootGo = new GameObject(AutoSegmentsRootName, typeof(RectTransform));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(anchor, false);
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.SetAsLastSibling();

            generatedSegmentsContainer = rootRect;
            return generatedSegmentsContainer;
        }

        private void ClearGeneratedVisuals()
        {
            ClearGeneratedVisuals(null);
        }

        private void ClearGeneratedVisuals(RectTransform container)
        {
            for (var i = 0; i < generatedSegmentImages.Count; i++)
            {
                var image = generatedSegmentImages[i];
                if (image != null)
                {
                    DestroyGeneratedObject(image.gameObject);
                }
            }

            generatedSegmentImages.Clear();

            for (var i = 0; i < generatedDividers.Count; i++)
            {
                var graphic = generatedDividers[i];
                if (graphic != null)
                {
                    DestroyGeneratedObject(graphic.gameObject);
                }
            }

            generatedDividers.Clear();

            var targetContainer = container ?? generatedSegmentsContainer;
            if (targetContainer == null &&
                generatedSegmentsRoot != null &&
                string.Equals(generatedSegmentsRoot.name, AutoSegmentsRootName, StringComparison.Ordinal))
            {
                targetContainer = generatedSegmentsRoot;
            }
            if (targetContainer != null)
            {
                for (var i = targetContainer.childCount - 1; i >= 0; i--)
                {
                    var child = targetContainer.GetChild(i);
                    if (child != null)
                    {
                        DestroyGeneratedObject(child.gameObject);
                    }
                }
            }

            CleanupLegacyGeneratedChildren();
        }

        private void CleanupLegacyGeneratedChildren()
        {
            if (generatedSegmentsRoot == null || string.Equals(generatedSegmentsRoot.name, AutoSegmentsRootName, StringComparison.Ordinal))
            {
                return;
            }

            for (var i = generatedSegmentsRoot.childCount - 1; i >= 0; i--)
            {
                var child = generatedSegmentsRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                var childName = child.name;
                if (!childName.StartsWith(AutoSegmentPrefix, StringComparison.Ordinal) &&
                    !childName.StartsWith(AutoDividerPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                DestroyGeneratedObject(child.gameObject);
            }
        }

        private void DestroyGeneratedObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }

        private RectTransform ResolveSegmentPulseTarget(int segmentIndex)
        {
            if (segmentVisualMode == SegmentVisualMode.FillBlocks &&
                segmentIndex >= 0 &&
                segmentIndex < segmentFills.Length &&
                segmentFills[segmentIndex] != null)
            {
                return segmentFills[segmentIndex].rectTransform;
            }

            if (generatedDividers.Count > 0)
            {
                var index = Mathf.Clamp(segmentIndex, 0, generatedDividers.Count - 1);
                var divider = generatedDividers[index];
                if (divider != null)
                {
                    return divider.rectTransform;
                }
            }

            return primaryFillImage != null ? primaryFillImage.rectTransform : transform as RectTransform;
        }

        private void EnsureFilledImageSpriteArray(Image[] images)
        {
            if (images == null)
            {
                return;
            }

            for (var i = 0; i < images.Length; i++)
            {
                EnsureFilledImageSprite(images[i]);
            }
        }

        private void EnsureFilledImageSprite(Image image)
        {
            if (image == null || image.type != Image.Type.Filled || image.sprite != null)
            {
                return;
            }

            var sprite = ResolveFilledSpriteCandidate();
            if (sprite != null)
            {
                image.sprite = sprite;
            }
        }

        private Sprite ResolveFilledSpriteCandidate()
        {
            if (primaryFillImage != null && primaryFillImage.sprite != null)
            {
                return primaryFillImage.sprite;
            }

            if (fillImage != null && fillImage.sprite != null)
            {
                return fillImage.sprite;
            }

            if (echoFillImage != null && echoFillImage.sprite != null)
            {
                return echoFillImage.sprite;
            }

            if (fallbackFilledSprite != null)
            {
                return fallbackFilledSprite;
            }

            fallbackFilledSprite = CreateFallbackFilledSprite();
            return fallbackFilledSprite;
        }

        private static Sprite CreateFallbackFilledSprite()
        {
            if (fallbackFilledTexture == null)
            {
                fallbackFilledTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "UIControls_FallbackFilledTexture",
                    hideFlags = HideFlags.HideAndDontSave
                };
                fallbackFilledTexture.SetPixel(0, 0, Color.white);
                fallbackFilledTexture.Apply(false, true);
            }

            var sprite = Sprite.Create(
                fallbackFilledTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = "UIControls_FallbackFilledSprite";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static float ResolveDuration(UITweenSettings settings)
        {
            return settings != null ? Mathf.Max(0f, settings.Duration) : 0f;
        }

        private void KillValueTweens()
        {
            if (primaryTween != null && primaryTween.IsActive())
            {
                primaryTween.Kill(false);
            }

            if (echoTween != null && echoTween.IsActive())
            {
                echoTween.Kill(false);
            }

            if (labelTween != null && labelTween.IsActive())
            {
                labelTween.Kill(false);
            }

            primaryTween = null;
            echoTween = null;
            labelTween = null;
        }

        private void KillAllTweens()
        {
            KillValueTweens();

            foreach (var pair in segmentPulseTweens)
            {
                var tween = pair.Value;
                if (tween != null && tween.IsActive())
                {
                    tween.Kill(false);
                }
            }

            segmentPulseTweens.Clear();
        }

        private void InvokeCustomActions(Action<UIProgressBarCustomAction> invocation)
        {
            if (customActions == null || invocation == null)
            {
                return;
            }

            for (var i = 0; i < customActions.Length; i++)
            {
                var action = customActions[i];
                if (action == null)
                {
                    continue;
                }

                try
                {
                    invocation(action);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }
    }
}
