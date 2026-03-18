# Decision: comet-migrate tool scope (v1)

- **Date:** 2026-03-10
- **Owner:** Naomi
- **Status:** Implemented

## Decision
The v1 `comet-migrate` tool uses MSBuildWorkspace to scan project documents, rewrites `State<T>` field declarations to `Signal<T>` with target-typed `new(...)` initializers, and wraps constructor arguments of Comet controls that pass `.Value` directly into `() => ...` lambdas. It inserts `using Comet.Reactive;` in modified files and removes `using Comet;` only when no non-reactive Comet symbols remain.

## Context
Phase 2 requires a mechanical migration aid without deep semantic flow analysis. Constraining the lambda wrapping to `Comet.*` types avoids unintended changes in unrelated constructors while still handling the common `new Text(state.Value)` trap.
