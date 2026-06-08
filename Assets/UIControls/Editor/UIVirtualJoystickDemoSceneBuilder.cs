using TMPro;
using UIControls.Runtime.Controls;
using UIControls.Runtime.Demo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Editor
{
    public static class UIVirtualJoystickDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Gameplay(F)/UIVirtualJoystickDemo.unity";

        [MenuItem("UIControls/Gameplay (F)/Create VirtualJoystick Demo Scene")]
        public static void FromMenu() => Build();

        public static void CreateVirtualJoystickDemoSceneBatch() => Build();

        private static void Build()
        {
            var panel = UIDemoSceneFactory.NewScene("UIControls Virtual Joystick Demo", out var scene);

            UIDemoSceneFactory.Caption(panel, new Vector2(0f, 248f),
                "Drag the joystick (bottom-left) to fly the ship around the field.");

            // Play field.
            var field = UIDemoSceneFactory.Image("Field", panel, new Vector2(0f, 70f), new Vector2(820f, 360f),
                new Color(0.12f, 0.16f, 0.24f, 1f), false);

            var ship = UIDemoSceneFactory.Image("Ship", field.rectTransform, Vector2.zero, new Vector2(46f, 46f),
                new Color(0.36f, 0.85f, 0.62f, 1f), false);

            // Joystick area (raycast target carries the control).
            var areaGo = new GameObject("Joystick", typeof(RectTransform), typeof(Image), typeof(UIVirtualJoystickControl));
            var area = areaGo.GetComponent<RectTransform>();
            area.SetParent(panel, false);
            area.anchoredPosition = new Vector2(0f, -200f);
            area.sizeDelta = new Vector2(260f, 260f);
            var areaImg = areaGo.GetComponent<Image>();
            areaImg.color = new Color(1f, 1f, 1f, 0.04f);
            areaImg.raycastTarget = true;

            var bg = UIDemoSceneFactory.Image("Background", area, Vector2.zero, new Vector2(220f, 220f),
                new Color(0.2f, 0.26f, 0.38f, 0.9f), false);
            var handle = UIDemoSceneFactory.Image("Handle", bg.rectTransform, Vector2.zero, new Vector2(96f, 96f),
                new Color(0.24f, 0.55f, 0.95f, 1f), false);

            var joystick = areaGo.GetComponent<UIVirtualJoystickControl>();
            UIDemoSceneFactory.SetRef(joystick, "background", bg.rectTransform);
            UIDemoSceneFactory.SetRef(joystick, "handle", handle.rectTransform);

            var status = UIDemoSceneFactory.Text("Status", panel, new Vector2(0f, -350f), new Vector2(900f, 36f),
                "dir (0.00, 0.00)   magnitude 0.00", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            status.color = UIDemoSceneFactory.Status;

            var presenter = panel.gameObject.AddComponent<UIVirtualJoystickDemoPresenter>();
            UIDemoSceneFactory.SetRef(presenter, "joystick", joystick);
            UIDemoSceneFactory.SetRef(presenter, "ship", ship.rectTransform);
            UIDemoSceneFactory.SetRef(presenter, "field", field.rectTransform);
            UIDemoSceneFactory.SetRef(presenter, "statusLabel", status);

            UIDemoSceneFactory.Save(scene, ScenePath);
        }
    }
}
