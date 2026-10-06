# Test Design

Which tests each kind of change needs. Start from what the change is meant to do (the commit message, linked issue, spec in `specs/`, or the code itself), then pick from the test kinds below.

## Test kinds

Each test has one Kind. Use these values in the plan's Kind column.

| Kind | Proves | Needed when |
|---|---|---|
| `Golden` | The feature does what it is meant to, used the normal way. | Every changed or new feature. |
| `Regression` | Existing behavior next to the change still works. | Changed existing code. Pick the behaviors the diff could plausibly break: other branches of the same method, other callers of a changed service method, other modes of the same block. |
| `Edge` | Boundaries and bad input are handled: empty, null, very long, zero, duplicates, inactive or archived records, missing page parameters, a person with no family, a date across midnight or DST. | Changes to validation, parsing, calculations, queries, or anything that takes user input. |
| `Security` | A user without the right permission cannot see or do it, and crafted requests cannot reach other records. Always paired with a control. | Changes to authorization, to anything that reads ids from the request (page parameters, hidden fields, block action arguments), or new block actions. |
| `Data` | Stored data is correct: a migration's schema or data change, a calculated or denormalized value, cleanup of old rows. | Migrations, hotfix migrations, jobs that write data, changes to `PreSave`/`PostSave` logic. |
| `Automated` | Existing automated tests still pass. The runner executes `dotnet test` with a filter. | The change touches code covered by `Rock.Tests*`, or the commit itself adds or changes tests. |

## By layer

### Obsidian blocks

