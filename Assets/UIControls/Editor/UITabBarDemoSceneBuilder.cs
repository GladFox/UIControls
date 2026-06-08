using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UITabBarDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Navigation(G)/UITabBarDemo.unity";

        private static readonly string[] Names = { "Home", "Search", "Library", "Profile" };
        private static readonly string[] Icons = { "⌂", "⌕", "≣", "☻" };
        private static readonly Color[] PageColors =
        {
            new Color(0.18f, 0.26f, 0.4f, 1f),
            new Color(0.2f, 0.32f, 0.3f, 1f),
            new Color(0.32f, 0.24f, 0.4f, 1f),
            new Color(0.36f, 0.28f, 0.22f, 1f),
        };

        [MenuItem("UIControls/Navigation (G)/Create TabBar Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateTabBarDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Tab Bar Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "App-style bottom navigation: tap a tab to switch pages; the pill slides under it.");

            // Pages area.
            var pages = new GameObject[Names.Length];
            for (var i = 0; i < Names.Length; i++)
            {
                var page = UIDemoSceneFactory.Image("Page_" + Names[i], panel, new Vector2(0f, 40f), new Vector2(820f, 360f), PageColors[i], false);
                UIDemoSceneFactory.Text("Title", page.rectTransform, Vector2.zero, new Vector2(800f, 80f),
                    Names[i] + " page", 40, FontStyles.Bold, TextAlignmentOptions.Center).color = UIDemoSceneFactory.Label;
                pages[i] = page.gameObject;
            }

            // Bottom bar.
            var bar = UIDemoSceneFactory.Image("TabBar", panel, new Vector2(0f, -250f), new Vector2(840f, 120f), new Color(0.07f, 0.1f, 0.16f, 1f), false);

            var indicator = UIDemoSceneFactory.Image("Indicator", bar.rectTransform, new Vector2(-315f, 48f), new Vector2(120f, 6f),
                new Color(0.24f, 0.55f, 0.95f, 1f), false);

            var tabRects = new RectTransform[Names.Length];
            var tabButtons = new UIButtonControl[Names.Length];
            var tabGraphics = new Graphic[Names.Length];
            float[] xs = { -315f, -105f, 105f, 315f };

            for (var i = 0; i < Names.Length; i++)
            {
                var tabGo = new GameObject("Tab_" + Names[i], typeof(RectTransform), typeof(UIButtonControl));
                var tab = tabGo.GetComponent<RectTransform>();
                tab.SetParent(bar.rectTransform, false);
                tab.anchoredPosition = new Vector2(xs[i], 0f);
                tab.sizeDelta = new Vector2(180f, 110f);

                // Transparent hit area (child graphic, so no white-override on the root).
                var hit = UIDemoSceneFactory.Image("Hit", tab, Vector2.zero, new Vector2(180f, 110f), new Color(1f, 1f, 1f, 0f), true);

                UIDemoSceneFactory.Text("Icon", tab, new Vector2(0f, 18f), new Vector2(80f, 50f),
                    Icons[i], 38, FontStyles.Normal, TextAlignmentOptions.Center).color = new Color(0.6f, 0.66f, 0.78f, 1f);
                var label = UIDemoSceneFactory.Text("Label", tab, new Vector2(0f, -30f), new Vector2(170f, 30f),
                    Names[i], 18, FontStyles.Bold, TextAlignmentOptions.Center);
                label.color = new Color(0.6f, 0.66f, 0.78f, 1f);

                tabRects[i] = tab;
                tabButtons[i] = tabGo.GetComponent<UIButtonControl>();
                tabGraphics[i] = label;
            }

            var tabBarGo = new GameObject("TabBarControl", typeof(RectTransform), typeof(UITabBarControl));
            (tabBarGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var tabBar = tabBarGo.GetComponent<UITabBarControl>();
            UIDemoSceneFactory.SetRefArray(tabBar, "tabs", tabRects);
            UIDemoSceneFactory.SetRefArray(tabBar, "tabButtons", tabButtons);
            UIDemoSceneFactory.SetRefArray(tabBar, "tabGraphics", tabGraphics);
            UIDemoSceneFactory.SetRefArray(tabBar, "pages", pages);
            UIDemoSceneFactory.SetRef(tabBar, "indicator", indicator.rectTransform);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -350f), new Vector2(900f, 36f),
                "Active tab: Home", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UITabBarDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "tabBar", tabBar);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            UIDemoSceneFactory.SetStringArray(presenter, "tabNames", Names);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
