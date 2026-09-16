---
description: Build Rock.sln and report errors. Use when the user says "build", "compile", "check build", or after making code changes that may have introduced errors.
---

Build the Rock.sln solution using `dotnet build`. Report:
- Whether the build succeeded or failed
- Any errors (with file path and line number)
- Any warnings that look like real issues (ignore nullable reference warnings unless they're in files I recently changed)

If the build fails, diagnose the root cause and suggest a fix. Do not fix anything automatically — just report.
