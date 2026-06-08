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
    public static class UIOTPInputDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Input(D)/UIOTPInputDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color EmptyColor = new Color(0.18f, 0.22f, 0.31f, 1f);
        private static readonly Color FilledColor = new Color(0.22f, 0.28f, 0.4f, 1f);
        private static readonly Color ActiveColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color CellTextColor = new Color(0.97f, 0.98f, 1f, 1f);
        private static readonly Color ButtonColor = new Color(0.24f, 0.29f, 0.4f, 1f);
        private static readonly Color LabelColor = new Color(0.98f, 0.99f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create OTPInput Demo Scene")]
        public static void CreateOTPInputDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateOTPInputDemoSceneBatch()
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
                "UIControls OTP Input Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 270f), new Vector2(900f, 40f),
                "Enter the 6-digit verification code.", 22, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var otp = CreateOTP(panel, "OTP", new Vector2(0f, 130f), 6);

            var status = CreateText("Status", panel, new Vector2(0f, 0f), new Vector2(900f, 40f),
                "Type or paste the code — focus auto-advances, backspace goes back.",
                20, FontStyles.Normal, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var clear = CreateButton(panel, "ClearButton", new Vector2(0f, -90f), new Vector2(200f, 72f), "Clear", ButtonColor);

            var hint = CreateText("Hint", panel, new Vector2(0f, -190f), new Vector2(900f, 40f),
                "Tip: paste a whole code at once, or use backspace to step back a cell.",
                18, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UIOTPInputDemoPresenter>();
            SetObjectReference(presenter, "otp", otp);
            SetObjectReference(presenter, "clearButton", clear);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"OTPInput demo scene created: {ScenePath}");
        }

        private static UIOTPInputControl CreateOTP(RectTransform parent, string name, Vector2 anchoredPosition, int length)
        {
            const float cellSize = 88f;
            const float cellHeight = 104f;
            const float spacing = 18f;
            var totalWidth = length * cellSize + (length - 1) * spacing;

            var rootGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UIOTPInputControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(totalWidth, cellHeight);
            // Transparent surface that catches clicks to focus the field.
            var rootImage = rootGo.GetComponent<Image>();
            rootImage.color = new Color(1f, 1f, 1f, 0.001f);
            rootImage.raycastTarget = true;

            // Hidden capture field (covers the row; its own text/caret are invisible).
            var input = CreateHiddenInputField(rootRect, new Vector2(totalWidth, cellHeight), length);

            var backgrounds = new Image[length];
            var labels = new TextMeshProUGUI[length];
            var startX = -totalWidth * 0.5f + cellSize * 0.5f;
            for (var i = 0; i < length; i++)
            {
                var cellGo = new GameObject($"Cell_{i + 1}", typeof(RectTransform), typeof(Image));
                var cellRect = cellGo.GetComponent<RectTransform>();
                cellRect.SetParent(rootRect, false);
                cellRect.anchorMin = new Vector2(0.5f, 0.5f);
                cellRect.anchorMax = new Vector2(0.5f, 0.5f);
                cellRect.pivot = new Vector2(0.5f, 0.5f);
                cellRect.sizeDelta = new Vector2(cellSize, cellHeight);
                cellRect.anchoredPosition = new Vector2(startX + i * (cellSize + spacing), 0f);
                var cellImage = cellGo.GetComponent<Image>();
                cellImage.color = EmptyColor;
                cellImage.raycastTarget = false;

                var label = CreateText("Label", cellRect, Vector2.zero, new Vector2(cellSize, cellHeight),
                    string.Empty, 40, FontStyles.Bold, TextAlignmentOptions.Center);
                label.color = CellTextColor;

                backgrounds[i] = cellImage;
                labels[i] = label;
            }

            var control = rootGo.GetComponent<UIOTPInputControl>();
            ConfigureOTP(control, input, backgrounds, labels);
            return control;
        }

        private static TMP_InputField CreateHiddenInputField(RectTransform parent, Vector2 size, int length)
        {
            var go = new GameObject("CaptureField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            var bg = go.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0f);
            bg.raycastTarget = false; // clicks are handled by the OTP root

            var textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            var areaRect = textArea.GetComponent<RectTransform>();
            areaRect.SetParent(rect, false);
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = Vector2.zero;
            areaRect.offsetMax = Vector2.zero;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.SetParent(areaRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.text = string.Empty;
            text.fontSize = 40;
            text.color = new Color(0f, 0f, 0f, 0f); // invisible — cells render the value
            text.richText = false;

            var input = go.GetComponent<TMP_InputField>();
            input.textViewport = areaRect;
            input.textComponent = text;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = length;
            input.customCaretColor = true;
            input.caretColor = new Color(0f, 0f, 0f, 0f);
            input.caretWidth = 1;
            input.selectionColor = new Color(0f, 0f, 0f, 0f);
            input.restoreOriginalTextOnEscape = false;

            return input;
        }

        private static void ConfigureOTP(
            UIOTPInputControl control,
            TMP_InputField input,
            IReadOnlyList<Image> backgrounds,
            IReadOnlyList<TextMeshProUGUI> labels)
        {
            var so = new SerializedObject(control);
            so.FindProperty("inputField").objectReferenceValue = input;
            so.FindProperty("digitsOnly").boolValue = true;
            so.FindProperty("mask").boolValue = false;
            so.FindProperty("maskChar").stringValue = "•";
            so.FindProperty("emptyColor").colorValue = EmptyColor;
            so.FindProperty("filledColor").colorValue = FilledColor;
            so.FindProperty("activeColor").colorValue = ActiveColor;

            var bgProp = so.FindProperty("cellBackgrounds");
            bgProp.arraySize = backgrounds.Count;
            for (var i = 0; i < backgrounds.Count; i++)
            {
                bgProp.GetArrayElementAtIndex(i).objectReferenceValue = backgrounds[i];
            }

            var labelProp = so.FindProperty("cellLabels");
            labelProp.arraySize = labels.Count;
            for (var i = 0; i < labels.Count; i++)
            {
                labelProp.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
            }

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
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
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
