---
description: Run Rock.Tests and report results. Use when the user says "run tests", "test", or wants to verify changes haven't broken anything.
---

Run the Rock.Tests project using `dotnet test Rock.Tests/Rock.Tests.csproj --no-build --verbosity normal`. If the solution hasn't been built yet, build first with `dotnet build`.

Report:
- Total tests run, passed, failed, skipped
- For any failures: test name, assertion message, and the relevant source location
- If all tests pass, say so concisely

Do not fix failing tests automatically — just report the results.
