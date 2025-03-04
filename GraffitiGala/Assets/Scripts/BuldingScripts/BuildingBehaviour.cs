/*************************************************
Author:                     Cade Naylor
Creation Date:              2/13/2025
Modified Date:              3/3/2025
Summary:                    Spawns saved images onto buildings
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.City
{
    public class BuildingBehavior : MonoBehaviour
    {
        [field: SerializeField, Tooltip("The sprite renderer of this building.")]
        public SpriteRenderer Rend { get; private set; }
        [field: SerializeField, Tooltip("Building Width, in Unity Units")]
        public float BuildingWidth { get; private set; }
        [field: SerializeField, Tooltip("Building Height, in Unity Units")]
        public float BuildingHeight { get; private set; }
        [SerializeField, Tooltip("All rectangles on the building that drawings can appear in")]
        private List<DrawingArea> validAreas = new List<DrawingArea>();
        //[SerializeField, Range(0f, 2f), Tooltip("Scale modifier for spawned Drawings")]
        //private float scaleModifier;
        //[SerializeField, Tooltip("Area between each drawing")]
        //private float bufferDistance;
        [SerializeField] private GraffitiSettings graffitiSettings;

        //[SerializeField, Tooltip("The unchanged width of a spawned object, in Unity Units")]
        //private float normalWidth;
        //[SerializeField, Tooltip("The unchanged width of a spawned object, in Unity Units")]
        //private float normalHeight;


        // The top left corner of where the sprite will spawn.
        private Vector3 localSpawningPosition;
        private int currentSpawnArea;

        private bool buildingIsFull = false;

        public Sprite testSprite;

        public bool BuildingIsFull { get => buildingIsFull; }

        public void SpawnDrawing(Sprite spawnMe)
        {
            GameObject g = new GameObject();
            g.transform.parent = this.transform;
            // Use the bounds of the sprite inherently so that the code has an accurate size of sprites.
            float width = spawnMe.bounds.size.x * graffitiSettings.ScaleModifier;
            float height = spawnMe.bounds.size.y * graffitiSettings.ScaleModifier;
            g.transform.localPosition = localSpawningPosition;
            localSpawningPosition.x += width + graffitiSettings.BufferDistance;
            if (localSpawningPosition.x > validAreas[currentSpawnArea].AreaWidth + validAreas[currentSpawnArea].StartingPosition.x)
            {
                if (localSpawningPosition.y - (graffitiSettings.BufferDistance + height) > 
                    validAreas[currentSpawnArea].StartingPosition.y - validAreas[currentSpawnArea].AreaHeight)
                {
                    localSpawningPosition.y -= graffitiSettings.BufferDistance + height;
                    localSpawningPosition.x = validAreas[currentSpawnArea].StartingPosition.x;
                }
                else if (currentSpawnArea < validAreas.Count - 1)
                {
                    currentSpawnArea++;
                    localSpawningPosition.x = validAreas[currentSpawnArea].StartingPosition.x;
                    localSpawningPosition.y = validAreas[currentSpawnArea].StartingPosition.y;
                }
                else
                {
                    Debug.Log("Building is full");
                    buildingIsFull = true;
                    // Returning here was preventing the graffiti that takes up the last space on the building to be destroyed.
                    // Because this is predicting where the next spawn point will be, we should let the graffiti spawn normally
                    // so it isnt lost.
                    //Destroy(g);
                    //return;
                }
            }
            g.transform.localScale = new Vector3(graffitiSettings.ScaleModifier, graffitiSettings.ScaleModifier, 1);
            SpriteRenderer sRend = g.AddComponent<SpriteRenderer>();
            sRend.sprite = spawnMe;
        }

        // Switched this to Awake so that it gets initialized when the prefab is spawned.
        void Awake()
        {
            localSpawningPosition = new Vector3(validAreas[0].StartingPosition.x, validAreas[0].StartingPosition.y, -1);
        }
    }

    [System.Serializable]
    public class DrawingArea
    {
        [SerializeField, Tooltip("The upper left corner of the building area")]
        private Vector2 startingPosition;
        [SerializeField, Tooltip("The width of the building area, in Unity Units")]
        private float areaWidth;
        [SerializeField, Tooltip("The height of the building area, in Unity Units")]
        private float areaHeight;

        public Vector2 StartingPosition { get => startingPosition; set => startingPosition = value; }
        public float AreaWidth { get => areaWidth; set => areaWidth = value; }
        public float AreaHeight { get => areaHeight; set => areaHeight = value; }
    }
}