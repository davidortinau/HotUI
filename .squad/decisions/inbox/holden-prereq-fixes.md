# Holden Decision: Skeptic Prereq Fixes — Lock Ordering & CometContext

**Date:** 2026-03-16  
**Owner:** Holden (Lead Architect)  
**Status:** Implemented  
**Commit:** 77d62da5

## Decision

Two skeptic review hard-conditions have been resolved as prerequisites for the state unification proposal:

### 1. Signal<T>.Value setter lock ordering (Skeptic Issue #1)

`_subscribers.NotifyAll(this)` is now called **outside** the `lock(_writeLock)` block. Previously it ran inside the lock, meaning subscriber callbacks executed while the write lock was held. In the unified system, `PropertySubscription.OnDependencyChanged()` callbacks may write to other Signals (two-way binding pattern), creating nested lock acquisition → deadlock.

**Pattern established:** All notifications (NotifyAll, PropertyChanged, diagnostics, scheduler) fire after value mutation is committed and lock is released.

### 2. CometContext.Current replaces StateManager.CurrentContext (Skeptic Issue #3)

New file: `src/Comet/CometContext.cs` — a minimal static class holding `IMauiContext Current`.

- `DatabindingExtensions.AreSameType` now reads `CometContext.Current`
- `CometApp.MauiContext` now reads `CometContext.Current`
- `StateManager.CurrentContext` is a forwarding property (backward compat)
- `StateManager.StartBuilding()` still sets the value (via forwarding)

**Rationale:** When `StateManager.cs` is deleted in unification Phase 4, the diff algorithm's renderer-type comparison would silently degrade — `AreSameType()` would get null context and skip handler-type checks, causing incorrect view reuse. `CometContext.Current` is the survival seam.

## Impact

- Zero test regressions (931 passed, 11 pre-existing failures)
- No API surface changes (forwarding property preserves all existing callers)
- Unblocks unification Phase 4 (StateManager deletion)
