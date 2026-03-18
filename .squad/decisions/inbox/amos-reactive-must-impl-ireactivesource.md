# Decision: Reactive<T> must implement IReactiveSource

**Author:** Amos (Controls & API Dev)
**Date:** 2026-03-16
**Status:** Implemented

## Context

After the reactive system migration, `View.GetRenderViewReactive()` uses `ReactiveScope` to track body dependencies. Any state type read during a body build must implement `IReactiveSource` and call `ReactiveScope.Current?.TrackRead(this)` in its getter — otherwise changes to that state are invisible and the view never rebuilds.

`Reactive<T>` extends the deprecated `State<T>` which only uses the old `BindingObject.CallPropertyRead()` path. This caused sidebar navigation (and any view using `Reactive<T>`) to be completely unresponsive.

## Decision

`Reactive<T>` now implements `IReactiveSource`. Its `Value` property participates in both the old (`CallPropertyRead`) and new (`ReactiveScope.TrackRead`) tracking systems.

## Impact

- **Holden:** Any future state bridge classes must implement `IReactiveSource`.
- **All agents:** When creating new observable/state types, always implement `IReactiveSource` if they may be read inside a `[Body]` method.
- **Testing:** The `Reactive<T>` path should have a dedicated unit test confirming body rebuilds on `.Value` changes.
