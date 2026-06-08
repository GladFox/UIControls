using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIContextMenuDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Data(I)/UIContextMenuDemo.unity";
        private static readonly string[] Items = { "Cut", "Copy", "Paste", "Rename", "Delete" };

        [MenuItem("UIControls/Data (I)/Create ContextMenu Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateContextMenuDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Context Menu Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Click anywhere in the area to open the menu at the cursor; it flips near the edges.");

            // Click surface (also the flip bounds).
            var areaGo = new GameObject("ContextMenu", typeof(RectTransform), typeof(Image), typeof(UIContextMenuControl));
            var area = areaGo.GetComponent<RectTransform>();
            area.SetParent(panel, false);
            area.anchoredPosition = new Vector2(0f, 20f);
            area.sizeDelta = new Vector2(820f, 380f);
            var areaImg = areaGo.GetComponent<Image>();
            areaImg.color = new Color(0.12f, 0.16f, 0.24f, 1f);
            areaImg.raycastTarget = true;

            UIDemoSceneFactory.Text("Hint", area, Vector2.zero, new Vector2(700f, 60f),
                "right-click style area", 24, FontStyles.Italic, TextAlignmentOptions.Center).color = UIDemoSceneFactory.Muted;

            // Menu panel (child of the area so positions are in area-local space).
            const float rowH = 50f;
            var menuGo = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var menu = menuGo.GetComponent<RectTransform>();
            menu.SetParent(area, false);
            menu.pivot = new Vector2(0f, 1f);
            menu.sizeDelta = new Vector2(220f, Items.Length * rowH + 12f);
            menuGo.GetComponent<Image>().color = new Color(0.1f, 0.13f, 0.2f, 1f);
            var menuGroup = menuGo.GetComponent<CanvasGroup>();

            var buttons = new UIButtonControl[Items.Length];
            for (var i = 0; i < Items.Length; i++)
            {
                var rowGo = new GameObject("Item" + i, typeof(RectTransform), typeof(UIButtonControl));
                var row = rowGo.GetComponent<RectTransform>();
                row.SetParent(menu, false);
                row.anchorMin = new Vector2(0.5f, 1f);
                row.anchorMax = new Vector2(0.5f, 1f);
                row.pivot = new Vector2(0.5f, 1f);
                row.sizeDelta = new Vector2(208f, rowH);
                row.anchoredPosition = new Vector2(0f, -6f - i * rowH);

                UIDemoSceneFactory.Image("Bg", row, Vector2.zero, new Vector2(208f, rowH), new Color(1f, 1f, 1f, 0f), true);
                var label = UIDemoSceneFactory.Text("Label", row, new Vector2(14f, 0f), new Vector2(180f, rowH), Items[i], 22, FontStyles.Normal, TextAlignmentOptions.Left);
                label.color = i == Items.Length - 1 ? new Color(0.9f, 0.45f, 0.45f, 1f) : UIDemoSceneFactory.Label;

                buttons[i] = rowGo.GetComponent<UIButtonControl>();
            }

            var menuControl = areaGo.GetComponent<UIContextMenuControl>();
            UIDemoSceneFactory.SetRef(menuControl, "menuPanel", menu);
            UIDemoSceneFactory.SetRef(menuControl, "menuGroup", menuGroup);
            UIDemoSceneFactory.SetRef(menuControl, "bounds", area);
            UIDemoSceneFactory.SetRefArray(menuControl, "itemButtons", buttons);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -260f), new Vector2(900f, 36f),
                "Chose: —", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIContextMenuDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "contextMenu", menuControl);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            UIDemoSceneFactory.SetStringArray(presenter, "itemNames", Items);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
