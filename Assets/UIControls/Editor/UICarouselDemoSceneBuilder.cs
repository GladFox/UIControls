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
    public static class UICarouselDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UICarouselDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color ButtonColor = new Color(0.24f, 0.29f, 0.4f, 1f);
        private static readonly Color LabelColor = new Color(0.98f, 0.99f, 1f, 1f);
        private static readonly Color ActiveDotColor = new Color(0.95f, 0.97f, 1f, 1f);
        private static readonly Color InactiveDotColor = new Color(0.45f, 0.5f, 0.62f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        private static readonly Color[] PageColors =
        {
            new Color(0.2f, 0.45f, 0.7f, 1f),
            new Color(0.45f, 0.32f, 0.6f, 1f),
            new Color(0.2f, 0.55f, 0.45f, 1f),
            new Color(0.65f, 0.4f, 0.3f, 1f),
        };

        private static readonly string[] PageTitles = { "Welcome", "Discover", "Customize", "Get Started" };
        private static readonly string[] PageBodies =
        {
            "Swipe through the pages, tap the arrows, or watch autoplay advance them.",
            "Each page is the full width of the viewport; release snaps to the nearest one.",
            "Dots below track the current page and highlight as you move.",
            "That's the carousel — a simple paged, snapping horizontal scroller.",
        };

        [MenuItem("UIControls/Create Carousel Demo Scene")]
        public static void CreateCarouselDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateCarouselDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 400f), new Vector2(900f, 60f),
                "UIControls Carousel / Pager Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            const float pageWidth = 720f;
            const float pageHeight = 480f;

            // ScrollView root (ScrollRect + carousel on the same object).
            var scrollGo = new GameObject("Carousel",
                typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(UICarouselControl));
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.SetParent(panel, false);
            scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0f, 60f);
            scrollRect.sizeDelta = new Vector2(pageWidth, pageHeight);
            var scrollBg = scrollGo.GetComponent<Image>();
            scrollBg.color = new Color(0.12f, 0.15f, 0.22f, 1f);
            scrollBg.raycastTarget = true;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(scrollRect, false);
            Stretch(viewport);
            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;

            var pageCount = PageColors.Length;
            var contentGo = new GameObject("Content", typeof(RectTransform));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot = new Vector2(0f, 0.5f);
            content.sizeDelta = new Vector2(pageWidth * pageCount, 0f);
            content.anchoredPosition = Vector2.zero;

            for (var i = 0; i < pageCount; i++)
            {
                CreatePage(content, i, pageWidth);
            }

            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = true;
            sr.vertical = false;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.elasticity = 0.08f;
            sr.inertia = false;
            sr.scrollSensitivity = 20f;

            // Controls row: Prev | dots | Next.
            var prev = CreateButton(panel, "PrevButton", new Vector2(-320f, -260f), new Vector2(150f, 70f), "‹ Prev", ButtonColor);
            var next = CreateButton(panel, "NextButton", new Vector2(320f, -260f), new Vector2(150f, 70f), "Next ›", ButtonColor);

            var dots = CreateDots(panel, new Vector2(0f, -260f), pageCount);

            var status = CreateText("Status", panel, new Vector2(0f, -330f), new Vector2(900f, 36f),
                "Page 1 of 4", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var hint = CreateText("Hint", panel, new Vector2(0f, -390f), new Vector2(900f, 36f),
                "Flick to swipe a page, or drag and release to snap. Autoplay ping-pongs forward then back.",
                18, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var control = scrollGo.GetComponent<UICarouselControl>();
            ConfigureCarousel(control, sr, content, pageWidth, dots);

            var presenter = panel.gameObject.AddComponent<UICarouselDemoPresenter>();
            SetObjectReference(presenter, "carousel", control);
            SetObjectReference(presenter, "prevButton", prev);
            SetObjectReference(presenter, "nextButton", next);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Carousel demo scene created: {ScenePath}");
        }

        private static void CreatePage(RectTransform content, int index, float pageWidth)
        {
            var go = new GameObject($"Page_{index + 1}", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(content, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(pageWidth, 0f);
            rect.anchoredPosition = new Vector2(index * pageWidth, 0f);
            var image = go.GetComponent<Image>();
            image.color = PageColors[index];
            image.raycastTarget = true;

            var title = CreateText("Title", rect, new Vector2(0f, 60f), new Vector2(pageWidth - 80f, 70f),
                PageTitles[index], 44, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = Color.white;

            var body = CreateText("Body", rect, new Vector2(0f, -40f), new Vector2(pageWidth - 120f, 160f),
                PageBodies[index], 22, FontStyles.Normal, TextAlignmentOptions.Top);
            body.color = new Color(0.95f, 0.97f, 1f, 0.92f);
        }

        private static Graphic[] CreateDots(RectTransform parent, Vector2 anchoredPosition, int count)
        {
            const float dotSize = 16f;
            const float spacing = 26f;
            var dots = new Graphic[count];
            var startX = -(count - 1) * spacing * 0.5f;

            for (var i = 0; i < count; i++)
            {
                var go = new GameObject($"Dot_{i + 1}", typeof(RectTransform), typeof(Image));
                var rect = go.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(dotSize, dotSize);
                rect.anchoredPosition = new Vector2(anchoredPosition.x + startX + i * spacing, anchoredPosition.y);
                var image = go.GetComponent<Image>();
                image.color = i == 0 ? ActiveDotColor : InactiveDotColor;
                image.raycastTarget = false;
                dots[i] = image;
            }

            return dots;
        }

        private static void ConfigureCarousel(
            UICarouselControl control,
            ScrollRect scrollRect,
            RectTransform content,
            float pageWidth,
            IReadOnlyList<Graphic> dots)
        {
            var so = new SerializedObject(control);
            so.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            so.FindProperty("content").objectReferenceValue = content;
            so.FindProperty("pageWidth").floatValue = pageWidth;
            so.FindProperty("activeDotColor").colorValue = ActiveDotColor;
            so.FindProperty("inactiveDotColor").colorValue = InactiveDotColor;
            so.FindProperty("activeDotScale").floatValue = 1.3f;
            so.FindProperty("initialPage").intValue = 0;
            so.FindProperty("wrap").boolValue = true;
            so.FindProperty("swipeGesture").boolValue = true;
            so.FindProperty("swipeMinDistance").floatValue = 50f;
            so.FindProperty("swipeMinSpeed").floatValue = 500f;
            so.FindProperty("autoplay").boolValue = true;
            so.FindProperty("autoplayInterval").floatValue = 3.5f;
            so.FindProperty("autoplayMode").enumValueIndex = (int)UICarouselControl.AutoplayMode.PingPong;

            var dotsProp = so.FindProperty("dots");
            dotsProp.arraySize = dots.Count;
            for (var i = 0; i < dots.Count; i++)
            {
                dotsProp.GetArrayElementAtIndex(i).objectReferenceValue = dots[i];
            }

            var tween = so.FindProperty("snapTween");
            tween.FindPropertyRelative("duration").floatValue = 0.3f;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutCubic;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UIButtonControl CreateButton(
            RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size, string label, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UIButtonControl));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var bgGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.SetParent(rect, false);
            Stretch(bgRect);
            var image = bgGo.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            var text = CreateText("Label", rect, Vector2.zero, size, label, 22, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = LabelColor;

            return go.GetComponent<UIButtonControl>();
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
            rect.sizeDelta = new Vector2(960f, 980f);

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
