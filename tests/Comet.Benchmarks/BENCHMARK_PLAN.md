# XAML vs MVU (Comet) Benchmark Plan

## Executive Summary

This plan defines benchmarks comparing traditional .NET MAUI XAML (with INotifyPropertyChanged bindings) against Comet's MVU approach (Body() rebuild + diff). Benchmarks run as BenchmarkDotNet console benchmarks targeting `net10.0` — no actual UI rendering required.

**Core architectural tension being measured:**
- XAML: One-time parse cost → targeted property updates via binding engine
- MVU: Zero parse cost → full Body() rebuild → tree diff → handler reuse

---

## Category 1: View Construction / Tree Building

**What we're measuring:** Time and allocations to build a view hierarchy from scratch.
XAML must parse markup and wire bindings; MVU evaluates Body() and sets up StateManager tracking.

### Benchmark 1.1: Flat View Construction
- **XAML baseline:** Programmatically construct a `VerticalStackLayout` with N `Label` controls, each bound to a ViewModel property via `SetBinding()`
- **MVU equivalent:** Create a Comet `View` with Body returning `VStack { Text(...) × N }`
- **Vary N:** 10, 50, 100, 500
- **Metrics:** Time, Gen0/Gen1/Gen2 allocations, allocated bytes
- **Why it matters:** Startup cost — how fast can each approach build a simple page?

### Benchmark 1.2: Deep Nested View Construction
- **XAML baseline:** N levels of nested `VerticalStackLayout > HorizontalStackLayout > Border > Label`
- **MVU equivalent:** N levels of nested `VStack { HStack { View { Text } } }`
- **Vary depth:** 5, 10, 20, 50
- **Metrics:** Time, allocations, stack depth concerns
- **Why it matters:** Complex layouts with deep component hierarchies

### Benchmark 1.3: Mixed Control Types
- **XAML baseline:** Build a realistic "form page" with Label, Entry, Button, Switch, Slider, Picker — each bound to ViewModel properties
- **MVU equivalent:** Same form using Comet Text, TextField, Button, Toggle, Slider
- **Fixed scenario:** 20 controls, 10 bindings
- **Metrics:** Time, allocations
- **Why it matters:** Real-world pages have diverse control types, not just labels

### Benchmark 1.4: State Infrastructure Setup
- **XAML baseline:** Time to create ViewModel with N `INotifyPropertyChanged` properties, call `SetBinding()` for each
- **MVU equivalent:** Time to create View with N `State<T>` fields, call Body() once so StateManager scans fields and registers property-read tracking
- **Vary N:** 5, 20, 50, 100
- **Metrics:** Time, allocations
- **Why it matters:** Isolates the cost of the reactive infrastructure itself, separate from view creation

---

## Category 2: State Change Propagation

**What we're measuring:** Time from state mutation to view tree being updated.
XAML fires `PropertyChanged` → binding engine finds affected controls → updates specific properties.
MVU fires `PropertyChanged` → `StateManager.OnPropertyChanged()` → re-invokes Body() → diffs new tree vs old → calls `UpdateFromOldView()`.

### Benchmark 2.1: Single Property Change
- **XAML baseline:** ViewModel with 1 bound property; change it, measure until `Label.Text` is updated
- **MVU equivalent:** View with 1 `State<string>`; change it, measure until Body() re-evaluates and diff completes
- **Metrics:** Time, allocations per update
- **Why it matters:** The most common operation — one thing changes

### Benchmark 2.2: Multiple Independent Property Changes
- **XAML baseline:** ViewModel with N properties, each bound to a separate Label; change all N in rapid succession
- **MVU equivalent:** View with N State<T> fields; change all N; each triggers a Body() rebuild + diff
- **Vary N:** 1, 5, 10, 20
- **Metrics:** Total time, per-update time, total allocations
- **Why it matters:** MVU may rebuild N times for N changes; XAML updates N bindings independently. Does MVU batch or coalesce?

