using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UISwipeCardDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Gameplay(F)/UISwipeCardDemo.unity";

        [MenuItem("UIControls/Gameplay (F)/Create SwipeCard Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateSwipeCardDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Swipe Card Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 248f),
                "Drag the card past the edge (or use the buttons) to like / pass; a short drag springs back.");

            var cardGo = new GameObject("SwipeCard", typeof(RectTransform), typeof(Image), typeof(UISwipeCardControl));
            var card = cardGo.GetComponent<RectTransform>();
            card.SetParent(panel, false);
            card.anchoredPosition = new Vector2(0f, 40f);
            card.sizeDelta = new Vector2(460f, 440f);
            var cardImg = cardGo.GetComponent<Image>();
            cardImg.color = new Color(0.16f, 0.21f, 0.32f, 1f);
            cardImg.raycastTarget = true;

            var avatar = UIDemoSceneFactory.Image("Avatar", card, new Vector2(0f, 90f), new Vector2(150f, 150f),
                new Color(0.3f, 0.42f, 0.62f, 1f), false);
            UIDemoSceneFactory.Text("Initials", avatar.rectTransform, Vector2.zero, new Vector2(150f, 150f),
                "?", 60, FontStyles.Bold, TextAlignmentOptions.Center).color = UIDemoSceneFactory.Label;

            var name = UIDemoSceneFactory.Text("Name", card, new Vector2(0f, -40f), new Vector2(420f, 50f),
                "—", 32, FontStyles.Bold, TextAlignmentOptions.Center);
            var role = UIDemoSceneFactory.Text("Role", card, new Vector2(0f, -90f), new Vector2(420f, 40f),
                "—", 22, FontStyles.Italic, TextAlignmentOptions.Center);
            role.color = UIDemoSceneFactory.Muted;

            var like = Overlay(card, "LikeOverlay", new Vector2(-150f, 150f), 18f, "LIKE", new Color(0.3f, 0.78f, 0.45f, 1f));
            var nope = Overlay(card, "NopeOverlay", new Vector2(150f, 150f), -18f, "NOPE", new Color(0.88f, 0.34f, 0.34f, 1f));

            var control = cardGo.GetComponent<UISwipeCardControl>();
            UIDemoSceneFactory.SetRef(control, "likeOverlay", like);
            UIDemoSceneFactory.SetRef(control, "nopeOverlay", nope);

            var nopeBtn = UIDemoSceneFactory.Button(panel, "NopeButton", new Vector2(-130f, -300f), new Vector2(150f, 80f), "Pass",
                new Color(0.88f, 0.34f, 0.34f, 1f));
            var likeBtn = UIDemoSceneFactory.Button(panel, "LikeButton", new Vector2(130f, -300f), new Vector2(150f, 80f), "Like",
                new Color(0.3f, 0.7f, 0.45f, 1f));

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -360f), new Vector2(900f, 36f),
                "Swipe right to like, left to pass.", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UISwipeCardDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "card", control);
            UIDemoSceneFactory.SetRef(presenter, "nameLabel", name);
            UIDemoSceneFactory.SetRef(presenter, "roleLabel", role);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            UIDemoSceneFactory.SetRef(presenter, "likeButton", likeBtn);
            UIDemoSceneFactory.SetRef(presenter, "nopeButton", nopeBtn);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }

        private static CanvasGroup Overlay(RectTransform card, string name, Vector2 pos, float rotation, string text, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(card, false);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(170f, 70f);
            rect.localEulerAngles = new Vector3(0f, 0f, rotation);
            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var badge = UIDemoSceneFactory.Image("Badge", rect, Vector2.zero, new Vector2(170f, 70f), new Color(color.r, color.g, color.b, 0.18f), false);
            var label = UIDemoSceneFactory.Text("Text", badge.rectTransform, Vector2.zero, new Vector2(170f, 70f),
                text, 34, FontStyles.Bold, TextAlignmentOptions.Center);
            label.color = color;
            return group;
        }
    }
}
