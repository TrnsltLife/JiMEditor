# Scenario scene graph (Avalonia) — implementation plan

This document replaces ad-hoc notes about GraphX migration. It lives in the port repo so it stays versioned with the Avalonia work.

## Implementation rule (non-negotiable)

**Do not store graph UI state in `.jime`.** That includes, at minimum:

- Node positions (manual or auto-layout results)
- Pan / scroll / zoom level or viewport transform
- Selection state, collapsed groups, or other editor chrome

All of the above may exist **only in memory** for the current editor session. On load, rebuild the graph from `Scenario` (and related collections) and **recompute layout and viewport defaults** (e.g. fit-to-graph or a fixed “home” zoom). This keeps the playback app and file format stable and avoids merge/conflict pain on shared scenarios.

If FlowGraph or any host serializes graphs to JSON for its own undo stack, keep that **outside** `.jime` (in-memory, temp files, or editor-local storage—not the scenario file).

## Context in this codebase

- WPF JiM uses **GraphX** (`ZoomControl` + custom `GraphArea`) in the sibling repo (`JiMEditor/Views/Visualization/GraphWindow.xaml`) and embeds graph UI in the main editor.
- Avalonia does **not** port GraphX; `JiMEditor-Avalonia` shows a placeholder in `src/JiMEditor.Avalonia/Views/MainWindow.axaml`.
- `src/JiMEditor.Avalonia/JiME/Models/Visualization/` still references GraphX types (e.g. `GraphLogic : GXLogicCore<...>`). The **scenario graph meaning** (what is a node, what is an edge) must become **JiM-owned** types independent of any UI package.

## Chosen stack: FlowGraph