### Benchmark 2.3: Cascading State Changes
- **XAML baseline:** Changing property A triggers computed property B (which also raises PropertyChanged), which triggers computed C
- **MVU equivalent:** Body() reads stateA, derives B and C inline; single state change triggers one rebuild
- **Chain depth:** 1, 3, 5
- **Metrics:** Time, rebuild count, allocations
- **Why it matters:** Tests whether MVU's "single rebuild" advantage holds vs XAML's cascading binding updates

### Benchmark 2.4: Selective vs Full Rebuild
- **XAML baseline:** 100 Labels bound to 100 properties; change only 1 property
- **MVU equivalent:** Body() builds 100 Text views from 100 State fields; change 1 field → full Body() rebuild + diff of all 100
- **Metrics:** Time, allocations
- **Why it matters:** XAML should win here (targeted update) — quantifies the MVU "rebuild everything" cost

### Benchmark 2.5: State Change with No Visual Effect
- **XAML baseline:** Set a property to its current value (binding engine should no-op via equality check)
- **MVU equivalent:** Set State<T>.Value to same value (SetProperty returns false → no rebuild)
- **Metrics:** Time, allocations
- **Why it matters:** Tests short-circuit paths in both systems

---

## Category 3: Diff Algorithm Performance

**What we're measuring:** The cost of Comet's tree reconciliation algorithm (`DiffUpdate` in `DatabindingExtensions.cs`). This is MVU-only — XAML has no equivalent because it uses targeted binding updates.

### Benchmark 3.1: Identical Trees (No Change)
- Build same Body() twice; diff them
- Vary tree size: 10, 50, 200, 1000 nodes
- **Metrics:** Diff time, allocations during diff
- **Why it matters:** Best-case diff — everything matches. How much overhead does "checking" cost?

### Benchmark 3.2: Single Node Changed
- Rebuild Body() with 1 Text value different out of N nodes
- Vary N: 10, 50, 200
- **Metrics:** Time to find and reconcile the change
- **Why it matters:** Common case — one small change in a large tree

### Benchmark 3.3: Node Insertion / Removal
- Old tree has N children; new tree has N+1 (insertion) or N-1 (removal)
- Tests the lookahead logic in DiffUpdate (lines 168-208 of DatabindingExtensions.cs)
- **Metrics:** Time, whether diff falls through to "Oh Well" break
- **Why it matters:** List operations are common; diff algorithm quality matters here

### Benchmark 3.4: Complete Tree Replacement
- Old tree: VStack with N Text nodes; new tree: VStack with N Button nodes (type mismatch)
- **Metrics:** Time, allocations
- **Why it matters:** Worst-case diff — everything mismatches. Quantifies the "give up" cost

### Benchmark 3.5: Container Diffing with Reordering
- Old tree: items [A, B, C, D, E]; new tree: items [A, C, B, D, E] (swap B and C)
- **Metrics:** Time, handler reuse rate
- **Why it matters:** The current diff algorithm uses simple forward iteration, not keyed reconciliation. This tests its limits.

---

## Category 4: Memory / Allocation Pressure

**What we're measuring:** GC pressure from each approach during normal operations. MVU creates new view objects on every Body() rebuild; XAML reuses objects and only updates property values.

### Benchmark 4.1: Steady-State Allocation Rate
- Perform 1000 state changes on a view with 50 controls
- **XAML baseline:** 1000 PropertyChanged events → binding engine updates
- **MVU equivalent:** 1000 Body() rebuilds → 1000 × 50 new view objects + 1000 diffs
- **Metrics:** Total allocated bytes, Gen0/Gen1/Gen2 collections
- **Why it matters:** The fundamental MVU tradeoff — simplicity vs GC pressure

### Benchmark 4.2: View Object Lifetime
- Create view, trigger 100 updates, measure objects surviving to Gen1+
- **XAML baseline:** Long-lived view objects, short-lived PropertyChangedEventArgs
- **MVU equivalent:** Short-lived view objects (replaced each rebuild), long-lived handlers
- **Metrics:** Gen1/Gen2 promotions, peak working set
- **Why it matters:** Gen1+ GC is expensive; does MVU avoid long-lived garbage?

