# Comet Documentation

Documentation for the Comet MVU framework -- a declarative, code-only UI layer
built on .NET MAUI. Views are defined as C# functions, state changes
automatically trigger re-rendering, and platform-native controls are used
throughout.


## Guides

These are the primary docs for developers building with Comet.

- [Reactive State Guide](reactive-state-guide.md) -- Practical, code-forward
  guide to every state management pattern: `State<T>`, `BindingObject`,
  `Binding<T>`, and automatic dependency tracking.
- [State Management](state-management.md) -- Definitive deep-dive covering the
  full reactive pipeline, `INotifyPropertyRead`, environment propagation, and
  advanced patterns.
- [Migration Guide](migration-guide.md) -- How to move from the prior Comet API
  surface to the evolved MVU API without renaming the project.


## Architecture and Design

Design proposals, ADRs, and technical analysis that informed implementation
decisions. Useful for contributors and anyone working on the framework internals.

- [ADR: Dual Reactive State Tracking](architecture/adr-dual-tracking-systems.md)
  -- Decision record for maintaining both classic and signal-based tracking.
- [State Management v2 Proposal](architecture/state-management-proposal.md) --
  Engineering design for the next-generation state system (Rev 4).
- [State Tracking Unification Analysis](architecture/state-unification-analysis.md)
  -- Technical analysis of unifying the two tracking systems.
- [Source Generator Design (Phase 2)](architecture/phase2-generator-design.md) --
  Template design for the Roslyn source generator under state unification.
- [Style and Theme Spec](architecture/STYLE_THEME_SPEC.md) -- Technical
  specification for `ControlStyle<T>`, `ViewModifier`, and theme propagation.
- [Slider Drag Bug RCA](architecture/slider-drag-bug-rca.md) -- Root cause
  analysis of the slider stuck-drag issue and the fix.


## Research and Comparison

Background research, multi-model analysis, and independent reviews that fed into
design decisions. Reference material, not prescriptive.

- [Comet vs MauiReactor Comparison](research/state-management-comparison.md) --
  Side-by-side analysis of both frameworks' state management systems.
- [State Management Deep Dive (GPT)](research/state-management-gpt.md) --
  Comprehensive GPT-generated analysis of the Comet state system.
- [State Management Deep Dive (Opus)](research/state-management-opus.md) --
  Independent Opus-generated analysis of the same system.
- [State Management Fact-Check](research/state-management-factcheck.md) --
  Line-by-line verification of code blocks and claims in the state docs.
- [Style/Theme API Comparison](research/STYLE_THEME_COMPARISON.md) -- Comet vs
  MauiReactor vs SwiftUI styling and theming APIs.

### Proposal Reviews

Independent reviews of the state management v2 proposal across multiple rounds:

- [Architect Review](research/state-management-proposal-architect-review.md)
- [Skeptic Review (Round 1)](research/state-management-proposal-skeptic-review.md)
- [Skeptic Review (Round 2)](research/state-management-proposal-skeptic-review-r2.md)
- [Skeptic Review (Round 3)](research/state-management-proposal-skeptic-review-r3.md)

### Style/Theme Spec Reviews

- [GPT-5.4 Review](research/reviews/SPEC_REVIEW_GPT54.md)
- [Gemini Review](research/reviews/SPEC_REVIEW_GEMINI.md)
- [GPT-5.4 Final Review](research/reviews/SPEC_FINAL_REVIEW_GPT54.md)
- [Review Response (Holden)](research/reviews/REVIEW_RESPONSE.md)


## Internal

Implementation tracking, audit reports, coverage matrices, and remediation plans
consumed during development. Retained for project history.

- [Implementation Status](internal/IMPLEMENTATION_STATUS.md)
- [Implementation Summary](internal/IMPLEMENTATION_SUMMARY.md)
- [Verification Checklist](internal/VERIFICATION_CHECKLIST.md)
- [Sample Validation Report](internal/SAMPLE_VALIDATION_REPORT.md)
- [MAUI 9.0 Audit Report](internal/MAUI_9_0_AUDIT_REPORT.md)
- [MAUI 9.0 Coverage Checklist](internal/MAUI_9_0_COVERAGE_CHECKLIST.md)
- [MAUI 9.0 Coverage Matrix](internal/MAUI_9_0_COVERAGE_MATRIX.txt)
- [MAUI SDK Coverage Audit](internal/MAUI_SDK_COVERAGE_AUDIT.md)
- [Comprehensive Audit Index](internal/COMPREHENSIVE_AUDIT_INDEX.md)
- [Audit Index](internal/AUDIT_INDEX.md)
- [Audit Summary](internal/AUDIT_SUMMARY.txt)
- [Audit Execution Summary](internal/AUDIT_EXECUTION_SUMMARY.txt)
- [Control Coverage (Phase 8.1)](internal/CONTROL_COVERAGE_PHASE_8_1.md)
- [Phase 4 Validation Report](internal/PHASE4_VALIDATION_REPORT.md)
- [Phase 6.2 Interop Baseline](internal/PHASE_6_2_INTEROP_BASELINE.md)
- [Option B Implementation Roadmap](internal/OPTION_B_IMPLEMENTATION_ROADMAP.md)
- [Remediation Plan](internal/REMEDIATION_PLAN.md)
- [Remediation Roadmap (SDK Only)](internal/REMEDIATION_ROADMAP_MAUI_SDK_ONLY.md)
- [Remediation Implementation Log](internal/REMEDIATION_IMPLEMENTATION.md)
