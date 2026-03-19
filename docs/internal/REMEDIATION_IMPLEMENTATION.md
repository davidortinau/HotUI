# Comet Remediation Implementation Log — Option B (All Tiers)

**Status**: �� IN PROGRESS - TIER 1 starting now  
**Target**: 98%+ coverage (36-46 hours)  
**Date Started**: March 2026

## TIER 1: MUST-FIX (Production Blockers) — 16-20 hours

### Task 1.1: Remove Fake MapView ⏱️ 0.5 hours
**Status**: ⏳ Starting...

MapView is a production footgun — renders placeholder text and throws NotSupportedException.

**Changes**:
- [ ] Delete src/Comet/Controls/MapView.cs
- [ ] Remove from ControlsGenerator comments
- [ ] Update docs: "Use MauiViewHost(new Maps.Map())"
- [ ] Remove from AUDIT docs (mark as removed)

---

### Task 1.2: Implement MediaElement Handler ⏱️ 3 hours
**Status**: ⏳ Planning...

MediaElement is property-only stub with no handler registration.

**Changes**:
- [ ] Create src/Comet/Handlers/MediaElement/MediaElementHandler.cs
- [ ] Create iOS implementation
- [ ] Create Android implementation
- [ ] Register in AppHostBuilderExtensions.UseCometMauiHandlers()
- [ ] Wire binding support
- [ ] Test basic play/pause/stop

---

### Task 1.3: Fix RadioButton/RadioGroup ⏱️ 5 hours
**Status**: ⏳ Planning...

RadioButton has broken handler resolution (Comet + MAUI mixed).

**Changes**:
- [ ] Create proper RadioButton wrapper (or remove)
- [ ] Implement RadioButtonHandler.iOS.cs
- [ ] Implement RadioButtonHandler.Android.cs
- [ ] Implement RadioButtonHandler.Windows.cs
- [ ] Fix handler registration in AppHostBuilderExtensions
- [ ] Test across all platforms

---

### Task 1.4: Add Windows CometViewHandler ⏱️ 2.5 hours
**Status**: ⏳ Planning...

Base CometViewHandler missing Windows implementation.

**Changes**:
- [ ] Create src/Comet/Handlers/View/CometViewHandler.Windows.cs
- [ ] Parallel to iOS/Android implementations
- [ ] Register with MAUI Windows handler system
- [ ] Test basic rendering

---

### Task 1.5: Remove Fake Implementations ⏱️ 3 hours
**Status**: ⏳ Planning...

MenuBar, TitleBar, BlazorWebView, MultiBinding, RelativeBinding are marked ✅ but non-functional.

**Changes**:
- [ ] Evaluate MenuBar (keep or remove?)
- [ ] Evaluate TitleBar (keep or remove?)
- [ ] Remove BlazorWebView (just inherits WebView)
- [ ] Remove MultiBinding claim (not implemented)
- [ ] Remove RelativeBinding claim (not implemented)
- [ ] Update docs with honest status

---

### Task 1.6: Fix Audit Documentation ⏱️ 2.5 hours
**Status**: ⏳ Planning...

Update all audit docs with honest coverage percentages.

**Changes**:
- [ ] MAUI_9_0_AUDIT_REPORT.md — Update coverage % (87.7% → 64%)
- [ ] MAUI_9_0_COVERAGE_CHECKLIST.md — Mark fakes as ❌
- [ ] REMEDIATION_PLAN.md — Mark completed items
- [ ] IMPLEMENTATION_STATUS.md — Mark outdated sections
- [ ] AUDIT_SUMMARY.txt — Update verdict
- [ ] Create REMEDIATION_IMPLEMENTATION.md (this file)

---

## TIER 2: IMPORTANT (Production Enhancements) — 12-16 hours

### Task 2.1: Implement DataTemplateSelector ⏱️ 3.5 hours
**Status**: ⏳ Planned

Add conditional template selection for heterogeneous collections.

**Files to Create**:
- src/Comet/Controls/DataTemplateSelector.cs (generic + base)

**Files to Modify**:
- src/Comet/Controls/CollectionView.cs (add ItemTemplateSelector property)
- src/Comet/Controls/ListView.cs (add ItemTemplateSelector property)

---

### Task 2.2: Implement MultiBinding / IMultiValueConverter ⏱️ 5 hours
**Status**: ⏳ Planned

Add binding infrastructure for multiple sources.

**Files to Create**:
- src/Comet/Binding/MultiBinding.cs
- src/Comet/Binding/IMultiValueConverter.cs

**Files to Modify**:
- src/Comet/Binding.cs (integrate MultiBinding)

---

### Task 2.3: Implement Lifecycle Parity ⏱️ 5 hours
**Status**: ⏳ Planned

Cross-platform Loaded/Unloaded events.

**Files to Create**:
- src/Comet/Platform/Android/ViewLifecycleListener.cs
- src/Comet/Platform/Windows/ViewLifecycleHelper.cs

**Files to Modify**:
- src/Comet/Controls/View.cs (add Loaded/Unloaded events)
- iOS handler (wire ViewDidAppear → Loaded)

---

### Task 2.4: Add Shell Advanced Features ⏱️ 6 hours
**Status**: ⏳ Planned

Shell SearchHandler, FlyoutItem/MenuItem modeling.

**Files to Create**:
- src/Comet/Controls/Shell/SearchHandler.cs
- src/Comet/Controls/Shell/FlyoutItem.cs
- src/Comet/Controls/Shell/MenuItem.cs

---

## TIER 3: NICE-TO-HAVE (Completeness) — 8-10 hours

### Task 3.1: Implement Spring Animations ⏱️ 3.5 hours
**Status**: ⏳ Planned

Spring easing functions and physics-based animations.

---

### Task 3.2: Improve Test Quality ⏱️ 5 hours
**Status**: ⏳ Planned

Replace trivial assertions with behavior verification.

---

### Task 3.3: Performance Benchmarks ⏱️ 2.5 hours
**Status**: ⏳ Planned

Startup time, scroll FPS, memory usage benchmarks.

---

## IMPLEMENTATION NOTES

- Each task commits separately with detailed messages
- All changes tested (build + basic sanity)
- Audit docs updated as fixes complete
- Multi-model validation of critical fixes (RadioButton, Windows handler)

---

**Total Effort**: 36-46 hours across 13 tasks  
**Expected Completion**: ~1 week (8 hours/day)  
**Outcome**: 98%+ coverage for all app types  
**Status**: TIER 1 starting now...

