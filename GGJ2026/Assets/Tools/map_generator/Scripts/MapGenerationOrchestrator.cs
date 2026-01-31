using UnityEngine;

/// <summary>
/// Orchestrates sequential execution of map generation:
/// 1. BiomeGenerator.GenerateMap() - generates base tiles using Perlin noise
/// 2. TilemapManhattanGenerator.GenerateOnTilemap() - places prefabs with Manhattan distance constraints
/// 
/// Both generators must reference the same Tilemap component.
/// Disable autoGenerateOnStart on both BiomeGenerator and TilemapManhattanGenerator to allow this orchestrator control.
/// </summary>
public class MapGenerationOrchestrator : MonoBehaviour
{
    [SerializeField] private BiomeGenerator biomeGenerator;
    [SerializeField] private TilemapManhattanGenerator manhattanGenerator;

    [Header("Execution")]
    [SerializeField] private bool autoGenerateOnStart = true;

    void Awake()
    {
        // Auto-find generators if not assigned
        if (biomeGenerator == null)
        {
            biomeGenerator = FindObjectOfType<BiomeGenerator>();
        }

        if (manhattanGenerator == null)
        {
            manhattanGenerator = FindObjectOfType<TilemapManhattanGenerator>();
        }

        if (biomeGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: BiomeGenerator not found. Please assign it or ensure it exists in the scene.");
        }

        if (manhattanGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: TilemapManhattanGenerator not found. Please assign it or ensure it exists in the scene.");
        }
    }

    void Start()
    {
        if (autoGenerateOnStart)
        {
            GenerateMap();
        }
    }

    /// <summary>
    /// Triggers sequential generation: BiomeGenerator first, then TilemapManhattanGenerator.
    /// </summary>
    public void GenerateMap()
    {
        if (biomeGenerator == null || manhattanGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: Cannot generate map. One or both generators are missing.");
            return;
        }

        Debug.Log("MapGenerationOrchestrator: Starting map generation...");

        // Step 1: Generate base tiles
        Debug.Log("MapGenerationOrchestrator: Running BiomeGenerator.GenerateMap()");
        biomeGenerator.GenerateMap();

        // Step 2: Place prefabs on generated tiles
        Debug.Log("MapGenerationOrchestrator: Running TilemapManhattanGenerator.GenerateOnTilemap()");
        manhattanGenerator.GenerateOnTilemap();

        Debug.Log("MapGenerationOrchestrator: Map generation complete!");
    }

    /// <summary>
    /// Re-runs only the biome generation (tile placement).
    /// Useful for regenerating with a new seed without re-running the Manhattan placement.
    /// </summary>
    public void RegenerateBiome()
    {
        if (biomeGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: Cannot regenerate biome. BiomeGenerator is missing.");
            return;
        }

        Debug.Log("MapGenerationOrchestrator: Regenerating biome...");
        biomeGenerator.GenerateMap();
    }

    /// <summary>
    /// Re-runs only the Manhattan distance placement.
    /// Useful for re-placing prefabs with different constraints without regenerating tiles.
    /// </summary>
    public void RegeneratePlacement()
    {
        if (manhattanGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: Cannot regenerate placement. TilemapManhattanGenerator is missing.");
            return;
        }

        Debug.Log("MapGenerationOrchestrator: Regenerating placement...");
        manhattanGenerator.GenerateOnTilemap();
    }
}
