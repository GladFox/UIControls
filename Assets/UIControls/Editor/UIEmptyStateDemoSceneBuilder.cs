using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIEmptyStateDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Data(I)/UIEmptyStateDemo.unity";

        [MenuItem("UIControls/Data (I)/Create EmptyState Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateEmptyStateDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Empty State Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "An empty-state placeholder with a call to action; add items to reveal the content.");

            // Card area framing both states.
            var card = UIDemoSceneFactory.Image("Card", panel, new Vector2(0f, 20f), new Vector2(700f, 400f), new Color(0.11f, 0.15f, 0.23f, 1f), false);

            // Empty-state root.
            var emptyGo = new GameObject("EmptyState", typeof(RectTransform));
            var empty = emptyGo.GetComponent<RectTransform>();
            empty.SetParent(card.rectTransform, false);
            empty.anchorMin = Vector2.zero; empty.anchorMax = Vector2.one; empty.offsetMin = Vector2.zero; empty.offsetMax = Vector2.zero;

            var icon = UIDemoSceneFactory.Text("Icon", empty, new Vector2(0f, 110f), new Vector2(200f, 120f), "🗂", 90, FontStyles.Normal, TextAlignmentOptions.Center);
            icon.color = UIDemoSceneFactory.Muted;
            var title = UIDemoSceneFactory.Text("Title", empty, new Vector2(0f, 10f), new Vector2(640f, 50f), "Nothing here yet", 32, FontStyles.Bold, TextAlignmentOptions.Center);
            var message = UIDemoSceneFactory.Text("Message", empty, new Vector2(0f, -44f), new Vector2(600f, 60f),
                "Your list is empty. Add a few sample items to get started.", 22, FontStyles.Normal, TextAlignmentOptions.Center);
            message.color = UIDemoSceneFactory.Muted;
            var action = UIDemoSceneFactory.Button(empty, "Action", new Vector2(0f, -130f), new Vector2(280f, 76f), "Add sample items",
                new Color(0.45f, 0.7f, 0.4f, 1f), 22);

            // Content root (hidden until filled).
            var contentGo = new GameObject("Content", typeof(RectTransform));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(card.rectTransform, false);
            content.anchorMin = Vector2.zero; content.anchorMax = Vector2.one; content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;
            string[] rows = { "🍎  Apples", "🥖  Bread", "🧀  Cheese" };
            for (var i = 0; i < rows.Length; i++)
            {
                var row = UIDemoSceneFactory.Image("Row" + i, content, new Vector2(0f, 130f - i * 90f), new Vector2(620f, 76f), new Color(0.18f, 0.24f, 0.36f, 1f), false);
                UIDemoSceneFactory.Text("Label", row.rectTransform, new Vector2(20f, 0f), new Vector2(560f, 60f), rows[i], 24, FontStyles.Bold, TextAlignmentOptions.Left).color = UIDemoSceneFactory.Label;
            }
            contentGo.SetActive(false);

            var esGo = new GameObject("EmptyStateControl", typeof(RectTransform), typeof(UIEmptyStateControl));
            (esGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var es = esGo.GetComponent<UIEmptyStateControl>();
            UIDemoSceneFactory.SetRef(es, "root", emptyGo);
            UIDemoSceneFactory.SetRef(es, "iconLabel", icon);
            UIDemoSceneFactory.SetRef(es, "titleLabel", title);
            UIDemoSceneFactory.SetRef(es, "messageLabel", message);
            UIDemoSceneFactory.SetRef(es, "actionButton", action);

            var clear = UIDemoSceneFactory.Button(panel, "Clear", new Vector2(0f, -260f), new Vector2(220f, 74f), "Clear list",
                new Color(0.5f, 0.34f, 0.6f, 1f));

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -330f), new Vector2(900f, 36f),
                "List is empty.", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIEmptyStateDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "emptyState", es);
            UIDemoSceneFactory.SetRef(presenter, "contentRoot", contentGo);
            UIDemoSceneFactory.SetRef(presenter, "clearButton", clear);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
