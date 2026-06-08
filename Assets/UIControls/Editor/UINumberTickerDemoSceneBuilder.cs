using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;

namespace UIControls.Editor
{
    public static class UINumberTickerDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Gameplay(F)/UINumberTickerDemo.unity";

        [MenuItem("UIControls/Gameplay (F)/Create NumberTicker Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateNumberTickerDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Number Ticker Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 248f),
                "The score rolls smoothly to its new value and pops on change.");

            var tickerGo = new GameObject("Ticker", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(UINumberTickerControl));
            var tickerRect = tickerGo.GetComponent<RectTransform>();
            tickerRect.SetParent(panel, false);
            tickerRect.anchoredPosition = new Vector2(0f, 90f);
            tickerRect.sizeDelta = new Vector2(700f, 130f);
            var label = tickerGo.GetComponent<TextMeshProUGUI>();
            label.text = "0";
            label.fontSize = 96;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.97f, 0.86f, 0.4f, 1f);
            label.raycastTarget = false;

            var ticker = tickerGo.GetComponent<UINumberTickerControl>();
            UIDemoSceneFactory.SetRef(ticker, "label", label);
            var tso = new SerializedObject(ticker);
            tso.FindProperty("prefix").stringValue = "★ ";
            tso.FindProperty("decimals").intValue = 0;
            tso.FindProperty("thousandsSeparator").boolValue = true;
            tso.ApplyModifiedPropertiesWithoutUndo();

            var add100 = UIDemoSceneFactory.Button(panel, "Add100", new Vector2(-330f, -120f), new Vector2(180f, 76f), "+100");
            var add2500 = UIDemoSceneFactory.Button(panel, "Add2500", new Vector2(-110f, -120f), new Vector2(180f, 76f), "+2 500");
            var random = UIDemoSceneFactory.Button(panel, "Random", new Vector2(110f, -120f), new Vector2(180f, 76f), "Random",
                new Color(0.45f, 0.7f, 0.4f, 1f));
            var reset = UIDemoSceneFactory.Button(panel, "Reset", new Vector2(330f, -120f), new Vector2(180f, 76f), "Reset",
                new Color(0.5f, 0.34f, 0.6f, 1f));

            var presenter = panel.gameObject.AddComponent<UINumberTickerDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "ticker", ticker);
            UIDemoSceneFactory.SetRef(presenter, "add100Button", add100);
            UIDemoSceneFactory.SetRef(presenter, "add2500Button", add2500);
            UIDemoSceneFactory.SetRef(presenter, "randomButton", random);
            UIDemoSceneFactory.SetRef(presenter, "resetButton", reset);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
