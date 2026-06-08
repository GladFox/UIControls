using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A single expandable node of a tree view. Clicking the row toggles its children container
    /// (driven by layout groups, so the tree reflows) and rotates the chevron. Compose a tree by
    /// nesting <see cref="UITreeNodeControl"/>s inside each other's children containers; leaves simply
    /// have no children. Raises <see cref="OnToggled"/> with the new expanded state.
    /// </summary>
    public sealed class UITreeNodeControl : MonoBehaviour
    {
        [System.Serializable]
        public sealed class BoolEvent : UnityEvent<bool>
        {
        }

        [Header("Targets")]
        [SerializeField] private UIButtonControl rowButton;
        [SerializeField] private GameObject childrenContainer;
        [SerializeField] private RectTransform chevron;

        [Header("State")]
        [SerializeField] private bool expanded = true;
        [SerializeField] private float chevronCollapsedAngle;
        [SerializeField] private float chevronExpandedAngle = -90f;
        [SerializeField] private float duration = 0.18f;

        [Header("Events")]
        [SerializeField] private BoolEvent onToggled = new BoolEvent();

        public BoolEvent OnToggled => onToggled;
        public bool Expanded => expanded;
        public bool IsLeaf => childrenContainer == null;

        private void Awake()
        {
            if (rowButton != null) rowButton.OnClick.AddListener(Toggle);
        }

        private void OnEnable()
        {
            Apply(false);
        }

        public void Toggle()
        {
            if (IsLeaf)
            {
                return;
            }

            expanded = !expanded;
            Apply(true);
            onToggled.Invoke(expanded);
        }

        public void SetExpanded(bool value, bool animate = true)
        {
            if (IsLeaf || value == expanded)
            {
                return;
            }

            expanded = value;
            Apply(animate);
        }

        private void Apply(bool animate)
        {
            if (childrenContainer != null)
            {
                childrenContainer.SetActive(expanded);
            }

            if (chevron != null)
            {
                var angle = new Vector3(0f, 0f, expanded ? chevronExpandedAngle : chevronCollapsedAngle);
                if (animate)
                {
                    chevron.DOLocalRotate(angle, duration).SetUpdate(true);
                }
                else
                {
                    chevron.localEulerAngles = angle;
                }
            }
        }
    }
}
