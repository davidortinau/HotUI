# Decision: Phase 3b — ReactiveScheduler replaces StateManager batching in core infrastructure

**Author:** Holden (Lead Architect)
**Date:** 2025-07-17
**Status:** Implemented

## Context

`View.BindingPropertyChanged` was the central dispatch point for property change notifications from the old Binding<T> system. It used `StateManager.IsBatching` and `StateManager.AddViewNeedingReload` to defer view rebuilds during batch operations. This was StateManager's manual coalescing mechanism — a bespoke solution for what `ReactiveScheduler` does automatically via its flush loop.

## Decision

1. **BindingPropertyChanged now routes through ReactiveScheduler.MarkViewDirty** instead of StateManager's manual batching. The scheduler's dirty-set + flush-on-main-thread pattern provides the same coalescing behavior.

2. **EnvironmentData hooks into both systems during transition.** `CallPropertyRead` calls `ReactiveEnvironment.TrackRead` (for ReactiveScope tracking) alongside `StateManager.OnPropertyRead` (for Binding<T> backward compat). `SetValue` similarly calls both. This dual-path ensures controls using either system work correctly.

3. **View.Dispose now calls StateManager.Disposing(this)** to clean up the view-object mappings that `ConstructingView` created. This was a missing cleanup step.

4. **BindingState, GetState(), and StateManager.ConstructingView are retained** for backward compat with Binding<T>-based controls. Phase 4 removes them.

## Consequences

- Old Binding<T>-based controls continue to work — their property changes still flow through BindingPropertyChanged → BindingState.UpdateValue.
- New PropertySubscription<T>-based controls bypass BindingPropertyChanged entirely — they use direct Signal → PropertySubscription → ViewPropertyChanged.
- Environment changes now automatically trigger rebuilds for views that read environment keys during body evaluation (via ReactiveScope tracking), even without explicit `StateManager.ListenToEnvironment` registration.
- StateManager.BeginBatch/EndBatch still works for external callers (e.g., benchmarks), but the view-reload path no longer checks IsBatching.
