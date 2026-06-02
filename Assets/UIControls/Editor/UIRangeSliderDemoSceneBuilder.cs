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
    public static class UIRangeSliderDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIRangeSliderDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color RailColor = new Color(0.16f, 0.2f, 0.29f, 1f);
        private static readonly Color PriceFillColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color TimeFillColor = new Color(0.18f, 0.62f, 0.45f, 1f);
        private static readonly Color HandleColor = new Color(0.95f, 0.97f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create RangeSlider Demo Scene")]
        public static void CreateRangeSliderDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateRangeSliderDemoSceneBatch()
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
                "UIControls RangeSlider Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 320f), new Vector2(900f, 36f),
                "Two handles over one track with a fill between. Drag a handle, or click the track to jump the nearer one.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var priceCaption = CreateText("PriceCaption", panel, new Vector2(0f, 210f), new Vector2(900f, 30f),
                "Price filter ($0..$1000, whole numbers, min gap $50)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            priceCaption.color = new Color(0.7f, 0.78f, 0.95f, 1f);

            var price = CreateRangeSlider(panel, "PriceSlider", new Vector2(0f, 150f),
                min: 0f, max: 1000f, low: 200f, high: 800f, minDistance: 50f, wholeNumbers: true, fillColor: PriceFillColor);

            var priceStatus = CreateText("PriceStatus", panel, new Vector2(0f, 95f), new Vector2(900f, 32f),
                "Price: $200 — $800", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            priceStatus.color = StatusColor;

            var timeCaption = CreateText("TimeCaption", panel, new Vector2(0f, 0f), new Vector2(900f, 30f),
                "Time window (00:00..24:00, whole hours)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            timeCaption.color = new Color(0.7f, 0.78f, 0.95f, 1f);

            var time = CreateRangeSlider(panel, "TimeSlider", new Vector2(0f, -60f),
                min: 0f, max: 24f, low: 9f, high: 18f, minDistance: 1f, wholeNumbers: true, fillColor: TimeFillColor);

            var timeStatus = CreateText("TimeStatus", panel, new Vector2(0f, -115f), new Vector2(900f, 32f),
                "Time: 09:00 — 18:00", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            timeStatus.color = StatusColor;

            var hint = CreateText("Hint", panel, new Vector2(0f, -260f), new Vector2(900f, 80f),
                "Handles can't cross and keep a minimum gap. Track clicks jump the nearer handle (animated); dragging tracks the pointer. Arrow keys nudge the last-grabbed handle.",
                18, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UIRangeSliderDemoPresenter>();
            SetObjectReference(presenter, "priceSlider", price);
            SetObjectReference(presenter, "timeSlider", time);
            SetObjectReference(presenter, "priceStatusLabel", priceStatus);
            SetObjectReference(presenter, "timeStatusLabel", timeStatus);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"RangeSlider demo scene created: {ScenePath}");
        }

        private static UIRangeSliderControl CreateRangeSlider(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            float min,
            float max,
            float low,
            float high,
            float minDistance,
            bool wholeNumbers,
            Color fillColor)
        {
            const float trackWidth = 640f;
            const float railHeight = 10f;
            const float handleSize = 34f;
            const float dragBandHeight = 56f;

            var rootGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(UIRangeSliderControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(trackWidth + handleSize * 2f, dragBandHeight);

            // The whole band is the drag/click surface; track/handles don't block raycasts.
            var bg = rootGo.GetComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.02f);
            bg.raycastTarget = true;

            // Track (rail).
            var trackGo = new GameObject("Track", typeof(RectTransform), typeof(Image));
            var trackRect = trackGo.GetComponent<RectTransform>();
            trackRect.SetParent(rootRect, false);
            trackRect.anchorMin = new Vector2(0.5f, 0.5f);
            trackRect.anchorMax = new Vector2(0.5f, 0.5f);
            trackRect.pivot = new Vector2(0.5f, 0.5f);
            trackRect.sizeDelta = new Vector2(trackWidth, railHeight);
            trackRect.anchoredPosition = Vector2.zero;
            var railImage = trackGo.GetComponent<Image>();
            railImage.color = RailColor;
            railImage.raycastTarget = false;

            // Fill (between handles) — child of track, drawn above the rail.
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.SetParent(trackRect, false);
            fillRect.anchorMin = new Vector2(0.5f, 0.5f);
            fillRect.anchorMax = new Vector2(0.5f, 0.5f);
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fillRect.sizeDelta = new Vector2(0f, railHeight);
            fillRect.anchoredPosition = Vector2.zero;
            var fillImage = fillGo.GetComponent<Image>();
            fillImage.color = fillColor;
            fillImage.raycastTarget = false;

            var lowHandle = CreateHandle(trackRect, "LowHandle", handleSize);
            var highHandle = CreateHandle(trackRect, "HighHandle", handleSize);

            var control = rootGo.GetComponent<UIRangeSliderControl>();
            ConfigureRangeSlider(control, min, max, low, high, minDistance, wholeNumbers,
                trackRect, lowHandle, highHandle, fillRect);

            return control;
        }

        private static RectTransform CreateHandle(RectTransform parent, string name, float size)
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
            image.color = HandleColor;
            image.raycastTarget = false;

            return rect;
        }

        private static void ConfigureRangeSlider(
            UIRangeSliderControl control,
            float min,
            float max,
            float low,
            float high,
            float minDistance,
            bool wholeNumbers,
            RectTransform track,
            RectTransform lowHandle,
            RectTransform highHandle,
            RectTransform fill)
        {
            var so = new SerializedObject(control);

            so.FindProperty("minLimit").floatValue = min;
            so.FindProperty("maxLimit").floatValue = max;
            so.FindProperty("lowValue").floatValue = low;
            so.FindProperty("highValue").floatValue = high;
            so.FindProperty("minDistance").floatValue = minDistance;
            so.FindProperty("wholeNumbers").boolValue = wholeNumbers;

            so.FindProperty("track").objectReferenceValue = track;
            so.FindProperty("lowHandle").objectReferenceValue = lowHandle;
            so.FindProperty("highHandle").objectReferenceValue = highHandle;
            so.FindProperty("fill").objectReferenceValue = fill;

            var tween = so.FindProperty("moveTween");
            tween.FindPropertyRelative("duration").floatValue = 0.16f;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutCubic;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            so.FindProperty("interactable").boolValue = true;
            so.FindProperty("canvasGroup").objectReferenceValue = control.GetComponent<CanvasGroup>();
            so.FindProperty("disabledAlpha").floatValue = 0.55f;

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
