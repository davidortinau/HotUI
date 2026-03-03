# Comet MAUI SDK-Only Remediation Roadmap

**Scope**: Fix all TIER 1 blockers for 100% honest MAUI SDK coverage  
**Total Effort**: ~12-16 hours across 5 critical tasks  
**Success Criteria**: All TIER 1 complete, build passes all platforms, honest 70%+ coverage

---

## TIER 1: Production Blockers (12-16 hours)

### Task 1.1: Fix RadioButton Handler Resolution — 4-6 hours [NOT STARTED]
**Severity**: 🔴 CRITICAL  
**Impact**: RadioButton doesn't work on any platform (handler resolution broken)  
**Current State**: Handler files exist but registration mixes Comet + MAUI logic

**What Needs to Happen**:
1. Examine src/Comet/Handlers/RadioButton/RadioButtonHandler.cs (base)
2. Examine src/Comet/Handlers/RadioButton/RadioButtonHandler.iOS.cs
3. Examine src/Comet/Handlers/RadioButton/RadioButtonHandler.Android.cs
4. Examine src/Comet/Handlers/RadioButton/RadioButtonHandler.Windows.cs
5. Identify why handler selection fails (likely namespace/type issue)
6. Fix handler registration in src/Comet/AppHostBuilderExtensions.cs
7. Test on iOS, Android, Windows
8. Verify RadioGroup (grouped radio buttons) works correctly
9. Add/update tests for radio button behavior

**Files Involved**:
- src/Comet/Handlers/RadioButton/RadioButtonHandler.cs
- src/Comet/Handlers/RadioButton/RadioButtonHandler.iOS.cs
- src/Comet/Handlers/RadioButton/RadioButtonHandler.Android.cs
- src/Comet/Handlers/RadioButton/RadioButtonHandler.Windows.cs
- src/Comet/Controls/RadioButton.cs
- src/Comet/Controls/RadioGroup.cs
- src/Comet/AppHostBuilderExtensions.cs

**Expected Outcome**:
```csharp
var radioGroup = new RadioGroup { ItemsSource = options, SelectedItem = State(someOption) };
// Should render native RadioButton groups on all platforms
```

---

### Task 1.2: Add Windows CometViewHandler — 2-3 hours [NOT STARTED]
**Severity**: 🔴 CRITICAL  
**Impact**: All Comet controls on Windows lack proper base handler  
**Current State**: Only iOS/Android CometViewHandler implementations exist

**What Needs to Happen**:
1. Create src/Comet/Handlers/View/CometViewHandler.Windows.cs
2. Study iOS/Android versions for pattern
3. Implement equivalent for Windows using WinUI patterns
4. Register with MAUI Windows handler system
5. Handle platform-specific properties (Gestures, Effects, etc.)
6. Test basic rendering on Windows

**Implementation Pattern**:
```csharp
namespace Comet.Handlers
{
    public partial class CometViewHandler : ViewHandler<IView, FrameworkElement>
    {
        public static void MapContent(CometViewHandler handler, IView view) { ... }
        // ... other mappings
    }
}
```

**Files Involved**:
- src/Comet/Handlers/View/CometViewHandler.Windows.cs (CREATE NEW)
- src/Comet/Handlers/View/CometViewHandler.iOS.cs (reference)
- src/Comet/Handlers/View/CometViewHandler.Android.cs (reference)

**Expected Outcome**: Windows platform controls render with proper handlers, no crash on Windows.

---

### Task 1.3: Remove Fake MapView — 0.5 hours [NOT STARTED]
**Severity**: 🟠 MEDIUM  
**Impact**: Documentation misleading; control throws NotSupportedException at runtime  
**Current State**: MapView exists but renders "Map functionality requires MauiViewHost" placeholder

**What Needs to Happen**:
1. Delete src/Comet/Controls/MapView.cs
2. Remove MapView handler registration from AppHostBuilderExtensions.cs
3. Remove MapView tests (if any)
4. Add note in README recommending MauiViewHost for maps
5. Verify build succeeds

**Expected Outcome**: No more fake controls; cleaner codebase.

---

### Task 1.4: Verify MenuBar Implementation — 2-3 hours [NOT STARTED]
**Severity**: 🟠 MEDIUM  
**Impact**: App menus may not work correctly; incomplete implementation  
**Current State**: MenuBar control exists, handler partially registered

**What Needs to Happen**:
1. Review src/Comet/Controls/MenuBar.cs implementation
2. Check handler registration (src/Comet/AppHostBuilderExtensions.cs)
3. Verify platform handlers exist (iOS, Android, Windows, macOS)
4. Test on each platform with sample menu
5. Verify click events fire correctly
6. Document platform limitations (if any)

**Investigation Points**:
- Does handler registration include MenuBar? Check AppHostBuilderExtensions.cs
- Are platform handlers complete? Check src/Comet/Handlers/MenuBar/
- Test sample: Create CometApp with MenuBar items

**Expected Outcome**:
- Either full working implementation documented, OR
- Acknowledge limitations per platform and document

---

### Task 1.5: Verify TitleBar Implementation — 2-3 hours [NOT STARTED]
**Severity**: 🟠 MEDIUM  
**Impact**: Custom title bars may not render; feature incomplete  
**Current State**: TitleBar control exists but implementation unclear

