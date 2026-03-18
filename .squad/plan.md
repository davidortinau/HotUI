# All-Sample Evolution & Verification Plan

## Problem Statement

The earlier Phase 9 gate only proved a narrow build-and-structure slice for `CometMauiApp`, `CometBaristaNotes`, and the migration guide. That is no longer sufficient. The next execution pass must treat **all sample apps under `/sample`** as in-scope, refactor them as needed to demonstrate the updated Comet API cleanly, verify them through real user flows, capture retained evidence, and fix issues discovered during validation before claiming success.

## Updated Scope

This plan now covers **all 10 sample projects**:

1. `sample/Comet.Sample`
2. `sample/CometMauiApp`
3. `sample/CometBaristaNotes`
4. `sample/CometFeatureShowcase`
5. `sample/CometAllTheLists`
6. `sample/CometTaskApp`
7. `sample/CometProjectManager`
8. `sample/CometWeather`
9. `sample/CometStressTest`
10. `sample/MauiReference`

### Scope promises

For every sample, execution must:

- build successfully using the correct Comet build order
- launch successfully on the selected validation platform(s)
- render visible content instead of blank/empty/faulted shells
- exercise the app through real end-to-end flows
- be refactored where needed to better demonstrate the evolved Comet surface
- capture **original vs evolved** screenshots for later user review
- log problems discovered during validation
- fix those problems and rerun verification before sign-off

## Non-Negotiable Validation Rule

For UI and sample work, verification is not complete until the running app experience has been exercised end-to-end. That includes, as applicable:

- visible initial render
- tapping buttons, tabs, and list items
- forward/back navigation
- changing inputs and text entry
- state changes and refresh behavior
- persistence or logging flows when the sample claims to support them
- post-fix reruns after any bug discovered during validation

Build success, focused unit tests, and structural checks remain required, but they are **insufficient on their own**.

## Preferred Runtime Tooling

The standing workflow for current and future UI/runtime validation in this repo is:

1. **`maui-ai-debugging` skill**
2. **MauiDevFlow** for build → run → inspect → interact → capture
3. **Appium only as fallback**

Primary runtime loop:

- `dotnet build -t:Run` in an async shell
- `maui-devflow list` / `wait`
- `maui-devflow MAUI tree`, `query`, `element`, `property`, `tap`, `fill`, `logs`, `screenshot`
- optional `maui-devflow batch` for longer flow execution

If a sample lacks the necessary DEBUG-only MauiDevFlow agent wiring, that wiring becomes part of the work before the sample can be considered verification-ready.

## What “Original vs Evolved” Means

For this execution plan, the **original baseline** for each sample is:

- the sample as it exists **before refactoring begins in this execution cycle**
- captured from the repo/worktree state at execution start
- preserved in the session workspace as the “original” evidence set

The **evolved version** is:

- the post-refactor sample after it has been updated to better demonstrate the current Comet MVU evolution
- validated through full runtime flow exercise

If the original sample already fails (blank screen, crash, broken flow), that failure still gets captured as baseline evidence using screenshots and logs. The side-by-side set must therefore show **what existed before the fix** and **what exists after the fix**.

## Evidence Retention

Evidence must be retained in the **session workspace**, not committed into the repo.

Planned evidence root:

`/Users/davidortinau/.copilot/session-state/b26a6593-f539-47de-8f7b-3bd72e7ad681/files/sample-validation/`

Planned structure:

```text
sample-validation/
  baselines/
    <SampleName>/
      original-<screen>.png
      original-<screen>.json
      original-logs.txt
  evolved/
    <SampleName>/
      evolved-<screen>.png
      evolved-<screen>.json
      evolved-logs.txt
  comparisons/
    <SampleName>/
      compare-<screen>.png
  issues/
    <SampleName>.md
  reports/
    sample-validation-report.md
```

Required retained artifacts per sample:

- original screenshots
- evolved screenshots
- side-by-side comparison images
- runtime logs
- interaction notes / flow checklist results
- issue list with discovered problems and dispositions

## Current Execution Status

- **Phase 10 is active.** The work is no longer in planning mode; execution is underway across P0 runtime verification and the next migration wave.
- **Baseline build chain is green** for the current framework/sample build pass.
- **P0 reviewer state is partial only.**
  - `CometMauiApp` may currently claim launch/render-progress evidence.
  - `CometBaristaNotes` may currently claim render-progress evidence only.
  - Neither sample may currently claim approved end-to-end interactive runtime validation.
