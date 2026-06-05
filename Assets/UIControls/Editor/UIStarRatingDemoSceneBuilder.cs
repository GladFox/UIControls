using System;
using System.Collections.Generic;
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
    public static class UIStarRatingDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIStarRatingDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color EmptyStarColor = new Color(0.3f, 0.34f, 0.44f, 1f);
        private static readonly Color FilledStarColor = new Color(1f, 0.78f, 0.28f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create StarRating Demo Scene")]
        public static void CreateStarRatingDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateStarRatingDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 320f), new Vector2(900f, 60f),
                "UIControls Star Rating Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var rateCaption = CreateText("RateCaption", panel, new Vector2(0f, 220f), new Vector2(900f, 32f),
                "Click or drag to rate (half-stars allowed)", 22, FontStyles.Italic, TextAlignmentOptions.Center);
            rateCaption.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var interactive = CreateStarRow(panel, new Vector2(0f, 150f), 5, 0f, allowHalf: true, readOnly: false);

            var status = CreateText("Status", panel, new Vector2(0f, 70f), new Vector2(900f, 36f),
                "Your rating: 0 / 5", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var roCaption = CreateText("RoCaption", panel, new Vector2(0f, -40f), new Vector2(900f, 32f),
                "Read-only display (3.5 stars)", 22, FontStyles.Italic, TextAlignmentOptions.Center);
            roCaption.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var readOnly = CreateStarRow(panel, new Vector2(0f, -110f), 5, 3.5f, allowHalf: true, readOnly: true);

            var hint = CreateText("Hint", panel, new Vector2(0f, -240f), new Vector2(900f, 40f),
                "Hover to preview; move off to keep your committed rating.",
                18, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UIStarRatingDemoPresenter>();
            SetObjectReference(presenter, "interactiveRating", interactive);
            SetObjectReference(presenter, "readOnlyRating", readOnly);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"StarRating demo scene created: {ScenePath}");
        }

        private static UIStarRatingControl CreateStarRow(
            RectTransform parent, Vector2 anchoredPosition, int count, float value, bool allowHalf, bool readOnly)
        {
            const float starSize = 72f;
            const float spacing = 10f;
            var width = count * starSize + (count - 1) * spacing;

            var rootGo = new GameObject(readOnly ? "ReadOnlyStars" : "InteractiveStars",
                typeof(RectTransform), typeof(Image), typeof(UIStarRatingControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(width, starSize);
            var rootImage = rootGo.GetComponent<Image>();
            rootImage.color = new Color(1f, 1f, 1f, 0.001f);
            rootImage.raycastTarget = !readOnly;

            var fills = new RectTransform[count];
            var roots = new RectTransform[count];
            var startX = -width * 0.5f + starSize * 0.5f;

            for (var i = 0; i < count; i++)
            {
                var starGo = new GameObject($"Star_{i + 1}", typeof(RectTransform));
                var starRect = starGo.GetComponent<RectTransform>();
                starRect.SetParent(rootRect, false);
                starRect.anchorMin = new Vector2(0.5f, 0.5f);
                starRect.anchorMax = new Vector2(0.5f, 0.5f);
                starRect.pivot = new Vector2(0.5f, 0.5f);
                starRect.sizeDelta = new Vector2(starSize, starSize);
                starRect.anchoredPosition = new Vector2(startX + i * (starSize + spacing), 0f);

                var empty = CreateStarGlyph("Empty", starRect, starSize, EmptyStarColor);
                FullStretch(empty.rectTransform);

                var viewportGo = new GameObject("FillViewport", typeof(RectTransform), typeof(RectMask2D));
                var viewport = viewportGo.GetComponent<RectTransform>();
                viewport.SetParent(starRect, false);
                viewport.anchorMin = new Vector2(0f, 0f);
                viewport.anchorMax = new Vector2(0f, 1f);
                viewport.pivot = new Vector2(0f, 0.5f);
                viewport.anchoredPosition = Vector2.zero;
                viewport.sizeDelta = new Vector2(0f, 0f);

                var bright = CreateStarGlyph("Fill", viewport, starSize, FilledStarColor);
                var bRect = bright.rectTransform;
                bRect.anchorMin = new Vector2(0f, 0.5f);
                bRect.anchorMax = new Vector2(0f, 0.5f);
                bRect.pivot = new Vector2(0f, 0.5f);
                bRect.sizeDelta = new Vector2(starSize, starSize);
                bRect.anchoredPosition = Vector2.zero;

                fills[i] = viewport;
                roots[i] = starRect;
            }

            var control = rootGo.GetComponent<UIStarRatingControl>();
            ConfigureRating(control, fills, roots, value, allowHalf, readOnly);
            return control;
        }

        private static TextMeshProUGUI CreateStarGlyph(string name, RectTransform parent, float size, Color color)
        {
            var glyph = CreateText(name, parent, Vector2.zero, new Vector2(size, size), "★", Mathf.RoundToInt(size * 0.86f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            glyph.color = color;
            glyph.raycastTarget = false;
            return glyph;
        }

        private static void ConfigureRating(
            UIStarRatingControl control,
            IReadOnlyList<RectTransform> fills,
            IReadOnlyList<RectTransform> roots,
            float value,
            bool allowHalf,
            bool readOnly)
        {
            var so = new SerializedObject(control);
            var fillsProp = so.FindProperty("starFills");
            fillsProp.arraySize = fills.Count;
            for (var i = 0; i < fills.Count; i++)
            {
                fillsProp.GetArrayElementAtIndex(i).objectReferenceValue = fills[i];
            }

            var rootsProp = so.FindProperty("starRoots");
            rootsProp.arraySize = roots.Count;
            for (var i = 0; i < roots.Count; i++)
            {
                rootsProp.GetArrayElementAtIndex(i).objectReferenceValue = roots[i];
            }

            so.FindProperty("value").floatValue = value;
            so.FindProperty("allowHalf").boolValue = allowHalf;
            so.FindProperty("readOnly").boolValue = readOnly;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void FullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
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
