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
    public static class UIVirtualListDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIVirtualListDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color ViewportColor = new Color(0.12f, 0.15f, 0.22f, 1f);
        private static readonly Color CellColor = new Color(0.18f, 0.22f, 0.31f, 1f);
        private static readonly Color CellLabelColor = new Color(0.9f, 0.93f, 1f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create VirtualList Demo Scene")]
        public static void CreateVirtualListDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateVirtualListDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 380f), new Vector2(900f, 60f),
                "UIControls Virtual List Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var status = CreateText("Status", panel, new Vector2(0f, 326f), new Vector2(900f, 40f),
                "10,000 items · recycled cells", 19, FontStyles.Italic, TextAlignmentOptions.Center);
            status.color = StatusColor;

            // ScrollView root (ScrollRect + virtual list on the same object).
            var scrollGo = new GameObject("ScrollView",
                typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(UIVirtualListControl));
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.SetParent(panel, false);
            scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0f, -20f);
            scrollRect.sizeDelta = new Vector2(660f, 620f);
            var scrollBg = scrollGo.GetComponent<Image>();
            scrollBg.color = ViewportColor;
            scrollBg.raycastTarget = true;

            // Viewport (masked).
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(scrollRect, false);
            Stretch(viewport);
            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;

            // Content (no layout group — the control positions cells manually).
            var contentGo = new GameObject("Content", typeof(RectTransform));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);

            // Cell template (inactive prototype, cloned into the pool).
            var cellTemplate = CreateCellTemplate(content);

            // Configure ScrollRect.
            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.inertia = true;
            sr.decelerationRate = 0.135f;
            sr.scrollSensitivity = 24f;

            var control = scrollGo.GetComponent<UIVirtualListControl>();
            ConfigureList(control, sr, viewport, content, cellTemplate);

            var presenter = panel.gameObject.AddComponent<UIVirtualListDemoPresenter>();
            SetObjectReference(presenter, "list", control);
            SetObjectReference(presenter, "statusLabel", status);
            SetInt(presenter, "itemCount", 10000);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"VirtualList demo scene created: {ScenePath}");
        }

        private static RectTransform CreateCellTemplate(RectTransform content)
        {
            var go = new GameObject("CellTemplate", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(content, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 64f);
            var image = go.GetComponent<Image>();
            image.color = CellColor;
            image.raycastTarget = true;

            var label = CreateText("Label", rect, Vector2.zero, Vector2.zero, "Item", 22, FontStyles.Bold, TextAlignmentOptions.Left);
            label.color = CellLabelColor;
            label.raycastTarget = false;
            var lrect = label.rectTransform;
            lrect.anchorMin = Vector2.zero;
            lrect.anchorMax = Vector2.one;
            lrect.offsetMin = new Vector2(24f, 0f);
            lrect.offsetMax = new Vector2(-24f, 0f);

            go.SetActive(false);
            return rect;
        }

        private static void ConfigureList(
            UIVirtualListControl control,
            ScrollRect scrollRect,
            RectTransform viewport,
            RectTransform content,
            RectTransform cellTemplate)
        {
            var so = new SerializedObject(control);
            so.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            so.FindProperty("viewport").objectReferenceValue = viewport;
            so.FindProperty("content").objectReferenceValue = content;
            so.FindProperty("cellTemplate").objectReferenceValue = cellTemplate;
            so.FindProperty("cellHeight").floatValue = 64f;
            so.FindProperty("spacing").floatValue = 8f;
            so.FindProperty("paddingTop").floatValue = 8f;
            so.FindProperty("paddingBottom").floatValue = 8f;
            so.FindProperty("bufferCells").intValue = 2;
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
            rect.sizeDelta = new Vector2(960f, 900f);

            var image = panelGo.GetComponent<Image>();
            image.color = PanelColor;
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
            text.raycastTarget = false;

            return text;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(UnityEngine.Object target, string propertyName, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).intValue = value;
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
