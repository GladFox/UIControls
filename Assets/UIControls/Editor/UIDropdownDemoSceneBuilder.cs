using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIDropdownDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Forms(H)/UIDropdownDemo.unity";
        private static readonly string[] Options = { "Warrior", "Mage", "Rogue", "Ranger", "Cleric", "Bard" };

        [MenuItem("UIControls/Forms (H)/Create Dropdown Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateDropdownDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Dropdown Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Animated select: the panel scales open, the arrow flips, the chosen row is highlighted.");

            // Trigger.
            var trigger = UIDemoSceneFactory.Button(panel, "Trigger", new Vector2(0f, 130f), new Vector2(440f, 76f),
                "Select…", new Color(0.16f, 0.2f, 0.3f, 1f), 26);
            var valueLabel = trigger.transform.Find("Label").GetComponent<TMP_Text>();
            (valueLabel.transform as RectTransform).anchoredPosition = new Vector2(-20f, 0f);
            var arrow = UIDemoSceneFactory.Text("Arrow", trigger.transform as RectTransform, new Vector2(180f, 0f), new Vector2(40f, 40f),
                "▾", 28, FontStyles.Bold, TextAlignmentOptions.Center);
            arrow.color = UIDemoSceneFactory.Muted;

            // Panel.
            const int slots = 6;
            const float rowH = 58f;
            var panelGo = new GameObject("OptionsPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(RectMask2D));
            var optPanel = panelGo.GetComponent<RectTransform>();
            optPanel.SetParent(panel, false);
            optPanel.pivot = new Vector2(0.5f, 1f);
            optPanel.anchoredPosition = new Vector2(0f, 92f);
            optPanel.sizeDelta = new Vector2(440f, slots * rowH + 12f);
            panelGo.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.22f, 1f);
            var panelGroup = panelGo.GetComponent<CanvasGroup>();

            var buttons = new UIButtonControl[slots];
            var labels = new TMP_Text[slots];
            var backgrounds = new Graphic[slots];

            for (var i = 0; i < slots; i++)
            {
                var rowGo = new GameObject("Option" + i, typeof(RectTransform), typeof(UIButtonControl));
                var row = rowGo.GetComponent<RectTransform>();
                row.SetParent(optPanel, false);
                row.anchorMin = new Vector2(0.5f, 1f);
                row.anchorMax = new Vector2(0.5f, 1f);
                row.pivot = new Vector2(0.5f, 1f);
                row.sizeDelta = new Vector2(424f, rowH);
                row.anchoredPosition = new Vector2(0f, -6f - i * rowH);

                var bg = UIDemoSceneFactory.Image("Bg", row, Vector2.zero, new Vector2(424f, rowH), new Color(0.24f, 0.55f, 0.95f, 0f), true);
                var label = UIDemoSceneFactory.Text("Label", row, Vector2.zero, new Vector2(404f, rowH), "", 24, FontStyles.Normal, TextAlignmentOptions.Left);
                label.color = UIDemoSceneFactory.Label;

                buttons[i] = rowGo.GetComponent<UIButtonControl>();
                labels[i] = label;
                backgrounds[i] = bg;
            }

            var ddGo = new GameObject("DropdownControl", typeof(RectTransform), typeof(UIDropdownControl));
            (ddGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var dropdown = ddGo.GetComponent<UIDropdownControl>();
            UIDemoSceneFactory.SetRef(dropdown, "triggerButton", trigger);
            UIDemoSceneFactory.SetRef(dropdown, "valueLabel", valueLabel);
            UIDemoSceneFactory.SetRef(dropdown, "arrow", arrow.rectTransform);
            UIDemoSceneFactory.SetRef(dropdown, "panel", optPanel);
            UIDemoSceneFactory.SetRef(dropdown, "panelGroup", panelGroup);
            UIDemoSceneFactory.SetRefArray(dropdown, "optionButtons", buttons);
            UIDemoSceneFactory.SetRefArray(dropdown, "optionLabels", labels);
            UIDemoSceneFactory.SetRefArray(dropdown, "optionBackgrounds", backgrounds);
            UIDemoSceneFactory.SetStringArray(dropdown, "options", Options);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -260f), new Vector2(900f, 36f),
                "Selected: —", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIDropdownDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "dropdown", dropdown);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
