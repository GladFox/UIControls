using System;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIMarqueeDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIMarqueeDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color TrackColor = new Color(0.13f, 0.18f, 0.27f, 1f);
        private static readonly Color LabelColor = new Color(0.97f, 0.98f, 1f, 1f);

        [MenuItem("UIControls/Create Marquee Demo Scene")]
        public static void CreateMarqueeDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateMarqueeDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 300f), new Vector2(900f, 60f),
                "UIControls Marquee Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            CreateCaption(panel, new Vector2(0f, 190f),
                "Loop — news-ticker style: text exits left and re-enters from the right.");
            CreateMarquee(panel, new Vector2(0f, 130f), UIMarqueeControl.Mode.Loop, 90f,
                "Breaking: UIControls now ships a marquee that scrolls overflowing text smoothly across its viewport — perfect for tickers, alerts, and now-playing labels.");

            CreateCaption(panel, new Vector2(0f, 0f),
                "PingPong — slides to the end of the text and back again.");
            CreateMarquee(panel, new Vector2(0f, -60f), UIMarqueeControl.Mode.PingPong, 70f,
                "Now playing — Adventures in Procedural UI · Greatest Hits of the Inspector · Live at the Canvas");

            CreateCaption(panel, new Vector2(0f, -190f),
                "Short text never scrolls when 'only when overflowing' is on:");
            CreateMarquee(panel, new Vector2(0f, -250f), UIMarqueeControl.Mode.Loop, 90f,
                "All good here.");

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Marquee demo scene created: {ScenePath}");
        }

        private static void CreateMarquee(RectTransform parent, Vector2 anchoredPosition, UIMarqueeControl.Mode mode, float speed, string text)
        {
            var viewportGo = new GameObject("Marquee", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(UIMarqueeControl));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(parent, false);
            viewport.anchorMin = new Vector2(0.5f, 0.5f);
            viewport.anchorMax = new Vector2(0.5f, 0.5f);
            viewport.pivot = new Vector2(0.5f, 0.5f);
            viewport.sizeDelta = new Vector2(720f, 56f);
            viewport.anchoredPosition = anchoredPosition;

            var bg = viewportGo.GetComponent<Image>();
            bg.color = TrackColor;
            bg.raycastTarget = false;

            // Label: left-anchored, left pivot, stretched vertically.
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.SetParent(viewport, false);
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(2000f, 0f);
            labelRect.anchoredPosition = Vector2.zero;

            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 26;
            label.fontStyle = FontStyles.Normal;
            label.alignment = TextAlignmentOptions.Left;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.color = LabelColor;
            label.raycastTarget = false;

            var control = viewportGo.GetComponent<UIMarqueeControl>();
            var so = new SerializedObject(control);
            so.FindProperty("viewport").objectReferenceValue = viewport;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("mode").enumValueIndex = (int)mode;
            so.FindProperty("speed").floatValue = speed;
            so.FindProperty("onlyWhenOverflowing").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateCaption(RectTransform parent, Vector2 anchoredPosition, string content)
        {
            var caption = CreateText("Caption", parent, anchoredPosition, new Vector2(820f, 40f),
                content, 18, FontStyles.Italic, TextAlignmentOptions.Center);
            caption.color = new Color(0.75f, 0.8f, 0.95f, 1f);
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
            rect.sizeDelta = new Vector2(960f, 760f);

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
            text.overflowMode = TextOverflowModes.Overflow;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
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