- Golden path through the UI for each changed action (`[BlockAction]` methods in the C# class) and each changed screen in the `.obs` files.
- Check block settings: a changed `[...Field]` attribute needs a test that sets it in Block Settings and sees the effect. New settings need their default checked too.
- Detail blocks: view, edit, cancel, save, and add-new (missing or zero id page parameter).
- List blocks: grid loads, filters, sorting if changed, row actions (delete, reorder), and the link to the detail page.
- `Security` for any new or changed block action that takes an id or key: replay the action (OB-ACT technique) with another record's IdKey as a user who can see only one of them.
- Mobile and TV blocks: test the server side through their block actions (OB-ACT) on an internal test page; the mobile shell itself is a `Needs:` item.

### WebForms blocks

- Golden path for each changed event handler (`btnSave_Click`, grid `RowCommand`, etc.).
- Hidden field or postback argument handling changed: `Security` tests with WF-HF or WF-PB, plus a control.
- `ViewState` or dynamic control changes: test a postback that rebuilds the controls (for example change a dropdown that causes an `AutoPostBack`, then Save).

### UI controls and framework

- Find two or three real blocks that use the control (`locating-ui.md`) and test it in each. Pick blocks that use the changed option or prop.
- Changed public props or events on a widely used control: add `Regression` tests in a block that does not use the new option.
- Styling-only changes: one test with a screenshot and a precise description of what should look different (element, property, before and after), so the check is not a matter of taste.

### REST API

- Call the endpoint from the browser console while logged in (`fetch('/api/...')`) and confirm the response and data.
- `Security`: call it as a user without access and anonymously (from a private window or with `credentials: 'omit'`).
- Changed query options (`$filter`, `$expand`, new parameters): one test per option.

### Model and service layer

There is no screen for a service method, so tests go through the features that call it.

1. Find callers (Grep for the method name across `Rock.Blocks`, `RockWeb/Blocks`, `Rock.Rest`, `Rock/Jobs`, `Rock/Workflow`).
2. Pick the most common caller for the `Golden` test and one or two others for `Regression`.
3. If no UI path reaches the code, use Lava (an HTML Content block on a test page, with the needed Lava commands enabled in its block settings) or a `Data` test that checks the stored result with SQL.
4. Changes to `PreSave`, `PostSave`, `IsValid`, or `CanDelete`: test saving or deleting the entity through any UI that does so, including the failure message for invalid input.

### Jobs

- Open **Admin Tools > System Settings > Jobs Administration**, find the job (add it if it does not exist on a stock database), set its attributes, click **Run Now**, then check the job's Last Status message and the data it should change.
- Changed job attributes: test each new or changed attribute value.
- Jobs that send communications or charge payments: run against data that makes the side effect harmless (a test person, the Test Gateway, an email sink from `ui-recipes.md`).

### Workflow actions

- Build a small workflow type that uses the action (setup steps in the test), launch it (the Workflow Launch block, or the workflow type's Launch button and entry form), and check the result in the workflow's log or attributes.
- Changed action attributes: one step per attribute.

### Field types

- Create an attribute of that field type (on a Defined Type, Group Type, or Person), then test: configuration, edit control, saving, read-only display, and filter (if the field type supports filtering, use a Data View or grid filter).
- Test in both Obsidian and WebForms editors when the field type has both (`Rock/Field/Types/*.cs` methods for WebForms controls and the `.ts` file under `Framework/FieldTypes/`).

### Lava

- Run the filter or command in an HTML Content block on a test page (enable any Lava commands it needs in block settings), with input values that hit each changed branch. Put the template and expected output in the test.
- Check that existing usages still render: search `RockWeb` and migrations for the filter name and pick one stock page that uses it.

### Check-in

- Use **Check-in Manager** and the check-in kiosk with the Weekly Service Check-in configuration from the sample data. Physical printing has `Needs: a label printer`; checking the label content (label preview, or the print error that proves the check passed) does not.

### Finance and communication engines

- Batches and transactions: sample batches are all closed, so tests create their own open `Verif` batches. Give the setup as SQL using the tested "Open batch with transactions" recipe in `../verify-run/references/ui-recipes.md`, with the UI path as an alternative, and include cleanup of the batch History rows that deletes create.
- Payment gateways: use the Test Gateway that ships with Rock. Anything that needs a real gateway or real money gets a `Needs:` line and must not be run against a live account.
- Communications: use the Email preview, or check the `CommunicationRecipient` rows and status with SQL. Delivery can be checked with the email sink and the SMS Test transport in `ui-recipes.md`.

### Migrations and data

- `Data`: the schema or data change is present (`INFORMATION_SCHEMA`, or query the rows). Stock pages and blocks a migration adds are reachable and work.
- Hotfix migrations (`Rock/Plugin/HotFixes`): run on startup; check the `PluginMigration` row for the number exists, then check the change itself.
- Data-fix migrations: give a SQL query that would have found the bad data and should now return nothing.
- Do not plan tests that require running a migration's `Down()`; note in Scope that rollbacks were not tested.

### Automated tests

- One `Automated` test per test project touched, with the filter that runs the relevant tests: `dotnet test Rock.Tests/Rock.Tests.csproj --filter "FullyQualifiedName~<Namespace or class>"`. Integration tests need their own database configured in `Rock.Tests.Integration/app.ConnectionStrings.config`; add `Needs: integration test database` to those.

## Dependencies (`Needs:`)

Every test is written to be run by an agent. In the pilot plan agents completed every test; a few needed a human to approve a step (such as creating a page for a block), never to do the test. So do not plan tests as "for a human".

Add a `Needs:` line only when a test depends on something outside the standard environment that a runner cannot create:

- Physical devices: label printers, kiosk hardware, scanners, the mobile or TV app shell.
- Outside services: payment gateways other than the Test Gateway, SMS providers, AI providers, map keys, third-party APIs.
- Anything that publishes outside the local environment (shared content library, Rock Shop). These tests describe the check but must not be run against the real service.

Data the runner can create (open batches, kiosks configured in Rock, mobile applications, LMS data) is setup, not a `Needs:` item.

## Existing automated tests

Before planning UI tests for a change, look for automated tests that already cover it (Grep `Rock.Tests*` for the class and method names).

- **Keep the UI test unless you are certain** the automated test proves the same behavior end to end. Almost no block code has automated tests today, and a unit test of a service method does not prove the block, bag, front end, and security wiring around it.
- Drop a UI test only for logic with no UI or request wiring of its own (a pure Lava filter, a utility method, a calculation) whose changed branches are each asserted by an existing test. Replace it with an `Automated` test that runs those tests, and list the decision in the Scope section.
- When automated tests exist but you keep the UI test, mention them in the Covers line (`Also covered by: Rock.Tests/...`) so a reviewer can see both.

## Description cell

The table's Description is a one or two sentence version of the test that a human can follow when the test is simple ("As admin open Finance > Batches, open any open batch, delete one transaction; it leaves the grid"). For tests with setup, crafted requests, or several steps, write "See Instructions Below" and put everything in the instruction section.
