using System;
using DG.Tweening;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Controls.Actions;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIProgressBarDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIProgressBarDemo.unity";
        private const string TapProfilePath = "Assets/UIControls/Animations/UI/Button/Profiles/TapProfile.asset";
        private const string ProgressBarActionPath = "Assets/UIControls/Animations/UI/ProgressBar/Actions/DemoProgressBarDebug.action.asset";
        private const string EnergyScalePulseActionPath = "Assets/UIControls/Animations/UI/ProgressBar/Actions/EnergySegmentScalePulse.action.asset";
        private const string SliderBackgroundSpritePath = "Assets/ThirdParty/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Slider/Slider_Basic04_Bg.png";
        private const string SliderFillSpritePath = "Assets/ThirdParty/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Slider/Slider_Icon04_Fill_Red.png";
        private const string SliderDividerSpritePath = "Assets/ThirdParty/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Slider/Slider_Basic04_DividerLine.png";
        private static Sprite defaultUiSprite;
        private static Texture2D defaultUiTexture;

        [MenuItem("UIControls/Create ProgressBar Demo Scene")]
        public static void CreateProgressBarDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateProgressBarDemoSceneBatch()
        {
            CreateDemoScene();
        }

        private static void CreateDemoScene()
        {
            EnsureProgressBarFolders();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            var panel = CreatePanel(canvas.transform as RectTransform);

            var title = CreateText("Title", panel, new Vector2(0f, 265f), new Vector2(760f, 60f),
                "UIControls ProgressBar v2 Demo", 36, FontStyle.Bold, TextAnchor.MiddleCenter);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 225f), new Vector2(900f, 44f),
                "Top: Health HitBar. Bottom: auto Energy (3 segments / 6 seconds).", 22, FontStyle.Italic, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.73f, 0.82f, 0.95f, 1f);

            var sliderBackgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SliderBackgroundSpritePath);
            var sliderFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SliderFillSpritePath);
            var sliderDividerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SliderDividerSpritePath);

            var progressBar = CreateHitBarProgressBar(
                panel,
                new Vector2(0f, 120f),
                sliderBackgroundSprite,
                sliderFillSprite,
                sliderDividerSprite);
            var segmentFillBar = CreateEnergyProgressBar(
                panel,
                new Vector2(0f, 30f),
                sliderBackgroundSprite,
                sliderFillSprite,
                sliderDividerSprite);

            var segmentModeLabel = CreateText(
                "SegmentModeLabel",
                panel,
                new Vector2(0f, -8f),
                new Vector2(860f, 34f),
                "Energy recharge: 0 -> 3 in 6s, each completed segment locks to main color.",
                18,
                FontStyle.Italic,
                TextAnchor.MiddleCenter);
            segmentModeLabel.color = new Color(0.8f, 0.88f, 0.98f, 1f);

            var energyLabel = CreateText("EnergyValue", panel, new Vector2(0f, -42f), new Vector2(460f, 34f),
                "Energy 0.00/3", 20, FontStyle.Bold, TextAnchor.MiddleCenter);
            energyLabel.color = new Color(0.98f, 0.92f, 0.55f, 1f);

            var damageButton = CreateActionButton(panel, "DamageButton", new Vector2(-255f, -110f), "Damage -12%", new Color(0.85f, 0.28f, 0.28f, 1f));
            var heavyDamageButton = CreateActionButton(panel, "HeavyDamageButton", new Vector2(-85f, -110f), "Heavy -35%", new Color(0.75f, 0.17f, 0.17f, 1f));
            var healButton = CreateActionButton(panel, "HealButton", new Vector2(85f, -110f), "Heal +8%", new Color(0.2f, 0.62f, 0.35f, 1f));
            var resetButton = CreateActionButton(panel, "ResetButton", new Vector2(255f, -110f), "Reset", new Color(0.24f, 0.47f, 0.82f, 1f));
            var spendSuperButton = CreateActionButton(panel, "SpendSuperButton", new Vector2(0f, -172f), "Spend 1 Super", new Color(0.92f, 0.74f, 0.18f, 1f));
            ConfigureButtonSize(spendSuperButton, new Vector2(220f, 54f), 20);

            var statusLabel = CreateText("Status", panel, new Vector2(0f, -228f), new Vector2(860f, 54f),
                "HitBar: damage has echo, heal updates HP immediately. Energy recharges automatically.", 20, FontStyle.Normal, TextAnchor.MiddleCenter);
            statusLabel.color = new Color(0.98f, 0.86f, 0.5f, 1f);

            var hint = CreateText("Hint", panel, new Vector2(0f, -275f), new Vector2(860f, 56f),
                "Top bar: apply damage/heal/reset with buttons.\nBottom bar: segmented energy 0->3 for super-hit charge, use Spend 1 Super button.",
                18, FontStyle.Italic, TextAnchor.MiddleCenter);
            hint.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UIProgressBarDemoPresenter>();
            SetObjectReference(presenter, "progressBarControl", progressBar.GetComponent<UIProgressBarControl>());
            SetObjectReference(presenter, "segmentFillProgressBarControl", segmentFillBar.GetComponent<UIProgressBarControl>());
            SetObjectReference(presenter, "damageButton", damageButton.GetComponent<UIButtonControl>());
            SetObjectReference(presenter, "heavyDamageButton", heavyDamageButton.GetComponent<UIButtonControl>());
            SetObjectReference(presenter, "healButton", healButton.GetComponent<UIButtonControl>());
            SetObjectReference(presenter, "resetButton", resetButton.GetComponent<UIButtonControl>());
            SetObjectReference(presenter, "spendSuperButton", spendSuperButton.GetComponent<UIButtonControl>());
            SetObjectReference(presenter, "statusLabel", statusLabel);
            SetObjectReference(presenter, "energyLabel", energyLabel);
            SetFloat(presenter, "startValue", 1f);
            SetInt(presenter, "energySegments", 3);
            SetFloat(presenter, "energyFillDuration", 6f);
            SetFloat(presenter, "energyVisualUpdateInterval", 0.08f);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"ProgressBar demo scene created: {ScenePath}");
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.06f, 0.1f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            var transform = cameraGo.transform;
            transform.position = new Vector3(0f, 0f, -10f);
            transform.rotation = Quaternion.identity;
            cameraGo.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();

            var inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                eventSystemGo.AddComponent(inputSystemModuleType);
                return;
            }

            eventSystemGo.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static RectTransform CreatePanel(RectTransform parent)
        {
            var panelGo = new GameObject("DemoPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var rect = panelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(980f, 620f);

            var image = panelGo.GetComponent<Image>();
            image.color = new Color(0.08f, 0.11f, 0.17f, 0.94f);
            image.raycastTarget = false;

            return rect;
        }

        private static GameObject CreateActionButton(RectTransform parent, string name, Vector2 anchoredPosition, string label, Color color)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UIButtonControl));
            var rect = buttonGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(158f, 68f);

            var image = buttonGo.GetComponent<Image>();
            image.color = color;
            AssignDefaultSprite(image);

            var text = CreateText("Label", rect, Vector2.zero, rect.sizeDelta, label, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            text.raycastTarget = false;
            text.color = Color.white;

            var buttonControl = buttonGo.GetComponent<UIButtonControl>();
            var tapProfile = AssetDatabase.LoadAssetAtPath<UIButtonAnimationProfile>(TapProfilePath);
            if (tapProfile != null)
            {
                SetObjectReference(buttonControl, "animationProfile", tapProfile);
            }

            return buttonGo;
        }

        private static void ConfigureButtonSize(GameObject buttonGo, Vector2 size, int labelFontSize)
        {
            if (buttonGo == null)
            {
                return;
            }

            var rect = buttonGo.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = size;
            }

            var label = buttonGo.transform.Find("Label");
            if (label == null)
            {
                return;
            }

            var labelRect = label.GetComponent<RectTransform>();
            if (labelRect != null)
            {
                labelRect.sizeDelta = size;
                labelRect.anchoredPosition = Vector2.zero;
            }

            var labelText = label.GetComponent<Text>();
            if (labelText != null && labelFontSize > 0)
            {
                labelText.fontSize = labelFontSize;
            }
        }

        private static GameObject CreateToggle(RectTransform parent, string namePrefix, Vector2 anchoredPosition, bool isOn)
        {
            var holder = new GameObject($"{namePrefix}ToggleHolder", typeof(RectTransform));
            var holderRect = holder.GetComponent<RectTransform>();
            holderRect.SetParent(parent, false);
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.anchoredPosition = anchoredPosition;
            holderRect.sizeDelta = new Vector2(220f, 82f);

            var toggleGo = new GameObject($"{namePrefix}Toggle", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(UIToggleControl));
            var toggleRect = toggleGo.GetComponent<RectTransform>();
            toggleRect.SetParent(holderRect, false);
            toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
            toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
            toggleRect.pivot = new Vector2(0.5f, 0.5f);
            toggleRect.anchoredPosition = Vector2.zero;
            toggleRect.sizeDelta = new Vector2(170f, 64f);

            var toggleBackground = toggleGo.GetComponent<Image>();
            toggleBackground.color = new Color(0.27f, 0.27f, 0.3f, 1f);
            AssignDefaultSprite(toggleBackground);

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.SetParent(toggleRect, false);
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = new Vector2(-24f, 0f);
            handleRect.sizeDelta = new Vector2(50f, 50f);

            var handleImage = handleGo.GetComponent<Image>();
            handleImage.color = Color.white;
            AssignDefaultSprite(handleImage);

            var toggleControl = toggleGo.GetComponent<UIToggleControl>();
            SetObjectReference(toggleControl, "handle", handleRect);
            SetObjectReference(toggleControl, "backgroundGraphic", toggleBackground);
            SetObjectReference(toggleControl, "handleGraphic", handleImage);
            SetObjectReference(toggleControl, "canvasGroup", toggleGo.GetComponent<CanvasGroup>());
            SetVector2(toggleControl, "offHandlePosition", new Vector2(-42f, 0f));
            SetVector2(toggleControl, "onHandlePosition", new Vector2(42f, 0f));
            SetBool(toggleControl, "isOn", isOn);

            return toggleGo;
        }

        private static GameObject CreateHitBarProgressBar(
            RectTransform parent,
            Vector2 anchoredPosition,
            Sprite backgroundSprite,
            Sprite fillSprite,
            Sprite dividerSprite)
        {
            const int segmentCount = 10;

            var progressGo = CreateProgressBarRoot("DemoHitBarProgress", parent, anchoredPosition, new Vector2(820f, 88f), backgroundSprite);
            var rect = progressGo.GetComponent<RectTransform>();

            var echoFill = CreateFillLayer(rect, "EchoFill", new Color(0.78f, 0.22f, 0.22f, 0.62f), fillSprite, 4f);
            var primaryFill = CreateFillLayer(rect, "PrimaryFill", new Color(0.2f, 0.78f, 0.37f, 1f), fillSprite, 4f);

            var valueText = CreateText("ValueText", rect, Vector2.zero, rect.sizeDelta,
                "100%", 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            valueText.color = Color.white;
            valueText.raycastTarget = false;

            var progressControl = progressGo.GetComponent<UIProgressBarControl>();
            SetObjectReference(progressControl, "fillImage", primaryFill);
            SetObjectReference(progressControl, "primaryFillImage", primaryFill);
            SetObjectReference(progressControl, "echoFillImage", echoFill);
            SetObjectReference(progressControl, "valueLabel", valueText);
            SetFloat(progressControl, "value", 1f);
            SetBool(progressControl, "useSegments", true);
            SetInt(progressControl, "segmentsCount", segmentCount);
            SetBool(progressControl, "autoGenerateSegments", true);
            SetEnum(progressControl, "segmentVisualMode", (int)UIProgressBarControl.SegmentVisualMode.DividersOnly);
            SetObjectReference(progressControl, "generatedSegmentsRoot", primaryFill.rectTransform);
            SetFloat(progressControl, "segmentGap", 0f);
            SetFloat(progressControl, "dividerWidth", 2f);
            SetColor(progressControl, "dividerColor", new Color(0.05f, 0.2f, 0.4f, 0.95f));
            SetObjectReference(progressControl, "segmentFillSprite", fillSprite);
            SetObjectReference(progressControl, "segmentDividerSprite", dividerSprite);
            SetObjectReferenceArray(progressControl, "segmentFills", Array.Empty<UnityEngine.Object>());
            SetColor(progressControl, "fillingColor", new Color(0.47f, 0.86f, 0.35f, 1f));
            SetColor(progressControl, "filledColor", new Color(0.17f, 0.72f, 0.33f, 1f));
            SetBool(progressControl, "triggerControlStateOnSegmentCompleted", false);
            SetBool(progressControl, "triggerSegmentStateOnSegmentCompleted", true);
            ConfigureSegmentPulse(progressControl, 1.08f, 0.16f, Ease.OutQuad, false);

            SetBool(progressControl, "useHitBar", true);
            SetFloat(progressControl, "primaryDropDuration", 0.06f);
            SetFloat(progressControl, "echoDelay", 0.12f);
            SetFloat(progressControl, "echoDuration", 0.45f);
            SetEnum(progressControl, "echoEase", (int)Ease.OutCubic);
            SetEnum(progressControl, "increaseMode", (int)UIProgressBarControl.HitBarIncreaseMode.SyncBoth);
            SetBool(progressControl, "hideEchoOnIncrease", true);

            var debugAction = LoadOrCreateAsset<UIProgressBarDebugLogAction>(ProgressBarActionPath);
            if (debugAction != null)
            {
                ConfigureProgressDebugAction(debugAction);
                SetObjectReferenceArray(progressControl, "customActions", new UnityEngine.Object[] { debugAction });
            }

            return progressGo;
        }

        private static GameObject CreateEnergyProgressBar(
            RectTransform parent,
            Vector2 anchoredPosition,
            Sprite backgroundSprite,
            Sprite fillSprite,
            Sprite dividerSprite)
        {
            const int segmentCount = 3;

            var progressGo = CreateProgressBarRoot("DemoSegmentFillProgress", parent, anchoredPosition, new Vector2(820f, 64f), backgroundSprite);
            var rect = progressGo.GetComponent<RectTransform>();
            var echoFill = CreateFillLayer(rect, "EchoFill", new Color(0.98f, 0.82f, 0.22f, 0.35f), fillSprite, 4f);
            var primaryFill = CreateFillLayer(rect, "PrimaryFill", new Color(1f, 0.86f, 0.22f, 0.92f), fillSprite, 4f);

            var progressControl = progressGo.GetComponent<UIProgressBarControl>();
            SetObjectReference(progressControl, "fillImage", primaryFill);
            SetObjectReference(progressControl, "primaryFillImage", primaryFill);
            SetObjectReference(progressControl, "echoFillImage", echoFill);
            SetObjectReference(progressControl, "valueLabel", null);
            SetFloat(progressControl, "value", 0f);
            SetBool(progressControl, "useSegments", true);
            SetInt(progressControl, "segmentsCount", segmentCount);
            SetBool(progressControl, "autoGenerateSegments", true);
            SetEnum(progressControl, "segmentVisualMode", (int)UIProgressBarControl.SegmentVisualMode.FillBlocks);
            SetObjectReference(progressControl, "generatedSegmentsRoot", primaryFill.rectTransform);
            SetFloat(progressControl, "segmentGap", 2f);
            SetFloat(progressControl, "dividerWidth", 2f);
            SetColor(progressControl, "dividerColor", new Color(0.22f, 0.16f, 0.05f, 0.95f));
            SetObjectReference(progressControl, "segmentFillSprite", fillSprite);
            SetObjectReference(progressControl, "segmentDividerSprite", dividerSprite);
            SetObjectReferenceArray(progressControl, "segmentFills", Array.Empty<UnityEngine.Object>());
            SetColor(progressControl, "fillingColor", new Color(1f, 0.82f, 0.26f, 1f));
            SetColor(progressControl, "filledColor", new Color(0.18f, 0.78f, 0.38f, 1f));
            SetBool(progressControl, "triggerControlStateOnSegmentCompleted", false);
            SetBool(progressControl, "triggerSegmentStateOnSegmentCompleted", false);
            ConfigureSegmentPulse(progressControl, 1.06f, 0.12f, Ease.OutQuad, false);
            ConfigureTween(progressControl, 0.08f, Ease.Linear, 0f, false);

            var scalePulseAction = LoadOrCreateAsset<UIProgressBarScalePulseAction>(EnergyScalePulseActionPath);
            if (scalePulseAction != null)
            {
                ConfigureScalePulseAction(scalePulseAction, 1.04f, 0.22f, Ease.OutQuad, false);
                SetObjectReferenceArray(progressControl, "customActions", new UnityEngine.Object[] { scalePulseAction });
            }

            SetBool(progressControl, "useHitBar", true);
            SetFloat(progressControl, "primaryDropDuration", 0f);
            SetFloat(progressControl, "echoDelay", 0f);
            SetFloat(progressControl, "echoDuration", 0.22f);
            SetEnum(progressControl, "echoEase", (int)Ease.OutSine);
            SetEnum(progressControl, "increaseMode", (int)UIProgressBarControl.HitBarIncreaseMode.SyncBoth);
            SetBool(progressControl, "hideEchoOnIncrease", false);
            SetBool(progressControl, "useEchoTimingOnIncrease", true);
            return progressGo;
        }

        private static GameObject CreateProgressBarRoot(
            string name,
            RectTransform parent,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Sprite backgroundSprite)
        {
            var progressGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UIProgressBarControl));
            var rect = progressGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var background = progressGo.GetComponent<Image>();
            if (backgroundSprite != null)
            {
                background.sprite = backgroundSprite;
                background.type = Image.Type.Sliced;
                background.color = Color.white;
            }
            else
            {
                background.color = new Color(0.12f, 0.14f, 0.2f, 1f);
                AssignDefaultSprite(background);
            }

            background.raycastTarget = false;
            return progressGo;
        }

        private static Image CreateFillLayer(RectTransform parent, string name, Color color, Sprite sprite, float padding)
        {
            var layerGo = new GameObject(name, typeof(RectTransform), typeof(Image));
            var layerRect = layerGo.GetComponent<RectTransform>();
            layerRect.SetParent(parent, false);
            layerRect.anchorMin = new Vector2(0f, 0f);
            layerRect.anchorMax = new Vector2(1f, 1f);
            layerRect.pivot = new Vector2(0.5f, 0.5f);
            layerRect.anchoredPosition = Vector2.zero;
            layerRect.offsetMin = new Vector2(padding, padding);
            layerRect.offsetMax = new Vector2(-padding, -padding);

            var image = layerGo.GetComponent<Image>();
            image.color = color;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = 0;
            image.fillAmount = 1f;
            image.raycastTarget = false;
            if (sprite != null)
            {
                image.sprite = sprite;
            }
            else
            {
                AssignDefaultSprite(image);
            }

            return image;
        }

        private static void AssignDefaultSprite(Image image)
        {
            if (image == null || image.sprite != null)
            {
                return;
            }

            image.sprite = GetDefaultSprite();
        }

        private static Sprite GetDefaultSprite()
        {
            if (defaultUiSprite != null)
            {
                return defaultUiSprite;
            }

            defaultUiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UISprite.psd");
            if (defaultUiSprite == null)
            {
                if (defaultUiTexture == null)
                {
                    defaultUiTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                    {
                        name = "UIControls_DemoDefaultTexture",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    defaultUiTexture.SetPixel(0, 0, Color.white);
                    defaultUiTexture.Apply(false, true);
                }

                defaultUiSprite = Sprite.Create(
                    defaultUiTexture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    100f);
                defaultUiSprite.name = "UIControls_DemoDefaultSprite";
                defaultUiSprite.hideFlags = HideFlags.HideAndDontSave;
            }

            return defaultUiSprite;
        }

        private static Text CreateText(
            string name,
            RectTransform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            string content,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment)
        {
            var textGo = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = textGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = textGo.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;

            return text;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetVector2(UnityEngine.Object target, string propertyName, Vector2 value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).vector2Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(UnityEngine.Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(UnityEngine.Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).enumValueIndex = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetColor(UnityEngine.Object target, string propertyName, Color value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).colorValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureScalePulseAction(UIProgressBarScalePulseAction action, float scaleMultiplier, float duration, Ease ease, bool independentUpdate)
        {
            var serializedObject = new SerializedObject(action);
            serializedObject.FindProperty("scaleMultiplier").floatValue = scaleMultiplier;
            serializedObject.FindProperty("duration").floatValue = duration;
            serializedObject.FindProperty("ease").enumValueIndex = (int)ease;
            serializedObject.FindProperty("independentUpdate").boolValue = independentUpdate;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(action);
        }

        private static void ConfigureSegmentPulse(UIProgressBarControl control, float scaleMultiplier, float duration, Ease ease, bool independentUpdate)
        {
            var serializedObject = new SerializedObject(control);
            var pulseProperty = serializedObject.FindProperty("segmentPulse");
            pulseProperty.FindPropertyRelative("scaleMultiplier").floatValue = scaleMultiplier;
            pulseProperty.FindPropertyRelative("duration").floatValue = duration;
            pulseProperty.FindPropertyRelative("ease").enumValueIndex = (int)ease;
            pulseProperty.FindPropertyRelative("independentUpdate").boolValue = independentUpdate;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureTween(UnityEngine.Object target, float duration, Ease ease, float delay, bool independentUpdate)
        {
            var serializedObject = new SerializedObject(target);
            var tweenProperty = serializedObject.FindProperty("tween");
            tweenProperty.FindPropertyRelative("duration").floatValue = duration;
            tweenProperty.FindPropertyRelative("ease").enumValueIndex = (int)ease;
            tweenProperty.FindPropertyRelative("delay").floatValue = delay;
            tweenProperty.FindPropertyRelative("independentUpdate").boolValue = independentUpdate;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureProgressDebugAction(UIProgressBarDebugLogAction action)
        {
            var serializedObject = new SerializedObject(action);
            serializedObject.FindProperty("logPrefix").stringValue = "[DemoProgressBar]";
            serializedObject.FindProperty("logValueChanged").boolValue = true;
            serializedObject.FindProperty("logSegmentCompleted").boolValue = true;
            serializedObject.FindProperty("logEchoStarted").boolValue = true;
            serializedObject.FindProperty("logEchoCompleted").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(action);
        }

        private static T LoadOrCreateAsset<T>(string path)
            where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureProgressBarFolders()
        {
            EnsureFolder("Assets/UIControls");
            EnsureFolder("Assets/UIControls/Animations");
            EnsureFolder("Assets/UIControls/Animations/UI");
            EnsureFolder("Assets/UIControls/Animations/UI/ProgressBar");
            EnsureFolder("Assets/UIControls/Animations/UI/ProgressBar/Actions");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes;
            for (var i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == path)
                {
                    return;
                }
            }

            var updated = new EditorBuildSettingsScene[scenes.Length + 1];
            for (var i = 0; i < scenes.Length; i++)
            {
                updated[i] = scenes[i];
            }

            updated[updated.Length - 1] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
