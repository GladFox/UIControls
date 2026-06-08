using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UITagInputDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Forms(H)/UITagInputDemo.unity";

        [MenuItem("UIControls/Forms (H)/Create TagInput Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateTagInputDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Tag Input Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Type a tag and press Enter to add a chip; click × on a chip to remove it.");

            var input = UIDemoSceneFactory.InputField("TagField", panel, new Vector2(-90f, 140f), new Vector2(320f, 76f), "Add a tag…");

            var clear = UIDemoSceneFactory.Button(panel, "Clear", new Vector2(190f, 140f), new Vector2(150f, 76f), "Clear",
                new Color(0.5f, 0.34f, 0.6f, 1f), 24);

            // Chips container (single masked row).
            var containerGo = new GameObject("Chips", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(HorizontalLayoutGroup));
            var container = containerGo.GetComponent<RectTransform>();
            container.SetParent(panel, false);
            container.anchoredPosition = new Vector2(0f, 50f);
            container.sizeDelta = new Vector2(660f, 70f);
            containerGo.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.22f, 0.6f);
            var clayout = containerGo.GetComponent<HorizontalLayoutGroup>();
            clayout.spacing = 8f;
            clayout.padding = new RectOffset(10, 10, 8, 8);
            clayout.childAlignment = TextAnchor.MiddleLeft;
            clayout.childControlWidth = false;
            clayout.childControlHeight = false;
            clayout.childForceExpandWidth = false;
            clayout.childForceExpandHeight = false;

            var template = BuildChipTemplate(container);

            var tagGo = new GameObject("TagInputControl", typeof(RectTransform), typeof(UITagInputControl));
            (tagGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var tagInput = tagGo.GetComponent<UITagInputControl>();
            UIDemoSceneFactory.SetRef(tagInput, "inputField", input);
            UIDemoSceneFactory.SetRef(tagInput, "chipsContainer", container);
            UIDemoSceneFactory.SetRef(tagInput, "chipTemplate", template);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -50f), new Vector2(900f, 36f),
                "No tags yet — type and press Enter.", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UITagInputDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "tagInput", tagInput);
            UIDemoSceneFactory.SetRef(presenter, "clearButton", clear);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }

        private static GameObject BuildChipTemplate(RectTransform container)
        {
            var chipGo = new GameObject("ChipTemplate", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            var chip = chipGo.GetComponent<RectTransform>();
            chip.SetParent(container, false);
            chip.sizeDelta = new Vector2(120f, 50f);
            chipGo.GetComponent<Image>().color = new Color(0.24f, 0.55f, 0.95f, 0.9f);

            var layout = chipGo.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(16, 8, 6, 6);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = chipGo.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            labelGo.GetComponent<RectTransform>().SetParent(chip, false);
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "tag";
            label.fontSize = 22;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            labelGo.GetComponent<LayoutElement>().minHeight = 38f;

            // Remove button: graphic-less root + Bg child + "×" label.
            var removeGo = new GameObject("Remove", typeof(RectTransform), typeof(UIButtonControl), typeof(LayoutElement));
            removeGo.GetComponent<RectTransform>().SetParent(chip, false);
            var removeLayout = removeGo.GetComponent<LayoutElement>();
            removeLayout.preferredWidth = 34f;
            removeLayout.preferredHeight = 34f;

            var removeBg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var removeBgRect = removeBg.GetComponent<RectTransform>();
            removeBgRect.SetParent(removeGo.transform, false);
            removeBgRect.anchorMin = Vector2.zero; removeBgRect.anchorMax = Vector2.one;
            removeBgRect.offsetMin = Vector2.zero; removeBgRect.offsetMax = Vector2.zero;
            removeBg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);

            var xGo = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
            var xRect = xGo.GetComponent<RectTransform>();
            xRect.SetParent(removeGo.transform, false);
            xRect.anchorMin = Vector2.zero; xRect.anchorMax = Vector2.one;
            xRect.offsetMin = Vector2.zero; xRect.offsetMax = Vector2.zero;
            var x = xGo.GetComponent<TextMeshProUGUI>();
            x.text = "×";
            x.fontSize = 26;
            x.fontStyle = FontStyles.Bold;
            x.alignment = TextAlignmentOptions.Center;
            x.color = Color.white;
            x.raycastTarget = false;

            chipGo.SetActive(false);
            return chipGo;
        }
    }
}