### Benchmark 4.3: State<T> vs BindableProperty Overhead
- Compare memory cost of N `State<T>` instances vs N `BindableProperty` registrations
- Vary N: 10, 50, 200
- **Metrics:** Bytes per state/property, dictionary overhead
- **Why it matters:** Per-property cost of the reactive infrastructure

### Benchmark 4.4: Disposal and Cleanup
- Create 100 views, trigger updates, then Dispose() all
- **Metrics:** Time to dispose, residual memory after GC, event handler leaks
- **Why it matters:** Memory leaks in binding/state infrastructure are a common real-world issue

---

## Category 5: Large List Performance

**What we're measuring:** How each approach handles data-bound lists — the most performance-sensitive UI pattern.

### Benchmark 5.1: List Construction
- **XAML baseline:** `CollectionView` with `ItemsSource` bound to `ObservableCollection<T>` of N items
- **MVU equivalent:** `ListView<T>(items) { ViewFor = item => ... }` with N items
- **Vary N:** 100, 1000, 10000
- **Metrics:** Time to construct, memory allocated
- **Why it matters:** Initial list render cost

### Benchmark 5.2: Item Addition to Large List
- Start with 1000 items; add 1 item to the end
- **XAML baseline:** `ObservableCollection.Add()` → `CollectionChanged` → add 1 item template
- **MVU equivalent:** State change → Body() rebuild → diff entire ListView → detect 1 new item
- **Metrics:** Time, allocations
- **Why it matters:** XAML should handle incremental changes efficiently; MVU must rebuild the view tree

### Benchmark 5.3: Batch Updates to List
- Start with 1000 items; replace 100 items at random positions
- **XAML baseline:** Multiple `ObservableCollection` operations
- **MVU equivalent:** Replace list, Body() rebuild + diff
- **Metrics:** Time, allocations
- **Why it matters:** Batch updates may favor MVU's "rebuild everything" approach over XAML's per-item updates

### Benchmark 5.4: ViewFor / DataTemplate Cost
- Measure the cost of invoking the item template factory N times
- **XAML baseline:** `DataTemplate` instantiation
- **MVU equivalent:** `ViewFor` delegate invocation
- **Metrics:** Time per template, allocations per template
- **Why it matters:** Template/ViewFor cost dominates large list performance

---

## Category 6: Complex View Tree Depth

**What we're measuring:** How depth affects state propagation, diffing, and memory.

### Benchmark 6.1: Deep Nesting State Propagation
- View with N levels of nesting; state change at root must propagate through
- **XAML baseline:** Binding context inheritance through N levels
- **MVU equivalent:** Body() at root rebuilds N-deep tree + diff
- **Vary depth:** 5, 10, 20, 50
- **Metrics:** Time from state change to leaf update
- **Why it matters:** Deep component trees are common in real apps (navigation stacks, tab views, etc.)

### Benchmark 6.2: Partial Tree Rebuild
- 10-deep tree; state change affects only level 5
- **XAML baseline:** Binding update only at level 5 — parents unaffected
- **MVU equivalent:** Root Body() rebuild, but diff should reuse levels 1-4 and 6-10
- **Metrics:** Nodes rebuilt, time, allocations
- **Why it matters:** Can MVU's diff avoid unnecessary work in deep trees?

### Benchmark 6.3: Handler Reuse in Deep Trees
- MVU-only: After diff, verify handlers are transferred (not recreated) at each level
- **Metrics:** Handler reuse rate, `UpdateFromOldView()` call count
- **Why it matters:** Handler recreation is expensive; diff should preserve them

---

## Category 7: Rapid State Changes (Animation Scenarios)

**What we're measuring:** Throughput under high-frequency updates — e.g., 60fps animation ticking a value.

