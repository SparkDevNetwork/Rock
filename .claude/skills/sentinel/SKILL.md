---
name: sentinel
description: >-
  Run a read-only Rock security review with the Sentinel agent. Use when the user says
  "sentinel", "security review", "security check my changes", "is this secure", or
  "review the X feature for security". Works on a PR, a commit, uncommitted changes, a
  file, a folder, or a whole feature. Codex and Grok second opinions run automatically
  when installed; add "--no-cross-check" to skip them. Do NOT use for correctness or
  style review.
argument-hint: "[what to review, e.g. 'PR 1234' or 'the Giving Overview block'] [--no-cross-check]"
context: fork
agent: sentinel
---

Run a Sentinel security review.

The developer's request: $ARGUMENTS

If the request is empty, review uncommitted changes, or the last commit if there are none.
If it contains "--no-cross-check", skip the Codex and Grok cross-check this run.
