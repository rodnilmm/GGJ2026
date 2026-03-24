using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManhattanGenerator : MonoBehaviour
{
    [SerializeField] private Transform mapBounds;
    public GameObject[] masks;
    public int maskCount = 4; // Number of mask copies to generate
    public int minManhattanDistance = 4; // Distance in "steps"
    public float zOffset = 1.0f; // Z position for spawned masks (below tiles at z=2)

    [Header("Execution")]
    [SerializeField] private bool autoGenerateOnStart = false; // Set to false to allow orchestrator control

    private List<Vector3Int> occupiedCells = new List<Vector3Int>();
    private List<Vector3> generatedPositions = new List<Vector3>();
    private bool externalOccupiedCells = false;

    public void SetMapBounds(Transform bounds)
    {
        mapBounds = bounds;
    }

    public void SetMasks(GameObject[] maskPrefabs)
    {
        masks = maskPrefabs;
    }

    public void SetMaskCount(int count)
    {
        maskCount = count;
    }

    public void SetOccupiedCells(List<Vector3Int> cells)
    {
        occupiedCells = cells;
        externalOccupiedCells = true;
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
    /// </summary>
    public void GenerateOnTilemap()
    {
        Tilemap tilemap = mapBounds != null ? mapBounds.GetComponent<Tilemap>() : null;

        if (tilemap == null)
        {
            Debug.LogError("TilemapManhattanGenerator: Cannot generate without a Tilemap assigned.");
            return;
        }

        // Clear only when not using a shared external list
        if (!externalOccupiedCells)
            occupiedCells.Clear();
        generatedPositions.Clear();

        // 1. Gather all valid tiles
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
                GameObject spawnedMask = Instantiate(masks[placedCount % masks.Length], spawnPos, Quaternion.identity, mapBounds.parent);

                SpriteRenderer spriteRenderer = spawnedMask.GetComponent<SpriteRenderer>();
                if (spriteRenderer is not null)
                {
                    spriteRenderer.sortingOrder = 0;
                }

                generatedPositions.Add(spawnPos);
                occupiedCells.Add(candidateCell);
                placedCount++;
            }
        }
    }

    public IReadOnlyList<Vector3> GetGeneratedPositions()
    {
        return generatedPositions.AsReadOnly();
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