### Benchmark 7.1: Single Property at 60fps
- Change 1 property 60 times/second for 1 simulated second (60 iterations)
- **XAML baseline:** ViewModel property change → binding update per tick
- **MVU equivalent:** State<T> change → Body() rebuild + diff per tick
- **Metrics:** Average time per frame, worst-case frame time, total allocations
- **Why it matters:** Animations that drive INPC/State changes are common; can MVU keep up?

### Benchmark 7.2: Multiple Properties at 30fps
- Change 5 properties per tick, 30 ticks/second, for 1 simulated second
- **Metrics:** Per-tick time, total allocations, coalescing behavior
- **Why it matters:** Complex animations drive multiple properties simultaneously

### Benchmark 7.3: State Change Throughput (Max Rate)
- No timing constraint — how many state changes per second can each approach process?
- Single property, tight loop, measure total time for 10,000 updates
- **Metrics:** Updates/second, allocations/update
- **Why it matters:** Raw throughput ceiling

### Benchmark 7.4: Coalescing / Batching Behavior
- Fire 100 state changes synchronously before Body() can rebuild
- Does MVU batch these into 1 rebuild or do 100 rebuilds?
- Does XAML's binding engine batch or process each individually?
- **Metrics:** Rebuild count, total time
- **Why it matters:** Determines whether rapid updates cause linear work growth

---

## Category 8: StateManager Infrastructure

**What we're measuring:** Overhead of Comet's StateManager (field scanning, property read tracking, view-to-state mappings). No XAML equivalent — these are MVU-specific costs.

### Benchmark 8.1: State Field Discovery
- `StateManager.ConstructingView()` uses reflection to find `State<T>` fields
- Vary number of State fields: 1, 5, 20, 50
- **Metrics:** Time for reflection scan, allocations
- **Why it matters:** This runs once per view construction; reflection can be slow

### Benchmark 8.2: Property Read Tracking During Build
- Body() with N state reads; StateManager records each via `OnPropertyRead()`
- Vary N: 5, 20, 100
- **Metrics:** Time overhead per read, dictionary operations
- **Why it matters:** Every `state.Value` access during Body() has tracking overhead

### Benchmark 8.3: View-to-State Mapping Lookups
- With N views registered, time to look up which views depend on a given state object
- Vary N views: 10, 100, 500
- **Metrics:** Lookup time in `NotifyToViewMappings`
- **Why it matters:** Scales with app complexity — does lookup degrade with many views?

### Benchmark 8.4: Thread Safety Overhead
- Measure cost of `lock(_lock)` contention in StateManager under concurrent access
- Single-threaded vs 4-thread contention scenarios
- **Metrics:** Time per operation with/without contention
- **Why it matters:** UI thread + background state changes is a real pattern

---

## Category 9: Startup / Initial Render Simulation

**What we're measuring:** Total cost of "create app → build first page → ready for interaction."

### Benchmark 9.1: Simple Page First Render
- **XAML baseline:** Create page with 10 controls + ViewModel + bindings
- **MVU equivalent:** Create View with Body() returning 10 controls + State fields
- **Metrics:** Total time from constructor to `BuiltView != null`
- **Why it matters:** First impression — app launch speed

### Benchmark 9.2: Complex Page First Render
- 50+ controls, nested layouts, list with 20 items, multiple State/binding sources
- **Metrics:** Total time, peak memory during construction
- **Why it matters:** Real apps have complex first pages

### Benchmark 9.3: Handler Registration Cost
- Time to register handler mappings (MAUI `IMauiHandlersCollection`)
- MVU-specific: Time for `UI.Init()` and handler factory setup
- **Metrics:** Time, allocations
- **Why it matters:** One-time cost that affects app startup

---

## Category 10: Real-World Composite Scenarios

**What we're measuring:** End-to-end scenarios that combine multiple categories.

### Benchmark 10.1: Todo App Simulation
- List of 50 items; add item, remove item, toggle completion, edit text
- Perform 100 operations in sequence
- **Metrics:** Total time, total allocations, per-operation averages
- **Why it matters:** Closest to a real app usage pattern

