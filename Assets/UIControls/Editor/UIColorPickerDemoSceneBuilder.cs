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
    public static class UIColorPickerDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Input(D)/UIColorPickerDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create ColorPicker Demo Scene")]
        public static void CreateColorPickerDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateColorPickerDemoSceneBatch()
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
                "UIControls Color Picker Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 270f), new Vector2(900f, 40f),
                "Drag in the square for saturation/value, the bar for hue. HSV textures are generated at runtime.",
                19, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            const float svSize = 300f;
            const float hueHeight = 36f;
            const float gap = 16f;

            // Interactive root (catches pointer input over the square + hue bar).
            var rootGo = new GameObject("ColorPicker", typeof(RectTransform), typeof(Image), typeof(UIColorPickerControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(panel, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(-110f, -30f);
            rootRect.sizeDelta = new Vector2(svSize, svSize + gap + hueHeight);
            var rootImage = rootGo.GetComponent<Image>();
            rootImage.color = new Color(1f, 1f, 1f, 0.001f);
            rootImage.raycastTarget = true;

            // SV square (top).
            var svGo = new GameObject("SV", typeof(RectTransform), typeof(RawImage));
            var svRect = svGo.GetComponent<RectTransform>();
            svRect.SetParent(rootRect, false);
            svRect.anchorMin = new Vector2(0.5f, 1f);
            svRect.anchorMax = new Vector2(0.5f, 1f);
            svRect.pivot = new Vector2(0.5f, 1f);
            svRect.sizeDelta = new Vector2(svSize, svSize);
            svRect.anchoredPosition = Vector2.zero;
            var svImage = svGo.GetComponent<RawImage>();
            svImage.raycastTarget = false;

            var svCursor = CreateMarker(svRect, "SVCursor", 16f, Color.white);

            // Hue bar (below).
            var hueGo = new GameObject("Hue", typeof(RectTransform), typeof(RawImage));
            var hueRect = hueGo.GetComponent<RectTransform>();
            hueRect.SetParent(rootRect, false);
            hueRect.anchorMin = new Vector2(0.5f, 0f);
            hueRect.anchorMax = new Vector2(0.5f, 0f);
            hueRect.pivot = new Vector2(0.5f, 0f);
            hueRect.sizeDelta = new Vector2(svSize, hueHeight);
            hueRect.anchoredPosition = Vector2.zero;
            var hueImage = hueGo.GetComponent<RawImage>();
            hueImage.raycastTarget = false;

            var hueHandle = CreateMarker(hueRect, "HueHandle", 0f, Color.white);
            hueHandle.sizeDelta = new Vector2(8f, hueHeight + 10f);

            // Preview swatch + hex (right side).
            var previewGo = new GameObject("Preview", typeof(RectTransform), typeof(Image));
            var previewRect = previewGo.GetComponent<RectTransform>();
            previewRect.SetParent(panel, false);
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.pivot = new Vector2(0.5f, 0.5f);
            previewRect.anchoredPosition = new Vector2(260f, 40f);
            previewRect.sizeDelta = new Vector2(180f, 180f);
            var preview = previewGo.GetComponent<Image>();
            preview.color = Color.white;
            preview.raycastTarget = false;

            var hex = CreateText("Hex", panel, new Vector2(260f, -80f), new Vector2(260f, 50f),
                "#000000", 28, FontStyles.Bold, TextAlignmentOptions.Center);
            hex.color = new Color(0.95f, 0.97f, 1f, 1f);

            var control = rootGo.GetComponent<UIColorPickerControl>();
            var so = new SerializedObject(control);
            so.FindProperty("svImage").objectReferenceValue = svImage;
            so.FindProperty("svRect").objectReferenceValue = svRect;
            so.FindProperty("svCursor").objectReferenceValue = svCursor;
            so.FindProperty("hueImage").objectReferenceValue = hueImage;
            so.FindProperty("hueRect").objectReferenceValue = hueRect;
            so.FindProperty("hueHandle").objectReferenceValue = hueHandle;
            so.FindProperty("preview").objectReferenceValue = preview;
            so.FindProperty("hexLabel").objectReferenceValue = hex;
            so.FindProperty("initialColor").colorValue = new Color(0.24f, 0.55f, 0.95f, 1f);
            so.ApplyModifiedPropertiesWithoutUndo();

            var status = CreateText("Status", panel, new Vector2(0f, -300f), new Vector2(900f, 40f),
                "RGB (—)", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var presenter = panel.gameObject.AddComponent<UIColorPickerDemoPresenter>();
            SetObjectReference(presenter, "picker", control);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"ColorPicker demo scene created: {ScenePath}");
        }

        private static RectTransform CreateMarker(RectTransform parent, string name, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
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
