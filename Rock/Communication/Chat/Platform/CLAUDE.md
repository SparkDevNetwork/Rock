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
| `Session/` | Gating, enrolment and token minting for a person opening chat |
| `Sync/` and `Sql/` | The projection queries, and the client that carries a submission to the platform |
| `LaneA/` | The immediate lane: save hooks record keys, a flush pushes them |
| `Doors/` | Block action handlers that change Rock truth, such as creating a direct message or joining a channel |
| `Configuration/` | The settings model, its cached parsed form, and secret handling |
| `Contract/` | The vendored wire contract and its hash |

Folders appear when a file needs them. The job classes live in `Rock/Jobs` with every other job, and
the scheduled sync keeps its whole run there rather than spread across this folder: whether the run
happens, which queries it loads, reading the church, shaping the payload, building the headers,
submitting, polling and what it reports. What stays here is the transport, which owns an HttpClient
and its lifetime and is the one piece that is not about this church's data.

The blocks live in `Rock.Blocks/Communication/Chat`, the bags in
`Rock.ViewModels/Blocks/Communication/Chat`, and the client in
`Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat`, each following the convention the root
file already states.

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
  transport; the awaited path exists for the doors alone and carries a time budget.
- A secret never enters a view model bag, a log line or an exception message.
- A door returns a typed result with a stable code, never exception text.
- Bulk operations bypass hooks, which is why the scheduled job, not the immediate lane, is the
  guarantee that the platform catches up.
