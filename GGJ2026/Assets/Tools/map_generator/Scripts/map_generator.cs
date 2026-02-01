using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeGenerator : MonoBehaviour {
    [SerializeField] private Transform parentContainer; // Container for spawned objects
    public GameObject[] rockTilePrefabs, sandTilePrefabs;
    public int width = 49, height = 26;
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
        // Get tilemap reference for proper cell-to-world conversion
        Tilemap tilemap = parentContainer.GetComponent<Tilemap>();
        
        if (seed == 0)
        {
            seed = Random.Range(0f, 10000f);
        }

        for (int x = 0; x < width-1; x++) {
            for (int y = 1; y < height; y++) {
                // Use tilemap's cell-to-world conversion for proper alignment
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector3 cellWorldPos;
                
                if (tilemap != null)
                {
                    cellWorldPos = tilemap.GetCellCenterWorld(cellPos);
                    cellWorldPos.z = cellWorldPos.z + zOffset;
                }
                else
                {
                    Vector3 containerPos = parentContainer.position;
                    cellWorldPos = new Vector3(
                        containerPos.x + x * cellSize, 
                        containerPos.y + y * cellSize, 
                        containerPos.z + zOffset
                    );
                }
                
                if (IsPlayerPosition(cellWorldPos) || IsObjectivePosition(cellWorldPos)){
                    Debug.Log("Skipping player/objective position at: " + cellWorldPos);
                    continue; // Skip placing objects where players start or objective is
                }

                // Calculate Perlin noise coordinate
                float xCoord = (float)x / width * scale + seed;
                float yCoord = (float)y / height * scale + seed;
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

                // Spawn prefabs based on noise thresholds
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
                    
                    // Set sorting order
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