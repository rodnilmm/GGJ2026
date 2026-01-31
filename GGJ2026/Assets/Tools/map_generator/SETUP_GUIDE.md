# Map Generation System - Setup & Integration Guide

## Overview
The map generation system consists of two sequential generators that work with Unity's built-in **Tilemap** system:

1. **BiomeGenerator** - Generates base terrain tiles using Perlin noise
2. **TilemapManhattanGenerator** - Places prefabs on generated tiles using Manhattan distance constraints
3. **MapGenerationOrchestrator** - Orchestrates sequential execution of both generators

---

## Architecture

### Data Flow
```
MapGenerationOrchestrator
├── BiomeGenerator.GenerateMap()
│   └── Populates Tilemap with sand/rock tiles
└── TilemapManhattanGenerator.GenerateOnTilemap()
    └── Places prefabs on populated tiles
```

### Shared Grid
- Both generators reference the **same Tilemap component**
- BiomeGenerator populates it with tile assets
- TilemapManhattanGenerator places game objects on populated cells

---

## Scene Setup Instructions

### Step 1: Prepare the Scene
1. Open your target scene (e.g., `GameLevel.unity`)
2. Ensure a **Tilemap** GameObject exists in the scene
   - It should have a `Tilemap` component attached
   - Note its name/path for reference

### Step 2: Create Generator GameObjects

#### Create BiomeGenerator GameObject
1. Create a new empty GameObject named `BiomeGenerator`
2. Add the **BiomeGenerator** script component
3. In the Inspector, assign:
   - **Tilemap** (the Tilemap from Step 1)
   - **Rock Tile** - Select from `Assets/TileMap/Blocks_*.asset` (e.g., `Blocks_0.asset`)
   - **Sand Tile** - Select from `Assets/TileMap/Blocks_*.asset` (e.g., `Blocks_1.asset`)
4. Configure (optional):
   - **Width** / **Height** - Tile grid dimensions (default: 100x100)
   - **Scale** - Perlin noise frequency (default: 10.0)
   - **Auto Generate On Start** - Set to `false` (let orchestrator control)

#### Create TilemapManhattanGenerator GameObject
1. Create a new empty GameObject named `TilemapManhattanGenerator`
2. Add the **TilemapManhattanGenerator** script component
3. In the Inspector, assign:
   - **Tilemap** - Same Tilemap from Step 1
   - **Prefabs** - Array of GameObjects to place (e.g., enemies, obstacles)
   - **Fixed Object** - Transform to maintain minimum distance from (e.g., player spawn)
   - **Min Manhattan Distance** - Minimum distance between placements (default: 4)
4. Configure:
   - **Auto Generate On Start** - Set to `false`

### Step 3: Create Orchestrator GameObject
1. Create a new empty GameObject named `MapGenerationOrchestrator`
2. Add the **MapGenerationOrchestrator** script component
3. In the Inspector (optional - can auto-find):
   - **Biome Generator** - Drag BiomeGenerator GameObject here
   - **Manhattan Generator** - Drag TilemapManhattanGenerator GameObject here
   - **Auto Generate On Start** - Set to `true` to auto-run on scene load
4. If left empty, the orchestrator will auto-find both generators via `FindObjectOfType<T>()`

### Step 4: (Optional) Add Test Component
1. Create another empty GameObject named `MapGenerationTest`
2. Add the **MapGenerationTest** script component
3. In the Inspector:
   - **Tilemap** - Assign the Tilemap from Step 1
   - **Run Test On Start** - Set to `true`
4. Play the scene and check the Console for test output

---

## Usage

### Automatic Generation (On Scene Load)
- Set `MapGenerationOrchestrator.autoGenerateOnStart = true`
- Generation will run automatically in `Start()`

### Manual Generation (From Script)
```csharp
// Get the orchestrator reference
MapGenerationOrchestrator orchestrator = FindObjectOfType<MapGenerationOrchestrator>();

// Full generation (biome + placement)
orchestrator.GenerateMap();

// Regenerate biome only (new Perlin noise, keep prefab placements)
orchestrator.RegenerateBiome();

// Regenerate placement only (re-shuffle prefab positions on same tiles)
orchestrator.RegeneratePlacement();
```

### Manual Generation (From Editor)
1. Select the `MapGenerationOrchestrator` GameObject
2. In the Inspector, click the function drop-down next to the script
3. Invoke `GenerateMap()`, `RegenerateBiome()`, or `RegeneratePlacement()`

---

## Configuration Reference