### Benchmark 10.2: Form with Validation
- 10-field form; each keystroke triggers validation on all fields
- 100 simulated keystrokes
- **Metrics:** Per-keystroke time (XAML: 1 binding update + 10 validation checks; MVU: full rebuild + diff)
- **Why it matters:** Forms with cross-field validation are a common MVU concern

### Benchmark 10.3: Master-Detail Navigation
- List page (100 items) → detail page (20 controls) → back → different detail
- 10 navigation cycles
- **Metrics:** Per-navigation time, cumulative memory growth
- **Why it matters:** Tests view construction + disposal + state preservation

---

## Implementation Notes

### Project Setup
```
tests/Comet.Benchmarks/Comet.Benchmarks.csproj
├── BenchmarkDotNet (NuGet)
├── References Comet.dll (same as Comet.Tests)
├── Target: net10.0
├── Uses GenericViewHandler from Comet.Tests for mock rendering
```

### XAML Baseline Approach
Since we can't parse actual XAML in a unit test context, the "XAML baseline" uses **programmatic MAUI** — creating `Microsoft.Maui.Controls` objects directly with `SetBinding()`. This is semantically equivalent to what XAML parsing produces, minus the XAML parse cost itself (which we can note separately).

### Shared Infrastructure
- `TestBase` from Comet.Tests: `InitializeHandlers()`, `UI.Init()`
- `GenericViewHandler`: Mock handler that tracks property changes without platform rendering
- Custom `BenchmarkViewModel : INotifyPropertyChanged` for XAML baselines
- `BenchmarkConfig` class with standard BenchmarkDotNet settings (memory diagnoser, short/medium runs)

### Key Metrics per Benchmark
| Metric | Source | Meaning |
|--------|--------|---------|
| Mean time | BenchmarkDotNet | Average execution time |
| Allocated | MemoryDiagnoser | Total bytes allocated |
| Gen0/Gen1/Gen2 | MemoryDiagnoser | GC collection counts |
| Op/s | BenchmarkDotNet | Operations per second |

### Running
```bash
cd tests/Comet.Benchmarks
dotnet run -c Release -- --filter "*"
```

---

## Priority Order

For maximum developer insight, implement in this order:

1. **Category 2 (State Change Propagation)** — Most impactful daily experience difference
2. **Category 4 (Memory/Allocations)** — MVU's biggest theoretical weakness
3. **Category 1 (View Construction)** — Startup/navigation cost
4. **Category 3 (Diff Algorithm)** — MVU-specific cost center
5. **Category 7 (Rapid State Changes)** — Animation/real-time scenarios
6. **Category 5 (Large Lists)** — Common performance-sensitive pattern
7. **Category 10 (Real-World Composites)** — Validates findings in context
8. **Category 6 (Deep Trees)** — Edge case but important for complex apps
9. **Category 8 (StateManager)** — MVU internals investigation
10. **Category 9 (Startup)** — One-time cost, less impactful

---

## Expected Hypotheses to Validate

| Hypothesis | Expected Result |
|------------|----------------|
| XAML is faster for single-property updates | Yes — targeted binding vs full rebuild |
| MVU is faster for batch/bulk updates | Maybe — single rebuild covers all changes |
| MVU allocates more memory per update | Yes — new view objects each rebuild |
| MVU's diff algorithm is O(n) in tree size | Yes — forward linear scan |
| XAML binding setup cost > MVU State setup | Likely — reflection + BindableProperty overhead |
| Handler reuse makes MVU diff cost tolerable | Key question — the diff must preserve handlers to be viable |
| StateManager reflection cost is amortized | Yes — runs once per view construction |
| Under rapid updates, MVU GC pressure spikes | Likely — 60 rebuilds/sec × N view objects |
| MVU handles cascading changes better | Yes — 1 rebuild vs N PropertyChanged events |
