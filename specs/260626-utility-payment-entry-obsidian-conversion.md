---
author: Jason Hendee
date_created: 2026-06-26
summary: >-
  Convert the Finance Utility Payment Entry WebForms block to Obsidian as part of the
  Fall 2025 / Spring 2026 block refresh. Applies the Figma UI polish, reorganizes block
  settings into tabs (with renamed labels and several new settings), relocates the
  CampusAccountAmountPicker account/campus mapping to the C# save path.
contributors: []
---

# Utility Payment Entry Obsidian Conversion

## Summary

`UtilityPaymentEntry` is the public-facing giving / payment block that creates a one-time or
scheduled financial transaction. This spec covers converting it from WebForms
(`RockWeb/Blocks/Finance/UtilityPaymentEntry.ascx[.cs]`, ~4,400 lines) to an Obsidian block
(`Rock.Blocks/Finance` + `Rock.JavaScript.Obsidian.Blocks/src/Finance`). The work is primarily a
UI polish (no flow changes intended) plus three deliberate technical changes: relocating the
campus/account mapping logic to the server, reorganizing block settings into tabs, and adding a set
of new presentation settings.

## Motivation

The block is part of the "Obsidian Block Refreshes, Fall 2025 / Spring 2026" effort (Asana
DEV-13440). WebForms is being phased out, and the product owner asked for a visual refresh of this
block with an explicit constraint: do not make feature changes, and flag anything that would "move
someone's cheese."

There is no existing Obsidian giving-flow block to clone. The closest Finance entry blocks are
`financialPledgeEntry.obs` and `fundraisingDonationEntry.obs`, neither of which models the
multi-step gateway-backed transaction flow. This is a net-new Custom interactive conversion.

## Block Overview

- **Purpose:** create a new one-time or scheduled `FinancialTransaction` (credit card or ACH) via a
  configured gateway. Also supports Text-to-Give account setup mode.
- **Audiences:** external (public giving page) and internal (staff entering a gift on someone's
  behalf, gated by the Staff Impersonation setting).
- **Flow:** Entry, an optional Confirmation step, then Success. Edge cases include "no gateways
  configured" and a possible-duplicate-transaction warning.
- **Key control:** the WebForms version uses `CampusAccountAmountPicker` in account/campus mapping
  mode (`UseAccountCampusMappingLogic`), which translates the selected account into the real
  financial account for the chosen campus. See `UtilityPaymentEntry.ascx.cs:1158` and `:1191`.

```mermaid
flowchart LR
    A[Entry] -->|gateways configured| B{Show Confirmation Step?}
    A -->|no gateways| Z[Config message]
    B -->|yes| C[Confirmation]
    B -->|no| D[Process transaction]
    C --> D
    D -->|possible duplicate| C2[Duplicate warning]
    C2 --> D
    D --> E[Success]
```

## Implementation Status

