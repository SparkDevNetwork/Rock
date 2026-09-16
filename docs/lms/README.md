# LMS Documentation

LMS is Rock's Learning Management System: programs, courses, classes, semesters, activities, completions, and grading. Used for staff training, volunteer onboarding, theological education, and structured curriculum. The model is academic-shaped: `LearningProgram` -> `LearningCourse` -> `LearningClass` with enrolled `LearningParticipant`s and `LearningClassActivity` completions.

LMS shares the Engagement domain folder for entities under `Rock/Model/Lms/` but has its own user-facing surface and its own overview here.

If you are new, start with [lms-overview.md](lms-overview.md). Sub-topics worth their own docs (Activity Components, Grading Systems, Public Block Security, Academic Calendars, Smart Scroll) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [LMS Activity Components](activity-components.md) | Pluggable activity types (Acknowledgment, Assessment, ContentArticle, FileUpload, VideoWatch, custom), per-component grading, retake-support advertisement. |
| [LMS Domain Overview](lms-overview.md) | Academic five-layer model, activity component pluggability, late-detection-by-submission-time, and file-retention-until-grading. |
| [LMS Grading Systems](grading-systems.md) | Configurable grading systems and scales, per-Class assignment, statistics aggregation, late-submission handling, assessment retakes. |
| [LMS Public Block Security](public-block-security.md) | Per-entity authorization on public blocks, Smart Scroll workspace UX, anonymous vs authenticated access. |
