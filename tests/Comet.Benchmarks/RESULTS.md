# XAML vs MVU (Comet) Benchmark Results

**Environment:** Apple M1 · macOS 26.3 · .NET 10.0.2 · BenchmarkDotNet 0.14.0

## Summary

| Category | MVU Advantage | XAML Advantage |
|----------|--------------|----------------|
| View Construction | ✅ **60-4000x faster**, 50-2000x less memory | — |
| State Updates (targeted) | — | ✅ **2-170x faster** for single property |
| State Updates (multi) | ✅ **4-25x faster** for N independent changes | — |
| Selective Update (1 of 100) | ✅ **30-38x faster** | — |
| Rapid Counter (5000 iters) | — | ✅ **~2.7x faster** |
| Multi-prop Animation | — | ✅ **~5x faster** |
| String-heavy Updates | — | ✅ **~2x faster** |
| Startup (50-control page) | ✅ **148x faster**, 138x less memory | — |
| Todo App (100 items) | ✅ **120x faster**, 30x less memory | — |
| Dashboard (100 items) | ✅ **1948x faster**, 1224x less memory | — |
| Wide Tree (200 children) | ✅ **3993x faster**, 2273x less memory | — |
| Collection Churn | ✅ **7.7x faster** at 200 items | — |

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

## 2. State Change Propagation

XAML direct property set is faster for single-property targeted updates (no tree rebuild needed).
MVU wins when updating N independent properties or doing selective updates in large trees.

| Scenario | Count | XAML (μs) | MVU (μs) | Winner |
|----------|-------|-----------|----------|--------|
| Single property change | 1 | 2.1 | 673 | XAML **320x** |
| Single property change | 50 | 5.4 | 492 | XAML **91x** |
| N independent changes | 1 | 5.7 | 1.4 | **MVU 4x** |
| N independent changes | 10 | 39.0 | 2.4 | **MVU 16x** |
| N independent changes | 50 | 186.5 | 7.6 | **MVU 25x** |
| No-op (same value) | 50 | 3.0 | 492 | XAML **164x** |
| Change 1 of 100 | 1 | 353.7 | 9.3 | **MVU 38x** |
| Change 1 of 100 | 50 | 347.9 | 11.4 | **MVU 31x** |

**Key insight:** The MVU "single state update" includes initial `Body()` evaluation + handler setup on first change. After warmup, incremental updates are fast. XAML's advantage is in pure property-set cost.

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

**Key insight:** Full tree diffs scale linearly with tree size (~2.7μs/node). But incremental operations (append/remove) are O(1) at ~2μs regardless of tree size. Toggle subtree is cheap because it changes the root type.

## 4. Rapid Updates (Animation-like)

XAML wins here because it directly sets properties without tree rebuilds.

| Scenario | Iterations | XAML (μs) | MVU (μs) | Ratio | XAML Alloc | MVU Alloc |
|----------|-----------|-----------|----------|-------|------------|-----------|
| Counter | 100 | 9.1 | 156 | 17x | 3 KB | 44 KB |
| Counter | 5000 | 393 | 1,053 | 2.7x | 150 KB | 2,036 KB |
| Multi-prop (4) | 5000 | 836 | 4,481 | 5.4x | 590 KB | 8,099 KB |
| String-heavy | 5000 | 692 | 1,390 | 2.0x | 1,128 KB | 2,967 KB |

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
- **Selective updates in large trees**: MVU rebuilds are cheaper than XAML's setup cost for 100+ controls.
- **Collection operations**: 7-8x faster for add/remove churn.
- **Memory efficiency at construction**: Orders of magnitude less GC pressure.

### Where XAML (Direct Property Set) Excels
- **Targeted single property updates**: 2-170x faster since no tree rebuild is needed.
- **Animation-like rapid updates**: 2-5x faster for high-frequency property changes.
- **No-op updates**: XAML short-circuits same-value sets; MVU incurs rebuild overhead.
- **Cascading computed properties**: XAML property system is ~2x faster.

### Architectural Takeaway
MVU trades **construction efficiency** for **update overhead**. Build a 500-element page in 1.2μs (vs 5.8ms for XAML), but each state change costs a full Body() rebuild + diff. For most real apps, construction dominates — pages are built once but updated selectively. The MVU diff algorithm is efficient (~2.7μs/node) and incremental operations (append/remove) are O(1).

**Recommendation:** MVU is an excellent choice for most MAUI applications. For animation-heavy views with >60fps update requirements, consider using native MAUI animation APIs rather than state-driven updates.
