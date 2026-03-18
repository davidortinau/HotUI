# Phase 6.2 Interop Tests — Baseline Analysis & Test Roadmap

**Date:** 2026-03-08  
**Phase:** 6.2 (Bobbie — Anticipatory Interop Tests)  
**Status:** Ready for test development (Amos's NativeHost in 6.1 not a blocker)

---

## Quick Reference: Existing Interop Foundation

### Two-Way Bridge Pattern
- **MauiViewHost** (Comet→MAUI): Embed `Microsoft.Maui.Controls` views inside Comet MVU tree
- **CometHost** (MAUI→Comet): Embed Comet views inside MAUI pages/controls

### Key Source Files
| File | Purpose |
|------|---------|
| `/src/Comet/Controls/MauiViewHost.cs` | Comet wrapper for MAUI views; factory + lazy loading |
| `/src/Comet/Controls/CometHost.cs` | MAUI Control that hosts Comet View; BindableProperty pattern |
| `/src/Comet/Handlers/MauiViewHost/*` | Platform handlers (iOS/Android/Windows) |
| `/src/Comet/Handlers/CometHost/*` | Platform handlers (iOS/Android/Windows) |
| `/src/Comet/Controls/View.cs` (lines 309-360) | GetView()/GetRenderView() rendering pipeline |
| `/src/Comet/AppHostBuilderExtensions.cs` (lines 540-648) | Handler registration & context fallback |

---

## What's Testable NOW (Phase 6.2 Scope)

### ✅ MauiViewHost Tests
- Constructor patterns (direct + factory)
- Lazy factory deferred creation & thread safety
- Size measurement (GetDesiredSize with frame constraints)
- Parent/child relationships & layout integration
- Disposal & cleanup (IDisposable handling)
- Nested hosts (host within host)

**Current:** 9 tests in `MauiViewHostTests.cs`  
**Extend to:** ~25 tests (add 16 more)

### ✅ CometHost Tests (NEW)
- BindableProperty binding & swapping
- Constructor patterns (with/without view)
- Integration as MAUI ContentPage child
- Property changed events

**Create:** `CometHostTests.cs` (15-20 tests)

### ✅ View Rendering Pipeline Tests
- View.GetView() body resolution
- BuiltView caching & idempotency
- Hot-reload replacement flow
- Parent/state preservation across builds

**Current:** 10 tests in `ViewGetViewTests.cs`  
**Extend to:** ~20 tests

### ✅ Handler Lifecycle Tests
- Handler discovery & initialization
- SetVirtualView() behavior
- DisconnectHandler() cleanup
- Property mapper chaining
- **Pattern:** Use `GenericViewHandler` mock (no platform views)

**Create:** `InteropContextTests.cs` (10-15 tests)

### ✅ Layout & Measurement Tests
- Measure/Arrange with frame constraints
- Integration in layouts (VStack, HStack, Grid)
- Margin adjustment
- Nested interop measurement propagation

**Create:** `InteropLayoutTests.cs` (20-30 tests)

---

## What's BLOCKED on Amos (Phase 6.1)

❌ **Native platform view embedding**
- Android View, UIView, FrameworkElement direct embedding
- Requires NativeHost interface design
- Requires ToPlatform() integration for native types

❌ **Platform handler implementation tests**
- CreatePlatformView() mechanics
- LayoutSubviews/OnLayout/ArrangeOverride behavior
- Container view type specifics (UIView vs FrameLayout vs Canvas)

❌ **Property synchronization (managed→native)**
- Property mapper definitions
- Data binding back to native controls

❌ **Full integration tests with real platform views**
- Requires handlers to actually connect
- Requires native view creation

---

## Safe Extensions for Existing Tests

### Option A: Extend `MauiViewHostTests.cs` (9 → 25 tests)
```csharp
[Fact] public void MauiViewHost_Factory_ThreadSafety() { }
[Fact] public void MauiViewHost_GetDesiredSize_WithMargin() { }
[Fact] public void MauiViewHost_InHStack_RespondsToSpacing() { }
[Fact] public void MauiViewHost_InGrid_Stretches() { }
[Fact] public void MauiViewHost_NestedInHost() { }
[Fact] public void MauiViewHost_Dispose_MultipleCallsSafe() { }
[Fact] public void MauiViewHost_Factory_ExceptionHandling() { }
// ... ~10 more
```

### Option B: Extend `ViewGetViewTests.cs` (10 → 20 tests)
```csharp
[Fact] public void CometHost_GetView_ReturnsSelf() { }
[Fact] public void CometHost_ReplaceableView_Behavior() { }
[Fact] public void GetView_HotReload_StatePreservation() { }
// ... ~7 more
```

### Option C: Create New Test Files

#### Create `CometHostTests.cs`
```csharp
public class CometHostTests : TestBase
{
    [Fact] public void Constructor_WithView_SetsCometView() { }
    [Fact] public void CometView_CanBeSwapped() { }
    [Fact] public void CometView_BindableProperty_Works() { }
    [Fact] public void CometHost_AsChildOfStackLayout() { }
    // ... ~16 more
}
```

#### Create `InteropContextTests.cs`
```csharp
public class InteropContextTests : TestBase
{
    [Fact] public void MauiViewHost_ToPlatform_FallsBackToAppContext() { }
    [Fact] public void HandlerLookup_ThirdPartyControl_Resolves() { }
    [Fact] public void MauiContext_Fallback_PrefersPrimaryContext() { }
    // ... ~12 more
}
```

#### Create `InteropLayoutTests.cs`
```csharp
public class InteropLayoutTests : TestBase
{
    [Fact] public void MauiViewHost_Measure_RespectsAvailableSize() { }
    [Fact] public void MauiViewHost_InVStack_RespondsToSpacing() { }
    [Fact] public void CometHost_FillsAvailableSpace() { }
    [Fact] public void NestedInterop_Measure_PropagatesConstraints() { }
    // ... ~26 more
}
```

---

## Test Infrastructure (Ready to Use)

### Base Class
- **File:** `/tests/Comet.Tests/TestBase.cs`
- **Use:** `public TestBase()` calls `UI.Init()` per test
- **Reset:** Call `TestBase.ResetComet()` between tests that modify global state

### Handler Mock
- **File:** `/tests/Comet.Tests/Handlers/GenericViewHandler.cs`
- **Pattern:** Tracks property changes without platform view creation
- **Use:** `view.SetViewHandlerToGeneric()` (in ViewExtensions.cs)

### Mock IView
- **Pattern:** Implement 30+ IView properties with minimal defaults
- **Reuse:** TestIView in MauiViewHostTests (copy for new tests)
- **Size:** Fixed Measure() return (e.g., `new Size(100, 40)`)

### Helper Extension
- **File:** `/tests/Comet.Tests/Helpers/ViewExtensions.cs`
- **Use:** `SetViewHandlerToGeneric()` to avoid platform-specific setup

---

## Handler Architecture (Reference)

### Base Pattern (All Handlers)
```csharp
public partial class XyzHandler : ViewHandler<VirtualViewType, PlatformViewType>
{
    public static IPropertyMapper<VirtualViewType, XyzHandler> Mapper = 
        new PropertyMapper<VirtualViewType, XyzHandler>(ViewHandler.ViewMapper);
    
    public XyzHandler() : base(Mapper) { }
    protected override PlatformViewType CreatePlatformView() { }
    protected override void ConnectHandler(PlatformViewType platformView) { }
    protected override void DisconnectHandler(PlatformViewType platformView) { }
}
```

### MauiViewHost Platform Specifics
- **iOS:** Custom `MauiViewHostContainerView : UIView` with layout override
- **Android:** FrameLayout with MatchParent children
- **Windows:** Grid with Stretch alignment

### CometHost Platform Specifics
- **iOS:** UIView with AutoresizingMask flexibility
- **Android:** FrameLayout with OnLayout + density conversion
- **Windows:** Canvas with MeasureOverride/ArrangeOverride

### Context Fallback (Critical Pattern)
```csharp
// In all MauiViewHost/CometHost handlers:
try {
    platformView = VirtualView.HostedView.ToPlatform(MauiContext);
}
catch (Exception) {
    var fallbackCtx = CometApp.MauiContext;  // App-wide context
    if (fallbackCtx != null && fallbackCtx != MauiContext)
        platformView = VirtualView.HostedView.ToPlatform(fallbackCtx);
}
```

---

## Concrete Test Examples (Ready to Adapt)

### Pattern 1: Lazy Factory Test
```csharp
[Fact]
public void MauiViewHost_Factory_CalledOnce()
{
    int count = 0;
    var host = new MauiViewHost(() => 
    { 
        count++; 
        return new TestIView(); 
    });
    
    _ = host.HostedView;
    _ = host.HostedView;
    
    Assert.Equal(1, count);  // Factory called exactly once
}
```

### Pattern 2: Size Constraints Test
```csharp
[Fact]
public void MauiViewHost_GetDesiredSize_RespectsWidthConstraint()
{
    var host = new MauiViewHost(new TestIView());
    host.Frame(width: 150);
    
    var size = host.GetDesiredSize(new Size(500, 500));
    
    Assert.Equal(150, size.Width);
    Assert.True(size.Height > 0);
}
```

### Pattern 3: Layout Integration Test
```csharp
[Fact]
public void MauiViewHost_CanBeChildOfHStack()
{
    var host = new MauiViewHost(new TestIView()).Frame(width: 100);
    var stack = new HStack { host };
    
    Assert.Single(stack.Where(v => v == host));
    Assert.Equal(stack, host.Parent);
}
```

### Pattern 4: CometHost Property Test
```csharp
[Fact]
public void CometHost_CometView_CanBeSwapped()
{
    var viewA = new Text("A");
    var viewB = new Text("B");
    var host = new CometHost(viewA);
    
    Assert.Same(viewA, host.CometView);
    
    host.CometView = viewB;
    
    Assert.Same(viewB, host.CometView);
}
```

---

## Recommended Test Plan for Phase 6.2

### Week 1: Extend Existing (Low Risk)
- [ ] Add 16 tests to `MauiViewHostTests.cs`
- [ ] Add 10 tests to `ViewGetViewTests.cs`

### Week 2: Create CometHost Tests
- [ ] Create `CometHostTests.cs` (20 tests)

### Week 3: Create Context & Layout Tests
- [ ] Create `InteropContextTests.cs` (15 tests)
- [ ] Create `InteropLayoutTests.cs` (30 tests)

**Total Anticipatory Tests: ~91 tests**

### Post-Phase 6.1 (When Amos Delivers NativeHost)
- Add platform handler tests (iOS/Android/Windows)
- Add property synchronization tests
- Add native view embedding integration tests

---

## Files to Watch (Amos Updates)

When NativeHost is delivered, watch for:
- `/src/Comet/Controls/NativeHost.cs` (new control)
- `/src/Comet/Handlers/NativeHost/*` (new handlers)
- Updates to handler registration in `AppHostBuilderExtensions.cs`
- New test patterns for native platform view integration

---

## Summary

**Bobbie can start Phase 6.2 immediately.** No blockers from Amos:
1. MauiViewHost/CometHost are **production code** (not placeholders)
2. Handler patterns are **fully implemented** (all platforms)
3. Test infrastructure is **proven** (TestBase, GenericViewHandler ready)
4. ~91 anticipatory tests are **safely testable now** (no native code required)

Amos's NativeHost will extend this foundation; tests will scale naturally.

