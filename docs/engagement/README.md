# Engagement Documentation

Engagement is the umbrella for Rock's "is the person growing" tracking systems: Streaks (binary did/did-not patterns), Steps (discipleship pathway tracking), Achievements (gamified badges), and the Outreach Toolbox (relational ministry). LMS shares this domain folder under `Rock/Model/Lms/` and has its own overview at [../lms/lms-overview.md](../lms/lms-overview.md).

If you are new, start with [engagement-overview.md](engagement-overview.md). Sub-topics worth their own docs (Streak Type configuration, Step Programs, Achievement Components, Outreach Toolbox, LMS-vs-Engagement boundary) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Achievements](achievements.md) | Earnable badges, AchievementType / AchievementAttempt model, prerequisite chains, component-based criteria. |
| [Engagement Domain Overview](engagement-overview.md) | Four parallel subsystems (Streaks, Steps, Achievements, Outreach), the bitmap streak model, and the relationship to LMS. |
| [Outreach Toolbox](outreach-toolbox.md) | Personal ministry tool, Contact + Touchpoint model, mobile-first design, relationship change tracking. |
| [Step Programs and Pathways](step-programs-and-pathways.md) | StepProgram / StepType / Step three-tier model, prerequisites, completion tracking, Step Analytics. |
| [Streak Types](streak-types.md) | Bitmap-based engagement tracking, cadence-driven bit width, exclusions for blackout periods, computed streak length. |
