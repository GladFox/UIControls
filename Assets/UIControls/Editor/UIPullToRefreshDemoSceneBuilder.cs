using System;
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
    public static class UIPullToRefreshDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIPullToRefreshDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color ViewportColor = new Color(0.12f, 0.15f, 0.22f, 1f);
        private static readonly Color RowColor = new Color(0.18f, 0.22f, 0.31f, 1f);
        private static readonly Color RowLabelColor = new Color(0.9f, 0.93f, 1f, 1f);
        private static readonly Color IndicatorColor = new Color(0.16f, 0.2f, 0.29f, 1f);
        private static readonly Color SpinnerColor = new Color(0.45f, 0.7f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create PullToRefresh Demo Scene")]
        public static void CreatePullToRefreshDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreatePullToRefreshDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 380f), new Vector2(900f, 60f),
                "UIControls Pull-to-Refresh Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var status = CreateText("Status", panel, new Vector2(0f, 326f), new Vector2(900f, 36f),
                "Pull the list down to refresh.  Refreshed 0x", 20, FontStyles.Italic, TextAlignmentOptions.Center);
            status.color = StatusColor;

            // ScrollView root (ScrollRect + pull-to-refresh on the same object).
            var scrollGo = new GameObject("ScrollView",
                typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(UIPullToRefreshControl));
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.SetParent(panel, false);
            scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0f, -20f);
            scrollRect.sizeDelta = new Vector2(640f, 600f);
            var scrollBg = scrollGo.GetComponent<Image>();
            scrollBg.color = ViewportColor;
            scrollBg.raycastTarget = true;

            // Viewport (masked).
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(scrollRect, false);
            Stretch(viewport);
            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;

            // Content (vertical list).
            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);
            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject firstRow = null;
            for (var i = 1; i <= 14; i++)
            {
                var row = CreateRow(content, $"Row_{i}", $"Item #{i}");
                if (firstRow == null)
                {
                    firstRow = row;
                }
            }

            // Configure ScrollRect.
            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.elasticity = 0.1f;
            sr.inertia = true;
            sr.decelerationRate = 0.135f;
            sr.scrollSensitivity = 20f;

            // Pull indicator — child of the MASKED viewport so it is clipped while hidden above the top.
            var indicator = CreateIndicator(viewport, out var spinner, out var indicatorLabel);

            var control = scrollGo.GetComponent<UIPullToRefreshControl>();
            ConfigurePull(control, sr, indicator, spinner, indicatorLabel);

            var presenter = panel.gameObject.AddComponent<UIPullToRefreshDemoPresenter>();
            SetObjectReference(presenter, "pull", control);
            SetObjectReference(presenter, "content", content);
            SetObjectReference(presenter, "rowTemplate", firstRow);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"PullToRefresh demo scene created: {ScenePath}");
        }

        private static GameObject CreateRow(RectTransform content, string name, string text)
        {
            var rowGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rect = rowGo.GetComponent<RectTransform>();
            rect.SetParent(content, false);
            var image = rowGo.GetComponent<Image>();
            image.color = RowColor;
            image.raycastTarget = true; // rows are part of the scrollable surface
            var le = rowGo.GetComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.minHeight = 64f;

            var label = CreateText("Label", rect, Vector2.zero, Vector2.zero, text, 22, FontStyles.Bold, TextAlignmentOptions.Left);
            label.color = RowLabelColor;
            label.raycastTarget = false;
            var lrect = label.rectTransform;
            lrect.anchorMin = Vector2.zero;
            lrect.anchorMax = Vector2.one;
            lrect.offsetMin = new Vector2(24f, 0f);
            lrect.offsetMax = new Vector2(-24f, 0f);

            return rowGo;
        }

        private static RectTransform CreateIndicator(RectTransform parent, out RectTransform spinner, out TMP_Text label)
        {
            var go = new GameObject("PullIndicator", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(320f, 56f);
            rect.anchoredPosition = new Vector2(0f, 64f); // hidden above the viewport top (clipped by the mask)
            var bg = go.GetComponent<Image>();
            bg.color = IndicatorColor;
            bg.raycastTarget = false;

            var spinnerGo = new GameObject("Spinner", typeof(RectTransform), typeof(TextMeshProUGUI));
            var spinnerRect = spinnerGo.GetComponent<RectTransform>();
            spinnerRect.SetParent(rect, false);
            spinnerRect.anchorMin = new Vector2(0f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0f, 0.5f);
            spinnerRect.pivot = new Vector2(0.5f, 0.5f);
            spinnerRect.sizeDelta = new Vector2(40f, 40f);
            spinnerRect.anchoredPosition = new Vector2(40f, 0f);
            var spinnerText = spinnerGo.GetComponent<TextMeshProUGUI>();
            spinnerText.text = "↻";
            spinnerText.fontSize = 34;
            spinnerText.fontStyle = FontStyles.Bold;
            spinnerText.alignment = TextAlignmentOptions.Center;
            spinnerText.color = SpinnerColor;
            spinnerText.raycastTarget = false;
            spinner = spinnerRect;

            label = CreateText("Label", rect, new Vector2(30f, 0f), new Vector2(220f, 40f),
                "Pull to refresh", 20, FontStyles.Normal, TextAlignmentOptions.Left);
            label.color = new Color(0.85f, 0.89f, 0.97f, 1f);
            label.raycastTarget = false;
            var lrect = label.rectTransform;
            lrect.anchorMin = new Vector2(0f, 0.5f);
            lrect.anchorMax = new Vector2(0f, 0.5f);
            lrect.pivot = new Vector2(0f, 0.5f);
            lrect.anchoredPosition = new Vector2(72f, 0f);

            rect.SetAsLastSibling();
            return rect;
        }

        private static void ConfigurePull(
            UIPullToRefreshControl control,
            ScrollRect scrollRect,
            RectTransform indicator,
            RectTransform spinner,
            TMP_Text label)
        {
            var so = new SerializedObject(control);
            so.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            so.FindProperty("indicator").objectReferenceValue = indicator;
            so.FindProperty("spinner").objectReferenceValue = spinner;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("pullThreshold").floatValue = 120f;
            so.FindProperty("hiddenY").floatValue = 64f;
            so.FindProperty("restY").floatValue = 0f;
            so.FindProperty("degreesPerPixel").floatValue = 2.2f;
            so.FindProperty("spinSpeed").floatValue = 320f;
            so.FindProperty("pullText").stringValue = "Pull to refresh";
            so.FindProperty("releaseText").stringValue = "Release to refresh";
            so.FindProperty("refreshingText").stringValue = "Refreshing…";

            var tween = so.FindProperty("returnTween");
            tween.FindPropertyRelative("duration").floatValue = 0.3f;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutCubic;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            so.ApplyModifiedPropertiesWithoutUndo();
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
            rect.sizeDelta = new Vector2(960f, 900f);

            var image = panelGo.GetComponent<Image>();
            image.color = PanelColor;
            image.raycastTarget = false;

            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
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
