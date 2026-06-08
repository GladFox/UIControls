using System;
using DG.Tweening;
using TMPro;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Custom animated single-select dropdown. The trigger shows the current value; opening reveals a
    /// panel of option rows (a fixed slot pool) that scales/fades in, with the selected row tinted.
    /// Picking a row closes the panel and raises <see cref="OnValueChanged"/>. A nicer alternative to
    /// the built-in uGUI dropdown.
    /// </summary>
    public sealed class UIDropdownControl : MonoBehaviour
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Trigger")]
        [SerializeField] private UIButtonControl triggerButton;
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField] private RectTransform arrow;

        [Header("Panel")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private UIButtonControl[] optionButtons;
        [SerializeField] private TMP_Text[] optionLabels;
        [SerializeField] private Graphic[] optionBackgrounds;

        [Header("Options")]
        [SerializeField] private string[] options = Array.Empty<string>();
        [SerializeField] private int selectedIndex;
        [SerializeField] private string placeholder = "Select…";

        [Header("Style")]
        [SerializeField] private Color rowNormal = new Color(0.16f, 0.2f, 0.3f, 0f);
        [SerializeField] private Color rowSelected = new Color(0.24f, 0.55f, 0.95f, 0.4f);

        [Header("Animation")]
        [SerializeField] private float duration = 0.18f;

        [Header("Events")]
        [SerializeField] private IntEvent onValueChanged = new IntEvent();

        private bool isOpen;

        public IntEvent OnValueChanged => onValueChanged;
        public int SelectedIndex => selectedIndex;
        public string SelectedValue => options != null && selectedIndex >= 0 && selectedIndex < options.Length ? options[selectedIndex] : string.Empty;

        private void Awake()
        {
            if (triggerButton != null) triggerButton.OnClick.AddListener(Toggle);
            if (optionButtons != null)
            {
                for (var i = 0; i < optionButtons.Length; i++)
                {
                    var index = i;
                    if (optionButtons[i] != null)
                    {
                        optionButtons[i].OnClick.AddListener(() => Choose(index));
                    }
                }
            }
        }

        private void OnEnable()
        {
            RenderOptions();
            RenderValue();
            CloseInstant();
        }

        public void SetOptions(string[] newOptions, int select = 0)
        {
            options = newOptions ?? Array.Empty<string>();
            selectedIndex = Mathf.Clamp(select, 0, Mathf.Max(0, options.Length - 1));
            RenderOptions();
            RenderValue();
        }

        public void Toggle()
        {
            if (isOpen) Close(); else Open();
        }

        public void Open()
        {
            isOpen = true;
            if (panel != null) panel.gameObject.SetActive(true);
            if (panelGroup != null)
            {
                panelGroup.blocksRaycasts = true;
                UIDOTweenUtility.TweenCanvasGroupAlpha(panelGroup, 1f, duration).SetUpdate(true);
            }

            if (panel != null)
            {
                panel.localScale = new Vector3(1f, 0.6f, 1f);
                panel.DOScaleY(1f, duration).SetEase(Ease.OutCubic).SetUpdate(true);
            }

            if (arrow != null) arrow.DOLocalRotate(new Vector3(0f, 0f, 180f), duration).SetUpdate(true);
        }

        public void Close()
        {
            isOpen = false;
            if (panelGroup != null)
            {
                panelGroup.blocksRaycasts = false;
                UIDOTweenUtility.TweenCanvasGroupAlpha(panelGroup, 0f, duration).SetUpdate(true)
                    .OnComplete(() => { if (panel != null) panel.gameObject.SetActive(false); });
            }
            else if (panel != null)
            {
                panel.gameObject.SetActive(false);
            }

            if (arrow != null) arrow.DOLocalRotate(Vector3.zero, duration).SetUpdate(true);
        }

        public void Choose(int index)
        {
            if (options == null || index < 0 || index >= options.Length)
            {
                Close();
                return;
            }

            selectedIndex = index;
            RenderValue();
            RenderOptions();
            Close();
            onValueChanged.Invoke(selectedIndex);
        }

        private void RenderValue()
        {
            if (valueLabel != null)
            {
                valueLabel.text = options != null && options.Length > 0 ? SelectedValue : placeholder;
            }
        }

        private void RenderOptions()
        {
            if (optionButtons == null)
            {
                return;
            }

            for (var i = 0; i < optionButtons.Length; i++)
            {
                var has = options != null && i < options.Length;
                if (optionButtons[i] != null) optionButtons[i].gameObject.SetActive(has);
                if (has && optionLabels != null && i < optionLabels.Length && optionLabels[i] != null)
                {
                    optionLabels[i].text = options[i];
                }

                if (optionBackgrounds != null && i < optionBackgrounds.Length && optionBackgrounds[i] != null)
                {
                    optionBackgrounds[i].color = i == selectedIndex ? rowSelected : rowNormal;
                }
            }
        }

        private void CloseInstant()
        {
            isOpen = false;
            if (panelGroup != null)
            {
                panelGroup.alpha = 0f;
                panelGroup.blocksRaycasts = false;
            }

            if (panel != null) panel.gameObject.SetActive(false);
            if (arrow != null) arrow.localEulerAngles = Vector3.zero;
        }
    }
}
