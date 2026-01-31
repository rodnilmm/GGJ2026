using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeGenerator : MonoBehaviour {
    [SerializeField] private Tilemap tilemap;
    public TileBase rockTile, sandTile;
    public int width = 100, height = 100;

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
                tilemap = FindObjectOfType<Tilemap>();
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
                // Calculate Perlin noise coordinate
                float xCoord = (float)x / width * scale + seed;
                float yCoord = (float)y / height * scale + seed;
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

                // Assign tiles based on noise thresholds
                if (noiseValue < 0.8f) 
                    tilemap.SetTile(new Vector3Int(x, y, 0), sandTile);
                else 
                    tilemap.SetTile(new Vector3Int(x, y, 0), rockTile);
            }
        }
    }
}