# Decision: Reactive analyzer v1 scope

- **Date:** 2026-03-10
- **Owner:** Naomi
- **Status:** Implemented

## Decision
COMET001/002 run only on fields in types inheriting `Comet.View` and require readonly Signal/Computed fields. COMET003 flags `Signal<T>.Value` reads outside `[Body]` methods or `new Computed(...)`/`new Effect(...)` lambdas, and ignores writes or increment/decrement expressions to reduce false positives.

## Context
Phase 2 required analyzer coverage with minimal noise. Limiting COMET001/002 to View-derived types follows the migration guidance, and the COMET003 scope aligns with the initial reactive tracking contexts without attempting deeper flow analysis.
