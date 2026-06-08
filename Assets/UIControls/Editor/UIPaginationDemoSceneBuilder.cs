using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIPaginationDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Navigation(G)/UIPaginationDemo.unity";
        private const int SlotCount = 7;

        [MenuItem("UIControls/Navigation (G)/Create Pagination Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreatePaginationDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Pagination Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Page numbers collapse to an ellipsis as the current page moves through 12 pages.");

            var prev = UIDemoSceneFactory.Button(panel, "Prev", new Vector2(-360f, 40f), new Vector2(72f, 72f), "‹",
                new Color(0.16f, 0.2f, 0.3f, 1f), 34);
            var next = UIDemoSceneFactory.Button(panel, "Next", new Vector2(360f, 40f), new Vector2(72f, 72f), "›",
                new Color(0.16f, 0.2f, 0.3f, 1f), 34);

            var buttons = new UIButtonControl[SlotCount];
            var labels = new TMP_Text[SlotCount];
            var backgrounds = new Graphic[SlotCount];
            var startX = -(SlotCount - 1) * 0.5f * 86f;

            for (var i = 0; i < SlotCount; i++)
            {
                var slotGo = new GameObject("Slot" + i, typeof(RectTransform), typeof(UIButtonControl));
                var slot = slotGo.GetComponent<RectTransform>();
                slot.SetParent(panel, false);
                slot.anchoredPosition = new Vector2(startX + i * 86f, 40f);
                slot.sizeDelta = new Vector2(72f, 72f);

                var bg = UIDemoSceneFactory.Image("Bg", slot, Vector2.zero, new Vector2(72f, 72f), new Color(0.18f, 0.24f, 0.36f, 1f), true);
                var label = UIDemoSceneFactory.Text("Label", slot, Vector2.zero, new Vector2(72f, 72f), (i + 1).ToString(), 26, FontStyles.Bold, TextAlignmentOptions.Center);
                label.color = UIDemoSceneFactory.Label;

                buttons[i] = slotGo.GetComponent<UIButtonControl>();
                labels[i] = label;
                backgrounds[i] = bg;
            }

            var pagerGo = new GameObject("PaginationControl", typeof(RectTransform), typeof(UIPaginationControl));
            (pagerGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var pager = pagerGo.GetComponent<UIPaginationControl>();
            UIDemoSceneFactory.SetRefArray(pager, "slotButtons", buttons);
            UIDemoSceneFactory.SetRefArray(pager, "slotLabels", labels);
            UIDemoSceneFactory.SetRefArray(pager, "slotBackgrounds", backgrounds);
            UIDemoSceneFactory.SetRef(pager, "prevButton", prev);
            UIDemoSceneFactory.SetRef(pager, "nextButton", next);
            var so = new SerializedObject(pager);
            so.FindProperty("pageCount").intValue = 12;
            so.FindProperty("currentPage").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -120f), new Vector2(900f, 36f),
                "Page 1 of 12", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIPaginationDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "pagination", pager);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
