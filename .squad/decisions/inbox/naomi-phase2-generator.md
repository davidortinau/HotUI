# Decision: Phase 2 — PropertySubscription<T> in Generated Controls

**Owner:** Naomi (Source Generator Dev)
**Date:** 2025-07-25
**Status:** Implemented

## Decision

Generated controls use `PropertySubscription<T>` for non-delegate key property fields and properties. Delegate properties (Action/Func) remain on `Binding<T>`. The Binding<T> constructor overload is kept via implicit conversion from `Binding<T>` → `PropertySubscription<T>`.

## Key Design Choices

1. **Conditional field types:** `isDelegate` flag determines whether a parameter's field/property uses `Binding<T>` (delegates) or `PropertySubscription<T>` (data properties). This is computed in the model data as `FieldDeclaration` and `PropertyDeclaration` strings, replacing the static Mustache template text.

2. **No Signal<T> extension overloads:** Adding `Signal<T>` overloads to fluent extension methods creates CS0121 ambiguity because both `Binding<T>` and `Signal<T>` have `implicit operator T→Self`. Signal support is through constructors only. Extension methods stay on `Binding<T>` + `Func<T>` for environment storage.

3. **Implicit conversion Binding<T> → PropertySubscription<T>:** Wraps `binding.CurrentValue` as a static PropertySubscription. This loses reactive tracking for the legacy Binding constructor path — acceptable since that path is deprecated in Phase 3.

4. **PropertySubscription<T> API additions:** `CurrentValue` property (alias for `Value`), `Set(T)` method (for interface implementations), and `FromValue`/`FromFunc`/`FromSignal` factory methods.

## Impact

- All 19 generated controls compile with PropertySubscription<T>
- 968 tests pass, 0 regressions (11 pre-existing failures unchanged)
- Gallery sample and main sample build clean
- ImageButton.cs handwritten partial updated for generated field type change
