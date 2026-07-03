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
    public static class UIStickyListDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Scrolling(C)/UIStickyListDemo.unity";

        private static readonly Color PanelColor  = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color ListColor   = new Color(0.11f, 0.14f, 0.22f, 1f);
        private static readonly Color HeaderColor = new Color(0.2f, 0.32f, 0.52f, 1f);
        private static readonly Color RowColor    = new Color(0.16f, 0.2f, 0.29f, 1f);
        private static readonly Color FooterColor = new Color(0.42f, 0.3f, 0.14f, 1f);
        private static readonly Color TextBright  = new Color(0.92f, 0.95f, 1f, 1f);
        private static readonly Color TextSoft    = new Color(0.78f, 0.84f, 0.95f, 1f);

        [MenuItem("UIControls/Create StickyList Demo Scene")]
        public static void CreateStickyListDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateStickyListDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 390f), new Vector2(900f, 56f),
                "UIControls Sticky List Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = TextBright;

            var hint = CreateText("Hint", panel, new Vector2(0f, 344f), new Vector2(900f, 36f),
                "Section headers pin to the top, the total row pins to the bottom", 19,
                FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.6f, 0.75f, 1f, 1f);

            // ScrollRect + UIStickyListControl
            var scrollGo = new GameObject("StickyList",
                typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(UIStickyListControl));
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.SetParent(panel, false);
            scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            scrollRt.anchoredPosition = new Vector2(0f, -40f);
            scrollRt.sizeDelta = new Vector2(680f, 660f);
            scrollGo.GetComponent<Image>().color = ListColor;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(scrollRt, false);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            var viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);

            var contentGo = new GameObject("Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var layout = contentGo.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.elasticity = 0.1f;
            sr.inertia = true;
            sr.decelerationRate = 0.135f;
            sr.scrollSensitivity = 26f;

            // Templates (inactive; the presenter clones them)
            var headerTemplate = CreateListEntry(content, "HeaderTemplate", 50f, HeaderColor, TextBright,
                21, FontStyles.Bold, sticky: UIStickyItemControl.StickyEdge.Top);
            var rowTemplate = CreateListEntry(content, "RowTemplate", 56f, RowColor, TextSoft,
                19, FontStyles.Normal, sticky: null);
            var footerTemplate = CreateListEntry(content, "FooterTemplate", 50f, FooterColor, TextBright,
                20, FontStyles.Bold, sticky: UIStickyItemControl.StickyEdge.Bottom);
            headerTemplate.SetActive(false);
            rowTemplate.SetActive(false);
            footerTemplate.SetActive(false);

            var presenter = panel.gameObject.AddComponent<UIStickyListDemoPresenter>();
            SetObjectReference(presenter, "content", content);
            SetObjectReference(presenter, "headerTemplate", headerTemplate);
            SetObjectReference(presenter, "rowTemplate", rowTemplate);
            SetObjectReference(presenter, "footerTemplate", footerTemplate);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"StickyList demo scene created: {ScenePath}");
        }

        private static GameObject CreateListEntry(
            RectTransform content,
            string name,
            float height,
            Color background,
            Color textColor,
            int fontSize,
            FontStyles fontStyle,
            UIStickyItemControl.StickyEdge? sticky)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(content, false);
            go.GetComponent<Image>().color = background;
            var element = go.GetComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;

            if (sticky.HasValue)
            {
                var item = go.AddComponent<UIStickyItemControl>();
                var so = new SerializedObject(item);
                so.FindProperty("edge").enumValueIndex = (int)sticky.Value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var label = CreateText("Label", rect, Vector2.zero, Vector2.zero,
                name, fontSize, fontStyle, TextAlignmentOptions.Left);
            label.color = textColor;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(24f, 0f);
            labelRect.offsetMax = new Vector2(-24f, 0f);

            return go;
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
            rect.sizeDelta = new Vector2(960f, 920f);
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
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();

            var inputSystemModuleType = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
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
