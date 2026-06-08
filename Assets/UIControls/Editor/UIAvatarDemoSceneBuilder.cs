using TMPro;
using UIControls.Runtime.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIAvatarDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Data(I)/UIAvatarDemo.unity";

        [MenuItem("UIControls/Data (I)/Create Avatar Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateAvatarDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Avatar Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Initials and colour are derived from the name; a status dot shows presence.");

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 150f), "Standalone avatars with status:");

            var names = new[] { "Ada Lovelace", "Grace Hopper", "Alan Turing", "Katherine Johnson", "Linus T" };
            var statuses = new[]
            {
                UIAvatarControl.Status.Online, UIAvatarControl.Status.Busy, UIAvatarControl.Status.Away,
                UIAvatarControl.Status.Online, UIAvatarControl.Status.Offline,
            };

            for (var i = 0; i < names.Length; i++)
            {
                BuildAvatar(panel, new Vector2(-360f + i * 180f, 70f), 120f, names[i], statuses[i], false, null);
            }

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, -60f), "Overlapping avatar group with overflow:");

            var groupNames = new[] { "Mira Castillo", "Leo Marsh", "Priya Sundar", "Dmitri Volkov" };
            for (var i = 0; i < groupNames.Length; i++)
            {
                BuildAvatar(panel, new Vector2(-200f + i * 64f, -150f), 96f, groupNames[i], UIAvatarControl.Status.None, true, null);
            }
            // Overflow chip.
            BuildAvatar(panel, new Vector2(-200f + groupNames.Length * 64f, -150f), 96f, string.Empty, UIAvatarControl.Status.None, true, "+3");

            UIDemoSceneFactory.Save(scene, ScenePath);
        }

        private static void BuildAvatar(RectTransform panel, Vector2 pos, float size, string name, UIAvatarControl.Status status, bool ring, string overflow)
        {
            var avatarGo = new GameObject("Avatar", typeof(RectTransform), typeof(UIAvatarControl));
            var avatar = avatarGo.GetComponent<RectTransform>();
            avatar.SetParent(panel, false);
            avatar.anchoredPosition = pos;
            avatar.sizeDelta = new Vector2(size, size);

            // Optional ring (for grouped/overlapping look).
            if (ring)
            {
                UIDemoSceneFactory.Image("Ring", avatar, Vector2.zero, new Vector2(size + 10f, size + 10f), new Color(0.05f, 0.08f, 0.14f, 1f), false);
            }

            var bg = UIDemoSceneFactory.Image("Background", avatar, Vector2.zero, new Vector2(size, size), new Color(0.3f, 0.36f, 0.5f, 1f), false);
            var initials = UIDemoSceneFactory.Text("Initials", bg.rectTransform, Vector2.zero, new Vector2(size, size),
                "?", Mathf.RoundToInt(size * 0.38f), FontStyles.Bold, TextAlignmentOptions.Center);
            initials.color = Color.white;

            var dot = UIDemoSceneFactory.Image("StatusDot", bg.rectTransform, new Vector2(size * 0.34f, -size * 0.34f), new Vector2(size * 0.24f, size * 0.24f), new Color(0.3f, 0.78f, 0.45f, 1f), false);

            var control = avatarGo.GetComponent<UIAvatarControl>();
            UIDemoSceneFactory.SetRef(control, "background", bg);
            UIDemoSceneFactory.SetRef(control, "initialsLabel", initials);
            UIDemoSceneFactory.SetRef(control, "statusDot", dot);

            var so = new SerializedObject(control);
            so.FindProperty("displayName").stringValue = name;
            so.FindProperty("status").enumValueIndex = (int)status;
            if (!string.IsNullOrEmpty(overflow))
            {
                so.FindProperty("overrideLabel").stringValue = overflow;
                so.FindProperty("useCustomColor").boolValue = true;
                so.FindProperty("customColor").colorValue = new Color(0.3f, 0.36f, 0.5f, 1f);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