**What Needs to Happening**:
1. Review src/Comet/Controls/TitleBar.cs implementation
2. Check handler registration (src/Comet/AppHostBuilderExtensions.cs)
3. Verify platform handlers exist
4. Test on each platform with custom title
5. Verify leading/trailing content renders correctly
6. Document limitations

**Investigation Points**:
- Handler registered? Search AppHostBuilderExtensions.cs for TitleBar
- Platform handlers complete? Check src/Comet/Handlers/TitleBar/
- Test sample: Custom app title with subtitle and buttons

**Expected Outcome**:
- Either full working implementation documented, OR
- Acknowledge limitations and document workaround

---

### Task 1.6: Implement Missing Gestures — 4-5 hours [NOT STARTED]
**Severity**: 🟡 LOW (Tier 1.5)  
**Impact**: Advanced gesture patterns not available  
**Current State**: TapGesture, PanGesture, PinchGesture work; others missing

**Missing Gestures**:
- [ ] Drop gesture (handle drop operations)
- [ ] Drag gesture (drag initiation)
- [ ] Pointer gesture (mouse/pointer hover)
- [ ] Mouse gesture (mouse-specific interactions)

**What Needs to Happen**:
1. For each gesture type:
   - Create handler in src/Comet/Handlers/Gestures/
   - Implement iOS/Android/Windows platform versions
   - Register in AppHostBuilderExtensions.cs
   - Add tests

**Expected Outcome**: All MAUI SDK gestures supported in Comet.

---

## Implementation Order

**Priority 1 (Do First)**:
1. Task 1.1: Fix RadioButton (unblocks forms)
2. Task 1.2: Add Windows CometViewHandler (unblocks Windows)

**Priority 2 (Do Second)**:
3. Task 1.3: Remove MapView (cleanup)
4. Task 1.4: Verify MenuBar
5. Task 1.5: Verify TitleBar

**Priority 3 (Nice-to-Have)**:
6. Task 1.6: Implement Missing Gestures

---

## Testing Strategy

### Per-Task Testing
- **RadioButton**: Test grouped radio buttons, selection events, enabled/disabled states
- **Windows Handler**: Test basic control rendering, layout, properties
- **MapView removal**: Verify build succeeds, no dangling references
- **MenuBar/TitleBar**: Manual platform testing with sample app
- **Gestures**: Unit tests + manual gesture input testing

### Full Integration Tests
After all tasks complete:
```bash
dotnet build  # All platforms
dotnet test   # Unit tests
# Manual: CometFeatureShowcase on iOS, Android, Windows, macOS
```

---

## Success Criteria for TIER 1 Complete

- ✅ RadioButton works on iOS, Android, Windows
- ✅ All controls render on Windows (CometViewHandler present)
- ✅ MapView removed, no fake controls remain
- ✅ MenuBar and TitleBar either fully working or limitations documented
- ✅ Missing gestures implemented
- ✅ Build succeeds on all platforms
- ✅ All unit tests pass
- ✅ Honest coverage claims (70%+, not inflated)
- ✅ Documentation updated with MauiViewHost recommendation pattern

---

## Files to Monitor

**Core Framework**:
- src/Comet/AppHostBuilderExtensions.cs (handler registration)
- src/Comet/Handlers/View/CometViewHandler.*.cs (all platforms)

**Controls**:
- src/Comet/Controls/RadioButton.cs, RadioGroup.cs
- src/Comet/Controls/MenuBar.cs, TitleBar.cs
- ~~src/Comet/Controls/MapView.cs~~ (to be deleted)

**Gestures**:
- src/Comet/Handlers/Gestures/*.cs (all gesture types)

**Documentation**:
- README.md (coverage claims)
- MAUI_SDK_COVERAGE_AUDIT.md (this audit)
- docs/api-reference.md (if exists)

---

## Effort Estimate Breakdown

| Task | Min Hours | Max Hours | Complexity |
|------|-----------|-----------|------------|
| 1.1: RadioButton | 4 | 6 | HIGH |
| 1.2: Windows Handler | 2 | 3 | MEDIUM |
| 1.3: Remove MapView | 0.5 | 0.5 | LOW |
| 1.4: MenuBar Verify | 2 | 3 | MEDIUM |
| 1.5: TitleBar Verify | 2 | 3 | MEDIUM |
| 1.6: Missing Gestures | 4 | 5 | MEDIUM |
| **TOTAL** | **14.5** | **20.5** | — |

**Realistic**: 15-18 hours with testing and verification.

---

## Success Checklist

After all tasks complete:

- [ ] RadioButton control works on all 3 platforms
- [ ] Windows CometViewHandler implemented and tested
- [ ] MapView deleted and build succeeds
- [ ] MenuBar functionality documented (working or limitations)
- [ ] TitleBar functionality documented (working or limitations)
- [ ] All missing gestures implemented
- [ ] All unit tests pass
- [ ] CometFeatureShowcase builds on iOS, Android, macOS, Windows
- [ ] README updated with honest 70%+ coverage claim
- [ ] MauiViewHost pattern documented for gaps
- [ ] All commits pushed with Copilot co-author
