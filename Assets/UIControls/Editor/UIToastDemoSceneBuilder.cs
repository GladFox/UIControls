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
    public static class UIToastDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Overlays(B)/UIToastDemo.unity";

        private static readonly Color PageColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color ToastColor = new Color(0.16f, 0.2f, 0.29f, 1f);
        private static readonly Color InfoColor = new Color(0.26f, 0.56f, 0.96f, 1f);
        private static readonly Color SuccessColor = new Color(0.2f, 0.65f, 0.42f, 1f);
        private static readonly Color ErrorColor = new Color(0.86f, 0.34f, 0.38f, 1f);
        private static readonly Color NeutralButtonColor = new Color(0.24f, 0.29f, 0.4f, 1f);
        private static readonly Color ActionButtonColor = new Color(0.98f, 0.84f, 0.45f, 1f);
        private static readonly Color LabelColor = new Color(0.98f, 0.99f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create Toast Demo Scene")]
        public static void CreateToastDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateToastDemoSceneBatch()
        {
            CreateDemoScene();
        }

        private static void CreateDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            var canvasRect = canvas.transform as RectTransform;
            var panel = CreateStretchPanel(canvasRect, "Page", PageColor);

            var title = CreateText("Title", panel, new Vector2(0f, 360f), new Vector2(900f, 60f),
                "UIControls Toast / Snackbar Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 300f), new Vector2(1000f, 60f),
                "Trigger toasts — they queue and play one at a time. Snackbar carries an action; swipe a toast down to dismiss early.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            // Trigger buttons row.
            var infoButton = CreateButton(panel, "InfoButton", new Vector2(-360f, 120f), new Vector2(220f, 72f), "Info", InfoColor);
            var successButton = CreateButton(panel, "SuccessButton", new Vector2(-120f, 120f), new Vector2(220f, 72f), "Success", SuccessColor);
            var errorButton = CreateButton(panel, "ErrorButton", new Vector2(120f, 120f), new Vector2(220f, 72f), "Error", ErrorColor);
            var snackbarButton = CreateButton(panel, "SnackbarButton", new Vector2(360f, 120f), new Vector2(220f, 72f), "Snackbar", NeutralButtonColor);

            var status = CreateText("Status", panel, new Vector2(0f, 20f), new Vector2(900f, 36f),
                "Trigger a toast above.", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            // --- Toast slot (bottom-center) ---
            const float toastWidth = 760f;
            const float toastHeight = 96f;
            var toastGo = new GameObject("Toast", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(UIToastControl));
            var toastRect = toastGo.GetComponent<RectTransform>();
            toastRect.SetParent(canvasRect, false);
            toastRect.anchorMin = new Vector2(0.5f, 0f);
            toastRect.anchorMax = new Vector2(0.5f, 0f);
            toastRect.pivot = new Vector2(0.5f, 0f);
            toastRect.sizeDelta = new Vector2(toastWidth, toastHeight);
            toastRect.anchoredPosition = new Vector2(0f, -200f);
            var toastImage = toastGo.GetComponent<Image>();
            toastImage.color = ToastColor;
            toastImage.raycastTarget = true;
            var toastGroup = toastGo.GetComponent<CanvasGroup>();
            toastGroup.alpha = 0f;

            // Accent strip (left).
            var accentGo = new GameObject("Accent", typeof(RectTransform), typeof(Image));
            var accentRect = accentGo.GetComponent<RectTransform>();
            accentRect.SetParent(toastRect, false);
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(10f, 0f);
            accentRect.anchoredPosition = Vector2.zero;
            var accentImage = accentGo.GetComponent<Image>();
            accentImage.color = InfoColor;
            accentImage.raycastTarget = false;

            // Message (left area).
            var message = CreateText("Message", toastRect, Vector2.zero, Vector2.zero,
                "Message", 22, FontStyles.Normal, TextAlignmentOptions.Left);
            var messageRect = message.rectTransform;
            messageRect.anchorMin = new Vector2(0f, 0f);
            messageRect.anchorMax = new Vector2(1f, 1f);
            messageRect.offsetMin = new Vector2(36f, 0f);
            messageRect.offsetMax = new Vector2(-180f, 0f);
            message.color = LabelColor;
            message.raycastTarget = false;

            // Action button (right).
            var actionButton = CreateButton(toastRect, "ActionButton", Vector2.zero, new Vector2(140f, 64f), "ACTION", ActionButtonColor);
            var actionRect = actionButton.transform as RectTransform;
            actionRect.anchorMin = new Vector2(1f, 0.5f);
            actionRect.anchorMax = new Vector2(1f, 0.5f);
            actionRect.pivot = new Vector2(1f, 0.5f);
            actionRect.anchoredPosition = new Vector2(-16f, 0f);
            var actionLabel = actionButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (actionLabel != null)
            {
                actionLabel.color = new Color(0.1f, 0.12f, 0.16f, 1f); // dark text on bright action button
            }

            var control = toastGo.GetComponent<UIToastControl>();
            ConfigureToast(control, toastRect, toastGroup, message, accentImage, actionButton, actionLabel);

            var presenter = panel.gameObject.AddComponent<UIToastDemoPresenter>();
            SetObjectReference(presenter, "toast", control);
            SetObjectReference(presenter, "infoButton", infoButton);
            SetObjectReference(presenter, "successButton", successButton);
            SetObjectReference(presenter, "errorButton", errorButton);
            SetObjectReference(presenter, "snackbarButton", snackbarButton);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Toast demo scene created: {ScenePath}");
        }

        private static void ConfigureToast(
            UIToastControl control,
            RectTransform panel,
            CanvasGroup group,
            TMP_Text message,
            Graphic accent,
            UIButtonControl actionButton,
            TMP_Text actionLabel)
        {
            var so = new SerializedObject(control);

            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("canvasGroup").objectReferenceValue = group;
            so.FindProperty("messageLabel").objectReferenceValue = message;
            so.FindProperty("accent").objectReferenceValue = accent;
            so.FindProperty("actionButton").objectReferenceValue = actionButton;
            so.FindProperty("actionButtonLabel").objectReferenceValue = actionLabel;

            so.FindProperty("hiddenY").floatValue = -200f;
            so.FindProperty("shownY").floatValue = 60f;
            so.FindProperty("defaultDuration").floatValue = 2.5f;
            so.FindProperty("swipeDismissDistance").floatValue = 60f;

            var showT = so.FindProperty("showTween");
            showT.FindPropertyRelative("duration").floatValue = 0.32f;
            showT.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutBack;
            showT.FindPropertyRelative("delay").floatValue = 0f;
            showT.FindPropertyRelative("independentUpdate").boolValue = false;

            var hideT = so.FindProperty("hideTween");
            hideT.FindPropertyRelative("duration").floatValue = 0.24f;
            hideT.FindPropertyRelative("ease").enumValueIndex = (int)Ease.InQuad;
            hideT.FindPropertyRelative("delay").floatValue = 0f;
            hideT.FindPropertyRelative("independentUpdate").boolValue = false;

            so.FindProperty("infoColor").colorValue = InfoColor;
            so.FindProperty("successColor").colorValue = SuccessColor;
            so.FindProperty("errorColor").colorValue = ErrorColor;
            so.FindProperty("maxQueued").intValue = 8;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UIButtonControl CreateButton(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            string label,
            Color color)
        {
            // Graphic-less root so UIButtonControl's animator does not force the fill to white.
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
            Stretch(bgRect);
            var image = bgGo.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            var text = CreateText("Label", rect, Vector2.zero, size, label, 22, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = LabelColor;
            text.raycastTarget = false;

            return go.GetComponent<UIButtonControl>();
        }

        private static RectTransform CreateStretchPanel(RectTransform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
