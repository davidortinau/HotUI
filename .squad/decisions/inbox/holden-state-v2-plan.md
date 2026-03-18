# Decision: State Management v2 — Phased Implementation Plan

**Owner:** Holden (Lead Architect)
**Date:** 2025-07-24
**Status:** Planned

## Decision

Implement the Signal<T>/Computed<T>/Effect reactive system as described in `docs/state-management-proposal.md` (Rev 4) using a three-phase migration approach with strict dependency ordering. All work tracked on branch `squad/state-management-v2` with 21 GitHub issues.

## Phased Approach

### Phase 0 — Infrastructure (issues #1–#5)
Foundation that everything else depends on. Must be completed first.

| Issue | Title | Owner |
|-------|-------|-------|
| #1 | Core reactive primitives — Signal<T>, Computed<T>, Effect | Holden |
| #2 | Reactive infrastructure — IReactiveSource, ReactiveScope, SubscriberList | Holden |
| #3 | ReactiveScheduler — microtask coalescing with flush depth guard | Holden |
| #4 | ReactiveDiagnostics with IDisposable subscriptions | Holden |
| #5 | Unit tests for reactive primitives | Bobbie |

**Dependency chain:** #1 + #2 can be parallel → #3 depends on #1 + #2 → #4 depends on #1 + #3 → #5 depends on all.

### Phase 1 — Add Signal<T> alongside State<T> (issues #6–#14)
Non-breaking additions. Existing State<T> code continues to work unchanged.

| Issue | Title | Owner |
|-------|-------|-------|
| #6 | Signal<T> backward compatibility bridge — INotifyPropertyRead | Holden |
| #7 | View integration — BodyDependencySubscriber | Holden |
| #8 | ReactiveEnvironment — per-key sources | Holden |
| #9 | SignalList<T> — reactive observable list | Amos |
| #10 | Source generator — Signal<T>/Func<T>/Computed<T> overloads | Naomi |
| #11 | StateManager bridge — recognize Signal<T> fields | Holden |
| #12 | Hot reload — TransferHotReloadStateToCore for signals | Holden |
| #13 | Component<TState> — wire SetState through ReactiveScheduler | Holden |
| #14 | Integration tests | Bobbie |

**Key dependencies:**
- #6 depends on #1, #2 (bridge needs core types)
- #7 depends on #1, #2, #3 (view integration needs scheduler)
- #8 depends on #2, #7 (environment needs scope + view integration)
- #9 depends on #2, #3 (SignalList needs scope + scheduler)
- #10 depends on #1 (generator needs Signal/Computed types)
- #11 depends on #6 (StateManager needs bridge)
- #12 depends on #1, #7 (hot reload needs signals + body subscriber)
- #13 depends on #3, #7 (Component needs scheduler + view integration)
- #14 depends on all Phase 1 issues

### Phase 2 — Deprecation (issues #15–#17)
Warnings and tooling to guide migration. Only after Phase 1 is stable.

| Issue | Title | Owner |
|-------|-------|-------|
| #15 | Deprecate implicit T→Binding<T> and State<T> | Amos |
| #16 | Roslyn analyzers — COMET001/002/003 | Naomi |
| #17 | comet-migrate dotnet tool | Naomi |

### Phase 3 — Removal (issues #18–#19)
Major version bump. Only after sufficient adoption period.

| Issue | Title | Owner |
|-------|-------|-------|
| #18 | Remove State<T>, Binding<T>, StateManager | Holden |
| #19 | Remove INotifyPropertyRead bridge from Signal<T> | Holden |

### Cross-cutting (issues #20–#21)
Can start once Phase 1 is complete.

| Issue | Title | Owner |
|-------|-------|-------|
| #20 | Performance benchmark suite | Bobbie |
| #21 | Migrate Comet.Sample to Signal<T> | Amos |

## Branch

`squad/state-management-v2` — branched from `squad/comet-mvu-evolution` at commit `50d5a701`.

## Key Constraints

1. **Phase 0 must complete before Phase 1 starts** — all primitives and infrastructure must be solid before integration.
2. **Phase 1 is non-breaking** — existing State<T> code works unchanged. Signal<T> is additive.
3. **Phase 2 requires Phase 1 stability** — don't deprecate until the replacement is proven.
4. **Phase 3 is a major version** — removal happens only after the deprecation period.
5. **Existing test suite must remain green** throughout all phases.

## Context

The proposal went through 4 revisions with 3 skeptic reviews and an architect review. All critical and high findings have been addressed in Rev 4. The design is based on the signals model (SolidJS, Preact Signals, Angular Signals) adapted for .NET and Comet's architecture. Key improvements over the current State<T>/Binding<T> system: elimination of the implicit conversion trap, automatic microtask coalescing, thread-safe Signal<T> with StrongBox<T>, compile-time enforcement via Roslyn analyzers, and per-key environment reactivity.
