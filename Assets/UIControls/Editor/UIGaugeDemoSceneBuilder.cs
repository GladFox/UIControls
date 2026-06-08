using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIGaugeDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Data(I)/UIGaugeDemo.unity";

        [MenuItem("UIControls/Data (I)/Create Gauge Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateGaugeDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Gauge Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "The needle sweeps smoothly to the new value across the arc.");

            var baseImg = UIDemoSceneFactory.Image("GaugeBase", panel, new Vector2(0f, 70f), new Vector2(340f, 340f), new Color(0.12f, 0.16f, 0.24f, 1f), false);
            var inner = UIDemoSceneFactory.Image("Inner", baseImg.rectTransform, new Vector2(0f, 0f), new Vector2(300f, 300f), new Color(0.09f, 0.12f, 0.19f, 1f), false);

            // Needle (pivot at the base so it swings around the centre).
            var needleGo = new GameObject("Needle", typeof(RectTransform), typeof(Image));
            var needle = needleGo.GetComponent<RectTransform>();
            needle.SetParent(baseImg.rectTransform, false);
            needle.pivot = new Vector2(0.5f, 0f);
            needle.anchorMin = new Vector2(0.5f, 0.5f);
            needle.anchorMax = new Vector2(0.5f, 0.5f);
            needle.anchoredPosition = Vector2.zero;
            needle.sizeDelta = new Vector2(12f, 132f);
            needleGo.GetComponent<Image>().color = new Color(0.95f, 0.42f, 0.36f, 1f);
            needleGo.GetComponent<Image>().raycastTarget = false;

            UIDemoSceneFactory.Image("Hub", baseImg.rectTransform, Vector2.zero, new Vector2(40f, 40f), new Color(0.7f, 0.74f, 0.85f, 1f), false);

            var value = UIDemoSceneFactory.Text("Value", baseImg.rectTransform, new Vector2(0f, -90f), new Vector2(260f, 60f),
                "0", 44, FontStyles.Bold, TextAlignmentOptions.Center);
            value.color = UIDemoSceneFactory.Label;

            var gaugeGo = new GameObject("GaugeControl", typeof(RectTransform), typeof(UIGaugeControl));
            (gaugeGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var gauge = gaugeGo.GetComponent<UIGaugeControl>();
            UIDemoSceneFactory.SetRef(gauge, "needle", needle);
            UIDemoSceneFactory.SetRef(gauge, "valueLabel", value);

            var low = UIDemoSceneFactory.Button(panel, "Low", new Vector2(-330f, -180f), new Vector2(150f, 74f), "15");
            var mid = UIDemoSceneFactory.Button(panel, "Mid", new Vector2(-110f, -180f), new Vector2(150f, 74f), "60");
            var high = UIDemoSceneFactory.Button(panel, "High", new Vector2(110f, -180f), new Vector2(150f, 74f), "95");
            var random = UIDemoSceneFactory.Button(panel, "Random", new Vector2(330f, -180f), new Vector2(150f, 74f), "Random",
                new Color(0.45f, 0.7f, 0.4f, 1f));

            var presenter = panel.gameObject.AddComponent<UIGaugeDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "gauge", gauge);
            UIDemoSceneFactory.SetRef(presenter, "lowButton", low);
            UIDemoSceneFactory.SetRef(presenter, "midButton", mid);
            UIDemoSceneFactory.SetRef(presenter, "highButton", high);
            UIDemoSceneFactory.SetRef(presenter, "randomButton", random);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
