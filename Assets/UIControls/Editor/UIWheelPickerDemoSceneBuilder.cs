using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIWheelPickerDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Forms(H)/UIWheelPickerDemo.unity";
        private static readonly string[] Months =
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December",
        };

        [MenuItem("UIControls/Forms (H)/Create WheelPicker Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateWheelPickerDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Wheel Picker Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Drag to spin the barrel; it snaps the nearest month to the centre line.");

            const float spacing = 64f;

            var viewportGo = new GameObject("Wheel", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(UIWheelPickerControl));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(panel, false);
            viewport.anchoredPosition = new Vector2(0f, 30f);
            viewport.sizeDelta = new Vector2(420f, 300f);
            var vpImg = viewportGo.GetComponent<Image>();
            vpImg.color = new Color(0.1f, 0.14f, 0.22f, 1f);
            vpImg.raycastTarget = true;

            // Centre highlight band.
            UIDemoSceneFactory.Image("CenterBand", viewport, Vector2.zero, new Vector2(420f, spacing), new Color(0.24f, 0.55f, 0.95f, 0.16f), false);

            var contentGo = new GameObject("Content", typeof(RectTransform));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.sizeDelta = new Vector2(420f, 10f);

            var labels = new TMP_Text[Months.Length];
            for (var i = 0; i < Months.Length; i++)
            {
                var label = UIDemoSceneFactory.Text("Item" + i, content, new Vector2(0f, -i * spacing), new Vector2(420f, spacing),
                    Months[i], 30, FontStyles.Bold, TextAlignmentOptions.Center);
                label.color = UIDemoSceneFactory.Label;
                label.raycastTarget = false;
                labels[i] = label;
            }

            var wheel = viewportGo.GetComponent<UIWheelPickerControl>();
            UIDemoSceneFactory.SetRef(wheel, "content", content);
            UIDemoSceneFactory.SetRefArray(wheel, "itemLabels", labels);
            var wso = new SerializedObject(wheel);
            wso.FindProperty("itemSpacing").floatValue = spacing;
            wso.FindProperty("selectedIndex").intValue = 5;
            wso.ApplyModifiedPropertiesWithoutUndo();

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -240f), new Vector2(900f, 36f),
                "Selected: June", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIWheelPickerDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "wheel", wheel);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            UIDemoSceneFactory.SetStringArray(presenter, "optionNames", Months);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
