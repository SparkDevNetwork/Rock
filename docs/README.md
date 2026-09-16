# Rock RMS Documentation

This directory holds the living "as built" technical reference for Rock RMS, organized by domain. Audience: core Rock developers, plugin authors, and quasi-technical community members. Voice: terse, code-anchored, why-first.

If you are new to a corner of the codebase, start at the relevant domain's `README.md` and follow it to the overview doc. Overview docs are deliberately top-loaded with the why and the conceptual model; the dense file:line references live in the Technical Reference section at the bottom for active contributors.

These docs are maintained by the [`docs` skill](../.claude/skills/docs/SKILL.md). Dates, citations, and "Recent Impactful Changes" entries should always be sourced from `git log`, never approximated.

## Domains

| Domain | Folder | What it covers |
|---|---|---|
| AI | [ai/](ai/) | Provider abstraction, Agent infrastructure (ChatAgent, Skills, Tools), Automations, MCP integration. |
| Check-in | [check-in/](check-in/) | Kiosk and self-service attendance recording, opportunity filters, label printing, v2 vs legacy engines. |
| CMS | [cms/](cms/) | Pages, Blocks, Layouts, Sites, Content Channels, Content Collections, Lava Applications, Personalization, Adaptive Messages, Media. |
| Communication | [communication/](communication/) | Bulk Communication, SystemCommunication, Communication Flows, SMS Pipeline, Snippets, System Phone Numbers. |
| Connection | [connection/](connection/) | Connection Type/Opportunity/Request, status automation, workflows, the Connections Hub and Board. |
| Core | [core/](core/) | Cross-cutting concepts that recur across domains. Currently: entity reference resolution (IdKey vs raw int). |
| CRM | [crm/](crm/) | Person, PersonAlias, Family-as-Group, search keys, duplicates, badges, assessments, background checks, record source. |
| Engagement | [engagement/](engagement/) | Streaks, Steps, Achievements, Outreach Toolbox. (LMS shares this domain folder for entities; see the LMS docs separately.) |
| Event | [event/](event/) | Event Calendar, EventItem, Registration Template/Instance/Registration, Interactive Experiences. |
| Farm | [farm/](farm/) | WebFarm multi-node coordination: cache invalidation, leader election, pluggable Bus. |
| Finance | [finance/](finance/) | Transactions, Batches, Accounts, Pledges, Scheduled Transactions, Gateways, Statements, Benevolence. |
| Group | [group/](group/) | Groups, GroupTypes, GroupMembers, Requirements, Sync, Scheduling, Attendance, Locations, History, Caching. |
| Lava | [lava/](lava/) | Templating engine, filters, blocks/tags, shortcodes, commands, security gating, Fluid migration. |
| LMS | [lms/](lms/) | Learning Management: programs, courses, classes, semesters, activities, completions, grading. |
| Mobile | [mobile/](mobile/) | Mobile-app block surface: separate from web/Obsidian, shares services and entities. |
| Prayer | [prayer/](prayer/) | Prayer Request lifecycle, approval, mobile/public blocks, AI Automation integration. |
| Reporting | [reporting/](reporting/) | DataViews, Reports, Metrics, Dynamic Data, Analytics Source tables. |
| Workflow | [workflow/](workflow/) | Workflow Type/Activity/Action, runtime, triggers, Form Builder. |

## Conventions

- **Folders are lowercase-kebab.** `check-in/`, `crm/`, `lms/`. Matches the convention from `specs/completed/`.
- **`README.md` per folder** is the directory index. Auto-maintained by the docs skill.
- **`{topic-kebab}.md` for individual docs.** Noun-phrases scoped to a feature, not whole subsystems.
- **`*-overview.md`** is the orientation doc for a domain.
- **Frontmatter has `title`, `last_updated`, optional `related_specs` and `related_files`** (both repo-root-relative paths).
- **Source citations use `path/to/file.cs:line`** (auto-linked by the harness). Historical citations (commits) use GitHub URLs; commits are immutable.
- **Recent Impactful Changes is sourced from `git log`.** Dates that cannot be verified are omitted, never approximated.

The canonical list of domains and their casing variants lives in [.claude/rules/rock-domains.md](../.claude/rules/rock-domains.md).
