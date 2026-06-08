using System.Globalization;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UIVirtualJoystickDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UIVirtualJoystickControl joystick;
        [SerializeField] private RectTransform ship;
        [SerializeField] private RectTransform field;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private float speed = 320f;

        private void Update()
        {
            if (joystick == null || ship == null)
            {
                return;
            }

            var dir = joystick.Direction * joystick.Magnitude;
            var pos = ship.anchoredPosition + dir * (speed * Time.deltaTime);

            if (field != null)
            {
                var half = field.rect.size * 0.5f - ship.rect.size * 0.5f;
                pos.x = Mathf.Clamp(pos.x, -half.x, half.x);
                pos.y = Mathf.Clamp(pos.y, -half.y, half.y);
            }

            ship.anchoredPosition = pos;

            if (dir.sqrMagnitude > 0.0001f)
            {
                ship.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
            }

            if (statusLabel != null)
            {
                statusLabel.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "dir ({0:0.00}, {1:0.00})   magnitude {2:0.00}",
                    joystick.Horizontal, joystick.Vertical, joystick.Magnitude);
            }
        }
    }
}
