# Scenario graph — WPF semantics (parity inventory)

Source: `JiMEditor/JiME/Models/Visualization/Graph.cs` (`Graph.Generate`) and `DataVertex.cs`, mirrored in Avalonia under `JiMEditor.Avalonia/JiME/Models/Visualization/` (compiled out of the Avalonia project via `Compile Remove`, but the algorithm is the reference).

This is the **authoritative behavior list** for a full editor graph; the Avalonia implementation may ship incrementally behind `ScenarioGraphBuildScope` (see `ScenarioGraphAvaloniaPlan.md`).

## Vertex kinds (`DataVertex.Type`)

| Kind | Meaning |
|------|---------|
| `Start` | Synthetic root (`"START"`). |
| `Trigger` | Named trigger (`dataName`, multi-trigger suffix `*`). Skips trigger named `"None"`. |
| `Objective` | Objective vertex; skips `"None"`. |
| `Resolution` | Resolution / text-book style node. |
| `Interaction` | Event / interaction vertex; skips `"None"`. |
| `InteractionGroup` | Derived group node for random interaction groups (`GRP…` in name). |
| `Chapter` | Tile block (chapter). |
| `Tile` | Tile inside a chapter (`idNumber` + `tileSide` as label key). |
| `Token` | Token on a tile. |
| `ThreatLevel` | Threat threshold nodes + synthetic max node. |

Internal dictionary keys use prefixes: `TRIG_`, `OBJ_`, `RES_`, `CHAP_`, `TILE_`, `TOKEN_`, `INT_`, `INTGRP_`, `Threat_`, etc.

## Major edge patterns (by area)

### Objectives

- Default scenario objective: `START` → objective, label **"initiates"** (`scenario.objectiveName`).
- **Triggered by** trigger → objective: **"initiates"**.
- **Completed by** (`triggerName`) trigger → objective: **"completes"**.
- **Next trigger** on completion: objective → trigger, **"on completion"**.

### Resolutions

- Trigger (`triggerName`) → resolution: **"triggers"**.

### Interactions (base)

- Trigger (`triggerName`) → interaction: **"initiates"**.
- `triggerAfterName`: interaction → trigger, **"triggers"**.
- Optional **interaction group**: group vertex → interaction, **"includes"**.

### Interaction-type-specific edges

Examples: `DecisionInteraction` / `DialogInteraction` (choice labels on edges to triggers), `TestInteraction` (**"PASS"` / `"FAIL"`**), `ConditionalInteraction` (condition list, **"finished"**), `PersistentTokenInteraction`, `ThreatInteraction`, `MultiEventInteraction`, `BranchInteraction`, `ReplaceTokenInteraction`, `ItemInteraction`, `TitleInteraction`, etc. See `Graph.Generate` for the full branching tree.

### Chapters (tile blocks)

- Tiles: chapter → tile **"contains"**; tile explore trigger → trigger **"explore triggers"**; token edges **"contains"**, **"activates"**, **"triggers"**, etc.
- **Start exploration**: if `triggeredBy` is null, empty, or `"None"`, `START` → chapter, **"explores"**.
- Random interaction group: chapter → group, **"activates"** when configured.
- `triggeredBy`, `triggerName`, `exploreTrigger`, `exploredAllTilesTrigger` each map to labeled edges to triggers where applicable.

### Threat ladder

If threats are used: ordered threat vertices chained with null label edges (`weight: 2`); edges from threat to events (**"initiates"**); chain ends at synthetic max threat vertex.

## Missing / unresolved targets

`getTriggerVertex` / `getEventVertex` log to console when a referenced name is missing (stale data after deletes).

## Avalonia mapping (current)

- JiM read model: `JiMEditor.Avalonia.ScenarioGraph` (`IScenarioGraphDocument`, builders, layout).
- **Synthetic START id:** Avalonia uses stable id `jime:graph:start` (WPF graph used display text `START` and dict key `START` for the same vertex role).
- `ScenarioGraphBuildScope.TileBlocksAndStart`: START + chapters only (smoke tests / narrow UI).
- `ScenarioGraphBuildScope.FullScenario`: parity build from `ScenarioGraphFullScenarioDocumentBuilder` (triggers, objectives, resolutions, interactions + branches, chapters/tiles/tokens, threat ladder). Node ids follow WPF key scheme (`TRIG_`, `OBJ_`, `CHAP_`, `INT_`, `TILE_{Guid}`, …).
- Layout and viewport are **session-only** (never in `.jime`).
