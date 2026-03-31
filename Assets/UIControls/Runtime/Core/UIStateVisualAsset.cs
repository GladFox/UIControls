using UnityEngine;

namespace UIControls.Runtime.Core
{
    [CreateAssetMenu(menuName = "UIControls/Visual State", fileName = "UIStateVisual")]
    public sealed class UIStateVisualAsset : ScriptableObject
    {
        [SerializeField] private UIStateVisual state = new UIStateVisual();

        public UIStateVisual State => state;
    }
}
