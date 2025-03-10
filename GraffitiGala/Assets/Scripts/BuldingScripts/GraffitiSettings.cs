/*************************************************
Brandon Koederitz
3/3/2025
3/3/2025
Settings for how graffiti spawns on buildings gathered in an independent object for easier modifications.
***************************************************/
using UnityEngine;

namespace GraffitiGala.City
{
    [CreateAssetMenu(fileName = "GraffitiSettings", menuName = "Graffiti Gala/Graffiti Settings")]
    public class GraffitiSettings : ScriptableObject
    {
        [field: Header("Sprite Settings")]
        [field: SerializeField, Tooltip("The normalized position of graffiti sprite's pivot points.")]
        public Vector2 PivotPoint { get; private set; } = new Vector2(0.5f, 0.5f);
        [field: SerializeField]
        public float PixelsPerUnit { get; private set; } = 256f;
        [field: SerializeField]
        public uint Extrude { get; private set; } = 0;
        [field: SerializeField]
        public SpriteMeshType MeshType { get; private set; }
        [field: SerializeField]
        public FilterMode SpriteFilterMode { get; private set; }

        [field: Header("Graffiti Display Settings")]
        [field: SerializeField, Range(0f, 2f), Tooltip("Scale modifier for spawned Drawings")]
        public float ScaleModifier { get; private set; }
        [field: SerializeField, Tooltip("Area between each drawing")]
        public float BufferDistance { get; private set; }
        [field: SerializeField, Tooltip("Area between each drawing")]
        public Material OutlineMaterial { get; private set; }
    }
}