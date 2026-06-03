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
    public static class UIAccordionDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIAccordionDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color HeaderColor = new Color(0.2f, 0.25f, 0.35f, 1f);
        private static readonly Color ContentColor = new Color(0.13f, 0.16f, 0.24f, 1f);
        private static readonly Color HeaderLabelColor = new Color(0.95f, 0.97f, 1f, 1f);
        private static readonly Color ChevronColor = new Color(0.7f, 0.78f, 0.95f, 1f);
        private static readonly Color BodyColor = new Color(0.8f, 0.85f, 0.94f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        private struct SectionData
        {
            public string Header;
            public string Body;
            public float ContentHeight;
            public bool Expanded;
        }

        [MenuItem("UIControls/Create Accordion Demo Scene")]
        public static void CreateAccordionDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateAccordionDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 370f), new Vector2(900f, 60f),
                "UIControls Accordion Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 326f), new Vector2(1000f, 40f),
                "Left: single-open (classic accordion). Right: multi-open. Click a header to expand/collapse.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            // FAQ — single-open.
            var faqCaption = CreateText("FaqCaption", panel, new Vector2(-240f, 270f), new Vector2(440f, 30f),
                "FAQ (single-open)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            faqCaption.color = new Color(0.7f, 0.78f, 0.95f, 1f);

            var faq = CreateAccordion(panel, "FaqAccordion", new Vector2(-240f, 240f), 440f, allowMultiple: false,
                new[]
                {
                    new SectionData { Header = "What is UIControls?", Body = "A small library of reusable uGUI controls with DOTween animations.", ContentHeight = 150f, Expanded = true },
                    new SectionData { Header = "Does it need DOTween?", Body = "Yes — DOTween must be present in the project. The package links DOTween.dll directly.", ContentHeight = 150f, Expanded = false },
                    new SectionData { Header = "How are demos generated?", Body = "Each control ships an editor scene builder that constructs a ready-to-run demo scene.", ContentHeight = 150f, Expanded = false },
                });

            var faqStatus = CreateText("FaqStatus", panel, new Vector2(-240f, -350f), new Vector2(440f, 30f),
                "FAQ: question 1 open", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            faqStatus.color = StatusColor;

            // Settings — multi-open.
            var settingsCaption = CreateText("SettingsCaption", panel, new Vector2(240f, 270f), new Vector2(440f, 30f),
                "Settings (multi-open)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            settingsCaption.color = new Color(0.7f, 0.78f, 0.95f, 1f);

            var settings = CreateAccordion(panel, "SettingsAccordion", new Vector2(240f, 240f), 440f, allowMultiple: true,
                new[]
                {
                    new SectionData { Header = "Audio", Body = "Master, music and SFX volumes. Mute on focus loss.", ContentHeight = 140f, Expanded = false },
                    new SectionData { Header = "Video", Body = "Resolution, fullscreen mode, vsync and quality preset.", ContentHeight = 140f, Expanded = false },
                    new SectionData { Header = "Controls", Body = "Key bindings, gamepad layout and sensitivity.", ContentHeight = 140f, Expanded = false },
                });

            var settingsStatus = CreateText("SettingsStatus", panel, new Vector2(240f, -350f), new Vector2(440f, 30f),
                "Settings: 0 section(s) open", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            settingsStatus.color = StatusColor;

            var presenter = panel.gameObject.AddComponent<UIAccordionDemoPresenter>();
            SetObjectReference(presenter, "faqAccordion", faq);
            SetObjectReference(presenter, "faqStatusLabel", faqStatus);
            SetObjectReference(presenter, "settingsAccordion", settings);
            SetObjectReference(presenter, "settingsStatusLabel", settingsStatus);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Accordion demo scene created: {ScenePath}");
        }

        private static UIAccordionControl CreateAccordion(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            float width,
            bool allowMultiple,
            SectionData[] data)
        {
            const float headerHeight = 72f;

            var go = new GameObject(name, typeof(RectTransform), typeof(UIAccordionControl));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(width, 200f);

            var roots = new RectTransform[data.Length];
            var headers = new UIButtonControl[data.Length];
            var chevrons = new RectTransform[data.Length];
            var viewports = new RectTransform[data.Length];

            for (var i = 0; i < data.Length; i++)
            {
                BuildSection(rect, i, data[i], headerHeight, out roots[i], out headers[i], out chevrons[i], out viewports[i]);
            }

            var control = go.GetComponent<UIAccordionControl>();
            ConfigureAccordion(control, rect, allowMultiple, data, headerHeight, roots, headers, chevrons, viewports);
            return control;
        }

        private static void BuildSection(
            RectTransform container,
            int index,
            SectionData data,
            float headerHeight,
            out RectTransform root,
            out UIButtonControl header,
            out RectTransform chevron,
            out RectTransform viewport)
        {
            var sectionGo = new GameObject($"Section_{index + 1}", typeof(RectTransform));
            root = sectionGo.GetComponent<RectTransform>();
            root.SetParent(container, false);
            TopStretch(root);
            root.sizeDelta = new Vector2(0f, headerHeight);

            // Header is a UIButtonControl on a Graphic-less root (so its animator doesn't recolor a fill).
            var headerGo = new GameObject("Header", typeof(RectTransform), typeof(UIButtonControl));
            var headerRect = headerGo.GetComponent<RectTransform>();
            headerRect.SetParent(root, false);
            TopStretch(headerRect);
            headerRect.sizeDelta = new Vector2(0f, headerHeight);
            headerRect.anchoredPosition = Vector2.zero;

            var headerBgGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var headerBgRect = headerBgGo.GetComponent<RectTransform>();
            headerBgRect.SetParent(headerRect, false);
            FullStretch(headerBgRect);
            var headerBg = headerBgGo.GetComponent<Image>();
            headerBg.color = HeaderColor;
            headerBg.raycastTarget = true;

            var headerLabel = CreateText("Label", headerRect, new Vector2(28f, 0f), new Vector2(360f, headerHeight),
                data.Header, 22, FontStyles.Bold, TextAlignmentOptions.Left);
            headerLabel.color = HeaderLabelColor;
            headerLabel.raycastTarget = false;
            var labelRect = headerLabel.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(28f, 0f);

            var chevronLabel = CreateText("Chevron", headerRect, Vector2.zero, new Vector2(40f, 40f),
                "❯", 26, FontStyles.Bold, TextAlignmentOptions.Center); // ❯
            chevronLabel.color = ChevronColor;
            chevronLabel.raycastTarget = false;
            chevron = chevronLabel.rectTransform;
            chevron.anchorMin = new Vector2(1f, 0.5f);
            chevron.anchorMax = new Vector2(1f, 0.5f);
            chevron.pivot = new Vector2(0.5f, 0.5f);
            chevron.anchoredPosition = new Vector2(-32f, 0f);

            header = headerGo.GetComponent<UIButtonControl>();

            // Content viewport — masked, below the header. Height is driven by the control.
            var viewportGo = new GameObject("Content", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(root, false);
            TopStretch(viewport);
            viewport.anchoredPosition = new Vector2(0f, -headerHeight);
            viewport.sizeDelta = new Vector2(0f, data.Expanded ? data.ContentHeight : 0f);
            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = ContentColor;
            viewportImage.raycastTarget = false;

            var body = CreateText("Body", viewport, Vector2.zero, Vector2.zero,
                data.Body, 19, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            body.color = BodyColor;
            body.raycastTarget = false;
            var bodyRect = body.rectTransform;
            bodyRect.anchorMin = new Vector2(0f, 1f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.offsetMin = new Vector2(28f, -(data.ContentHeight - 24f));
            bodyRect.offsetMax = new Vector2(-28f, -20f);
        }

        private static void ConfigureAccordion(
            UIAccordionControl control,
            RectTransform container,
            bool allowMultiple,
            SectionData[] data,
            float headerHeight,
            IReadOnlyList<RectTransform> roots,
            IReadOnlyList<UIButtonControl> headers,
            IReadOnlyList<RectTransform> chevrons,
            IReadOnlyList<RectTransform> viewports)
        {
            var so = new SerializedObject(control);

            so.FindProperty("container").objectReferenceValue = container;
            so.FindProperty("spacing").floatValue = 8f;
            so.FindProperty("allowMultipleOpen").boolValue = allowMultiple;
            so.FindProperty("collapsedChevronZ").floatValue = 0f;
            so.FindProperty("expandedChevronZ").floatValue = -90f;

            var tween = so.FindProperty("tween");
            tween.FindPropertyRelative("duration").floatValue = 0.28f;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutCubic;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            var sectionsProp = so.FindProperty("sections");
            sectionsProp.arraySize = data.Length;
            for (var i = 0; i < data.Length; i++)
            {
                var el = sectionsProp.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("root").objectReferenceValue = roots[i];
                el.FindPropertyRelative("header").objectReferenceValue = headers[i];
                el.FindPropertyRelative("chevron").objectReferenceValue = chevrons[i];
                el.FindPropertyRelative("contentViewport").objectReferenceValue = viewports[i];
                el.FindPropertyRelative("headerHeight").floatValue = headerHeight;
                el.FindPropertyRelative("contentHeight").floatValue = data[i].ContentHeight;
                el.FindPropertyRelative("expanded").boolValue = data[i].Expanded;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void TopStretch(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
        }

        private static void FullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
            text.color = Color.white;

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