| # | Item | Implemented | Tested |
|---|---|:---:|:---:|
| 1 | [Block scaffold and initialization](#1-block-scaffold-and-initialization) | ✅ | ✅ |
| 2 | [Block header section](#2-block-header-section) | ✅ | ✅ |
| 3 | [Heading model (block header vs panel title)](#3-heading-model-block-header-vs-panel-title) | ✅ | ✅ |
| 4 | ["No gateways configured" help](#4-no-gateways-configured-help) | ✅ | ✅ |
| 5a | [Entry: Campus Information section](#5a-entry-campus-information-section) | ✅ | ✅ |
| 5b | [Entry: Contribution Information section](#5b-entry-contribution-information-section) | ✅ | ✅ |
| 5c | [Entry: Contact Information section](#5c-entry-contact-information-section) | ✅ | ✅ |
| 5d | [Entry: Payment Information section](#5d-entry-payment-information-section) | ✅ | ✅ |
| 6 | [Confirmation step](#6-confirmation-step) | ✅ | ✅ |
| 7 | [Success step: Save Payment Method and Text-to-Give](#7-success-step-save-payment-method-and-text-to-give) | ✅ | ✅ |
| 8 | [Other gateway states (ACH/CC off, unsupported, Test)](#8-other-gateway-states-achcc-off-unsupported-test) | ✅ | ✅ |
| 9 | [Transaction processing: one-time gift (Entry to charge to Success)](#9-transaction-processing-one-time-gift-entry-to-charge-to-success) | ✅ | ✅ |
| 10 | [Possible-duplicate warning (reachable trigger)](#10-possible-duplicate-warning-reachable-trigger) | ✅ | ✅ |
| 11 | [Scheduled / recurring gifts (save path)](#11-scheduled--recurring-gifts-save-path) | ✅ | ✅ |
| 12 | [Show Initial Back Button](#12-show-initial-back-button) | ✅ | ✅ |
| 13 | [Transfer flow (move a scheduled gift to another gateway)](#13-transfer-flow-move-a-scheduled-gift-to-another-gateway) | ✅ | ✅ |
| 14 | [Layout Style (Vertical / Fluid)](#14-layout-style-vertical--fluid) | ✅ | ✅ |
| 15 | [Transaction Header merge fields](#15-transaction-header-merge-fields) | ✅ | ✅ |
| 16 | [URL account options](#16-url-account-options) | ✅ | ✅ |
| 17 | [Account Campus Context Filter](#17-account-campus-context-filter) | ✅ | ✅ |
| 18 | [Account Confirmation Email setting](#18-account-confirmation-email-setting) | ✅ | ✅ |
| 19 | [Page-parameter ID hardening](#19-page-parameter-id-hardening) | ✅ | ✅ |
| 20 | [Transaction attributes from URL (`Attribute_*` params)](#20-transaction-attributes-from-url) | ✅ | ✅ |
| 21 | [Block-settings reconciliation](#block-settings-reconciliation) | ✅ | ✅ |
| 22 | Soft-launch registration (coexist with legacy) | ✅ | ✅ |
| 23 | Business Work-phone write-back (revisit) | | |
| 24 | Preload account amounts on transfer (consider) | | |
| 25 | Use modelValue instead of presetAccountAmounts in UPE (investigate) | | |
| 26 | WebForms chop (deferred) | | |

Legend: ✅ = complete; 🔄️ = in progress; blank = not started. The **Tested** column is a rollup: a row
is ✅ only when every one of its sub-feature test items in [Test Plan](#test-plan) passes.

## Remaining Work

Each item's remaining work, keyed to the [Implementation Status](#implementation-status) grid.
Completed work is tracked by the grid markers and the checked [Test Plan](#test-plan) items, not
restated here.

- **23. Business Work-phone write-back (revisit).** When giving as a business, the Work-phone write-back skips saving the entered number if the contact already has a mobile number on file, carried over from the legacy block and documented as a parity flag under [Business giving](#business-giving-give-as-business). This is a bad behavior; if the extended testing window allows, fix it so the business Work phone is saved regardless. See the [Business Work-phone write-back question](https://app.asana.com/1/20866866924293/task/1216405822150745).
- **24. Preload account amounts on transfer (consider).** The transfer flow does not pre-fill account amounts today ([Test 13](#13-transfer-flow-move-a-scheduled-gift-to-another-gateway)), because the shared `campusAccountAmountPicker` did not rehydrate from an incoming value. That mechanism now exists (the control resolves the amount from its `modelValue`), so the block could seed the transferred schedule's amounts by passing them as the picker's model value. Consider implementing; mind the campus-mapping caveat: a schedule's stored details are the campus child accounts, so seeding may need to reverse-map to the parent account the picker displays (or accept showing the child).
- **25. Use `modelValue` instead of `presetAccountAmounts` in UPE (investigate).** Now that the shared `campusAccountAmountPicker` resolves amounts from `modelValue` (Carter's approach), investigate whether UPE can seed its accounts and amounts through the `modelValue` and `accounts` props alone and drop the `presetAccountAmounts` prop and its overhead. Quick take: the seeding looks feasible; the snag is read-only locking, which multi-mode already honors from a model entry's `readOnly` but single-mode currently derives only from a preset, so fully dropping presets would need the single-mode resolver to also honor a model-supplied `readOnly` (or keep presets just for locks). Server-side lock enforcement re-parses the URL, so it is unaffected either way.
- **26. WebForms chop (deferred).** The Obsidian block is soft-launched under a new BlockTypeGuid so it coexists with the legacy WebForms block; the chop's BlockTypeGuid is kept commented-out beside it in `Rock.Blocks/Finance/UtilityPaymentEntry.cs`. At chop time, add a migration that removes the orphaned soft-launch block type (by its Guid) and its attributes and values (each by their respective Guids). Optionally, a more involved and delicate migration could move any "snuck" instances of the old block onto the chopped version; not committed.

### Page parameters (carry-forward tracking)

The legacy block's page parameters are all declared in `PageParameterKey`. The rows below track which
are threaded; any still-unconsumed ones must be wired as their owning slice lands. `RecordSource` is
declared and also read via `RecordSourceHelper` in the new-person path. None may be dropped without a
decision. Keys preserve the legacy names (PascalCase) for backward compatibility.

| Parameter | Purpose | Status | Owning slice |
|---|---|---|---|
| `CampusId` | Pre-selects the campus | Threaded | 5a ✅ |
| `StartDate` | Seeds and clamps the scheduled start date | Threaded | 5b-iii ✅ |
| `Frequency` | Pre-selects and optionally locks the frequency (`id^false`) | Threaded | 5b-iii ✅ |
| `AccountIds` | URL account options (ids, preset / read-only amounts) | Threaded | 16 ✅ |
| `AccountGlCodes` | URL accounts resolved by GL code | Threaded | 16 ✅ |
| `AmountLimit` | Caps the total at submit; also a header / footer merge field | Threaded | Submit cap ✅ (in ValidateEntry); header / footer merge field ✅ (item 15) |
| `Attribute_*` | Sets allow-listed transaction attributes from the URL | Threaded | 20 ✅ |
| `rckid` | Target-person / impersonation token | Threaded | Target-person resolution ✅ |
| `Transfer` | Marks a scheduled-gift transfer | Threaded | Transfer flow ✅ |
| `ScheduledTransactionGuid` | The schedule being transferred | Threaded | Transfer flow ✅ |
| `ParticipationMode` | Family vs individual fundraising totals (GroupMember entity) | Threaded | Fundraising merge fields ✅ (item 15) |
| `RecordSource` | Overrides the new-person record source (via `RecordSourceHelper`) | Threaded | New-person creation ✅ |

The block-settings reconciliation pass (below) covers attributes; this table is its page-parameter
counterpart, and the same "threaded, wired, or dropped" rule applies to every row before the
conversion is considered complete.

## Test Plan

Per-sub-feature test checklists backing the **Tested** rollup in
[Implementation Status](#implementation-status). The rule: a status-grid row is **Tested** only when
all of its items here are checked. Each slice adds its own checklist when it is built; do not mark a
parent row tested on the strength of the happy path alone.

Items under rows already marked ✅ in the grid are carried pre-checked from earlier review. Uncheck
and re-verify any you want to spot-check; if one fails, the parent row drops back to blank.

### Test environment setup

The entry flow only renders when a gateway resolves, so most of these need a configured block:

- **Gateway:** assign the block's Financial Gateway to the **Test Gateway**. It is non-fatal (shows
  a "Testing" notice, then proceeds to the entry flow), so it is the quickest way to exercise the
  entry sections without real charges.
- **Campuses:** have at least two active campuses to see the picker; one campus (or a filter that
  leaves one) to test the auto-hide. Assign Campus Type and Campus Status defined values to some
  campuses to test those filters.
- **People:** test signed in as a person whose family has a campus, and as one with none, to cover
  the default-selection paths. Add `?CampusId=` to the URL to test the param path.
- **Audiences:** test on both an internal admin instance and an actual external giving site, since
  the external giving theme is not the NextGen theme (see the external-theme item under 5a).

### 1. Block scaffold and initialization
- [x] Block loads with no console errors and the options bag reaches the client.
- [x] `errorMessage` path renders the danger `NotificationBox`.

### 2. Block header section
- [x] Header (icon + title + description) shows when Show Block Header Section is ON.
- [x] Header hidden when OFF.
- [x] Header renders centered on the external (non-NextGen) giving theme (scoped CSS).

### 3. Heading model (block header vs panel title)
- [x] Panel title shows on internal when headings are ON and the block header is OFF.
- [x] Panel title hidden when the block header is shown (the two never stack).
- [x] No-gateway state shows the fixed title "Getting Started With Contributions".
- [x] With a gateway configured, the panel title shows the configured Panel Title setting (not the fixed no-gateway string).
- [x] Editing the Panel Title setting is reflected in the rendered title.

### 4. "No gateways configured" help
- [x] Welcome `NotificationBox` plus one `DisplayCard` per supported gateway.
- [x] Test Gateway excluded; only gateways with an active instance listed.
- [x] Configure / Learn More links resolve to the right URLs.

### 5a. Entry: Campus Information section
Visibility and gating:
- [x] Section renders in the entry flow when a gateway is configured and at least two campuses are selectable.
- [x] Section is fully hidden (no header, no empty gap) when only one or zero campuses are selectable.
- [x] Section header (icon + title + description) shows when Show Panel & Section Headings is ON.
- [x] Section header is fully suppressed but the picker still shows when Show Panel & Section Headings is OFF.

Prompt-when-known (legacy `AskForCampusIfKnown` parity):
- [x] Prompt for Campus When Known ON + a known campus: picker is still shown.
- [x] Prompt for Campus When Known OFF + a known campus: section is suppressed (the known campus is carried forward silently).
- [x] Prompt for Campus When Known OFF + no known campus: section is shown.

Default selection:
- [x] `CampusId` page parameter pre-selects that campus.
- [x] No param: the current person's family campus is pre-selected.
- [x] No param and no person campus: nothing is pre-selected. The picker offers a blank "no campus" option (`showBlankItem`) so campus stays optional, matching legacy (legacy did not require a campus selection).

Filters:
- [x] Include Inactive Campuses OFF hides inactive campuses; ON shows them.
- [x] Campus Type Filter limits the list to the selected types.
- [x] Campus Status Filter limits the list to the selected statuses.
- [x] A pre-selected campus that a filter would exclude still appears in the list.

Copy and rendering:
- [x] Section title, icon, and description reflect the three block settings (and their Figma defaults out of the box).
- [x] Only the Campus section renders as a light (bordered) card; Contribution, Contact, and Payment use the default section styling, per Figma.
- [x] The whole entry flow, including this section's `ContentSection` chrome, renders correctly on the actual external giving theme, not just the internal admin site.

### Entry: Transaction Header (Lava banner, shared across the entry flow)
- [x] A configured Transaction Header Template renders as HTML above the entry sections.
- [x] Lava in the template resolves on initial load with the common merge fields (e.g. `{{ CurrentPerson }}`).
- [x] A blank template renders nothing (no empty banner).

### 5b. Entry: Contribution Information section
- [x] Multi-account mode (`Allow Multiple Accounts` on): one amount box per configured account, each labeled with the account name.
- [x] Single-account mode (`Allow Multiple Accounts` off): account dropdown + one amount box, with proper spacing between them.
- [x] The configured `Accounts to Display` all appear, ordered by each account's `Account.Order` (give them distinct orders to verify). Moves-cheese note: legacy ordered by stored Id, so on-screen order can differ after upgrade.
- [x] No accounts configured: the picker falls back to all active/public/in-date accounts (legacy parity).
- [x] The control's own campus picker never renders (`alwaysHideCampus`); campus stays solely in the Campus Information section.
- [x] Section header (icon + title + description) respects the Show Panel & Section Headings toggle.
- [x] Renders correctly on the external giving theme, not just internal.
**5b-ii: Add Another Account dropdown (flat).** Setup: have 3+ active/public accounts; configure a *subset* in `Accounts to Display`. The dropdown's visibility is **not** gated by Enable Multi-Account; it shows in single and multi alike. Combos (✅ = dropdown shows):

| Allow Additional | Accounts configured | Account left to add | Dropdown? |
|:---:|:---:|:---:|:---:|
| on | subset | yes | ✅ |
| **off** | subset | yes | hidden |
| on | subset | **no** (all eligible already shown) | hidden |
| on | **none** (show-all) | n/a | hidden |

- [x] The four combos above match.
- [x] Mode-independence: dropdown shows in **single** mode AND **multi** mode (subset configured + Allow Additional on).
- [x] Multi mode: selecting an account adds a new amount box and **keeps amounts already typed** in the others (merge fix).
- [x] Single mode: selecting an account adds it to the account dropdown's options and **keeps the current selection** (merge fix).
- [x] The added account disappears from the add dropdown; when none remain, the dropdown hides.
- [x] The dropdown's prompt text reflects the `Add Account Button Text` setting.

**5b-ii-b: Add Another Account (hierarchy).** Setup: turn ON `Group Additional Accounts by Hierarchy`; configure a *subset* in `Accounts to Display`; in the addable pool have a parent account with 2+ active/public child accounts, plus at least one standalone account with no children. The control is a `treeItemPicker` (a button, labeled by Add Account Button Text, that opens a popup tree). The server ships the addable accounts as a `TreeItemBag` tree (nested in hierarchy mode, flat roots otherwise); folder selection is enabled so any node is addable.
- [x] The popup shows a tree with children nested under their parent, expandable and collapsible.
- [x] Every node is selectable, parents included; clicking a node adds it and closes the popup (single-select applies on click).
- [x] Standalone accounts (no parent in the pool) appear at the tree root.
- [x] A child whose parent is one of the configured (displayed) accounts appears at the root, since its parent is not in the addable pool.
- [x] Adding an account removes it from the tree; if it was a parent, its children stay addable (promoted into its place). When every account is added, the picker hides.
- [x] Hierarchy OFF: the popup is a flat tree (no nesting) ordered by account order; behavior is otherwise identical.
- [x] A 3-level hierarchy nests each level and keeps all three selectable.

**5b-iii: Scheduling ("how often") and comment.** These render as a second `ContentStack` below the accounts (a divider appears between the two stacks). Setup: assign a gateway that supports recurring schedules (the Test Gateway works); turn ON `Allow Scheduled Gifts`, `Allow Scheduled End Date`, and `Allow Comment Entry`.
- [x] Frequency dropdown lists the gateway's supported frequencies plus One-Time (inserted first), defaulting to One-Time.
- [x] One-Time selected: the start-date label reads "When" and the end date is hidden.
- [x] A recurring frequency selected: the start-date label reads "First Gift" and the end date shows (Allow Scheduled End Date on).
- [x] Allow Scheduled End Date OFF: the end date never shows, even for a recurring frequency.
- [x] Start date defaults to today and rejects past dates.
- [x] `Allow Scheduled Gifts` OFF, a gateway with no supported schedules, or Text-to-Give mode on: the frequency/date fields do not render.
- [x] `StartDate` page param pre-fills the start date (clamped to today); `Frequency` page param (`frequencyId^false`) pre-selects and locks the frequency, replacing the dropdown with static text (matching legacy's read-only literal).
- [x] Comment field shows only when `Allow Comment Entry` is on, labeled by `Comment Field Label`, as a multiline textarea.
- [x] If scheduling is off but the comment is on, the second stack still appears with just the comment.
- [x] Comment is optional: it shows no required indicator and can be left blank. This deliberately differs from legacy (`Required="true"`) and the Figma, which both required it, per developer (JPH) decision.

### 5c. Entry: Contact Information section
Setup: turn ON Prompt for Email, Prompt for Phone, SMS Opt-In, and Allow Anonymous Giving; test signed in
as an individual with a name, email, phone, and address on file, and as an anonymous visitor. (Business
and business-contact fields are covered under [Business giving](#business-giving-give-as-business).)
- [x] Section renders after the Contribution section with its header (icon + title + description), respecting the Show Panel & Section Headings toggle.
- [x] The address control renders.
- [x] Email input shows only when Prompt for Email is on.
- [x] Phone input shows only when Prompt for Phone is on.
- [x] SMS opt-in checkbox shows only when SMS Opt-In is on and phone is prompted, labeled by the SMS opt-in system setting.
- [x] Give Anonymously checkbox shows only when Allow Anonymous Giving is on; the Anonymous Giving Tooltip appears as a help bubble beside it.
- [x] Renders correctly on the external giving theme, not just internal.
- [x] A signed-in individual with a name sees their name read-only (labeled "Name"); the First/Last name entry fields do not show.
- [x] A new, nameless, or anonymous individual sees the editable First and Last name inputs side by side (no read-only name).
- [x] Email, phone (with country code), and address prefill from the resolved individual; the SMS opt-in checkbox starts checked when the prefilled number has messaging enabled.
- [x] Unlisted-phone flip: when the individual's best number (home, then mobile) is unlisted, the phone section and the SMS opt-in do not show and no number is prefilled; a listed number prefills and shows.
- [x] Prefill honors the target person under impersonation (`rckid`): an admin entering a gift on someone's behalf sees that individual's contact details, not their own.

### 5d. Entry: Payment Information section
Setup: a configured hosted gateway (the Test Gateway works); optionally toggle Disable CAPTCHA; for the saved-account items, sign in as an individual with one or more saved accounts on that gateway. The hosted control does not collect a billing address (the Contact section captures it); verify the billing sent to the gateway.
- [x] Section renders after the Contact section with its header, respecting the Show Panel & Section Headings toggle.
- [x] The hosted gateway control renders its card / ACH entry for the configured gateway, reflecting Enable ACH / Enable Credit Card.
- [x] The CAPTCHA widget shows, and is hidden when Disable CAPTCHA is on.
- [x] Renders correctly on the external giving theme, not just internal.
- [x] The action buttons live in the Panel footer (with its divider above the content), not in the section bodies: Previous on the left, the primary action on the right.
- [x] The CAPTCHA renders right-aligned in the Panel footer, taking the action button's place until solved (per Figma); it still shows/hides per Disable CAPTCHA.
- [x] With Disable CAPTCHA on (or none configured server-wide), the action button shows right-aligned in the footer.
- [x] With CAPTCHA enabled, the entry action button (Give/Next) is hidden until the CAPTCHA is solved, then appears; solving does not trigger a server round-trip (matches legacy's client-side reveal).
- [x] With Disable CAPTCHA on, or no CAPTCHA configured server-wide, the button shows immediately (never stuck hidden).
- [x] Validate-at-submit: the solved token rides with the entry submit (GetConfirmation when a confirmation step is on, else ProcessTransaction) and is validated via RequestContext.IsCaptchaValid; an invalid or stale token is rejected with "Please complete the verification again to continue." and the giver stays on the entry step.
- [x] Re-solve on failure: any failed entry submit re-shows the CAPTCHA, hides the action button, and remounts the gateway control. Covers all three failure modes: invalid/empty payment fields, a gateway/tokenize error, and a charge decline (confirmation off). A charge failure on Finish (confirmation on) does not reset it. Verify the gateway-control remount on a hosted-field gateway (the Test Gateway does not reproduce the teardown).
- [x] With a confirmation step on, the CAPTCHA is validated once at Next (GetConfirmation); Finish (ProcessTransaction) does not re-validate the one-shot token.

Saved payment methods (built this pass; sign in as an individual with one or more saved accounts on the configured gateway):
- [x] The saved-account list shows only when the target individual has at least one saved account for the gateway; each is a display card with the account name, a currency-type icon (credit card, bank, or Apple/Google Pay), a detail line that leads with the card brand when known (e.g. "Visa Ending in 6789 · Expires 02/30"), and a radio button (in the card's actions) marking selection. Built from the shared `SavedFinancialAccountListItemBag` list (extended with the card brand) so it matches the other Obsidian giving blocks.
- [x] Only saved accounts whose currency type is allowed appear: credit card requires Enable Credit Card, ACH requires Enable ACH; Apple Pay / Google Pay appear whenever the gateway supports saving them.
- [x] Selecting a card, by clicking anywhere on it or its radio, marks it selected (radio checked), hides the hosted card / ACH control, and leaves only one card selected at a time.
- [x] The list defaults to the first saved account. An "Add Payment Method" card (a first-class `DisplayCard` with its own radio button) stays visible at the end of the list; selecting it clears the saved-account selection, keeps its own radio checked, and reveals the hosted card / ACH control; selecting a saved card again re-hides the control.
- [x] Giving with a saved account selected charges its stored payment method (no tokenization) and records the gift, authorized to the resolved giver, with the confirmation step on and off.
- [x] With CAPTCHA enabled and a saved account selected (no card to tokenize), the action button is still gated by the CAPTCHA and the solved token is validated at submit; the saved-account path does not bypass it.
- [x] Give as Business on: the saved-account list still shows the target individual's saved cards (the list is not gated by the Give-as-Business toggle, matching legacy); selecting one charges that card while the gift is authorized to the business.
- [x] Under impersonation (`rckid`), the saved accounts listed are the target individual's, not the admin's.
- [x] A saved account deleted between page load and submit surfaces "The selected saved payment method is no longer available. Please choose another payment method." and the giver stays on the entry step (no crash).
- [x] The saved-account cards render correctly on the actual external giving theme, not just the internal admin site (DisplayCard is a design-system component; confirm its styling holds off the NextGen theme). The card icons pass `iconStyle="tile"` so they render through DisplayCard's own scoped CSS-variable tile, not the theme's `.label-default` (which the non-NextGen giving theme leaves as the dark Bootstrap label).

### 6. Confirmation step
Setup: assign the Test Gateway, turn Show Confirmation Step ON, sign in as a person with a primary alias,
and enter an amount for at least one account.

Flow and gating:
- [x] With Show Confirmation Step ON, the entry action button reads "Next" (not "Give").
- [x] Clicking Next tokenizes the card, then shows the confirmation step; Entry is hidden, not unmounted.
- [x] With Show Confirmation Step OFF, the button reads "Give" and the gift processes straight to Success with no confirmation step (regression check of the existing one-time path).

Content:
- [x] The Confirmation Header Lava renders above the summary (default: "Please confirm the information below...").
- [x] The Confirmation Body renders the gift summary: one row per entered account (mapped account public name + amount), a Total, and When / Name / Email / Address.
- [x] Giving as a business: the "Name" row shows the business name (matching the billing name sent to the gateway).
- [x] No Phone row appears in the confirmation summary (dropped per Figma), but the `Phone` merge field is retained so a customized template can still reference `{{ Phone }}`.
- [x] "When" reads "Today" for a one-time gift; for a recurring frequency it reads e.g. "Monthly starting on {date}" (and "... and ending on {date}" when an end date is set).
- [x] The Address is formatted the Rock way, via the Location's country-aware address format: a second street line shows when entered, and a non-US country renders in that country's layout (not a hardcoded US single line).
- [x] The account rows use the campus-mapped account name (Campus Account Mapping on + a campus with a child account), so the summary name matches what will be saved.
- [x] A customized Confirmation Body template resolves its Lava (e.g. `{{ Total | FormatAsCurrency }}`, a `{% for accountDetail in AccountDetails %}` loop).
- [x] The Confirmation Footer Lava renders below the summary (default agreement text); a blank footer renders nothing.
- [x] Section heading "Confirm Information" (Confirmation Section Heading) shows when Show Panel & Section Headings is on and is suppressed when off.
- [x] The confirmation renders its Lava (header/body/footer) directly, not inside a ContentSection card; the success step does the same, matching the Figma (the Text-to-Give success has no section wrapper).
- [x] Renders correctly on the external giving theme, not just internal.
- [x] The updated (Figma) confirmation Lava templates (Header / Body / Footer) are implemented, replacing the interim legacy-faithful defaults. Each summary is a `.table` (label left, value right via `text-right`, rows split by the table's dividers, bold Total as the last row) wrapped in a `<div class='panel panel-default shadow-none'>` for the Figma's bordered, rounded, shadowless card. All baked-in classes except one block rule muting the label column (the bold Total row excluded).
- [x] The panel card, table, and muted labels render correctly on the external giving theme, not just internal. The `.panel` / `.table` component styles reach both themes (confirmed: the entry-flow ContentSection cards already render externally). Labels are muted via `--color-interface-medium` in the block's one custom rule; the bold Total is excluded.
- [x] Payment Method and Account Number remain omitted from the confirmation (the gateway returns only a token pre-charge, so neither is known then), tracked under the [Payment Method / Account Number question](https://app.asana.com/1/20866866924293/task/1216405822150737).

Navigation:
- [x] Previous returns to the entry step with the typed amounts and name/email/address intact (entry stays mounted). The payment form re-initializes fresh: hosted-field gateways tear their tokenizer down after tokenizing and do not survive a hide/show, so the gateway control remounts on return. Verify the card/ACH form shows again (no infinite spinner) on both the Test Gateway and a hosted gateway.
- [x] After Previous, changing the amount and clicking Next again re-tokenizes and shows an updated summary.
- [x] After Previous with CAPTCHA enabled, the CAPTCHA resets and the action button hides until it is re-solved (its token was consumed validating the first Next); re-solving then allows Next again, and the fresh token validates on the server.
- [x] Finish charges the reviewed gift and shows the Success step (the full one-time money path in item 9 still applies).
- [x] A gateway/charge error on Finish shows a danger message on the confirmation step and re-enables Finish (does not bounce to Entry or spin forever).
- [x] Every control is disabled while Next is tokenizing and while Finish is charging.

Parity flags:
- [x] Payment Method and Account Number are intentionally omitted from the summary; the shared gateway control surfaces only a token pre-charge ([Payment Method / Account Number question](https://app.asana.com/1/20866866924293/task/1216405822150737)).
- [x] No Fee Coverage row: this block has no fee-coverage feature, so the Figma's generic Fee Coverage row does not apply ([Fee Coverage question](https://app.asana.com/1/20866866924293/task/1216405822150740)).
- [x] No transfer-specific wording appears in the confirmation summary: the "When" row shows the frequency and dates the giver entered (e.g. "Monthly starting on {date}"), the same as any scheduled gift. Legacy has no separate "Next Gift" summary text; the only transfer-specific label is the entry start date. See [Transfer flow](#13-transfer-flow-move-a-scheduled-gift-to-another-gateway).

### 7. Success step: Save Payment Method and Text-to-Give
Setup: assign the Test Gateway (its saved accounts support saving); Show Confirmation Step off; sign in as an individual with a payment method to enter, and separately as a not-signed-in giver so the create-login fields appear. The base success page (finish Lava, footer, confirmation code) is exercised under [item 9](#9-transaction-processing-one-time-gift-entry-to-charge-to-success).

Updated Lava template:
- [x] The updated (Figma) success Lava template is implemented, replacing the interim legacy-faithful default. It renders the same two summary cards (a `.table` inside a `.panel panel-default shadow-none`), shows the confirmation code as a `label label-info` badge in a "Confirmation" row, and shows a green `alert alert-success` below the summary.
- [x] Both success variants show the green alert box; only the message differs. The two messages are assigned to a `successMessage` Lava variable from the new `IsTextToGive` merge field at the top of the template, so an admin can customize each without touching the markup. Normal mode reads "submitted successfully"; Text-to-Give mode reads the "text 'give' followed by the dollar amount" wording. The `.obs` stays free of Text-to-Give logic.
- [x] Payment Method and Account Number show (both known post-charge, resolved via the eager-load fix noted under item 9); the guarded Fee Coverage row renders nothing for this block, and the `{{ Transaction.TotalFeeCoverageAmount }}` merge field is preserved for customized templates (must-preserve behavior #5).
- [x] The success summary cards and the confirmation-code badge render correctly on the external giving theme, not just internal.

Save Payment Method offer (personal gift, new payment method):
- [x] After a successful personal gift entered with a new payment method, the Save Payment Method section renders below the success content (icon + heading + description from the settings), with the shared save-account form inside it. It respects the Show Panel & Section Headings toggle: when off, no section heading shows (the shared control's own header is suppressed, so the section provides the only heading).
- [x] The offer does NOT appear for a business gift, when a saved account was used to give, in Text-to-Give mode, or on a gateway that cannot save the payment method.
- [x] The "Save as New Account" checkbox reveals the account-name field and the Save Account button; left unchecked, nothing further shows.
- [x] The account name is required (the form will not submit blank).
- [x] Saving a valid name creates a saved account for the giver (visible under their saved accounts) whose masked number, currency / card type, expiration, and billing location match the gift, and the form is replaced with "The account has been saved for future use."
- [x] A signed-in individual sees no username / password fields, and saving does not create a second login.
- [x] Under impersonation, the saved account is created for the target individual (the endpoint resolves the transaction's authorized person), and no create-login fields show (the admin is signed in).

Create-login and Passwordless (issue #6877):
- [x] A not-signed-in giver sees the username / password / confirm fields and the security note. A taken username, an invalid password (the friendly password rules), or a mismatched confirm are each rejected; a valid login is created, the Account Confirmation communication is sent, and the account saves.
- [x] On a Passwordless-only site (Database authentication inactive), a not-signed-in giver is NOT offered the save (the section is hidden), matching Registration Entry's fix for issue #6877; the giver is never shown a login they could not use.

Text-to-Give success variant (Text-to-Give Mode on):
- [x] A completed Text-to-Give gift shows NO Save Payment Method offer (it saves automatically).
- [x] With a new payment method, a "Text-To-Give ..." saved account is created for the target individual and Text-to-Give is configured (the individual can then give by text).
- [x] With a saved account selected, no duplicate saved account is created and Text-to-Give is pointed at the selected account.
- [x] Renders correctly on the external giving theme, not just internal.

### 8. Other gateway states (ACH/CC off, unsupported, Test)
- [x] Test Gateway configured: the "You are using the Test Financial Gateway..." warning shows at the top of the entry flow, and the entry flow still renders below it.
- [x] A non-Test gateway: the notice does not show.
- [x] Both ACH and Credit Card disabled: the warning "Configuration" / "Enable ACH and/or Enable Credit Card needs to be enabled." shows in place of the entry flow, with no campus/contribution/contact/payment sections, no saved-account list, no gateway control, and no footer action buttons. (This also removes the endless gateway-control spinner and the all-saved-accounts leak that both appeared when the payment section still rendered with both currency types off.)
- [x] A non-hosted (unsupported) gateway: the warning "Unsupported Gateway" / "This block only supports Gateways that have a hosted payment interface." shows in place of the entry flow.
- [x] When a gateway is both non-hosted and has both currency types disabled, the "Configuration" warning wins (both-disabled is checked first, matching legacy order).

### 9. Transaction processing: one-time gift (Entry to charge to Success)
Covers the one-time money path (`ProcessTransaction`), campus/account mapping at save, the double-charge guard, and the finish-template Lava.

**Setup.** Sign in as a person who has a primary alias. Assign the Test Gateway. Turn **Show Confirmation Step OFF** so Entry goes straight to processing. In the Test Gateway card field, **11+ digits** tokenize successfully, **10 or fewer digits** raise a validation error, and **`0000`** simulates a gateway error. Configure a Transaction Source, Transaction Type, and Batch Name Prefix to confirm they land; optionally assign a Receipt Email.

One-time gift, happy path:
- [x] Enter an amount for one account and click Give. The button shows its spinner, the gateway tokenizes, and the Success page renders the finish Lava.
- [x] A `FinancialTransaction` is created for the entered total, authorized to the signed-in person's primary alias, with the configured Transaction Type, Transaction Source, and gateway.
- [x] The transaction is added to a batch whose name uses the Batch Name Prefix, and the batch control amount increases by the gift total.
- [x] The Success page shows the confirmation code and the account/amount summary (finish Lava resolves `Transaction`, `Person`, `PaymentDetail`, and `BillingLocation`).
- [x] Account names and payment method resolve on the success page (not blank), on both the one-time and scheduled success pages.
- [x] Multi-account: enter amounts for two or more accounts; each becomes its own transaction detail with the correct account and amount, and the total equals the sum.
- [x] The billing name sent to the gateway is the signed-in person's name, not whatever is typed in the Contact name fields.
- [x] Prompt for Phone on with a phone entered: the phone sent to the gateway is country-code formatted (via `PhoneNumber.FormattedNumber`), not the raw typed value.

Success page:
- [x] The Success Page Footer, when configured, renders below the success content and resolves the same merge fields as the Success Page Template (e.g. `{{ Transaction.TransactionCode }}` or `{{ Person.NickName }}`).
- [x] The Success Page Footer left blank renders nothing below the success content (no empty element).

Disable while processing:
- [x] After clicking Give, every entry control stays disabled until the charge finishes: campus, the amount picker and its amount box(es), Add Another Account, frequency, dates, comment, first/last name, address, email, phone, and both checkboxes.
- [x] The amount box specifically cannot be changed mid-charge (verifies the new `disabled` prop on `CampusAccountAmountPicker`).
- [x] On a gateway error (`0000`), the controls re-enable and the error shows; the form is not left stuck-disabled.
- [x] On an invalid / incomplete card (10 or fewer digits, which raises a validation event rather than an error): the controls re-enable and the invalid field is surfaced; the Give button does not spin forever.

Campus / account mapping (item 9):
- [x] Campus Account Mapping ON, a campus selected, and an account that has a campus-specific child: the transaction detail is filed under the campus child account.
- [x] Campus Account Mapping ON and the account is NOT flagged to use campus child accounts but a matching child exists: still filed under the child (force-child parity with the legacy control).
- [x] An account flagged to use campus child accounts: mapped to the child regardless of the block setting.
- [x] Mapping OFF and the account not flagged: filed under the parent account.
- [x] No campus selected: filed under the parent account.

Transaction entity on details:
- [x] Transaction Entity Type set and the Entity ID Parameter naming a URL parameter that holds a valid entity key: each transaction detail's Entity Type and Entity Id point to that entity.
- [x] An integer entity id in the URL resolves when the site allows predictable ids; with Disable Predictable Ids on it does not resolve by integer (IdKey or Guid still work).
- [x] No Transaction Entity Type configured: details carry no entity (ordinary gift).
- [x] A Success Page Template referencing `{{ TransactionEntity }}` resolves to the configured entity (e.g. its name), not blank.

Comment composition:
- [x] Allow Comment Entry ON, a Payment Comment Template set, and a typed comment: the saved transaction summary reads "{resolved template}: {typed comment}".
- [x] Allow Comment Entry OFF: the summary is the resolved template only.
- [x] A template that references `{{ TransactionAccountDetails }}` (iterated in a `{% for %}`) or `{{ TransactionDateTime }}` resolves those; per-account rows show the mapped account's name.

Anonymous giving:
- [x] Give Anonymously checked: the transaction's Show As Anonymous is true and it appears as Anonymous on public-facing contribution lists.
- [x] Server-side guard: with Allow Anonymous Giving off, a submitted anonymous flag is ignored and Show As Anonymous saves false (the setting is re-checked server-side, not trusted from the request).

Receipt email:
- [x] Receipt Email configured: a receipt is queued and sent after a successful gift.
- [x] Receipt Email blank: no receipt is attempted and no error occurs.

### 10. Possible-duplicate warning (reachable trigger)
- [x] Double-clicking Give, or a network retry of the submit, does not create two transactions; only one `FinancialTransaction` results.
- [x] Reloading the page mints a fresh idempotency Guid, so a genuinely new gift after a completed one still processes.
- [x] The possible-duplicate warning ("Yes, Submit Anyway") is built, though its trigger is dormant in the current single-pass flow (no path returns to Finish after a completed charge).
- [x] Not applicable (resolved 2026-07-20). The double-charge failure mode the warning guarded is structurally prevented by the idempotency Guid guard plus the single-pass Vue flow: there is no in-session re-submit path (unlike legacy's ViewState-persisted `TransactionCode`), and a reload mints a fresh Guid. Must-preserve behavior #2 is satisfied by the guard, not the warning, so no reachable trigger is added. The warning UI is left dormant. No giver- or admin-visible change (a giver could never reach the old warning without the in-session re-submit the new flow prevents), so this is an engineering determination, not a PO decision.

### 11. Scheduled / recurring gifts (save path)
Setup: assign a gateway that supports recurring schedules (the Test Gateway works); turn ON Allow
Scheduled Gifts and Allow Scheduled End Date; Show Confirmation Step off (straight to processing) unless a
row says otherwise; sign in as a person with a primary alias.

Immediate vs scheduled branch:
- [x] One-Time frequency with today's start date: charges immediately, creating a `FinancialTransaction` (regression check of the one-time path); no schedule is created.
- [x] One-Time frequency with a future start date: creates a `FinancialScheduledTransaction` (One-Time frequency) starting on that date; no immediate charge.
- [x] A recurring frequency (e.g. Monthly): creates a `FinancialScheduledTransaction` with that frequency and start date; the end date is saved when Allow Scheduled End Date is on and one was entered.
- [x] Earliest start date: selecting a recurring frequency holds the start date forward to the gateway's earliest scheduled date (e.g. the Test Gateway's tomorrow), and the saved schedule's `StartDate` is never today-or-earlier. This is required for real hosted gateways (e.g. MyWell), which reject a subscription that starts before their earliest allowed date; the block clamps server-side and the picker holds the shown date forward so it matches the saved schedule.
- [x] A scheduled gift creates no batch and queues no receipt email (unlike a one-time gift).

Details, mapping, and authorization:
- [x] Multi-account recurring: each entered account/amount becomes a `ScheduledTransactionDetail` with the correct account and amount, and the total equals the sum.
- [x] Campus Account Mapping ON, a campus selected, and an account with a campus-specific child: the scheduled detail is filed under the campus child account (same mapping as the one-time item 9).
- [x] Personal recurring gift: the schedule's `PersonId` and the scheduled transaction's authorized alias are both the signed-in individual.
- [x] Business recurring gift: the schedule's `PersonId` is the contact individual while the scheduled transaction's authorized alias is the business (legacy split preserved).
- [x] Under impersonation (`rckid` + Staff Impersonation): a recurring gift's schedule is owned by and authorized to the target individual (business gift: authorized to the business, schedule owned by the target individual as contact).

Idempotency, success, and mode gating:
- [x] Double-submit or a retry of a scheduled gift does not create two schedules; the shared idempotency Guid short-circuits to success. Reloading the page mints a fresh Guid so a genuinely new schedule still saves.
- [x] The success page renders the finish Lava for a scheduled gift: the `FinancialScheduledTransaction` is the `Transaction` merge field, so `Transaction.TransactionCode` and the account/amount summary resolve.
- [x] The manual Save Payment Method offer appears on the scheduled success step (personal gift, new payment method, savable gateway), the same as a one-time gift; saving a name creates a `FinancialPersonSavedAccount` for the giver from the scheduled transaction's payment detail. See [Save Payment Method on a scheduled gift](#save-payment-method-on-a-scheduled-gift-parity-restored).
- [x] Smoke test (MyWell): the saved account created from a scheduled gift is chargeable later (e.g. give with it from another block), confirming the vault-reference reuse holds on a real gateway.
- [x] Text-to-Give Mode on: the frequency/date fields do not render and no schedule is created even with Allow Scheduled Gifts on (the server forces an immediate gift in that mode); Text-to-Give still auto-saves the account.
- [x] Every entry control stays disabled while the scheduled gift is saving, and a gateway error on scheduling re-enables the controls and shows the error (no stuck spinner), matching the one-time path.

### 12. Show Initial Back Button
Setup: place the block on a page reachable by a link from another page (so a referrer exists); toggle Show Initial Back Button.
- [x] Show Initial Back Button ON with a known referrer: a "Previous" link renders on the entry step footer (left) and navigates to the page the individual came from.
- [x] The back action appears on the entry step only, never on the confirmation or success steps (confirmation's own Previous still returns to entry).
- [x] Show Initial Back Button OFF (default): no back action renders on the entry step.
- [x] No referrer available (direct navigation): the back action does not render even when the setting is on.

### 13. Transfer flow (move a scheduled gift to another gateway)
Moves a scheduled gift from its current gateway to this block's gateway: it re-creates the schedule here and cancels the old one. The giver reaches it from the `ScheduledTransactionListLiquid` block's Transfer-to-Gateway action, which links here with `?Transfer=true&ScheduledTransactionGuid={guid}`. Setup: configure this block's gateway to differ from the scheduled gift's current gateway; sign in as a person who owns a scheduled gift; open the block with those params (or click Transfer to Gateway in `ScheduledTransactionListLiquid`).

- [x] Reached via the `ScheduledTransactionListLiquid` block's Transfer-to-Gateway action: it links to this block with the `Transfer` and `ScheduledTransactionGuid` params set.
- [x] The frequency pre-selects the schedule's frequency and the start date pre-fills its next payment date (tomorrow when it has none); the start-date label reads "Next Gift".
- [x] The account amounts are NOT pre-filled (the giver re-enters them). This matches legacy, which seeds only the frequency, date, and business, not the amounts. Seeding them would require rehydrating the shared `campusAccountAmountPicker.obs`, which is out of scope for this block.
- [x] Transferring a personal scheduled gift: Give As Business stays off; the new schedule is authorized to the individual.
- [x] Transferring a business scheduled gift (Allow Business Giving on): Give As Business starts on with the schedule's business preselected and its contact fields prefilled; the new schedule is authorized to that business.
- [x] Transferring a business scheduled gift with Allow Business Giving OFF: the business section does not show and the new schedule is authorized to the individual (a personal gift). Matches legacy, whose give-as visibility and save path both gate business giving on the setting.
- [x] On Finish: a new `FinancialScheduledTransaction` is created on this block's gateway and the old one (on the source gateway) is cancelled and no longer appears in the giver's scheduled gifts.
- [x] Switching the frequency to One-Time (immediate) during a transfer charges now and does NOT cancel the old schedule (matches legacy, which cancels only on the scheduled save path).
- [x] Authorization: a `ScheduledTransactionGuid` for a schedule the signed-in / impersonated person does not own (not theirs and not one of their businesses') is ignored; the flow renders as a normal new gift with no seeding and cancels nothing. Re-checked server-side at save.
- [x] A missing or unresolvable `ScheduledTransactionGuid`, or `Transfer` absent, renders a normal new gift.

### 14. Layout Style (Vertical / Fluid)
Setup: assign the Test Gateway; have at least two selectable campuses (so the campus picker shows), a couple of accounts, and Contact + Payment sections enabled. Toggle the block's **Layout Style** between Vertical (default) and Fluid. Test both light and dark themes, on the external giving theme as well as internal, and at desktop and narrow widths.
- [x] Vertical (default): unchanged from before, the sections stack in one column and Campus renders as its own light card above Contribution (regression check).
- [x] Fluid, desktop: two columns, left = Contribution then Payment, right = Contact. The Transaction Header banner and the Test Gateway notice span the full width above the columns.
- [x] Fluid, campus fold-in: the campus picker renders at the top of the Contribution card (not a separate card). It still auto-hides when only one campus is selectable and honors Prompt for Campus When Known, exactly as Vertical.
- [x] Fluid, spacing: the gap between the two columns and between Contribution and Payment matches the section gap (no doubled or missing gaps); no wrapper artifacts.
- [x] Fluid, responsive: at the medium breakpoint and below the columns collapse to a single stacked column (Contribution with its campus, then Contact, then Payment).
- [x] Fluid renders correctly in both light and dark themes and on the external giving theme, not just internal.
- [x] The action buttons stay in the full-width footer (right-aligned), unchanged by layout.
- [x] Changing the Layout Style setting flips the layout on reload.
- [x] Section header titles and descriptions truncate to a single line with an ellipsis when too long (both layouts), rather than wrapping to multiple lines.

### 15. Transaction Header merge fields
The enriched merge-field set (`TransactionEntity`, `TransactionEntityTransactions`, `TransactionEntityTransactionsTotal`, `AmountLimit`, and the fundraising `FundraisingGoal` / `AmountRaised`) feeds all four Lava spots: the Transaction Header banner, Confirmation Header, Confirmation Footer, and Success Page Footer.

**Setup.** Assign the Test Gateway. Paste this diagnostic block into each of the four settings, changing `[SPOT]` so you can tell which one rendered:

```lava
<div style="border:1px dashed #999;padding:6px;margin:4px 0;font-family:monospace;font-size:12px;">
  <strong>[SPOT]</strong><br>
  AmountLimit: {{ AmountLimit }}<br>
  TransactionEntity: {{ TransactionEntity }} (Id {{ TransactionEntity.Id }})<br>
  EntityTxnTotal: {{ TransactionEntityTransactionsTotal }}<br>
  FundraisingGoal: {{ FundraisingGoal }}<br>
  AmountRaised: {{ AmountRaised }}<br>
  FinancialTransaction: {{ FinancialTransaction }} (Id {{ FinancialTransaction.Id }})<br>
</div>
```

| Block setting | `[SPOT]` |
|---|---|
| Transaction Header Template | `HEADER` |
| Confirmation Header | `CONF-HEADER` |
| Confirmation Footer | `CONF-FOOTER` |
| Success Page Footer | `SUCCESS-FOOTER` |

For the fundraising rows: make a **Fundraising Opportunity** group, set the group's **Individual Fundraising Goal** to `500` (members inherit it via fallback), add the test giver as a member, and note that GroupMember's Id. On the block set **Transaction Entity Type = Group Member** and **Entity ID Parameter = `GroupMemberId`**, then load with `?GroupMemberId={id}`.

Each row lists what the diagnostic block should show. "All four" means the HEADER, CONF-HEADER, CONF-FOOTER, and SUCCESS-FOOTER boxes as you walk entry, confirmation, and success.

- [x] Bare load (no URL params, no Transaction Entity Type): every value is blank in all four boxes and no error, `AmountLimit:`, `TransactionEntity:`, `EntityTxnTotal:`, `FundraisingGoal:`, `AmountRaised:` all empty.
- [x] `AmountLimit`: add `?AmountLimit=100` and reload, all four show `AmountLimit: 100`; remove the param, all four show `AmountLimit:` (blank).
- [x] `TransactionEntity`: with the entity type + param set and `?GroupMemberId={id}`, all four show `TransactionEntity:` with the member and its `(Id N)`, and `EntityTxnTotal:` shows the total filed against that member (`0` with no prior gifts). With no Transaction Entity Type configured, both are blank.
- [x] Fundraising (Individual), no `ParticipationMode` param (or `1`): all four show `FundraisingGoal: 500` (the group's inherited goal, or the member's own if set) and `AmountRaised:` = that member's contribution total (`0` until a gift is filed against them, then their total).
- [x] Fundraising (Family), `?ParticipationMode=2`: `FundraisingGoal:` is the goal summed across the giver's family members in that group (e.g. two members at 500 shows `1000`) and `AmountRaised:` is their combined contributions.
- [x] Non-GroupMember entity type (e.g. Person): `FundraisingGoal:` and `AmountRaised:` are blank in all four, while `TransactionEntity:` still shows the entity.
- [x] `FinancialTransaction` timing: blank on `HEADER`, `CONF-HEADER`, and `CONF-FOOTER` (no transaction pre-charge). On `SUCCESS-FOOTER` after a completed one-time gift it shows the transaction and its `(Id N)`; for a scheduled gift it stays blank on `SUCCESS-FOOTER` (legacy had no FinancialTransaction for a schedule).
- [x] All four render correctly on the external giving theme, not just internal.
- [x] Parity note: `FinancialTransaction` being blank on the entry banner and confirmation is expected. Obsidian resolves the banner once at entry (there is no in-flight transaction yet, unlike legacy's ViewState-persisted guid), so it only populates on the completed-gift success footer, matching what legacy rendered on its initial load.

### 16. URL account options
- [x] `?AccountIds=1,2,3`: exactly those accounts show, ordered by `Account.Order`, replacing the configured `Accounts to Display`.
- [x] `?AccountGlCodes=40100,40110`: accounts resolved by GL code show (a GL code is treated as unique).
- [x] Preset amount (`?AccountIds=1^50`): the amount box is seeded with 50 and stays editable.
- [x] Locked amount (`?AccountIds=1^50^false`): the amount is seeded and the box is disabled; `true` or omitted leaves it editable.
- [x] Multi mode seeds and locks each box independently. Single mode seeds and locks the selected account; on switching accounts, a read-only preset re-locks and forces its amount, while an editable preset fills only an empty box (a value already entered is kept).
- [x] `Restrict URL Accounts to Public Only` ON (default): a private account named in the URL does not show and triggers the Invalid Account Message. OFF: it shows (resolved server-side and injected, so it renders even though the shared accounts endpoint would not return a private account).
- [x] Invalid Account Message fires for any specified account that does not resolve or is not active, in date, and public-when-restricted: a bad id, an unresolvable GL code, an inactive account, an out-of-date account, and a private account when restricted. A blank message shows nothing.
- [x] Invalid or unresolvable accounts are left out of the picker and the presets (no phantom account, no orphan preset); the message is the only signal.
- [x] Consistency: an inactive / out-of-date / private account triggers the message the same way whether it is named by id or by GL code.
- [x] Campus context filter still applies: a URL account whose campus the context filter excludes is dropped, exactly like a configured account.
- [x] Account labels use the Account Label Template on every list (configured, URL, and Add Another Account), not just `PublicName`. A custom template such as `{{ Account.PublicName }} ({{ Account.GlCode }})` shows on all of them.
- [x] Add Another Account excludes accounts already shown from the URL; adding one appends it (labeled via the template) with no preset.

Save-path re-enforcement (crafted-request guard; exercise with a tampered request body, e.g. browser dev tools or a REST client, since the UI cannot produce these):
- [x] Locked amount: submit a `GetConfirmation` / `ProcessTransaction` request that changes a locked account's amount (`?AccountIds=1^50^false`, then post `75`). The confirmation summary and the charge both use `50`, not `75`.
- [x] Disallowed account (public-only): with `Restrict URL Accounts to Public Only` ON and a URL naming only public accounts, post an amount against a private account's Guid. Submit is rejected with the "not available" error; no charge.
- [x] Disallowed account (not offered): post an amount against any account Guid not in the URL list or the Add Another Account pool. Same rejection.
- [x] Add Another Account still works: an account legitimately added from the addable pool is accepted (it is in the allowed set), confirming the guard does not over-reject.
- [x] No URL options: the guard is inert. A normal configured-account gift (no `AccountIds` / `AccountGlCodes`) submits unchanged.

### 17. Account Campus Context Filter
Setup: put a campus context on the page (a Campus Context Setter block, or a page campus context). Configure a subset of accounts in Accounts to Display, mixing accounts tied to a campus with accounts that have no campus; turn on Allow Additional Accounts with the same campus mix in the addable pool.
- [x] Filter off (`-1`, default): every configured account and every addable account shows, regardless of campus context.
- [x] No campus context on the page: no filtering applies even when the setting is `0` or `1` (every account shows).
- [x] Mode `0` (Only Accounts with Current Campus Context): only accounts whose campus matches the context show, in both the configured list and the Add Another Account pool; no-campus accounts are hidden.
- [x] Mode `1` (Accounts with No Campus and Current Campus Context): accounts on the context campus plus accounts with no campus show; accounts on a different campus are hidden.
- [x] Changing the page's campus context changes which accounts appear accordingly, in both the configured list and the addable pool.

### 18. Account Confirmation Email setting
The `ConfirmAccountTemplate` setting (Account Confirmation Email) now flows to the shared `SaveFinancialAccountFormSaveAccount` endpoint through the save-account control, and the endpoint supplies the previously-missing `ConfirmAccountUrl` merge field. Setup: make a custom system communication (copy of Confirm Account, with distinguishable wording that references `{{ ConfirmAccountUrl }}`) and set the block's Account Confirmation Email to it; use a savable gateway (Test Gateway); Show Confirmation Step off; give as a **not-signed-in** individual so the create-login path runs (check Save as New Account and enter a username/password).
- [x] The confirmation email an anonymous saver receives is built from the configured Account Confirmation Email system communication (the custom wording), confirming the setting is honored end to end.
- [x] With the setting left at its default, the built-in Confirm Account communication is used (fallback path).
- [x] The `{{ ConfirmAccountUrl }}` merge field resolves to a working absolute URL ending in `/ConfirmAccount` (it rendered blank before), and the confirm link works.
- [x] Regression (shared endpoint): Registration Entry still sends its confirm-account email using the default communication, since it passes no template (additive parameter, no behavior change for existing callers).

### 19. Page-parameter ID hardening
Setup: note a campus, a financial account, and a transaction-frequency DefinedValue in all three key forms (Id, IdKey, Guid). Test with the site's Disable Predictable Ids OFF (default) and then ON (Admin Tools > CMS Configuration > Sites > [site] > Advanced).
- [x] `CampusId` accepts an IdKey and a Guid: `?CampusId=<idkey>` and `?CampusId=<guid>` pre-select the campus the same as `?CampusId=<int>`.
- [x] `AccountIds` accepts an IdKey and a Guid per entry: `?AccountIds=<idkey>^50` and `?AccountIds=<guid>^50` resolve the account and seed the preset the same as `?AccountIds=<int>^50`.
- [x] `Frequency` accepts an IdKey and a Guid: `?Frequency=<guid>^false` pre-selects and locks the frequency the same as `?Frequency=<int>^false`.
- [x] Disable Predictable Ids ON: the integer form of `CampusId`, `AccountIds`, and `Frequency` no longer resolves (campus not pre-selected; the URL account is dropped to the Invalid Account Message; frequency not pre-selected), while the IdKey and Guid forms still work.
- [x] Regression, Disable Predictable Ids OFF (default): integer ids keep resolving for all three, unchanged from before.
- [x] `AccountGlCodes` is unaffected (a GL code is not an entity id): it still resolves by GL code regardless of the Disable Predictable Ids setting.
- [x] Regression: the Transaction Entity id parameter (item 9) still resolves by Id / IdKey / Guid and honors Disable Predictable Ids (refactored onto the shared helper, no behavior change).

### 20. Transaction attributes from URL
Setup: add one or more attributes to the Financial Transaction entity and select a subset in the block's Transaction Attributes from URL setting. Use the Test Gateway and give a one-time (immediate) gift.
- [x] An allow-listed attribute is set from its `Attribute_<Key>` page parameter: give with `?Attribute_<Key>=hello`, open the resulting transaction, and confirm the attribute value is `hello`.
- [x] A transaction attribute NOT in the allow-list is ignored even when its `Attribute_<Key>` param is present (value stays at default).
- [x] Multiple allow-listed attributes are set independently from multiple `Attribute_` params in one URL.
- [x] No `Attribute_` params present: allow-listed attributes stay at their default; no error.
- [x] Allow-list empty (default): the feature is inert; a gift with `Attribute_` params in the URL sets nothing and does not error.
- [x] Scope note (parity): attributes are applied only to the immediate one-time transaction, matching legacy; a scheduled / recurring gift's `FinancialScheduledTransaction` does not receive them (legacy applied URL attributes only in its one-time `SaveTransaction`).

### Target-person resolution and impersonation (rckid)
Setup: create a person impersonation token for another individual (a `PersonToken` with the `transaction`
usage) and append it to the URL as `?rckid={token}`. Test as an admin who is NOT that individual.

- [x] No `rckid`, signed in: the gift is for the signed-in individual; the default campus is their family campus (unchanged baseline).
- [x] `rckid` + Staff Impersonation ON: the entry flow renders; the default campus is the target individual's campus; a charged gift's authorized person alias is the target individual (not the admin), and the Success `{{ Person }}` merge field is the target individual.
- [x] `rckid` + Text-to-Give Mode ON, Staff Impersonation OFF: impersonation is still allowed (the mode forces it on); resolution behaves as the Staff Impersonation case above.
- [x] `rckid` + impersonation NOT allowed (both off) and the token resolves to a different individual: the danger `NotificationBox` shows "Impersonation is not allowed on this block." and the entry flow is hidden (only the warning shows).
- [x] `rckid` + impersonation allowed but the token is invalid or expired: the danger `NotificationBox` shows "Invalid or Expired Person Token specified"; the flow is hidden; no server error / crash (the null check precedes the campus pre-load).
- [x] `rckid` resolving to the SAME individual as the one signed in, impersonation OFF: the flow renders normally with no warning.
- [x] Server-side enforcement: a crafted ProcessTransaction carrying a disallowed `rckid` is rejected with the warning and charges nothing; GetConfirmation likewise returns the warning rather than a summary.

### Individual person creation and contact write-back
Setup: assign the Test Gateway, Show Confirmation Step off. Test three ways: signed in; via `rckid`
impersonation; and not signed in (anonymous). Configure Connection Status / Record Status / Record Source
(New People) to confirm they land on created individuals.

- [x] Signed-in giver: the gift is authorized to that individual; no new person is created; their email / phone / address on file are updated from the entry form.
- [x] Not-signed-in giver, no match: entering first / last / email / phone / address creates a new individual (Record Type Person, with the configured connection status, record status, and record source), and the gift is authorized to them.
- [x] Not-signed-in giver matching an existing individual (same first / last / email, and phone): the gift is authorized to that existing individual rather than creating a duplicate (`FindPerson`).
- [x] Nameless placeholder (e.g. a Give-by-SMS record resolved via `rckid`): a real individual is created from the entered name and the nameless record is merged into it.
- [x] Name required: with name entry shown (new / nameless / anonymous), submitting without a first and last name is rejected ("Make sure to enter both a first and last name") at Next (confirmation on) and at Give (confirmation off); a signed-in named giver is never asked for a name.
- [x] Contact write-back: the entered email and phone (home number, created or updated; a matching mobile is reused rather than duplicated) and address (configured Address Type, home default) are saved to the individual / their family; SMS opt-in is saved only when the SMS opt-in choice was shown.
- [x] Record source: a `RecordSource` page parameter or session record source overrides the Record Source setting on a created individual.

Submit validation (matches legacy `ValidatePaymentInfo`; validated at both Next with a confirmation step and Give without one, and pre-tokenize via the `Validate` action so every problem surfaces together in one validation summary before the payment token is consumed):
- [x] Amount: no positive amount is rejected ("Please specify an amount").
- [x] Amount limit: with an `AmountLimit` URL parameter, a total over the limit is rejected ("The maximum amount is limited to ...").
- [x] Name (only when name entry is shown): first and last required; names containing special characters (quotes, parentheses, brackets, braces) or emoji / special fonts are rejected with the legacy messages.
- [x] Address: submitting without a street address is rejected, always, regardless of any setting.
- [x] Phone: required when the phone field is shown; not required when it is hidden (phone off, or the individual's best number is unlisted).
- [x] Email: required when Prompt for Email is on.
- [x] Scheduled end date (recurring gift): an end date before the schedule's start is rejected with "When scheduling a repeating payment, the minimum end date is {start}." The start is held forward to the gateway's earliest scheduled date, so the end is validated against that clamped start, not the entered start; an end after today but before the clamped start is still caught. The client bumps the end up to the start as it is edited, so the server check is the backstop. (No separate "minimum start date" error: the start is clamped forward instead, per item 11's Earliest start date, and the picker disallows past dates.)

### Business giving (Give As Business)
Setup: turn ON Enable Business Giving; assign the Test Gateway; configure Connection Status / Record
Status / Record Source. Test three ways: signed in with one or more businesses on file, signed in with
none, and anonymous.

Give As switch and section:
- [x] The Give as Business switch shows only when Enable Business Giving is on and Text-to-Give mode is off; it defaults to off (Person).
- [x] Selecting Business relabels the Address / Phone / Email fields to "Business ..." and hides the person first/last name fields.
- [x] With Enable Business Giving off (or Text-to-Give on) the switch does not show and only the person fields render.

Business selection and prefill:
- [x] Signed-in giver with businesses on file: a business radio button list lists each business plus "Add New Business", defaulting to the first; selecting a business prefills its name, email, phone, and address; "Add New Business" clears them.
- [x] Signed-in giver with no businesses: no radio button list, just an empty business name field (a new business).
- [x] Anonymous giver: no radio button list.

Business-contact fields:
- [x] The business-contact fields (first, last, email, phone, SMS opt-in) show only when no one is signed in; they are hidden for a signed-in giver, including under impersonation.
- [x] When shown, they render in their own stack (with a divider above it) under a "Business Contact" heading, labeled First Name / Last Name / Email / Phone (no "Contact" prefix). The Give Anonymously checkbox stays in the main business stack, above the divider.

Validation (business mode):
- [x] Business name required ("Make sure to enter a Business Name.").
- [x] Business address (street), and business phone / email when prompted, are required just like the person path; the person first/last-name checks are not applied.
- [x] Anonymous only: business-contact first and last required ("... for Business Contact"), with the special-character and emoji checks, plus contact phone (when Prompt for Phone) and contact email (when Prompt for Email).
- [x] All business errors surface together in the one validation summary and turn the relevant fields red.

Save path:
- [x] New business ("Add New Business" selected, or no match): a Record Type Business person is created with the configured connection status, record status, and record source; the giver is added as its contact (`AddContactToBusiness`); and the gift is authorized to the business, not the individual.
- [x] Selected existing business: the gift is authorized to that business; no duplicate business is created.
- [x] Name match: with "Add New Business" selected, an entered business name that matches the giver's single business is used rather than creating a new one.
- [x] Business write-back: the entered email, phone (Work number), and address (Work location on the business's family) are saved to the business; the billing name sent to the gateway and the confirmation "Name" row are the business name.
- [x] Anonymous: the business contact is matched by the entered contact name / email (its primary email is not updated on a match) or created, and is linked to the business.
- [x] Impersonation (`rckid` + Staff Impersonation): the business is linked to the target individual, not the signed-in admin.

Parity flags (do not expect):
- [x] Business address / phone save under the Work type, not the configured Address Type.
- [x] Business Work-phone write-back can skip saving the number when the contact already has a mobile on file (legacy behavior, pending the [Business Work-phone write-back question](https://app.asana.com/1/20866866924293/task/1216405822150745)).

## Requirements

### Functional parity
- The Obsidian block MUST preserve the existing capabilities and flow of the WebForms block. No
  feature additions or removals beyond what is listed below.
- Any behavior change that an existing admin or giver would notice (a "moves cheese" item) MUST be
  called out for PO sign-off. The known set is tracked as the [Questions for PO](https://app.asana.com/1/20866866924293/task/1216360748852200) Asana subtasks.
- Backward compatibility MUST be preserved: attribute Keys are unchanged so existing block
  configurations carry over untouched. Page parameters remain PascalCase.

### Legacy behaviors that must be preserved (not shown in Figma)
These behaviors exist only in the WebForms code. The Figma refresh does not show them, so they are
easy to drop during the rewrite. None of them are feature changes; each is existing behavior the
Obsidian block MUST keep. The legacy `UtilityPaymentEntry.ascx.cs` is the ground truth for all five.

1. **Route each gift to the correct campus account (money).** With Campus Account Mapping on, each
   gift must be filed under the campus-specific child account, exactly as WebForms does today. This
   now happens in the C# save path; the corrected logic is in [Campus / account mapping moves to the
   server](#campus--account-mapping-moves-to-the-server). Getting it wrong files gifts under the
   wrong account.
2. **Prevent a double charge (money).** Besides the visible "possible duplicate" warning, the block
   assigns the transaction a Guid up front and, before charging the gateway, checks whether a
   transaction with that Guid already exists; if it does, it shows Success without charging again.
   This guards against double-clicks, retries, and dropped connections. Keep both the up-front Guid
   check and the visible warning (`ProcessTransaction` in the legacy code).
3. **Treat "Text-to-Give" as a mode, not a checkbox.** Turning it on also forces impersonation on
   (this overrides the Staff Impersonation setting and is a security-relevant choice), hides
   scheduling, disables business giving, relabels the action button to "Give", and on success
   auto-creates a saved account via `PersonService.ConfigureTextToGive` instead of showing the manual
   "save this payment method" UI.
4. **Never re-render the payment form mid-flow.** The hosted gateway iframe and the CAPTCHA both lose
   the giver's typed card data if the payment control re-renders after entry. Legacy avoids this on
   purpose: the gateway control is mounted once and tokenized only at submit, and the CAPTCHA is
   validated at submit time (solving it only reveals the button; it does not trigger a round-trip).
   In Obsidian, use the shared `gatewayControl.obs` abstraction the way `RegistrationEntry`'s
   `payment.partial.obs` does, and validate CAPTCHA at submit. Obsidian caveat (discovered in testing):
   hosted-field gateways tear their tokenizer down after tokenizing and
   do NOT re-initialize on a `display` hide/show, so keeping the control mounted-but-hidden across the
   confirmation round-trip leaves it stuck on an infinite spinner. The block therefore remounts the
   gateway control when the giver returns to the entry step (via a `currentStep === 'entry'` guard),
   which re-initializes it cleanly. This does not violate the rule: the form is never re-rendered during
   active entry, only on a deliberate Back after the one-shot token was already captured.
5. **Keep customized Lava templates working.** Churches customize the confirmation, success, and
   footer templates. Preserving the setting Keys is not enough: the block must resolve those
   templates against the same Lava merge fields WebForms provides (Transaction, Person, PaymentDetail,
   billing location, transaction entity, fundraising goal and amount, and so on), and the default
   success template must keep showing fee coverage (`TotalFeeCoverageAmount`). Otherwise existing
   customized templates break silently.

### Save Payment Method on a scheduled gift (parity restored)

Legacy offers "Save Payment Method" on a recurring gift's success step, same as a one-time gift. This was
briefly dropped because the shared `SaveFinancialAccountFormSaveAccount` endpoint
(`Rock.Rest/v2/ControlsController.cs`) resolved the payment method only from a completed one-time
`FinancialTransaction` by code, which a schedule has none of until its first charge runs. It is now
restored: the shared endpoint (and its options bag) takes an optional `ScheduledTransactionGuid` and, when
set, resolves the `FinancialScheduledTransaction` instead, building the saved account from its payment
detail and authorized person. The block re-enables `IsSaveAccountOffered` for the scheduled path and passes
the scheduled transaction's Guid through the shared control. The change is additive (one-time callers, e.g.
Registration Entry, pass no Guid and are unchanged). Remaining validation: a real-gateway smoke test on
MyWell that the resulting saved account is chargeable later (the reuse key is the gateway customer
reference, captured identically for one-time and scheduled gifts).

### UI redesign (Figma is canonical)
- The block MUST match the Figma refresh: a card/panel layout with per-section headers (icon +
  title + description), a primary action button, and a fluid-layout option.
- The three flow steps (Entry, Confirmation, Success) MUST be supported for both external and
  internal audiences, including the variants captured in [UI Reference](#ui-reference).

### Block settings
- Setting labels and help text SHOULD adopt the polished copy in
  [Block Settings: copy polish](#block-settings-copy-polish). Attribute Keys MUST NOT change.
- Settings MUST be organized into the three tabs in
  [Block Settings: tab organization](#block-settings-tab-organization).
- The new settings in [Block Settings: new settings](#block-settings-new-settings) MUST be added.
- `Show Block Header Section` (BooleanField, default True) is part of the public refresh: the external
  design includes the header, the internal "Add Transaction" design does not. It MUST be shown on the
  external giving instances and hidden on the internal Add Transaction instance, set via a data
  migration that writes False on Add Transaction (targeted by its page/site, since upgraded instances
  have generated Guids). Affected instances in this deployment: Add Transaction (internal, OFF); Give
  Now, Text To Give Setup, and Fundraising Transaction Entry (external, ON).

### Campus / account mapping
- The Obsidian control MUST NOT carry the account/campus mapping logic. The selected account is sent
  to the C# save path, which performs the translation. See [Design](#design).

## Design

### File layout
| Artifact | Path |
|---|---|
| C# block | `Rock.Blocks/Finance/UtilityPaymentEntry.cs` |
| ViewModels / bags | `Rock.ViewModels/Blocks/Finance/UtilityPaymentEntry/` |
| Obsidian component | `Rock.JavaScript.Obsidian.Blocks/src/Finance/utilityPaymentEntry.obs` |
| Obsidian partials | `Rock.JavaScript.Obsidian.Blocks/src/Finance/UtilityPaymentEntry/` |

Classification: **Custom** interactive multi-step block (not a generated Detail/List). The actual
file generation and WebForms chop are handled later by `/convert-block`; this spec fixes the
requirements and the decisions below.

### Saved accounts and Save Payment Method reuse the shared controls
Both the saved-account selection (entry) and the Save Payment Method offer (success) reuse the shared
Obsidian controls that Registration Entry uses, rather than bespoke block code:
- Entry selection ships the target individual's accounts as `SavedFinancialAccountListItemBag`s (built by
  `FinancialPersonSavedAccountClientService.GetSavedFinancialAccountsForPersonAsAccountListItems`), so each
  shows as a card with name, description, and card image, consistent with the other giving blocks.
- The success-step offer is the shared `saveFinancialAccountForm.obs` control, posting to the shared
  `SaveFinancialAccountFormSaveAccount` endpoint (`Rock.Rest/v2/ControlsController.cs`). The endpoint saves
  the account to the transaction's authorized person (so impersonation and anonymous both resolve correctly)
  and handles the optional login creation. It is wrapped in a section for the Figma icon / heading /
  description. The control itself was refreshed to the Figma: a modern inline "Save as New Account" checkbox
  reveals a `ConditionalWell` (the indented blue-accented well) holding the hardcoded "Account Name" field
  (with help text) above the Save Account button. Because the control is shared, Registration Entry inherits
  the refresh, and its now-redundant outer `well` wrapper on the control was removed.

Because the block reuses that endpoint, it inherits the fix for
[GitHub issue #6877](https://github.com/SparkDevNetwork/Rock/issues/6877): on a Passwordless-only site
(Database authentication inactive) an anonymous saver would otherwise get an orphaned, unusable Database
login, so the endpoint refuses and this block hides the offer instead. This is a giver-visible change on
Passwordless-only sites (saving a payment method is unavailable there), flagged for PO sign-off as an Asana
"Question for PO" subtask. The Text-to-Give auto-save is the one saved-account path that stays in the block,
since it is automatic and has no shared-control equivalent.

A related parity note falls out of the same reuse. Legacy shows the create-a-login sub-panel based on
whether the *authorized (target) individual* has a `UserLogin`
(`phCreateLogin.Visible = !UserLoginService.GetByPersonId(person.Id).Any()`), while the shared control shows
it based on whether the *session* is anonymous (`!CurrentPerson`). The gift and the saved account are
unaffected either way, the account is still stored against the target individual. The one difference is the
login-creation step: when a staff member enters a gift under impersonation (Staff Impersonation on, not
Text-to-Give) for a target individual who has no login, legacy would show the create-login fields and let
the staff member set that individual's username and password, whereas the shared control does not (the
staff member is signed in). The same holds for the rare signed-in giver who has no login. Matching legacy
exactly would mean adding a target-person / login-needed input to the shared control and endpoint, which
Registration Entry also depends on. Recommend accepting the shared behavior; this folds into the same #6877
"Question for PO" subtask, since both concern whether a login is created when a payment method is saved.

### Campus / account mapping moves to the server
On WebForms, `CampusAccountAmountPicker` translates each selected (displayed) account into the
campus-specific child account before saving. The translation already lives in a reusable cache
helper, `FinancialAccountCache.GetMappedAccountForCampus(campus)`
(`Rock/Web/Cache/Entities/FinancialAccountCache.cs:298`); the WebForms control is just a thin
wrapper over it (`Rock/Web/UI/Controls/CampusAccountAmountPicker.cs:486`). The Asana note's
`FinancialAccountCache.TranslateAccount(...)` is illustrative; the real method already exists under
this name.

The Obsidian `campusAccountAmountPicker.obs` control already exists and is already streamlined: it
emits the selected account(s) and the campus separately (`update:modelValue` as `AccountAmount[]`,
`update:campusGuid`) and performs no mapping. There is nothing to remove from the client. The only
change for this conversion is in the C# save path: on first create, map each selected account to its
campus child, then save. To reproduce WebForms exactly, map when Campus Account Mapping is enabled OR
the account itself uses campus child accounts, and force child-account matching (see the Mapping
fidelity item under [Engineering items to resolve in build](#engineering-items-to-resolve-in-build-not-po-decisions)).

```csharp
// On create only; the picker hands the block the displayed account(s) + campus.
if ( entity.Id == 0 )
{
    var campus = CampusCache.Get( selectedCampusGuid );

    foreach ( var accountAmount in selectedAccountAmounts )
    {
        var account = FinancialAccountCache.Get( accountAmount.AccountGuid );

        // Map when the block setting is on or the account itself uses campus child
        // accounts, forcing child-account matching via the now-public force overload.
        var shouldMap = isCampusAccountMappingEnabled || account.UsesCampusChildAccounts;
        var mappedAccount = shouldMap
            ? account.GetMappedAccountForCampus( campus, forceChildAccounts: true )
            : account;
        // use mappedAccount.Id for this account's transaction detail
    }
}
```

On edit the stored account is already the mapped child, so the `Id == 0` guard avoids
re-translating. The blocks that use this control in mapping mode do not allow editing today, so the
guard matches current behavior. This block only creates transactions (no edit flow), so it is always
on the `Id == 0` path, exactly as the Asana task describes for the existing mapping-mode blocks.

### Client account/amount entry: `campusAccountAmountPicker` with `alwaysHideCampus`
Decision (resolves the earlier keep-vs-bespoke question): the Contribution Information section uses
the shared `campusAccountAmountPicker.obs` control for account and amount entry, which is the
approach the Asana task assumes. Because the Figma places campus selection in its own section, the
control's built-in campus picker is suppressed with a new additive prop, `alwaysHideCampus` (default
false, so existing consumers are unaffected). This is safe: the control's campus output is vestigial,
it does not influence which accounts load (the account REST endpoint takes no campus), so hiding it
loses nothing. Campus is captured in the Campus Information section and handed to the C# save path,
which performs the parent-to-campus-child translation via
`FinancialAccountCache.GetMappedAccountForCampus` on first save only, satisfying the Asana mapping
requirements above.

Two control limitations are handled block-side, not by changing the shared control:
- The control's `modelValue` is read-only-out (its amount inputs are internally driven and are not
  rehydrated from an incoming value). Preset and read-only amounts from the `AccountIds` URL option
  are therefore applied in the block, and the entry step is kept mounted across confirmation/back
  navigation so typed amounts survive. The gateway control is the one exception: it is remounted on
  returning to entry (hosted-field gateways do not survive a hide/show after tokenizing), so
  the card/ACH form re-initializes while the picker and contact fields stay mounted.
- The shared account endpoint returns only active, public, in-date accounts, so the
  private/URL-specified account path is resolved block-side (see the [engineering items](#engineering-items-to-resolve-in-build-not-po-decisions)).

Shared-control edits (this conversion modifies the framework control `campusAccountAmountPicker.obs`;
all additive or corrective, no behavior change for existing consumers, which were gallery-only):
- Added the `alwaysHideCampus` prop (plus an `isCampusPickerVisible` computed driving both pickers'
  `v-if`) so a consumer that owns campus selection elsewhere can hide the built-in picker.
- Single-account mode: added `disableLabel` to the amount box so it renders a standard form-group
  with the normal bottom margin (it previously had none and butted against the next field).
- The amount-entry watcher now MERGES on reload, preserving amounts already typed (multiple-account)
  and the current selection (single-account) instead of resetting them. Required so adding an account
  (which reloads the selectable list) does not wipe in-progress entry.
- The loading indicator shows only on the first account fetch; later reloads (e.g. adding an account)
  refresh in the background so amounts already entered stay visible instead of flashing the loading
  state. Also removed a stray debug `console.log` from the required-amount validation rule.
- Added a `disabled` prop (default false) that forwards to the amount inputs, the single-account
  account picker, and the campus picker, so a consumer can disable the whole control (the block sets it
  while a payment is being processed). The control had no disabled support before.
- Gallery (`campusAccountAmountPickerGallery.partial.obs`): removed the phantom
  `disableAccountCampusMappingLogic` prop (never a real control prop) and exposed `alwaysHideCampus`.

The "Add Another Account" control (5b-ii) is block work, not a control feature: the block ships the
addable-account tree on the options bag and appends the chosen Guid to `selectableAccountGuids`. It
renders with the shared `treeItemPicker` (a popup tree, every node selectable); the conversion added
one additive prop to that control, `hideMainActionButtons` (default false), so the popup's
Select/Cancel footer is hidden here, where single-select applies on click and the buttons are
redundant. Its visibility is gated by Allow Additional Accounts + a configured account subset + a
non-empty pool; it is NOT gated by single/multiple-account mode (legacy parity, confirmed against the
WebForms code and live testing). Account display order follows each account's `Account.Order`
(admin-controlled), a deliberate, PO-flagged departure from the legacy block's
effectively-uncontrollable Id order.

### Settings tab organization
Settings move from one flat list into three tabs. The full per-tab grouping is in
[Block Settings: tab organization](#block-settings-tab-organization). Attribute Keys are unchanged;
only the display label, help text, and editor grouping change.

### Heading model: block header vs panel title

Two heading treatments exist and must not stack:
- **Block header section** (icon + title + description), gated by `Show Block Header Section` (on for the public giving instances; off for the internal Add Transaction instance via migration).
- **Panel title** (`Panel Title`) plus section headings, gated by the existing `Show Panel & Section Headings`. In the legacy a single toggle controlled both the top panel title and every section heading (`UtilityPaymentEntry.ascx.cs:2317`).

Decision: render the panel title with `hasTitle = ShowPanelHeadings && !ShowBlockHeaderSection`, titled by `Panel Title`. This keeps the legacy `ShowPanelHeadings` gate and stops the block header and the panel title from both showing (internal uses the panel title; external uses the block header). Implemented as of the no-gateway slice (`ShowPanelHeadings` is surfaced as `IsPanelAndSectionHeadingsShown` on the options bag). In the no-gateway state the panel title is the fixed string "Getting Started With Contributions" rather than `Panel Title`.

Resolved (PO not required): keep the legacy single toggle. `Show Panel & Section Headings` (`IsPanelAndSectionHeadingsShown`) gates both the panel title and every section header together. When it is off, sections render their body with no header (icon, title, description all suppressed). No independent per-section heading control is added.

### Styling delivery: scoped, not the block SCSS

All of this block's custom CSS lives in a scoped `<style>` in `utilityPaymentEntry.obs`, **not** in
`Rock.Frontend.Styles/src/styles/styles-v2/blocks/_blocks-finance.scss`. Reason: the styles-v2
`core.css` (which holds both the Obsidian component layer and the `_blocks-[domain].scss` files) is
linked only by the StyleV2/NextGen themes (`RockNextGen`, `RockManagerNextGen`, `NextGenCheckin`). An
external giving site on another theme never loads it, so block styles placed in the SCSS reach the
internal admin but not the public giving pages. A scoped `<style>` bundles with the block and applies
on any theme, the same reason `emailPreferenceEntry.obs` scopes its header CSS. This intentionally
deviates from the house convention (block styles go in `_blocks-[domain].scss`, not scoped); the UI
team will reconcile that convention later.

Within the scoped block: use Rock CSS variables, no utility classes (`text-primary`, `mb-3`, etc.)
and no hardcoded pixels. Example already in place: the block-header icon replicates the
`.contentsection-header-icon` badge (`--color-primary` foreground, `--color-primary-soft` background,
`--spacing-small` padding, `--font-size-h5` glyph, `--rounded-small` corners). Vertical spacing
between the no-gateway gateway cards comes from `DisplayCardContainer`, not margins.

### No gateways configured (as built)

When the block's `FinancialGateway` attribute resolves to no gateway, the options bag carries
`IsGatewayConfigured = false` plus a `SupportedGateways` list (`SupportedGatewayBag`: Name,
Description, ConfigureUrl, LearnMoreUrl). The list reproduces the legacy `ShowGatewayHelp`: installed
`IHostedGatewayComponent`s, excluding `TestGateway`, keeping only those with at least one active
`FinancialGateway` instance. The `.obs` shows the welcome warning `NotificationBox` and one
`DisplayCard` per gateway (Configure / Learn More). The other config states from the legacy
`LoadGatewayOptions`, ACH and Credit Card both disabled, an unsupported (non-hosted) gateway, and the
Test Gateway notice, are not yet built.

## UI Reference

Full-node exports from the Figma file (node 6248-4190), one image per design frame, captured at
(near) 1:1 so the designer's annotation badges and UX/UI Notes callouts stay legible. Each image is
a 1-for-1 representation of its Figma frame: the OLD (current WebForms) reference, the refreshed
external and internal screens, the variants, and the notes the designer attached. All images live in
`artifacts/260626-utility-payment-entry-obsidian-conversion/`.

### Step 1a: Entry (all settings on)

Current WebForms, external and internal:

![Entry 1a, current WebForms, external](artifacts/260626-utility-payment-entry-obsidian-conversion/1a-entry-old-external.png)

![Entry 1a, current WebForms, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/1a-entry-old-internal.png)

Refreshed, external and internal:

![Entry 1a, refreshed, external](artifacts/260626-utility-payment-entry-obsidian-conversion/1a-entry-new-external.png)

![Entry 1a, refreshed, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/1a-entry-new-internal.png)

Variants and notes: fluid layout (external, internal), the confirmation-enabled note, and the CAPTCHA
note:

![Entry 1a, fluid layout, external](artifacts/260626-utility-payment-entry-obsidian-conversion/1a-entry-new-external-fluid.png)

![Entry 1a, fluid layout, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/1a-entry-new-internal-fluid.png)

![Entry 1a, confirmation enabled note](artifacts/260626-utility-payment-entry-obsidian-conversion/1a-entry-confirmation-enabled.png)

![Entry 1a, CAPTCHA note](artifacts/260626-utility-payment-entry-obsidian-conversion/1a-entry-captcha.png)

### Step 1b: Entry (no gateways configured)

Current WebForms, external and internal:

![Entry 1b, current WebForms, external](artifacts/260626-utility-payment-entry-obsidian-conversion/1b-nogateway-old-external.png)

![Entry 1b, current WebForms, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/1b-nogateway-old-internal.png)

Refreshed, external and internal:

![Entry 1b, refreshed, external](artifacts/260626-utility-payment-entry-obsidian-conversion/1b-nogateway-new-external.png)

![Entry 1b, refreshed, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/1b-nogateway-new-internal.png)

### Step 2: Confirmation

Current WebForms, external and internal:

![Confirmation, current WebForms, external](artifacts/260626-utility-payment-entry-obsidian-conversion/2-confirm-old-external.png)

![Confirmation, current WebForms, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/2-confirm-old-internal.png)

Refreshed, external and internal:

![Confirmation, refreshed, external](artifacts/260626-utility-payment-entry-obsidian-conversion/2-confirm-new-external.png)

![Confirmation, refreshed, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/2-confirm-new-internal.png)

Possible-duplicate-transaction warning:

![Confirmation, duplicate warning](artifacts/260626-utility-payment-entry-obsidian-conversion/2-confirm-duplicate-warning.png)

### Step 3: Success

Current WebForms, external and internal:

![Success, current WebForms, external](artifacts/260626-utility-payment-entry-obsidian-conversion/3-success-old-external.png)

![Success, current WebForms, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/3-success-old-internal.png)

Refreshed, external and internal:

![Success, refreshed, external](artifacts/260626-utility-payment-entry-obsidian-conversion/3-success-new-external.png)

![Success, refreshed, internal](artifacts/260626-utility-payment-entry-obsidian-conversion/3-success-new-internal.png)

Text-to-Give success variant:

![Success, Text-to-Give](artifacts/260626-utility-payment-entry-obsidian-conversion/3-success-texttogive.png)

### Settings

![Settings copy polish guide](artifacts/260626-utility-payment-entry-obsidian-conversion/settings-copy-polish-guide.png)

## Block Settings: tab organization

Source: the three "Block Settings" sidebar callouts in the Figma (node ids 6544:4332, 6544:4350,
6544:4358). Items marked NEW are added by this refresh (see
[new settings](#block-settings-new-settings)); all others are existing settings shown in their new
grouping.

**Tab 1: Basic Settings**
- *1a. General:* Financial Gateway, Enable ACH, Enable Credit Card, Batch Name Prefix, Transaction
  Source, Prompt for Campus When Known, Include Inactive Campuses, Campus Type Filter, Campus Status
  Filter, Allow Multiple Accounts, Layout Style, Accounts to Display, Allow Additional Accounts,
  Group Additional Accounts by Hierarchy, Campus Account Mapping, Allow Scheduled Gifts, Allow
  Scheduled End Date, Staff Impersonation, Show Confirmation Step.
- *1b. Payer Settings:* Prompt for Phone, SMS Opt-In, Prompt for Email, Address Type, Connection
  Status (New People), Record Status (New People), Record Source (New People), Allow Business
  Giving, Allow Anonymous Giving, Allow Comment Entry, Disable CAPTCHA.
- *1c. Email Templates:* Account Confirmation Email, Receipt Email.

**Tab 2: Customize Text**
- *2a. General:* Show Panel & Section Headings, Panel Title, Transaction Header Template, Payment
  Comment Template.
- *2b. Block Header Section:* NEW Show Block Header Section, NEW Header Title, NEW Header
  Description, NEW Header Icon.
- *2c. Campus Information Section:* NEW Campus Information Section Title, NEW Campus Information
  Section Icon, NEW Campus Information Section Description.
- *2d. Contribution Information Section:* Contribution Information Section Heading, NEW Contribution
  Information Section Icon, NEW Contribution Information Section Description, Add Account Button Text,
  Account Label Template, Comment Field Label.
- *2e. Contact Information Section:* Contact Information Section Heading, NEW Contact Information
  Section Icon, NEW Contact Information Section Description, Anonymous Giving Tooltip.
- *2f. Payment Information Section:* Payment Information Section Heading, NEW Payment Information
  Section Icon, NEW Payment Information Section Description.
- *2g. Confirmation Page:* Confirmation Section Heading, Confirmation Header, NEW Confirmation Body,
  Confirmation Footer.
- *2h. Success Page:* Success Page Template, Save Payment Method Section Heading, NEW Save Payment
  Method Section Icon, NEW Save Payment Method Section Description, Success Page Footer.

**Tab 3: Advanced**
- Allow Account Options in URL, Restrict URL Accounts to Public Only, Invalid Account Message,
  Account Campus Context Filter, Transaction Attributes from URL, Transaction Type, Transaction
  Entity Type, Entity ID Parameter, Show Initial Back Button, Text-to-Give Mode.

## Block Settings: new settings

16 additive settings, almost all supporting the redesigned sectioned layout. Defaults below
are from the Figma callouts.

| Setting | Type | Default | Description |
|---|---|---|---|
| Show Block Header Section | BooleanField | True. External giving pages show the header; the internal Add Transaction instance is disabled via migration (see [Requirements](#requirements)) | When enabled, displays a title and description at the top of the block. |
| Header Title | TextField | "New Contribution" | The title displayed at the top of the block. |
| Header Description | TextField | "Provide details to set up a new contribution." | The supporting text displayed below the header title. |
| Header Icon | TextField | ti ti-cash | The icon displayed in the block header. |
| Campus Information Section Title | TextField | "Campus Information" | The label displayed in the Campus Information section header. |
| Campus Information Section Icon | TextField | ti ti-map-pin | The icon displayed in the Campus Information section header. |
| Campus Information Section Description | TextField | "Select the campus that your gift should be associated with." | Supporting text below the section title. |
| Contribution Information Section Icon | TextField | ti ti-gift | The icon displayed in the Contribution Information section header. |
| Contribution Information Section Description | TextField | "Specify how much to contribute, where it should go, and how often." | Supporting text below the section title. |
| Contact Information Section Icon | TextField | ti ti-user-circle | The icon displayed in the Contact Information section header. |
| Contact Information Section Description | TextField | "Provide contact details to associate with this gift." | Supporting text below the section title. |
| Payment Information Section Icon | TextField | ti ti-wallet | The icon displayed in the Payment Information section header. |
| Payment Information Section Description | TextField | "Enter the payment method and billing details used to process this gift." | Supporting text below the section title. |
| Confirmation Body | CodeEditorField (Lava) | (none) | Body content rendered on the confirmation step. Supports Lava. |
| Save Payment Method Section Icon | TextField | ti ti-bolt | The icon displayed in the Save Payment Method section header. |
| Save Payment Method Section Description | TextField | (none) | Supporting text below the section title. |

These are additive configuration options, not flow changes. The one with upgrade impact is
`Show Block Header Section`: a data migration disables it on the known internal instance(s) so only
public giving pages gain the new header (see [Requirements](#requirements)).

## Block Settings: copy polish

Existing settings whose label and/or help text changed. Attribute Keys are unchanged, so saved
configuration is preserved. Reference image:
[settings-copy-polish-guide.png](artifacts/260626-utility-payment-entry-obsidian-conversion/settings-copy-polish-guide.png).

| Category | Original Label | New Label | Original Help Text | New Help Text | Notes |
|---|---|---|---|---|---|
| Default | Financial Gateway | Financial Gateway | The payment gateway to use for Credit Card and ACH transactions. | The payment gateway for credit card and ACH transactions. | Removed redundant "to use". |
| Default | Enable ACH | Enable ACH | (none) | Whether ACH bank account payments are accepted. | Added help text. |
| Default | Enable Credit Card | Enable Credit Card | (none) | Whether credit card payments are accepted. | Added help text. |
| Default | Batch Name Prefix | Batch Name Prefix | The batch prefix name to use when creating a new batch. | The prefix applied to new batch names created by this block. | Removed "to use when". |
| Default | Source | Transaction Source | The Financial Source Type to use when creating transactions. | The financial source type applied to transactions created by this block. | Label clarified; "to use when" removed. |
| Default | Ask for Campus if Known | Prompt for Campus When Known | If the campus for the person is already known, should the campus still be prompted for? | Whether to prompt for campus even when the person's campus is already known. | Label reworded to be less ambiguous. |
| Default | Include Inactive Campuses | Include Inactive Campuses | Set this to true to include inactive campuses | Whether inactive campuses are included in the campus list. | Removed instructional "Set this to true". |
| Default | Campus Types | Campus Type Filter | Set this to limit campuses by campus type. | Limits the campus list to the selected campus types. | Label clarified as a filter. |
| Default | Campus Statuses | Campus Status Filter | Set this to limit campuses by campus status. | Limits the campus list to the selected campus statuses. | Same pattern as Campus Types. |
| Default | Enable Multi-Account | Allow Multiple Accounts | Should the person be able specify amounts for more than one account? | Whether the giver can split their gift across multiple accounts. | Label clearer; help text outcome-oriented. |
| Default | Impersonation | Staff Impersonation | Should the current user be able to view and edit other people's transactions? IMPORTANT: Only enable on internal pages secured to trusted users. | Allows staff to view and edit transactions on behalf of another person. Only enable this on internal pages secured to trusted individuals. | Label clarified; warning preserved. |
| Default | Layout Style | Layout Style | How the sections of this page should be displayed. | Controls whether the block's sections are stacked vertically or displayed in a fluid layout. | Made concrete by describing the two options. |
| Default | Account Header Template | Account Label Template | The Lava Template to use as the amount input label for each account. | The Lava template used as the label for each account's amount input. | Label clarified. Threaded, not dropped: the block resolves this template server-side and ships the labeled account list to the picker. Only accounts fetched through the shared accounts endpoint at runtime fall back to `{{ Account.PublicName }}`. |
| Default | Accounts | Accounts to Display | The accounts to display. If Account Campus mapping logic is enabled and the account has a child account for the selected campus, the child account will be used. | The accounts shown to the giver. When campus mapping is enabled, a matching child account for the selected campus will be used in place of the parent. | Shortened and clarified. |
| Default | Additional Accounts | Allow Additional Accounts | Should users be allowed to select additional accounts? If so, any active account with a Public Name value will be available. | Whether givers can add accounts beyond the configured list. Any active, publicly named account will be available. | Label clarified; help text split. |
| Default | Enable Account Hierarchy for Additional Accounts | Group Additional Accounts by Hierarchy | When "Additional Accounts" is enabled, this allows grouping of accounts under their respective parents. | When additional accounts are enabled, groups them under their parent accounts. Note: campus-mapped accounts still appear in the hierarchy when campus mapping is on. | Label shorter and action-oriented. |
| Default | Use Account Campus Mapping Logic | Campus Account Mapping | If enabled, the accounts will be determined based on campus matching logic. | When enabled, the block selects child accounts that match the giver's campus. If no matching child exists, the parent account is used. | Condensed multi-bullet description into prose. |
| Default | Scheduled Transactions | Allow Scheduled Gifts | If the selected gateway(s) allow scheduled transactions, should that option be provided to user? Not compatible with Text-to-Give mode. | Whether givers can set up recurring scheduled gifts. Not compatible with Text-to-Give mode. | Label clearer; help text tightened. |
| Default | Prompt for Phone | Prompt for Phone | Should the user be prompted for their phone number? | Whether givers are prompted to enter their phone number. | Removed question framing. |
| Default | SMS Opt-in | SMS Opt-In | If "Prompt for Phone" is set to "Yes" then selecting "Show" will allow a user to opt into receiving SMS communications. | When phone prompting is enabled, displays an opt-in checkbox for SMS communications on the entered number. | Removed conditional instruction framing. |
| Default | Prompt for Email | Prompt for Email | Should the user be prompted for their email address? | Whether givers are prompted to enter their email address. | Same pattern as phone. |
| Default | Address Type | Address Type | The location type to use for the person's address. | The location type used when saving or updating the person's address. | More specific about what it does. |
| Default | Connection Status | Connection Status (New People) | The connection status to use for new individuals (default: "Prospect"). | The connection status assigned to new individuals created through this block. | Label scopes it to new people. |
| Default | Record Status | Record Status (New People) | The record status to use for new individuals (default: "Pending"). | The record status assigned to new individuals created through this block. | Same pattern as Connection Status. |
| Default | Record Source | Record Source (New People) | The record source to use for new individuals (default = "Giving"). If a "RecordSource" page parameter is found, it will be used instead. | The record source assigned to new individuals. Can be overridden by a RecordSource page parameter. | Cleaner; parameter name preserved. |
| Default | Enable Comment Entry | Allow Comment Entry | Allows the guest to enter the value that's put into the comment field (appended to the "Payment Comment Template"). | Whether givers can enter a custom comment. The entered value is appended to the Payment Comment Template. | Label less passive; help text declarative. |
| Default | Comment Entry Label | Comment Field Label | The label to use on the comment edit field (e.g. Trip Name to give to a specific trip). | The label shown on the comment input field (e.g., Trip Name). | Shorter; example preserved. |
| Default | Enable Business Giving | Allow Business Giving | Should the option to give as a business be displayed? | Whether the option to give as a business is shown to the giver. | Label clearer; help declarative. |
| Default | Enable Anonymous Giving | Allow Anonymous Giving | Should the option to give anonymously be displayed? Giving anonymously will display the transaction as "Anonymous" in public-facing places. | Whether givers can choose to give anonymously. Anonymous gifts appear as "Anonymous" on public-facing contribution lists. | Label clearer; help text tightened. |
| Default | Disable Captcha Support | Disable CAPTCHA | If set to "Yes" the CAPTCHA verification step will not be performed. | Skips the CAPTCHA verification step when enabled. | Label capitalization fixed; removed "if set to Yes". |
| Default | Enable End Date | Allow Scheduled End Date | When enabled, this setting allows an individual to specify an optional end date for their recurring scheduled gifts. | Whether givers can set an optional end date for recurring scheduled gifts. | Label clarified; help text declarative. |
| Email Templates | Confirm Account | Account Confirmation Email | Confirm Account Email Template | The system communication sent to confirm a new account. | Label more descriptive. |
| Email Templates | Receipt Email | Receipt Email | The system email to use to send the receipt. | The system communication used to send giving receipts. | Minor tightening. |
| Text Options | Panel Title | Panel Title | The text to display in panel heading | The heading text shown at the top of the block panel. | Added context about where it appears. |
| Text Options | Contribution Info Title | Contribution Information Section Heading | The text to display as heading of section for selecting account and amount. | The heading for the account and amount selection section. | Label clearer. |
| Text Options | Personal Info Title | Contact Information Section Heading | The text to display as heading of section for entering personal information. | The heading for the contact information section. | Renamed from Personal to Contact Information. |
| Text Options | Payment Info Title | Payment Information Section Heading | The text to display as heading of section for entering credit card or bank account information. | The heading for the payment method section. | Same pattern. |
| Text Options | Show Confirmation Page | Show Confirmation Step | Show a confirmation page before processing the transaction. | Whether a confirmation step is shown before the transaction is processed. | Label shorter; help text declarative. |
| Text Options | Confirmation Title | Confirmation Section Heading | The text to display as heading of section for confirming information entered. | The heading for the confirmation review section. | Same pattern. |
| Text Options | Confirmation Header | Confirmation Header | The text (HTML) to display at the top of the confirmation section. | HTML displayed at the top of the confirmation section. Supports Lava. | Removed tip HTML spans; noted Lava support. |
| Text Options | Confirmation Footer | Confirmation Footer | The text (HTML) to display at the bottom of the confirmation section. | HTML displayed at the bottom of the confirmation section. Supports Lava. | Same as Confirmation Header. |
| Text Options | Finish Lava Template | Success Page Template | The text (HTML) to display on the success page. | The Lava template rendered on the success page after a transaction completes. | Label clearer. |
| Text Options | Success Footer | Success Page Footer | The text (HTML) to display at the bottom of the success section. | HTML displayed at the bottom of the success page. Supports Lava. | Label clarified. |
| Text Options | Save Account Title | Save Payment Heading | The text to display as heading of section for saving payment information. | The heading for the save payment method section. | Label clearer. Section callout calls this "Save Payment Method Section Heading" (reconcile). |
| Text Options | Add Account Text | Add Account Button Text | The button text to display for adding an additional account | The label on the button that adds another account. | Label describes what it controls. |
| Text Options | Payment Comment Template | Payment Comment Template | The comment to include with the payment transaction when sending to Gateway. | The Lava template for the comment sent to the payment gateway with each transaction. | Noted Lava support; clarified "Gateway" context. |
| Text Options | Anonymous Giving Tooltip | Anonymous Giving Tooltip | The tooltip for the "Give Anonymously" checkbox. | The tooltip text shown on the Give Anonymously checkbox. | Minor clarity improvement. |
| Advanced | Allow Account Options In URL | Allow Account Options in URL | Set to true to allow account options to be set via URL. | Whether account options (IDs, GL codes, amounts, editability) can be passed as URL parameters. | Removed "Set to true"; condensed long example URL text. |
| Advanced | Only Public Accounts In URL | Restrict URL Accounts to Public Only | Set to true if using "Allow Account Options In URL" to prevent non-public accounts from being specified. | When URL account options are enabled, prevents non-public accounts from being specified in the URL. | Label clearer; removed "Set to true". |
| Advanced | Invalid Account Message | Invalid Account Message | Display this text (HTML) as an error alert if an invalid account or GL account is passed through the URL. | HTML error message shown when an invalid account ID or GL code is passed in the URL. | Removed instructional "Display this text". |
| Advanced | Account Campus Context | Account Campus Context Filter | Should any context be applied to the Account List | Whether and how the current campus context filters the account list. | Label clarified as a filter. |
| Advanced | Allowed Transaction Attributes From URL | Transaction Attributes from URL | Specify any Transaction Attributes that can be populated from the URL. | Transaction attributes that can be set via URL parameters using the Attribute_ prefix. | Removed "Specify any"; example condensed. |
| Advanced | Transaction Type | Transaction Type | (none) | The financial transaction type applied to transactions created by this block. | Added help text. |
| Advanced | Transaction Entity Type | Transaction Entity Type | The Entity Type for the Transaction Detail Record (usually left blank) | The entity type for the transaction detail record. Leave blank unless this block is linked to a specific entity. | Reworded parenthetical into a sentence. |
| Advanced | Entity Id Param | Entity ID Parameter | The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entity Type to be configured) | The page parameter used to populate the entity ID on the transaction detail record. Requires Transaction Entity Type to be set. | Label cleaned up; help text clearer. |
| Advanced | Transaction Header | Transaction Header Template | The Lava template which will be displayed prior to the Amount entry | The Lava template displayed above the amount entry fields. | Minor tightening. |
| Advanced | Enable Initial Back button | Show Initial Back Button | Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry | Whether a Back button is shown on the first step, navigating the individual to the previous page. | Label casing fixed; help text declarative. |
| Advanced | Show Panel Headings | Show Panel & Section Headings | Show the text headings at the top of the block and in panel sections. | Whether the block title and section headings are visible. | Declarative; removed instructional "Show". |
| Advanced | Enable Text-To-Give Mode | Text-to-Give Mode | This setting enables specific behavior for setting up Text-To-Give accounts. | Enables the Text-to-Give account setup flow. Not compatible with scheduled transactions. | Label shortened; scheduling incompatibility surfaced. |

## Block Settings: reconciliation

The block is built reveal-as-needed: settings are threaded in slice by slice as each part of the flow is implemented. That makes it easy to leave a setting unwired without noticing, so before the conversion is considered complete, every block setting is reconciled against the implementation.

Reconciled against the authoritative lists in this spec ([tab organization](#block-settings-tab-organization), [new settings](#block-settings-new-settings), and [copy polish](#block-settings-copy-polish)), which together enumerate every attribute Key (the preserved legacy Keys plus the 16 new settings). Each Key is exactly one of:

- **Threaded** end to end: the C# block reads the setting and it reaches the giver, either through an OptionsBag/Bag field consumed by the `.obs`, or through the C# save/action path (including Lava resolved server-side and shipped as HTML).
- **Pending** a remaining item, with a one-line reason and the item number.
- **Dropped** by an approved decision.

**Result (2026-07-21):** all 75 attribute Keys are threaded end to end; none are pending, dropped, or left unaccounted for. The 11 page parameters in the [carry-forward table](#page-parameters-carry-forward-tracking) are likewise all consumed. Two items surfaced during the pass:

- **Account Label Template** (`AccountHeaderTemplate`) is threaded, not dropped. The block resolves the label template server-side and supplies its own labeled account list (configured, URL, and addable) to `campusAccountAmountPicker.obs`, which it already had to feed with the block's account list. The shared accounts REST endpoint still hardcodes `{{ Account.PublicName }}` (see the [engineering items](#engineering-items-to-resolve-in-build-not-po-decisions)), so only accounts fetched through that endpoint at runtime fall back to the public name; the block's displayed lists honor the template.
- **`RecordSource` page parameter** reaches the save path through the shared `RecordSourceHelper`, not through this block's `PageParameterKey` class. Left as is: functionally correct, no local constant added.

### Engineering items to resolve in build (not PO decisions)
Dev-team items, listed because they drive effort. The CampusAccountAmountPicker parity spike is
complete; findings below.

- **Additional Accounts / hierarchy is block work, not a shared-control gap (resolved).** Neither the
  WebForms nor the Obsidian `campusAccountAmountPicker` control implements additional accounts. The
  WebForms block renders a separate "Add Another Account" dropdown (`phbtnAddAccount` in
  `RockWeb/Blocks/Finance/UtilityPaymentEntry.ascx`) and appends the chosen account to the picker's
  selectable accounts (`UtilityPaymentEntry.ascx.cs:1377` and `:1533`). The Obsidian block must
  reimplement that dropdown (flat or hierarchical per the setting) and append to the control's
  `selectableAccountGuids`; the shared control needs no change. Expected block work, not a hidden
  control dependency.
- **Account Label Template (resolved: threaded).** The block resolves the label template server-side in
  `GetAccountLabelResolver()` and applies it to every account list it ships: configured accounts via
  `ResolveAccountListItems`, addable accounts via `GetAvailableAdditionalAccounts`. The block supplies
  its own account list to `campusAccountAmountPicker.obs` rather than letting the control fetch from the
  shared endpoint, so the displayed labels honor the template. The shared REST endpoint at
  `Rock.Rest/v2/ControlsController.cs:2637` still hardcodes `{{ Account.PublicName }}` (it resolved a
  client-supplied template before), so any consumer that fetches accounts through it at runtime falls
  back to the public name; the underlying Lava Fluid file-access issue is a platform-level concern
  tracked separately, not solved by this spec. See the
  [Account Header Template question](https://app.asana.com/1/20866866924293/task/1216405822150734).
- **Private / URL-specified accounts are filtered out.** The same endpoint always restricts to active,
  public, in-date accounts (`ControlsController.cs:2613`) with no `AllowPrivateSelectableAccounts`
  equivalent. The Advanced "Allow Account Options in URL" path with non-public accounts will not
  display them. Edge case, but a real parity gap; also shared.
- **Mapping fidelity (implemented).** The save path reproduces the WebForms control's mapping decision
  exactly. It maps when Campus Account Mapping is on OR the account's own `UsesCampusChildAccounts` is
  true (matching the control's guards at `CampusAccountAmountPicker.cs:442` and `:479`), then calls
  `FinancialAccountCache.GetMappedAccountForCampus( campus, forceChildAccounts: true )` so a child match
  is forced even when the account itself is not flagged for campus children (matching the control at
  `CampusAccountAmountPicker.cs:486`). That force overload was `internal`; it was made `public` and
  marked `[RockInternal( "20.0", keepInternalForever: true )]` so the block can call it from another
  assembly while keeping it off-limits to plugins, honoring the Asana directive to keep the mapping
  logic in one place instead of duplicating it in the block.

## Considered but Rejected

### Keep the campus/account mapping logic in the Obsidian control
Rejected. Duplicating the mapping in the client control spreads the logic across two places and
complicates the Obsidian `CampusAccountAmountPicker`. Keeping it in the C# save path (first-save
only) keeps the mapping in one place and matches the WebForms intent.

### Load without flash/resize (scrapped by PO)
Rejected. The PO decided not to pursue flash/resize prevention for this conversion, so the block will
load like other Obsidian blocks. The analysis is preserved here in case it is revisited.

The plan was the "pseudo-static block" pattern the `ContentChannel[Item]View` blocks use: override
`GetInitialHtmlContent()` on the C# block (base contract `Rock/Blocks/RockBlockType.cs:504`; the
returned HTML is embedded in the page and SEO-indexable, and Obsidian replaces it once it loads). The
Obsidian host wires that string in as `staticContent` (`rockBlock.partial.ts:465`, sourced from
`config.InitialContent`), and the `.obs` paints it immediately via `<div v-content="staticContent">`
fed by `useStaticContent()`, the same way `contentChannelView.obs` does, so first paint is already
correct. `ContentChannelView` returns its fully rendered output at `ContentChannelView.cs:274`.

The chosen approach was (a): emit a non-personalized structural skeleton from `GetInitialHtmlContent()`
and let Vue hydrate the personalized fields, with `InitialBlockHeightAttribute`
(`Rock/Blocks/InitialBlockHeightAttribute.cs`, used at `RockBlockType.cs:451`) as a fallback height.
Options weighed:

| Option | What it does | Risk / cost |
|---|---|---|
| (a) chosen | Emit a non-personalized structural skeleton from `GetInitialHtmlContent`; Vue hydrates personalized fields. | Lowest risk. Skeleton dimensions may not exactly match the hydrated content, so a small residual shift is possible. |
| (b) | Render the full first-step form server-side per request, no shared output cache. | Highest fidelity (no shift), but doubles the initial render logic (server HTML must track the Vue markup) and adds per-request server cost. |
| (c) | Output-cache the rendered form like `ContentChannelView`. | Not safe as-is: the form is personalized (current person, saved accounts, campus), so a shared cache could leak one giver's data to another. Would require strict per-person keying or a non-personalized cache scope. `ContentChannelView` itself refuses to output-cache personalized content (`Rock.Blocks/Cms/ContentChannelView.cs:1425`). |

A fixed-height placeholder alone (`InitialBlockHeightAttribute` without the skeleton) was rejected as
a sole fix: reserving a fixed height reduces but does not remove the shift, since the real content
height differs from the reserved height.

## Related
- Asana task DEV-13440: https://app.asana.com/1/20866866924293/project/1208321217019996/task/1208355544127392 (requirements live in this spec; task note is the source for the mapping relocation and the no-feature-changes constraint).
- Figma design (node 6248-4190): https://www.figma.com/design/N60VRdhtRtjO9EA9nba9fB/Obsidian-Block-Refreshes---Fall-2025---Spring-2026?node-id=6248-4190 (treated as canonical for UI; last-modified date not captured).
- Figma block-settings copy polish (node 6544-4369): https://www.figma.com/design/N60VRdhtRtjO9EA9nba9fB/Obsidian-Block-Refreshes---Fall-2025---Spring-2026?node-id=6544-4369 (canonical for setting labels and help text).
- WebForms source: `RockWeb/Blocks/Finance/UtilityPaymentEntry.ascx`, `RockWeb/Blocks/Finance/UtilityPaymentEntry.ascx.cs`.
- Load-without-flash pattern references (rejected approach, see Considered but Rejected): `Rock.Blocks/Cms/ContentChannelView.cs`, `Rock.Blocks/Cms/ContentChannelItemView.cs`, `Rock.JavaScript.Obsidian.Blocks/src/Cms/contentChannelView.obs`, `Rock/Blocks/RockBlockType.cs` (`GetInitialHtmlContent`, `InitialBlockHeightAttribute` usage), `Rock/Blocks/InitialBlockHeightAttribute.cs`.
