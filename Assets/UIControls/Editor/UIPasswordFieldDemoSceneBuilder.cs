using TMPro;
using UIControls.Runtime.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIPasswordFieldDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Forms(H)/UIPasswordFieldDemo.unity";

        [MenuItem("UIControls/Forms (H)/Create PasswordField Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreatePasswordFieldDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Password Field Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Type a password: the meter rates it live; Show / Hide reveals the characters.");

            var input = UIDemoSceneFactory.InputField("Password", panel, new Vector2(-70f, 130f), new Vector2(360f, 78f), "Password…");
            input.contentType = TMP_InputField.ContentType.Password;

            var toggle = UIDemoSceneFactory.Button(panel, "ToggleReveal", new Vector2(200f, 130f), new Vector2(150f, 78f), "Show",
                new Color(0.16f, 0.2f, 0.3f, 1f), 24);
            var toggleLabel = toggle.transform.Find("Label").GetComponent<TMP_Text>();

            // Strength meter.
            var track = UIDemoSceneFactory.Image("StrengthTrack", panel, new Vector2(0f, 50f), new Vector2(510f, 18f), new Color(0.13f, 0.18f, 0.27f, 1f), false);

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fill = fillGo.GetComponent<RectTransform>();
            fill.SetParent(track.rectTransform, false);
            fill.anchorMin = new Vector2(0f, 0.5f);
            fill.anchorMax = new Vector2(0f, 0.5f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.anchoredPosition = Vector2.zero;
            fill.sizeDelta = new Vector2(0f, 18f);
            fillGo.GetComponent<Image>().color = new Color(0.88f, 0.34f, 0.34f, 1f);

            var strengthLabel = UIDemoSceneFactory.Text("Strength", panel, new Vector2(0f, 14f), new Vector2(500f, 34f),
                "", 22, FontStyles.Bold, TextAlignmentOptions.Center);

            var pwGo = new GameObject("PasswordControl", typeof(RectTransform), typeof(UIPasswordFieldControl));
            (pwGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var pw = pwGo.GetComponent<UIPasswordFieldControl>();
            UIDemoSceneFactory.SetRef(pw, "inputField", input);
            UIDemoSceneFactory.SetRef(pw, "toggleButton", toggle);
            UIDemoSceneFactory.SetRef(pw, "toggleLabel", toggleLabel);
            UIDemoSceneFactory.SetRef(pw, "strengthFill", fill);
            UIDemoSceneFactory.SetRef(pw, "strengthLabel", strengthLabel);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
