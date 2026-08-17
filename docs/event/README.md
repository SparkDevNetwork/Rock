# Event Documentation

Event is Rock's calendar, registration, and interactive-experience system. Two parallel hierarchies: `EventCalendar` -> `EventItem` -> `EventItemOccurrence` for public-facing events, and `RegistrationTemplate` -> `RegistrationInstance` -> `Registration` -> `RegistrationRegistrant` for signups. Both connect to `Attendance` (shared with Group and Check-in domains).

If you are new, start with [event-overview.md](event-overview.md). Sub-topics worth their own docs (Registration Templates, Registration Entry Flow, Eligibility Rules, Discounts/Fees, Interactive Experiences, Calendar Lava) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Calendar and Occurrences](calendar-and-occurrences.md) | Three-layer calendar model, multi-calendar membership, audiences, Group / ContentChannelItem links. |
| [Eligibility and Discounts](eligibility-and-discounts.md) | `RegistrantEligibilityEvaluator`, configurable eligibility rules, discount types (sibling / early-bird / code), stacking. |
| [Event Domain Overview](event-overview.md) | Two-hierarchy model, the Registration save lifecycle, eligibility rules, signature documents, and Interactive Experiences. |
| [Interactive Experiences](interactive-experiences.md) | Real-time audience-response subgraph, action types (Poll/Prayer/Q&A), per-campus scheduling, anonymous response support. |
| [Registration Entry Flow](registration-entry-flow.md) | The user-facing flow, server-side validation, capacity recheck, payment-before-commit, RegistrationSession resume. |
| [Registration Template Design](registration-template-design.md) | Template / Instance separation, forms, fees, eligibility rules, Prevent Duplicate Registrants, signature documents. |
