---
author: Joshua Henninger
date_created: 2026-05-08
summary: >-
  Refresh the Form Builder family of blocks (form list, form detail, template
  detail, form submissions, form analytics) to the v20 Obsidian polish targets,
  rename the Communications tab to Automations, restructure Person Entry into
  collapsible sections, expand Confirmation Email recipient and Notification
  Email send-to options, and add first-class support for opening a Connection
  Request on form submission with per-form-field attribute mapping.
contributors: []
---

# Form Builder Updates

## Summary

Update the Form Builder family of blocks to the v20 visual refresh and extend the per-form post-submission configuration with a new "Connection Requests" section. The work covers five blocks: Form List, Form Detail (`formBuilderDetail.obs`), Form Template Detail, Form Submissions, and Form Analytics. Form List is restructured into a two-panel Category Tree layout with last-selected-category persistence. Form Detail's left-rail tab strip is replaced by top-anchored sibling tabs (Settings, Form Builder, Automations); the Form Builder canvas's edit sidebar moves to the right of the canvas; Header/Footer editing moves out of modals and into the sidebar; the Person Entry Form is restructured into collapsible accordions with select-to-radio conversions. The Automations tab gains a third toggle-on section, Connection Requests, that uses the form's Person Entry primary person as the requestor and introduces an Attribute Matching panel that maps each form field to either the Connection Request's Comment field or a compatible attribute on the chosen Connection Opportunity. Confirmation Email gains a Recipient picker (Person, Spouse, Both), and Notification Email gains a Campus Topic Address send-to option. Form Submissions and Form Analytics are converted from WebForms to Obsidian as basic conversions and re-rooted under the Form List card affordances; their cross-block nav tabs and the related block settings are removed. Persistence for the new section layers onto `WorkflowActionForm` and `WorkflowFormBuilderTemplate` as a new JSON settings column, mirroring the existing email settings columns. No schema rework is required for `ConnectionRequest`.

This spec supersedes the WIP `260507-form-builder-polish-and-connections-support.md` draft, which has been replaced by an updated Figma pass with materially different decisions on tab placement, sidebar position, Header/Footer location, and Connection Request shape (Attribute Matching replaces explicit Person/Connector/Campus attribute pickers and the Lava comment template).

## Mockups

