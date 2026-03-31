using DG.Tweening;
using UIControls.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    public sealed class UIModalControl : MonoBehaviour
    {
        [SerializeField] private bool visible = true;
        [SerializeField] private bool deactivateOnHide = true;
        [SerializeField] private bool blockRaycastsWhenVisible = true;

        [Header("Targets")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Hidden State")]
        [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -120f);
        [SerializeField] private float hiddenScale = 0.92f;

        [Header("Animation")]
        [SerializeField] private UITweenSettings showTween = new UITweenSettings();
        [SerializeField] private UITweenSettings hideTween = new UITweenSettings();

        [Header("Events")]
        [SerializeField] private UnityEvent onShown = new UnityEvent();
        [SerializeField] private UnityEvent onHidden = new UnityEvent();

        private Sequence activeSequence;
        private Vector2 visiblePosition;
        private Vector3 visibleScale;

        public bool Visible => visible;

        private void Awake()
        {
            if (panel == null)
            {
                panel = transform as RectTransform;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (panel != null)
            {
                visiblePosition = panel.anchoredPosition;
                visibleScale = panel.localScale;
            }
            else
            {
                visiblePosition = Vector2.zero;
                visibleScale = Vector3.one;
            }

            if (visible)
            {
                ApplyVisibleInstant();
            }
            else
            {
                ApplyHiddenInstant(deactivateOnHide);
            }
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public void Toggle(bool instant = false)
        {
            SetVisible(!visible, instant);
        }

        public void Show(bool instant = false)
        {
            SetVisible(true, instant);
        }

        public void Hide(bool instant = false)
        {
            SetVisible(false, instant);
        }

        public void SetVisible(bool value, bool instant = false)
        {
            if (visible == value)
            {
                if (instant)
                {
                    if (visible)
                    {
                        ApplyVisibleInstant();
                    }
                    else
                    {
                        ApplyHiddenInstant(deactivateOnHide);
                    }
                }

                return;
            }

            visible = value;

            if (visible)
            {
                PlayShow(instant);
            }
            else
            {
                PlayHide(instant);
            }
        }

        private void PlayShow(bool instant)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            KillTweens();

            var duration = showTween != null ? showTween.Duration : 0f;

            if (instant || duration <= Mathf.Epsilon)
            {
                ApplyVisibleInstant();
                onShown?.Invoke();
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (panel != null)
            {
                panel.anchoredPosition = visiblePosition + hiddenOffset;
                panel.localScale = visibleScale * hiddenScale;
            }

            var sequence = DOTween.Sequence();

            sequence.Join(canvasGroup.DOFade(1f, duration));

            if (panel != null)
            {
                sequence.Join(panel.DOAnchorPos(visiblePosition, duration));
                sequence.Join(panel.DOScale(visibleScale, duration));
            }

            showTween?.Apply(sequence);
            sequence.OnComplete(() =>
            {
                ApplyVisibleInteractionState();
                onShown?.Invoke();
            });

            activeSequence = sequence;
        }

        private void PlayHide(bool instant)
        {
            KillTweens();

            var duration = hideTween != null ? hideTween.Duration : 0f;

            if (instant || duration <= Mathf.Epsilon)
            {
                ApplyHiddenInstant(deactivateOnHide);
                onHidden?.Invoke();
                return;
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var sequence = DOTween.Sequence();

            sequence.Join(canvasGroup.DOFade(0f, duration));

            if (panel != null)
            {
                sequence.Join(panel.DOAnchorPos(visiblePosition + hiddenOffset, duration));
                sequence.Join(panel.DOScale(visibleScale * hiddenScale, duration));
            }

            hideTween?.Apply(sequence);
            sequence.OnComplete(() =>
            {
                if (deactivateOnHide)
                {
                    gameObject.SetActive(false);
                }

                onHidden?.Invoke();
            });

            activeSequence = sequence;
        }

        private void ApplyVisibleInstant()
        {
            if (panel != null)
            {
                panel.anchoredPosition = visiblePosition;
                panel.localScale = visibleScale;
            }

            canvasGroup.alpha = 1f;
            ApplyVisibleInteractionState();
        }

        private void ApplyHiddenInstant(bool deactivate)
        {
            if (panel != null)
            {
                panel.anchoredPosition = visiblePosition + hiddenOffset;
                panel.localScale = visibleScale * hiddenScale;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (deactivate)
            {
                gameObject.SetActive(false);
            }
        }

        private void ApplyVisibleInteractionState()
        {
            canvasGroup.interactable = blockRaycastsWhenVisible;
            canvasGroup.blocksRaycasts = blockRaycastsWhenVisible;
        }

        private void KillTweens()
        {
            if (activeSequence != null && activeSequence.IsActive())
            {
                activeSequence.Kill();
            }

            activeSequence = null;
        }
    }
}
