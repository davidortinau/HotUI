# Decision: PropertySubscription<T> API Design

**Author:** Holden (Lead Architect)  
**Date:** 2026-07-14  
**Status:** Implemented (Phase 1)  
**Scope:** `src/Comet/Reactive/PropertySubscription.cs`

## Context

Phase 1 of the state unification plan (see `docs/state-unification-analysis.md` §5). PropertySubscription<T> is the new unified reactive property primitive that will eventually replace Binding<T>.

## Decisions

### 1. Synchronous OnDependencyChanged (not deferred)

PropertySubscription re-evaluates immediately when a dependency changes, not via ReactiveScheduler. This preserves the fine-grained, per-property update model: every Signal write → immediate handler update. This matters for animations and slider drags where coalescing would drop intermediate frames.

The `IPropertySubscriptionFlushable` interface is defined for Phase 2, when scheduler-deferred dispatch may be added for thread-safety when Signal.Value is set from background threads.

### 2. Nested ReactiveScope (not Suppress/Resume)

Evaluate() opens a nested ReactiveScope via `BeginTracking()`, which pushes a new scope onto the thread-static stack. This isolates property-level reads from the outer body scope without using the `Suppress()/Resume()` hack that Binding<T> requires. The nested scope captures exactly the dependencies read during the Func evaluation.

### 3. Action<T> for PropertyChangedCallback property

The public property uses `Action<T>?` rather than the custom `PropertyChangedCallback<T>` delegate to keep the API simple and lambda-friendly. The delegate type is defined as a public API for source-generator templates in Phase 2.

### 4. WriteBack as read-only property

Signal-based subscriptions expose `WriteBack` as a get-only auto-property initialized in the constructor. This makes the bidirectional capability discoverable without allowing external mutation of the write-back path.

### 5. SetPropertySubscription disposes old subscription

The `SetPropertySubscription<T>` helper (in DatabindingExtensions) always disposes the previous subscription before assigning the new one. This ensures clean unsubscription from old dependencies during body rebuilds and hot reload.

## Impact on Phase 2-3

- **Phase 2 (Source Generator):** Naomi will generate `PropertySubscription<T>`-based constructors using the three overloads (T, Func<T>, Signal<T>). Property setters will call `SetPropertySubscription`.
- **Phase 3 (Manual Controls):** Handwritten controls (Picker, Image, ListView) will migrate from Binding<T> to PropertySubscription<T>.
- **Phase 4 (Cleanup):** Binding<T>, StateManager, and BindingObject can be deleted once all controls are migrated.