- **Reviewer gate outcome:** Bobbie rejected full-runtime signoff for the current P0 validation artifact and required a different revision owner. The next P0 runtime-host revision is therefore assigned to Holden, not Amos.
- **Current active lanes:**
  - Holden — root-view debug-host/runtime-host revision to move P0 from render-only evidence toward interactive validation
  - Amos — next tractable sample migration wave outside the rejected P0 artifact
  - Scribe — logging reviewer gates, decisions, and phase progress
- **Current blocker:** shared DEBUG runtime hosting still wraps `CometApp` roots (`MyApp`, `BaristaApp`) via `CometHost`, and MauiDevFlow interaction remains unreliable when the tree is hidden/disabled.
- **Board note:** GitHub issue-board polling is currently blocked in this checkout until `gh repo set-default` is configured.

## Sample Inventory

### P0 baseline samples

#### 1. `CometMauiApp`
- **Type:** Comet
- **Surface status:** Evolved
- **Comparison:** original `CometMauiApp` vs evolved `CometMauiApp`
- **Minimum flows:** launch, counter visible, increment, decrement, reset, slider/toggle behavior
- **Likely work:** strengthen as the clean reference implementation and baseline proof that the evolved surface works

#### 2. `CometBaristaNotes`
- **Type:** Comet (port of a MauiReactor app)
- **Surface status:** Broken — UI was fabricated by agents, does not match real app
- **Ground truth:** `~/work/BaristaNotes` (MauiReactor app by David Ortinau). This is the ONLY reference. The Comet version must faithfully reproduce its UI and behavior.
- **Comparison:** real BaristaNotes app (`~/work/BaristaNotes`, MauiReactor) → `CometBaristaNotes` (Comet MVU port)
- **Original structure:** 3 tabs — "New Shot" (ShotLoggingPage), "Activity" (ActivityFeedPage), "Settings" (SettingsPage). Shell navigation. Detail routes for beans, bags, equipment, profiles.
- **Minimum flows:** launch, 3 tabs visible, shot logging with form inputs, activity feed display, settings, detail navigation (bean/bag/equipment), return navigation
- **Required work:** Strip all fabricated UI (CoffeeDashboardPage, mock AI/Speech/Vision services, educational copy). Restore Shell-based 3-tab structure matching the real app. Keep Amos's framework crash fix (ConditionalWeakTable in AppHostBuilderExtensions.cs).

### P1 high-value samples

#### 3. `Comet.Sample`
- **Type:** Comet
- **Surface status:** Legacy
- **Comparison:** original `Comet.Sample` vs evolved `Comet.Sample`
- **Minimum flows:** launch, demo landing/navigation, representative control demos, state-changing interactions across major categories
- **Likely work:** modernize representative screens to the evolved API and verify the broadest demo coverage

#### 4. `CometFeatureShowcase`
- **Type:** Comet
- **Surface status:** Legacy
- **Comparison:** original `CometFeatureShowcase` vs evolved `CometFeatureShowcase`
- **Minimum flows:** launch, feature navigation, interaction through each showcased capability
- **Likely work:** rewrite showcase paths to better teach `Component<TState>`, `Render()`, and updated navigation/state patterns

#### 5. `CometProjectManager`
- **Type:** Comet
- **Surface status:** Mixed
- **Comparison:** original `CometProjectManager` vs evolved `CometProjectManager`
- **Secondary context:** `MauiReference` can be used as a MAUI-only reference where useful, but it is not the primary before/after pair
- **Minimum flows:** launch, shell/tab/nav flow, project/task navigation, theme/toolkit interaction, edit/update flows
- **Likely work:** align its larger app surface with the evolved API while preserving feature breadth

#### 6. `MauiReference`
- **Type:** Pure MAUI reference
- **Surface status:** N/A (not Comet)
- **Comparison:** original `MauiReference` vs evolved `MauiReference` only if touched for parity/supporting validation
- **Minimum flows:** launch, key navigation/render paths used as reference comparison
- **Likely work:** primarily validation/reference usage, not a Comet-surface migration target

### P2 specialized samples

#### 7. `CometAllTheLists`
- **Type:** Comet
- **Surface status:** Legacy
- **Comparison:** original `CometAllTheLists` vs evolved `CometAllTheLists`
- **Minimum flows:** launch, each list style visible, selection/scroll/update behavior, empty/populated states where present
- **Likely work:** update list patterns toward current MAUI/Comet guidance and verify list fidelity

#### 8. `CometTaskApp`
- **Type:** Comet
- **Surface status:** Legacy
- **Comparison:** original `CometTaskApp` vs evolved `CometTaskApp`
- **Minimum flows:** launch, task navigation, create/edit/complete flows, tab/state persistence behavior
- **Likely work:** refactor app-level state and navigation surfaces toward the evolved API

