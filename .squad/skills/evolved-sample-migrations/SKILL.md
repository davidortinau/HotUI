---
name: "evolved-sample-migrations"
description: "How Amos updates Comet samples/docs to teach the evolved component-first MVU surface without rewriting the whole repo."
domain: "samples-docs"
confidence: "high"
source: "earned"
---

## Context

Use this when a Comet feature needs sample and documentation coverage, especially when the new API is additive and older surfaces still exist in the same repository.

## Patterns

### Reuse the lightest credible sample first
- If the goal is a starter/reference sample, prefer evolving `sample/CometMauiApp` instead of creating a new project.
- Keep `CometApp` as the app root; show the evolved API at the page/component layer where `UseCometApp<TApp>()` already expects it.
- When a reusable starter pattern stabilizes, mirror it into `templates/single-project/` so future sample/page migrations can copy from the template instead of re-deriving `Component<TState>` + `Render()` + `SetState(...)` from older `[Body]` files.

### Pair a green-path sample with an incremental-migration sample
- Use one minimal sample to show the clean happy path (`Component<TState>`, `Render()`, `SetState(...)`).
- Use one richer sample to prove coexistence with older pages, services, and interop (`sample/CometBaristaNotes` is the preferred host today).

### Prefer runtime-proven tab shells in samples
- Use `TabView` for sample tab layouts unless `TabbedPage` handler wiring is known to be active on the target runtime.
- Wrap each tab root in `NavigationView` when you still need typed in-tab navigation flows.
- If a legacy sample is still a pure Comet app but routes everything through a MAUI `Shell` + `CometHost` wrapper, collapse it back to `CometApp` + `TabView` before doing deeper page migrations. Keep the MAUI host wrapper only when the sample is explicitly teaching MAUI hosting or interop seams.
- Keep interop-heavy pages off eagerly-created root tabs when they bring in native/third-party UI that is better entered through an explicit navigation flow.
- For sample list rows inside `ScrollView`-driven pages, prefer simple `HStack`/`VStack` card rows over `Grid` if iOS runtime validation shows `CALayerInvalidGeometry` crashes during first layout.

### Teach typed navigation with real props
- Prefer `Navigation.Navigate<TView>(props)` or typed shell routes in the richer sample.
- When showing detail pages, use `Component<TState, TProps>` so navigation data is explicit and reviewable.

### Keep older references, but redirect readers
- Do not delete legacy `[Body]` samples just because a newer surface exists.
- Add concise notes in older sample files/READMEs pointing to the current reference sample and migration guide.

### Preserve MAUI-current guidance in touched samples
- Avoid `Compatibility` APIs in refreshed sample code.
- Removing stale `Microsoft.Maui.Controls.Compatibility` package references is fair game when the refreshed sample no longer depends on MAUI compatibility hosting.
- Prefer MAUI 10-safe controls (`Border`, async dialog APIs, `MainThread`, etc.).
- If reused sample code emits warnings directly related to current-platform guidance, clean them while you are already in that sample.

### Modernize list-heavy samples with the smallest contract change first
- Replace `ListView` with `CollectionView` before attempting deeper list UX rewrites.
- Preserve row actions with existing `.OnTap(...)` handlers or `ItemSelected` callbacks so the sample keeps its behavior while moving onto the current MAUI-safe control.
- In richer samples, pair that list swap with one typed-props detail page so the sample teaches both current list primitives and current navigation contracts in the same pass.

### Validate in repository order
- Build in the documented sequence:
  1. `dotnet build src/Comet.SourceGenerator/Comet.SourceGenerator.csproj -c Release`
  2. `dotnet build src/Comet/Comet.csproj -c Release`
  3. `dotnet build tests/Comet.Tests/Comet.Tests.csproj -c Release`
  4. build the touched sample projects
  5. run the safest focused tests that exercise the evolved surface without confusing historical framework noise for regressions

## Examples

- **Phase 9 counter sample:** evolve `sample/CometMauiApp` into a counter page backed by `Component<CounterState>` and `Reactive<string>`.
- **Phase 9 coffee sample:** keep `ShotLoggingPage` intact, add a `CoffeeDashboardPage` and a typed `CoffeeBeanDetailPage`, then document the mixed-surface migration story.

## Anti-Patterns

- Creating a brand-new sample when an existing sample can communicate the feature with less maintenance.
- Rewriting legacy sample pages just to make everything uniform when coexistence is the lesson.
- Documenting the evolved API as if the project/package must be renamed; Comet stays Comet.
