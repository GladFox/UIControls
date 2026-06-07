using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A circular (ring) progress indicator. A radial-filled ring shows progress 0..1 over a dim
    /// track; an optional center label shows the percentage. In <see cref="indeterminate"/> mode the
    /// ring becomes a spinning arc. The ring sprite is generated at runtime (no art needed).
    /// </summary>
    public sealed class UICircularProgressControl : MonoBehaviour
    {
        [Serializable]
        public sealed class ValueEvent : UnityEvent<float>
        {
        }

        [Header("Targets")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image trackImage;
        [SerializeField] private TMP_Text label;

        [Header("Value")]
        [Range(0f, 1f)]
        [SerializeField] private float value = 0.35f;
        [SerializeField] private bool indeterminate;
        [SerializeField] private bool showLabel = true;

        [Header("Indeterminate")]
        [SerializeField] private float spinSpeed = 200f;
        [Range(0.05f, 0.9f)]
        [SerializeField] private float indeterminateArc = 0.25f;

        [Header("Events")]
        [SerializeField] private ValueEvent onValueChanged = new ValueEvent();

        private Sprite ringSprite;
        private float spinAngle;

        public ValueEvent OnValueChanged => onValueChanged;

        public float Value
        {
            get => value;
            set => SetValue(value);
        }

        public bool Indeterminate
        {
            get => indeterminate;
            set { indeterminate = value; ApplyMode(); }
        }

        private void Awake()
        {
            BuildRingSprite();
        }

        private void OnEnable()
        {
            ApplyMode();
        }

        private void OnDisable()
        {
            if (ringSprite != null)
            {
                if (ringSprite.texture != null)
                {
                    Destroy(ringSprite.texture);
                }

                Destroy(ringSprite);
                ringSprite = null;
            }
        }

        private void Update()
        {
            if (!indeterminate || fillImage == null)
            {
                return;
            }

            spinAngle -= spinSpeed * Time.unscaledDeltaTime;
            fillImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, spinAngle);
        }

        public void SetValue(float newValue, bool notify = true)
        {
            value = Mathf.Clamp01(newValue);
            if (!indeterminate)
            {
                ApplyDeterminate();
            }

            if (notify)
            {
                onValueChanged?.Invoke(value);
            }
        }

        private void ApplyMode()
        {
            if (indeterminate)
            {
                if (fillImage != null)
                {
                    fillImage.fillAmount = indeterminateArc;
                }

                if (label != null)
                {
                    label.gameObject.SetActive(false);
                }
            }
            else
            {
                if (fillImage != null)
                {
                    fillImage.rectTransform.localEulerAngles = Vector3.zero;
                }

                if (label != null)
                {
                    label.gameObject.SetActive(showLabel);
                }

                ApplyDeterminate();
            }
        }

        private void ApplyDeterminate()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = value;
            }

            if (label != null && showLabel)
            {
                label.text = Mathf.RoundToInt(value * 100f) + "%";
            }
        }

        private void BuildRingSprite()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var center = (size - 1) * 0.5f;
            var outer = size * 0.48f;
            var inner = size * 0.34f;
            var clear = new Color(1f, 1f, 1f, 0f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    // Antialias the inner/outer edges by ~1px.
                    var a = Mathf.Clamp01(outer - d) * Mathf.Clamp01(d - inner);
                    a = Mathf.Clamp01(a);
                    tex.SetPixel(x, y, a > 0f ? new Color(1f, 1f, 1f, a) : clear);
                }
            }

            tex.Apply();
            ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);

            AssignSprite(fillImage, true);
            AssignSprite(trackImage, false);
        }

        private void AssignSprite(Image image, bool isFill)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = ringSprite;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = true;
            image.fillAmount = isFill ? value : 1f;
        }
    }
}
