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
    public static class UISkeletonLoaderDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Feedback(E)/UISkeletonLoaderDemo.unity";

        private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.2f, 0.92f);
        private static readonly Color CardColor = new Color(0.13f, 0.16f, 0.24f, 1f);
        private static readonly Color BoneColor = new Color(0.22f, 0.26f, 0.36f, 1f);
        private static readonly Color AvatarColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color TextColor = new Color(0.92f, 0.95f, 1f, 1f);
        private static readonly Color SubTextColor = new Color(0.72f, 0.78f, 0.92f, 1f);
        private static readonly Color ButtonColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        private static readonly Color StatusColor = new Color(0.82f, 0.88f, 1f, 1f);

        [MenuItem("UIControls/Create SkeletonLoader Demo Scene")]
        public static void CreateSkeletonLoaderDemoSceneFromMenu()
        {
            CreateDemoScene();
        }

        public static void CreateSkeletonLoaderDemoSceneBatch()
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
                "UIControls Skeleton Loader Demo", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.95f, 1f, 1f);

            // Card.
            var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(UISkeletonLoaderControl));
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.SetParent(panel, false);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(0f, 80f);
            cardRect.sizeDelta = new Vector2(640f, 280f);
            var cardImage = card.GetComponent<Image>();
            cardImage.color = CardColor;
            cardImage.raycastTarget = false;

            // Skeleton root (bones).
            var skeletonRoot = CreateChild(cardRect, "Skeleton");
            FullStretch(skeletonRoot);
            var avatarBone = CreateBone(skeletonRoot, new Vector2(-220f, 60f), new Vector2(120f, 120f));
            CreateBone(skeletonRoot, new Vector2(40f, 90f), new Vector2(320f, 32f));
            CreateBone(skeletonRoot, new Vector2(-20f, 40f), new Vector2(200f, 24f));
            CreateBone(skeletonRoot, new Vector2(0f, -40f), new Vector2(560f, 22f));
            CreateBone(skeletonRoot, new Vector2(-30f, -80f), new Vector2(500f, 22f));

            // Shimmer overlay.
            var shimmerGo = new GameObject("Shimmer", typeof(RectTransform), typeof(RawImage));
            var shimmerRect = shimmerGo.GetComponent<RectTransform>();
            shimmerRect.SetParent(skeletonRoot, false);
            shimmerRect.anchorMin = new Vector2(0.5f, 0.5f);
            shimmerRect.anchorMax = new Vector2(0.5f, 0.5f);
            shimmerRect.pivot = new Vector2(0.5f, 0.5f);
            shimmerRect.sizeDelta = new Vector2(160f, 280f);
            shimmerRect.anchoredPosition = Vector2.zero;
            var shimmer = shimmerGo.GetComponent<RawImage>();
            shimmer.raycastTarget = false;

            // Content root (real content).
            var contentRoot = CreateChild(cardRect, "Content");
            FullStretch(contentRoot);
            var avatar = new GameObject("Avatar", typeof(RectTransform), typeof(Image));
            var avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.SetParent(contentRoot, false);
            avatarRect.anchorMin = new Vector2(0.5f, 0.5f); avatarRect.anchorMax = new Vector2(0.5f, 0.5f); avatarRect.pivot = new Vector2(0.5f, 0.5f);
            avatarRect.sizeDelta = new Vector2(120f, 120f);
            avatarRect.anchoredPosition = new Vector2(-220f, 60f);
            avatar.GetComponent<Image>().color = AvatarColor;

            var name = CreateText("Name", contentRoot, new Vector2(60f, 92f), new Vector2(360f, 40f),
                "Ada Lovelace", 30, FontStyles.Bold, TextAlignmentOptions.Left);
            name.color = TextColor;
            var role = CreateText("Role", contentRoot, new Vector2(20f, 48f), new Vector2(280f, 30f),
                "Software Engineer", 22, FontStyles.Italic, TextAlignmentOptions.Left);
            role.color = SubTextColor;
            var bio = CreateText("Bio", contentRoot, new Vector2(0f, -50f), new Vector2(580f, 80f),
                "Pioneered the idea of a general-purpose computing machine and wrote the first algorithm intended for one.",
                20, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            bio.color = SubTextColor;

            var control = card.GetComponent<UISkeletonLoaderControl>();
            var so = new SerializedObject(control);
            so.FindProperty("skeletonRoot").objectReferenceValue = skeletonRoot.gameObject;
            so.FindProperty("contentRoot").objectReferenceValue = contentRoot.gameObject;
            so.FindProperty("shimmer").objectReferenceValue = shimmer;
            so.FindProperty("travelArea").objectReferenceValue = cardRect;
            so.FindProperty("speed").floatValue = 0.8f;
            so.FindProperty("isLoading").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            var reload = CreateButton(panel, "ReloadButton", new Vector2(0f, -150f), new Vector2(220f, 72f), "Reload", ButtonColor);

            var status = CreateText("Status", panel, new Vector2(0f, -240f), new Vector2(900f, 36f),
                "Loading…", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = StatusColor;

            var presenter = panel.gameObject.AddComponent<UISkeletonLoaderDemoPresenter>();
            SetObjectReference(presenter, "skeleton", control);
            SetObjectReference(presenter, "reloadButton", reload);
            SetObjectReference(presenter, "statusLabel", status);

            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"SkeletonLoader demo scene created: {ScenePath}");
        }

        private static RectTransform CreateChild(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static RectTransform CreateBone(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject("Bone", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            var image = go.GetComponent<Image>();
            image.color = BoneColor;
            image.raycastTarget = false;
            return rect;
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
            FullStretch(bgRect);
            bgGo.GetComponent<Image>().color = color;

            var text = CreateText("Label", rect, Vector2.zero, size, label, 22, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = new Color(0.97f, 0.98f, 1f, 1f);

            return go.GetComponent<UIButtonControl>();
        }

        private static void FullStretch(RectTransform rect)
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
