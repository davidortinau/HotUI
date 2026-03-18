# Comet Control Coverage — Phase 8.1 Assessment

**Date:** 2026-03-08  
**Author:** Naomi (Source Generator Dev)  
**Status:** Complete

## Executive Summary

Comet's generated control coverage is **comprehensive** for .NET MAUI 10. All simple, property-based IView interfaces suitable for source generation are covered. Complex controls with custom logic, collections, or platform-specific behavior are correctly implemented as handwritten classes.

---

## Generated Controls (19)

These controls are generated via `[assembly: CometGenerate(...)]` attributes in `src/Comet/Controls/ControlsGenerator.cs`:

| # | Generated Class | MAUI Interface | Purpose | Key Parameters |
|---|----------------|----------------|---------|----------------|
| 1 | `Button` | `ITextButton` | Text button | Text, Clicked |
| 2 | `ImageButton` | `IImageButton` | Image button | Source, Clicked |
| 3 | `Text` | `ILabel` | Text display | Text (as Value) |
| 4 | `TextField` | `IEntry` | Single-line text input | Text, Placeholder, Completed |
| 5 | `SecureField` | `IEntry` | Password input (IsPassword=true) | Text, Placeholder, Completed |
| 6 | `TextEditor` | `IEditor` | Multi-line text input | Text |
| 7 | `Slider` | `ISlider` | Range selector | Value, Minimum, Maximum |
| 8 | `Toggle` | `ISwitch` | Boolean toggle | Value (IsOn) |
| 9 | `CheckBox` | `ICheckBox` | Boolean checkbox | IsChecked |
| 10 | `DatePicker` | `IDatePicker` | Date selection | Date, MinimumDate, MaximumDate |
| 11 | `TimePicker` | `ITimePicker` | Time selection | Time |
| 12 | `Stepper` | `IStepper` | Increment/decrement | Value, Minimum, Maximum, Interval |
| 13 | `SearchBar` | `ISearchBar` | Search input | Text, SearchButtonPressed |
| 14 | `ProgressBar` | `IProgress` | Progress indicator | Progress (as Value) |
| 15 | `ActivityIndicator` | `IActivityIndicator` | Loading spinner | IsRunning |
| 16 | `IndicatorView` | `IIndicatorView` | Page indicator | Count |
| 17 | `RefreshView` | `IRefreshView` | Pull-to-refresh wrapper | IsRefreshing |
| 18 | `Toolbar` | `IToolbar` | Toolbar configuration | BackButtonVisible, IsVisible |
| 19 | `FlyoutView` | `IFlyoutView` | Flyout container | (ContentView-based) |

**Coverage:** All simple input, display, and basic container controls in MAUI 10.

---

## Handwritten Controls (Justified Complexity)

These controls require custom logic and are correctly implemented as handwritten classes:

### Input & Display
- **`Picker`** (`IPicker`) — Collection binding, SelectedIndex tracking
- **`RadioButton`** (no interface) — RadioGroup coordination, custom selection logic
- **`Image`** (`IImage`) — ImageSource type handling, async loading
- **`BoxView`** — CornerRadius customization
- **`FormattedString`** — Span collection management

### Containers & Layouts
- **`Border`** (`IBorderStroke`) — LayoutManager, stroke properties
- **`ContentView`** (`IContentView`) — Base for many other controls
- **`ScrollView`** (`IScrollView`) — Scroll offset tracking, orientation
- **`SwipeView`** — SwipeItem collection management
- **`CarouselView`**, **`CollectionView`** — ItemsSource, templates, virtualization
- **`ListView`**, **`TableView`** — (Deprecated in MAUI 10, kept for migration)

### Layouts
- **`Grid`**, **`HStack`**, **`VStack`**, **`ZStack`** — AbstractLayout subclasses
- **`AbsoluteLayout`**, **`FlexLayout`** — LayoutManager implementations
- **`HGrid`**, **`VGrid`** — Custom grid variants

