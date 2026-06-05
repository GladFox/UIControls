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
    public static class UISearchFieldDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UISearchFieldDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color FieldColor = new Color(0.16f, 0.2f, 0.29f, 1f);
        private static readonly Color TextColor = new Color(0.95f, 0.97f, 1f, 1f);
        private static readonly Color PlaceholderColor = new Color(0.6f, 0.66f, 0.78f, 1f);
        private static readonly Color SuggestionColor = new Color(0.2f, 0.25f, 0.35f, 1f);
        private static readonly Color ClearColor = new Color(0.4f, 0.45f, 0.56f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        private static readonly string[] Cities =
        {
            "Amsterdam", "Athens", "Bangkok", "Barcelona", "Berlin", "Boston", "Cairo", "Chicago",
            "Copenhagen", "Dubai", "Dublin", "Helsinki", "Istanbul", "Lisbon", "London", "Madrid",
            "Melbourne", "Milan", "Moscow", "Munich", "New York", "Oslo", "Paris", "Prague",
            "Rome", "San Francisco", "Seoul", "Singapore", "Stockholm", "Sydney", "Tokyo", "Toronto",
            "Vienna", "Warsaw", "Zurich",
        };

        [MenuItem("UIControls/Create SearchField Demo Scene")]
        public static void CreateSearchFieldDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateSearchFieldDemoSceneBatch()
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
                "UIControls Search Field Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var subtitle = CreateText("Subtitle", panel, new Vector2(0f, 268f), new Vector2(900f, 40f),
                "Type a city — clear button, debounced search, suggestions dropdown.",
                20, FontStyles.Italic, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var search = CreateSearchField(panel, new Vector2(0f, 170f), new Vector2(620f, 76f));

            var status = CreateText("Status", panel, new Vector2(0f, -210f), new Vector2(900f, 40f),
                "Type to search — suggestions appear after a short pause.",
                20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var presenter = panel.gameObject.AddComponent<UISearchFieldDemoPresenter>();
            SetObjectReference(presenter, "search", search);
            SetObjectReference(presenter, "statusLabel", status);
            SetStringArray(presenter, "source", Cities);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"SearchField demo scene created: {ScenePath}");
        }

        private static UISearchFieldControl CreateSearchField(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            var rootGo = new GameObject("SearchField", typeof(RectTransform), typeof(UISearchFieldControl));
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = size;

            // Input field.
            var fieldGo = new GameObject("Field", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            var fieldRect = fieldGo.GetComponent<RectTransform>();
            fieldRect.SetParent(rootRect, false);
            fieldRect.anchorMin = new Vector2(0f, 1f);
            fieldRect.anchorMax = new Vector2(1f, 1f);
            fieldRect.pivot = new Vector2(0.5f, 1f);
            fieldRect.offsetMin = new Vector2(0f, -size.y);
            fieldRect.offsetMax = new Vector2(0f, 0f);
            var fieldImage = fieldGo.GetComponent<Image>();
            fieldImage.color = FieldColor;
            fieldImage.raycastTarget = true;

            var textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            var areaRect = textArea.GetComponent<RectTransform>();
            areaRect.SetParent(fieldRect, false);
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(24f, 6f);
            areaRect.offsetMax = new Vector2(-72f, -6f);

            var placeholder = CreateText("Placeholder", areaRect, Vector2.zero, Vector2.zero,
                "Search cities…", 24, FontStyles.Italic, TextAlignmentOptions.Left);
            placeholder.color = PlaceholderColor;
            FillParent(placeholder.rectTransform);

            var text = CreateText("Text", areaRect, Vector2.zero, Vector2.zero, string.Empty, 24, FontStyles.Normal, TextAlignmentOptions.Left);
            text.color = TextColor;
            FillParent(text.rectTransform);

            var input = fieldGo.GetComponent<TMP_InputField>();
            input.textViewport = areaRect;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.customCaretColor = true;
            input.caretColor = TextColor;
            input.selectionColor = new Color(0.24f, 0.55f, 0.95f, 0.4f);

            // Clear button (right side, inside the field).
            var clear = CreateButton(fieldRect, "ClearButton", new Vector2(-20f, 0f), new Vector2(40f, 40f), "✕", ClearColor);
            var clearRect = clear.transform as RectTransform;
            clearRect.anchorMin = new Vector2(1f, 0.5f);
            clearRect.anchorMax = new Vector2(1f, 0.5f);
            clearRect.pivot = new Vector2(1f, 0.5f);
            clearRect.anchoredPosition = new Vector2(-16f, 0f);
            clear.gameObject.SetActive(false);

            // Suggestions panel.
            var suggGo = new GameObject("Suggestions", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var suggRect = suggGo.GetComponent<RectTransform>();
            suggRect.SetParent(rootRect, false);
            suggRect.anchorMin = new Vector2(0f, 1f);
            suggRect.anchorMax = new Vector2(1f, 1f);
            suggRect.pivot = new Vector2(0.5f, 1f);
            suggRect.anchoredPosition = new Vector2(0f, -(size.y + 6f));
            suggRect.sizeDelta = new Vector2(0f, 0f);
            var suggImage = suggGo.GetComponent<Image>();
            suggImage.color = new Color(0.12f, 0.15f, 0.22f, 1f);
            suggImage.raycastTarget = true;
            var vlg = suggGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 4f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var fitter = suggGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var template = CreateSuggestionItem(suggRect);
            suggGo.SetActive(false);

            var control = rootGo.GetComponent<UISearchFieldControl>();
            var so = new SerializedObject(control);
            so.FindProperty("inputField").objectReferenceValue = input;
            so.FindProperty("clearButton").objectReferenceValue = clear;
            so.FindProperty("suggestionsPanel").objectReferenceValue = suggRect;
            so.FindProperty("suggestionTemplate").objectReferenceValue = template;
            so.FindProperty("debounceSeconds").floatValue = 0.3f;
            so.FindProperty("maxSuggestions").intValue = 6;
            so.FindProperty("caseSensitive").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();

            return control;
        }

        private static UIButtonControl CreateSuggestionItem(RectTransform parent)
        {
            var go = new GameObject("SuggestionTemplate", typeof(RectTransform), typeof(UIButtonControl), typeof(LayoutElement));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 52f;
            le.minHeight = 52f;

            var bgGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.SetParent(rect, false);
            FillParent(bgRect);
            var image = bgGo.GetComponent<Image>();
            image.color = SuggestionColor;
            image.raycastTarget = true;

            var label = CreateText("Label", rect, Vector2.zero, Vector2.zero, "Suggestion", 22, FontStyles.Normal, TextAlignmentOptions.Left);
            label.color = TextColor;
            var lrect = label.rectTransform;
            lrect.anchorMin = Vector2.zero;
            lrect.anchorMax = Vector2.one;
            lrect.offsetMin = new Vector2(20f, 0f);
            lrect.offsetMax = new Vector2(-20f, 0f);

            go.SetActive(false);
            return go.GetComponent<UIButtonControl>();
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
            FillParent(bgRect);
            var image = bgGo.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            var text = CreateText("Label", rect, Vector2.zero, size, label, 22, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = new Color(0.97f, 0.98f, 1f, 1f);

            return go.GetComponent<UIButtonControl>();
        }

        private static void FillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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

        private static void SetStringArray(UnityEngine.Object target, string propertyName, string[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            prop.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).stringValue = values[i];
            }

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
