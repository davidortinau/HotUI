# Decision: State System Unification — Analysis Complete

**Owner:** Naomi (Source Generator Dev)  
**Date:** 2026-03-16  
**Status:** Proposed (awaiting David's review)  
**Type:** Architecture

## Context

Comet runs two parallel state tracking systems:
- **System A (StateManager/Binding)** — fine-grained property tracking via `INotifyPropertyRead` events, static dictionaries, `ReaderWriterLockSlim`, thread-static stacks. ~1,320 lines.
- **System B (ReactiveScope/Signal)** — scope-based dependency tracking via `IReactiveSource` subscriptions, weak references, scheduler batching. ~999 lines.

They're bridged by 29 lines of `ReactiveScope.Suppress()/Resume()`. David confirmed: zero external users, breaking changes acceptable.

## Decision

**Recommend Option C: Unified Reactive System** — extend ReactiveScope/Signal infrastructure with a `PropertySubscription<T>` type that replaces `Binding<T>` for fine-grained property updates.

## Rationale

1. Preserves fine-grained update performance (slider drag at 60fps = O(K) bindings, not O(N) body children)
2. Eliminates ~1,385 lines of legacy infrastructure
3. Hot reload becomes more reliable (reactive subscriptions rebuild naturally; no stale BindingState)
4. Source generator templates simplify (no StateManager.StartProperty/EndProperty calls)
5. Signal<T> can drop dual INotifyPropertyRead implementation
6. One mental model for developers: Signal → Computed → Effect → PropertySubscription

## Alternatives Considered

- **Option A (Pure Rebuild):** Simplest API but performance regression for high-frequency updates (60fps slider). MauiReactor model — Comet should be better.
- **Option B (Pure StateManager):** Moves backwards. Loses the reactive primitive library. State<T> already [Obsolete].
- **Option D (Status Quo):** Unsustainable dual maintenance. Every new feature touches both systems.

## Impact

- 5 files deleted, ~35 files modified, ~47 sample files updated
- Source generator templates change (all 19 generated controls)
- Estimated 5-6 weeks across 5 phases
- Full analysis: `docs/state-unification-analysis.md`

## Next Steps

1. David reviews analysis and approves/rejects direction
2. If approved: Phase 1 — build `PropertySubscription<T>` prototype (1 week)
3. Team review of Phase 1 API before proceeding
