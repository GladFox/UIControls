using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIKnobDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Gameplay(F)/UIKnobDemo.unity";

        [MenuItem("UIControls/Gameplay (F)/Create Knob Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateKnobDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Knob / Rotary Dial Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 248f),
                "Drag around each dial to turn it. Left is continuous; right snaps to 5 steps.");

            BuildKnob(panel, new Vector2(-220f, 30f), "Volume", "%", 100f, 0, 0.6f);
            BuildKnob(panel, new Vector2(220f, 30f), "Quality", "", 5f, 5, 0.5f);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }

        private static void BuildKnob(RectTransform panel, Vector2 pos, string caption, string suffix, float scale, int steps, float initial)
        {
            var areaGo = new GameObject("Knob_" + caption, typeof(RectTransform), typeof(Image), typeof(UIKnobControl));
            var area = areaGo.GetComponent<RectTransform>();
            area.SetParent(panel, false);
            area.anchoredPosition = pos;
            area.sizeDelta = new Vector2(220f, 220f);
            var areaImg = areaGo.GetComponent<Image>();
            areaImg.color = new Color(1f, 1f, 1f, 0.04f);
            areaImg.raycastTarget = true;

            var cap = UIDemoSceneFactory.Image("Cap", area, Vector2.zero, new Vector2(180f, 180f),
                new Color(0.18f, 0.22f, 0.32f, 1f), false);
            // Indicator notch near the top of the cap; rotates with it.
            UIDemoSceneFactory.Image("Indicator", cap.rectTransform, new Vector2(0f, 62f), new Vector2(16f, 44f),
                new Color(0.24f, 0.55f, 0.95f, 1f), false);

            var knob = areaGo.GetComponent<UIKnobControl>();
            UIDemoSceneFactory.SetRef(knob, "knobVisual", cap.rectTransform);
            var kso = new SerializedObject(knob);
            kso.FindProperty("value").floatValue = initial;
            kso.FindProperty("steps").intValue = steps;
            kso.ApplyModifiedPropertiesWithoutUndo();

            var cap2 = UIDemoSceneFactory.Text("Caption", panel, pos + new Vector2(0f, -140f), new Vector2(260f, 32f),
                caption, 22, FontStyles.Bold, TextAlignmentOptions.Center);
            cap2.color = UIDemoSceneFactory.Muted;

            var value = UIDemoSceneFactory.Text("Value", panel, pos + new Vector2(0f, -178f), new Vector2(260f, 34f),
                "—", 24, FontStyles.Bold, TextAlignmentOptions.Center);
            value.color = UIDemoSceneFactory.Status;

            var presenter = areaGo.AddComponent<UIKnobDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "knob", knob);
            UIDemoSceneFactory.SetRef(presenter, "valueLabel", value);
            var pso = new SerializedObject(presenter);
            pso.FindProperty("suffix").stringValue = suffix;
            pso.FindProperty("scale").floatValue = scale;
            pso.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
