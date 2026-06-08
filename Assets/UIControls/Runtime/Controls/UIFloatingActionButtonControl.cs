using System;
using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Floating action button with a speed-dial: tapping the main button fans a stack of mini action
    /// buttons upward (staggered) and rotates the main icon (＋ → ×). Each action child that carries a
    /// <see cref="UIButtonControl"/> is auto-wired to raise <see cref="OnActionSelected"/> and close the
    /// dial. Actions live under <see cref="actionsContainer"/>.
    /// </summary>
    public sealed class UIFloatingActionButtonControl : MonoBehaviour
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [SerializeField] private UIButtonControl mainButton;
        [SerializeField] private RectTransform mainIcon;
        [SerializeField] private RectTransform actionsContainer;
        [SerializeField] private CanvasGroup actionsGroup;

        [Header("Layout")]
        [Tooltip("Vertical gap between stacked action buttons.")]
        [SerializeField] private float spacing = 84f;
        [Tooltip("Offset of the first action above the main button.")]
        [SerializeField] private float firstOffset = 96f;

        [Header("Animation")]
        [SerializeField] private float duration = 0.24f;
        [SerializeField] private float stagger = 0.04f;
        [SerializeField] private float openIconAngle = 135f;

        [Header("Events")]
        [SerializeField] private IntEvent onActionSelected = new IntEvent();

        private RectTransform[] actions;
        private bool isOpen;

        public IntEvent OnActionSelected => onActionSelected;
        public bool IsOpen => isOpen;

        private void Awake()
        {
            CollectActions();
            if (mainButton != null)
            {
                mainButton.OnClick.AddListener(Toggle);
            }
        }

        private void OnEnable()
        {
            ApplyClosedInstant();
        }

        private void CollectActions()
        {
            if (actionsContainer == null)
            {
                actions = Array.Empty<RectTransform>();
                return;
            }

            var count = actionsContainer.childCount;
            actions = new RectTransform[count];
            for (var i = 0; i < count; i++)
            {
                var child = actionsContainer.GetChild(i) as RectTransform;
                actions[i] = child;
                var index = i;
                var button = child != null ? child.GetComponent<UIButtonControl>() : null;
                if (button != null)
                {
                    button.OnClick.AddListener(() => Select(index));
                }
            }
        }

        public void Toggle()
        {
            if (isOpen) Close(); else Open();
        }

        public void Open()
        {
            if (actions == null) CollectActions();
            isOpen = true;

            if (actionsGroup != null)
            {
                actionsGroup.blocksRaycasts = true;
                UIDOTweenUtility.TweenCanvasGroupAlpha(actionsGroup, 1f, duration).SetUpdate(true);
            }

            for (var i = 0; i < actions.Length; i++)
            {
                var item = actions[i];
                if (item == null) continue;
                item.gameObject.SetActive(true);
                item.localScale = Vector3.one * 0.6f;
                var target = new Vector2(0f, firstOffset + spacing * i);
                var delay = stagger * i;
                UIDOTweenUtility.TweenAnchoredPosition(item, target, duration).SetEase(Ease.OutBack).SetDelay(delay).SetUpdate(true);
                item.DOScale(Vector3.one, duration).SetEase(Ease.OutBack).SetDelay(delay).SetUpdate(true);
            }

            if (mainIcon != null)
            {
                mainIcon.DOLocalRotate(new Vector3(0f, 0f, openIconAngle), duration).SetUpdate(true);
            }
        }

        public void Close()
        {
            isOpen = false;

            if (actionsGroup != null)
            {
                actionsGroup.blocksRaycasts = false;
                UIDOTweenUtility.TweenCanvasGroupAlpha(actionsGroup, 0f, duration).SetUpdate(true);
            }

            if (actions != null)
            {
                foreach (var item in actions)
                {
                    if (item == null) continue;
                    UIDOTweenUtility.TweenAnchoredPosition(item, Vector2.zero, duration).SetEase(Ease.InBack).SetUpdate(true);
                    item.DOScale(Vector3.one * 0.6f, duration).SetEase(Ease.InBack).SetUpdate(true);
                }
            }

            if (mainIcon != null)
            {
                mainIcon.DOLocalRotate(Vector3.zero, duration).SetUpdate(true);
            }
        }

        public void Select(int index)
        {
            onActionSelected.Invoke(index);
            Close();
        }

        private void ApplyClosedInstant()
        {
            isOpen = false;
            if (actionsGroup != null)
            {
                actionsGroup.alpha = 0f;
                actionsGroup.blocksRaycasts = false;
            }

            if (actions != null)
            {
                foreach (var item in actions)
                {
                    if (item == null) continue;
                    item.anchoredPosition = Vector2.zero;
                    item.localScale = Vector3.one * 0.6f;
                }
            }

            if (mainIcon != null)
            {
                mainIcon.localEulerAngles = Vector3.zero;
            }
        }
    }
}
