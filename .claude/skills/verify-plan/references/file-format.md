# Verification File Format

The plan is one markdown file. `/verify-run` parses it, so keep the structure exact: the heading levels, the table columns, and the Test ID headings.

## Skeleton

````markdown
# Verification: <label>

<One paragraph: what this plan verifies, in plain words. Name the input (commits, range, or working tree) and say that agents run what they can and a human completes the rest.>

## How to use this file

- Tests are grouped under [Test Tables](#test-tables). Each row has a Test ID; its detailed steps are under a heading with the same Test ID in [Test Instructions](#test-instructions).
- **Status**: `PASS`, `FAIL`, `BLOCKED`, or blank (not tested yet). Agents set only `PASS` or `FAIL`. **`BLOCKED` is set only by a human.** If an agent cannot perform a test, it leaves Status blank, writes the reason in Notes, and moves on.
- **Notes**: filled in for `FAIL` (what happened), `BLOCKED` (why a human skipped it), and for a test an agent could not run (Status stays blank). For a skipped test, name the missing data, setting, service, or skill (for example "No SMS phone number configured"), not just "could not run". Notes also record anything learned while running the test that differs from the instructions. Leave Notes empty for a plain `PASS` and for rows nobody has attempted.
- If the code differs from a test's instructions, follow the code and say so in Notes.

## Environment

<Copied from verify-run/references/environment.md "Required state", plus anything this plan adds: extra data, block settings, a configured provider.>

- Built from: `<branch>` at `<HEAD short hash>`. The site under test must be running this build.

## Scope

- **Input:** <as given by the user>
- **Resolved:** <refs, or "working tree at <hash>">; <N> commits; <N> files.
- **Choices:** <confirmed scoping answers, or "small change, full scope">
- **Excluded:** <layers, commits, or changes without tests, each with a reason>
- **Open questions:** <Test IDs whose intended behavior was unclear, or "None">

## Shared setup

<Optional. Setup recipes that several tests or groups reuse, each under a `### Setup: <Name>` heading (for example "Setup: Verif open batches", "Setup: Verif test page"). Tests refer to them by name: "Uses Setup: Verif open batches". Omit the section if no setup is shared across Subjects.>

## Results summary

<Left as the single line "Not run yet." by verify-plan. verify-run replaces it with a count table and highlights.>

## Test users

<Copied from verify-run/references/environment.md.>

## Techniques

<Only the techniques this plan's tests use, copied from verify-run/references/techniques.md. Omit the section if no test needs them.>

# Test Tables

## <Group name>

| Test ID | Subject | Kind | Status | Description | Notes |
|---|---|---|---|---|---|
| TransactionList-01 | Finance/TransactionList (Obsidian) | Golden |  | As admin open an open batch, delete one transaction; it leaves the grid. |  |

# Test Instructions

## <Group name>

### TransactionList-01 Finance/TransactionList (Obsidian)

Covers: `aed54c978f` (`Rock.Blocks/Finance/TransactionList.cs`)

Needs: <optional; only when the test depends on something a local environment may not have>

<One sentence on what this test proves, when that is not obvious from the steps.>

Setup (as admin):

1. ...

Steps:

1. ...

**Expected:** <observable result in the UI> and <SQL that confirms it>.

Cleanup: <what to remove or restore>.
````

## Rules

### Test IDs

- Format: `<Subject>-<NN>`, where Subject is the PascalCase name of the block, service, job, action, field type, filter, or migration (`TransactionList-01`, `GroupService-03`, `AddWorkflowBuilder-01`). Number from `01` within each Subject.
- If two subjects share a name (a WebForms and an Obsidian `TransactionList`), prefix the domain to both: `Finance.TransactionListWF-01` and `Finance.TransactionList-01`.
- IDs never change once written. When updating a plan, add new numbers; do not renumber.

### Subject column

`<Domain>/<Name> (<Kind of thing>)`, for example `Finance/TransactionList (Obsidian)`, `Group/GroupDetail (WebForms)`, `Core/PersonService (Service)`, `Rock.Jobs.DataAutomation (Job)`, `Migration 202609222138362_AddWorkflowBuilder (Data)`.

### Groups

The `##` group headings under Test Tables and Test Instructions must match exactly and appear in the same order. Rows within a group are ordered by Subject, then by number, with golden paths first for each Subject.

### Instructions

- Every row has an instruction section, even when the Description is enough to run it. Simple tests can have just the Covers line and one step.
- **Covers** lists the commit short hashes (or "working tree") and the files.
- Shared setup: when several tests reuse setup, put it in the first test and have later tests say "Uses the setup from `<Test ID>`". Keep those tests in the same group.
- Steps that a later test depends on (for example "copy the `BlockActions/.../Delete` request for TransactionList-04") say so explicitly.
- **Needs** is an optional line after Covers that names an outside dependency the test cannot work without ("Needs: a label printer", "Needs: an AI provider linked on the system"). It is a hint, not a skip instruction: the runner checks whether the dependency exists and tries the test anyway. Omit it for tests that only need the standard environment, setup the test describes, or data the runner can create.
- Data a test creates is named with a `Verif ` prefix ("Verif Batch A", "Verif Child Page") so leftovers are easy to find and clean up. The `/verify-run` preflight looks for them.
- SQL is read-only in Expected sections. Setup and cleanup SQL that writes data is allowed but must be scoped tightly (by the Ids or names the setup created) and should be followed by "Clear the cache" when it changes cached entities (pages, blocks, attributes, defined values, security).

### Writing

- Plain, specific language. Name the menu path, the button text, and the value to type.
- No em-dashes or double hyphens in prose.
- Use the standard users by name (admin, Ted Decker, Pete Foster). Never put other credentials in the file.
