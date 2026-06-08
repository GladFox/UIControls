using System;
using System.IO;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIControls.Editor
{
    /// <summary>
    /// Shared helpers for the category F–I demo scene builders: camera/event-system/canvas/panel
    /// scaffolding, themed text and buttons, and scene saving into grouped sub-folders. Keeps each
    /// per-control builder small and visually consistent.
    /// </summary>
    public static class UIDemoSceneFactory
    {
        public static readonly Color Background = new Color(0.05f, 0.08f, 0.14f, 1f);
        public static readonly Color Panel = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        public static readonly Color Accent = new Color(0.24f, 0.55f, 0.95f, 1f);
        public static readonly Color Track = new Color(0.13f, 0.18f, 0.27f, 1f);
        public static readonly Color Label = new Color(0.97f, 0.98f, 1f, 1f);
        public static readonly Color Status = new Color(0.82f, 0.88f, 1f, 1f);
        public static readonly Color Muted = new Color(0.75f, 0.8f, 0.95f, 1f);
        public static readonly Color TitleColor = new Color(0.92f, 0.95f, 1f, 1f);

        /// <summary>
        /// Creates a fresh empty scene with camera, event system, canvas and a centered dark panel
        /// carrying a title. Returns the panel RectTransform for the caller to populate.
        /// </summary>
        public static RectTransform NewScene(string title, out Scene scene, Vector2? panelSize = null)
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();
            var panel = CreatePanel(canvas.transform as RectTransform, panelSize ?? new Vector2(960f, 760f));

            var titleText = Text("Title", panel, new Vector2(0f, panel.sizeDelta.y * 0.5f - 60f), new Vector2(900f, 60f),
                title, 36, FontStyles.Bold, TextAlignmentOptions.Center);
            titleText.color = TitleColor;

            return panel;
        }

        public static void Save(Scene scene, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            EditorSceneManager.SaveScene(scene, path, true);
            AddSceneToBuildSettings(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Demo scene created: {path}");
        }

        public static TextMeshProUGUI Text(
            string name,
            RectTransform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            string content,
            int fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.color = Label;
            text.raycastTarget = false;
            return text;
        }

        public static TextMeshProUGUI Caption(RectTransform parent, Vector2 anchoredPosition, string content, float width = 860f)
        {
            var cap = Text("Caption", parent, anchoredPosition, new Vector2(width, 40f),
                content, 18, FontStyles.Italic, TextAlignmentOptions.Center);
            cap.color = Muted;
            return cap;
        }

        public static Image Image(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size, Color color, bool raycast = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        /// <summary>
        /// Themed button: a Graphic-less root (so UIStateAnimator doesn't force the label white) with a
        /// child "Bg" image and a centered label. Returns the UIButtonControl on the root.
        /// </summary>
        public static UIButtonControl Button(
            RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size, string label, Color? color = null, int fontSize = 26)
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
            bgGo.GetComponent<Image>().color = color ?? Accent;

            var text = Text("Label", rect, Vector2.zero, size, label, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = Label;

            return go.GetComponent<UIButtonControl>();
        }

        public static void SetRef(UnityEngine.Object target, string property, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static RectTransform CreatePanel(RectTransform parent, Vector2 size)
        {
            var go = new GameObject("DemoPanel", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = Panel;
            img.raycastTarget = false;
            return rect;
        }

        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera");
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            go.transform.position = new Vector3(0f, 0f, -10f);
            go.transform.rotation = Quaternion.identity;
            go.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

            var inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                go.AddComponent(inputSystemModuleType);
                return;
            }

            go.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
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
            Array.Copy(scenes, updated, scenes.Length);
            updated[scenes.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
