---
name: "reviewer-gate-validation"
description: "How Bobbie converts anticipatory tests into a formal reviewer gate for new Comet features."
domain: "testing"
confidence: "high"
source: "earned"
---

## Context

Use this when a feature landed behind anticipatory/skipped tests and you need a real reviewer verdict instead of a placeholder sign-off.

## Patterns

### Convert skips before judging
- If Bobbie previously left `[Fact(Skip = ...)]` placeholders, treat them as the review checklist.
- Replace each skip with a runnable assertion against the landed API before deciding approve/reject.

### Validate in repository order
- Follow Comet's documented build/test order exactly:
  1. `dotnet build src/Comet.SourceGenerator/Comet.SourceGenerator.csproj -c Release`
  2. `dotnet build src/Comet/Comet.csproj -c Release`
  3. `dotnet build tests/Comet.Tests/Comet.Tests.csproj -c Release`
  4. `dotnet test tests/Comet.Tests/Comet.Tests.csproj --no-build -c Release`

### Prove the feature locally, then widen the net
- Run the smallest feature-specific slice first (for Phase 6: `NativeHostTests` + `NativeHostInteropTests`).
- After that, run a broader filtered suite that excludes only well-documented pre-existing failures.
- Compare the unfiltered failure signature with the historical baseline so new failures are obvious.

### Catch order-dependent hot reload regressions
- If the feature touches `TriggerReload()`, handler attachment, or active-view registration, do **not** trust an isolated hot reload slice by itself.
- Run the focused slice first, then a broader filtered suite in the same build so stale/global hot reload registrations can surface.
- Treat “passes alone, fails in the wider suite” as a blocker, not flaky noise. In Comet, watch for `CometApp.MauiContext` null crashes from `DatabindingExtensions.AreSameType(...)` during reload.

### Record the exact gate evidence
- Capture which files were reviewed.
- Record focused pass counts, filtered-suite status, and any historical failures that still reproduce.
- Approval requires “feature complete for scope” plus “no new regressions.”

## Examples

- **Phase 6 NativeHost:** unskip the 4 `NativeHostInteropTests` placeholders, verify 23/23 NativeHost-focused tests pass, then run the broader suite with known keyed stack-overflow and historical hot-reload failures filtered out.
- **Phase 7 Component hot reload:** isolated `ComponentHotReloadTests` may pass even when the broader suite still fails. Always widen the net after `TriggerReload()` work to catch active-view registry regressions.

## Anti-Patterns

- Approving because skipped tests still exist.
- Running only the narrow feature tests and skipping broader regression coverage.
- Treating historical failures as new regressions without checking the prior baseline.
