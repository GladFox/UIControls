using TMPro;
using UIControls.Runtime.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UITreeViewDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Data(I)/UITreeViewDemo.unity";

        [MenuItem("UIControls/Data (I)/Create TreeView Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateTreeViewDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Tree View Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Click a folder row to expand / collapse it; the chevron rotates and the tree reflows.");

            var rootGo = new GameObject("Tree", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var root = rootGo.GetComponent<RectTransform>();
            root.SetParent(panel, false);
            root.anchorMin = new Vector2(0.5f, 1f);
            root.anchorMax = new Vector2(0.5f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            root.anchoredPosition = new Vector2(0f, 200f);
            root.sizeDelta = new Vector2(640f, 0f);
            ConfigVLG(rootGo.GetComponent<VerticalLayoutGroup>(), 0);
            var fitter = rootGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var assets = AddNode(root, "📁  Assets", true, true, out var assetsChildren);
            var textures = AddNode(assetsChildren, "📁  Textures", true, true, out var texturesChildren);
            AddNode(texturesChildren, "🖼  hero.png", false, false, out _);
            AddNode(texturesChildren, "🖼  tiles.png", false, false, out _);
            var audio = AddNode(assetsChildren, "📁  Audio", true, false, out var audioChildren);
            AddNode(audioChildren, "♪  music.ogg", false, false, out _);
            AddNode(audioChildren, "♪  jump.wav", false, false, out _);
            var scripts = AddNode(assetsChildren, "📁  Scripts", true, true, out var scriptsChildren);
            AddNode(scriptsChildren, "≣  Player.cs", false, false, out _);
            AddNode(scriptsChildren, "≣  Enemy.cs", false, false, out _);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }

        private static GameObject AddNode(RectTransform container, string text, bool branch, bool expanded, out RectTransform childrenContainer)
        {
            childrenContainer = null;

            var nodeGo = new GameObject("Node", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(UITreeNodeControl));
            var node = nodeGo.GetComponent<RectTransform>();
            node.SetParent(container, false);
            ConfigVLG(nodeGo.GetComponent<VerticalLayoutGroup>(), 0);

            // Row.
            var rowGo = new GameObject("Row", typeof(RectTransform), typeof(UIButtonControl), typeof(LayoutElement));
            var row = rowGo.GetComponent<RectTransform>();
            row.SetParent(node, false);
            var rowLayout = rowGo.GetComponent<LayoutElement>();
            rowLayout.minHeight = 48f;
            rowLayout.preferredHeight = 48f;

            UIDemoSceneFactory.Image("Bg", row, Vector2.zero, new Vector2(620f, 44f), new Color(0.16f, 0.21f, 0.32f, 1f), true);

            RectTransform chevron = null;
            if (branch)
            {
                var chev = UIDemoSceneFactory.Text("Chevron", row, new Vector2(-280f, 0f), new Vector2(40f, 44f),
                    "▸", 26, FontStyles.Bold, TextAlignmentOptions.Center);
                chev.color = UIDemoSceneFactory.Muted;
                chevron = chev.rectTransform;
            }

            var label = UIDemoSceneFactory.Text("Label", row, new Vector2(20f, 0f), new Vector2(560f, 44f), text, 24, FontStyles.Normal, TextAlignmentOptions.Left);
            label.color = UIDemoSceneFactory.Label;

            var control = nodeGo.GetComponent<UITreeNodeControl>();
            UIDemoSceneFactory.SetRef(control, "rowButton", rowGo.GetComponent<UIButtonControl>());

            if (branch)
            {
                var childGo = new GameObject("Children", typeof(RectTransform), typeof(VerticalLayoutGroup));
                var child = childGo.GetComponent<RectTransform>();
                child.SetParent(node, false);
                ConfigVLG(childGo.GetComponent<VerticalLayoutGroup>(), 34);
                childrenContainer = child;

                UIDemoSceneFactory.SetRef(control, "childrenContainer", childGo);
                if (chevron != null) UIDemoSceneFactory.SetRef(control, "chevron", chevron);

                var so = new SerializedObject(control);
                so.FindProperty("expanded").boolValue = expanded;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return nodeGo;
        }

        private static void ConfigVLG(VerticalLayoutGroup vlg, int indentLeft)
        {
            vlg.spacing = 4f;
            vlg.padding = new RectOffset(indentLeft, 0, 0, 0);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }
    }
}
