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
    public static class UIChipGroupDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIChipGroupDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color GroupBackdropColor = new Color(1f, 1f, 1f, 0.02f);
        private static readonly Color NormalBgColor = new Color(0.18f, 0.22f, 0.31f, 1f);
        private static readonly Color RadioSelectedColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color TagSelectedColor = new Color(0.18f, 0.62f, 0.45f, 1f);
        private static readonly Color NormalLabelColor = new Color(0.82f, 0.86f, 0.95f, 1f);
        private static readonly Color SelectedLabelColor = Color.white;
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create ChipGroup Demo Scene")]
        public static void CreateChipGroupDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateChipGroupDemoSceneBatch()
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
                "UIControls ChipGroup Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 320f), new Vector2(900f, 36f),
                "Single-select behaves like a radio group; multi-select toggles each chip independently.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var radioCaption = CreateText("RadioCaption", panel, new Vector2(0f, 230f), new Vector2(900f, 30f),
                "Quality (single-select / radio)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            radioCaption.color = new Color(0.7f, 0.78f, 0.95f, 1f);

            var radioGroup = CreateChipGroup(
                panel,
                "RadioGroup",
                new Vector2(0f, 170f),
                new[] { "Low", "Medium", "High", "Ultra" },
                UIChipGroup.SelectionMode.Single,
                RadioSelectedColor,
                initialIndex: 1);

            var radioStatus = CreateText("RadioStatus", panel, new Vector2(0f, 110f), new Vector2(900f, 32f),
                "Single-select: Medium (index 1)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            radioStatus.color = StatusColor;

            var tagsCaption = CreateText("TagsCaption", panel, new Vector2(0f, 10f), new Vector2(900f, 30f),
                "Topics (multi-select / tags)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            tagsCaption.color = new Color(0.7f, 0.78f, 0.95f, 1f);

            var tagsGroup = CreateChipGroup(
                panel,
                "TagsGroup",
                new Vector2(0f, -50f),
                new[] { "Unity", "C#", "Shaders", "UI", "Tools" },
                UIChipGroup.SelectionMode.Multi,
                TagSelectedColor,
                initialIndex: 0);

            var tagsStatus = CreateText("TagsStatus", panel, new Vector2(0f, -110f), new Vector2(900f, 32f),
                "Multi-select: (none)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            tagsStatus.color = StatusColor;

            var hint = CreateText("Hint", panel, new Vector2(0f, -260f), new Vector2(900f, 80f),
                "No sliding indicator: each chip animates its own state (color + pop). Radio keeps exactly one on; tags allow any number. Arrow keys move focus when the group is selected; Submit toggles.",
                18, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UIChipGroupDemoPresenter>();
            SetObjectReference(presenter, "radioGroup", radioGroup);
            SetObjectReference(presenter, "tagsGroup", tagsGroup);
            SetObjectReference(presenter, "radioStatusLabel", radioStatus);
            SetObjectReference(presenter, "tagsStatusLabel", tagsStatus);
            SetStringArray(presenter, "radioNames", new[] { "Low", "Medium", "High", "Ultra" });
            SetStringArray(presenter, "tagNames", new[] { "Unity", "C#", "Shaders", "UI", "Tools" });

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"ChipGroup demo scene created: {ScenePath}");
        }

        private static UIChipGroup CreateChipGroup(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            string[] labels,
            UIChipGroup.SelectionMode mode,
            Color selectedColor,
            int initialIndex)
        {
            const float height = 56f;
            const float spacing = 16f;

            var widths = new float[labels.Length];
            var total = 0f;
            for (var i = 0; i < labels.Length; i++)
            {
                widths[i] = Mathf.Max(96f, labels[i].Length * 15f + 52f);
                total += widths[i];
            }

            total += spacing * (labels.Length - 1);

            var rootGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(UIChipGroup));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(total + 40f, height + 24f);

            // The whole group rect is the click surface; chips don't block raycasts, so clicks
            // fall through to the group, which hit-tests the position to find a chip.
            var bg = rootGo.GetComponent<Image>();
            bg.color = GroupBackdropColor;
            bg.raycastTarget = true;

            var roots = new RectTransform[labels.Length];
            var backgrounds = new Image[labels.Length];
            var chipLabels = new TextMeshProUGUI[labels.Length];

            var startX = -total * 0.5f;
            var cursor = startX;
            for (var i = 0; i < labels.Length; i++)
            {
                var w = widths[i];
                var centerX = cursor + w * 0.5f;
                cursor += w + spacing;

                var chipGo = new GameObject($"Chip_{i + 1}", typeof(RectTransform), typeof(Image));
                var chipRect = chipGo.GetComponent<RectTransform>();
                chipRect.SetParent(rootRect, false);
                chipRect.anchorMin = new Vector2(0.5f, 0.5f);
                chipRect.anchorMax = new Vector2(0.5f, 0.5f);
                chipRect.pivot = new Vector2(0.5f, 0.5f);
                chipRect.sizeDelta = new Vector2(w, height);
                chipRect.anchoredPosition = new Vector2(centerX, 0f);

                var chipBg = chipGo.GetComponent<Image>();
                chipBg.color = NormalBgColor;
                chipBg.raycastTarget = false;

                var label = CreateText("Label", chipRect, Vector2.zero, new Vector2(w, height),
                    labels[i], 20, FontStyles.Bold, TextAlignmentOptions.Center);
                label.color = NormalLabelColor;
                label.raycastTarget = false;

                roots[i] = chipRect;
                backgrounds[i] = chipBg;
                chipLabels[i] = label;
            }

            var control = rootGo.GetComponent<UIChipGroup>();
            ConfigureChipGroup(control, roots, backgrounds, chipLabels, mode, selectedColor, initialIndex);

            // Pre-paint initial state for single-select so the scene preview matches runtime.
            if (mode == UIChipGroup.SelectionMode.Single)
            {
                var sel = Mathf.Clamp(initialIndex, 0, labels.Length - 1);
                backgrounds[sel].color = selectedColor;
                chipLabels[sel].color = SelectedLabelColor;
            }

            return control;
        }

        private static void ConfigureChipGroup(
            UIChipGroup control,
            IReadOnlyList<RectTransform> roots,
            IReadOnlyList<Image> backgrounds,
            IReadOnlyList<TextMeshProUGUI> labels,
            UIChipGroup.SelectionMode mode,
            Color selectedColor,
            int initialIndex)
        {
            var so = new SerializedObject(control);

            so.FindProperty("mode").enumValueIndex = (int)mode;
            so.FindProperty("allowEmptyInSingle").boolValue = false;
            so.FindProperty("initialIndex").intValue = initialIndex;

            so.FindProperty("normalBackgroundColor").colorValue = NormalBgColor;
            so.FindProperty("selectedBackgroundColor").colorValue = selectedColor;
            so.FindProperty("normalLabelColor").colorValue = NormalLabelColor;
            so.FindProperty("selectedLabelColor").colorValue = SelectedLabelColor;

            var tween = so.FindProperty("toggleTween");
            tween.FindPropertyRelative("duration").floatValue = 0.22f;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutQuad;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            so.FindProperty("popOnToggle").boolValue = true;
            so.FindProperty("popScale").floatValue = 1.08f;
            so.FindProperty("focusScale").floatValue = 1.05f;

            so.FindProperty("interactable").boolValue = true;
            so.FindProperty("canvasGroup").objectReferenceValue = control.GetComponent<CanvasGroup>();
            so.FindProperty("disabledAlpha").floatValue = 0.55f;

            var chipsProp = so.FindProperty("chips");
            chipsProp.arraySize = roots.Count;
            for (var i = 0; i < roots.Count; i++)
            {
                var element = chipsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("root").objectReferenceValue = roots[i];
                element.FindPropertyRelative("background").objectReferenceValue = backgrounds[i];
                element.FindPropertyRelative("label").objectReferenceValue = labels[i];
                element.FindPropertyRelative("checkmark").objectReferenceValue = null;
            }

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

        private static void SetStringArray(UnityEngine.Object target, string propertyName, IReadOnlyList<string> values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            prop.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).stringValue = values[i];
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
