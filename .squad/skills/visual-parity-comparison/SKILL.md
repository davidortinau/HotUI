# Visual Parity Comparison

**Confidence:** low
**Domain:** visual-qa, gallery, controls
**Created:** 2026-03-11
**Created by:** David Ortinau + Squad Coordinator

## Purpose

Ensures the Comet Controls Gallery achieves **near-identical** visual parity with
the MAUI macOS reference app. "Near" allows natural Mac Catalyst vs macOS differences
(window chrome, scrollbar style). Everything else — colors, alignment, sizing, spacing,
backgrounds, typography — must match.

## When to Use

- Any gallery page creation or modification
- Any visual parity review or comparison task
- Any time an agent claims a page "looks good" or "matches the reference"
- Before marking any gallery visual work as done

## Golden Rule

> **Never claim visual parity without dual screenshots compared side-by-side.**
> If you don't have both screenshots, you haven't verified anything.

---

## App Identification

Both apps run with MauiDevFlow agents. Always verify before screenshotting.

```bash
# Step 0: Discover both apps and their ports
maui-devflow list
```

Look for these two entries:

| App | Platform | Identifier |
|-----|----------|------------|
| **MAUI macOS** (reference) | macOS | App name contains "MAUI macOS" or "SampleMac" |
| **Comet Controls Gallery** | MacCatalyst | App name contains "Comet Controls Gallery" or "CometControlsGallery" |

```bash
# Step 1: Verify each agent — ALWAYS do this before screenshots
maui-devflow MAUI status --agent-port {MAUI_PORT}
# Must show: App: SampleMac, Platform: macOS

maui-devflow MAUI status --agent-port {COMET_PORT}
# Must show: App: CometControlsGallery, Platform: MacCatalyst
```

**⚠️ If `maui-devflow list` does not show both apps, STOP.** Do not proceed with
screencapture or AppleScript as a substitute — those capture the wrong window.
Ask the user to launch the missing app.

**⚠️ Ports change between launches.** Always run `maui-devflow list` at the start
of every session and after any app restart.

---

## Comparison Workflow

### Per-Page Comparison (the core loop)

For each gallery page you're working on:

#### 1. Navigate to the same page in both apps

```bash
# Navigate Comet gallery (use visual tree to find sidebar items)
maui-devflow MAUI tree --agent-port {COMET_PORT} --depth 3
# Find the sidebar item, tap it
maui-devflow MAUI tap {sidebar_item_id} --agent-port {COMET_PORT}

# Navigate MAUI reference (same approach)
maui-devflow MAUI tree --agent-port {MAUI_PORT} --depth 3
maui-devflow MAUI tap {sidebar_item_id} --agent-port {MAUI_PORT}
```

#### 2. Take dual screenshots

```bash
# Define output directory (session workspace — not committed)
PARITY_DIR="{SESSION_FILES}/visual-parity/{page-name}"
mkdir -p "$PARITY_DIR"

# Screenshot MAUI reference FIRST (this is the gold standard)
maui-devflow MAUI screenshot --agent-port {MAUI_PORT} \
  --output "$PARITY_DIR/maui.png"

# Screenshot Comet gallery
maui-devflow MAUI screenshot --agent-port {COMET_PORT} \
  --output "$PARITY_DIR/comet.png"
```

**Naming convention:** `{page-name}/maui.png` and `{page-name}/comet.png`
After fixes: `{page-name}/comet-after.png`

#### 3. Compare using vision

Load BOTH images into context and produce a structured diff report.
Use the checklist below — check every item, not just what looks obviously wrong.

#### 4. Verify with property inspection (when visual comparison is ambiguous)

```bash
# Check specific property values on both apps
maui-devflow MAUI property {element_id} BackgroundColor --agent-port {MAUI_PORT}
maui-devflow MAUI property {element_id} BackgroundColor --agent-port {COMET_PORT}

maui-devflow MAUI property {element_id} HorizontalOptions --agent-port {MAUI_PORT}
maui-devflow MAUI property {element_id} WidthRequest --agent-port {MAUI_PORT}
```

#### 5. Produce actionable fix list

Each difference becomes a fix item with:
- **What:** Description of the difference
- **Where:** Exact file and approximate line
- **Fix:** Specific property/value change needed
- **Priority:** High (structural/color), Medium (spacing), Low (minor cosmetic)

---

## Comparison Checklist

For every page, check ALL of these. Do not skip items.

### Layout & Alignment
- [ ] Page background color (white vs grey vs other)
- [ ] Content alignment (left-aligned, not centered)
- [ ] Content width (natural vs fill — buttons especially)
- [ ] Spacing between sections
- [ ] Padding/margins around content area
- [ ] Scroll behavior (both should scroll similarly)

### Controls
- [ ] Button style (outlined vs filled, color, corner radius)
- [ ] Button width (natural/auto vs full-width)
- [ ] Button text alignment
- [ ] Entry/Editor border style and background
- [ ] Slider track and thumb appearance
- [ ] Switch/Toggle appearance
- [ ] CheckBox appearance
- [ ] Stepper appearance

### Typography
- [ ] Section header color (should match reference — typically blue, NOT purple)
- [ ] Section header font size and weight
- [ ] Body text size and color
- [ ] Label text color

### Sidebar (if visible in screenshot)
- [ ] Icons present (SF Symbols in reference)
- [ ] Item alignment (left-aligned)
- [ ] Selected state highlight
- [ ] Category header styling
- [ ] Font size and weight