**Decision:** implement the first shipping graph using **[prismify-co/FlowGraph](https://github.com/prismify-co/FlowGraph)** (`FlowGraph.Avalonia`, MIT).

**Rationale (summary):**

- README documents **pan & zoom**, **ports**, multiple **edge** styles, **minimap**, **controls**, **routing** (including A*), **virtualization**, undo/redo, and **custom renderers**—a good match for a rich editor graph without building everything on raw `Canvas`.
- Targets **.NET 9** and **Avalonia 11.3** (align with tightening project TFMs over time).

**Risks (explicit):**

- The project is **young** and smaller than mature alternatives (e.g. wieslawsoltes/NodeEditor). Mitigate with the **interface layer** below so we can swap the host implementation if FlowGraph becomes a bottleneck.

**Fallback (not v1):** [wieslawsoltes/NodeEditor](https://github.com/wieslawsoltes/NodeEditor) (`NodeEditorAvalonia`) remains the best-documented Avalonia-native alternative; [Oaz/AvaloniaGraphControl](https://github.com/Oaz/AvaloniaGraphControl) is a lighter option if needs simplify to read-only layout.

## Architecture: interface layer (swappable graph host)

JiM talks to a **small, stable** API; `FlowGraph.Avalonia` sits behind a host adapter.

### 1. `IScenarioGraphDocument` (read model)

- Built from `Scenario` + derived edges (triggers, `triggerAfterName`, chapter links, etc.).
- **No** FlowGraph / GraphX types on this surface.
- Pure data: node ids, labels, kinds, edge list with typed semantics.

### 2. `IScenarioGraphLayoutEngine`

- Input: `IScenarioGraphDocument` + layout policy (e.g. hierarchical left-to-right, compact).
- Output: **ephemeral** positions/sizes for the host (in memory only). **Never** “load saved positions from `.jime`.”
- Optional: `FitViewportSuggestion` (bounds) for “zoom to fit” after layout—still not persisted.

### 3. `IScenarioGraphViewHost` (+ `IScenarioGraphInteractionSink` or events)

- Owns FlowGraph `FlowCanvas` (and optional `FlowBackground`, `FlowControls`, `FlowMinimap`, etc.).
- Maps clicks / double-clicks to JiM commands (`OpenInteractionEditor(...)`, `FocusTrigger(...)`, etc.).
- Pan/zoom are **session state** only.

### 4. `IScenarioGraphTheme` (optional)

- Maps JiM node kinds to brushes, shapes, icons—keeps styling centralized and independent of FlowGraph’s default look.

### Swapping libraries later

Implement `IScenarioGraphViewHost` twice only if needed: `FlowGraphScenarioGraphHost` today; a second host would reuse `IScenarioGraphDocument` + `IScenarioGraphLayoutEngine` and replace XAML/control wiring.

## Phased implementation plan

### Phase A — Inventory and graph spec

- **Graph spec (living):** [ScenarioGraphSpec.md](ScenarioGraphSpec.md) — node kinds, edge labels, and major wiring rules distilled from WPF `Graph.Generate` / `DataVertex`.
- Remaining inventory: `GraphWindow.xaml` + code-behind (filters, save-image, click routing) when those behaviors are ported.

### Phase B — JiM graph model + layout (no FlowGraph yet)

- Introduce JiM-owned vertex/edge (or node/connector) models and remove GraphX coupling from compiled Avalonia paths.
- **Status:** `ScenarioGraphBuildScope.FullScenario` builds the full read model (WPF `Graph.Generate` parity). **Layout:** `LayeredRelaxationLayoutEngine` (longest-path relaxation + column stacking) is a first-pass v1; replace with a smarter layered/Sugiyama-style solver when needed. Unit tests use **`Scenario("name")`** fixtures (avoid parameterless `Scenario()` in tests — it runs `CreateDefaults()` and needs editor `Utils` init).

### Phase C — FlowGraph integration spike

- Add NuGet `FlowGraph.Avalonia` (version compatible with repo Avalonia line).
- Host minimal `FlowCanvas` in the `MainWindow` graph placeholder (feature flag acceptable).
- Pipe `IScenarioGraphDocument` + layout output into FlowGraph’s graph model; verify pan/zoom, selection, double-click → stub handler.

### Phase D — Parity and product behavior

- **Status:** Main graph pane hosts **FlowControls** + **FlowMinimap**; **simplified FlowGraph rendering** turns on automatically above ~90 nodes; **direct rendering** turns on above ~220 nodes; rebuilds are **debounced** (~85ms) and **chapter `tileObserver` / tile `tokenList`** changes trigger refresh (nested collections).
- **Status:** edge kind → visual style mapping is now in place (e.g. `Contains`/`Includes`/`ThreatChain` use non-default path/marker choices), while node theming remains mostly default.
- **Status:** interaction parity wiring started: `IScenarioGraphInteractionSink` is in place and node activation (FlowGraph double-click) routes to Scenario-backed editors for **Chapter / Objective / Trigger / Interaction** nodes.
- **Context menu policy (important):** right-click actions must operate on **`Scenario` domain data** (or open existing editor dialogs), not mutate the rendered graph directly. We do **not** allow unmanaged graph-only node edits.
- **TODO (context menu parity):** remove/disable FlowGraph default graph-edit items that act on canvas state (e.g. direct delete/duplicate/new-node behavior) and replace with JiM-aware actions consistent with sidebar operations (Objectives/Events/Triggers/Enemy Attack Groups/Bonuses/etc.).
- **Image export** (WPF `GraphWindow` had “Save image”) via Avalonia render APIs—export is file output, not `.jime` mutation.
- Performance: large scenario fixture; tune virtualization if needed.

### Phase E — Harden and ship

- Remove dead GraphX references from the Avalonia build path.
- Document for contributors: **never** extend `FileManager` / scenario JSON for graph UI state.

## Product decisions (locked)

- **Interaction:** Draggable nodes during a session are **allowed**; **no graph UI state** (positions, pan/zoom, selection) is persisted to **any file** — same rule as `.jime` above.
- **Scope:** **Entire scenario graph** (WPF parity target). **Filtered subgraphs** (per chapter, per objective chain, etc.) are **explicitly deferred** until the full graph reaches parity with the WPF editor.
- **Platforms:** **Desktop only** for this workstream. **Browser-hosted editor** is **not** a requirement. **Tablet (iOS/Android)** may become a requirement later; no current obligation to design for WASM or mobile in the graph stack.

## References

- FlowGraph: https://github.com/prismify-co/FlowGraph  
- NodeEditor (fallback): https://github.com/wieslawsoltes/NodeEditor  
- Port guidelines: [AGENTS.md](../AGENTS.md), [PLAN.md](../PLAN.md)

## Layout algorithm explorations (future)

This section is a parking lot for layout experiments after baseline parity is stable. All options remain subject to the non-negotiable rule above: layout output is session-only and never persisted to `.jime`.

### Candidate algorithms

- **Layered (current baseline):** left-to-right/topological bands using trigger/objective causality.
- **Radial outside-in:** put major anchors (TileBlocks/Chapters, Threat ladder nodes, optionally START) on an outer ring; place dependent/branch nodes inward toward the center.
- **Radial inside-out:** place START / central control nodes in the center; expand outward by graph distance (rings per depth).
- **Concentric hybrid:** fixed semantic rings (e.g., center=`START/objectives`, middle=`interactions/triggers`, outer=`chapters/tiles/threats`) with per-ring ordering heuristics.
- **Force-directed (bounded):** spring/repulsion pass with constraints for semantic groups; useful for discovery views but less deterministic.
- **Clustered layered:** run layered layout globally, then compact chapter-local subgraphs (tile/token clusters) as collapsible “islands.”

### Radial-specific ideas to test

- **Threat arc anchoring:** keep threat nodes on a visible arc or ring segment so progression remains readable.
- **Chapter hub strategy:** chapter node as local hub, tile/token leaves either radial-inward or radial-outward based on density.
- **Ring crossing minimization:** ring ordering by edge crossing score rather than alphabetical label.
- **Dual mode:** allow switching between outside-in and inside-out radial from a view option (session-only UI setting).

### Evaluation criteria (for selecting a v2 default)

- **Readability:** can users follow primary scenario flow quickly (start/objective/trigger chains)?
- **Edge crossings:** total crossings and overlap density on medium/large scenarios.
- **Stability:** small data edits should not completely reshuffle positions.
- **Performance:** layout runtime on large fixtures and interaction smoothness in FlowGraph.
- **Cognitive fit:** whether chapter/threat-heavy scenarios are easier to reason about than with layered.

### Implementation notes

- Keep algorithms behind `IScenarioGraphLayoutEngine`; no renderer-specific types in engine APIs.
- Add fixture-driven benchmark and screenshot comparisons before promoting a new default.
- Treat radial/force layouts as opt-in experiments first; maintain layered as fallback until parity + usability confidence.

### Observed FlowGraph behavior to potentially leverage

- **Box selection:** `Shift + drag` can select multiple nodes in the current canvas.
- **Ad-hoc grouping:** context menu **Group** wraps selected nodes in a movable group box.
- **Potential future use:** temporary grouping could help with manual visual organization during analysis sessions, and may inform future cluster-aware layout experiments.
- **Constraint reminder:** these interactions remain **session-only** unless we explicitly design Scenario-backed semantics for grouping later.

## Graph context menu roadmap

- **Design goal:** context menu actions are a convenience surface for existing scenario workflows, not a separate graph authoring model.
- **Allowed action types:** open/edit/duplicate/delete domain entities through the same Scenario-backed commands used elsewhere in the editor.
- **Disallowed action types:** direct graph-canvas mutations that bypass Scenario ownership (creating free nodes/edges, deleting visual nodes only, etc.).
- **Implementation note:** use FlowGraph context menu hooks to intercept or replace defaults; route commands through JiM handlers (`MainWindow` / dialog workflows), then rebuild graph from Scenario.
