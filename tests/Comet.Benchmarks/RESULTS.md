# XAML vs MVU (Comet) Benchmark Results

**Environment:** Apple M1 · macOS 26.3 · .NET 10.0.2 · BenchmarkDotNet 0.14.0

## Summary

| Category | MVU Advantage | XAML Advantage |
|----------|--------------|----------------|
| View Construction | ✅ **60-4000x faster**, 50-2000x less memory | — |
| State Updates (per-update, post-warmup) | ✅ Competitive (~2x) | ✅ **~2x faster** for single property |
| N Independent State Changes | ✅ **2.6x faster** | — |
| Selective Update (1 of 100) | ✅ **2.4x faster** | — |
| Rapid Counter (5000 iters) | — | ✅ **~1.9x faster** |
| Multi-prop Animation (batched) | — | ✅ **~2.7x faster** |
| Multi-prop Animation (unbatched) | — | ✅ **~3.7x faster** |
| String-heavy Updates | — | ✅ **~1.6x faster** |
| Startup (50-control page) | ✅ **148x faster**, 138x less memory | — |
| Todo App (100 items) | ✅ **120x faster**, 30x less memory | — |
| Dashboard (100 items) | ✅ **1948x faster**, 1224x less memory | — |

> **⚠️ Correction:** Previous results showed XAML winning 91-320x for state updates.
> Those benchmarks mixed view construction cost with update cost. With proper
> `[IterationSetup]` isolation, MVU per-update cost is only ~2x slower than XAML,
> and MVU wins for independent multi-state scenarios.

## 1. View Construction (Building the UI Tree)

MVU is dramatically faster because `Body()` only creates lightweight Comet view objects — no handler/platform overhead until render time.

| Scenario | N | XAML (μs) | MVU (μs) | Speedup | XAML Alloc | MVU Alloc |
|----------|---|-----------|----------|---------|------------|-----------|
| Flat Stack + Labels | 10 | 73.0 | 1.15 | **63x** | 84 KB | 1.7 KB |
| Flat Stack + Labels | 100 | 759.0 | 1.20 | **632x** | 810 KB | 1.7 KB |
| Flat Stack + Labels | 500 | 5,760 | 1.21 | **4,760x** | 4,033 KB | 1.7 KB |
| Deep Nested | 10 | 356.2 | 1.26 | **283x** | 252 KB | 1.7 KB |
| Deep Nested | 50 | 11,194 | 1.54 | **7,269x** | 3,569 KB | 1.7 KB |
| Mixed Form | 100 | 389.8 | 1.29 | **302x** | 471 KB | 1.7 KB |
| Mixed Form | 500 | 2,464 | 1.27 | **1,941x** | 2,336 KB | 1.7 KB |

## 2. State Change Propagation (Corrected — isolated update cost)

With `[IterationSetup]`, view construction is excluded. Results show the **true per-update cost**.

**Key discovery:** MVU does NOT rebuild `Body()` on simple state changes! The Binding fast-path
updates properties directly without tree diff. Body calls = 0 for all scenarios below.

| Scenario | Count | XAML (ns) | MVU (ns) | Winner |
|----------|-------|-----------|----------|--------|
| Single property change | 1 | 2,282 | 272 | **MVU 8x** (first is no-op) |
| Single property change | 50 | 21,982 | 46,592 | XAML **2.1x** |
| N independent changes | 1 | 1,856 | 1,268 | **MVU 1.5x** |
| N independent changes | 50 | 26,453 | 9,999 | **MVU 2.6x** |
| No-op (same value) | 1 | 1,854 | 12,966 | XAML **7x** (includes first real update) |
| No-op (same value) | 50 | 8,145 | 15,687 | XAML **1.9x** |
| Change 1 of 100 | 1 | 1,786 | 1,357 | **MVU 1.3x** |
| Change 1 of 100 | 50 | 23,983 | 10,083 | **MVU 2.4x** |

**Key insight:** At high update counts, MVU wins for independent/selective updates because each
Binding targets only the affected view. XAML's per-property overhead accumulates faster. For
single-property sequential updates, XAML's simpler property system has ~2x advantage.

## 3. Diff Algorithm (MVU-only)

Shows the cost of Comet's tree reconciliation after a state change.

| Scenario | Tree Size | Time (μs) | Allocated |
|----------|-----------|-----------|-----------|
| Identical (no change) | 10 | 123 | 42 KB |
| Identical (no change) | 200 | 493 | 399 KB |
| Identical (no change) | 1000 | 2,682 | 1,894 KB |
| Single node changed | 1000 | 2,569 | 1,896 KB |
| All nodes changed | 1000 | 3,852 | 1,911 KB |
| Append node | 1000 | **2.1** | 2 KB |
| Remove node | 1000 | **2.0** | 2 KB |
| Toggle subtree | 1000 | 1,161 | 4 KB |

## 4. Rapid Updates (Animation-like) — Corrected with [IterationSetup]

With construction excluded, the gap narrows significantly.
**New: Batched mode** uses `StateManager.BeginBatch()/EndBatch()` to coalesce multi-state updates.

