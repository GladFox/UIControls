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
    public static class UITooltipDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UITooltipDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color MarkerColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color MarkerLabelColor = new Color(0.98f, 0.99f, 1f, 1f);
        private static readonly Color BubbleColor = new Color(0.12f, 0.15f, 0.22f, 1f);
        private static readonly Color BubbleTextColor = new Color(0.95f, 0.97f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create Tooltip Demo Scene")]
        public static void CreateTooltipDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateTooltipDemoSceneBatch()
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

            // Center info panel.
            var panel = CreatePanel(canvasRect, new Vector2(820f, 360f));
            var title = CreateText("Title", panel, new Vector2(0f, 110f), new Vector2(760f, 60f),
                "UIControls Tooltip Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 40f), new Vector2(760f, 80f),
                "Hover the markers near the screen edges. Each prefers a side, but the tooltip auto-flips so it never runs off screen.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var status = CreateText("Status", panel, new Vector2(0f, -60f), new Vector2(760f, 40f),
                "Hover a marker to see its tooltip.", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            // Shared tooltip bubble (on top).
            var tooltip = CreateTooltip(canvasRect);

            // Markers near edges; each prefers the side toward the edge so it must flip.
            CreateMarker(canvasRect, "TopLeft", new Vector2(0f, 1f), new Vector2(120f, -120f),
                "Top-left marker — prefers Top, flips to Bottom near the edge.", UITooltipControl.Placement.Top, tooltip);
            CreateMarker(canvasRect, "TopRight", new Vector2(1f, 1f), new Vector2(-120f, -120f),
                "Top-right marker — prefers Top, flips to Bottom.", UITooltipControl.Placement.Top, tooltip);
            CreateMarker(canvasRect, "BottomLeft", new Vector2(0f, 0f), new Vector2(120f, 120f),
                "Bottom-left marker — prefers Bottom, flips to Top.", UITooltipControl.Placement.Bottom, tooltip);
            CreateMarker(canvasRect, "BottomRight", new Vector2(1f, 0f), new Vector2(-120f, 120f),
                "Bottom-right marker — prefers Bottom, flips to Top.", UITooltipControl.Placement.Bottom, tooltip);
            CreateMarker(canvasRect, "LeftEdge", new Vector2(0f, 0.5f), new Vector2(120f, 0f),
                "Left marker — prefers Left, flips to Right.", UITooltipControl.Placement.Left, tooltip);
            CreateMarker(canvasRect, "RightEdge", new Vector2(1f, 0.5f), new Vector2(-120f, 0f),
                "Right marker — prefers Right, flips to Left.", UITooltipControl.Placement.Right, tooltip);

            var presenter = panel.gameObject.AddComponent<UITooltipDemoPresenter>();
            SetObjectReference(presenter, "tooltip", tooltip);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Tooltip demo scene created: {ScenePath}");
        }

        private static UITooltipControl CreateTooltip(RectTransform canvasRect)
        {
            var go = new GameObject("Tooltip", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(UITooltipControl));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(canvasRect, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(240f, 80f);
            rect.anchoredPosition = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = BubbleColor;
            image.raycastTarget = false;

            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var label = CreateText("Label", rect, Vector2.zero, new Vector2(280f, 40f),
                "Tooltip", 19, FontStyles.Normal, TextAlignmentOptions.Center);
            label.color = BubbleTextColor;
            label.raycastTarget = false;

            rect.SetAsLastSibling();

            var control = go.GetComponent<UITooltipControl>();
            var so = new SerializedObject(control);
            so.FindProperty("bubble").objectReferenceValue = rect;
            so.FindProperty("canvasGroup").objectReferenceValue = group;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("bounds").objectReferenceValue = canvasRect;
            so.FindProperty("maxWidth").floatValue = 320f;
            so.FindProperty("padding").floatValue = 16f;
            so.FindProperty("gap").floatValue = 12f;
            so.FindProperty("edgePadding").floatValue = 16f;

            var showT = so.FindProperty("showTween");
            showT.FindPropertyRelative("duration").floatValue = 0.14f;
            showT.FindPropertyRelative("ease").enumValueIndex = (int)Ease.OutQuad;
            showT.FindPropertyRelative("delay").floatValue = 0f;
            showT.FindPropertyRelative("independentUpdate").boolValue = false;

            var hideT = so.FindProperty("hideTween");
            hideT.FindPropertyRelative("duration").floatValue = 0.12f;
            hideT.FindPropertyRelative("ease").enumValueIndex = (int)Ease.InQuad;
            hideT.FindPropertyRelative("delay").floatValue = 0f;
            hideT.FindPropertyRelative("independentUpdate").boolValue = false;

            so.ApplyModifiedPropertiesWithoutUndo();
            return control;
        }

        private static void CreateMarker(
            RectTransform canvasRect,
            string name,
            Vector2 anchor,
            Vector2 anchoredPosition,
            string text,
            UITooltipControl.Placement placement,
            UITooltipControl tooltip)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UITooltipTrigger));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(canvasRect, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = new Vector2(88f, 88f);
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = MarkerColor;
            image.raycastTarget = true;

            var label = CreateText("Label", rect, Vector2.zero, new Vector2(88f, 88f),
                "?", 40, FontStyles.Bold, TextAlignmentOptions.Center);
            label.color = MarkerLabelColor;
            label.raycastTarget = false;

            var trigger = go.GetComponent<UITooltipTrigger>();
            var so = new SerializedObject(trigger);
            so.FindProperty("tooltip").objectReferenceValue = tooltip;
            so.FindProperty("text").stringValue = text;
            so.FindProperty("placement").enumValueIndex = (int)placement;
            so.FindProperty("hoverDelay").floatValue = 0.3f;
            so.FindProperty("longPress").boolValue = true;
            so.FindProperty("longPressTime").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform CreatePanel(RectTransform parent, Vector2 size)
        {
            var panelGo = new GameObject("DemoPanel", typeof(RectTransform), typeof(Image));
            var rect = panelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

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
            text.raycastTarget = false; // labels never intercept pointer events in this demo

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
