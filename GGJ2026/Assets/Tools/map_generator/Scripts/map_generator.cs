using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeGenerator : MonoBehaviour {
    [SerializeField] private Tilemap tilemap;
    public TileBase rockTile, rockTilebroken, rockTileCrab, rockTileOnk, rockTileEye, sandTile, sandTile2;
    public int width = 100, height = 100;
    public Transform player1InitPosition, player2InitPosition, player3InitPosition, player4InitPosition;

    [Header("Generation Settings")]
    public float scale = 10f; // Lower = larger zones, Higher = more scattered
    public float seed; // Change this to get a new layout

    [Header("Execution")]
    [SerializeField] private bool autoGenerateOnStart = false; // Set to false to allow orchestrator control

    void Awake()
    {
        // Auto-find Tilemap if not assigned
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = FindFirstObjectByType<Tilemap>();
            }
            if (tilemap == null)
            {
                Debug.LogError("BiomeGenerator: Tilemap not assigned and none found in scene. Please assign a Tilemap or add one to the scene.");
            }
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
        if (tilemap == null)
        {
            Debug.LogError("BiomeGenerator: Cannot generate map without a Tilemap assigned.");
            return;
        }

        if (seed == 0)
        {
            seed = Random.Range(0f, 10000f);
        }

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if( isPlayerPosition()){
                    continue; // Skip placing tiles where players start
                }

                // Calculate Perlin noise coordinate
                float xCoord = (float)x / width * scale + seed;
                float yCoord = (float)y / height * scale + seed;
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

                // Assign tiles based on noise thresholds
                if (noiseValue < 0.4f) 
                    tilemap.SetTile(new Vector3Int(x, y, 0), sandTile);
                else if (noiseValue < 0.8f)
                    tilemap.SetTile(new Vector3Int(x, y, 0), sandTile2);
                else if (noiseValue < 0.84f)
                    tilemap.SetTile(new Vector3Int(x, y, 0), rockTileEye);
                else if (noiseValue < 0.88f)
                    tilemap.SetTile(new Vector3Int(x, y, 0), rockTileOnk);
                else if (noiseValue < 0.92f)
                    tilemap.SetTile(new Vector3Int(x, y, 0), rockTileCrab);
                else if (noiseValue < 0.96f)
                    tilemap.SetTile(new Vector3Int(x, y, 0), rockTilebroken);
                else 
                    tilemap.SetTile(new Vector3Int(x, y, 0), rockTile);
            }
        }
    }

    private bool isPlayerPosition(){
        // Check if any player init position is within the tilemap bounds
        Vector3Int pos1 = tilemap.WorldToCell(player1InitPosition.position);
        Vector3Int pos2 = tilemap.WorldToCell(player2InitPosition.position);
        Vector3Int pos3 = tilemap.WorldToCell(player3InitPosition.position);
        Vector3Int pos4 = tilemap.WorldToCell(player4InitPosition.position);

        BoundsInt bounds = tilemap.cellBounds;

        return bounds.Contains(pos1) || bounds.Contains(pos2) || bounds.Contains(pos3) || bounds.Contains(pos4);
    }
}