---
last_updated: 2026-03-08T22:59:00.000Z
---

# Team Wisdom

Reusable patterns and heuristics learned through work. NOT transcripts — each entry is a distilled, actionable insight.

## North Star

**REQUIRED READING before any implementation work:**

`.squad/FRAMEWORK_COMPARISON_AND_PROPOSAL.md` — the foundational design document for this entire branch. It defines:
- What Comet is adopting from MauiReactor (Component<S,P>, SetState, Render, themes, factory methods)
- What Comet keeps from its own engine (handler-level IView architecture)
- The specific API surface the evolution targets

Every code change, sample update, and review gate must be evaluated against this document. If your work contradicts the proposal, stop and raise it.

## User Directives

These are David's standing instructions. They override any agent decision.

1. **NOT renaming Comet to Orbit.** The framework stays "Comet."
2. **Tooling order:** maui-ai-debugging → MauiDevFlow → Appium (as fallback)
3. **Appium is the standard for interactive verification** when MauiDevFlow fails.
4. **Render-only ≠ done.** Screenshots without interactive flow exercise do not count as verification.
5. **Samples must match real apps.** CometBaristaNotes ground truth is `~/work/BaristaNotes` (MauiReactor). Do NOT fabricate UI.
6. **Current scope: CometMauiApp ONLY.** Other samples are deferred until David says otherwise.
7. **Plan lives in `.squad/plan.md`**, not session-state.

## Patterns

<!-- Append entries below. Format: **Pattern:** description. **Context:** when it applies. -->

**Pattern:** ConditionalWeakTable to prevent duplicate handler subscriptions in AppendToMapping callbacks. **Context:** Comet's Reload cycle calls SetVirtualView repeatedly, causing AppendToMapping to fire multiple times on the same native view. Track subscribed views to skip duplicates.

**Pattern:** Agent decisions must be captured separately from user directives in decisions.md. **Context:** User directives are standing orders. Agent decisions are implementation choices. Mixing them causes user intent to get buried.
