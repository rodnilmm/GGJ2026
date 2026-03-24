using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeGenerator : MonoBehaviour {
    [SerializeField] private Transform mapBounds;
    public GameObject[] rockTile, sandTile;
    private int width, height;
    public float cellSize = 1f; // Size of each cell in world units

    [Header("Generation Settings")]
    public float scale = 5f; // Lower = larger zones, Higher = more scattered
    public float seed; // Change this to get a new layout
    public float zOffset = 3.0f; // Height above the container to spawn objects

    [Header("Execution")]
    [SerializeField] private bool autoGenerateOnStart = false; // Set to false to allow orchestrator control

    private List<Vector3Int> objectiveMaskCells = new List<Vector3Int>();

    public void SetMapBounds(Transform bounds)
    {
        mapBounds = bounds;
    }

    public void SetObjectiveMaskCells(List<Vector3Int> cells)
    {
        objectiveMaskCells = cells;
    }

    void Start() {
        if (autoGenerateOnStart)
        {
            seed = Random.Range(0f, 10000f);
            GenerateMap();
        }
    }

    /// <summary>
    /// Generates the biome map using Perlin noise. Should be called before mask generator.
    /// </summary>
    public void GenerateMap() {
        CalculateGridDimensionsFromContainer();

        Tilemap tilemap = mapBounds.GetComponent<Tilemap>();

        if (seed == 0)
        {
            seed = Random.Range(0f, 10000f);
        }

        // Resolve iteration bounds and world origin from the container
        BoundsInt cellBounds;
        Vector3 worldMin;
        if (tilemap != null)
        {
            cellBounds = tilemap.cellBounds;
            worldMin = Vector3.zero; // unused; tilemap.GetCellCenterWorld handles positioning
        }
        else
        {
            Collider col = mapBounds.GetComponent<Collider>();
            worldMin = col != null ? col.bounds.min : mapBounds.position;
            cellBounds = new BoundsInt(0, 0, 0, width, height, 1);
        }

        for (int x = cellBounds.xMin; x < cellBounds.xMax; x++) {
            for (int y = cellBounds.yMin; y < cellBounds.yMax; y++) {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector3 cellWorldPos;

                if (tilemap != null)
                {
                    cellWorldPos = tilemap.GetCellCenterWorld(cellPos);
                    cellWorldPos.z += zOffset;
                }
                else
                {
                    int lx = x - cellBounds.xMin;
                    int ly = y - cellBounds.yMin;
                    cellWorldPos = new Vector3(
                        worldMin.x + (lx + 0.5f) * cellSize,
                        worldMin.y + (ly + 0.5f) * cellSize,
                        worldMin.z + zOffset
                    );
                }

                if (!IsWithinContainerBounds(cellPos, cellWorldPos))
                    continue;

                if (objectiveMaskCells.Contains(cellPos))
                    continue;

                // Normalize noise coordinates to [0, scale] over the full bounds
                float xCoord = (float)(x - cellBounds.xMin) / width * scale + seed;
                float yCoord = (float)(y - cellBounds.yMin) / height * scale + seed;
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

                GameObject prefabToSpawn = null;
                if (noiseValue < 0.55f)
                    prefabToSpawn = null;
                else if (noiseValue < 0.8f)
                    prefabToSpawn = GetRandomPrefab(sandTile);
                else
                    prefabToSpawn = GetRandomPrefab(rockTile);

                if (prefabToSpawn != null)
                {
                    GameObject spawnedObj = Instantiate(prefabToSpawn, cellWorldPos, Quaternion.identity, mapBounds);
                    spawnedObj.name = $"{prefabToSpawn.name}_{x}_{y}";

                    SpriteRenderer spriteRenderer = spawnedObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = 2;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates grid dimensions from the map bounds transform.
    /// Works with Tilemap, RectTransform (UI) or 3D Collider bounds.
    /// </summary>
    private void CalculateGridDimensionsFromContainer()
    {
        if (mapBounds == null) return;

        // Try to get Tilemap first
        Tilemap tilemap = mapBounds.GetComponent<Tilemap>();
        if (tilemap != null)
        {
            BoundsInt cellBounds = tilemap.cellBounds;
            width = cellBounds.size.x;
            height = cellBounds.size.y;
            Debug.Log("MapGeneration from tilemap width: " + width + " height: " + height);
            return;
        }

        // Try to get RectTransform if it's a UI element
        RectTransform rectTransform = mapBounds.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            width = Mathf.RoundToInt(rectTransform.rect.width / cellSize);
            height = Mathf.RoundToInt(rectTransform.rect.height / cellSize);
            return;
        }

        // Try to get bounds from a 3D collider
        Collider collider = mapBounds.GetComponent<Collider>();
        if (collider != null)
        {
            Bounds bounds = collider.bounds;
            width = Mathf.RoundToInt(bounds.size.x / cellSize);
            height = Mathf.RoundToInt(bounds.size.y / cellSize);
            return;
        }

        // If no collider or RectTransform, try to calculate from child bounds
        if (mapBounds.childCount > 0)
        {
            Bounds childBounds = GetChildrenBounds(mapBounds);
            width = Mathf.RoundToInt(childBounds.size.x / cellSize);
            height = Mathf.RoundToInt(childBounds.size.y / cellSize);
        }

    }

    /// <summary>
    /// Gets the combined bounds of all children under a transform.
    /// </summary>
    private Bounds GetChildrenBounds(Transform parent)
    {
        Bounds bounds = new Bounds(parent.position, Vector3.zero);
        foreach (Renderer renderer in parent.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    /// <summary>
    /// Spawns a random sandTile prefab at each of the provided world positions.
    /// Call this after the objective mask has been placed to mark its location.
    /// </summary>
    public void SpawnSandTilesAtPositions(IReadOnlyList<Vector3> positions)
    {
        if (sandTile == null || sandTile.Length == 0)
        {
            Debug.LogWarning("BiomeGenerator: No sandTile prefabs assigned.");
            return;
        }

        foreach (Vector3 pos in positions)
        {
            GameObject prefab = GetRandomPrefab(sandTile);
            GameObject spawnedObj = Instantiate(prefab, pos, Quaternion.identity, mapBounds);
            spawnedObj.name = $"{prefab.name}_objective_{pos.x}_{pos.y}";

            SpriteRenderer spriteRenderer = spawnedObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer is not null)
            {
                spriteRenderer.sortingOrder = 2;
            }
        }
    }

    /// <summary>
    /// Gets a random prefab from the provided array.
    /// </summary>
    private GameObject GetRandomPrefab(GameObject[] prefabArray)
    {
        if (prefabArray == null || prefabArray.Length == 0)
            return null;
        return prefabArray[Random.Range(0, prefabArray.Length)];
    }

    /// <summary>
    /// Returns true if the cell falls within the actual shape of the map bounds,
    /// handling irregular forms (tilemap with holes, non-rectangular colliders).
    /// </summary>
    private bool IsWithinContainerBounds(Vector3Int cellPos, Vector3 worldPos)
    {
        // Tilemap: only generate on cells that have a tile painted (respects holes and irregular shapes)
        Tilemap tilemap = mapBounds.GetComponent<Tilemap>();
        if (tilemap != null)
            return tilemap.HasTile(cellPos);

        // 2D collider: OverlapPoint correctly handles circles, polygons, etc.
        Collider2D col2D = mapBounds.GetComponent<Collider2D>();
        if (col2D != null)
            return col2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));

        // 3D collider: bounds.Contains is accurate for box colliders; approximation for others
        Collider col = mapBounds.GetComponent<Collider>();
        if (col != null)
            return col.bounds.Contains(worldPos);

        return true;
    }

}
