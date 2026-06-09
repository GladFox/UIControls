using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UISideMenuDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Navigation(G)/UISideMenuDemo.unity";

        private static readonly string[] Items = { "Home", "Profile", "Settings", "Notifications", "Help", "Log out" };
        private static readonly string[] Icons = { "⌂", "☻", "⚙", "🔔", "?", "⎋" };

        [MenuItem("UIControls/Navigation (G)/Create SideMenu Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateSideMenuDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Side Menu Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Tap Menu to slide the drawer in; the items fly in one by one from the same side.");

            // Trigger + side switch (sit under the drawer/backdrop when open).
            var menuButton = UIDemoSceneFactory.Button(panel, "MenuButton", new Vector2(-360f, 150f), new Vector2(180f, 76f), "☰  Menu",
                new Color(0.24f, 0.55f, 0.95f, 1f), 26);
            var switchButton = UIDemoSceneFactory.Button(panel, "SwitchSide", new Vector2(-360f, 60f), new Vector2(220f, 64f), "Side: Left",
                new Color(0.3f, 0.36f, 0.5f, 1f), 22);
            var switchLabel = switchButton.transform.Find("Label").GetComponent<TMP_Text>();

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -300f), new Vector2(900f, 36f),
                "Selected: —", 22, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            // Backdrop (Graphic-less UIButtonControl root + Dim child, so no white-override on the button).
            var backdropGo = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasGroup), typeof(UIButtonControl));
            var backdropRect = backdropGo.GetComponent<RectTransform>();
            backdropRect.SetParent(panel, false);
            Stretch(backdropRect);
            var backdropGroup = backdropGo.GetComponent<CanvasGroup>();
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            var dimRect = dim.GetComponent<RectTransform>();
            dimRect.SetParent(backdropRect, false);
            Stretch(dimRect);
            var dimImg = dim.GetComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.55f);
            dimImg.raycastTarget = true;

            // Bake the closed state so the backdrop never covers the trigger in the editor / first frame.
            backdropGroup.alpha = 0f;
            backdropGroup.blocksRaycasts = false;
            backdropGo.SetActive(false);

            // Drawer panel (anchors are applied by the control at runtime).
            var drawerGo = new GameObject("SideMenuPanel", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var drawer = drawerGo.GetComponent<RectTransform>();
            drawer.SetParent(panel, false);
            drawer.sizeDelta = new Vector2(420f, 0f);
            drawerGo.GetComponent<Image>().color = new Color(0.1f, 0.13f, 0.2f, 1f);

            UIDemoSceneFactory.Text("Header", drawer, new Vector2(0f, -60f), new Vector2(360f, 50f),
                "Menu", 34, FontStyles.Bold, TextAlignmentOptions.Center).color = UIDemoSceneFactory.Label;
            UIDemoSceneFactory.Image("HeaderRule", drawer, new Vector2(0f, -110f), new Vector2(360f, 3f), new Color(1f, 1f, 1f, 0.12f), false);

            var itemsGo = new GameObject("Items", typeof(RectTransform));
            var items = itemsGo.GetComponent<RectTransform>();
            items.SetParent(drawer, false);
            items.anchorMin = new Vector2(0.5f, 1f);
            items.anchorMax = new Vector2(0.5f, 1f);
            items.pivot = new Vector2(0.5f, 1f);
            items.anchoredPosition = new Vector2(0f, -150f);
            items.sizeDelta = new Vector2(420f, 540f);

            for (var i = 0; i < Items.Length; i++)
            {
                BuildItem(items, i, Items[i], Icons[i]);
            }

            // Control.
            var menuGo = new GameObject("SideMenuControl", typeof(RectTransform), typeof(UISideMenuControl));
            (menuGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var sideMenu = menuGo.GetComponent<UISideMenuControl>();
            UIDemoSceneFactory.SetRef(sideMenu, "panel", drawer);
            UIDemoSceneFactory.SetRef(sideMenu, "backdrop", backdropGroup);
            UIDemoSceneFactory.SetRef(sideMenu, "backdropButton", backdropGo.GetComponent<UIButtonControl>());
            UIDemoSceneFactory.SetRef(sideMenu, "toggleButton", menuButton);
            UIDemoSceneFactory.SetRef(sideMenu, "itemsContainer", items);

            var presenter = panel.gameObject.AddComponent<UISideMenuDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "sideMenu", sideMenu);
            UIDemoSceneFactory.SetRef(presenter, "switchSideButton", switchButton);
            UIDemoSceneFactory.SetRef(presenter, "switchSideLabel", switchLabel);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            UIDemoSceneFactory.SetStringArray(presenter, "itemNames", Items);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }

        private static void BuildItem(RectTransform container, int index, string label, string icon)
        {
            var itemGo = new GameObject("Item_" + label, typeof(RectTransform), typeof(CanvasGroup), typeof(UIButtonControl));
            var item = itemGo.GetComponent<RectTransform>();
            item.SetParent(container, false);
            item.anchorMin = new Vector2(0.5f, 1f);
            item.anchorMax = new Vector2(0.5f, 1f);
            item.pivot = new Vector2(0.5f, 1f);
            item.sizeDelta = new Vector2(360f, 72f);
            item.anchoredPosition = new Vector2(0f, -(index * 82f) - 36f);

            var bg = UIDemoSceneFactory.Image("Bg", item, Vector2.zero, new Vector2(360f, 72f),
                index == Items.Length - 1 ? new Color(0.4f, 0.22f, 0.24f, 1f) : new Color(0.17f, 0.22f, 0.33f, 1f), true);

            UIDemoSceneFactory.Text("Icon", bg.rectTransform, new Vector2(-140f, 0f), new Vector2(50f, 50f),
                icon, 28, FontStyles.Normal, TextAlignmentOptions.Center).color = UIDemoSceneFactory.Label;
            UIDemoSceneFactory.Text("Label", bg.rectTransform, new Vector2(30f, 0f), new Vector2(250f, 60f),
                label, 24, FontStyles.Bold, TextAlignmentOptions.Left).color = UIDemoSceneFactory.Label;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
