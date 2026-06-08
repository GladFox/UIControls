using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIReorderableListDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Gameplay(F)/UIReorderableListDemo.unity";

        private static readonly string[] Rows =
        {
            "1. Opening cinematic",
            "2. Tutorial level",
            "3. First boss",
            "4. Hub world",
            "5. Side quests",
            "6. Final showdown",
        };

        [MenuItem("UIControls/Gameplay (F)/Create ReorderableList Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateReorderableListDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Reorderable List Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 248f),
                "Drag any row up or down — the others slide aside and the order commits on drop.");

            var listGo = new GameObject("ReorderableList", typeof(RectTransform), typeof(UIReorderableListControl));
            var listRect = listGo.GetComponent<RectTransform>();
            listRect.SetParent(panel, false);
            listRect.anchoredPosition = Vector2.zero;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(listRect, false);
            content.anchorMin = new Vector2(0.5f, 1f);
            content.anchorMax = new Vector2(0.5f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = new Vector2(0f, 200f);
            content.sizeDelta = new Vector2(620f, 500f);

            for (var i = 0; i < Rows.Length; i++)
            {
                var row = UIDemoSceneFactory.Image("Row" + i, content, Vector2.zero, new Vector2(620f, 72f),
                    new Color(0.18f, 0.24f, 0.36f, 1f), true);
                var label = UIDemoSceneFactory.Text("Label", row.rectTransform, Vector2.zero, new Vector2(560f, 60f),
                    Rows[i], 24, FontStyles.Bold, TextAlignmentOptions.Left);
                label.color = UIDemoSceneFactory.Label;
                UIDemoSceneFactory.Text("Grip", row.rectTransform, new Vector2(270f, 0f), new Vector2(60f, 60f),
                    "≡", 30, FontStyles.Normal, TextAlignmentOptions.Center).color = UIDemoSceneFactory.Muted;
            }

            var list = listGo.GetComponent<UIReorderableListControl>();
            UIDemoSceneFactory.SetRef(list, "content", content);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -300f), new Vector2(900f, 36f),
                "Drag rows to reorder.", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIReorderableListDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "list", list);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
