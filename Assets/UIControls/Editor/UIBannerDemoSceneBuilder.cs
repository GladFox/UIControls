using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIBannerDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Data(I)/UIBannerDemo.unity";

        [MenuItem("UIControls/Data (I)/Create Banner Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateBannerDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Banner / Alert Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Persistent inline alerts (info / success / warning / error). They stay until dismissed.");

            // Banner.
            var bannerGo = new GameObject("Banner", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var bannerRect = bannerGo.GetComponent<RectTransform>();
            bannerRect.SetParent(panel, false);
            bannerRect.anchoredPosition = new Vector2(0f, 110f);
            bannerRect.sizeDelta = new Vector2(760f, 96f);
            var bg = bannerGo.GetComponent<Image>();
            bg.color = new Color(0.34f, 0.6f, 0.95f, 0.18f);
            var group = bannerGo.GetComponent<CanvasGroup>();

            var accent = UIDemoSceneFactory.Image("Accent", bannerRect, new Vector2(-372f, 0f), new Vector2(8f, 96f), new Color(0.34f, 0.6f, 0.95f, 1f), false);
            var icon = UIDemoSceneFactory.Text("Icon", bannerRect, new Vector2(-330f, 0f), new Vector2(60f, 60f), "ℹ", 36, FontStyles.Bold, TextAlignmentOptions.Center);
            icon.color = new Color(0.34f, 0.6f, 0.95f, 1f);
            var message = UIDemoSceneFactory.Text("Message", bannerRect, new Vector2(20f, 0f), new Vector2(560f, 80f),
                "Banner message goes here.", 22, FontStyles.Normal, TextAlignmentOptions.Left);
            var dismiss = UIDemoSceneFactory.Button(bannerRect, "Dismiss", new Vector2(330f, 0f), new Vector2(56f, 56f), "×",
                new Color(1f, 1f, 1f, 0.12f), 30);

            var bannerControl = bannerGo.AddComponent<UIBannerControl>();
            UIDemoSceneFactory.SetRef(bannerControl, "root", bannerRect);
            UIDemoSceneFactory.SetRef(bannerControl, "canvasGroup", group);
            UIDemoSceneFactory.SetRef(bannerControl, "background", bg);
            UIDemoSceneFactory.SetRef(bannerControl, "accentBar", accent);
            UIDemoSceneFactory.SetRef(bannerControl, "iconLabel", icon);
            UIDemoSceneFactory.SetRef(bannerControl, "messageLabel", message);
            UIDemoSceneFactory.SetRef(bannerControl, "dismissButton", dismiss);

            // Trigger buttons.
            var info = UIDemoSceneFactory.Button(panel, "Info", new Vector2(-330f, -60f), new Vector2(160f, 78f), "Info",
                new Color(0.34f, 0.6f, 0.95f, 1f));
            var success = UIDemoSceneFactory.Button(panel, "Success", new Vector2(-110f, -60f), new Vector2(160f, 78f), "Success",
                new Color(0.3f, 0.72f, 0.45f, 1f));
            var warning = UIDemoSceneFactory.Button(panel, "Warning", new Vector2(110f, -60f), new Vector2(160f, 78f), "Warning",
                new Color(0.9f, 0.68f, 0.3f, 1f));
            var error = UIDemoSceneFactory.Button(panel, "Error", new Vector2(330f, -60f), new Vector2(160f, 78f), "Error",
                new Color(0.9f, 0.36f, 0.36f, 1f));

            var presenter = panel.gameObject.AddComponent<UIBannerDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "banner", bannerControl);
            UIDemoSceneFactory.SetRef(presenter, "infoButton", info);
            UIDemoSceneFactory.SetRef(presenter, "successButton", success);
            UIDemoSceneFactory.SetRef(presenter, "warningButton", warning);
            UIDemoSceneFactory.SetRef(presenter, "errorButton", error);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
