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

        private static readonly Color BgColor         = new Color(0.06f, 0.09f, 0.16f, 1f);
        private static readonly Color PanelColor      = new Color(0.09f, 0.13f, 0.20f, 0.95f);
        private static readonly Color ViewportColor   = new Color(0.11f, 0.15f, 0.23f, 1f);
        private static readonly Color HeaderBg        = new Color(0.13f, 0.22f, 0.38f, 1f);
        private static readonly Color HeaderText      = new Color(0.85f, 0.92f, 1f,  1f);
        private static readonly Color RowBg           = new Color(0.16f, 0.20f, 0.29f, 1f);
        private static readonly Color RowText         = new Color(0.78f, 0.85f, 0.98f, 1f);
        private static readonly Color ZoneColor       = new Color(0.13f, 0.22f, 0.38f, 0.97f);
        private static readonly Color ZoneBorderColor = new Color(0.30f, 0.55f, 0.90f, 0.60f);

        [MenuItem("UIControls/Create StickyList Demo Scene")]
        public static void CreateFromMenu() => CreateDemoScene();
        public static void CreateStickyListDemoSceneBatch() => CreateDemoScene();

        private static void CreateDemoScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            var panel  = CreatePanel(canvas.transform as RectTransform);

            // Title
            var title = CreateText("Title", panel, new Vector2(0f, 390f), new Vector2(900f, 56f),
                "UIControls — Sticky List Demo", 34, FontStyles.Bold, TextAlignmentOptions.Center);
            title.color = new Color(0.92f, 0.96f, 1f, 1f);

            // Hint
            var hint = CreateText("Hint", panel, new Vector2(0f, 350f), new Vector2(820f, 36f),
                "Section headers stick to the top as you scroll", 19, FontStyles.Italic, TextAlignmentOptions.Center);
            hint.color = new Color(0.65f, 0.78f, 1f, 1f);

            // ── ScrollRect ─────────────────────────────────────────────────────────
            const float scrollW = 680f;
            const float scrollH = 640f;

            var scrollGo = new GameObject("ScrollView",
                typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(UIStickyListControl));
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.SetParent(panel, false);
            scrollRect.anchorMin = scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.pivot     = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0f, -30f);
            scrollRect.sizeDelta = new Vector2(scrollW, scrollH);
            scrollGo.GetComponent<Image>().color = ViewportColor;

            // Viewport
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport   = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(scrollRect, false);
            Stretch(viewport);
            viewportGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);

            // Content
            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var content   = contentGo.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot     = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding             = new RectOffset(0, 0, 0, 12);
            vlg.spacing             = 2f;
            vlg.childAlignment      = TextAnchor.UpperCenter;
            vlg.childControlWidth   = true;
            vlg.childControlHeight  = true;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Templates ─────────────────────────────────────────────────────────
            var headerTemplate = CreateSectionHeader(content);
            var rowTemplate    = CreateRow(content);
            headerTemplate.SetActive(false);
            rowTemplate.SetActive(false);

            // ── Sticky Top Zone ────────────────────────────────────────────────────
            // Lives inside viewport (so it masks with the scroll) and overlays content
            var topZone = CreateZone("StickyTopZone", viewport, isTop: true);

            // ── Wire ScrollRect ───────────────────────────────────────────────────
            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.content          = content;
            sr.viewport         = viewport;
            sr.horizontal       = false;
            sr.vertical         = true;
            sr.movementType     = ScrollRect.MovementType.Elastic;
            sr.elasticity       = 0.1f;
            sr.inertia          = true;
            sr.decelerationRate = 0.135f;
            sr.scrollSensitivity = 28f;

            // ── Wire UIStickyListControl ──────────────────────────────────────────
            var stickyCtrl = scrollGo.GetComponent<UIStickyListControl>();
            var so = new SerializedObject(stickyCtrl);
            so.FindProperty("scrollRect").objectReferenceValue    = sr;
            so.FindProperty("stickyTopZone").objectReferenceValue = topZone;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ── Demo Presenter ────────────────────────────────────────────────────
            var presenter = scrollGo.AddComponent<UIStickyListDemoPresenter>();
            SetObjectReference(presenter, "content", content);
            SetObjectReference(presenter, "sectionHeaderTemplate", headerTemplate);
            SetObjectReference(presenter, "rowTemplate", rowTemplate);

            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
                ScenePath, true);

            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"StickyList demo scene created: {ScenePath}");
        }

        // ── Element builders ─────────────────────────────────────────────────────

        private static GameObject CreateSectionHeader(RectTransform parent)
        {
            var go   = new GameObject("SectionHeader", typeof(RectTransform), typeof(Image),
                typeof(LayoutElement), typeof(UIStickyItemControl));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.color        = HeaderBg;
            img.raycastTarget = true;

            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 48f;
            le.minHeight       = 48f;

            var label = CreateText("Label", rect, Vector2.zero, Vector2.zero,
                "Section", 20, FontStyles.Bold, TextAlignmentOptions.Left);
            label.color        = HeaderText;
            label.raycastTarget = false;
            var lr = label.rectTransform;
            lr.anchorMin  = Vector2.zero;
            lr.anchorMax  = Vector2.one;
            lr.offsetMin  = new Vector2(20f, 0f);
            lr.offsetMax  = new Vector2(-20f, 0f);

            // Edge = Top is the default; no need to set it explicitly
            return go;
        }

        private static GameObject CreateRow(RectTransform parent)
        {
            var go   = new GameObject("Row", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.color         = RowBg;
            img.raycastTarget = true;

            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 52f;
            le.minHeight       = 52f;

            var label = CreateText("Label", rect, Vector2.zero, Vector2.zero,
                "Item", 18, FontStyles.Normal, TextAlignmentOptions.Left);
            label.color        = RowText;
            label.raycastTarget = false;
            var lr = label.rectTransform;
            lr.anchorMin  = Vector2.zero;
            lr.anchorMax  = Vector2.one;
            lr.offsetMin  = new Vector2(36f, 0f);
            lr.offsetMax  = new Vector2(-20f, 0f);

            return go;
        }

        private static RectTransform CreateZone(string name, RectTransform parent, bool isTop)
        {
            var go   = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            if (isTop)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot     = new Vector2(0.5f, 1f);
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot     = new Vector2(0.5f, 0f);
            }

            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(0f, 0f); // height driven by stuck items

            var img = go.GetComponent<Image>();
            img.color        = new Color(0f, 0f, 0f, 0f); // transparent container
            img.raycastTarget = false;

            // Add a bottom border line for the top zone
            if (isTop)
            {
                var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
                var brect  = border.GetComponent<RectTransform>();
                brect.SetParent(rect, false);
                brect.anchorMin = new Vector2(0f, 0f);
                brect.anchorMax = new Vector2(1f, 0f);
                brect.pivot     = new Vector2(0.5f, 1f);
                brect.sizeDelta = new Vector2(0f, 2f);
                brect.anchoredPosition = Vector2.zero;
                border.GetComponent<Image>().color = ZoneBorderColor;
            }

            return rect;
        }

        // ── Shared helpers ────────────────────────────────────────────────────────

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static RectTransform CreatePanel(RectTransform parent)
        {
            var go   = new GameObject("DemoPanel", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(960f, 900f);
            go.GetComponent<Image>().color = PanelColor;
            return rect;
        }

        private static void CreateCamera()
        {
            var go  = new GameObject("Main Camera");
            var cam = go.AddComponent<Camera>();
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = BgColor;
            cam.orthographic     = true;
            cam.orthographicSize = 5f;
            go.transform.position = new Vector3(0f, 0f, -10f);
            go.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            var inputSystemType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemType != null)
                go.AddComponent(inputSystemType);
            else
                go.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var go     = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static TextMeshProUGUI CreateText(
            string name, RectTransform parent,
            Vector2 anchoredPosition, Vector2 size,
            string content, int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
        {
            var go   = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text              = content;
            tmp.fontSize          = fontSize;
            tmp.fontStyle         = fontStyle;
            tmp.alignment         = alignment;
            tmp.textWrappingMode  = TextWrappingModes.Normal;
            tmp.overflowMode      = TextOverflowModes.Truncate;
            tmp.color             = Color.white;
            tmp.raycastTarget     = false;
            return tmp;
        }

        private static void SetObjectReference(UnityEngine.Object target, string prop, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(prop).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
                if (s.path == path) return;

            var updated = new EditorBuildSettingsScene[scenes.Length + 1];
            for (var i = 0; i < scenes.Length; i++) updated[i] = scenes[i];
            updated[updated.Length - 1] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
