# Scoping

How to turn the user's input into a list of changes, inventory them, and confirm what the plan covers.

## Resolving the input

| Input | Commands | Notes |
|---|---|---|
| Nothing, "unstaged", "my changes", "what I'm about to commit" | `git diff HEAD --stat`, `git diff HEAD`, `git ls-files --others --exclude-standard` | Covers staged, unstaged and untracked files. Read untracked files in full; they are new code. |
| "staged" | `git diff --cached` | |
| One commit | `git show --stat <hash>`, `git show <hash>` | |
| Several commits | `git show` each | Keep them in the order given. |
| Range `A..B` | `git log --no-merges --cherry-pick --right-only --format='%h %s' A...B`, then `git diff --stat A...B` | `--cherry-pick --right-only` drops commits whose patch already exists on A (fixes merged forward from a hotfix branch that A already has). Use the three-dot form for both. |
| Two versions ("19.6 and 20.0") | Resolve each to a ref first, then treat as a range. | See below. |

### Resolving a version to a ref

Rock uses both tags and branches:

- Tags mark builds: `19.4.4`, `20.0.9-alpha`, `20.0.7-pre`. List with `git tag --sort=-creatordate`.
- Branches: `hotfix-19.6`, `release-20.0`, `develop`.

"Version 19.6" usually means the released 19.6 build. Prefer the newest non-prerelease tag that matches (`19.6.N`). If there is none, fall back to `origin/hotfix-19.6`. "20.0" as the target of an upcoming release usually means `origin/release-20.0`. **Always state which refs you resolved and get confirmation** as part of the scope question, because a wrong base ref silently changes the whole plan. Run `git fetch` first if the refs might be stale (ask before fetching if the user may be offline).

## Inventory

Classify every changed file into a layer, then a domain. Present the result as a table of counts (layer rows, with a few top domains or a domain column for large ranges) before asking about scope.

### Layers

| Layer | Paths |
|---|---|
| Obsidian blocks | `Rock.Blocks/**` (except `Mobile/`, `Tv/`), `Rock.ViewModels/Blocks/**`, `Rock.JavaScript.Obsidian.Blocks/src/**` |
| Mobile and TV blocks | `Rock.Blocks/Mobile/**`, `Rock.Blocks/Tv/**` |
| WebForms blocks | `RockWeb/Blocks/**` |
| UI controls and framework | `Rock.JavaScript.Obsidian/Framework/**` (except `ViewModels/`), `Rock/Web/UI/**` |
| REST API | `Rock.Rest/**` |
| Model and service layer | `Rock/Model/**`, `Rock.ViewModels/**` outside `Blocks/` |
| Jobs | `Rock/Jobs/**` |
| Workflow actions | `Rock/Workflow/**` |
| Field types | `Rock/Field/**` |
| Lava | `Rock/Lava/**`, `Rock.Lava*/**` |
| Check-in | `Rock/CheckIn/**`, `Rock.Blocks/CheckIn/**` (also counts as Obsidian blocks) |
| Finance and communication engines | `Rock/Financial/**`, `Rock/Communication/**` |
| Cache and other core | everything else under `Rock/**`, `Rock.Common/**`, `Rock.Enums/**` |
| Migrations and data | `Rock.Migrations/**`, `Rock/Plugin/HotFixes/**`, SQL scripts |
| Styles and themes | `RockWeb/Styles/**`, `RockWeb/Themes/**`, `*.less`, `*.scss`, `*.css` |
| Automated tests | `Rock.Tests*/**` (not tested by hand; signals which automated tests to run) |
| Not testable | `.claude/**`, `docs/**`, `specs/**`, build scripts, `*.d.ts` and `Framework/ViewModels/**` (generated), project files with only file-list changes, binaries |

A file can count toward two layers (a check-in Obsidian block). Count it once in the total.

### New code vs changed code

For each layer, split counts into **new** (file added, or a class/block/action that did not exist before) and **changed** (existing code modified). For ranges, also note **deleted** files: removals of user-visible features need a test that the replacement exists or the removal is intended.

`git diff --name-status A...B` gives A/M/D/R per file.

### Domain

Use the path-to-domain table in `.claude/rules/rock-domains.md`. Use the release-note form (`Finance`, `Check-in`) for group headings in the plan.

### Commit signals

For ranges, also list commit counts by message prefix: `+` (release note) and `-` (trivial). Release-note commits carry the user-visible intent and are the best source for what a test should prove; read their messages before their diffs.

## When scope must be confirmed

"Confirmation" means: stop, show the inventory, and ask the user how to scope the plan before designing any tests. It never means abandoning the request.

| Input | Confirmation |
|---|---|
| Version or branch range | **Always.** Before designing any test. |
| Any input with more than 30 commits or more than 75 testable files | Required. |
| Unstaged, staged, or a few commits under the thresholds | Not required. State the scope in one line and continue. |

### How to ask

Show the inventory table first, then ask with AskUserQuestion. Use multiSelect where choices combine. Typical questions (pick the ones that matter for this change; at most four per call):

1. **Refs** (ranges only): "Base `19.6.3` (tag) and target `origin/release-20.0`. Correct?"
2. **Kind of code**: Changed existing code / New code / Both.
3. **Layers**: multi-select from the layers that actually have changes, for example "Obsidian blocks", "WebForms blocks", "Model and service layer", "Migrations and data", "Everything".
4. **Depth**: "Golden path only" (one test per changed feature), "Standard" (golden path, regression and key edges), "Thorough" (adds security checks and edge cases for every changed input).
5. **Commit filter** (ranges only): "Release-note (`+`) commits only" / "All commits".

If the user answers "Everything" for a large range, warn once with the rough size (for example "about 340 changed files in 9 groups; expect several hundred tests") and offer splitting into one plan per group. Respect the answer.

## Groups

Groups are the top-level sections of the plan (the role versions played in the pilot plan). Default grouping:

- **Small plans (under about 40 tests):** one group per domain that has changes, or a single group if everything is in one domain.
- **Large plans:** one group per domain, and split a domain further by layer when it would exceed about 60 tests (`Finance: Blocks`, `Finance: Service layer`).

Groups also become the unit of work when `/verify-run` splits a plan across agents, so keep tests that share setup data in the same group.

## Recording scope in the plan

The plan's Scope section lists: the input as given, resolved refs and HEAD hash, commit count, the confirmed choices, and an **Excluded** list (layers or commits left out, and changes that got no test with the reason). Someone reading only the plan must be able to tell what was not covered.
