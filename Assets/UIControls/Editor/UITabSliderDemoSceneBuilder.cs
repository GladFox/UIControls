using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UITabSliderDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UITabSliderDemo.unity";
        private const string TapProfilePath = "Assets/UIControls/Animations/UI/Button/Profiles/TapProfile.asset";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color TabBarBackgroundColor = new Color(0.17f, 0.21f, 0.3f, 1f);
        private static readonly Color SelectionIndicatorPrimaryColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color SelectionIndicatorSecondaryColor = new Color(0.95f, 0.61f, 0.24f, 1f);
        private static readonly Color TabLabelColor = new Color(0.92f, 0.95f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create TabSlider Demo Scene")]
        public static void CreateTabSliderDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateTabSliderDemoSceneBatch()
        {
            CreateDemoScene();
        }

        private static void CreateDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            var panel = CreatePanel(canvas.transform as RectTransform);

            var title = CreateText("Title", panel, new Vector2(0f, 360f), new Vector2(900f, 60f),
                "UIControls TabSlider Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText(
                "Subtitle",
                panel,
                new Vector2(0f, 320f),
                new Vector2(900f, 36f),
                "Variant A: 3 tabs that switch views. Variant B: 4 tabs that fire events only.",
                20,
                FontStyles.Italic,
                TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var horizontal = CreateHorizontalSlider(panel, new Vector2(0f, 200f));
            var horizontalStatus = CreateText(
                "HorizontalStatus",
                panel,
                new Vector2(0f, -10f),
                new Vector2(900f, 36f),
                "Horizontal slider: tab 1 active",
                20,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            horizontalStatus.color = StatusColor;

            var eventOnly = CreateEventOnlySlider(panel, new Vector2(0f, -90f));
            var eventOnlyStatus = CreateText(
                "EventOnlyStatus",
                panel,
                new Vector2(0f, -180f),
                new Vector2(900f, 36f),
                "Event-only slider: section index = 0",
                20,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            eventOnlyStatus.color = StatusColor;

            var hint = CreateText(
                "Hint",
                panel,
                new Vector2(0f, -300f),
                new Vector2(900f, 56f),
                "Selection indicator slides under the chosen tab. Hover/press feedback lives on each button independently.",
                18,
                FontStyles.Italic,
                TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UITabSliderDemoPresenter>();
            SetObjectReference(presenter, "horizontalSlider", horizontal);
            SetObjectReference(presenter, "eventOnlySlider", eventOnly);
            SetObjectReference(presenter, "horizontalStatusLabel", horizontalStatus);
            SetObjectReference(presenter, "eventOnlyStatusLabel", eventOnlyStatus);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"TabSlider demo scene created: {ScenePath}");
        }

        private static UITabSliderControl CreateHorizontalSlider(RectTransform parent, Vector2 anchoredPosition)
        {
            var rootGo = new GameObject("HorizontalTabSlider", typeof(RectTransform), typeof(UITabSliderControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(720f, 240f);

            var tabBar = CreateTabBar(rootRect, new Vector2(0f, 92f), new Vector2(720f, 70f));
            var labels = new[] { "Stats", "Inventory", "Settings" };
            var viewColors = new[]
            {
                new Color(0.2f, 0.5f, 0.32f, 1f),
                new Color(0.36f, 0.32f, 0.6f, 1f),
                new Color(0.55f, 0.32f, 0.32f, 1f),
            };
            var viewMessages = new[]
            {
                "STATS — character attributes & progress",
                "INVENTORY — equipment & resources",
                "SETTINGS — audio, video, controls",
            };

            var indicator = CreateSelectionIndicator(tabBar, "SelectionIndicator", SelectionIndicatorPrimaryColor);

            var tabSize = new Vector2(220f, 64f);
            var buttons = new UIButtonControl[labels.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                buttons[i] = CreateTabButton(tabBar, $"Tab_{i + 1}", labels[i], i, labels.Length, tabSize);
            }

            AlignIndicatorToButton(indicator, buttons[0]);

            var viewsRoot = CreateViewsRoot(rootRect, new Vector2(0f, -60f), new Vector2(720f, 130f));
            var views = new GameObject[labels.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                views[i] = CreateViewPanel(viewsRoot, $"View_{i + 1}", viewMessages[i], viewColors[i]);
                views[i].SetActive(i == 0);
            }

            var control = rootGo.GetComponent<UITabSliderControl>();
            ConfigureTabSlider(control, indicator, true, 0, true, 0.28f, Ease.OutCubic);
            ConfigureTabs(control, buttons, views);

            return control;
        }

        private static UITabSliderControl CreateEventOnlySlider(RectTransform parent, Vector2 anchoredPosition)
        {
            var rootGo = new GameObject("EventOnlyTabSlider", typeof(RectTransform), typeof(UITabSliderControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(820f, 80f);

            var tabBar = CreateTabBar(rootRect, Vector2.zero, new Vector2(820f, 70f));
            var labels = new[] { "Day", "Week", "Month", "All time" };
            var indicator = CreateSelectionIndicator(tabBar, "SelectionIndicator", SelectionIndicatorSecondaryColor);

            var tabSize = new Vector2(190f, 64f);
            var buttons = new UIButtonControl[labels.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                buttons[i] = CreateTabButton(tabBar, $"Range_{i + 1}", labels[i], i, labels.Length, tabSize);
            }

            AlignIndicatorToButton(indicator, buttons[0]);

            var control = rootGo.GetComponent<UITabSliderControl>();
            ConfigureTabSlider(control, indicator, true, 0, false, 0.22f, Ease.OutQuad);
            ConfigureTabs(control, buttons, null);

            return control;
        }

        private static RectTransform CreateTabBar(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject("TabBar", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = TabBarBackgroundColor;
            image.raycastTarget = false;

            return rect;
        }

        private static RectTransform CreateSelectionIndicator(RectTransform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(180f, 60f);

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            rect.SetAsFirstSibling();
            return rect;
        }

        private static UIButtonControl CreateTabButton(
            RectTransform parent,
            string name,
            string label,
            int index,
            int count,
            Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UIButtonControl));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            var totalWidth = count * size.x;
            var startX = -totalWidth * 0.5f + size.x * 0.5f;
            rect.anchoredPosition = new Vector2(startX + index * size.x, 0f);

            // No Image on the button itself — UIButtonControl auto-binds the first Graphic
            // it finds as a color animation target. The Label is the raycast target instead;
            // pointer events bubble up to UIButtonControl on the parent.
            var labelText = CreateText("Label", rect, Vector2.zero, size,
                label, 22, FontStyles.Bold, TextAlignmentOptions.Center);
            labelText.color = TabLabelColor;
            labelText.raycastTarget = true;

            var control = go.GetComponent<UIButtonControl>();
            var tapProfile = AssetDatabase.LoadAssetAtPath<UIButtonAnimationProfile>(TapProfilePath);
            if (tapProfile != null)
            {
                SetObjectReference(control, "animationProfile", tapProfile);
            }

            return control;
        }

        private static RectTransform CreateViewsRoot(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject("Views", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }

        private static GameObject CreateViewPanel(RectTransform parent, string name, string message, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            var text = CreateText("Caption", rect, Vector2.zero, new Vector2(680f, 110f),
                message, 26, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = Color.white;
            text.raycastTarget = false;

            return go;
        }

        private static void AlignIndicatorToButton(RectTransform indicator, UIButtonControl button)
        {
            if (indicator == null || button == null)
            {
                return;
            }

            var buttonRect = button.transform as RectTransform;
            if (buttonRect == null)
            {
                return;
            }

            indicator.anchoredPosition = buttonRect.anchoredPosition;
            indicator.sizeDelta = buttonRect.rect.size;
        }

        private static void ConfigureTabSlider(
            UITabSliderControl control,
            RectTransform indicator,
            bool matchIndicatorSize,
            int initialIndex,
            bool autoToggleViews,
            float duration,
            Ease ease)
        {
            var serializedObject = new SerializedObject(control);
            serializedObject.FindProperty("selectionIndicator").objectReferenceValue = indicator;
            serializedObject.FindProperty("matchIndicatorSize").boolValue = matchIndicatorSize;
            serializedObject.FindProperty("initialIndex").intValue = initialIndex;
            serializedObject.FindProperty("autoToggleViews").boolValue = autoToggleViews;

            var tween = serializedObject.FindProperty("slideTween");
            tween.FindPropertyRelative("duration").floatValue = duration;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)ease;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureTabs(
            UITabSliderControl control,
            IReadOnlyList<UIButtonControl> buttons,
            IReadOnlyList<GameObject> views)
        {
            var serializedObject = new SerializedObject(control);
            var tabsProperty = serializedObject.FindProperty("tabs");
            tabsProperty.arraySize = buttons.Count;

            for (var i = 0; i < buttons.Count; i++)
            {
                var element = tabsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("button").objectReferenceValue = buttons[i];
                element.FindPropertyRelative("view").objectReferenceValue = views != null && i < views.Count ? views[i] : null;
                element.FindPropertyRelative("selectedVisual").objectReferenceValue = null;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform CreatePanel(RectTransform parent)
        {
            var panelGo = new GameObject("DemoPanel", typeof(RectTransform), typeof(Image));
            var rect = panelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(960f, 820f);

            var image = panelGo.GetComponent<Image>();
            image.color = PanelColor;
            image.raycastTarget = false;

            return rect;
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.08f, 0.14f, 1f);
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

        private static TextMeshProUGUI CreateText(
            string name,
            RectTransform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            string content,
            int fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment)
        {
            var textGo = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = textGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.color = Color.white;

            return text;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
