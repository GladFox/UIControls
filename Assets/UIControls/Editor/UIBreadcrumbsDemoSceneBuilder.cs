using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIBreadcrumbsDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Navigation(G)/UIBreadcrumbsDemo.unity";
        private const int SlotCount = 5;
        private static readonly string[] FullPath = { "Home", "Library", "Controls", "Navigation" };

        [MenuItem("UIControls/Navigation (G)/Create Breadcrumbs Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateBreadcrumbsDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Breadcrumbs Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 250f),
                "Click any crumb to navigate up to it; the trail truncates there. Reset restores it.");

            var containerGo = new GameObject("Crumbs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var container = containerGo.GetComponent<RectTransform>();
            container.SetParent(panel, false);
            container.anchoredPosition = new Vector2(0f, 60f);
            container.sizeDelta = new Vector2(900f, 64f);
            var layout = containerGo.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var crumbButtons = new UIButtonControl[SlotCount];
            var crumbLabels = new TMP_Text[SlotCount];
            var separators = new GameObject[SlotCount - 1];

            for (var i = 0; i < SlotCount; i++)
            {
                var btn = UIDemoSceneFactory.Button(container, "Crumb" + i, Vector2.zero, new Vector2(190f, 56f),
                    "Crumb", new Color(0.16f, 0.2f, 0.3f, 1f), 22);
                crumbButtons[i] = btn;
                crumbLabels[i] = btn.transform.Find("Label").GetComponent<TMP_Text>();

                if (i < SlotCount - 1)
                {
                    var sep = UIDemoSceneFactory.Text("Sep" + i, container, Vector2.zero, new Vector2(30f, 56f),
                        "›", 30, FontStyles.Normal, TextAlignmentOptions.Center);
                    sep.color = UIDemoSceneFactory.Muted;
                    separators[i] = sep.gameObject;
                }
            }

            var crumbsGo = new GameObject("BreadcrumbsControl", typeof(RectTransform), typeof(UIBreadcrumbsControl));
            (crumbsGo.GetComponent<RectTransform>()).SetParent(panel, false);
            var crumbs = crumbsGo.GetComponent<UIBreadcrumbsControl>();
            UIDemoSceneFactory.SetRefArray(crumbs, "crumbButtons", crumbButtons);
            UIDemoSceneFactory.SetRefArray(crumbs, "crumbLabels", crumbLabels);
            UIDemoSceneFactory.SetRefArray(crumbs, "separators", separators);
            UIDemoSceneFactory.SetStringArray(crumbs, "path", FullPath);

            var reset = UIDemoSceneFactory.Button(panel, "Reset", new Vector2(0f, -60f), new Vector2(180f, 70f), "Reset",
                new Color(0.5f, 0.34f, 0.6f, 1f));

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -150f), new Vector2(900f, 36f),
                "Path: " + string.Join(" / ", FullPath), 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIBreadcrumbsDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "breadcrumbs", crumbs);
            UIDemoSceneFactory.SetRef(presenter, "resetButton", reset);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);
            UIDemoSceneFactory.SetStringArray(presenter, "fullPath", FullPath);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
