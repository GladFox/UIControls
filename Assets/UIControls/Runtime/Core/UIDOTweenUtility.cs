using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIControls.Runtime.Core
{
    public static class UIDOTweenUtility
    {
        public static Tweener TweenCanvasGroupAlpha(CanvasGroup target, float endValue, float duration)
        {
            if (target == null)
            {
                return null;
            }

            return DOTween.To(() => target.alpha, value => target.alpha = value, endValue, duration)
                .SetTarget(target);
        }

        public static Tweener TweenGraphicColor(Graphic target, Color endValue, float duration)
        {
            if (target == null)
            {
                return null;
            }

            return DOTween.To(() => target.color, value => target.color = value, endValue, duration)
                .SetTarget(target);
        }

        public static Tweener TweenAnchoredPosition(RectTransform target, Vector2 endValue, float duration, bool snapping = false)
        {
            if (target == null)
            {
                return null;
            }

            return DOTween.To(() => target.anchoredPosition, value => target.anchoredPosition = value, endValue, duration)
                .SetOptions(snapping)
                .SetTarget(target);
        }

        public static Tweener TweenSizeDelta(RectTransform target, Vector2 endValue, float duration, bool snapping = false)
        {
            if (target == null)
            {
                return null;
            }

            return DOTween.To(() => target.sizeDelta, value => target.sizeDelta = value, endValue, duration)
                .SetOptions(snapping)
                .SetTarget(target);
        }
    }
}