### BiomeGenerator
| Parameter | Default | Purpose |
|-----------|---------|---------|
| `width` | 100 | Grid width in tiles |
| `height` | 100 | Grid height in tiles |
| `scale` | 10.0 | Perlin noise frequency (lower = larger zones) |
| `seed` | 0 | Random seed (0 = auto-generate) |
| `autoGenerateOnStart` | false | Run generation in `Start()` |

### TilemapManhattanGenerator
| Parameter | Default | Purpose |
|-----------|---------|---------|
| `prefabs` | - | Array of GameObjects to place |
| `fixedObject` | - | Transform to maintain distance from |
| `minManhattanDistance` | 4 | Minimum tiles between placements |
| `autoGenerateOnStart` | false | Run generation in `Start()` |

### MapGenerationOrchestrator
| Parameter | Default | Purpose |
|-----------|---------|---------|
| `biomeGenerator` | null | Reference to BiomeGenerator |
| `manhattanGenerator` | null | Reference to TilemapManhattanGenerator |
| `autoGenerateOnStart` | true | Run generation in `Start()` |

---

## Troubleshooting

### "Tilemap not assigned" Error
- **Cause**: Tilemap reference is null and auto-find failed
- **Solution**: 
  1. Manually assign Tilemap in Inspector for both generators
  2. Ensure Tilemap is on a GameObject with a `Tilemap` component

### "BiomeGenerator/TilemapManhattanGenerator not found" Error
- **Cause**: Orchestrator couldn't find one or both generators
- **Solution**:
  1. Manually assign them in the Orchestrator Inspector
  2. Ensure both scripts are attached to GameObjects in the scene

### Tiles Not Appearing
- **Cause**: Rock/Sand tiles not assigned, or width/height is 0
- **Solution**:
  1. Assign tile assets to BiomeGenerator
  2. Set width/height > 0 (minimum 1)
  3. Check that scale is reasonable (too high = mostly one color)

### Prefabs Not Appearing
- **Cause**: Prefabs array empty, fixed object not assigned, or Manhattan distance too high
- **Solution**:
  1. Assign prefabs to the array
  2. Assign fixedObject transform
  3. Reduce `minManhattanDistance` if few prefabs are placed

### Generation Runs Twice
- **Cause**: Both `autoGenerateOnStart` enabled on generators AND orchestrator
- **Solution**: Set `autoGenerateOnStart = false` on both generators, keep only orchestrator at `true`

---

## Testing

Run the included `MapGenerationTest.cs` to validate setup:
1. Attach to a GameObject in the scene
2. Set `Run Test On Start = true`
3. Play the scene
4. Check Console output for:
   - `✓ MapGenerationOrchestrator found`
   - `✓ Tilemap found`
   - `Tiles after generation: [number > 0]`
   - `✓ TEST PASSED` or error details

---

## File Locations

```
Assets/Tools/map_generator/Scripts/
├── map_generator.cs                 (BiomeGenerator class)
├── mask_generator.cs                (TilemapManhattanGenerator class)
├── MapGenerationOrchestrator.cs     (Orchestrator)
└── MapGenerationTest.cs             (Validation test)

Assets/TileMap/
├── Blocks_0.asset                   (Tile asset option 1)
├── Blocks_1.asset                   (Tile asset option 2)
├── ... (more tile options)
```

---

## Advanced Usage

### Regenerating with New Seed
```csharp
BiomeGenerator biome = FindObjectOfType<BiomeGenerator>();
biome.seed = Random.Range(0f, 10000f);
biome.GenerateMap();
```

### Changing Manhattan Distance
```csharp
TilemapManhattanGenerator manhattan = FindObjectOfType<TilemapManhattanGenerator>();
manhattan.minManhattanDistance = 6; // Increase spacing
manhattan.GenerateOnTilemap();
```

### Clearing and Regenerating
```csharp
MapGenerationOrchestrator orchestrator = FindObjectOfType<MapGenerationOrchestrator>();
Tilemap tilemap = FindObjectOfType<Tilemap>();
tilemap.ClearAllTiles(); // Clear existing tiles
orchestrator.GenerateMap(); // Regenerate from scratch
```

---

## Integration Checklist

- [ ] BiomeGenerator script added to scene
- [ ] TilemapManhattanGenerator script added to scene
- [ ] MapGenerationOrchestrator added to scene
- [ ] Tilemap assigned to all three scripts (or auto-find working)
- [ ] Rock & Sand tiles assigned to BiomeGenerator
- [ ] Prefabs array assigned to TilemapManhattanGenerator
- [ ] Fixed object assigned to TilemapManhattanGenerator
- [ ] autoGenerateOnStart = false on both generators
- [ ] autoGenerateOnStart = true on orchestrator (or manually trigger)
- [ ] Test scene loads without errors
- [ ] Console shows successful generation with tile counts
