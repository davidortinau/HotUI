# Decision: Source generator overload binding strategy

- **Date:** 2026-03-10
- **Owner:** Naomi
- **Status:** Implemented

## Decision
Static value overloads now construct explicit `Binding<T>` instances (null-guarded) instead of relying on implicit `T -> Binding<T>` conversion. Optional value-type parameters are emitted as nullable (`T?`) and unwrap `.Value` when building bindings.

## Context
Implicit conversion failed for certain interface types (e.g., `IImageSource`) and nullable default values for value-type parameters produced invalid signatures. Explicit bindings and nullable signatures keep overloads reliable while preserving optional parameter convenience.
