using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIRadialMenuDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Gameplay(F)/UIRadialMenuDemo.unity";

        private static readonly string[] Items = { "Attack", "Defend", "Item", "Magic", "Run", "Wait" };
        private static readonly Color[] Colors =
        {
            new Color(0.88f, 0.34f, 0.34f, 1f),
            new Color(0.34f, 0.55f, 0.88f, 1f),
            new Color(0.45f, 0.7f, 0.4f, 1f),
            new Color(0.66f, 0.45f, 0.85f, 1f),
            new Color(0.88f, 0.66f, 0.3f, 1f),
            new Color(0.45f, 0.55f, 0.62f, 1f),
        };

        [MenuItem("UIControls/Gameplay (F)/Create RadialMenu Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateRadialMenuDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Radial Menu Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 248f),
                "Tap the centre button to fan the actions out and back; pick one to select it.");

            var center = new Vector2(0f, 30f);

            // Items container with a CanvasGroup.
            var containerGo = new GameObject("Items", typeof(RectTransform), typeof(CanvasGroup));
            var container = containerGo.GetComponent<RectTransform>();
            container.SetParent(panel, false);
            container.anchoredPosition = center;
            container.sizeDelta = new Vector2(10f, 10f);
            var canvasGroup = containerGo.GetComponent<CanvasGroup>();

            var menuGo = new GameObject("RadialMenu", typeof(RectTransform), typeof(UIRadialMenuControl));
            var menuRect = menuGo.GetComponent<RectTransform>();
            menuRect.SetParent(panel, false);
            menuRect.anchoredPosition = Vector2.zero;
            var menu = menuGo.GetComponent<UIRadialMenuControl>();

            for (var i = 0; i < Items.Length; i++)
            {
                var item = UIDemoSceneFactory.Button(container, "Item" + i, Vector2.zero, new Vector2(110f, 110f), Items[i], Colors[i], 22);
            }

            UIDemoSceneFactory.SetRef(menu, "itemsContainer", container);
            UIDemoSceneFactory.SetRef(menu, "canvasGroup", canvasGroup);

            // Centre toggle button on top.
            var toggle = UIDemoSceneFactory.Button(panel, "ToggleButton", center, new Vector2(120f, 120f), "Menu",
                new Color(0.16f, 0.2f, 0.3f, 1f), 24);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -300f), new Vector2(900f, 36f),
                "Selected: —", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIRadialMenuDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "menu", menu);
            UIDemoSceneFactory.SetRef(presenter, "toggleButton", toggle);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            var so = new SerializedObject(presenter);
            var arr = so.FindProperty("itemNames");
            arr.arraySize = Items.Length;
            for (var i = 0; i < Items.Length; i++)
            {
                arr.GetArrayElementAtIndex(i).stringValue = Items[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
