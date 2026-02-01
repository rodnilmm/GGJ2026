using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Test script to verify sequential map generation works correctly.
/// Place this on a test object in GameLevel.unity to run validation.
/// </summary>
public class MapGenerationTest : MonoBehaviour
{
    [SerializeField] private MapGenerationOrchestrator orchestrator;
    [SerializeField] private Tilemap tilemap;

    [Header("Test Configuration")]
    [SerializeField] private bool runTestOnStart = true;

    void Awake()
    {
        if (orchestrator == null)
        {
            orchestrator = FindFirstObjectByType<MapGenerationOrchestrator>();
        }

        if (tilemap == null)
        {
            tilemap = FindFirstObjectByType<Tilemap>();
        }
    }

    void Start()
    {
        if (runTestOnStart)
        {
            RunTest();
        }
    }

    /// <summary>
    /// Validates that the orchestrator successfully runs both generators and populates the tilemap.
    /// </summary>
    public void RunTest()
    {
        Debug.Log("=== MAP GENERATION TEST START ===");

        // Verify orchestrator exists
        if (orchestrator == null)
        {
            Debug.LogError("TEST FAILED: MapGenerationOrchestrator not found in scene.");
            return;
        }
        Debug.Log("✓ MapGenerationOrchestrator found");

        // Verify tilemap exists
        if (tilemap == null)
        {
            Debug.LogError("TEST FAILED: Tilemap not found in scene.");
            return;
        }
        Debug.Log("✓ Tilemap found");

        // Get initial tile count
        int tilesBeforeGeneration = CountTilesInTilemap(tilemap);
        Debug.Log($"Tiles before generation: {tilesBeforeGeneration}");

        // Clear tilemap for clean test
        tilemap.ClearAllTiles();
        Debug.Log("✓ Tilemap cleared");

        // Run generation
        orchestrator.GenerateMap();
        Debug.Log("✓ GenerateMap() called");

        // Get final tile count
        int tilesAfterGeneration = CountTilesInTilemap(tilemap);
        Debug.Log($"Tiles after generation: {tilesAfterGeneration}");

        // Validate results
        if (tilesAfterGeneration > 0)
        {
            Debug.Log($"✓ TEST PASSED: Tilemap populated with {tilesAfterGeneration} tiles");
            Debug.Log("=== MAP GENERATION TEST END (SUCCESS) ===");
        }
        else
        {
            Debug.LogError("TEST FAILED: Tilemap was not populated. Check generator configurations.");
            Debug.Log("=== MAP GENERATION TEST END (FAILURE) ===");
        }
    }

    /// <summary>
    /// Counts all non-empty tiles in the tilemap.
    /// </summary>
    private int CountTilesInTilemap(Tilemap targetTilemap)
    {
        int count = 0;
        BoundsInt bounds = targetTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (targetTilemap.HasTile(pos))
            {
                count++;
            }
        }

        return count;
    }
}
