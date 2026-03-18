# Phase 2: PropertySubscription Wiring Architecture

**Owner:** Holden (Lead Architect)  
**Date:** 2026-03-16  
**Status:** Implemented  

## Decision

PropertySubscription<T> integrates into the existing View lifecycle through three mechanisms:

1. **View._propertySubscriptions** — A `List<IDisposable>` on View that holds attached PropertySubscription instances. Disposed when the View is disposed. This is the ownership model for subscriptions that aren't stored in generated fields (e.g., from extension methods).

2. **View.AttachPropertySubscription<T>()** — Internal method for attaching subscriptions. Calls `BindToView(view, propertyName)` and adds to the disposal list. This is the API that SignalExtensions use, and that Naomi's updated generator templates can use.

3. **SignalExtensions factory methods** — Dual-path construction: controls are created with their existing Signal→Binding constructor (for handler write-back compatibility), AND a PropertySubscription is attached for new reactive tracking. This is a transitional pattern — once the generator emits PropertySubscription fields, the Binding path will be removed.

## Component.SetState() Consolidation

Removed StateManager.BeginBatch/EndBatch and explicit `ThreadHelper.RunOnMainThread(() => Reload())` from `Component<TState>.SetState()`. Now calls only `ReactiveScheduler.MarkViewDirty(this)`. Rationale: the scheduler already coalesces and dispatches to the main thread. The old code was triple-dispatching (batch + mark + explicit reload).

## Context

- SetPropertySubscription<T> was already added in Phase 1 (DatabindingExtensions.cs) — parallel to SetBindingValue<T>.
- ViewPropertyChanged path works identically for both Binding and PropertySubscription: callback → view.ViewPropertyChanged → SetPropertyValue → ViewHandler.UpdateValue.
- The dual-path approach in SignalExtensions ensures backward compatibility during the transition from Binding to PropertySubscription.

## Impact

- Zero new test regressions (963 pass, 18 pre-existing failures).
- Generated controls unchanged — Naomi's generator update will produce PropertySubscription fields in Phase 3.
- Existing Binding-based controls continue to work identically.
