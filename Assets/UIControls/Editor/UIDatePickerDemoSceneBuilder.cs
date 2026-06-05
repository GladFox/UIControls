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
    public static class UIDatePickerDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIDatePickerDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color CardColor = new Color(0.13f, 0.16f, 0.24f, 1f);
        private static readonly Color CellColor = new Color(0.16f, 0.2f, 0.29f, 1f);
        private static readonly Color SelectedColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color TodayColor = new Color(0.28f, 0.34f, 0.46f, 1f);
        private static readonly Color InMonthText = new Color(0.95f, 0.97f, 1f, 1f);
        private static readonly Color OutMonthText = new Color(0.5f, 0.56f, 0.68f, 1f);
        private static readonly Color HeaderColor = new Color(0.92f, 0.95f, 1f, 1f);
        private static readonly Color WeekdayColor = new Color(0.68f, 0.74f, 0.88f, 1f);
        private static readonly Color NavColor = new Color(0.24f, 0.29f, 0.4f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        private static readonly string[] Weekdays = { "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su" };

        [MenuItem("UIControls/Create DatePicker Demo Scene")]
        public static void CreateDatePickerDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateDatePickerDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 410f), new Vector2(900f, 60f),
                "UIControls Date Picker Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            const float cellW = 76f;
            const float cellH = 64f;
            const float spacing = 6f;
            var gridWidth = 7 * cellW + 6 * spacing;

            // Calendar card.
            var card = new GameObject("Calendar", typeof(RectTransform), typeof(Image), typeof(UIDatePickerControl));
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.SetParent(panel, false);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 1f);
            cardRect.anchoredPosition = new Vector2(0f, 350f);
            cardRect.sizeDelta = new Vector2(gridWidth + 48f, 620f);
            var cardImage = card.GetComponent<Image>();
            cardImage.color = CardColor;
            cardImage.raycastTarget = false;

            // Header row.
            var prev = CreateButton(cardRect, "Prev", new Vector2(0f, 0f), new Vector2(56f, 56f), "‹", NavColor);
            var prevRect = prev.transform as RectTransform;
            prevRect.anchorMin = new Vector2(0f, 1f); prevRect.anchorMax = new Vector2(0f, 1f); prevRect.pivot = new Vector2(0f, 1f);
            prevRect.anchoredPosition = new Vector2(24f, -22f);

            var next = CreateButton(cardRect, "Next", new Vector2(0f, 0f), new Vector2(56f, 56f), "›", NavColor);
            var nextRect = next.transform as RectTransform;
            nextRect.anchorMin = new Vector2(1f, 1f); nextRect.anchorMax = new Vector2(1f, 1f); nextRect.pivot = new Vector2(1f, 1f);
            nextRect.anchoredPosition = new Vector2(-24f, -22f);

            var header = CreateText("Header", cardRect, new Vector2(0f, -50f), new Vector2(gridWidth - 120f, 50f),
                "Month Year", 26, FontStyles.Bold, TextAlignmentOptions.Center);
            header.color = HeaderColor;
            var headerRect = header.rectTransform;
            headerRect.anchorMin = new Vector2(0.5f, 1f); headerRect.anchorMax = new Vector2(0.5f, 1f); headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -22f);

            var gridLeft = -gridWidth * 0.5f + cellW * 0.5f;

            // Weekday row.
            for (var c = 0; c < 7; c++)
            {
                var wd = CreateText($"WD_{c}", cardRect, Vector2.zero, new Vector2(cellW, 34f),
                    Weekdays[c], 18, FontStyles.Bold, TextAlignmentOptions.Center);
                wd.color = WeekdayColor;
                var wr = wd.rectTransform;
                wr.anchorMin = new Vector2(0.5f, 1f); wr.anchorMax = new Vector2(0.5f, 1f); wr.pivot = new Vector2(0.5f, 1f);
                wr.anchoredPosition = new Vector2(gridLeft + c * (cellW + spacing), -96f);
            }

            // Day grid (6 rows × 7 cols).
            var dayButtons = new UIButtonControl[42];
            var dayLabels = new TextMeshProUGUI[42];
            var dayBackgrounds = new Image[42];
            var gridTop = -136f;
            for (var r = 0; r < 6; r++)
            {
                for (var c = 0; c < 7; c++)
                {
                    var i = r * 7 + c;
                    var pos = new Vector2(gridLeft + c * (cellW + spacing), gridTop - r * (cellH + spacing));
                    CreateDayCell(cardRect, i, pos, new Vector2(cellW, cellH), out dayButtons[i], out dayLabels[i], out dayBackgrounds[i]);
                }
            }

            var control = card.GetComponent<UIDatePickerControl>();
            ConfigurePicker(control, header, prev, next, dayButtons, dayLabels, dayBackgrounds);

            var status = CreateText("Status", panel, new Vector2(0f, -360f), new Vector2(900f, 40f),
                "Selected: —", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var hint = CreateText("Hint", panel, new Vector2(0f, -420f), new Vector2(900f, 36f),
                "Use ‹ › to change month; click a day to select it. Today and the selection are highlighted.",
                18, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UIDatePickerDemoPresenter>();
            SetObjectReference(presenter, "picker", control);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"DatePicker demo scene created: {ScenePath}");
        }

        private static void CreateDayCell(
            RectTransform parent, int index, Vector2 anchoredPosition, Vector2 size,
            out UIButtonControl button, out TextMeshProUGUI label, out Image background)
        {
            var go = new GameObject($"Day_{index}", typeof(RectTransform), typeof(UIButtonControl));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var bgGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.SetParent(rect, false);
            FullStretch(bgRect);
            background = bgGo.GetComponent<Image>();
            background.color = CellColor;
            background.raycastTarget = true;

            label = CreateText("Label", rect, Vector2.zero, size, string.Empty, 22, FontStyles.Bold, TextAlignmentOptions.Center);
            label.color = InMonthText;

            button = go.GetComponent<UIButtonControl>();
        }

        private static void ConfigurePicker(
            UIDatePickerControl control,
            TMP_Text header,
            UIButtonControl prev,
            UIButtonControl next,
            IReadOnlyList<UIButtonControl> dayButtons,
            IReadOnlyList<TextMeshProUGUI> dayLabels,
            IReadOnlyList<Image> dayBackgrounds)
        {
            var so = new SerializedObject(control);
            so.FindProperty("headerLabel").objectReferenceValue = header;
            so.FindProperty("prevButton").objectReferenceValue = prev;
            so.FindProperty("nextButton").objectReferenceValue = next;
            so.FindProperty("mondayFirst").boolValue = true;
            so.FindProperty("cellColor").colorValue = CellColor;
            so.FindProperty("selectedColor").colorValue = SelectedColor;
            so.FindProperty("todayColor").colorValue = TodayColor;
            so.FindProperty("inMonthText").colorValue = InMonthText;
            so.FindProperty("outMonthText").colorValue = OutMonthText;

            SetArray(so, "dayButtons", dayButtons);
            SetArray(so, "dayLabels", dayLabels);
            SetArray(so, "dayBackgrounds", dayBackgrounds);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray<T>(SerializedObject so, string prop, IReadOnlyList<T> values) where T : UnityEngine.Object
        {
            var p = so.FindProperty(prop);
            p.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
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
            FullStretch(bgRect);
            var image = bgGo.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            var text = CreateText("Label", rect, Vector2.zero, size, label, 28, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = new Color(0.97f, 0.98f, 1f, 1f);

            return go.GetComponent<UIButtonControl>();
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
            rect.sizeDelta = new Vector2(960f, 980f);

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
