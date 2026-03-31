using System;
using UnityEngine;

namespace UIControls.Runtime.Core
{
    [Serializable]
    public sealed class UIStateVisual
    {
        [Min(0f)]
        public float scale = 1f;

        [Range(0f, 1f)]
        public float alpha = 1f;

        public Color color = Color.white;

        public UITweenSettings tween = new UITweenSettings();
    }
}
