---
name: "phase9-sample-gate-triage"
description: "How Bobbie separates a real sample build break from a deterministic Phase 9 validation-rule mismatch."
domain: "testing"
confidence: "high"
source: "earned"
---

## Context

Use this when a Comet sample is reported as “broken after a rebuild/import fix,” but the failure may actually come from the opt-in Phase 9 sample/documentation gate rather than from `dotnet build`.

## Patterns

### Build first, label second
- Run the documented build order before calling it a compile break:
  1. `dotnet build src/Comet.SourceGenerator/Comet.SourceGenerator.csproj -c Release`
  2. `dotnet build src/Comet/Comet.csproj -c Release`
  3. `dotnet build sample/<SampleName>/<SampleName>.csproj -c Release -f net10.0-maccatalyst`
- If those commands are green, the sample is not currently suffering a compiler/import failure even if a validation wrapper is red.

### Reproduce the gate in isolation
- Run the focused Phase 9 test directly with the environment variable for the artifact under test instead of only relying on the wrapper script.
- For the coffee lane, use `COMET_PHASE9_COFFEE_SAMPLE_PROJECT=... dotnet test tests/Comet.Tests/Comet.Tests.csproj --no-build -c Release --filter "FullyQualifiedName~Phase9SampleDocumentationValidationTests.CoffeeAppPassesPhase9GateWhenConfigured"`.

### Normalize env paths when bypassing the wrapper
- The xUnit gate resolves `COMET_PHASE9_*` paths with `Path.GetFullPath(...)` inside the test host, so repo-relative values can resolve under `tests/Comet.Tests/bin/...` instead of the repository root.
- When running the env-driven gate directly, pass **absolute** project/guide paths. The wrapper script is safer because it normalizes relative CLI arguments before exporting `COMET_PHASE9_COUNTER_SAMPLE_PROJECT`, `COMET_PHASE9_COFFEE_SAMPLE_PROJECT`, and `COMET_PHASE9_MIGRATION_GUIDE`.

### Check bootstrap exclusion against the accepted feature tokens
- `Phase9Project.IsBootstrapFile(...)` excludes `MauiProgram.cs`, `Program.cs`, `AppShell.cs`, and any `*App.cs`.
- If the only `NavigationView`, `TabView`, or other “rich surface” token lives in `*App.cs`, the gate can false-fail even though the sample is wired correctly.

### Compare the sample's real navigation API to the regex
- In the current validator, `RichCoffeeSurfacePattern` recognizes `CollectionView`, `NavigationView`, `TabView`, `CometShell`, `GoToAsync<T>()`, and `RegisterRoute<T>()`.
- If the sample uses `Navigation.Navigate<T>()` instead, the gate will miss it unless the regex is expanded or the sample exposes another accepted token in non-bootstrap files.

### Validate mixed-surface samples through the evolved reference files
- If the sample intentionally keeps older `[Body]` / `State<T>` pages for migration storytelling, do not run the “no legacy tokens anywhere” rule across the whole sample.
- Instead, identify the current-surface reference files (for example, files that actually use `Component`, `Render()`, `SetState(...)`, `Reactive<T>`, or typed navigation) and run the strict deprecated-token scan there.

### Separate deprecated `Frame` control checks from `.Frame(...)` layout helpers
- A broad `\bFrame\b` regex will incorrectly flag Comet's fluent sizing helper (`.Frame(height: 48)`), even when the sample uses `Border` and never instantiates the deprecated MAUI `Frame` control.
- Prefer matching `new Frame` / `: Frame` shapes (or other type-usage patterns) when the gate is trying to ban the control type itself.

### Scan for the next failure before calling it fixed
- After the first assertion, search the sample for the deprecated Phase 9 tokens (`[Body]`, `State<T>`, `ListView`, `TableView`, `Frame`, `Device.*`, sync alerts, `Compatibility.*`, etc.).
- This prevents misclassifying a rule-mismatch as the whole problem when the sample still contains real migration debt behind it.

## Examples

- **Coffee sample triage:** `sample/CometBaristaNotes` built successfully, but the focused gate failed because `BaristaApp.cs` held `NavigationView` and got excluded as bootstrap, while the evolved pages used `Navigation.Navigate<T>()` that the regex did not count.
- **Closure revision:** the fix was to count `Navigation.Navigate<T>()`, scope the strict legacy-token scan to the evolved reference files, and tighten the `Frame` check so `.Frame(...)` sizing calls stay valid.

## Anti-Patterns

- Calling a validation failure a compile/import break without running `dotnet build`.
- Fixating on the first failed assertion and ignoring the next layer of legacy-token hits in the sample.
- Assuming all typed navigation APIs are equivalent to the validator unless the regex proves it.
