using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates sequential execution of map generation:
/// 1. BiomeGenerator.GenerateMap() - generates base tiles using Perlin noise
/// 2. TilemapManhattanGenerator.GenerateOnTilemap() - places masks with Manhattan distance constraints
/// 
/// Both generators must reference the same Tilemap component.
/// Disable autoGenerateOnStart on both BiomeGenerator and TilemapManhattanGenerator to allow this orchestrator control.
/// </summary>
public class MapGenerationOrchestrator : MonoBehaviour
{
    [SerializeField] private BiomeGenerator biomeGenerator;
    [SerializeField] private TilemapManhattanGenerator commonMaskGenerator;
    [SerializeField] private TilemapManhattanGenerator objectiveMaskGenerator;
    [SerializeField] private Transform mapBounds;
    [SerializeField] private Transform objectiveAreaBounds;
    [SerializeField] private GameObject[] commonMasks;
    [SerializeField] private int commonMaskCount = 4;
    [SerializeField] private GameObject[] objectiveMasks;
    [SerializeField] private int objectiveMaskCount = 1;

    [Header("Execution")]
    [SerializeField] private bool autoGenerateOnStart = true;

    private readonly List<Vector3Int> sharedOccupiedCells = new List<Vector3Int>();

    void Awake()
    {
        // Auto-find generators if not assigned
        if (biomeGenerator == null)
        {
            biomeGenerator = FindFirstObjectByType<BiomeGenerator>();
        }
        if (biomeGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: BiomeGenerator not found. Please assign it or ensure it exists in the scene.");
        }

        if (objectiveMaskGenerator == null)
        {
            objectiveMaskGenerator = FindFirstObjectByType<TilemapManhattanGenerator>();
        }
        if (objectiveMaskGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: ObjectiveMaskGenerator not found. Please assign it or ensure it exists in the scene.");
        }

        if (commonMaskGenerator == null)
        {
            commonMaskGenerator = FindFirstObjectByType<TilemapManhattanGenerator>();
        }
        if (commonMaskGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: CommonMaskGenerator not found. Please assign it or ensure it exists in the scene.");
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
    /// Triggers sequential generation: ObjectiveMaskGenerator first, then TilemapManhattanGenerator, then BiomeGenerator.
    /// </summary>
    public void GenerateMap()
    {
        if (biomeGenerator == null || commonMaskGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: Cannot generate map. One or both generators are missing.");
            return;
        }

        Debug.Log("MapGenerationOrchestrator: Starting map generation...");

        sharedOccupiedCells.Clear();

        // Step 1: Place objective mask on generated tiles
        Debug.Log("MapGenerationOrchestrator: Running TilemapManhattanGenerator.GenerateOnTilemap() for objective masks");
        objectiveMaskGenerator.SetMapBounds(objectiveAreaBounds);
        objectiveMaskGenerator.SetMasks(objectiveMasks);
        objectiveMaskGenerator.SetMaskCount(objectiveMaskCount);
        objectiveMaskGenerator.SetOccupiedCells(sharedOccupiedCells);
        objectiveMaskGenerator.GenerateOnTilemap();
        Debug.Log("MapGenerationOrchestrator: sharedOccupiedCells after objective mask placement: " + sharedOccupiedCells.Count);

        // Step 1b: Pass objective mask cells to BiomeGenerator so those cells are skipped during generation,
        // then spawn a sand tile at each objective mask position
        biomeGenerator.SetMapBounds(mapBounds);
        biomeGenerator.SetObjectiveMaskCells(new List<Vector3Int>(sharedOccupiedCells));
        biomeGenerator.SpawnSandTilesAtPositions(objectiveMaskGenerator.GetGeneratedPositions());

        // Step 2: Place common masks on generated tiles
        Debug.Log("MapGenerationOrchestrator: Running TilemapManhattanGenerator.GenerateOnTilemap()");
        commonMaskGenerator.SetMapBounds(mapBounds);
        commonMaskGenerator.SetMasks(commonMasks);
        commonMaskGenerator.SetMaskCount(commonMaskCount);
        commonMaskGenerator.SetOccupiedCells(sharedOccupiedCells);
        commonMaskGenerator.GenerateOnTilemap();
        Debug.Log("MapGenerationOrchestrator: sharedOccupiedCells after common mask placement: " + sharedOccupiedCells.Count);

        // Step 3: Generate base tiles
        Debug.Log("MapGenerationOrchestrator: Running BiomeGenerator.GenerateMap()");
        biomeGenerator.GenerateMap();

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
    /// Useful for re-placing masks with different constraints without regenerating tiles.
    /// </summary>
    public void RegeneratePlacement()
    {
        if (commonMaskGenerator == null)
        {
            Debug.LogError("MapGenerationOrchestrator: Cannot regenerate placement. TilemapManhattanGenerator is missing.");
            return;
        }

        Debug.Log("MapGenerationOrchestrator: Regenerating placement...");
        commonMaskGenerator.GenerateOnTilemap();
    }
}
