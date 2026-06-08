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
    public static class UICircularProgressDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Feedback(E)/UICircularProgressDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color TrackColor = new Color(0.2f, 0.24f, 0.33f, 1f);
        private static readonly Color FillColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color SpinColor = new Color(0.98f, 0.7f, 0.3f, 1f);
        private static readonly Color LabelColor = new Color(0.95f, 0.97f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create CircularProgress Demo Scene")]
        public static void CreateCircularProgressDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateCircularProgressDemoSceneBatch()
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
                "UIControls Circular Progress Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var determinate = CreateRing(panel, "Determinate", new Vector2(-200f, 40f), 240f, FillColor, true, false);
            var indeterminate = CreateRing(panel, "Indeterminate", new Vector2(200f, 40f), 240f, SpinColor, false, true);

            CreateText("DetCaption", panel, new Vector2(-200f, -120f), new Vector2(300f, 32f),
                "Determinate", 22, FontStyles.Italic, TextAlignmentOptions.Center).color = StatusColor;
            CreateText("IndCaption", panel, new Vector2(200f, -120f), new Vector2(300f, 32f),
                "Indeterminate", 22, FontStyles.Italic, TextAlignmentOptions.Center).color = StatusColor;

            var status = CreateText("Status", panel, new Vector2(0f, -210f), new Vector2(900f, 36f),
                "—", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var presenter = panel.gameObject.AddComponent<UICircularProgressDemoPresenter>();
            SetObjectReference(presenter, "determinate", determinate);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"CircularProgress demo scene created: {ScenePath}");
        }

        private static UICircularProgressControl CreateRing(
            RectTransform parent, string name, Vector2 anchoredPosition, float size, Color fillColor, bool showLabel, bool indeterminate)
        {
            var rootGo = new GameObject(name, typeof(RectTransform), typeof(UICircularProgressControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(size, size);

            var track = CreateRingImage(rootRect, "Track", size, TrackColor);
            var fill = CreateRingImage(rootRect, "Fill", size, fillColor);

            var label = CreateText("Label", rootRect, Vector2.zero, new Vector2(size, size), "0%", 44, FontStyles.Bold, TextAlignmentOptions.Center);
            label.color = LabelColor;

            var control = rootGo.GetComponent<UICircularProgressControl>();
            var so = new SerializedObject(control);
            so.FindProperty("fillImage").objectReferenceValue = fill;
            so.FindProperty("trackImage").objectReferenceValue = track;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("value").floatValue = 0.35f;
            so.FindProperty("indeterminate").boolValue = indeterminate;
            so.FindProperty("showLabel").boolValue = showLabel;
            so.FindProperty("spinSpeed").floatValue = 200f;
            so.FindProperty("indeterminateArc").floatValue = 0.25f;
            so.ApplyModifiedPropertiesWithoutUndo();

            return control;
        }

        private static Image CreateRingImage(RectTransform parent, string name, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
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
