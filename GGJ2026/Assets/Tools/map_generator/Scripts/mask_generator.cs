using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManhattanGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    public GameObject[] prefabs; 
    public Transform fixedObject;
    public int minManhattanDistance = 4; // Distance in "steps"

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
                tilemap = FindObjectOfType<Tilemap>();
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
    /// Generates placement of prefabs on existing tiles using Manhattan distance.
    /// Should be called after BiomeGenerator.GenerateMap() to ensure tiles are populated.
    /// </summary>
    public void GenerateOnTilemap()
    {
        if (tilemap == null)
        {
            Debug.LogError("TilemapManhattanGenerator: Cannot generate without a Tilemap assigned.");
            return;
        }

        if (fixedObject == null)
        {
            Debug.LogWarning("TilemapManhattanGenerator: fixedObject not assigned. Placement may be unexpected.");
        }

        // Clear occupied cells for fresh generation
        occupiedCells.Clear();

        // 1. Convert fixed object position to Cell Space
        Vector3Int fixedCell = tilemap.WorldToCell(fixedObject.position);
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
        foreach (Vector3Int candidateCell in availableCells)
        {
            if (placedCount >= prefabs.Length) break;

            if (IsValidManhattan(candidateCell))
            {
                Vector3 spawnPos = tilemap.GetCellCenterWorld(candidateCell);
                Instantiate(prefabs[placedCount], spawnPos, Quaternion.identity);
                
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