### Navigation & Shell
- **`CometShell`** — Shell wrapper with typed navigation
- **`NavigationView`** — Navigation stack management
- **`FlyoutNavigationView`** — Flyout menu logic
- **`ContentPage`** — Page lifecycle, toolbar
- **`ModalView`** — Modal presentation

### Advanced
- **`MenuBar`** — MenuBarItem collection (IMenuBar interface)
- **`MenuFlyout`** — Context menu management
- **`TabView`** — Tab collection, selection
- **`BlazorWebView`** — Blazor hosting (complex interop)
- **`WebView`** — Web content rendering
- **`GraphicsView`** — Custom drawing with ICanvas
- **`CometHost`**, **`MauiViewHost`**, **`NativeHost`** — View hosting abstractions

### Specialized
- **`ToolbarItem`** — Simple data class (not IView)
- **`SwipeItem`** — Simple data class (not IView)
- **`BackButtonBehavior`** — Behavior configuration
- **`TitleBar`** — Window title bar customization

---

## MAUI 10 New Controls Assessment

### HybridWebView (`IHybridWebView`)
**Status:** TOO COMPLEX for generator  
**Reason:** Requires JavaScript interop (`RawMessageReceived` event, `InvokeJavaScriptAsync` method), asset loading logic  
**Owner:** Amos (complex handwritten lane)

### MenuItem / MenuBarItem / MenuFlyoutItem
**Status:** TOO COMPLEX for generator  
**Reason:** Inherit from `BaseMenuItem` with collection management, command binding, template support  
**Owner:** Amos (if needed) or keep using MenuBar + simple factory methods

---

## Not Suitable for Generation (Commented Out)

```csharp
//[assembly: CometGenerate(typeof(IBorder), BaseClass = "ContentView", Namespace = "Comet")]
//[assembly: CometGenerate(typeof(IRadioButton), nameof(IRadioButton.IsChecked), Namespace = "Comet")]
```

**Reason:** Both have custom handwritten implementations with logic that can't be generated:
- `Border`: Custom layout manager for stroke rendering
- `RadioButton`: RadioGroup coordination for mutual exclusivity

---

## Coverage Assessment

### ✅ Complete
- All MAUI 10 simple input controls
- All MAUI 10 simple display controls
- All MAUI 10 simple picker/selector controls
- Basic container controls suitable for generation

### ✅ Justified Handwritten
- Complex containers (ScrollView, SwipeView, CollectionView, etc.)
- Navigation (Shell, NavigationView, Pages)
- Layouts (Grid, Stack, Flex, Absolute)
- Advanced (Web views, graphics, Blazor)
- Data classes (ToolbarItem, SwipeItem)

### ⚠️ Not Applicable
- Deprecated controls (`ListView`, `TableView`, `Frame`) — kept for migration only, not expanded
- Internal/handler-only interfaces — not part of public API surface

---

## Generator Enhancements Already Applied (Phases 2-3)

1. **Factory Methods** (Phase 2.1-2.2)
   - Static factory methods in `Comet.CometControls` class
   - Binding<T>, Func<T>, and parameterless overloads
   - "On" prefixed extension aliases for Action properties

2. **Style Builders** (Phase 2.3)
   - 19 `{Control}StyleBuilder` classes in `Comet.Styles` namespace
   - Fluent setters for all control properties
   - Background() and TextColor() on every builder
   - Implicit conversion to `ControlStyle<T>`

---

## Recommendation

**Phase 8.1 Status:** COMPLETE  
**Coverage:** Comprehensive for Reactor-style API needs  
**Next Steps:**  
- Phase 8.2 (Amos): Add handwritten complex controls as needed (e.g., HybridWebView wrapper if desired)
- Phase 8.3 (Bobbie): Expand test coverage for generated controls (factory methods, style builders)

**Conclusion:** Comet's generator-owned control surface is fully expanded. All simple IView interfaces in MAUI 10 are covered. Complex controls are correctly in the handwritten lane. No action needed for Phase 8.1 beyond this documentation.
