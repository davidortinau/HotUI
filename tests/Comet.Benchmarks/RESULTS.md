# XAML vs MVU (Comet) Benchmark Results

**Environment:** Apple M1, macOS 26.3, .NET 10.0.2, Arm64 RyuJIT AdvSIMD  
**Framework:** BenchmarkDotNet v0.14.0

## Executive Summary

| Category | XAML Advantage | MVU Advantage |
|----------|---------------|---------------|
| View Construction | — | **63-4760x faster**, 50-2000x less memory |
| State Updates (targeted) | **2-25x faster** for rapid property sets | — |
| State Updates (rebuild 1-of-100) | — | **30-38x faster**, 11x less memory |
| Startup (50-control page) | — | **148x faster**, 143x less memory |
| Real-World (Todo/Forms/Dashboard) | — | **40-2000x faster** construction |
| Rapid Animation (5000 iterations) | **2.7x faster**, 13x less alloc | — |
| Diff (1000-node tree) | N/A (no diff needed) | 2.6ms per full diff |

**Key Insight:** MVU dominates view construction and startup. XAML dominates rapid property updates (direct setter vs body() rebuild+diff). For most real-world apps, MVU's construction advantage far outweighs the update cost.

---

## 1. View Construction (Build view hierarchy from scratch)

| Scenario | N | XAML (µs) | MVU (µs) | Speedup | XAML Alloc | MVU Alloc |
|----------|---|-----------|----------|---------|------------|-----------|
| Flat StackLayout + Labels | 10 | 73.0 | 1.15 | **63x** | 84 KB | 1.7 KB |
| Flat StackLayout + Labels | 100 | 759 | 1.20 | **632x** | 810 KB | 1.7 KB |
| Flat StackLayout + Labels | 500 | 5,760 | 1.21 | **4,760x** | 4,033 KB | 1.7 KB |
| Deep nested layouts | 10 | 356 | 1.26 | **283x** | 252 KB | 1.7 KB |
| Deep nested layouts | 50 | 11,194 | 1.54 | **7,268x** | 3,569 KB | 1.7 KB |
| Mixed form controls | 10 | 42.3 | 1.51 | **28x** | 50 KB | 1.7 KB |
| Mixed form controls | 100 | 390 | 1.29 | **302x** | 471 KB | 1.7 KB |
| Mixed form controls | 500 | 2,464 | 1.27 | **1,941x** | 2,336 KB | 1.7 KB |

**MVU wins massively** — Comet views are lightweight wrappers; MAUI controls have heavy initialization (property system, binding infrastructure, visual state managers).

---

## 2. State Change Propagation

| Scenario | Count | XAML (µs) | MVU (µs) | Winner | XAML Alloc | MVU Alloc |
|----------|-------|-----------|----------|--------|------------|-----------|
| Single property update | 1 | 2.06 | 673 | **XAML 327x** | 3.3 KB | 3.2 KB |
| Single property update | 50 | 5.44 | 492 | **XAML 90x** | 3.3 KB | 23.9 KB |
| N independent changes | 1 | 5.73 | 1.44 | **MVU 4x** | 8.1 KB | 2.2 KB |
| N independent changes | 10 | 39.0 | 2.43 | **MVU 16x** | 51 KB | 6.3 KB |
| N independent changes | 50 | 187 | 7.61 | **MVU 25x** | 244 KB | 25 KB |
| No-op (same value) | 50 | 2.96 | 492 | **XAML 166x** | 3.3 KB | 3.9 KB |
| Change 1 of 100 | 1 | 354 | 9.34 | **MVU 38x** | 479 KB | 38 KB |
| Change 1 of 100 | 50 | 348 | 11.4 | **MVU 31x** | 481 KB | 43 KB |

**Note:** MVU "single property update" includes initial handler setup overhead. The "Change 1 of 100" benchmarks show MVU's real advantage — XAML must construct 100 controls upfront while MVU diff is efficient.

---

## 3. Diff Algorithm (MVU-only)

| Scenario | Tree Size | Time (µs) | Allocated |
|----------|-----------|-----------|-----------|
| Identical trees (no change) | 10 | 123 | 42 KB |
| Identical trees (no change) | 200 | 493 | 399 KB |
| Identical trees (no change) | 1000 | 2,682 | 1,894 KB |
| Single node changed | 1000 | 2,569 | 1,896 KB |
| All nodes changed | 1000 | 3,852 | 1,911 KB |
| Append node | 10-1000 | **~2.0** | 2.2 KB |
| Remove node | 10-1000 | **~1.9** | 2.2 KB |
| Toggle subtree | 10-1000 | ~1,100 | 4.4 KB |

