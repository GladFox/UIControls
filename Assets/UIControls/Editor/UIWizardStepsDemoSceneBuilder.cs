using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIWizardStepsDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Navigation(G)/UIWizardStepsDemo.unity";
        private static readonly string[] Titles = { "Account", "Profile", "Plan", "Done" };

        [MenuItem("UIControls/Navigation (G)/Create WizardSteps Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateWizardStepsDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Wizard Steps Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Step indicator with done / active / upcoming states. Use Back / Next to advance.");

            var count = Titles.Length;
            var circles = new Graphic[count];
            var numbers = new TMP_Text[count];
            var titles = new TMP_Text[count];
            var connectors = new Graphic[count - 1];

            float[] xs = { -315f, -105f, 105f, 315f };
            const float y = 70f;

            // Connectors first (behind circles).
            for (var i = 0; i < count - 1; i++)
            {
                var midX = (xs[i] + xs[i + 1]) * 0.5f;
                var width = (xs[i + 1] - xs[i]) - 84f;
                var bar = UIDemoSceneFactory.Image("Connector" + i, panel, new Vector2(midX, y), new Vector2(width, 8f),
                    new Color(0.22f, 0.28f, 0.4f, 1f), false);
                connectors[i] = bar;
            }

            for (var i = 0; i < count; i++)
            {
                var circle = UIDemoSceneFactory.Image("Step" + i, panel, new Vector2(xs[i], y), new Vector2(84f, 84f),
                    new Color(0.22f, 0.28f, 0.4f, 1f), false);
                var num = UIDemoSceneFactory.Text("Num", circle.rectTransform, Vector2.zero, new Vector2(84f, 84f),
                    (i + 1).ToString(), 32, FontStyles.Bold, TextAlignmentOptions.Center);
                num.color = UIDemoSceneFactory.Label;
                var title = UIDemoSceneFactory.Text("Title", panel, new Vector2(xs[i], y - 76f), new Vector2(200f, 34f),
                    Titles[i], 20, FontStyles.Bold, TextAlignmentOptions.Center);

                circles[i] = circle;
                numbers[i] = num;
                titles[i] = title;
            }

            var wizGo = new GameObject("WizardStepsControl", typeof(RectTransform), typeof(UIWizardStepsControl));
            (wizGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var wizard = wizGo.GetComponent<UIWizardStepsControl>();
            UIDemoSceneFactory.SetRefArray(wizard, "stepCircles", circles);
            UIDemoSceneFactory.SetRefArray(wizard, "stepNumbers", numbers);
            UIDemoSceneFactory.SetRefArray(wizard, "stepTitles", titles);
            UIDemoSceneFactory.SetRefArray(wizard, "connectors", connectors);

            var back = UIDemoSceneFactory.Button(panel, "Back", new Vector2(-130f, -140f), new Vector2(200f, 76f), "Back",
                new Color(0.3f, 0.36f, 0.5f, 1f));
            var next = UIDemoSceneFactory.Button(panel, "Next", new Vector2(130f, -140f), new Vector2(200f, 76f), "Next");

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -230f), new Vector2(900f, 36f),
                "Step 1 of 4: Account", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIWizardStepsDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "wizard", wizard);
            UIDemoSceneFactory.SetRef(presenter, "backButton", back);
            UIDemoSceneFactory.SetRef(presenter, "nextButton", next);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            UIDemoSceneFactory.SetStringArray(presenter, "stepTitles", Titles);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
