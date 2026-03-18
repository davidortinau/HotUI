# Plan: CometControlsGallery — Controls & Theme Gallery

> Authored by **Holden**, Lead Architect · Comet Squad

## Problem Statement

We have a style/theme system (ColorTokens, TypographyTokens, SpacingTokens, ShapeTokens, ButtonStyles, ViewModifier, ControlState, Theme switching) and a full set of generated controls — but no single sample app that exercises all of them together. CometMauiApp's `MainPage.cs` covers the style system basics on a single scrolling page; what we need is a **tabbed gallery** that a developer can browse to see every control styled, every token rendered, and theme switching in action.

The reference is [Redth's mauiplatforms sample](https://github.com/Redth/mauiplatforms/tree/main/samples/Sample) — a pure MAUI C# code-only app with ~25 pages, TabbedPage navigation, buttons/entries/sliders/toggles/pickers/images/progress/borders/shadows/grids/collections/graphics/device-info and theme-aware colors. We're building the Comet equivalent, but shaped around our token-based style system rather than raw colors.

## Architecture Decisions

### AD-1: CometApp + TabView, not Shell

We use the **CometApp pattern** — `builder.UseCometApp<App>()` with `TabView(...)` as the root view. This is pure Comet, no MAUI Shell bridging. The `TabView` factory method takes `(string title, View content)` tuples. Each tab's content is a `ScrollView` wrapping a `VStack` with sectioned content.

Rationale: Shell adds MAUI interop complexity (`MakeCometPage`, `EmbedCometView`). This gallery should prove Comet can stand on its own. CometTaskApp already validates this pattern.

### AD-2: Component<TState> for stateful pages, View for static pages

Pages that need reactive state (controls with counters, theme toggle, input bindings) use `Component<TState>` with `SetState()`. Pages that are pure display (typography scale, token swatches) can use plain `View` with `[Body]`.

### AD-3: Four tabs, not five

The draft plan had 5 tabs. DeviceInfo doesn't exercise controls or styles — it's a platform API demo. Merge it into the Theme tab as an "About" section, or defer entirely. Four tabs keep the gallery focused:

1. **Controls** — Every input/display control, styled
2. **Layouts** — Grid, stacks, spacing tokens, borders, shadows
3. **Lists** — ListView<T> with themed templates, sections
4. **Theme** — Token swatches, typography scale, button styles, modifiers, control state, theme toggle

### AD-4: One SectionCard modifier, not three

The draft proposed CardModifier, SectionModifier, ChipModifier. That's over-engineering for a sample. One `SectionCard` modifier that wraps content in a Surface-colored rounded container with padding is enough. Every section on every page uses it. If we need a chip, we inline it — it's two lines of fluent API.

### AD-5: Scope the control coverage to what the style system actually touches

The draft missed several generated controls. Here's the full inventory of controls we should demonstrate, mapped to what the style system provides:

**Styled controls (have built-in control styles or token integration):**
- Button (4 ButtonStyles + custom)
- Slider (SliderStyle: TrackColor, ProgressColor, ThumbColor)
- ProgressBar (ProgressBarStyle: TrackColor, ProgressColor)
- Text (TextStyle, TypographyTokens)
- TextField (entry theming)
- Toggle (on-color theming)

**Controls to demonstrate without deep style integration:**
- SecureField, TextEditor, SearchBar (text input variants)
- CheckBox, RadioButton/RadioGroup (selection controls)
- DatePicker, TimePicker (platform pickers)
- Stepper (increment/decrement)
- Image, ImageButton (media)
- ActivityIndicator (loading)
- Picker (dropdown)
- Border (stroke, corner radius, ShapeTokens)

### AD-6: Skip GraphicsView for Phase 1

GraphicsView is canvas drawing, not controls or styling. It doesn't use tokens, doesn't respond to themes, and is a different concern. Defer to Phase 2. This keeps scope tight.

### AD-7: Build order matters

```
1. dotnet build src/Comet.SourceGenerator/Comet.SourceGenerator.csproj -c Release
2. dotnet build src/Comet/Comet.csproj -c Release
3. dotnet build sample/CometControlsGallery/CometControlsGallery.csproj -c Release
```

The test project won't be affected since this is a new sample, not a framework change.

## File Structure

```
sample/CometControlsGallery/
├── CometControlsGallery.csproj   — MAUI project, Comet project reference
├── App.cs                         — CometApp + TabView (4 tabs), theme init
├── SectionCard.cs                 — Reusable ViewModifier for card sections
├── Pages/
│   ├── ControlsPage.cs            — All input/display controls
│   ├── LayoutsPage.cs             — Grid, stacks, spacing, borders, shadows
│   ├── ListsPage.cs               — ListView<T> with sections + templates
│   └── ThemePage.cs               — Tokens, typography, button styles, modifiers, theme toggle
├── Platforms/
│   ├── Android/
│   │   ├── MainActivity.cs
│   │   └── MainApplication.cs
│   ├── iOS/
│   │   ├── AppDelegate.cs
│   │   ├── Info.plist
│   │   └── Program.cs
│   └── MacCatalyst/
│       ├── AppDelegate.cs
│       ├── Entitlements.Debug.plist
│       ├── Entitlements.Release.plist
│       ├── Info.plist
│       └── Program.cs
└── Resources/
    ├── AppIcon/
    ├── Images/
    └── Splash/
```

## Tab Details

### Tab 1: Controls (`ControlsPage`)
A scrolling VStack of SectionCard sections. State: tap count, slider value, toggle state, text value, picker index, checkbox state.

| Section | Controls | Style Coverage |
|---------|----------|----------------|
| Buttons | Button × 4 (Filled, Outlined, Text, Elevated) + tap counter | `ButtonStyles.*`, `ColorTokens.Primary/OnPrimary` |
| Text Input | TextField, SecureField, TextEditor, SearchBar | `ColorTokens.Surface/OnSurface`, `TypographyTokens.BodyLarge` |
| Selection | Toggle, CheckBox, RadioGroup (3 options), Picker | `ColorTokens.Primary` for on-color |
| Numeric | Slider (with live value label), Stepper | `SliderStyle` tokens |
| Date/Time | DatePicker, TimePicker | Token-colored labels |
| Display | Text (styled), Image (URL), ActivityIndicator, ProgressBar | `ProgressBarStyle`, `TypographyTokens.*` |

### Tab 2: Layouts (`LayoutsPage`)
Demonstrates layout containers and spacing/shape tokens.

| Section | Content | Style Coverage |
|---------|---------|----------------|
| VStack Spacing | 6 colored boxes at each SpacingToken value (None→ExtraLarge) | `SpacingTokens.*` |
| HStack | Horizontal row of labeled items | `ColorTokens.Primary/Secondary/Tertiary` |
| Grid | 2×3 grid with colored cells, column/row definitions | `ColorTokens.*Container` variants |
| Border & Shape | Borders at each ShapeToken radius (None→Full) | `ShapeTokens.*`, `ColorTokens.Outline` |
| Shadows | Text + Button + Border with shadow effects | `ColorTokens.Surface` |
| ScrollView | Horizontal scroll of cards | Composed layout |

### Tab 3: Lists (`ListsPage`)
ListView<T> demonstrating themed item templates.

| Section | Content | Style Coverage |
|---------|---------|----------------|
| Simple List | 20 items with name + subtitle, themed | `ColorTokens.Surface/OnSurface/OnSurfaceVariant` |
| Styled Items | Items with primary-colored leading indicator | `ColorTokens.Primary`, `ShapeTokens.Small` |
| Item Selection | Selected item highlight with reactive state | `ColorTokens.PrimaryContainer/OnPrimaryContainer` |

### Tab 4: Theme (`ThemePage`)
The style system showcase. State: current theme (light/dark), enabled toggle.

| Section | Content | Style Coverage |
|---------|---------|----------------|
| Theme Toggle | Toggle + label switching `Theme.Current` between `Defaults.Light` and `Defaults.Dark` | `Theme.Current`, `Defaults.*` |
| Color Tokens | Grid of all ~30 ColorTokens as colored boxes with name labels | Every `ColorTokens.*` constant |
| Typography Scale | All 15 TypographyTokens rendered as labeled text samples | Every `TypographyTokens.*` constant |
| Button Styles | All 4 built-in ButtonStyles side by side + disabled state | `ButtonStyles.*`, `ControlState.Disabled` |
| ViewModifier | SectionCard modifier + a "highlight" composed modifier (`.Then()`) | `ViewModifier.Then()`, `.Modifier()` |
| Scoped Theme | A sub-section with `view.OverrideToken()` showing token overrides | `OverrideToken()` API |

## Style System Coverage Matrix

| Style API | Where Demonstrated | Verified By |
|-----------|-------------------|-------------|
| `ColorTokens.*` (30 tokens) | ThemePage swatch grid + every page background | Visual inspection |
| `TypographyTokens.*` (15 tokens) | ThemePage scale + section headers | Visual inspection |
| `SpacingTokens.*` (6 tokens) | LayoutsPage spacing demo | Visual measurement |
| `ShapeTokens.*` (7 tokens) | LayoutsPage border radius demo | Visual inspection |
| `ButtonStyles.Filled/Outlined/Text/Elevated` | ControlsPage + ThemePage | Tap interaction |
| `SliderStyle` (Track/Progress/Thumb) | ControlsPage slider | Drag interaction |
| `ProgressBarStyle` (Track/Progress) | ControlsPage progress bars | Visual |
| `ViewModifier` + `.Then()` composition | ThemePage modifier section | Visual |
| `.Modifier(modifier)` extension | Every section card on every page | Visual |
| `Theme.Current = Defaults.Light/Dark` | ThemePage toggle | Toggle and observe |
| `ControlState` (Disabled/Pressed/Hovered) | ThemePage button states | Toggle + tap |
| `view.OverrideToken()` scoped overrides | ThemePage scoped section | Visual |
| `Component<TState>` + `SetState()` | ControlsPage, ThemePage | Interaction |
| `TabView` navigation | App.cs root | Tab switching |

## Work Assignments

### Phase 1: Scaffold (Amos)

**Todo: `scaffold-project`** — Create the csproj, platform bootstrapping, App.cs with 4-tab TabView, SectionCard modifier. Copy platform files from CometMauiApp as a template. This is the skeleton that everything hangs on.

**Deliverable:** `dotnet build sample/CometControlsGallery/CometControlsGallery.csproj` succeeds. App launches with 4 empty tabs on maccatalyst.

### Phase 2: Pages (Amos)

Build in this order — each one should compile and run after being added:

1. **`controls-page`** — ControlsPage.cs. Heaviest page, exercises the most controls. Build this first to shake out any control API issues.
2. **`theme-page`** — ThemePage.cs. Exercises the full token/style system. The theme toggle here validates that all other pages respond to theme changes.
3. **`layouts-page`** — LayoutsPage.cs. Spacing tokens, shape tokens, borders, shadows.
4. **`lists-page`** — ListsPage.cs. ListView<T> with templates.

### Phase 3: Verify (Bobbie)

**Todo: `build-verify`** — Full build chain (source gen → Comet → gallery). Launch on maccatalyst. Walk each tab and verify:
- Every section renders without crashes
- Theme toggle affects all pages (switch tabs after toggling)
- Button styles render distinctly
- Token swatch grid shows different colors for each token
- Typography scale shows visible size differences
- Slider/Toggle/Picker interactions work reactively

Report any compilation errors or rendering issues back to Amos.

## What We Defer

| Item | Why Deferred |
|------|-------------|
| GraphicsView/Canvas drawing | Not about controls or tokens — different concern |
| DeviceInfo/AppInfo page | Platform API demo, not style system coverage |
| Custom IControlStyle<Button> | Complex API — demonstrate built-in styles first |
| Navigation (push/pop) | NavigationView is separate from controls gallery |
| Flyout/Shell patterns | Out of scope — this is CometApp pattern |
| Images from bundle | Requires adding image assets — URL images are sufficient |
| SecureField/TextEditor/SearchBar | Include in Phase 1 ControlsPage if straightforward; defer if they cause issues |

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|-----------|
| TabView doesn't render tab titles correctly | App navigation broken | Fall back to a single ScrollView with anchor-style sections. But CometTaskApp proves TabView works. |
| Some generated controls have incomplete token integration | Controls render but ignore theme | Use raw `.Color()` / `.Background()` as fallback — note in code comments what's missing |
| Theme toggle doesn't propagate to all visible views | Theme page works but other tabs stale | Comet's reactive system should handle this — if not, force re-render via `SetState()` on theme change |
| ListView<T> template rendering with tokens | List items don't pick up theme colors | Build template with explicit token resolution via `ThemeManager.TokenBinding()` |
| Large page compilations hit source generator limits | Build failures | Keep pages focused — no single file over ~300 lines |

## Code Conventions Reminder

- **Tabs for indentation** (configured in .editorconfig)
- **Factory methods**: `Text("hello")`, `VStack(spacing, children...)`, `Button("label", action)` — NOT `new Text("hello")`
- **`using static Comet.CometControls;`** at the top of every page
- **`Component<TState>`** with `SetState(s => s.Prop = value)` for reactive state
- **Token usage**: `ColorTokens.Primary` directly in `.Background()`, `.Color()` — implicit `Binding<Color>` conversion
- **Typography**: `.Typography(TypographyTokens.BodyLarge)` extension method
- **Button styles**: `.ButtonStyle(ButtonStyles.Filled)` extension
- **Theme**: `Theme.Current = Defaults.Light` at startup, toggle with `Theme.Current = Defaults.Dark`

## Todos

- [ ] **scaffold-project** — (Amos) Create CometControlsGallery.csproj, platform files, App.cs with 4-tab TabView, SectionCard.cs modifier. Must compile and launch with empty tabs.
- [ ] **controls-page** — (Amos) ControlsPage.cs with buttons (4 styles), TextField, Toggle, CheckBox, RadioGroup, Picker, Slider, Stepper, DatePicker, TimePicker, Image, ActivityIndicator, ProgressBar. All styled with tokens.
- [ ] **theme-page** — (Amos) ThemePage.cs with light/dark toggle, color token swatch grid, typography scale, button style showcase, modifier demo, scoped token override, control state demo.
- [ ] **layouts-page** — (Amos) LayoutsPage.cs with VStack spacing demo (all SpacingTokens), HStack, Grid (2×3), Border with all ShapeTokens, shadow effects, horizontal ScrollView.
- [ ] **lists-page** — (Amos) ListsPage.cs with ListView<T>, themed item templates, selection state.
- [ ] **build-verify** — (Bobbie) Full build chain, launch on maccatalyst, verify every tab and interaction. Report issues.
