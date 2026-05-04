using System;
using System.Collections.Generic;
using TMPro;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIRubberBandPrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIRubberBandPrototype.unity";

        private static readonly Color BackgroundColor = new Color(0.05f, 0.08f, 0.14f, 1f);
        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color StationBarColor = new Color(0.17f, 0.21f, 0.3f, 1f);
        private static readonly Color StationButtonColor = new Color(0.22f, 0.27f, 0.36f, 1f);
        private static readonly Color RubberRectColor = new Color(0.95f, 0.45f, 0.18f, 1f);
        private static readonly Color RubberLabelColor = new Color(1f, 0.97f, 0.85f, 1f);
        private static readonly Color StationLabelColor = new Color(0.85f, 0.9f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        private const float StationWidth = 200f;
        private const float StationHeight = 70f;
        private const float StationSpacing = 20f;
        private const int StationCount = 4;

        [MenuItem("UIControls/Create RubberBand Prototype Scene")]
        public static void CreateFromMenu()
        {
            CreateScene();
        }

        public static void CreateBatch()
        {
            CreateScene();
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();

            var panel = CreatePanel(canvas.transform as RectTransform);

            CreateText(panel, "Title", new Vector2(0f, 360f), new Vector2(900f, 60f),
                "Rubber Band Slide — Prototype", 36, FontStyles.Bold, TextAlignmentOptions.Center,
                new Color(0.92f, 0.95f, 1f));

            CreateText(panel, "Subtitle", new Vector2(0f, 310f), new Vector2(900f, 36f),
                "Click stations to slide. Lead edge starts first, trail catches up — stretches text & image vertices uniformly via localScale.x.",
                18, FontStyles.Italic, TextAlignmentOptions.Center,
                new Color(0.75f, 0.8f, 0.95f));

            // Station bar background
            var bar = CreateStationBar(panel, new Vector2(0f, 80f),
                new Vector2(StationCount * StationWidth + (StationCount - 1) * StationSpacing + 40f, StationHeight + 40f));

            var stations = new RectTransform[StationCount];
            var buttons = new Button[StationCount];
            var totalWidth = StationCount * StationWidth + (StationCount - 1) * StationSpacing;
            var startX = -totalWidth * 0.5f + StationWidth * 0.5f;
            for (var i = 0; i < StationCount; i++)
            {
                var x = startX + i * (StationWidth + StationSpacing);
                var (rect, btn) = CreateStation(bar, $"Station_{i + 1}", $"Station {i + 1}", new Vector2(x, 0f));
                stations[i] = rect;
                buttons[i] = btn;
            }

            // Rubber rect — sized to one station, contains an image + a TMP label so we can see all
            // vertices stretching together. The rect is a sibling under the bar so its anchored position
            // shares the same parent space as the stations.
            var rubber = CreateRubberRect(bar, "RubberRect", "RUBBER", new Vector2(StationWidth, StationHeight));

            var statusLabel = CreateText(panel, "Status", new Vector2(0f, -50f), new Vector2(900f, 36f),
                "Selected station: 0", 20, FontStyles.Bold, TextAlignmentOptions.Center, StatusColor);

            CreateText(panel, "Hint", new Vector2(0f, -120f), new Vector2(900f, 80f),
                "Tune duration / lag / leadEase / trailEase in the Inspector on the controller. Try clicking the same station twice (no-op) and clicking far stations (longer travel = more stretch).",
                16, FontStyles.Italic, TextAlignmentOptions.Center,
                new Color(0.7f, 0.78f, 0.95f));

            // Wire controller via SerializedObject so all references resolve at scene-save time.
            var controller = panel.gameObject.AddComponent<UIRubberBandSlideDemo>();
            var so = new SerializedObject(controller);
            so.FindProperty("rubberRect").objectReferenceValue = rubber;

            var stationsProp = so.FindProperty("stations");
            stationsProp.arraySize = stations.Length;
            for (var i = 0; i < stations.Length; i++)
            {
                stationsProp.GetArrayElementAtIndex(i).objectReferenceValue = stations[i];
            }

            var buttonsProp = so.FindProperty("stationButtons");
            buttonsProp.arraySize = buttons.Length;
            for (var i = 0; i < buttons.Length; i++)
            {
                buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            }

            so.FindProperty("statusLabel").objectReferenceValue = statusLabel;
            so.FindProperty("duration").floatValue = 0.55f;
            so.FindProperty("lag").floatValue = 0.4f;
            so.FindProperty("leadEase").enumValueIndex = (int)DG.Tweening.Ease.OutCubic;
            so.FindProperty("trailEase").enumValueIndex = (int)DG.Tweening.Ease.OutCubic;
            so.FindProperty("initialIndex").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"RubberBand prototype scene created: {ScenePath}");
        }

        private static RectTransform CreatePanel(RectTransform parent)
        {
            var go = new GameObject("DemoPanel", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(1100f, 800f);

            var image = go.GetComponent<Image>();
            image.color = PanelColor;
            image.raycastTarget = false;
            return rect;
        }

        private static RectTransform CreateStationBar(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject("StationBar", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = StationBarColor;
            image.raycastTarget = false;
            return rect;
        }

        private static (RectTransform rect, Button button) CreateStation(RectTransform parent, string name, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(StationWidth, StationHeight);

            var image = go.GetComponent<Image>();
            image.color = StationButtonColor;
            image.raycastTarget = true;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;

            CreateText(rect, "Label", Vector2.zero, new Vector2(StationWidth, StationHeight),
                label, 22, FontStyles.Bold, TextAlignmentOptions.Center, StationLabelColor);

            return (rect, button);
        }

        private static RectTransform CreateRubberRect(RectTransform parent, string name, string label, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = RubberRectColor;
            image.raycastTarget = false;

            CreateText(rect, "RubberLabel", Vector2.zero, size,
                label, 28, FontStyles.Bold, TextAlignmentOptions.Center, RubberLabelColor);

            // Indicator must render on top of the station bar but below the station buttons —
            // putting it as the first sibling under the bar means it's behind the stations,
            // letting the station Image color show outside the rubber rect. We want stations on
            // top so they remain clickable AND the indicator visibly slides under them.
            rect.SetAsFirstSibling();
            return rect;
        }

        private static TMP_Text CreateText(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            string content,
            int fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera");
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            go.transform.position = new Vector3(0f, 0f, -10f);
            go.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

            var inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                go.AddComponent(inputSystemModuleType);
                return;
            }

            go.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
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

            var updated = new List<EditorBuildSettingsScene>(scenes) { new EditorBuildSettingsScene(path, true) };
            EditorBuildSettings.scenes = updated.ToArray();
        }
    }
}
