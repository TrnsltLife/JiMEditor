# Agent guidelines: JiMEditor → Avalonia port

Instructions for AI assistants and humans working in **this repository** while porting [JiMEditor](../JiMEditor) to Avalonia UI.

## Project boundaries

1. **We are porting from WPF to Avalonia UI.** Expect API and XAML differences; prefer current Avalonia documentation and patterns (e.g. `StorageProvider` for file pickers, `avares://` assets).

2. **Source of truth for the existing app is `/JiMEditor` (sibling directory).** Do **not** edit files under `JiMEditor` as part of this port. Treat it as read-only reference.  
   *Pragmatic exception:* If a bug must be fixed upstream, do that in a separate change/commit in `JiMEditor` and note it here or in commit messages—do not mix porting edits into the WPF tree by accident.

3. **All porting work lives in `/JiMEditor-Avalonia`.** New projects, copied files, tests, and docs belong here.

## How we copy and evolve code

4. **Incremental port, whole-file copies.** Enable the app in small steps. When a file from `JiMEditor` is needed, **copy the entire file** into `JiMEditor-Avalonia` (then adapt namespaces, project file, and APIs). Avoid copying “half a file” and intending to merge the rest later—easy to lose pieces.  
   *Suggestion:* At the top of a copied file, add a one-line comment noting the source path and date, e.g. `// Ported from: JiMEditor/Common/FileManager.cs (YYYY-MM-DD)`.

5. **Temporarily disabled code:** Comment it with **`TODO-Avalonia-Reenable`** (C#: `//` or `/* */`; AXAML: `<!-- TODO-Avalonia-Reenable -->`). Use a single consistent tag so we can search the repo.

6. **Code kept only for reference (likely to delete later):** Mark with **`TODO-Avalonia-Delete`**. Same comment-style rules as above. Prefer a short note *why* it is deferred (e.g. “replaced by StorageProvider”).

7. **Splitting files in the target is allowed.** If breaking one WPF file into multiple types/files in `JiMEditor-Avalonia` improves clarity or separation of UI vs logic, do it. When splitting, consider leaving a short comment in each part pointing to the original WPF file.

## Additional guidelines (recommended)

- **`.jime` compatibility is non-negotiable unless explicitly decided.** Scenario files are JSON consumed by the playback app. When touching serialization models or converters copied from `JiMEditor`, preserve field names, `interactionType` values, and the **pseudo-HTML** text format documented in [PLAN.md](PLAN.md). Prefer golden-file or round-trip tests as this repo grows.

- **Replace WPF-specific pieces with Avalonia equivalents** as you port: `MessageBox`, `OpenFileDialog`/`SaveFileDialog`, `pack://` URIs, `Window`-chrome hacks (`user32`), `FlowDocument`/`RichTextBox`, **GraphX**. See [PLAN.md](PLAN.md) for hotspots.

- **Prefer async APIs** where Avalonia expects them (e.g. file pickers) instead of blocking patterns carried over from WPF.

- **Keep the solution buildable.** After each meaningful chunk of work, the Avalonia solution should compile (or only fail in known, commented `TODO-Avalonia-Reenable` areas if you use feature flags—avoid a permanently broken main branch).

- **Naming:** Align assembly/root namespace with whatever this repo adopts (e.g. `JiME` vs `JiMEditor.Avalonia`) and stay consistent across new projects.

## Quick reference: TODO tags

| Tag | Meaning |
|-----|--------|
| `TODO-Avalonia-Reenable` | Intentionally disabled; should be restored once the Avalonia equivalent exists. |
| `TODO-Avalonia-Delete` | Kept temporarily for reference; safe to remove after the port replaces it. |

Search: `TODO-Avalonia-` to find all markers.

## Related docs

- [PLAN.md](PLAN.md) — migration analysis, `.jime` format notes, and risk areas.
- [jimeditor_avalonia_migration_238308f8.plan.md](jimeditor_avalonia_migration_238308f8.plan.md) — extended plan copy (if present).

---

*Add new sections here as the port evolves (e.g. chosen rich-text approach, test layout, CI).*
