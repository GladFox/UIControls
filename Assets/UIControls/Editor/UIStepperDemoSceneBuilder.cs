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
    public static class UIStepperDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIStepperDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color TrackColor = new Color(0.15f, 0.19f, 0.27f, 1f);
        private static readonly Color ButtonColor = new Color(0.22f, 0.27f, 0.37f, 1f);
        private static readonly Color ArrowEnabledColor = new Color(0.92f, 0.95f, 1f, 1f);
        private static readonly Color ArrowDisabledColor = new Color(0.92f, 0.95f, 1f, 0.28f);
        private static readonly Color ValueColor = new Color(0.95f, 0.97f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create Stepper Demo Scene")]
        public static void CreateStepperDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateStepperDemoSceneBatch()
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
                "UIControls Stepper Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 320f), new Vector2(900f, 36f),
                "Tap to nudge; hold a button for accelerating auto-repeat. Arrows dim at bounds.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var qtyCaption = CreateText("QtyCaption", panel, new Vector2(0f, 210f), new Vector2(900f, 30f),
                "Quantity (integer, 0..99, hold-to-repeat)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            qtyCaption.color = new Color(0.7f, 0.78f, 0.95f, 1f);

            var quantity = CreateStepper(panel, "QuantityStepper", new Vector2(0f, 150f),
                initial: 1f, min: 0f, max: 99f, step: 1f, format: "0");

            var qtyStatus = CreateText("QtyStatus", panel, new Vector2(0f, 90f), new Vector2(900f, 32f),
                "Quantity: 1 item(s)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            qtyStatus.color = StatusColor;

            var volCaption = CreateText("VolCaption", panel, new Vector2(0f, 0f), new Vector2(900f, 30f),
                "Volume (fractional, 0..1, step 0.05)", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            volCaption.color = new Color(0.7f, 0.78f, 0.95f, 1f);

            var volume = CreateStepper(panel, "VolumeStepper", new Vector2(0f, -60f),
                initial: 0.5f, min: 0f, max: 1f, step: 0.05f, format: "0.00");

            var volStatus = CreateText("VolStatus", panel, new Vector2(0f, -120f), new Vector2(900f, 32f),
                "Volume: 50%", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            volStatus.color = StatusColor;

            var hint = CreateText("Hint", panel, new Vector2(0f, -260f), new Vector2(900f, 80f),
                "Hold [-] or [+] to watch the repeat accelerate. The value label pops on each change; arrows fade when min/max is reached. Arrow keys also step when focused.",
                18, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.72f, 0.78f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UIStepperDemoPresenter>();
            SetObjectReference(presenter, "quantityStepper", quantity);
            SetObjectReference(presenter, "volumeStepper", volume);
            SetObjectReference(presenter, "quantityStatusLabel", qtyStatus);
            SetObjectReference(presenter, "volumeStatusLabel", volStatus);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Stepper demo scene created: {ScenePath}");
        }

        private static UIStepperControl CreateStepper(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            float initial,
            float min,
            float max,
            float step,
            string format)
        {
            const float height = 72f;
            const float buttonSize = 72f;
            const float valueWidth = 160f;
            var width = buttonSize * 2f + valueWidth;

            var rootGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(UIStepperControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(width, height);

            // Whole stepper is the click surface; the control resolves which button by position.
            var bg = rootGo.GetComponent<Image>();
            bg.color = TrackColor;
            bg.raycastTarget = true;

            var decrementRect = CreateButton(rootRect, "Decrement", new Vector2(-(valueWidth + buttonSize) * 0.5f, 0f),
                new Vector2(buttonSize, height), "−", out var decrementArrow);
            var incrementRect = CreateButton(rootRect, "Increment", new Vector2((valueWidth + buttonSize) * 0.5f, 0f),
                new Vector2(buttonSize, height), "+", out var incrementArrow);

            var valueLabel = CreateText("Value", rootRect, Vector2.zero, new Vector2(valueWidth, height),
                initial.ToString(format, System.Globalization.CultureInfo.InvariantCulture),
                34, FontStyles.Bold, TextAlignmentOptions.Center);
            valueLabel.color = ValueColor;
            valueLabel.raycastTarget = false;

            var control = rootGo.GetComponent<UIStepperControl>();
            ConfigureStepper(control, initial, min, max, step, format,
                decrementRect, incrementRect, decrementArrow, incrementArrow, valueLabel);

            return control;
        }

        private static RectTransform CreateButton(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            string symbol,
            out TextMeshProUGUI arrow)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = ButtonColor;
            image.raycastTarget = false;

            arrow = CreateText("Symbol", rect, Vector2.zero, size, symbol, 44, FontStyles.Bold, TextAlignmentOptions.Center);
            arrow.color = ArrowEnabledColor;
            arrow.raycastTarget = false;

            return rect;
        }

        private static void ConfigureStepper(
            UIStepperControl control,
            float initial,
            float min,
            float max,
            float step,
            string format,
            RectTransform decrementRect,
            RectTransform incrementRect,
            Graphic decrementArrow,
            Graphic incrementArrow,
            TMP_Text valueLabel)
        {
            var so = new SerializedObject(control);

            so.FindProperty("value").floatValue = initial;
            so.FindProperty("min").floatValue = min;
            so.FindProperty("max").floatValue = max;
            so.FindProperty("step").floatValue = step;
            so.FindProperty("wrapAround").boolValue = false;
            so.FindProperty("valueFormat").stringValue = format;

            so.FindProperty("decrementButton").objectReferenceValue = decrementRect;
            so.FindProperty("incrementButton").objectReferenceValue = incrementRect;
            so.FindProperty("decrementGraphic").objectReferenceValue = decrementArrow;
            so.FindProperty("incrementGraphic").objectReferenceValue = incrementArrow;
            so.FindProperty("valueLabel").objectReferenceValue = valueLabel;

            so.FindProperty("holdDelay").floatValue = 0.4f;
            so.FindProperty("repeatInterval").floatValue = 0.15f;
            so.FindProperty("repeatAcceleration").floatValue = 0.85f;
            so.FindProperty("minRepeatInterval").floatValue = 0.04f;

            var tween = so.FindProperty("labelTween");
            tween.FindPropertyRelative("duration").floatValue = 0.18f;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutQuad;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            so.FindProperty("popOnChange").boolValue = true;
            so.FindProperty("popScale").floatValue = 1.18f;

            so.FindProperty("arrowEnabledColor").colorValue = ArrowEnabledColor;
            so.FindProperty("arrowDisabledColor").colorValue = ArrowDisabledColor;

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