#### 9. `CometWeather`
- **Type:** Comet
- **Surface status:** Legacy
- **Comparison:** original `CometWeather` vs evolved `CometWeather`
- **Minimum flows:** launch, weather summary visible, refresh/update path, navigation/details if present
- **Likely work:** modernize reactive/state presentation and ensure clean runtime rendering

#### 10. `CometStressTest`
- **Type:** Comet
- **Surface status:** Legacy
- **Comparison:** original `CometStressTest` vs evolved `CometStressTest`
- **Minimum flows:** launch, category selection, representative stress scenarios execute without blanking or layout corruption
- **Likely work:** validate that evolved API changes do not regress heavier or more pathological UI paths

## Execution Phases

### Phase A — Baseline capture and tooling readiness

- verify `maui-ai-debugging` + `maui-devflow` workflow per sample
- add minimal DEBUG-only MauiDevFlow agent support where missing
- build and launch each sample in current state
- capture original screenshots, logs, and runtime tree evidence
- note any immediate faults before refactoring

**Acceptance criteria**
- every sample has an original evidence folder
- every sample either launches or has captured failure evidence
- no sample moves into refactor work without a preserved baseline

### Phase B — Sample modernization / evolution pass

- refactor samples as needed to better demonstrate the updated Comet API
- favor `Component<TState>`, `Render()`, `Reactive<T>`, `SetState(...)`, typed navigation, and current MAUI-safe patterns
- keep behavior fidelity where the sample already has a clear user-facing intent
- update sample docs/readmes when the sample surface meaningfully changes

**Acceptance criteria**
- each touched sample clearly demonstrates the evolved surface more cleanly than before
- legacy-only patterns are reduced where appropriate
- no refactor is declared complete without rerunnable validation steps

### Phase C — Full verification and bug-discovery loop

For every sample:

1. run the app
2. exercise the defined flows
3. capture runtime tree/log evidence
4. note issues discovered
5. fix the issues
6. rerun the same flows
7. recapture screenshots/logs after the fix

**Acceptance criteria**
- every sample has a completed flow checklist
- every bug found during validation is either fixed or explicitly called out as remaining work for user review
- any sample with unresolved failures cannot be reported as “verified”

### Phase D — Fidelity review and comparison set

- produce side-by-side original/evolved screenshots for key screens in each sample
- compare visible layout/content behavior for regressions and improvements
- note where fidelity intentionally changed because of the evolved API or MAUI 10 guardrails

**Acceptance criteria**
- every sample has a side-by-side comparison set
- every intentional visual/behavioral delta is explained
- no unexplained regression remains in the comparison set

### Phase E — Final report and squad-state cleanup

- assemble a full report of sample status, issues found, fixes applied, and evidence paths
- update squad state so it no longer overclaims prior Phase 9 validation coverage
- record what was fully validated, what changed, and what still remains

**Acceptance criteria**
- final report references retained evidence for every sample
- squad state distinguishes build/structure gates from full runtime validation
- the user can review screenshots later without rerunning the apps

## Bug Discovery Loop

The execution plan must enforce this loop:

1. **discover** a problem during runtime validation
2. **capture** screenshot, logs, and reproduction notes
3. **classify** the issue (blocking / serious / minor)
4. **fix** the issue
5. **rerun** the affected flow
6. **re-screenshot** and retain the corrected state
7. **record** the issue and fix in the final report

No sample is considered done after a bug is merely identified. Discovery without correction is only acceptable if it is explicitly surfaced as unresolved and blocks sign-off.

## Completion Criteria

Execution may only claim success when all of the following are true:

- all in-scope samples have been built and launched
- all required runtime flows have been exercised
- all baseline screenshots have been captured
- all evolved screenshots have been captured
- all side-by-side comparisons have been generated
- issues discovered during validation have been fixed or explicitly carried as unresolved blockers
- evidence has been retained in the session workspace
- report-ready notes exist for every sample

If even one sample still fails launch, renders blank, or lacks retained comparison evidence, the work is **not complete**.

## Squad-State Follow-up

Once execution happens, the squad state must stop implying that the earlier Phase 9 closure was enough on its own. The eventual update should distinguish:

- earlier **build/structure validation**
- new **full runtime/evidence-backed sample verification**

This follow-up should likely be framed as a post-Phase-9 stabilization/verification effort unless execution proves that the prior closure language can still be justified after the new full-sample validation pass.
