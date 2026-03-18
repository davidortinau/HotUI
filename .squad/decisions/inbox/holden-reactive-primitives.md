# Decision: Phase 0 Reactive Primitives Implementation

**Owner:** Holden (Lead Architect)
**Date:** 2026-03-12
**Status:** Implemented

## Decision

Implement the Phase 0 reactive primitives from `docs/state-management-proposal.md` Rev 4 in `src/Comet/Reactive/`: Signal<T>, Computed<T>, Effect, plus the supporting reactive infrastructure types (IReactiveSource, IReactiveSubscriber, ReactiveScope, SubscriberList).

## Rationale

Signal<T> needs StrongBox-backed atomic reads, per-signal write locks, and versioning to avoid torn reads and missed notifications. Computed<T> and Effect require diff-based dependency updates with exception-safe tracking to prevent gaps during evaluation and to avoid wedging on failures.

## Notes

- Signal<T> and Computed<T> propagate changes via SubscriberList and schedule flushes via ReactiveScheduler.
- Added minimal ReactiveScheduler/ReactiveDiagnostics stubs to keep the core primitives compilable until issues #3 and #4 land.

## Impact

Core primitives are now in place as an additive layer. Integration with Views, scheduler microtask coalescing, diagnostics subscriptions, and tests remain in subsequent Phase 0 issues.