Source of truth for the visual pass: [Figma — Obsidian Block Refreshes, Fall 2025 - Spring 2026](https://www.figma.com/design/N60VRdhtRtjO9EA9nba9fB/Obsidian-Block-Refreshes---Fall-2025---Spring-2026?node-id=5024-63662&m=dev). The PNGs below are point-in-time exports of frames from that file (preserved here so the spec is readable without Figma access). When the Figma and an export disagree, the Figma wins.

Form List two-panel layout with Category Tree, refreshed display cards, and panel-footer Edit/Delete:

![Form List](artifacts/260508-form-builder-updates/1-form-builder-form-list.png)

Create New Form view (Name, Slug, Description, Category, Template) with "Start Building" primary action:

![Create New Form](artifacts/260508-form-builder-updates/2-form-builder-form-list-add-form.png)

Add Category view (Name, Description, Highlight Color, Icon):

![Add Category](artifacts/260508-form-builder-updates/3-form-builder-form-list-add-category.png)

Form Builder Detail Settings tab refresh (top fields, General Settings section, Completion Settings with radio Completion Action):

![Settings tab](artifacts/260508-form-builder-updates/4-form-builder-detail-settings-tab.png)

Settings tab Completion Action "Redirect to New Page" variant (radio + Redirect URL):

![Completion Action redirect variant](artifacts/260508-form-builder-updates/5-form-builder-detail-settings-tab-completion-action-variant.png)

Automations tab top half (Confirmation Email with Recipient Person/Spouse/Both, Notification Email with Send To Individual/Email Address(es)/Campus Topic Address):

![Automations top half](artifacts/260508-form-builder-updates/6-form-builder-detail-automations-tab-top-half.png)

Automations tab bottom half (Connection Requests section with Configuration and Attribute Matching panels, plus Person Entry alert):

![Automations bottom half](artifacts/260508-form-builder-updates/7-form-builder-detail-automations-tab-bottom-half.png)

Email-to-Send "Provide Custom Email" variant (Subject, Reply To, Email Body, Append Organization Header and Footer):

![Custom email variant](artifacts/260508-form-builder-updates/8-form-builder-detail-automations-tab-confirmation-email-email-to-send-variant.png)

Notification Email Send To variants (Email Address(es) recipients input, Campus Topic Address topic picker):

![Notification send-to variants](artifacts/260508-form-builder-updates/9-form-builder-detail-automations-tab-notification-email-send-to-variants.png)

`contentSection` tweak: when a switch is added to `headerActions`, toggling it must drive the section's collapsed state from the parent (today the prop only initializes once on mount):

![Section switch tweak](artifacts/260508-form-builder-updates/10-form-builder-detail-automations-tab-content-section-tweak.png)

Form Builder canvas refresh (top tabs, edit sidebar moved to right of canvas, canvas sections include Header, Person Entry Form, Add Form Fields empty state, Form Sections, Footer):

![Form Builder canvas top half](artifacts/260508-form-builder-updates/11-form-builder-detail-form-builder-tab-top-half.png)

Form Builder canvas bottom half (Footer placeholder, Additional Fields in sidebar):

![Form Builder canvas bottom half](artifacts/260508-form-builder-updates/12-form-builder-detail-form-builder-tab-bottom-half.png)

Section selecting state (gear/hamburger/X side-control bar, hover and active states):

![Section selecting state](artifacts/260508-form-builder-updates/13-form-builder-detail-form-builder-tab-section-selecting-state.png)

Field selecting state (40px right margin reserved for hover-control behavior, hover and active bar states):

![Field selecting state](artifacts/260508-form-builder-updates/14-form-builder-detail-form-builder-tab-field-selecting-state.png)

Drag-and-drop states (drop into section, drop between fields):

![Drag-and-drop states](artifacts/260508-form-builder-updates/15-form-builder-detail-form-builder-tab-drag-n-drop-state.png)

Sidebar / edit-state changes (sidebar on right, gray background on Settings, square field blocks with new icons, Field Type / Conditionals / Format / Advanced accordions, restyled Conditionals Add button):

![Sidebar edit state](artifacts/260508-form-builder-updates/16-form-builder-detail-form-builder-tab-sidebar-changes.png)

Header / Footer settings move from modal to sidebar; Content textarea defaults to 6 rows; help-text panel notes [HEADER] and [FOOTER] support HTML:

![Header Footer changes](artifacts/260508-form-builder-updates/17-form-builder-detail-form-builder-tab-header-footer-changes.png)

Person Entry Form collapsible-sections refresh (1 of 3) with General expanded by default, Campus collapsible, select-to-radio conversions:

![Person Entry 1 of 3](artifacts/260508-form-builder-updates/18-form-builder-detail-form-builder-tab-person-entry-form-1-of-3.png)

Person Entry Form (2 of 3) Personal Information accordion (Gender, Email, Mobile Phone, SMS Opt-In, Address as Hidden/Optional/Required radios):

![Person Entry 2 of 3](artifacts/260508-form-builder-updates/19-form-builder-detail-form-builder-tab-person-entry-form-2-of-3.png)

Person Entry Form (3 of 3) Family and Demographics accordions:

![Person Entry 3 of 3](artifacts/260508-form-builder-updates/20-form-builder-detail-form-builder-tab-person-entry-form-3-of-3.png)

Form Submissions converted to Obsidian (Obsidian Grid, filters lifted into a modal, bulk/grid actions dropdown):

![Form Submissions](artifacts/260508-form-builder-updates/21-form-submission-list.png)

Form Analytics converted to Obsidian (Total Views / Completions / Conversion Rate cards, line chart):

![Form Analytics](artifacts/260508-form-builder-updates/22-form-analytics.png)

Form Analytics empty state (v20 pattern with workflow-entry guidance copy folded into the empty state's sub-copy):

![Form Analytics empty state](artifacts/260508-form-builder-updates/23-form-analytics-empty-state.png)

## Motivation

The Form Builder family is a high-traffic admin surface and was last styled before Rock's design-token refresh, so it sits on the v20 polish list with a refreshed Figma pass already published. The Figma scope is broader than the original Asana ticket implied: it covers Form List, Form Detail, Form Template Detail, **and** the still-WebForms Form Submissions and Form Analytics blocks, both of which are folded in here as basic Obsidian conversions so the family ships consistent in v20 instead of a half-Obsidian, half-WebForms hybrid.

Beyond the polish, two behavior gaps drive the rest of the work:

1. **Connections support.** Churches that want a Form Builder submission to open a Connection Request must today wire up a separate Workflow with a Create Connection Request action, identical to the friction the inbound-SMS shortcut spec ([260506-sms-action-create-connection-request.md](260506-sms-action-create-connection-request.md)) is removing on its side. Form Builder already owns first-class shortcuts for "send confirmation email" and "send notification email" so admins do not have to drop into a workflow for those common cases; "open a connection request" is the same shape and earns the same treatment. The new section reuses the form's Person Entry primary person as the requestor (avoiding a parallel person-attribute picker) and adds an Attribute Matching panel so admins can route any form field into either the Connection Request Comment or a specific attribute on the selected Opportunity.
2. **Confirmation/notification flexibility.** The current Confirmation Email always sends to the form's primary person and the current Notification Email's "send to" choice does not include Campus Topic Addresses. Both are common asks; both are small additions on top of the existing settings JSON and editor partials, and the polish pass is already touching every partial in the renamed Automations tab.

Doing the polish, the structural moves (top tabs, right-side sidebar, Header/Footer-in-sidebar, Person Entry collapsibles), and the new automation in one coordinated change keeps the v20 Form Builder churn to a single PR family rather than three.

## Requirements

### UI polish: Form List

`formList.obs` is the most affected block in the polish pass. The refreshed Figma calls for a structural reorganization of the page, not just a visual refresh.

The Form List polish MUST:

- Split the page into two side-by-side panels: a left "Form Categories" panel and a right "{Selected Category} Forms" panel. The current single-panel layout is replaced.
- Render the left panel using a Category Tree control (Rock's existing tree component) sourced from the workflow form category hierarchy. Nodes render with the category icon and a chevron toggle for expandable nodes. Selecting a leaf populates the right panel with that category's forms.
- Persist the last selected Category as a per-user preference (Rock's `PersonPreference` API) so reopening the block lands the user back on their previously selected category. New users with no preference land on the first available category, or an empty state when no categories exist.
- Move the "Add Form" button from inline within the form list to the right panel's header (`+` affordance), alongside any panel-level actions.
- Move "Edit", "Delete", and security action buttons from per-row inline icons to the right panel's footer. Edit acts on the currently selected form. Delete remains disabled when the selected form cannot be deleted (existing behavior preserved verbatim).
- Move the per-row "Edit" (Form Builder) button to the end of the per-form display-card button cluster. Refreshed cluster, in order: list view (Submissions), chart (Analytics), share (Link to Form), kebab, edit (Form Builder). The kebab menu retains Clone and Delete; the polish pass adds no entries to it.
- Sort Forms by a "Sort By" dropdown at the top of the right panel. Default: "Date Created (newest first)". Full option set preserved verbatim from today's block.
- Continue to host the entry-point routing for the rest of the family. Form Submissions and Form Analytics are accessible only through the per-row buttons on a Form List card (no top-level page navigation), and the per-card "view form" affordance still links to a page hosting a Workflow Entry block (the runtime form), unchanged from today.

The Form List polish MUST update the Create New Form view:

- Replace any inline / pre-flight form-creation UI with a single "Create New Form" view (rendered in the right panel, not a modal) launched from the Add Form button.
- Fields: `Name` (required), `Slug` (required, validates uniqueness via `WorkflowTypeService.GetUniqueSlug`; max length 400; lowercase-alphanumeric-and-hyphens normalization handled by `MakeSlugValid`), `Description` (multiline), `Category` (picker, defaulted to the currently selected Category Tree node), `Template` (picker sourced from `WorkflowFormBuilderTemplate`).
- Copy: "Complete the fields below to setup your new form. Upon completion you'll be taken to the form builder."
- Primary action label: "Start Building". On success, navigate the user directly into the Form Builder for the new form.

The Form List polish MUST add an Add Category view:

- Launched from the "+" affordance on the Form Categories panel header.
- Fields: `Name` (required), `Description`, `Highlight Color` (color picker, persisted to `Category.HighlightColor`), `Icon` (icon picker, persisted to `Category.IconCssClass`). Both target columns already exist on `Category`; no schema changes are needed.
- Persists as a new `Category` record under the workflow form category root.
- Primary action label: "Save".

### UI polish: Form Detail and Form Template Detail

The polish pass MUST:

- Bring `formBuilderDetail.obs` and its partials in line with the refreshed Figma. PNG exports of the per-frame designs are staged under `artifacts/260508-form-builder-updates/` for cross-reference.
- Replace any remaining inline styles or hard-coded colors with Rock CSS variables and utility classes (per the `css-cleanup` skill).
- Audit `formTemplateDetail.obs` for the same token / utility migration even where the Figma refresh does not change layout.
- Preserve the existing drag-and-drop section/field editing behavior. The polish is presentational and structural (sidebar moves, tab moves), not a reauthor of the editor.

The polish pass MUST rename and re-arrange the tabs:

- Rename the **Communications** tab to **Automations**. The on-disk filename (`communicationsTab.partial.obs`) can stay or be renamed for clarity, author's choice; user-visible copy MUST read "Automations" everywhere.
- Replace the today's left-rail tab-bar with a top-anchored tab strip: **Settings**, **Form Builder**, **Automations**. The Settings tab content is a peer of the canvas, not a header above it.
- Place the Save button and the "preview form" eye icon in the same top-right cluster, visible across all three tabs.

The polish pass MUST update the Settings tab:

- Top section (in a card): `Form Name` (required) and `Slug` (required) on one row, `Description` underneath, `Category` (required) underneath. The Slug field uses the same uniqueness validation as the Create New Form view.
- General Settings section (`SectionContainer` with description prop): `Template` picker, `Form Entry Starts` (date + time), `Form Entry Ends` (date + time), `Is Login Required` (checkbox).
- Completion Settings section (`SectionContainer` with description prop): `Completion Action` rendered as a **radio field** (`Display Message` / `Redirect to New Page`), replacing today's button toggle. When `Display Message` is selected, render an inline `Completion Message` editor. When `Redirect to New Page` is selected, render an inline `Redirect URL` field.
- Use the section header's `description` prop for the secondary subtext on each `SectionContainer`, replacing today's secondary-line treatment.
- Refresh accordion section styling.

The polish pass MUST update the Form Builder canvas:

- Move the field/section edit sidebar to the **right** of the canvas (today it sits on the left). The sidebar contains `Settings` (with a gray background, top of the sidebar), `Common Fields`, and `Additional Fields` groupings.
- Restyle Field blocks in the sidebar as **square** cards with new icons.
- Update the Conditionals editor's "Add" button styling.
- Add a back-button-style button (no background) and a box-shadow at the bottom of the active edit-state panel.
- Render Header and Footer as canvas placeholders that, when selected, open their settings panel **in the sidebar** (not in a modal). Today these open in modals; the modal pathway is retired.
- Header / Footer settings panel MUST default the Content textarea to 6 rows in height, and MUST include help text noting that `[HEADER]` and `[FOOTER]` content supports HTML.
- Custom (empty) sections render an "Add Form Fields" placeholder.
- Render new hover/select state styling for borders, background, and the per-block config bars: each section/field has a right-edge bar with hamburger (drag), gear (settings), X (delete). Field cards reserve a 40px right margin for the hover-control bar.
- Default field/section card styling: white containers on a gray section backdrop.
- Block settings: keep the existing `Default Preview Page` setting (or add it if not yet present). NOTE: this preview page must include a Workflow Entry block with "Enable for Form Sharing" enabled; default to the first eligible page if the setting is left blank.

The polish pass MUST update the Person Entry Form:

- Restructure the editor into collapsible accordion sections: **General**, **Campus**, **Personal Information**, **Address**, **Family**, **Demographics**.
- General is expanded by default; the rest are collapsed by default.
- Convert select fields to radio fields where appropriate. Specifically:
  - General: `Record Status` becomes a radio (Active / Inactive / Pending). `Record Source` and `Connection Status` remain dropdowns.
  - Campus: `Type` becomes a radio (Online / Physical). `Status` becomes a radio (Closed / Open / Pending). Existing `Show Campus` and `Include Inactive` checkboxes preserved.
  - Personal Information: `Gender`, `Email`, `Mobile Phone`, and `Address` each become a Hidden / Optional / Required radio. `SMS Opt-In` becomes a Hide / Show radio.
  - Address: `Address` becomes a Hidden / Optional / Required radio. `Type` remains a dropdown.
  - Family: `Marital Status` and `Spouse Entry` each become a Hidden / Optional / Required radio. `Spouse Label` is a text field. `Type` remains a dropdown.
  - Demographics: `Race Entry` and `Ethnicity Entry` each become a Hidden / Optional / Required radio. (The Figma misspells "Ethnicity" as "Enthnicity"; the spec uses the correct spelling and the implementation MUST use the correct spelling.)
- Render the Person Entry Form on the canvas as a **larger** card with a skeleton graphic, distinct from the other form-section cards.

The polish pass MUST update the Automations tab:

- Render three sibling `SectionContainer` sections, each with an `Enable` switch in `headerActions` and the section body collapsing when disabled: **Confirmation Email**, **Notification Email**, **Connection Requests**. All three default to disabled.
- Update each `SectionContainer` to use a content-section header style (the new icon-leading header treatment that has already landed on `develop`).
- The `contentSection` shared component MUST be tweaked so a parent-driven `isCollapsed` prop change causes the section to expand/collapse reactively. Today the prop initializes the internal state once on mount and subsequent prop changes are ignored. Add a `watch(props.isCollapsed)` that updates the internal state, so the Enable switch in the header can drive the collapsed state from the parent without breaking existing callers.

#### Confirmation Email section

The Confirmation Email section MUST add a `Recipient` radio field with options: `Person`, `Spouse`, `Both`. Default: `Person` (preserves today's behavior).

- `Spouse` sends only to the form's primary-person spouse. If the primary person has no spouse on the family record, the email is skipped (warn-and-continue, do not error the workflow).
- `Both` sends to the primary person AND the primary person's spouse (one delivery each). If the spouse is missing, only the primary person is sent to.
- Persistence layers a new `RecipientType` enum (`Person` = 0, `Spouse` = 1, `Both` = 2) onto `FormConfirmationEmailViewModel`. Existing rows default to `Person`.

The Confirmation Email and Notification Email sections MUST update the `Email to Send` block:

- Radio: `Use Email Template` / `Provide Custom Email` (today's `EmailTemplateAndCustom` enum stays).
- When `Provide Custom Email`: render `Subject` (required), `Reply To` (optional), `Email Body` (required), and an `Append Organization Header and Footer` checkbox. Move the existing "append organization header / footer" toggle into this group.

#### Notification Email section

The Notification Email section MUST update the `Send To` field:

- Radio options: `Individual`, `Email Address(es)` (renamed from today's `Email Address` to make plurality explicit), `Campus Topic Address` (NEW).
- When `Individual` is selected: today's individual person picker.
- When `Email Address(es)` is selected: a `Recipient(s)` input rendered as a `PillList` (matching the Communication Wizard refactor that landed earlier on this branch, commit `b933a3b0c5`). Each pill is a single email address; PillList handles separator parsing, validation, and chip rendering. Field copy MUST read "Recipient(s)" (plural).
- When `Campus Topic Address` is selected: a `Topic` dropdown sourced from the `Campus Topic` defined-type. The runtime resolves the topic to its email address through the form's primary-person campus (or the form's configured campus if no primary person is set), failing back to skip-and-warn if no address resolves.

Persistence: `FormNotificationEmailViewModel` gains a third destination variant (`CampusTopicValueGuid : Guid?`) and the existing `Destination` enum gains a `CampusTopicAddress` member. The existing per-form override columns absorb the change with no migration; new variants persist as JSON.

#### Connection Requests section (NEW)

A new "Connection Requests" section MUST be added to the renamed **Automations** tab, sitting below Notification Email. It opens a Connection Request when the form is submitted, using the form's Person Entry primary person as the requestor.

The Connection Requests section MUST:

- Toggle on and off via the same `SectionContainer` collapse pattern used by Confirmation Email and Notification Email, with the Enable switch in `headerActions`.
- Render an inline alert at the top of the section body: **"Person Entry must be enabled on the Form Builder tab. The primary person will be used as the connection requestor."** The alert is informational at all times.
- Disable the Enable switch (and surface the alert in error tone) if Person Entry is not enabled on the Form Builder tab. Re-enabling Person Entry on the Form Builder tab re-enables the switch reactively.
- When enabled, render two stacked sub-panels: **Configuration** and **Attribute Matching**, each with a description column on the left and a fields column on the right.

Configuration sub-panel:

- Required: `Connection Type` (dropdown of `ConnectionType`), `Connection Opportunity` (dropdown of `ConnectionOpportunity`, filtered by Type).
- Optional: `Status` (dropdown of `ConnectionStatus`, filtered by Type; defaults at runtime to the Type's default status), `Connection Source` (dropdown of `ConnectionRequestSourceValue` defined-type values; null at runtime means no source).

Attribute Matching sub-panel:

- Description column copy: "Maps form responses to attribute fields on the connection request."
- Fields column lists every form attribute on the current form (label + dropdown), in the same order they appear on the form.
- Each dropdown's options are: `Add to Connection Request Comment` (default; persisted as `null` for the target attribute, with the form-field value appended to the Comment), plus every attribute on the selected Connection Opportunity whose field type matches the form attribute's field type (or whose field type is `Text` so any value can be coerced via `ToString`).
- Mapping target dropdown only repopulates when the Opportunity changes. Changing the Opportunity drops any mapping whose target attribute is not present on the new Opportunity (with a one-time toast warning).
- The pattern mirrors the CSV Import block's column-to-attribute mapping (label is form field, dropdown select is connection field to map to).

The Connection Requests section MUST persist to a new `ConnectionRequestSettingsJson` column on `WorkflowActionForm` and a matching column on `WorkflowFormBuilderTemplate` (so templates can pre-seed the section the same way they pre-seed email settings today). Storage shape is detailed in the **Design** section below.

The Connection Requests section's runtime behavior MUST:

- Run on form submission, after the form completes and the workflow's existing email actions fire. Failure to open the Connection Request MUST NOT prevent the workflow from completing or the confirmation/notification emails from sending; an exception is logged via `ExceptionLogService.LogException()`.
- Resolve the requestor from the form's Person Entry primary person. If Person Entry is disabled (defensive guard, since the editor disables the section in that case), log a warning and skip the Connection Request creation.
- Use the configured `Connection Type`, `Connection Opportunity`, optional `Status` (falling back to `opportunity.ConnectionType.ConnectionStatuses.First( s => s.IsDefault )`), and optional `Connection Source` (falling back to null).
- Set `ConnectorPersonAliasId` from `opportunity.GetDefaultConnectorPersonAliasId( campusId )`, where `campusId` is the primary person's `PrimaryCampusId` or null.
- For each form-field attribute: if the mapping target is null, append `"{Label}: {Value}"` (one per line) to `ConnectionRequest.Comments`; otherwise, set the target attribute on the new `ConnectionRequest` to the form-field value (using `SetAttributeValue` so the attribute value is persisted with the request).
- Set `ConnectionState = ConnectionState.Active` and `ConnectionTypeId = opportunity.ConnectionTypeId`.

### UI polish: Form Submission List

`Rock/Blocks/...` does not yet have a Form Submission List block; the live block is the WebForms `RockWeb/Blocks/WorkFlow/FormBuilder/FormSubmissionList.ascx` (and its `.ascx.cs`). This polish pass converts it to Obsidian as a basic conversion (no new functionality).

The Form Submission List polish MUST:

- Ship a new entity-based Obsidian block at `Rock.Blocks/WorkFlow/FormBuilder/FormSubmissionList.cs` with a Vue SFC at `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/formSubmissionList.obs`. Use the standard Obsidian `Grid` for the submissions grid. Class name matches the existing `FormList` / `WorkflowList` family rather than the working-title `FormSubmissions`.
- Move the existing Filters out of an inline panel and into a modal launched from a "Filters" button on the grid toolbar. Field set unchanged: Person picker, Campus picker.
- Be reachable only through the per-card "Submissions" affordance on a Form List card. No top-level page navigation, no nav-tabs across to Form Builder / Analytics / Person Profile (those nav-tabs are removed).
- Preserve the bulk/grid-actions menu (Launch Workflow, Export to Excel, Merge Template).
- Preserve the per-row "Add" button's link target: a page hosting a Workflow Entry block for the underlying workflow type (no change from today).
- Keep these existing block settings: `Detail Page` (page that displays a workflow's details, default `WORKFLOW_DETAIL`) and `Entry Page` (page used to launch a new workflow of the selected type, default `WORKFLOW_ENTRY`). REMOVE these now-obsolete settings tied to the removed nav-tabs: `Form Builder Page`, `Analytics Page`, `Person Profile Page` (the WebForms block has a Person Profile Page setting tied to the removed nav-tabs; per the Figma notes this code is dead and is removed in this conversion).
- Adopt the WebForms block's `BlockTypeGuid` (`A23592BB-25F7-4A81-90CD-46700724110A`) on the new Obsidian counterpart and delete the WebForms `.ascx` / `.ascx.cs` files. No migration is required: the new Obsidian class registers itself against the existing BlockType GUID at startup, so existing pages keep working without DB changes.

### UI polish: Form Analytics

`RockWeb/Blocks/WorkFlow/FormBuilder/FormAnalytics.ascx` is similarly converted to a basic Obsidian block.

The Form Analytics polish MUST:

- Ship a new entity-based Obsidian block at `Rock.Blocks/WorkFlow/FormBuilder/FormAnalytics.cs` with a Vue SFC at `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/formAnalytics.obs`. The chart presentation (Total Views / Completions / Conversion Rate over a date axis) is preserved unchanged.
- Be reachable only through the per-card "Analytics" affordance on a Form List card. No top-level page navigation, no nav-tabs across to Form Builder / Submissions (those nav-tabs are removed).
- Replace the empty state with the v20 empty-state pattern (icon + headline + sub-copy). Headline: "Nothing To Show". Sub-copy: "Try adjusting your filters, or verify that metric collection is enabled on the Workflow Entry block hosting this form." This subsumes the existing top-of-block guidance note.
- REMOVE all WebForms block settings other than `Name`: `Submissions Page`, `Form Builder Page`, and any other LinkedPage settings. The new Obsidian block ships with no `LinkedPage` settings.
- Adopt the WebForms block's `BlockTypeGuid` (`778EFA7B-56BC-4ABB-B86D-FFD87B97691F`) on the new Obsidian counterpart and delete the WebForms `.ascx` / `.ascx.cs` files. Same chop pattern as Form Submission List, no migration required.

## Design

### Where the new settings live

```
WorkflowFormBuilderTemplate
├── PersonEntrySettingsJson         (existing)
├── ConfirmationEmailSettingsJson   (existing; gains RecipientType)
├── NotificationEmailSettingsJson   (existing; gains CampusTopicAddress destination)
├── ConnectionRequestSettingsJson   (NEW)
└── CompletionSettingsJson          (existing)

WorkflowActionForm
├── PersonEntrySettingsJson         (existing per-form override)
├── ConfirmationEmailSettingsJson   (existing per-form override; gains RecipientType)
├── NotificationEmailSettingsJson   (existing per-form override)
├── ConnectionRequestSettingsJson   (NEW per-form override)
└── CompletionSettingsJson          (existing)
```

The new ConnectionRequest settings persist into a new `nvarchar(max) NULL` column on each of `WorkflowActionForm` and `WorkflowFormBuilderTemplate`. The migration token is currently held on the `release-19.0` branch, so this branch does not author an EF migration; the columns will be added when the token returns to `develop` (a downstream pass picks this up). The Confirmation Email RecipientType and the Notification Email CampusTopicAddress variant ride inside the existing JSON columns, so no new columns are needed for those two.

### ViewModel shapes

```csharp
public class FormConnectionRequestsViewModel
{
    /// <summary>Whether the Connection Requests section is enabled for this form.</summary>
    public bool Enabled { get; set; }

    /// <summary>The selected Connection Type.</summary>
    public Guid? ConnectionTypeGuid { get; set; }

    /// <summary>The selected Connection Opportunity. Required at runtime when Enabled is true.</summary>
    public Guid? ConnectionOpportunityGuid { get; set; }

    /// <summary>Optional explicit Connection Status. Null falls back to the type's default status.</summary>
    public Guid? ConnectionStatusGuid { get; set; }

    /// <summary>Optional Connection Source defined-value.</summary>
    public Guid? ConnectionSourceValueGuid { get; set; }

    /// <summary>Per-form-field mappings. Order matches the form's attribute order.</summary>
    public List<FormFieldAttributeMappingViewModel> AttributeMappings { get; set; } = new();
}

public class FormFieldAttributeMappingViewModel
{
    /// <summary>The form attribute (workflow form field) being mapped.</summary>
    public Guid FormAttributeGuid { get; set; }

    /// <summary>The target attribute on the Connection Opportunity. Null means
    /// "Add to Connection Request Comment" (the default).</summary>
    public Guid? TargetAttributeGuid { get; set; }
}
```

The shape sits next to `FormConfirmationEmailViewModel` and `FormNotificationEmailViewModel` in `Rock.ViewModels/Blocks/WorkFlow/FormBuilder/`.

```csharp
// Add to FormConfirmationEmailViewModel
public FormConfirmationEmailRecipientType Recipient { get; set; } = FormConfirmationEmailRecipientType.Person;

public enum FormConfirmationEmailRecipientType
{
    Person = 0,
    Spouse = 1,
    Both = 2
}
```

```csharp
// Update FormNotificationEmailViewModel destination enum.
// Existing C# member names are preserved to avoid breaking plugins; only the
// user-visible label on the editor changes from "Email Address" to "Email Address(es)".
public enum FormNotificationEmailDestination
{
    Individual = 0,
    EmailAddress = 1,          // unchanged; UI label updated to "Email Address(es)"
    CampusTopicAddress = 2     // NEW
}

public Guid? CampusTopicValueGuid { get; set; }   // NEW; populated only when Destination == CampusTopicAddress
```

The persisted enum integers and C# member names stay stable; new deployments writing `CampusTopicAddress` use 2.

### Submission flow

```mermaid
sequenceDiagram
    participant User as Form submitter
    participant Block as Form (RockBlockType)
    participant WF as Workflow engine
    participant Email as Email actions (confirmation / notification)
    participant CR as Connection Request creator
    participant DB as Database

    User->>Block: Submit form
    Block->>WF: ProcessActivity / Complete
    WF->>Email: Send confirmation (Person/Spouse/Both) + notification (Individual/Addresses/Topic)
    WF->>CR: Section enabled?
    CR->>CR: Resolve requestor from Person Entry primary person
    alt Primary person not resolved
        CR->>WF: Log warning, skip
    else Primary person resolved
        CR->>CR: Resolve opportunity / status / source / campus / connector
        CR->>CR: Walk AttributeMappings: append-to-Comments OR SetAttributeValue
        CR->>DB: SaveChanges (new ConnectionRequest)
    end
    WF->>Block: Workflow complete
    Block->>User: Confirmation page or redirect
```

The Connection Request creation runs in its own try/catch so a misconfigured section (missing opportunity, deleted target attribute, etc.) does not break the workflow. Errors land in the Exception Log via `ExceptionLogService.LogException()`.

### UI placement

The renamed Automations tab partial (currently named `FormBuilderDetail/communicationsTab.partial.obs` on disk) renders Confirmation Email then Notification Email today. The Connection Requests section is appended as a third `SectionContainer`, keeping the same visual rhythm. A new partial under `Shared/connectionRequests.partial.obs` houses the editor so the FormTemplate Detail block can include it for template-level defaults the same way it includes `confirmationEmail.partial.obs` and `notificationEmail.partial.obs` today.

The on-disk filename can stay as `communicationsTab.partial.obs` (a code path that does not surface to admins) or be renamed to `automationsTab.partial.obs` for clarity. Either is acceptable; the constraint is that all user-visible copy, the tab label, and any related admin-doc copy use "Automations".

### `contentSection` reactivity tweak

The shared `contentSection.obs` initializes its internal collapsed state from `props.isCollapsed` once on mount and ignores subsequent prop changes. The Automations tab's pattern (Enable switch in `headerActions` driving the collapse) needs the parent to be the source of truth.

Add a watcher inside `contentSection.obs`:

```ts
watch( () => props.isCollapsed, ( newVal ) => {
    internalCollapsed.value = newVal;
} );
```

This is a non-breaking change for existing callers (they only ever pass an initial value and don't update it later) and unblocks the new Automations switch-to-collapse behavior.

### File touch list

Connection Requests feature:

- New: `Rock.ViewModels/Blocks/WorkFlow/FormBuilder/FormConnectionRequestsViewModel.cs` and `FormFieldAttributeMappingViewModel.cs`.
- New: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/Shared/connectionRequests.partial.obs`.
- Updated: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormBuilderDetail/communicationsTab.partial.obs` to render the new section.
- Updated: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/formTemplateDetail.obs` to render the new section in the template editor.
- Updated: `Rock.Blocks/WorkFlow/FormBuilder/FormBuilderDetail.cs` to load / save the new JSON property and create the Connection Request on submit.
- Updated: `Rock.Blocks/WorkFlow/FormBuilder/FormTemplateDetail.cs` to load / save the new JSON property on the template.
- Updated: `Rock/Model/Workflow/WorkflowActionForm/WorkflowActionForm.cs` to add `ConnectionRequestSettingsJson` property.
- Updated: `Rock/Model/Workflow/WorkflowFormBuilderTemplate/WorkflowFormBuilderTemplate.cs` to add `ConnectionRequestSettingsJson` property.
- EF migration NOT authored on this branch (token held on `release-19.0`). The two `nvarchar(max) NULL` columns are added when the token returns to `develop`.

Confirmation Email + Notification Email enhancements:

- Updated: `FormConfirmationEmailViewModel.cs` to add `Recipient` (`FormConfirmationEmailRecipientType` enum).
- Updated: `FormNotificationEmailViewModel.cs` to add `CampusTopicAddress` member to the destination enum and a `CampusTopicValueGuid` property. The existing `EmailAddress` member name is preserved; only the editor label updates to "Email Address(es)".
- Updated: `confirmationEmail.partial.obs` to render the Recipient radio.
- Updated: `notificationEmail.partial.obs` to render the new send-to options and use PillList for `Email Address(es)`.
- Updated: runtime senders for both emails to honor the new options.

Form List polish:

- Updated: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/formList.obs` for two-panel layout, Category Tree integration, panel-header / panel-footer button moves, last-category preference, restructured per-row card.
- Updated: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormList/newForm.partial.obs` (existing partial; replaces the spec's mention of a new `createFormView.partial.obs`) — already covers Name / Slug / Description / Category / Template with "Start Building" primary action and the spec's intro copy.
- Updated: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormList/addOrEditCategory.partial.obs` (existing partial; replaces the spec's mention of a new `addCategoryView.partial.obs`) — already covers Name / Description / Highlight Color / Icon and is dual-purpose (Add + Edit).
- `formList.obs` carries scoped placeholder styles for `.formlist-main-footer` (auto-margin-top footer pinned below the form cards) and `.formlist-main-empty` (no-category-selected empty state). UI team migrates these into `RockWeb/Styles/_blocks-workflow.less` in the styling pass after the dev team merge.
- The Rock.Blocks/Rock.ViewModels surfaces for slug uniqueness, category CRUD, and per-user preference already exist on this block; no further C# work was needed for Form List polish in this chunk.

Form Detail polish (canvas, sidebar, settings, person entry, header/footer):

- Updated: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/formBuilderDetail.obs` for top-anchored sibling tabs, sidebar-on-right layout, save/preview cluster, token / utility migration.
- Updated: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormBuilderDetail/settingsTab.partial.obs` to reorder fields, switch toggle to radio for Completion Action, refresh `SectionContainer` styling, remove the old Completion-Action button toggle.
- Updated: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormBuilderDetail/formBuilderTab.partial.obs` (or equivalent) for sidebar-on-right, hover/select state styling, 40px right-edge bar, Header/Footer panels in sidebar.
- New / Updated: Person Entry editor partial(s) restructured into General / Campus / Personal Information / Address / Family / Demographics accordions with the radio conversions listed above.
- Updated: `formContentModal.partial.obs` retired or repurposed as the in-sidebar Header/Footer panel.
- Updated: `Rock.JavaScript.Obsidian.Blocks/src/Shared/contentSection.obs` to add the `isCollapsed` watcher.
- Updated: `formTemplateDetail.obs` for token / utility migration and to expose template-level defaults for Connection Requests.

Form Submission List conversion:

- New: `Rock.Blocks/WorkFlow/FormBuilder/FormSubmissionList.cs` (entity-based Obsidian block; reuses BlockTypeGuid `A23592BB-25F7-4A81-90CD-46700724110A` from the WebForms version).
- New: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/formSubmissionList.obs` (main SFC).
- New: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormSubmissionList/filtersModal.partial.obs`.
- New: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormSubmissionList/types.partial.ts`.
- New: `Rock.ViewModels/Blocks/WorkFlow/FormBuilder/FormSubmissionList/FormSubmissionListOptionsBag.cs`.
- Removed: `RockWeb/Blocks/WorkFlow/FormBuilder/FormSubmissionList.ascx` and `FormSubmissionList.ascx.cs` (chopped). No migration required — the new Obsidian class adopts the existing BlockTypeGuid.

Form Analytics conversion:

- New: `Rock.Blocks/WorkFlow/FormBuilder/FormAnalytics.cs` (entity-based Obsidian block; reuses BlockTypeGuid `778EFA7B-56BC-4ABB-B86D-FFD87B97691F` from the WebForms version).
- New: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/formAnalytics.obs` (main SFC, including the v20 empty state with the workflow-entry guidance copy folded in).
- New: `Rock.ViewModels/Blocks/WorkFlow/FormBuilder/FormAnalytics/FormAnalyticsInitializationBox.cs` and `FormAnalyticsChartDataBag.cs`.
- Removed: `RockWeb/Blocks/WorkFlow/FormBuilder/FormAnalytics.ascx` and `FormAnalytics.ascx.cs` (chopped). No migration required — the new Obsidian class adopts the existing BlockTypeGuid.

No changes to `ConnectionRequest`, `ConnectionRequestService`, or any Connection migrations.

## Chunk 5 progress notes

Landed in the dev-team pass:

- `formBuilderDetail.obs` — renamed user-visible "Communications" → "Automations", reordered top-tab strip to Settings / Form Builder / Automations, dropped the Submissions and Analytics top-level tab links (those routes now reach via per-card buttons on Form List, see Chunks 2 and 3). Internal tab indices kept stable; the legacy `?tab=communications` query string maps to the renamed Automations tab.
- `generalSettings.partial.obs` — top section is now a plain card with Form Name + Slug (one row), Description, Category. General Settings SectionContainer below it carries Template, Form Entry Starts/Ends, Is Login Required.
- `completionSettings.partial.obs` (Shared) — Completion Action toggle replaced with a horizontal `RadioButtonList` (`Display Message` / `Redirect to New Page`).
- `personEntrySettings.partial.obs` (Shared) — restructured into 6 collapsible `Panel` accordions (General default-open, Campus / Personal Information / Address / Family / Demographics default-collapsed). Per-spec select→radio conversions applied: Record Status, Campus Type, Campus Status, Gender, Email, Mobile Phone, Birthdate, Address, Marital Status, Spouse Entry, Race Entry, Ethnicity Entry. SMS Opt-In rendered as Hide/Show radio. Record Source, Connection Status, Address Type retained as dropdowns.

Spec/state mismatches encountered:

- Spec listed `Address` under both the Personal Information and Address accordions; honored only the dedicated Address accordion.
- Spec mentioned a Family `Type` dropdown with no corresponding field in the current data model; skipped.
- `Birthdate` is not mentioned in the spec accordion structure but exists in the current data; placed in Personal Information for consistency with the surrounding visibility radios.

Landed in the follow-up pass (also Chunk 5):

- `formBuilderTab.partial.obs` field/section edit sidebar now sits on the right of the canvas (scoped `order: 2` + border flip).
- Header/Footer editing moved out of the modal pathway into a new `headerFooterEditAside.partial.obs` sidebar component. The Content textarea defaults to 6 rows and ships with help text noting `[HEADER]` / `[FOOTER]` HTML support. The retired `formContentModal.partial.obs` and the per-modal intermediate edit-content refs were deleted.
- `sectionZone.partial.obs` renders an "Add Form Fields" placeholder when a section has no fields yet.
- `generalAside.partial.obs` restructured: pinned "Settings" panel at the top with a gray backdrop (Enable Person Entry switch + Set Campus From dropdown), then the Section drag handle, Common Fields, and Additional Fields. The field type lists render as a 3-column grid of square cards (icon on top, label below) per the v20 refresh.
- `fieldEditAside.partial.obs` and `sectionEditAside.partial.obs` Conditionals editor: icon-only `<RockButton>` replaced with `+ Add` / `pencil Edit` text buttons depending on whether conditions already exist.
- `formBuilderTab.partial.obs` Person Entry canvas zone now renders as a taller card with a centered person icon plus three skeleton bars instead of the plain "Person Entry Form" text.
- `formBuilderDetail.obs` page-level `.sidebar-back` rule: removed the dark gray background pane in favor of a transparent chevron-only treatment that picks up a soft hover backdrop. Added an inset bottom shadow on `.sidebar-body` so the active edit-state panel lifts off the canvas.
- Section/field backdrop treatment: scoped overrides in `formBuilderTab.partial.obs` set the section card to a gray (`--color-interface-softer`) backdrop with a solid border, and the inner field cards to white (`--color-interface-softest`) with rounded corners so they pop against the gray section. The right-edge config bar (drag / settings / delete) inherits the gray/white treatment to match.

The `Default Preview Page` block setting on `FormBuilderDetail.cs` already exists; no change needed for that spec line.

## Chunk 6 progress notes

Confirmation Email — Recipient (Person / Spouse / Both):

- New enum `FormConfirmationEmailRecipientType` (Person = 0, Spouse = 1, Both = 2). Mirrored on the editor side in `Rock.ViewModels/Blocks/WorkFlow/FormBuilder/FormConfirmationEmailRecipientType.cs` and on the runtime side in `Rock/Workflow/FormBuilder/FormConfirmationEmailRecipientType.cs` (`[RockInternal( "1.20" )]`).
- New `Recipient` property added to `FormConfirmationEmailViewModel` and `FormConfirmationEmailSettings` (orthogonal to the existing `Destination` / `RecipientAttributeGuid` pair, which still identifies the primary person).
- `Rock.Blocks/WorkFlow/FormBuilder/ExtensionMethods.cs` round-trips `Recipient` via direct integer cast (identical enum value layout on both sides).
- `confirmationEmail.partial.obs` adds a horizontal `RadioButtonList` for the new Recipient field; default is `Person`.
- `Rock/Workflow/Action/WorkflowControl/UserForm.cs` `SendFormBuilderConfirmationEmail` honors the new field. Person and Both add the resolved primary person to recipients; Spouse and Both also look up the spouse via `Person.GetSpouse( rockContext )` and append. If no spouse is on the family record, the spouse delivery is skipped (warn-and-continue per spec).

Notification Email — radio + PillList + label refresh:

- `SegmentedPicker` replaced with `RadioButtonList` ("Send To"). Option labels: "Individual" (was "Specific Individual"), "Email Address(es)" (was "Email Address"), "Campus Topic Address" (existing).
- Recipient(s) field for the Email Address(es) destination now renders as a `PillList`. Each pill is one email string. The add handler accepts comma/semicolon/whitespace-separated input via the browser's `prompt()` and dedupes case-insensitively; the parent serializes the array back to the existing comma-separated `emailAddress` field for ViewModel/Settings parity. UI team can swap the prompt for a richer inline picker in the styling pass without changing the underlying model.
- C# `FormNotificationEmailDestination` enum member names already match the spec intent (`SpecificIndividual`, `EmailAddress`, `CampusTopic`). The spec referenced different member names (`Individual`, `EmailAddress`, `CampusTopicAddress`) — kept the existing C# names per Decision 4 (no member renames; UI labels only).

Email-to-Send block (shared by both emails):

- `emailSource.partial.obs` swapped its `SegmentedPicker` for `RadioButtonList` ("Email to Send"). Already had Subject / Reply To / Email Body / Append Organization Header and Footer wired for the Custom path; no functional change to those fields.

Build (`Rock.Blocks.csproj`) clean (only pre-existing CommunicationList obsolete warnings). Lint clean across the changed `.obs` and `.ts` files.

## Chunk 7 progress notes

Connection Requests automation section landed end-to-end without a new database column. Discovery during implementation: the existing Form Builder settings already ride inside `WorkflowType.FormBuilderSettingsJson` as one JSON blob, so the spec's proposed new `ConnectionRequestSettingsJson` columns on `WorkflowActionForm` / `WorkflowFormBuilderTemplate` aren't needed — the new sub-object stashes inside the existing blob. The original spec's column-level persistence note is superseded by this approach.

ViewModels (`Rock.ViewModels/Blocks/WorkFlow/FormBuilder/`):

- `FormConnectionRequestsViewModel` — Enabled, ConnectionTypeGuid, ConnectionOpportunityGuid, ConnectionStatusGuid, ConnectionSourceValueGuid, AttributeMappings.
- `FormFieldAttributeMappingViewModel` — FormAttributeGuid, TargetAttributeGuid (null = "Add to Connection Request Comment").
- `FormSettingsViewModel` gains a `ConnectionRequests` property.
- `FormValueSourcesViewModel` gains a static `ConnectionTypeOptions` list (opportunities / statuses / sources load on-demand because they are type-scoped).

Runtime settings (`Rock/Workflow/FormBuilder/`):

- `FormConnectionRequestsSettings` and `FormFieldAttributeMappingSettings` mirror the ViewModels with `[RockInternal( "1.20" )]`.
- `FormSettings` gains a `ConnectionRequests` property that round-trips inside `FormBuilderSettingsJson`.

Editor surface:

- `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/Shared/connectionRequests.partial.obs` — Configuration sub-panel (Connection Type / Opportunity / Status / Source dropdowns with the spec's required + optional treatment) plus an Attribute Matching sub-panel that lists every form field with a per-field target-attribute dropdown. Mapping options always include "Add to Connection Request Comment" plus every attribute on the selected Opportunity whose field type matches the form field or is Text.
- Disables its own Enable switch and flips the alert tone to `danger` when the parent reports Person Entry is off; auto-disables the section when the user turns Person Entry off via the Form Builder tab.
- Changing the Opportunity reloads the attribute list and resets any mapping whose target no longer exists, surfacing the dropped fields in a single dialog.
- `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormBuilderDetail/communicationsTab.partial.obs` renders the new section below Notification Email and forwards `isPersonEntryEnabled` + `formFields` from the parent.
- `formBuilderDetail.obs` provides a flat `automationsFormFields` computed (every field across every section) and threads the Person Entry enabled flag down.
- `Shared/types.partial.ts` and `FormBuilderDetail/types.partial.ts` carry the new TS types and the `connectionTypeOptions` source.

Server-side block actions (`Rock.Blocks/WorkFlow/FormBuilder/FormBuilderDetail.cs`):

- `GetConnectionOpportunities( connectionTypeGuid )` — active opportunities for the selected type.
- `GetConnectionStatuses( connectionTypeGuid )` — active statuses for the selected type, ordered by Order/Name.
- `GetConnectionSources( connectionTypeGuid )` — `ConnectionTypeSource` rows for the selected type (the spec called this a "defined-value" but it is actually a type-scoped entity; the editor calls this out as a Source dropdown that disables when no Connection Type is chosen).
- `GetOpportunityAttributes( opportunityGuid )` — returns each attribute's Guid, Name, and FieldTypeGuid so the client can filter by field-type compatibility (matching field-type or Text).

Mapping (`ExtensionMethods.cs`):

- `FormConnectionRequestsSettings.ToViewModel()` / `FormConnectionRequestsViewModel.FromViewModel()` round-trip the new settings, including the per-field mapping list.
- `FormBuilderDetail.cs` reads `settings.ConnectionRequests` and writes `formSettings.ConnectionRequests.FromViewModel()` in the existing save path; no new column needed.

Runtime (`Rock/Workflow/Action/WorkflowControl/UserForm.cs`):

- `CreateFormBuilderConnectionRequest` runs after the confirmation + notification emails so a misconfigured section never blocks delivery. Wrapped in `try { ... } catch { ExceptionLogService.LogException(...) }` per spec.
- Resolves the requestor from the workflow's "Person" attribute (the Form Builder Person Entry primary person). Skip-and-continue when no person is resolved.
- Resolves the opportunity, status (explicit selection wins; otherwise the type's default status), and source (`ConnectionTypeSource`).
- Sets `ConnectorPersonAliasId` from `opportunity.GetDefaultConnectorPersonAliasId( campusId )` where `campusId` is the requestor's `PrimaryCampusId`.
- Walks `AttributeMappings`: null target appends `"{Label}: {Value}"` (one per line) to `ConnectionRequest.Comments`; non-null target sets the attribute via `SetAttributeValue` after `SaveChanges`.

Persistence note for downstream pass: the spec's separate per-form-override and per-template-override `ConnectionRequestSettingsJson` columns are NOT authored on this branch. Both inputs ride inside the existing `WorkflowType.FormBuilderSettingsJson` blob. If a future spec genuinely wants per-form-override-of-template, that wiring is additive on top of the current shape and remains a follow-up.

`Rock.Blocks.csproj` build clean (only pre-existing CommunicationList obsolete warnings). Lint clean across the new and modified `.obs` / `.ts` files.

## Post-chunk follow-ups

Landed in a clean-up pass after the seven chunks:

- **Slug edit warning (Decision 2).** `FormBuilderDetailViewModel.HasSubmissions` is now populated in `GetObsidianBlockInitialization` (a single `Any()` against `WorkflowService`). The flag flows through `formBuilderDetail.obs` → `settingsTab.partial.obs` → `generalSettings.partial.obs`. The general settings card captures the slug at mount (`originalSlug`) and renders a warning `NotificationBox` above the Description / Category fields when `hasSubmissions && slug !== originalSlug`. The save itself is not blocked — warn-only per Decision 2.

Still deferred and worth tracking:

- **Per-template Connection Requests.** `WorkflowFormBuilderTemplate` uses discrete JSON columns (PersonEntrySettingsJson, ConfirmationEmailSettingsJson, CompletionSettingsJson) rather than the general blob pattern that `WorkflowType.FormBuilderSettingsJson` provides, so there is no existing column the new ConnectionRequests settings can ride inside. The spec's "Configuration only at template level" decision (Decision 1) is good design but needs either (a) a fresh `nvarchar(max) NULL` column on the template entity, which needs the migration token currently held on `release-19.0`, or (b) packing the new settings into one of the existing template JSON blobs (rejected as hacky). Both the C# entity property and the editor section on `formTemplateDetail.obs` are deferred until the migration can be authored.
- **Form Template Detail token / utility audit.** Spec calls for a css-cleanup pass on `formTemplateDetail.obs` even where the Figma refresh does not change layout. UI team owns this work per the user's preference around visual polish.

## Decisions

- **Per-form vs per-template Connection Requests defaults.** Template-level defaults expose the **Configuration** sub-panel only (Connection Type / Opportunity / Status / Source). Attribute Matching is per-form by definition (the template does not know the form's field set), and is not pre-seeded by the template.
- **Slug edits on existing forms.** The Settings tab keeps `Slug` editable, but **warns** (non-blocking, inline alert above the field) when the form already has submissions. The warning notes that existing links built against the old slug will break. No read-only enforcement.
- **Notification Email enum rename.** The C# member `FormNotificationEmailDestination.EmailAddress` is **NOT renamed**. Only the user-visible label changes from "Email Address" to "Email Address(es)" to reflect plurality of the recipients input. New `CampusTopicAddress` member is still added.
- **Connection Request runtime sequencing.** Connection Requests run after the workflow's existing person-resolution and email actions, using whatever `PersonAlias` resolved the form-submitter (Person Entry primary person). No parallel/race path with person persistence.

## Considered but Rejected

### Keep the secondary tabs at the bottom of the canvas (prior 260507 spec direction)

Rejected. The refreshed Figma anchors the tabs at the top, which matches the rest of v20's detail-page conventions and pairs cleanly with the Save/Preview cluster in the same top-right region. Bottom-anchored tabs would be a Form Builder-only treatment.

### Keep the edit sidebar on the left of the canvas

Rejected. The refreshed Figma moves the sidebar to the right so the canvas reads first and the edit affordances follow the user's eye to the field/section they just clicked. Left-rail placement also conflicts with the new top-anchored tab strip; the tab strip and the sidebar header would compete for the upper-left.

### Keep Header / Footer editing in modals

Rejected. Header/Footer are two short HTML editors, modal usage adds friction (open, edit, close, can't see canvas state), and the right-side sidebar already has the room to host them with a 6-row textarea by default. The modal pathway is retired.

### Keep the inline Header / Footer treatment proposed in the prior 260507 spec

Rejected. Inline Header / Footer would push the canvas down whenever those panels are expanded and would conflict with the sidebar's edit-state experience (selecting a field opens the sidebar, but selecting Header/Footer would open inline). The sidebar treatment unifies all "configure something on the canvas" interactions into one location.

### Use explicit Person / Connector / Campus attribute pickers for Connection Requests (prior 260507 spec direction)

Rejected. The new design routes the requestor through Person Entry's primary person, which is the only person the form natively knows about post-submission. Adding parallel person-attribute pickers for Connector and Campus reintroduces the workflow-attribute friction the feature is removing; the runtime falls back to the opportunity's default connector and the primary person's primary campus, which covers the realistic configuration space. Connector overrides are a workflow-author concern, not a form-author concern.

### Use a Lava `CommentTemplate` for Connection Request Comments (prior 260507 spec direction)

Rejected. Attribute Matching is a stronger model: form responses either land on a structured Connection Request attribute (queryable, filterable, displayable on the request detail page) or are appended to Comments verbatim, in form order, with a label prefix. A free-form Lava template accomplishes the same routing in theory but requires admins to know merge-field syntax for what is otherwise a point-and-click mapping; it also bypasses Connection Opportunity attributes entirely.

### Build "Connection Requests" as a new completion-action option in `CompletionSettings`

Rejected. `CompletionSettings` configures what happens to the *form view* after submit (show message, redirect). Bolting "open connection request" into CompletionSettings would conflate "what does the user see next" with "what does Rock do behind the scenes." Email shortcuts already establish the right precedent: their own section, their own toggle, their own settings JSON, separate from CompletionSettings.

### Add a fully separate "Connections" tab next to Automations

Rejected for v1. A whole tab is overweight for one settings panel, and the Automations rename already broadens the existing tab's name to legitimately host non-email post-submission automations. If future work adds multi-action Connection support (e.g. open one request, transfer another, log activity on a third), revisit the tab split then.

### Keep the tab named "Communications" and add Connection Requests alongside email

Rejected. The Figma renames the tab to **Automations** and that name reads accurately once a non-email automation lives in the same tab. Keeping the legacy "Communications" name would confuse the new section and require a cosmetic-only rename in a follow-up release.

### Defer Form Submissions and Form Analytics conversions to v20.1

Rejected. The v20 Form Builder family ships visually consistent or it doesn't. Leaving Submissions and Analytics on WebForms while Form List, Form Detail, and Template Detail are in Obsidian creates a half-and-half experience where a v20 admin clicks "Submissions" from the new Form List and lands on a WebForms page from the v17 era. The conversions are basic (no new feature work) so the additional cost is bounded; the consistency win is large.

### Keep the cross-block nav-tabs across Form Submissions, Form Analytics, and the runtime form

Rejected. Form List is the only entry point into a specific form's Submissions and Analytics views in the new design. The cross-tabs duplicate that navigation, complicate the block settings (each tab requires a `LinkedPage` setting on every other block), and are obsoleted by the per-card affordances on Form List.

### Modify the existing `SectionContainer` to add a switch slot rather than tweak `contentSection`

Rejected. The Automations sections specifically use `contentSection` (the shared low-level component); promoting the switch into `SectionContainer` would require either rewriting Automations to use `SectionContainer` (large blast radius across other blocks that depend on `contentSection`'s exact API) or adding parallel switch handling in two components. The minimal `watch` on `props.isCollapsed` in `contentSection` is non-breaking and unblocks the feature with one line of code.

### Hard-depend on the SMS Action spec landing first

Rejected as a blocker, but coordinated. If the SMS spec ([260506-sms-action-create-connection-request.md](260506-sms-action-create-connection-request.md)) lands first and ships a shared `ConnectionTypeSettingsFieldType` with a `connectionTypeSettingsPicker.obs`, this feature can adopt it for the Configuration sub-panel. If not, this feature ships its own picker partial and the SMS work consolidates onto the shared control later. Blocking the v20 Form Builder polish on a separate spec would be overcoupling.

### Polish-only, defer Connections and Confirmation/Notification email enhancements to v20.1

Rejected. The Asana ticket bundles them deliberately because the polish pass touches every partial in the renamed Automations tab (Confirmation Email, Notification Email, and the new Connection Requests). Landing the new options and the new section in the same change avoids re-touching the same partials in a follow-up. A polish-only PR would still need to leave hooks for the new fields, defeating the point of splitting them.

## Related

- Asana task: [DEV-12581 - Update Form Builder - Polish and Connections Support](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1214411802884699)
- Refreshed Figma frames (auth required, not verifiable via tooling; PNG exports staged under `artifacts/260508-form-builder-updates/` are treated as canonical for this spec):
  - Form List + Create New Form view + Add Category view (frame 16-2850)
  - Form Detail polish, Settings refresh, Automations tab with Connection Requests, Form Submissions conversion, Form Analytics conversion + new empty state (frame 5023-63652)
- Prior WIP draft (superseded by this spec): `specs/260507-form-builder-polish-and-connections-support.md` (deleted from `specs/` in commit `513f381b8e`).
- Sibling spec: [260506-sms-action-create-connection-request.md](260506-sms-action-create-connection-request.md). Same "open a connection request without going through a workflow" pattern, applied to inbound SMS. Source of the proposed shared `ConnectionTypeSettingsFieldType` and `connectionTypeSettingsPicker.obs`.
- Reference workflow action: `Rock/Workflow/Action/Connections/CreateConnectionRequest.cs`
- Existing Form Builder block: `Rock.Blocks/WorkFlow/FormBuilder/FormBuilderDetail.cs`
- Existing communications tab partial: `Rock.JavaScript.Obsidian.Blocks/src/WorkFlow/FormBuilder/FormBuilderDetail/communicationsTab.partial.obs`
- Existing notification email shape (closest model for the new section's persistence and partial layout): `Rock.ViewModels/Blocks/WorkFlow/FormBuilder/FormNotificationEmailViewModel.cs`
- Branch commit that established the PillList pattern referenced in the polish requirements: `b933a3b0c5`
