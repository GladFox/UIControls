using System;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIControlsDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/UIControlsDemo.unity";
        private const string TapProfilePath = "Assets/UIControls/Animations/UI/Button/Profiles/TapProfile.asset";

        [MenuItem("UIControls/Create Demo Scene")]
        public static void CreateDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateDemoSceneBatch()
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

            var title = CreateText("Title", panel, new Vector2(0f, 200f), new Vector2(460f, 60f),
                "UIControls Demo", 36, FontStyle.Bold, TextAnchor.MiddleCenter);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            var button = CreateButton(panel, new Vector2(0f, 105f));
            var toggle = CreateToggle(panel, new Vector2(0f, 10f));
            var progressBar = CreateProgressBar(panel, new Vector2(0f, -95f));

            var hint = CreateText("Hint", panel, new Vector2(0f, -180f), new Vector2(460f, 50f),
                "Toggle sets a base value. Button adds +10% each click.", 20, FontStyle.Italic, TextAnchor.MiddleCenter);
            hint.color = new Color(0.75f, 0.8f, 0.95f, 1f);

            var presenter = panel.gameObject.AddComponent<UIControlsDemoPresenter>();
            SetObjectReference(presenter, "buttonControl", button.GetComponent<UIButtonControl>());
            SetObjectReference(presenter, "toggleControl", toggle.GetComponent<UIToggleControl>());
            SetObjectReference(presenter, "progressBarControl", progressBar.GetComponent<UIProgressBarControl>());

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Demo scene created: {ScenePath}");
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

        private static RectTransform CreatePanel(RectTransform parent)
        {
            var panelGo = new GameObject("DemoPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var rect = panelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(540f, 470f);

            var image = panelGo.GetComponent<Image>();
            image.color = new Color(0.09f, 0.13f, 0.2f, 0.92f);
            image.raycastTarget = false;

            return rect;
        }

        private static GameObject CreateButton(RectTransform parent, Vector2 anchoredPosition)
        {
            var buttonGo = new GameObject("DemoButton", typeof(RectTransform), typeof(Image), typeof(UIButtonControl));
            var rect = buttonGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(420f, 74f);

            var image = buttonGo.GetComponent<Image>();
            image.color = new Color(0.19f, 0.41f, 0.82f, 1f);

            var text = CreateText("Label", rect, Vector2.zero, rect.sizeDelta,
                "Increase Progress", 28, FontStyle.Bold, TextAnchor.MiddleCenter);
            text.raycastTarget = false;
            text.color = Color.white;

            var buttonControl = buttonGo.GetComponent<UIButtonControl>();
            var tapProfile = AssetDatabase.LoadAssetAtPath<UIButtonAnimationProfile>(TapProfilePath);
            if (tapProfile != null)
            {
                SetObjectReference(buttonControl, "animationProfile", tapProfile);
            }

            return buttonGo;
        }

        private static GameObject CreateToggle(RectTransform parent, Vector2 anchoredPosition)
        {
            var holder = new GameObject("DemoToggleHolder", typeof(RectTransform));
            var holderRect = holder.GetComponent<RectTransform>();
            holderRect.SetParent(parent, false);
            holderRect.anchorMin = new Vector2(0.5f, 0.5f);
            holderRect.anchorMax = new Vector2(0.5f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.anchoredPosition = anchoredPosition;
            holderRect.sizeDelta = new Vector2(420f, 82f);

            var label = CreateText("ToggleLabel", holderRect, new Vector2(-95f, 0f), new Vector2(220f, 60f),
                "Base value", 24, FontStyle.Normal, TextAnchor.MiddleLeft);
            label.color = new Color(0.9f, 0.94f, 1f, 1f);

            var toggleGo = new GameObject("DemoToggle", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(UIToggleControl));
            var toggleRect = toggleGo.GetComponent<RectTransform>();
            toggleRect.SetParent(holderRect, false);
            toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
            toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
            toggleRect.pivot = new Vector2(0.5f, 0.5f);
            toggleRect.anchoredPosition = new Vector2(120f, 0f);
            toggleRect.sizeDelta = new Vector2(170f, 64f);

            var toggleBackground = toggleGo.GetComponent<Image>();
            toggleBackground.color = new Color(0.27f, 0.27f, 0.3f, 1f);

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.SetParent(toggleRect, false);
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = new Vector2(-24f, 0f);
            handleRect.sizeDelta = new Vector2(50f, 50f);

            var handleImage = handleGo.GetComponent<Image>();
            handleImage.color = Color.white;

            var toggleControl = toggleGo.GetComponent<UIToggleControl>();
            SetObjectReference(toggleControl, "handle", handleRect);
            SetObjectReference(toggleControl, "backgroundGraphic", toggleBackground);
            SetObjectReference(toggleControl, "handleGraphic", handleImage);
            SetObjectReference(toggleControl, "canvasGroup", toggleGo.GetComponent<CanvasGroup>());
            SetVector2(toggleControl, "offHandlePosition", new Vector2(-42f, 0f));
            SetVector2(toggleControl, "onHandlePosition", new Vector2(42f, 0f));
            SetBool(toggleControl, "isOn", true);

            return toggleGo;
        }

        private static GameObject CreateProgressBar(RectTransform parent, Vector2 anchoredPosition)
        {
            var progressGo = new GameObject("DemoProgressBar", typeof(RectTransform), typeof(Image), typeof(UIProgressBarControl));
            var rect = progressGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(420f, 56f);

            var background = progressGo.GetComponent<Image>();
            background.color = new Color(0.14f, 0.16f, 0.22f, 1f);

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.SetParent(rect, false);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.offsetMin = new Vector2(6f, 6f);
            fillRect.offsetMax = new Vector2(-6f, -6f);

            var fillImage = fillGo.GetComponent<Image>();
            fillImage.color = new Color(0.11f, 0.74f, 0.39f, 1f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 0.85f;

            var valueText = CreateText("ValueText", rect, Vector2.zero, rect.sizeDelta,
                "85%", 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            valueText.color = Color.white;
            valueText.raycastTarget = false;

            var progressControl = progressGo.GetComponent<UIProgressBarControl>();
            SetObjectReference(progressControl, "fillImage", fillImage);
            SetObjectReference(progressControl, "valueLabel", valueText);
            SetFloat(progressControl, "value", 0.85f);

            return progressGo;
        }

        private static Text CreateText(
            string name,
            RectTransform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            string content,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment)
        {
            var textGo = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = textGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = textGo.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;

            return text;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetVector2(UnityEngine.Object target, string propertyName, Vector2 value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).vector2Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
