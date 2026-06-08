using System;
using DG.Tweening;
using TMPro;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// Pop-up context menu. Click anywhere on this surface to open the menu at the cursor; it flips
    /// near the edges so it always stays on-screen, fades/scales in, and closes when you click away or
    /// pick an item. Item children that carry a <see cref="UIButtonControl"/> are auto-wired to raise
    /// <see cref="OnItemSelected"/>.
    /// </summary>
    public sealed class UIContextMenuControl : MonoBehaviour, IPointerClickHandler
    {
        [Serializable]
        public sealed class IntEvent : UnityEvent<int>
        {
        }

        [Header("Targets")]
        [SerializeField] private RectTransform menuPanel;
        [SerializeField] private CanvasGroup menuGroup;
        [Tooltip("Bounds used for edge-flipping; defaults to this object's RectTransform.")]
        [SerializeField] private RectTransform bounds;

        [Header("Items")]
        [SerializeField] private UIButtonControl[] itemButtons;

        [Header("Animation")]
        [SerializeField] private float duration = 0.15f;

        [Header("Events")]
        [SerializeField] private IntEvent onItemSelected = new IntEvent();

        private RectTransform self;
        private bool isOpen;

        public IntEvent OnItemSelected => onItemSelected;
        public bool IsOpen => isOpen;

        private void Awake()
        {
            self = transform as RectTransform;
            if (bounds == null) bounds = self;

            if (itemButtons != null)
            {
                for (var i = 0; i < itemButtons.Length; i++)
                {
                    var index = i;
                    if (itemButtons[i] != null)
                    {
                        itemButtons[i].OnClick.AddListener(() => Select(index));
                    }
                }
            }
        }

        private void OnEnable()
        {
            CloseInstant();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isOpen)
            {
                Close();
                return;
            }

            ShowAt(eventData.position, eventData.pressEventCamera);
        }

        public void ShowAt(Vector2 screenPosition, Camera cam)
        {
            if (menuPanel == null || bounds == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, screenPosition, cam, out var local))
            {
                return;
            }

            // Flip the pivot so the panel always stays inside the bounds. By default the menu opens
            // with its top-left at the cursor (pivot 0,1), extending right and down.
            var size = menuPanel.rect.size;
            var rightFits = local.x + size.x <= bounds.rect.xMax;
            var belowFits = local.y - size.y >= bounds.rect.yMin;
            menuPanel.pivot = new Vector2(rightFits ? 0f : 1f, belowFits ? 1f : 0f);
            menuPanel.anchoredPosition = local;

            Open();
        }

        public void Open()
        {
            isOpen = true;
            if (menuPanel != null) menuPanel.gameObject.SetActive(true);
            if (menuGroup != null)
            {
                menuGroup.blocksRaycasts = true;
                UIDOTweenUtility.TweenCanvasGroupAlpha(menuGroup, 1f, duration).SetUpdate(true);
            }

            if (menuPanel != null)
            {
                menuPanel.localScale = Vector3.one * 0.9f;
                menuPanel.DOScale(Vector3.one, duration).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        public void Close()
        {
            isOpen = false;
            if (menuGroup != null)
            {
                menuGroup.blocksRaycasts = false;
                UIDOTweenUtility.TweenCanvasGroupAlpha(menuGroup, 0f, duration).SetUpdate(true)
                    .OnComplete(() => { if (menuPanel != null) menuPanel.gameObject.SetActive(false); });
            }
            else if (menuPanel != null)
            {
                menuPanel.gameObject.SetActive(false);
            }
        }

        public void Select(int index)
        {
            onItemSelected.Invoke(index);
            Close();
        }

        private void CloseInstant()
        {
            isOpen = false;
            if (menuGroup != null)
            {
                menuGroup.alpha = 0f;
                menuGroup.blocksRaycasts = false;
            }

            if (menuPanel != null) menuPanel.gameObject.SetActive(false);
        }
    }
}
