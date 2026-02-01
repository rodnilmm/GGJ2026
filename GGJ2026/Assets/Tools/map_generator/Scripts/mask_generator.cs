using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManhattanGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    public GameObject[] masks; 
    public int maskCount = 7; // Number of mask copies to generate
    public Transform player1InitPosition, player2InitPosition, player3InitPosition, player4InitPosition;
    public int minManhattanDistance = 4; // Distance in "steps"
    public float zOffset = 3.0f; // Z position for spawned masks

    [Header("Execution")]
    [SerializeField] private bool autoGenerateOnStart = false; // Set to false to allow orchestrator control

    private List<Vector3Int> occupiedCells = new List<Vector3Int>();

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
                Debug.LogError("TilemapManhattanGenerator: Tilemap not assigned and none found in scene. Please assign a Tilemap or add one to the scene.");
            }
        }
    }

    void Start()
    {
        if (autoGenerateOnStart)
        {
            GenerateOnTilemap();
        }
    }

    /// <summary>
    /// Generates placement of masks on existing tiles using Manhattan distance.
    /// Should be called after BiomeGenerator.GenerateMap() to ensure tiles are populated.
    /// </summary>
    public void GenerateOnTilemap()
    {
        if (tilemap == null)
        {
            Debug.LogError("TilemapManhattanGenerator: Cannot generate without a Tilemap assigned.");
            return;
        }

        if ( player1InitPosition == null || player2InitPosition == null)
        {
            Debug.LogWarning("TilemapManhattanGenerator: playerInitPosition not assigned. Placement may be unexpected.");
        }

        // Clear occupied cells for fresh generation
        occupiedCells.Clear();

        // 1. Convert player 1 starting position to Cell Space
        Vector3Int fixedCell = tilemap.WorldToCell(player1InitPosition.position);
        occupiedCells.Add(fixedCell);
        fixedCell = tilemap.WorldToCell(player2InitPosition.position);
        occupiedCells.Add(fixedCell);
        fixedCell = tilemap.WorldToCell(player3InitPosition.position);
        occupiedCells.Add(fixedCell);
        fixedCell = tilemap.WorldToCell(player3InitPosition.position);
        occupiedCells.Add(fixedCell);

        // 2. Gather all valid tiles
        List<Vector3Int> availableCells = new List<Vector3Int>();
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                availableCells.Add(pos);
            }
        }

        // 3. Shuffle (Fisher-Yates)
        for (int i = 0; i < availableCells.Count; i++)
        {
            Vector3Int temp = availableCells[i];
            int randomIndex = Random.Range(i, availableCells.Count);
            availableCells[i] = availableCells[randomIndex];
            availableCells[randomIndex] = temp;
        }

        // 4. Placement Loop
        int placedCount = 0;
        int totalMasksToGenerate = maskCount * masks.Length; // Generate maskCount copies of each mask
        foreach (Vector3Int candidateCell in availableCells)
        {
            if (placedCount >= totalMasksToGenerate) break;

            if (IsValidManhattan(candidateCell))
            {
                Vector3 spawnPos = tilemap.GetCellCenterWorld(candidateCell);
                spawnPos.z = zOffset;
                Instantiate(masks[placedCount % masks.Length], spawnPos, Quaternion.identity, tilemap.transform.parent);
                
                occupiedCells.Add(candidateCell);
                placedCount++;
            }
        }
    }

    bool IsValidManhattan(Vector3Int pos)
    {
        foreach (Vector3Int occupied in occupiedCells)
        {
            // Manhattan Calculation: |x1 - x2| + |y1 - y2|
            int distance = Mathf.Abs(pos.x - occupied.x) + Mathf.Abs(pos.y - occupied.y);
            
            if (distance < minManhattanDistance)
            {
                return false;
            }
        }
        return true;
    }
}