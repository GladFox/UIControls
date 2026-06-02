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
    public static class UIBottomSheetDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIBottomSheetDemo.unity";

        private static readonly Color PageColor = new Color(0.07f, 0.1f, 0.16f, 1f);
        private static readonly Color SheetColor = new Color(0.14f, 0.18f, 0.26f, 1f);
        private static readonly Color GrabberColor = new Color(0.5f, 0.56f, 0.68f, 1f);
        private static readonly Color ButtonColor = new Color(0.26f, 0.56f, 0.96f, 1f);
        private static readonly Color CloseButtonColor = new Color(0.86f, 0.34f, 0.38f, 1f);
        private static readonly Color LabelColor = new Color(0.98f, 0.99f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create BottomSheet Demo Scene")]
        public static void CreateBottomSheetDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateBottomSheetDemoSceneBatch()
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

            // --- Background page content (behind everything) ---
            var page = CreateStretchPanel(canvasRect, "Page", PageColor, raycast: false);

            var title = CreateText("Title", page, new Vector2(0f, 360f), new Vector2(900f, 60f),
                "UIControls BottomSheet Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", page, new Vector2(0f, 300f), new Vector2(1000f, 60f),
                "Open the sheet, then drag it between snap points. Flick down or tap the dimmed backdrop to close.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var openButton = CreateButton(page, "OpenButton", new Vector2(0f, 140f), new Vector2(360f, 84f),
                "Open bottom sheet", ButtonColor);

            var status = CreateText("Status", page, new Vector2(0f, 40f), new Vector2(900f, 36f),
                "Sheet: closed", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            // --- Backdrop dimmer (above page, below sheet) ---
            var backdropGo = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var backdropRect = backdropGo.GetComponent<RectTransform>();
            backdropRect.SetParent(canvasRect, false);
            Stretch(backdropRect);
            var backdropImage = backdropGo.GetComponent<Image>();
            backdropImage.color = new Color(0f, 0f, 0f, 1f);
            backdropImage.raycastTarget = true;
            var backdropGroup = backdropGo.GetComponent<CanvasGroup>();
            backdropGroup.alpha = 0f;
            backdropGroup.blocksRaycasts = false;

            // --- Sheet (top) ---
            const float sheetHeight = 900f;
            const float closedY = -sheetHeight;
            var collapsedY = -(sheetHeight - 380f); // ~380px peeking
            const float expandedY = 0f;

            var sheetGo = new GameObject("BottomSheet", typeof(RectTransform), typeof(Image), typeof(UIBottomSheetControl));
            var sheetRect = sheetGo.GetComponent<RectTransform>();
            sheetRect.SetParent(canvasRect, false);
            sheetRect.anchorMin = new Vector2(0.5f, 0f);
            sheetRect.anchorMax = new Vector2(0.5f, 0f);
            sheetRect.pivot = new Vector2(0.5f, 0f);
            sheetRect.sizeDelta = new Vector2(960f, sheetHeight);
            sheetRect.anchoredPosition = new Vector2(0f, closedY);
            var sheetImage = sheetGo.GetComponent<Image>();
            sheetImage.color = SheetColor;
            sheetImage.raycastTarget = true;

            var grabber = new GameObject("Grabber", typeof(RectTransform), typeof(Image));
            var grabberRect = grabber.GetComponent<RectTransform>();
            grabberRect.SetParent(sheetRect, false);
            grabberRect.anchorMin = new Vector2(0.5f, 1f);
            grabberRect.anchorMax = new Vector2(0.5f, 1f);
            grabberRect.pivot = new Vector2(0.5f, 1f);
            grabberRect.sizeDelta = new Vector2(80f, 8f);
            grabberRect.anchoredPosition = new Vector2(0f, -18f);
            var grabberImage = grabber.GetComponent<Image>();
            grabberImage.color = GrabberColor;
            grabberImage.raycastTarget = false;

            var sheetTitle = CreateText("SheetTitle", sheetRect, new Vector2(0f, -90f), new Vector2(820f, 50f),
                "Details", 30, FontStyles.Bold, TextAlignmentOptions.Center);
            sheetTitle.color = LabelColor;
            sheetTitle.raycastTarget = false;

            var sheetBody = CreateText("SheetBody", sheetRect, new Vector2(0f, -210f), new Vector2(820f, 200f),
                "Drag this sheet up to expand, down to collapse. A quick flick down — or a tap on the dimmed area behind — dismisses it. The backdrop darkens the further it opens.",
                20, FontStyles.Normal, TextAlignmentOptions.Top);
            sheetBody.color = new Color(0.82f, 0.86f, 0.95f, 1f);
            sheetBody.raycastTarget = false;

            var expandButton = CreateButton(sheetRect, "ExpandButton", new Vector2(-150f, -360f), new Vector2(260f, 76f),
                "Expand", ButtonColor);
            var closeButton = CreateButton(sheetRect, "CloseButton", new Vector2(150f, -360f), new Vector2(260f, 76f),
                "Close", CloseButtonColor);

            var control = sheetGo.GetComponent<UIBottomSheetControl>();
            ConfigureSheet(control, sheetRect, backdropGroup, closedY, collapsedY, expandedY);

            // --- Presenter ---
            var presenter = page.gameObject.AddComponent<UIBottomSheetDemoPresenter>();
            SetObjectReference(presenter, "sheet", control);
            SetObjectReference(presenter, "openButton", openButton);
            SetObjectReference(presenter, "expandButton", expandButton);
            SetObjectReference(presenter, "closeButton", closeButton);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"BottomSheet demo scene created: {ScenePath}");
        }

        private static void ConfigureSheet(
            UIBottomSheetControl control,
            RectTransform sheet,
            CanvasGroup backdrop,
            float closedY,
            float collapsedY,
            float expandedY)
        {
            var so = new SerializedObject(control);

            so.FindProperty("sheet").objectReferenceValue = sheet;
            so.FindProperty("backdrop").objectReferenceValue = backdrop;
            so.FindProperty("closedY").floatValue = closedY;

            var snaps = so.FindProperty("openSnapPoints");
            snaps.arraySize = 2;
            snaps.GetArrayElementAtIndex(0).floatValue = collapsedY;
            snaps.GetArrayElementAtIndex(1).floatValue = expandedY;

            so.FindProperty("initialSnapIndex").intValue = -1;
            so.FindProperty("dismissBelowY").floatValue = closedY + 220f;
            so.FindProperty("flickDismissSpeed").floatValue = 1300f;
            so.FindProperty("backdropMaxAlpha").floatValue = 0.6f;
            so.FindProperty("closeOnBackdropClick").boolValue = true;

            var tween = so.FindProperty("snapTween");
            tween.FindPropertyRelative("duration").floatValue = 0.4f;
            tween.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutBack;
            tween.FindPropertyRelative("delay").floatValue = 0f;
            tween.FindPropertyRelative("independentUpdate").boolValue = false;

            so.FindProperty("interactable").boolValue = true;

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
            // UIButtonControl sits on a Graphic-less root, so its UIStateAnimator does NOT auto-bind
            // the background as a color target (which would force it to white on the Normal state and
            // make the label blend in). The colored fill lives on a child; hover/press animate the
            // root's scale instead, keeping the label fully readable at rest and on highlight.
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

        private static RectTransform CreateStretchPanel(RectTransform parent, string name, Color color, bool raycast)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
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
