using System;
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
    public static class UIInfiniteScrollDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Scrolling(C)/UIInfiniteScrollDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color ViewportColor = new Color(0.12f, 0.15f, 0.22f, 1f);
        private static readonly Color RowColor = new Color(0.18f, 0.22f, 0.31f, 1f);
        private static readonly Color RowLabelColor = new Color(0.9f, 0.93f, 1f, 1f);
        private static readonly Color FooterColor = new Color(0.14f, 0.17f, 0.25f, 1f);
        private static readonly Color SpinnerColor = new Color(0.45f, 0.7f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create InfiniteScroll Demo Scene")]
        public static void CreateInfiniteScrollDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateInfiniteScrollDemoSceneBatch()
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
                "UIControls Infinite Scroll Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var status = CreateText("Status", panel, new Vector2(0f, 326f), new Vector2(900f, 40f),
                "Loaded 0 of 80 — scroll to the bottom to load more", 19, FontStyles.Italic, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var scrollGo = new GameObject("ScrollView",
                typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(UIInfiniteScrollControl));
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.SetParent(panel, false);
            scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0f, -20f);
            scrollRect.sizeDelta = new Vector2(660f, 620f);
            var scrollBg = scrollGo.GetComponent<Image>();
            scrollBg.color = ViewportColor;
            scrollBg.raycastTarget = true;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(scrollRect, false);
            Stretch(viewport);
            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
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

            var rowTemplate = CreateRow(content);
            var footer = CreateFooter(content, out var footerSpinner, out var footerLabel);

            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.elasticity = 0.1f;
            sr.inertia = true;
            sr.decelerationRate = 0.135f;
            sr.scrollSensitivity = 24f;

            var control = scrollGo.GetComponent<UIInfiniteScrollControl>();
            ConfigureControl(control, sr, viewport, content, footer, footerSpinner, footerLabel);

            var presenter = panel.gameObject.AddComponent<UIInfiniteScrollDemoPresenter>();
            SetObjectReference(presenter, "list", control);
            SetObjectReference(presenter, "content", content);
            SetObjectReference(presenter, "rowTemplate", rowTemplate);
            SetObjectReference(presenter, "footer", footer);
            SetObjectReference(presenter, "statusLabel", status);
            SetInt(presenter, "initialItems", 14);
            SetInt(presenter, "batchSize", 10);
            SetInt(presenter, "maxItems", 80);
            SetFloat(presenter, "loadSeconds", 1f);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"InfiniteScroll demo scene created: {ScenePath}");
        }

        private static GameObject CreateRow(RectTransform content)
        {
            var go = new GameObject("RowTemplate", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(content, false);
            var image = go.GetComponent<Image>();
            image.color = RowColor;
            image.raycastTarget = true;
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.minHeight = 64f;

            var label = CreateText("Label", rect, Vector2.zero, Vector2.zero, "Item", 22, FontStyles.Bold, TextAlignmentOptions.Left);
            label.color = RowLabelColor;
            label.raycastTarget = false;
            var lrect = label.rectTransform;
            lrect.anchorMin = Vector2.zero;
            lrect.anchorMax = Vector2.one;
            lrect.offsetMin = new Vector2(24f, 0f);
            lrect.offsetMax = new Vector2(-24f, 0f);

            go.SetActive(false);
            return go;
        }

        private static RectTransform CreateFooter(RectTransform content, out RectTransform spinner, out TMP_Text label)
        {
            var go = new GameObject("Footer", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(content, false);
            var image = go.GetComponent<Image>();
            image.color = FooterColor;
            image.raycastTarget = false;
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 56f;
            le.minHeight = 56f;

            var spinnerGo = new GameObject("Spinner", typeof(RectTransform), typeof(TextMeshProUGUI));
            var spinnerRect = spinnerGo.GetComponent<RectTransform>();
            spinnerRect.SetParent(rect, false);
            spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRect.pivot = new Vector2(0.5f, 0.5f);
            spinnerRect.sizeDelta = new Vector2(36f, 36f);
            spinnerRect.anchoredPosition = new Vector2(-120f, 0f);
            var spinnerText = spinnerGo.GetComponent<TextMeshProUGUI>();
            spinnerText.text = "↻";
            spinnerText.fontSize = 30;
            spinnerText.fontStyle = FontStyles.Bold;
            spinnerText.alignment = TextAlignmentOptions.Center;
            spinnerText.color = SpinnerColor;
            spinnerText.raycastTarget = false;
            spinner = spinnerRect;

            label = CreateText("Label", rect, new Vector2(20f, 0f), new Vector2(360f, 40f),
                "Loading more…", 20, FontStyles.Normal, TextAlignmentOptions.Left);
            label.color = new Color(0.82f, 0.86f, 0.95f, 1f);
            label.raycastTarget = false;
            var lrect = label.rectTransform;
            lrect.anchorMin = new Vector2(0.5f, 0.5f);
            lrect.anchorMax = new Vector2(0.5f, 0.5f);
            lrect.pivot = new Vector2(0f, 0.5f);
            lrect.anchoredPosition = new Vector2(-86f, 0f);

            return rect;
        }

        private static void ConfigureControl(
            UIInfiniteScrollControl control,
            ScrollRect scrollRect,
            RectTransform viewport,
            RectTransform content,
            RectTransform footer,
            RectTransform footerSpinner,
            TMP_Text footerLabel)
        {
            var so = new SerializedObject(control);
            so.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            so.FindProperty("viewport").objectReferenceValue = viewport;
            so.FindProperty("content").objectReferenceValue = content;
            so.FindProperty("footer").objectReferenceValue = footer;
            so.FindProperty("footerSpinner").objectReferenceValue = footerSpinner;
            so.FindProperty("footerLabel").objectReferenceValue = footerLabel;
            so.FindProperty("bottomThreshold").floatValue = 220f;
            so.FindProperty("spinSpeed").floatValue = 320f;
            so.FindProperty("loadingText").stringValue = "Loading more…";
            so.FindProperty("doneText").stringValue = "You're all caught up";
            so.FindProperty("hasMore").boolValue = true;
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

        private static void SetInt(UnityEngine.Object target, string propertyName, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).floatValue = value;
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
