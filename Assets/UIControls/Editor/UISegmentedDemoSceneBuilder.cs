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
    public static class UISegmentedDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UISegmentedDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color ContainerColor = new Color(0.17f, 0.21f, 0.3f, 1f);
        private static readonly Color ThumbPrimaryColor = new Color(0.9f, 0.93f, 1f, 1f);
        private static readonly Color ThumbSecondaryColor = new Color(0.98f, 0.84f, 0.45f, 1f);
        private static readonly Color NormalLabelColor = new Color(0.82f, 0.86f, 0.95f, 1f);
        private static readonly Color SelectedLabelColor = new Color(0.07f, 0.1f, 0.16f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create Segmented Demo Scene")]
        public static void CreateSegmentedDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateSegmentedDemoSceneBatch()
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
                "UIControls Segmented Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 320f), new Vector2(900f, 36f),
                "Variant A: 3 segments that switch views. Variant B: 4 segments, event-only with rubber-band.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            // Variant A — view-switching segmented control (plain slide).
            var viewSegmented = CreateSegmented(
                panel,
                "ViewSegmented",
                new Vector2(0f, 210f),
                new Vector2(720f, 72f),
                new[] { "List", "Grid", "Map" },
                ThumbPrimaryColor,
                rubberBand: false,
                duration: 0.28f,
                out _);

            var viewsRoot = CreateViewsRoot(panel, new Vector2(0f, 70f), new Vector2(720f, 150f));
            var viewMessages = new[]
            {
                "LIST — compact rows of items",
                "GRID — thumbnail tiles",
                "MAP — geographic pins",
            };
            var viewColors = new[]
            {
                new Color(0.2f, 0.5f, 0.32f, 1f),
                new Color(0.36f, 0.32f, 0.6f, 1f),
                new Color(0.55f, 0.32f, 0.32f, 1f),
            };
            var views = new GameObject[viewMessages.Length];
            for (var i = 0; i < viewMessages.Length; i++)
            {
                views[i] = CreateViewPanel(viewsRoot, $"View_{i + 1}", viewMessages[i], viewColors[i]);
                views[i].SetActive(i == 0);
            }

            var viewStatus = CreateText("ViewStatus", panel, new Vector2(0f, -40f), new Vector2(900f, 36f),
                "View segmented: showing view 1", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            viewStatus.color = StatusColor;

            // Variant B — event-only segmented control (rubber-band).
            var eventSegmented = CreateSegmented(
                panel,
                "EventSegmented",
                new Vector2(0f, -130f),
                new Vector2(820f, 72f),
                new[] { "Low", "Medium", "High", "Ultra" },
                ThumbSecondaryColor,
                rubberBand: true,
                duration: 0.4f,
                out _);

            var eventStatus = CreateText("EventStatus", panel, new Vector2(0f, -210f), new Vector2(900f, 36f),
                "Event-only segmented: index = 0", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            eventStatus.color = StatusColor;

            var hint = CreateText("Hint", panel, new Vector2(0f, -320f), new Vector2(900f, 56f),
                "Thumb slides under the active segment; the selected label recolors. Arrow keys move the selection when focused.",
                18, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UISegmentedDemoPresenter>();
            SetObjectReference(presenter, "viewSegmented", viewSegmented);
            SetObjectReference(presenter, "eventSegmented", eventSegmented);
            SetObjectReference(presenter, "viewStatusLabel", viewStatus);
            SetObjectReference(presenter, "eventStatusLabel", eventStatus);
            SetObjectArray(presenter, "views", views);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Segmented demo scene created: {ScenePath}");
        }

        private static UISegmentedControl CreateSegmented(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            string[] labels,
            Color thumbColor,
            bool rubberBand,
            float duration,
            out RectTransform thumb)
        {
            var rootGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(UISegmentedControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = size;

            // Container background is the whole clickable surface; the control hit-tests segments by position.
            var bg = rootGo.GetComponent<Image>();
            bg.color = ContainerColor;
            bg.raycastTarget = true;

            var thumbPadding = new Vector2(4f, 4f);
            var segWidth = size.x / labels.Length;
            var segSize = new Vector2(segWidth, size.y);

            // Thumb first so it renders above the container background but below the labels.
            thumb = CreateThumb(rootRect, "Thumb", thumbColor, new Vector2(segWidth - thumbPadding.x * 2f, size.y - thumbPadding.y * 2f));

            var segmentRoots = new RectTransform[labels.Length];
            var segmentLabels = new TextMeshProUGUI[labels.Length];
            var startX = -size.x * 0.5f + segWidth * 0.5f;
            for (var i = 0; i < labels.Length; i++)
            {
                var segGo = new GameObject($"Segment_{i + 1}", typeof(RectTransform));
                var segRect = segGo.GetComponent<RectTransform>();
                segRect.SetParent(rootRect, false);
                segRect.anchorMin = new Vector2(0.5f, 0.5f);
                segRect.anchorMax = new Vector2(0.5f, 0.5f);
                segRect.pivot = new Vector2(0.5f, 0.5f);
                segRect.sizeDelta = segSize;
                segRect.anchoredPosition = new Vector2(startX + i * segWidth, 0f);

                var label = CreateText("Label", segRect, Vector2.zero, segSize,
                    labels[i], 22, FontStyles.Bold, TextAlignmentOptions.Center);
                label.color = i == 0 ? SelectedLabelColor : NormalLabelColor;
                label.raycastTarget = false;

                segmentRoots[i] = segRect;
                segmentLabels[i] = label;
            }

            // Align thumb to first segment.
            thumb.anchoredPosition = segmentRoots[0].anchoredPosition;

            var control = rootGo.GetComponent<UISegmentedControl>();
            ConfigureSegmented(control, thumb, thumbPadding, segmentRoots, segmentLabels, rubberBand, duration);

            return control;
        }

        private static RectTransform CreateThumb(RectTransform parent, string name, Color color, Vector2 size)
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
            image.color = color;
            image.raycastTarget = false;

            rect.SetAsFirstSibling();
            return rect;
        }

        private static void ConfigureSegmented(
            UISegmentedControl control,
            RectTransform thumb,
            Vector2 thumbPadding,
            IReadOnlyList<RectTransform> roots,
            IReadOnlyList<TextMeshProUGUI> labels,
            bool rubberBand,
            float duration)
        {
            var so = new SerializedObject(control);

            so.FindProperty("thumb").objectReferenceValue = thumb;
            var pad = so.FindProperty("thumbPadding");
            pad.vector2Value = thumbPadding;

            so.FindProperty("normalLabelColor").colorValue = NormalLabelColor;
            so.FindProperty("selectedLabelColor").colorValue = SelectedLabelColor;
            so.FindProperty("tintIcons").boolValue = true;
            so.FindProperty("initialIndex").intValue = 0;

            var tween = so.FindProperty("slideTween");
            tween.FindPropertyRelative("duration").floatValue = duration;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutCubic;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            so.FindProperty("rubberBand").boolValue = rubberBand;
            so.FindProperty("rubberBandLag").floatValue = 0.35f;
            so.FindProperty("rubberBandLeadEase").enumValueIndex = (int)Ease.OutCubic;
            so.FindProperty("rubberBandTrailEase").enumValueIndex = (int)Ease.OutCubic;

            so.FindProperty("interactable").boolValue = true;
            so.FindProperty("canvasGroup").objectReferenceValue = control.GetComponent<CanvasGroup>();
            so.FindProperty("disabledAlpha").floatValue = 0.55f;

            var segmentsProp = so.FindProperty("segments");
            segmentsProp.arraySize = roots.Count;
            for (var i = 0; i < roots.Count; i++)
            {
                var element = segmentsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("root").objectReferenceValue = roots[i];
                element.FindPropertyRelative("label").objectReferenceValue = labels[i];
                element.FindPropertyRelative("icon").objectReferenceValue = null;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
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
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            var text = CreateText("Caption", rect, Vector2.zero, new Vector2(680f, 120f),
                message, 26, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = Color.white;
            text.raycastTarget = false;

            return go;
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

        private static void SetObjectArray(UnityEngine.Object target, string propertyName, IReadOnlyList<GameObject> values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            prop.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

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
