# Decision: ReactiveScope Nesting — GO

**Owner:** Amos (Controls & API Dev)
**Date:** 2025-07-24
**Status:** Validated
**Verdict:** ✅ GO — ReactiveScope handles nested scopes correctly

## Context

The state unification proposal requires `PropertySubscription<T>` to call `ReactiveScope.BeginTracking()` INSIDE an already-active body scope. The skeptic review flagged that `ReactiveScope.Dispose()` only restores `_previous` if `_current == this` — raising concern that nested scopes could silently fail.

## What Was Tested

6 tests in `tests/Comet.Tests/ReactiveTests/ScopeNestingTests.cs`:

1. **Sequential nesting** (Body A → nested B → nested C): Each scope captures only its own Signal reads. No leakage. ✅
2. **Triple nesting** (A → B → C deep): `_previous` chain restores correctly through 3 levels. ✅
3. **Inner throw recovery**: When B's Func throws, `using` disposes B, restoring A. Body tracking continues. ✅
4. **Suppress inside nested scope**: ProcessGetFunc's Suppress/Resume pattern works correctly within a scope. ✅
5. **Suppress with exception + try/finally**: The new Binding.cs fix keeps scopes clean when Func throws during Suppress. ✅
6. **Out-of-order dispose**: Documented as a logic error (not a real-world scenario). B.Dispose() restores B._previous even if A was already disposed. Not harmful but documented.

## Key Finding

ReactiveScope's `_previous` linked-list pattern is a textbook scope stack. As long as scopes are disposed in LIFO order (guaranteed by `using` statements), nesting works perfectly. The `if (_current == this)` guard in `Dispose()` is actually a safety net against double-dispose, not a nesting hazard.

## What This Means for Unification

`PropertySubscription<T>` can safely call `BeginTracking()` inside a body scope. The pattern is:

```
Body scope A begins (BeginTracking)
  → PropertySubscription evaluates (BeginTracking B, nested)
  → B captures its reads
  → B disposes (restores A)
  → More PropertySubscriptions...
Body scope A ends (captures body-level reads)
```

This is exactly what the tests prove works.

## Also Fixed

Binding.cs `ProcessGetFunc()` and `implicit operator Binding<T>(State<T>)` now wrap Suppress/Resume in try/finally, closing skeptic issue #2. If `Get.Invoke()` throws, `ReactiveScope._current` is properly restored.

## Impact

- No changes to ReactiveScope itself required
- Binding.cs try/finally is a pure safety fix (no behavior change on success path)
- 3 pre-existing BindingTests failures are unrelated (StateManager.CurrentView is null in test harness)
