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

- **Pseudo-HTML preview rendering:** For read-only/display previews of JiME pseudo-HTML, use [`PseudoHtmlPreviewTextBlock`](src/JiMEditor.Avalonia/PseudoHtmlPreviewTextBlock.cs) instead of plain `TextBlock` text binding. It renders `<b>`, `<i>`, and `<font="Icon">` (icon font runs) consistently via `JiMeAvaloniaRichPreview`.

- **Replace WPF-specific pieces with Avalonia equivalents** as you port: `MessageBox`, `OpenFileDialog`/`SaveFileDialog`, `pack://` URIs, `Window`-chrome hacks (`user32`), `FlowDocument`/`RichTextBox`, **GraphX**. See [PLAN.md](PLAN.md) for hotspots.

- **Prefer async APIs** where Avalonia expects them (e.g. file pickers) instead of blocking patterns carried over from WPF.

- **Keep the solution buildable.** After each meaningful chunk of work, the Avalonia solution should compile (or only fail in known, commented `TODO-Avalonia-Reenable` areas if you use feature flags—avoid a permanently broken main branch).

- **Naming:** Align assembly/root namespace with whatever this repo adopts (e.g. `JiME` vs `JiMEditor.Avalonia`) and stay consistent across new projects.

- **Tabs (porting WPF `TabControl`):** Avalonia’s default **Fluent** tab chrome differs from classic WPF. For JiMEditor-like flat dark tabs, put **`Classes="jimeTabStrip"`** on the **`TabControl`** (styles live in [`Assets/ProjectWindowStyles.axaml`](src/JiMEditor.Avalonia/Assets/ProjectWindowStyles.axaml)). **CampaignWindow** already uses this; reuse for other ported windows with tabs.
- **`TabItem` padding (WPF → Avalonia):** Copying WPF **`TabItem`** **`Padding`** (e.g. `Padding="5"`) onto Avalonia often looks wrong—layout/chrome differs. Prefer something like **`Margin="2,2,2,0"`** **`Padding="2"`** on each **`TabItem`** until a shared style exists. **CampaignWindow.axaml** and **HelpWindow.axaml** in this repo use that pattern for now.

## Avalonia popup dialogs: hydrating the form and saving back

Many JiMEditor windows are **model-owned** (`Scenario`, `Objective`, interactions, etc.). WPF often relied on **`TwoWay`** bindings and **`LostFocus`** update timing; Avalonia needs a more explicit pattern so values **show on open** and **persist on OK**.

**Reference implementation:** [`ObjectiveEditorWindow`](src/JiMEditor.Avalonia/Views/ObjectiveEditorWindow.axaml) / [code-behind](src/JiMEditor.Avalonia/Views/ObjectiveEditorWindow.axaml.cs).

1. **Hydrate after the visual tree exists**  
   Use a **one-shot `Window.Loaded`** handler (subscribe from the ctor that receives the model). Do not rely on **`Opened` alone** for filling nested controls inside **`ScrollViewer`** / templates—ordering can leave fields empty even when the model is correct.

2. **Named controls + explicit model → UI**  
   For fields that are awkward to bind (see below), use **`x:Name`** on **`TextBox`** / pickers and a method such as **`Populate*FromModel()`** that assigns **`.Text`** (or selected items) from the domain object when the dialog opens.

3. **Explicit UI → model on OK**  
   At the start of **`OkButton_Click`** (before validation), read **`TextBox.Text`** (trim, **`int.TryParse`** with **`CultureInfo.InvariantCulture`** for integers) and write into the model. Do not assume **`TwoWay`** binding has already updated **`int`** properties from **`Text`**—that path is unreliable without a converter.

4. **`ComboBox` / `ItemsSource` churn**  
   Setting **`ItemsSource = null`** clears **selection** and can fire **`SelectionChanged`** with **`SelectedItem == null`**. If a handler maps “null selection” to a sentinel (e.g. **`"None"`**), it can **wipe** a value you just set (e.g. after “add new item” + rebind). Use a **`_suppressSelectionSync`** flag around programmatic rebinding + re-select, and/or restore the previous suppress state in **`finally`** so nested helpers do not clear it early.

5. **Translatable entities after `ShowDialog`**  
   WPF’s **`Translatable.HandleWindow`** wraps WPF **`ShowDialog`**. For Avalonia windows, capture **`CaptureStartingValues()`**, **`TranslationKeyPrefix()`**, **`TranslationKeyName()`** *before* **`await ShowDialog`**, then call **`ApplyTranslationUpdatesAfterEditorDialog(...)`** on the entity after the dialog closes (see **`JiME/Common/Common.cs`**). Run that **whether the user OK’d or Cancelled**, to match WPF’s post-dialog key/decertify behavior.

6. **Success flag**  
   Expose something like **`Accepted`** (set only on successful OK) so callers do not add clones or new rows when the user cancels.

Reuse this checklist for each new popup: **Loaded hydrate → OK commit → combo suppress as needed → translation tail → `Accepted`.**

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