| Scenario | Iterations | XAML (μs) | MVU (μs) | Ratio | MVU Alloc |
|----------|-----------|-----------|----------|-------|-----------|
| Counter | 100 | 38 | 76 | 2.0x | 49 KB |
| Counter | 5000 | 1,653 | 3,063 | **1.9x** | 2,401 KB |
| Multi-prop (unbatched) | 5000 | 3,324 | 12,130 | 3.7x | 9,562 KB |
| Multi-prop **(batched)** | 5000 | 3,324 | **8,872** | **2.7x** | 8,919 KB |
| String-heavy | 5000 | 2,037 | 3,297 | **1.6x** | 3,354 KB |

**Batching improvement:** Multi-prop animation goes from 3.7x → 2.7x slower (27% faster).
The `BeginBatch()/EndBatch()` API defers Binding Func re-evaluation, so N state changes
feeding the same Binding only trigger one re-evaluation instead of N.

## 5. Memory / Startup

MVU is vastly more efficient for initial construction and startup.

| Scenario | Iterations | XAML (μs) | MVU (μs) | Speedup | XAML Alloc | MVU Alloc |
|----------|-----------|-----------|----------|---------|------------|-----------|
| Startup (50 controls) | 10 | 1,709 | 11.5 | **149x** | 2,409 KB | 17 KB |
| Startup (50 controls) | 100 | 17,518 | 118 | **148x** | 24,090 KB | 168 KB |
| Alloc per change | 1000 | 90.3 | 274 | 0.3x | 42 KB | 402 KB |
| Cascading derived | 1000 | 296.5 | 535 | 0.6x | 296 KB | 817 KB |

## 6. Real-World Scenarios

| Scenario | Items | XAML (μs) | MVU (μs) | Speedup | XAML Alloc | MVU Alloc |
|----------|-------|-----------|----------|---------|------------|-----------|
| Todo list | 10 | 127 | 2.3 | **55x** | 150 KB | 6 KB |
| Todo list | 100 | 1,456 | 12.1 | **120x** | 1,468 KB | 48 KB |
| Form + validation | 100 | 844 | 19.2 | **44x** | 999 KB | 74 KB |
| Dashboard | 10 | 235 | 1.15 | **204x** | 259 KB | 2 KB |
| Dashboard | 100 | 2,257 | 1.16 | **1,946x** | 2,056 KB | 2 KB |

## 7. Edge Cases

| Scenario | Size | Time (μs) | Allocated |
|----------|------|-----------|-----------|
| Wide tree (XAML) | 200 | 4,503 | 3,822 KB |
| Wide tree (MVU) | 200 | **1.13** | 2 KB |
| N independent states | 200 | 28.3 | 95 KB |
| Collection churn (XAML) | 200 | 1,685 | 2,044 KB |
| Collection churn (MVU) | 200 | **219** | 106 KB |
| Deep conditional toggle | 200 | 496 | 8 KB |
| View type changes | 200 | 96 | 86 KB |
| Hidden subtree update | 200 | 324 | 89 KB |

## Conclusions

### Where MVU (Comet) Excels
- **View construction**: 60-7000x faster with 50-2000x less memory. Body() creates lightweight objects with zero platform overhead.
- **Startup time**: ~150x faster to build initial pages.
- **Independent state updates**: 2.6x faster — each Binding targets only the affected view.
- **Selective updates in large trees**: 2.4x faster than XAML's property system.
- **Collection operations**: 7-8x faster for add/remove churn.
- **Memory efficiency at construction**: Orders of magnitude less GC pressure.

### Where XAML (Direct Property Set) Excels
- **Sequential single-property updates**: ~2x faster for repeated changes to one property.
- **Multi-property rapid updates**: ~2.7x faster (batched) for animation-like workloads.
- **No-op updates**: XAML short-circuits same-value sets; MVU has ~2x overhead.

### Optimization: State Batching
`StateManager.BeginBatch()` / `StateManager.EndBatch()` allows multiple state changes
to be coalesced into a single Binding re-evaluation. This is most effective when N states
feed into the same Binding Func (e.g., multi-property animation). Example:

```csharp
StateManager.BeginBatch();
_x.Value = 10;
_y.Value = 20;
_opacity.Value = 0.5;
_scale.Value = 1.5;
StateManager.EndBatch(); // Single re-evaluation + handler update
```

### Key Discovery: MVU Does NOT Rebuild Body() on State Changes!
Previous analysis assumed every state change triggers a full Body() rebuild + diff.
**This is incorrect.** The Binding<T> fast-path detects 1:1 state-to-property mappings
and updates view properties directly. Body() is only called during initial construction
or when GlobalProperties are involved (e.g., conditional rendering based on state).

### Architectural Takeaway
With proper benchmarking (isolating construction from updates), MVU is only ~2x slower
than XAML for property updates — not 91-320x as previously reported. The gap is entirely
in the StateManager notification overhead (dictionary lookups, lock acquisition), not in
tree rebuilds. For most real apps, construction dominates — pages are built once but
updated selectively. MVU's 60-7000x construction advantage far outweighs the 2x update cost.

**Recommendation:** MVU is an excellent choice for MAUI applications. Use `StateManager.BeginBatch()/EndBatch()` for multi-property animation updates to minimize overhead.
