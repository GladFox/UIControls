using System;
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
    public static class UIBadgeDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIBadgeDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color IconColor = new Color(0.24f, 0.29f, 0.4f, 1f);
        private static readonly Color BadgeColor = new Color(0.9f, 0.27f, 0.3f, 1f);
        private static readonly Color ButtonColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color LabelColor = new Color(0.97f, 0.98f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create Badge Demo Scene")]
        public static void CreateBadgeDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateBadgeDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 300f), new Vector2(900f, 60f),
                "UIControls Badge Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            // Icon with badge.
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.SetParent(panel, false);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0f, 90f);
            iconRect.sizeDelta = new Vector2(140f, 140f);
            iconGo.GetComponent<Image>().color = IconColor;

            var glyph = CreateText("Glyph", iconRect, Vector2.zero, new Vector2(140f, 140f), "✉", 72, FontStyles.Normal, TextAlignmentOptions.Center);
            glyph.color = LabelColor;

            // Badge bubble at top-right corner of the icon.
            var bubbleGo = new GameObject("Badge", typeof(RectTransform), typeof(Image), typeof(UIBadgeControl));
            var bubbleRect = bubbleGo.GetComponent<RectTransform>();
            bubbleRect.SetParent(iconRect, false);
            bubbleRect.anchorMin = new Vector2(1f, 1f);
            bubbleRect.anchorMax = new Vector2(1f, 1f);
            bubbleRect.pivot = new Vector2(0.5f, 0.5f);
            bubbleRect.sizeDelta = new Vector2(56f, 56f);
            bubbleRect.anchoredPosition = new Vector2(-6f, -6f);
            bubbleGo.GetComponent<Image>().color = BadgeColor;

            var countLabel = CreateText("Count", bubbleRect, Vector2.zero, new Vector2(56f, 56f), "0", 26, FontStyles.Bold, TextAlignmentOptions.Center);
            countLabel.color = LabelColor;

            var badge = bubbleGo.GetComponent<UIBadgeControl>();
            var so = new SerializedObject(badge);
            so.FindProperty("bubble").objectReferenceValue = bubbleRect;
            so.FindProperty("countLabel").objectReferenceValue = countLabel;
            so.FindProperty("count").intValue = 3;
            so.FindProperty("maxDisplay").intValue = 99;
            so.FindProperty("showZero").boolValue = false;
            so.FindProperty("dotMode").boolValue = false;
            so.FindProperty("popScale").floatValue = 1.35f;
            so.FindProperty("popDuration").floatValue = 0.25f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var minus = CreateButton(panel, "MinusButton", new Vector2(-110f, -80f), new Vector2(140f, 72f), "−", ButtonColor);
            var plus = CreateButton(panel, "PlusButton", new Vector2(110f, -80f), new Vector2(140f, 72f), "+", ButtonColor);

            var status = CreateText("Status", panel, new Vector2(0f, -180f), new Vector2(900f, 36f),
                "Count: 3", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var presenter = panel.gameObject.AddComponent<UIBadgeDemoPresenter>();
            SetObjectReference(presenter, "badge", badge);
            SetObjectReference(presenter, "plusButton", plus);
            SetObjectReference(presenter, "minusButton", minus);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Badge demo scene created: {ScenePath}");
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
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
            bgGo.GetComponent<Image>().color = color;

            var text = CreateText("Label", rect, Vector2.zero, size, label, 30, FontStyles.Bold, TextAlignmentOptions.Center);
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
            rect.sizeDelta = new Vector2(960f, 700f);

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
