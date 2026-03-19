using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeGenerator : MonoBehaviour {
    [SerializeField] private Transform parentContainer; // Container for spawned objects
    public GameObject[] rockTilePrefabs, sandTilePrefabs;
    private int width, height;
    public Transform player1InitPosition, player2InitPosition, player3InitPosition, player4InitPosition, objectivePosition;
    public float cellSize = 1f; // Size of each cell in world units

    [Header("Generation Settings")]
    public float scale = 4f; // Lower = larger zones, Higher = more scattered
    public float seed; // Change this to get a new layout
    public float zOffset = 2.0f; // Height above the container to spawn objects

    [Header("Execution")]
    [SerializeField] private bool autoGenerateOnStart = false; // Set to false to allow orchestrator control

    void Awake()
    {
        // Auto-use this transform as parent container if not assigned
        if (parentContainer == null)
        {
            parentContainer = transform;
        }
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
        Debug.Log("Initial player positions: P1 " + player1InitPosition.position + " P2 " + player2InitPosition.position +
                  " P3 " + player3InitPosition.position + " P4 " + player4InitPosition.position +
                  " Objective " + objectivePosition.position);

        Tilemap tilemap = parentContainer.GetComponent<Tilemap>();

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
            Collider col = parentContainer.GetComponent<Collider>();
            worldMin = col != null ? col.bounds.min : parentContainer.position;
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

                if (IsPlayerPosition(cellWorldPos) || IsObjectivePosition(cellWorldPos)){
                    Debug.Log("Skipping player/objective position at: " + cellWorldPos);
                    continue;
                }

                // Normalize noise coordinates to [0, scale] over the full bounds
                float xCoord = (float)(x - cellBounds.xMin) / width * scale + seed;
                float yCoord = (float)(y - cellBounds.yMin) / height * scale + seed;
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

                GameObject prefabToSpawn = null;
                if (noiseValue < 0.55f)
                    prefabToSpawn = null;
                else if (noiseValue < 0.8f)
                    prefabToSpawn = GetRandomPrefab(sandTilePrefabs);
                else
                    prefabToSpawn = GetRandomPrefab(rockTilePrefabs);

                if (prefabToSpawn != null)
                {
                    GameObject spawnedObj = Instantiate(prefabToSpawn, cellWorldPos, Quaternion.identity, parentContainer);
                    spawnedObj.name = $"{prefabToSpawn.name}_{x}_{y}";

                    SpriteRenderer spriteRenderer = spawnedObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = 1;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates grid dimensions from the parent container's bounds.
    /// Works with Tilemap, RectTransform (UI) or 3D Collider bounds.
    /// </summary>
    private void CalculateGridDimensionsFromContainer()
    {
        if (parentContainer == null) return;

        // Try to get Tilemap first
        Tilemap tilemap = parentContainer.GetComponent<Tilemap>();
        if (tilemap != null)
        {
            BoundsInt cellBounds = tilemap.cellBounds;
            width = cellBounds.size.x;
            height = cellBounds.size.y;
            Debug.Log("MapGeneration from tilemap width: " + width + " height: " + height);
            return;
        }

        // Try to get RectTransform if it's a UI element
        RectTransform rectTransform = parentContainer.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            width = Mathf.RoundToInt(rectTransform.rect.width / cellSize);
            height = Mathf.RoundToInt(rectTransform.rect.height / cellSize);
            return;
        }

        // Try to get bounds from a 3D collider
        Collider collider = parentContainer.GetComponent<Collider>();
        if (collider != null)
        {
            Bounds bounds = collider.bounds;
            width = Mathf.RoundToInt(bounds.size.x / cellSize);
            height = Mathf.RoundToInt(bounds.size.y / cellSize);
            return;
        }

        // If no collider or RectTransform, try to calculate from child bounds
        if (parentContainer.childCount > 0)
        {
            Bounds childBounds = GetChildrenBounds(parentContainer);
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
    /// Gets a random prefab from the provided array.
    /// </summary>
    private GameObject GetRandomPrefab(GameObject[] prefabArray)
    {
        if (prefabArray == null || prefabArray.Length == 0)
            return null;
        return prefabArray[Random.Range(0, prefabArray.Length)];
    }

    /// <summary>
    /// Returns true if the cell falls within the actual shape of the container,
    /// handling irregular forms (tilemap with holes, non-rectangular colliders).
    /// </summary>
    private bool IsWithinContainerBounds(Vector3Int cellPos, Vector3 worldPos)
    {
        // Tilemap: only generate on cells that have a tile painted (respects holes and irregular shapes)
        Tilemap tilemap = parentContainer.GetComponent<Tilemap>();
        if (tilemap != null)
            return tilemap.HasTile(cellPos);

        // 2D collider: OverlapPoint correctly handles circles, polygons, etc.
        Collider2D col2D = parentContainer.GetComponent<Collider2D>();
        if (col2D != null)
            return col2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));

        // 3D collider: bounds.Contains is accurate for box colliders; approximation for others
        Collider col = parentContainer.GetComponent<Collider>();
        if (col != null)
            return col.bounds.Contains(worldPos);

        return true;
    }

    /// <summary>
    /// Check if a cell position is occupied by a player spawn point (with buffer zone).
    /// </summary>
    private bool IsPlayerPosition(Vector3 cellWorldPos)
    {
        float checkRadius = cellSize * 1f;
        return Vector3.Distance(new Vector3(cellWorldPos.x, cellWorldPos.y, 0), player1InitPosition.position) < checkRadius ||
               Vector3.Distance(new Vector3(cellWorldPos.x, cellWorldPos.y, 0), player2InitPosition.position) < checkRadius ||
               Vector3.Distance(new Vector3(cellWorldPos.x, cellWorldPos.y, 0), player3InitPosition.position) < checkRadius ||
               Vector3.Distance(new Vector3(cellWorldPos.x, cellWorldPos.y, 0), player4InitPosition.position) < checkRadius;

    }

    /// <summary>
    /// Check if a cell position is occupied by the objective spawn point (with buffer zone).
    /// </summary>
    private bool IsObjectivePosition(Vector3 cellWorldPos)
    {
        float checkRadius = cellSize * 1f;
        return Vector3.Distance(new Vector3(cellWorldPos.x, cellWorldPos.y, 0), objectivePosition.position) < checkRadius;

    }
}