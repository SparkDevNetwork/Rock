# Rock Domains

Canonical list of Rock domains and their casing variants. Always loaded as a project instruction.

This is the single source of truth referenced by:
- The `bugfix` skill (release-note classification, path-to-domain mapping).
- The `spec` skill (`specs/completed/{folder}/` directory structure, INDEX.md `Domain` column).
- The `docs` skill (`docs/{folder}/` directory structure, README headings).
- Any future commit skill.
- The "Commit Messages" section of `CLAUDE.md`.

Do not duplicate this list anywhere else. Reference this file.

---

## The List

Three casings exist for the same domain. Use the form appropriate to the context.

| Release-note form | Folder name | Namespace / Enum form |
|---|---|---|
| `AI` | `ai` | `AI` |
| `API` | `api` | (no direct namespace; use `Net` or domain-specific) |
| `CMS` | `cms` | `Cms` |
| `Check-in` | `check-in` | `CheckIn` |
| `Communication` | `communication` | `Communication` |
| `Connection` | `connection` | `Connection` |
| `Core` | `core` | `Core` |
| `CRM` | `crm` | `Crm` |
| `Engagement` | `engagement` | `Engagement` |
| `Event` | `event` | `Event` |
| `Farm` | `farm` | `WebFarm` |
| `Finance` | `finance` | `Finance` |
| `Group` | `group` | `Group` |
| `Lava` | `lava` | `Lava` (under `Rock.Lava`) |
| `LMS` | `lms` | `Lms` |
| `Mobile` | `mobile` | `Mobile` |
| `Prayer` | `prayer` | `Prayer` |
| `Reporting` | `reporting` | `Reporting` |
| `Workflow` | `workflow` | `Workflow` |
| `Other` | `other` | (n/a) |

### Where each form is used

- **Release-note form** — commit messages (`+ (Domain) ...`), the `Domain` column of `specs/completed/INDEX.md`, the H1 of every `docs/{folder}/README.md`. Human-facing.
- **Folder name** — every directory under `specs/completed/` and `docs/`. Lowercase, hyphens for spaces, no exceptions. Path-safe and case-insensitive-filesystem-safe.
- **Namespace / Enum form** — C# `[RockDomain]` and `[Enums.EnumDomain]` attributes (the latter is legacy-only — it appears solely on enums still declared in the `Rock.Model` namespace; new enums live in `Rock.Enums.[Domain]` and carry no such attribute), file paths under `Rock.Enums/{Domain}/`, namespace placement. PascalCase. Includes some domains that do NOT appear in the release-note list (`Blocks`, `Controls`, `Geography`, `Net`, `Observability`, `Security`) because those are code organization, not user-visible feature areas. See `.claude/rules/code-conventions.md` for the complete namespace list.

---

## Path-to-Domain Mapping

When a change touches code at a known path, the domain is usually inferable. This is the same table the `bugfix` skill uses for picking a domain from a bug location.

| Path contains | Release-note domain |
|---|---|
| `/AI/` | `AI` |
| `/Api/` or REST controllers | `API` |
| `/Cms/` or `/Blocks/Cms/` | `CMS` |
| `/CheckIn/` | `Check-in` |
| `/Communication/` | `Communication` |
| `/Connection/` | `Connection` |
| `/Core/` | `Core` |
| `/Crm/` | `CRM` |
| `/Engagement/` | `Engagement` |
| `/Event/` | `Event` |
| `/WebFarm/` | `Farm` |
| `/Finance/` | `Finance` |
| `/Group/` | `Group` |
| `/Lava/` | `Lava` |
| `/Lms/` | `LMS` |
| `/Mobile/` | `Mobile` |
| `/Prayer/` | `Prayer` |
| `/Reporting/` | `Reporting` |
| `/Workflow/` | `Workflow` |
| Cross-cutting or unclear | `Other` |

When a change spans two paths, pick the one closest to the user-visible feature, not the supporting infrastructure. A bug in a `Finance` block that happens to live in a `Core` cache layer is a `Finance` change.

---

## Hard Rules

- **Do not invent new domains.** The list above is exhaustive for release-note and folder use. If a topic genuinely does not fit, use `Other` / `other`.
- **Do not mix forms within one context.** A folder name is always lowercase. A release-note domain is always release-note casing. Picking the wrong form is a bug.
- **Do not silently translate.** When a skill needs to convert between forms, it should be explicit about which form it is using and why.
