/*************************************************
Author:                     Cade Naylor
Creation Date:              2/13/2025
Modified Date:              2/23/2025
Summary:                    Spawns saved images onto buildings
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField, Range(0f, 2f), Tooltip("Scale modifier for spawned Drawings")]
    private float scaleModifier;
    [SerializeField, Tooltip("Area between each drawing")]
    private float bufferDistance;

    [SerializeField, Tooltip("The unchanged width of a spawned object, in Unity Units")]
    private float normalWidth;
    [SerializeField, Tooltip("The unchanged width of a spawned object, in Unity Units")]
    private float normalHeight;


    private Vector3 localSpawningPosition;
    private int currentSpawnArea;

    private bool buildingIsFull = false;

    public Sprite testSprite;

    public bool BuildingIsFull { get => buildingIsFull; }

    public void SpawnDrawing(Sprite spawnMe)
    {
        GameObject g = new GameObject();
        g.transform.parent = this.transform;
        g.transform.localPosition = localSpawningPosition;
        localSpawningPosition.x += (normalWidth * scaleModifier) + bufferDistance;
        if (localSpawningPosition.x > validAreas[currentSpawnArea].AreaWidth + validAreas[currentSpawnArea].StartingPosition.x)
        {
            if (localSpawningPosition.y + bufferDistance + (normalHeight * scaleModifier) > validAreas[currentSpawnArea].StartingPosition.y - validAreas[currentSpawnArea].AreaHeight)
            {
                localSpawningPosition.y -= bufferDistance + (normalHeight * scaleModifier);
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
                Destroy(g);
                return;
            }
        }
        g.transform.localScale = new Vector3(scaleModifier, scaleModifier, 1);
        g.AddComponent<SpriteRenderer>();
        g.GetComponent<SpriteRenderer>().sprite = spawnMe;

    }

    // Start is called before the first frame update
    void Start()
    {
        localSpawningPosition = new Vector3(validAreas[0].StartingPosition.x, validAreas[0].StartingPosition.y, -1);
    }

    // Update is called once per frame
    void Update()
    {

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