/*************************************************
Brandon Koederitz
3/6/2025
3/6/2025
Set of information for visualizing a sprite tween.
***************************************************/

using UnityEngine;

namespace GraffitiGala.City
{
    [CreateAssetMenu(fileName = "SpriteVisualizerSettings", menuName = "Graffiti Gala/Sprite Visualizer Settings")]
    public class SpriteVisualizerSettings : ScriptableObject
    {
        [field: Header("Sprite Settings")]
        [field: SerializeField, Tooltip("The scale of the sprite visualization in comparison to it's base scale")] 
        public float SpriteMagnification { get; private set; }
        [field: SerializeField, Tooltip("The name of the sorting layer that this sprite should render on.")]
        public string SortingLayerName { get; private set; }
        [field: SerializeField]
        public int OrderInLayer { get; private set; }
        [field: SerializeField]
        public Sprite BackgroundSprite { get; private set; }
        [field: Header("Animation Settings")]
        [field: SerializeField, Tooltip("The amount of time the sprite tween should take.")]
        public float TweenTime { get; private set; }
        [field: SerializeField, Tooltip("The amount of time the sprite visual should wait before tweening.")]
        public float InitialDelay { get; private set; }
        [field: SerializeField, Tooltip("The amount of time that the sprite should initially fade in for.")]
        public float FadeInDuration { get; private set; }
        [field: SerializeField, Tooltip("The curve that controls the sprite's tween animation.")]
        public AnimationCurve TweenCurve { get; private set; }
    }
}