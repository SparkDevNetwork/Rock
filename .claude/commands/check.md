---
description: Pre-commit verification (build + test + diff review). Use before committing, when the user says "check", "verify", "pre-commit", or wants to validate changes before pushing.
---

Pre-commit verification. Run these checks in order and stop at the first failure:

1. **Build** — `dotnet build Rock.sln` (must succeed)
2. **Tests** — `dotnet test Rock.Tests/Rock.Tests.csproj --no-build` (must pass)
3. **Diff review** — Run `git diff --cached` (or `git diff` if nothing is staged) and scan for:
   - Missing copyright headers on new files
   - `DateTime` usage instead of `RockDateTime`
   - `System.Web` references outside `#if WEBFORMS` blocks
   - `lock()` statements (flag for review)
   - Public methods that should be internal
   - Missing XML doc comments on public methods

Report a pass/fail summary. For failures, list each issue with file and line number.
