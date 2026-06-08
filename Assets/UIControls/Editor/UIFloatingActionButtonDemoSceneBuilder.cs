using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIFloatingActionButtonDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Navigation(G)/UIFloatingActionButtonDemo.unity";
        private static readonly string[] Actions = { "Edit", "Copy", "Share" };
        private static readonly string[] Icons = { "✎", "⧉", "↗" };
        private static readonly Color[] Colors =
        {
            new Color(0.45f, 0.7f, 0.4f, 1f),
            new Color(0.34f, 0.55f, 0.88f, 1f),
            new Color(0.85f, 0.55f, 0.3f, 1f),
        };

        [MenuItem("UIControls/Navigation (G)/Create FloatingActionButton Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateFloatingActionButtonDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Floating Action Button Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Tap the ＋ button to fan out the speed-dial actions; the icon rotates to ×.");

            var anchor = new Vector2(300f, -230f);

            // Actions container with CanvasGroup.
            var containerGo = new GameObject("Actions", typeof(RectTransform), typeof(CanvasGroup));
            var container = containerGo.GetComponent<RectTransform>();
            container.SetParent(panel, false);
            container.anchoredPosition = anchor;
            container.sizeDelta = new Vector2(10f, 10f);
            var group = containerGo.GetComponent<CanvasGroup>();

            for (var i = 0; i < Actions.Length; i++)
            {
                var action = UIDemoSceneFactory.Button(container, "Action_" + Actions[i], Vector2.zero, new Vector2(84f, 84f), Icons[i], Colors[i], 30);
                UIDemoSceneFactory.Text("Tag", action.transform as RectTransform, new Vector2(-118f, 0f), new Vector2(160f, 40f),
                    Actions[i], 20, FontStyles.Bold, TextAlignmentOptions.Right).color = UIDemoSceneFactory.Muted;
            }

            // Main FAB on top.
            var main = UIDemoSceneFactory.Button(panel, "Fab", anchor, new Vector2(120f, 120f), "＋",
                new Color(0.24f, 0.55f, 0.95f, 1f), 48);
            var mainIcon = main.transform.Find("Label") as RectTransform;

            var fabGo = new GameObject("FabControl", typeof(RectTransform), typeof(UIFloatingActionButtonControl));
            (fabGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var fab = fabGo.GetComponent<UIFloatingActionButtonControl>();
            UIDemoSceneFactory.SetRef(fab, "mainButton", main);
            UIDemoSceneFactory.SetRef(fab, "mainIcon", mainIcon);
            UIDemoSceneFactory.SetRef(fab, "actionsContainer", container);
            UIDemoSceneFactory.SetRef(fab, "actionsGroup", group);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(-120f, -250f), new Vector2(640f, 36f),
                "Action: —", 22, FontStyles.Bold, TextAlignmentOptions.Left);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIFloatingActionButtonDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "fab", fab);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            UIDemoSceneFactory.SetStringArray(presenter, "actionNames", Actions);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
