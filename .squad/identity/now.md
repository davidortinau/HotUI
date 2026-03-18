---
updated_at: 2026-03-09T013300Z
focus_area: "Factory methods — P1 dealbreaker requirement (AC-13). Naomi: implement source generation. Holden: update acceptance criteria."
north_star: ".squad/FRAMEWORK_COMPARISON_AND_PROPOSAL.md"
active_agents: ["naomi-factory-methods", "holden-ac13"]
p0_status: "FIXED — Re-entrancy guard + reconciliation identity fix. 720/739 tests passing (0 failures)."
---

# What We're Focused On

## READ FIRST

**North star document:** `.squad/FRAMEWORK_COMPARISON_AND_PROPOSAL.md`
Read this before doing ANY work. It defines the entire Comet MVU evolution.

**Current scope (per David):** CometMauiApp ONLY. All other samples are deferred.

## PRIORITY: Factory Methods (AC-13)

**Status:** David declared factory methods a **dealbreaker requirement** (2026-03-09T01:33:00Z).

**What:** Factory methods (no `new` keyword for controls). Syntax: `VStack { }` not `new VStack { }`.

**Why:** Core PRD requirement (FRAMEWORK_COMPARISON_AND_PROPOSAL.md, lines 228, 432, 1253-1254). Signature MauiReactor DX improvement.

**Agents Spawned:**
- **Naomi:** Implement factory method source generation (background, claude-sonnet-4.5)
- **Holden:** Update P1 acceptance criteria to add AC-13 (background, claude-sonnet-4.5)

**Next:** CometMauiApp uses factory syntax once AC-13 implementation is complete.

## Goal

Update `sample/CometMauiApp` to demonstrate the evolved Comet API surface:
- `Component<TState>` and `Component<TState, TProps>` base classes
- `SetState(...)` for state mutations
- `Render()` method (replaces `[Body]`)  
- `Reactive<T>` state wrappers
- Theme system (MD3 tokens, `ControlStyle<T>`)
- **Factory methods** and `On`-prefixed event extensions
- Typed navigation via Shell wrapper

The app must build, launch, and be verified end-to-end with Appium (interactive flows, not just screenshots).

## What Phases 1-9 Built (the API surface being demonstrated)

- Phase 1: `Component<S>`, `Component<S,P>`, `Reactive<T>` base classes
- Phase 2: Source generator (factory methods, On-prefixed extensions, 19 StyleBuilders)
- Phase 3: Theme system (29 MD3 tokens, `ControlStyle<T>`, `DefaultThemeStyles`)
- Phase 4: Reconciliation upgrade (key-aware diffing, Component merge)
- Phase 5: Navigation (Shell wrapper, typed GoToAsync, IReactor, IfElse, Switch, ForEach)
- Phase 6: Interop bridge (NativeHost for MAUI controls)
- Phase 7: Hot reload for Components
- Phase 8: Control expansion (~23 → ~40 controls)
- Phase 9: Sample apps + migration docs

All reviewer-approved. 625+ tests passing.

## User Directives (Standing Orders)

1. NOT renaming Comet to Orbit
2. Tooling: maui-ai-debugging → MauiDevFlow → Appium (fallback)
3. Render-only ≠ done
4. Samples must match real apps, never fabricate UI
5. Current scope: CometMauiApp ONLY
6. **NEW (2026-03-09): Factory methods are a dealbreaker (AC-13)**
7. Plan lives in `.squad/plan.md`


