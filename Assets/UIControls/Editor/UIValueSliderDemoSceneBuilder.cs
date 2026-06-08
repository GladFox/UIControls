using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIValueSliderDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Forms(H)/UIValueSliderDemo.unity";

        [MenuItem("UIControls/Forms (H)/Create ValueSlider Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateValueSliderDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Value Slider Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Drag the track; a value bubble follows the handle and appears while dragging.");

            const float width = 600f;

            var rootGo = new GameObject("ValueSlider", typeof(RectTransform), typeof(Image), typeof(UIValueSliderControl));
            var root = rootGo.GetComponent<RectTransform>();
            root.SetParent(panel, false);
            root.anchoredPosition = new Vector2(0f, 50f);
            root.sizeDelta = new Vector2(width, 80f);
            var rootImg = rootGo.GetComponent<Image>();
            rootImg.color = new Color(1f, 1f, 1f, 0f);
            rootImg.raycastTarget = true;

            // Visual track bar.
            UIDemoSceneFactory.Image("Bar", root, Vector2.zero, new Vector2(width, 12f), new Color(0.13f, 0.18f, 0.27f, 1f), false);

            // Fill (left-anchored).
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fill = fillGo.GetComponent<RectTransform>();
            fill.SetParent(root, false);
            fill.anchorMin = new Vector2(0.5f, 0.5f);
            fill.anchorMax = new Vector2(0.5f, 0.5f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.sizeDelta = new Vector2(0f, 12f);
            fillGo.GetComponent<Image>().color = new Color(0.24f, 0.55f, 0.95f, 1f);
            fillGo.GetComponent<Image>().raycastTarget = false;

            var handle = UIDemoSceneFactory.Image("Handle", root, Vector2.zero, new Vector2(40f, 40f), new Color(0.97f, 0.98f, 1f, 1f), false);

            // Bubble.
            var bubbleGo = new GameObject("Bubble", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var bubble = bubbleGo.GetComponent<RectTransform>();
            bubble.SetParent(root, false);
            bubble.pivot = new Vector2(0.5f, 0f);
            bubble.anchoredPosition = new Vector2(0f, 36f);
            bubble.sizeDelta = new Vector2(90f, 52f);
            bubbleGo.GetComponent<Image>().color = new Color(0.24f, 0.55f, 0.95f, 1f);
            bubbleGo.GetComponent<Image>().raycastTarget = false;
            var bubbleGroup = bubbleGo.GetComponent<CanvasGroup>();
            bubbleGroup.alpha = 0f;
            var bubbleLabel = UIDemoSceneFactory.Text("Value", bubble, Vector2.zero, new Vector2(90f, 52f), "50", 26, FontStyles.Bold, TextAlignmentOptions.Center);

            var slider = rootGo.GetComponent<UIValueSliderControl>();
            UIDemoSceneFactory.SetRef(slider, "track", root);
            UIDemoSceneFactory.SetRef(slider, "fill", fill);
            UIDemoSceneFactory.SetRef(slider, "handle", handle.rectTransform);
            UIDemoSceneFactory.SetRef(slider, "bubbleGroup", bubbleGroup);
            UIDemoSceneFactory.SetRef(slider, "bubble", bubble);
            UIDemoSceneFactory.SetRef(slider, "bubbleLabel", bubbleLabel);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -60f), new Vector2(900f, 36f),
                "Value: 50%", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIValueSliderDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "slider", slider);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
