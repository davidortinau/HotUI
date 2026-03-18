### 2026-03-16T00:55:00Z: User directive — NON-NEGOTIABLE end-to-end validation
**By:** David Ortinau (via Copilot)
**What:** Before ANY implementation can be claimed as "tested" or "complete", the team MUST run the actual sample app (Comet.Sample / control gallery) on a real simulator or device and perform end-to-end validation with actual clicks, taps, typing, and navigation. Unit tests and `dotnet build` are necessary but NOT sufficient. The `maui-ai-debugging` skill and `maui-devflow` tool (or Appium as fallback) MUST be used to interact with the running app and verify the UI works correctly. No exceptions. No shortcuts. This applies to every agent on the team.
**Why:** User requirement — zero tolerance for "builds and tests pass" claims without runtime visual validation. This has been a recurring gap.
**Scope:** ALL agents, ALL phases, ALL future work. This is a permanent team-wide directive.
**Enforcement:** If an agent cannot perform runtime validation (e.g., no simulator available), they must explicitly say so and mark the work as "builds but NOT runtime-validated" — never claim it's complete.