**Key finding:** Append/remove operations are **O(1) constant time** (~2µs regardless of tree size). Full tree diffs scale linearly. "All changed" is only ~1.5x slower than "identical" for 1000 nodes.

---

## 4. Rapid Updates (Animation-like workloads)

| Scenario | Iterations | XAML (µs) | MVU (µs) | Winner | XAML Alloc | MVU Alloc |
|----------|------------|-----------|----------|--------|------------|-----------|
| Counter updates | 100 | 9.1 | 156 | **XAML 17x** | 3.3 KB | 44 KB |
| Counter updates | 5000 | 393 | 1,053 | **XAML 2.7x** | 150 KB | 2,036 KB |
| Multi-property (4 props) | 100 | 21.4 | 556 | **XAML 26x** | 15 KB | 168 KB |
| Multi-property (4 props) | 5000 | 836 | 4,481 | **XAML 5.4x** | 590 KB | 8,099 KB |
| String-heavy | 5000 | 692 | 1,390 | **XAML 2.0x** | 1,128 KB | 2,967 KB |

**XAML wins for rapid updates** — direct property setters skip body() rebuild + diff. Gap narrows at scale.

---

## 5. Memory & Startup

| Scenario | Iterations | XAML (µs) | MVU (µs) | Speedup | XAML Alloc | MVU Alloc |
|----------|------------|-----------|----------|---------|------------|-----------|
| Startup (50-control page) | 10 | 1,709 | 11.5 | **MVU 149x** | 2,409 KB | 17 KB |
| Startup (50-control page) | 100 | 17,518 | 118 | **MVU 148x** | 24,090 KB | 168 KB |
| Startup (50-control page) | 1000 | 171,425 | 1,159 | **MVU 148x** | 240,898 KB | 1,743 KB |
| Cascading (3 derived) | 1000 | 297 | 535 | **XAML 1.8x** | 296 KB | 817 KB |

---

## 6. Real-World Scenarios

| Scenario | Items | XAML (µs) | MVU (µs) | Speedup | XAML Alloc | MVU Alloc |
|----------|-------|-----------|----------|---------|------------|-----------|
| Todo list build + mutations | 10 | 127 | 2.32 | **MVU 55x** | 150 KB | 6.4 KB |
| Todo list build + mutations | 100 | 1,456 | 12.1 | **MVU 120x** | 1,468 KB | 48 KB |
| Form build + validation | 10 | 78.1 | 3.01 | **MVU 26x** | 103 KB | 9.0 KB |
| Form build + validation | 100 | 844 | 19.2 | **MVU 44x** | 999 KB | 74 KB |
| Dashboard build | 10 | 235 | 1.15 | **MVU 204x** | 259 KB | 1.7 KB |
| Dashboard build | 100 | 2,257 | 1.16 | **MVU 1,946x** | 2,056 KB | 1.7 KB |

---

## 7. Edge Cases

| Scenario | Size | XAML (µs) | MVU (µs) | Winner |
|----------|------|-----------|----------|--------|
| Wide tree (N children) | 10 | 169 | 1.14 | **MVU 148x** |
| Wide tree (N children) | 200 | 4,440 | 1.16 | **MVU 3,828x** |
| Collection churn | 200 | 1,720 | 227 | **MVU 7.6x** |
| N independent states | 200 | N/A | 28.7 | 95 KB alloc |
| View type changes | 200 | N/A | 109 | 86 KB alloc |

---

## Conclusions

### When to Choose MVU (Comet)
- **Page-heavy apps** with many screens (startup is 148x faster)
- **Form-heavy apps** with complex layouts (construction dominates)
- **Apps with conditional rendering** (show/hide patterns are natural in C#)
- **Apps where state changes affect many related views** (1-of-100: MVU 30x faster)

### When to Choose XAML
- **Animation-heavy apps** with high-frequency property updates (2-5x faster)
- **Apps with many cascading computed properties**
- **Apps where individual properties change independently at high rates**

### Architecture Recommendation
For most real-world .NET MAUI applications, MVU's view construction advantage (50-5000x) vastly outweighs XAML's update advantage (2-5x for animations). The typical app spends more time building and navigating pages than animating individual properties at 60fps.
