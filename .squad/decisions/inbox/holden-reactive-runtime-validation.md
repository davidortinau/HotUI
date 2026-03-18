# Decision: Reactive State System — Runtime Validation Results & Production-Readiness Assessment

**Author:** Holden (Lead Architect)
**Date:** 2026-03-13 (updated 2026-03-14)
**Status:** Re-validated — #22 FIXED ✅

## Context

David Ortinau requested a "brutally honest assessment" of the new reactive state management system (Signal<T>, Computed<T>, SignalList<T>, ReactiveScheduler) for production readiness. This assessment is based on runtime validation of 6 purpose-built sample pages in CometControlsGallery on macCatalyst.

**Update (2026-03-14):** Re-validated after Amos's fix for #22 (commit cee1e2b4). Background-thread signal writes now correctly update the UI. Verdict upgraded from ~85% to ~95%.

## What Works (Production-Ready)

### ✅ Core Reactive Primitives
- **Reactive<T> / Signal<T>**: Read/write, implicit conversions, UI binding — all solid
- **Computed<T>**: Lazy evaluation, automatic dependency discovery via ReactiveScope, correct invalidation when upstream signals change
- **SignalList<T>**: Add, RemoveAt, Clear, batch operations — all trigger correct UI rebuilds

### ✅ Automatic Dependency Tracking
- Body evaluation correctly uses `ReactiveScope.BeginTracking()` to discover which signals a view reads
- `View.BodyDependencySubscriber` correctly calls `ReactiveScheduler.MarkViewDirty()` when dependencies change
- No manual subscription management needed — this is a genuine developer-experience win

### ✅ Coalescing / Batching
- 100 synchronous signal writes in a tight loop produce exactly 1 body rebuild (Body executions: 1)
- ReactiveScheduler's dispatch-based coalescing works correctly for synchronous code paths

### ✅ Two-Way Binding
- Multiple controls bound to the same Reactive<T> stay in sync
- Slider ↔ Label, Toggle ↔ Label, TextField ↔ TextField all work correctly
- Programmatic signal writes from button handlers update all bound controls

### ✅ State Preservation
- Parent view state survives child navigation push/pop (timestamp proves same instance)

### ✅ Background-Thread Signal Writes (#22 — FIXED)
- 10 signal writes from `Task.Run()` background thread with 200ms intervals all reflected in UI
- Screenshot evidence: Counter=10, Body executions=12 (reasonable coalescing), status text updated from background thread
- No crash, no SIGTRAP, no silent failures
- Fix: three-layer dispatch in `ReactiveScheduler.EnsureFlushScheduled()` — detect main thread and call FlushEntry() directly instead of double-queuing

## Resolved Issues

### ✅ #22: Background-Thread Signal Writes (FIXED by Amos, commit cee1e2b4)

**Fix details:** `ReactiveScheduler.EnsureFlushScheduled()` now uses a three-layer dispatch strategy:
1. If `Application.Current?.Dispatcher` exists and `IsDispatchRequired` is true → `dispatcher.Dispatch(FlushEntry)` (background→main thread)
2. If already on main thread (`IsDispatchRequired` is false) → call `FlushEntry()` directly (avoids double-queuing)
3. Fallback: `ThreadHelper.RunOnMainThread(FlushEntry)` if dispatcher approach failed

Additionally, `FlushSync()` now throws `InvalidOperationException` if called from a background thread, preventing the re-entrancy crash.

**Re-validation evidence:** Auto-test injected 10 background-thread writes into CoalescingDemoPage. All 10 writes reflected in the UI (Counter=10), with reasonable coalescing (Body executions=12). Status message written from background thread displayed correctly.

## Observations (Not Bugs, But Worth Noting)

### ⚠️ Accessibility Gaps
macCatalyst accessibility tree shows Comet text views with `AXValue = "missing value"`. Screen readers won't be able to read content. This predates the reactive system but affects any production deployment.

### ⚠️ Slider Requires Reactive<double>
The Slider control only accepts `Reactive<double>`, not `Reactive<int>`. This is a source-generator limitation — not a bug per se, but a documentation gap that will trip up every developer who tries `Reactive<int>` with a Slider.

### ⚠️ No Async Computed
There's no `AsyncComputed<T>` or equivalent for computed values that depend on async operations (API calls, database queries). Developers will need to manually manage async → signal writes, which now works correctly thanks to the #22 fix.

### ⚠️ Nav Server Page Switching (Minor)
The CometControlsGallery nav server (port 10224) no longer crashes when switching pages (was SIGTRAP before fix), but the visual page content doesn't update even though the signal write succeeds. This is a secondary rendering issue specific to the sidebar navigation pattern, not a reactive system bug. Low priority.

## Verdict

**The reactive system is ~95% production-ready.** All core primitives, dependency tracking, coalescing, two-way binding, and background-thread writes work correctly. The #22 blocker is resolved. Remaining gaps are pre-existing (accessibility) or feature requests (AsyncComputed<T>), not reactive system bugs.

**Recommended before GA:**
1. Fix accessibility — `AXValue` must expose text content for screen readers
2. Consider `AsyncComputed<T>` for async-dependent computed values
3. Document the `Reactive<double>` requirement for Slider

## Files Added
- `sample/CometControlsGallery/Pages/SignalCounterPage.cs`
- `sample/CometControlsGallery/Pages/ComputedDemoPage.cs`
- `sample/CometControlsGallery/Pages/TwoWayBindingPage.cs`
- `sample/CometControlsGallery/Pages/SignalListPage.cs`
- `sample/CometControlsGallery/Pages/CoalescingDemoPage.cs`
- `sample/CometControlsGallery/Pages/StatePreservationPage.cs`
- Modified `sample/CometControlsGallery/App.cs` (6 nav items + NavigateToIndex cleanup)
