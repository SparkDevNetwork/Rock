# Chat platform (Rock side)

The repository root `CLAUDE.md` and `.claude/rules/` still apply here in full. This file adds what
is specific to the chat platform and replaces nothing.

## What this is

Chat runs on a Supabase project that Rock owns the truth for. Rock projects its people, channels,
memberships and badges to that platform, and the platform serves the conversation. Rock initiates
every call: the platform never calls Rock, because a large share of installations are outbound
only.

The chat that already ships in Rock, under `Rock/Communication/Chat`, is a different provider
integration. It is not touched and not referenced from here.

## Layout

| Folder | Holds |
|---|---|
| `Session/` | Gating, enrolment, token minting and the birthdate chat asks for, for a person opening chat; `ChatSessionHelper` answers each chat block action in one call, reading the settings and the Direct Message Access data view itself |
| `Sync/` | `ChatPlatformSyncHelper`, everything the sync job says to the platform, and what Sync Now on a chat block may start and report |
| `LaneA/` | The immediate lane: save hooks record keys, a flush pushes them |
| `Configuration/` | The settings model, its cached parsed form, and secret handling |
| `Contract/` | The vendored wire contract and its hash |

Folders appear when a file needs them. The scheduled sync follows Rock's job and helper pattern, as
`GivingAutomation` does with `GivingAutomationHelper`. The job, `Rock/Jobs/ChatPlatformSync.cs`,
decides whether the run happens, reads the church, writes the body and records what it reports.
`Sync/ChatPlatformSyncHelper.cs` holds the rest: the submission headers, the credential, submit and
poll over one HttpClient per run, what the answer means, the rules for writing a row value,
which the immediate lane will share, and Sync Now. Each block that shows Sync Now asks its own
authority and hands the answer to the helper, which returns the block's action result. The
signing stays in `Session/`, the settings in
`Configuration/` and the contract in `Contract/`; the helper calls them.

The projection itself is one stored procedure, `spChat_SyncProjection`, which marks the groups that
are chat channels, stages the sets the sections read, and returns the sections as result sets. It
is created by a Rock migration like every other procedure, so it changes by a new migration, never
by editing one that has shipped. `Rock.Tests` links the migration's file to read the text that ships.

The blocks live in `Rock.Blocks/Communication/Chat`, the bags in
`Rock.ViewModels/Blocks/Communication/Chat`, and the client in
`Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat`, each following the convention the root
file already states.

Sync Now is the exception on the client. It is one button shared by the Chat Configuration block and
upstream's Group Type Detail block, and each block folder is its own TypeScript project that may not
import from another, so the button and its behaviour live with the framework's internal controls in
`Rock.JavaScript.Obsidian/Framework/Controls/Internal`. A press asks Rock to run the Chat Platform
Sync job now, exactly as the Jobs Administration page does, and follows that run in the job's own
history; it never runs the sync itself.

## The vendored contract

`Contract/chat-wire-contract.json` is generated on the platform side from the catalog of the
database its migrations produce. What is here is a copy of the same bytes, and the two are compared
byte for byte, so it is replaced by regenerating it and never edited by hand. Its line endings are
pinned beside it for the same reason.

Rows go over the wire as positional JSON arrays rather than as named fields, so the column order in
that file is something both sides have to agree about: two columns of the same width swapped on one
side shift every value one place, and a row-width check cannot see it. `ChatWireContract` hashes the
column lists by the recipe the file states, and a submission carries that hash so the platform can
refuse a payload built from a contract it does not recognise.

The hash covers the column lists and nothing else, deliberately. It is compared when a submission
arrives, so anything else it covered would refuse every submission from every church still on the
previous Rock build the moment it changed. A new enum value is meant to reach the platform first and
leave those churches working.

## Tests

| Kind | Where | Runner |
|---|---|---|
| Unit | `Rock.Tests/Communication/Chat/Platform/` | MSTest, on every pull request |
| Projection, merge and hooks | `Rock.Tests.Integration/Communication/Chat/Platform/` | MSTest against SQL Server, on the `run-integration` label |
| Client | `Rock.JavaScript.Obsidian.Blocks/tests/Communication/Chat/` | jest, on every pull request |

Write the failing test first and confirm it fails for the reason you expect. Never delete, skip or
weaken a failing test to make a run pass.

Run `node ci.mjs` in `Dev Tools/chat-ci` before pushing. It runs the same gates as
`.github/workflows/chat-ci.yml`, in the same order.

## Comments and commits

Comments explain in place. No file here refers to a planning document, a decision number, a
guardrail number or a work-item id, in code or in a commit message: a reader of this repository has
none of those to open, so the reason belongs in the comment itself. `Dev Tools/chat-ci` runs a gate
that refuses it.

Commit messages follow the repository's own convention, which the root file states. Commits carry
no agent attribution trailer and no co-author line.

## Traps worth knowing before writing here

- No synchronous HTTP inside a save hook. The immediate lane hands rows to an asynchronous
  transport; an awaited call belongs to a block action alone and carries a time budget.
- A secret never enters a view model bag, a log line or an exception message.
- A block action that changes Rock truth returns a typed result with a stable code, never exception
  text. The birthdate save writes only a value Rock does not hold, and only when the age gate is
  the one asking, so it can never change a recorded age.
- Bulk operations bypass hooks, which is why the scheduled job, not the immediate lane, is the
  guarantee that the platform catches up.