### Colors
- [ ] Primary accent color (reference uses blue, not purple)
- [ ] Button colors match reference exactly
- [ ] Background colors match (page, sections, controls)
- [ ] Text colors match (headers, body, placeholder)
- [ ] Gradient colors match (if gradients present)

---

## Diff Report Format

After comparing, produce this structured output:

```
## Visual Parity Report: {Page Name}
**Date:** {ISO timestamp}
**MAUI screenshot:** {path}
**Comet screenshot:** {path}

### 🔴 Critical Differences (must fix)
1. **{What}** — MAUI: {description}, Comet: {description}
   Fix: {specific change in specific file}

### 🟡 Medium Differences (should fix)
1. **{What}** — MAUI: {description}, Comet: {description}
   Fix: {specific change}

### 🟢 Minor Differences (nice to fix)
1. **{What}** — ...

### ✅ Matches
- {list what already matches}

### ⚪ Platform Differences (ignore)
- {list natural mac catalyst vs macos differences}
```

---

## Known Platform Differences (IGNORE these)

These are natural differences between macOS (AppKit) and Mac Catalyst — do NOT
report or fix them:

- Window title bar style (traffic lights position, title bar height)
- Scrollbar rendering style
- Menu bar behavior
- Minor font rendering differences (sub-pixel antialiasing)
- Focus ring style
- System dialog appearance

---

## Known Comet vs MAUI Differences to Watch

These are recurring issues the team has seen — check these specifically:

| Issue | Symptom | Root Cause | Typical Fix |
|-------|---------|------------|-------------|
| Full-width buttons | Buttons stretch edge-to-edge | Default Fill alignment | Set `HorizontalLayoutAlignment` to Start, or set explicit width |
| Purple vs Blue | Controls are purple instead of blue | Comet default theme color | Match the MAUI reference color exactly |
| Grey background | Page has grey/lavender background | View/container background not set | Set explicit white background on page/container |
| Missing sidebar icons | Text-only sidebar items | Icons not implemented | Add SF Symbol names or image sources |
| Centered text | Text centered instead of left-aligned | Explicit TextAlignment.Center | Remove or set TextAlignment.Start |

---

## Scrolling for Full Comparison

Most pages have content below the fold. After the initial screenshot comparison:

```bash
# Scroll down in both apps and take additional screenshots
maui-devflow MAUI scroll --dy -400 --agent-port {MAUI_PORT}
maui-devflow MAUI screenshot --agent-port {MAUI_PORT} \
  --output "$PARITY_DIR/maui-scrolled.png"

maui-devflow MAUI scroll --dy -400 --agent-port {COMET_PORT}
maui-devflow MAUI screenshot --agent-port {COMET_PORT} \
  --output "$PARITY_DIR/comet-scrolled.png"
```

Compare scrolled screenshots too — differences often hide below the fold.

---

## Known Limitation: Comet Views and MauiDevFlow Interaction

**MauiDevFlow screenshots work perfectly** for both apps. However, **tap, scroll, and
property inspection fail on Comet views** because `CometHost` blocks the hit-testing
that MauiDevFlow uses to resolve element IDs.

**What works on Comet:**
- `maui-devflow MAUI screenshot` ✅
- `maui-devflow MAUI tree` ✅ (shows `ContentPage > CometHost`)
- `maui-devflow MAUI status` ✅

**What fails on Comet:**
- `maui-devflow MAUI tap {id}` ❌ — can't resolve Comet child elements
- `maui-devflow MAUI scroll` ❌ — fails to find scrollable element
- `maui-devflow MAUI property {id} {prop}` ❌ — only resolves ContentPage/CometHost

**Workarounds:**
- **Navigation:** Use AppleScript or Appium to click sidebar items in Comet gallery
- **Scrolling:** Use AppleScript keyboard events (Page Down) or Appium swipe
- **Property values:** Read from source code instead of runtime inspection
- **Screenshots:** Always use `maui-devflow MAUI screenshot` (reliable, app-specific)

**MAUI reference app** has full MauiDevFlow support — tap, scroll, property all work.

---

## Fallback: When MauiDevFlow is Unavailable

If an app doesn't have a MauiDevFlow agent registered:

1. **Appium** (preferred fallback) — use the appium-automation skill
2. **screencapture + AppleScript** (last resort) — BUT you MUST verify the
   window belongs to the correct app:
   ```bash
   # Get the frontmost app name BEFORE capturing
   osascript -e 'tell application "System Events" to get name of first application process whose frontmost is true'
   # Must match "CometControlsGallery" or "SampleMac"
   ```

**Never use screencapture without verifying the frontmost window.**

---

## Agent Instructions

When spawning an agent for visual parity work, include in their prompt:

```
VISUAL PARITY SKILL: Read .squad/skills/visual-parity-comparison/SKILL.md before starting.

KEY REQUIREMENTS:
- Use `maui-devflow list` to discover both app ports at session start
- Use `maui-devflow MAUI status --agent-port {PORT}` to verify each app BEFORE screenshots
- Take screenshots of BOTH apps for every page comparison
- Save to: {SESSION_FILES}/visual-parity/{page-name}/maui.png and comet.png
- Use vision model to compare — produce structured diff report
- Every difference becomes an actionable fix item with file + property + value
- Check the FULL checklist — do not skip items
- Scroll and compare below-the-fold content too
```
