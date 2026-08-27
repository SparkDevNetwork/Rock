---
author: Joshua Henninger
date_created: 2026-08-17
summary: >-
  The v19 Obsidian Communication Entry Wizard showed all three medium options
  even when the block's "Communication Types" setting excluded SMS, because it
  collapsed two distinct concepts (which mediums appear as standalone picker
  options, and which mediums Recipient Preference actually composes and
  delivers) into one. The reported symptom is fixed; this spec remains the
  record for the deferred work: explicit deliverability validation, an
  admin-only configuration error banner, a send block, and treating Recipient
  Preference as unavailable until both Email and SMS are deliverable.
contributors: []
---

# Communication Entry Wizard: Recipient Preference and Medium Validation

| Field | Value |
|---|---|
| Issue | #6938 / DEV-14623 |
| Target | v19.5 |

## Problem

The reported symptom (#6938): in the v19 Obsidian Communication Entry Wizard, the medium picker shows all three options (Recipient Preference, SMS, Email) even when the block's "Communication Types" setting excludes SMS. A site configured "Recipient Preference and Email" specifically so a standalone "SMS to everyone" blast could never be sent (SMS opt-in / legal exposure). v19 no longer honored that.

Manual testing during the fix surfaced deeper issues in how Recipient Preference handles mediums, and review of the intended behavior evolved the expected design (see "Expected behavior (agreed)"). The deeper issues:

1. When Email is not an enabled option (only Recipient Preference and SMS), selecting Recipient Preference composes only SMS, yet the send still routes Email-preferring recipients to Email, which has no content (an empty or failed send). The symmetric case exists for "Recipient Preference + Email" and SMS-preferring recipients.
2. The Recipient Preference reachable-audience count and the confirmation-step medium tabs were tied to the standalone enabled mediums rather than to what Recipient Preference actually delivers.
3. Misconfiguration (a selected type with no active transport) is handled by silently hiding the option, so an admin gets no signal that their configuration cannot deliver.

## Background: two distinct medium concepts

- **Standalone picker options**: which concrete mediums (Email / SMS / Push), plus the Recipient Preference option itself, appear as selectable choices. Driven by the "Communication Types" block setting checkboxes.
- **Recipient Preference mediums**: when Recipient Preference is selected, the set of mediums the wizard composes content for, counts reachable audience against, and delivers through. Recipient Preference resolves each recipient to their preferred medium (Email or SMS, never Push).

These two are not the same. The core bug is that v19 collapsed them into one and used silent transport filtering for both.

## How we got here

- v19 rebuilt the wizard in Obsidian (beta commit `4f69ab8006`, Feb 2025).
- `98c5308c9a` (2025-11-12): removed Push from the Recipient Preference editor flow. Correct, and legacy-consistent (legacy never composed Push for Recipient Preference).
- `dd95a7c0ac` (2025-11-13): gated the per-medium reachable-audience counts by the enabled mediums.

Both Nov 2025 commits tied Recipient Preference's composition and counts to the **standalone enabled mediums** (the client `allowedMediums`, derived from the picker). For "all enabled" and "none enabled" configs this matches legacy. For subset configs that include Recipient Preference (for example "Recipient Preference + Email"), it diverges from legacy.

## Root cause

Legacy always composed Email and SMS for Recipient Preference (transport-gated, never Push). Concretely, legacy `CommunicationOptionCanBeShown` (RockWeb `CommunicationEntryWizard.ascx.cs` lines 2092-2100) used `GetAllowedCommunicationTypes()` in its expand form, so selecting Recipient Preference showed both the Email and SMS editors for every config that includes Recipient Preference. Legacy therefore always composed content for every medium Recipient Preference might deliver to, and the raw-preference delivery always had content.

v19 instead tied Recipient Preference composition and counts to the standalone picker mediums. So "Recipient Preference + Email" composes only Email, but delivery still routes SMS-preferring recipients to SMS.

The per-recipient delivery resolver is **identical** in both blocks (Obsidian `CommunicationEntryWizard.cs` line 3682 and RockWeb `CommunicationEntryWizard.ascx.cs` lines 4417-4423, both calling `Rock.Model.Communication.DetermineMediumEntityTypeId` with all three medium ids and the raw preference walk). It never constrains to the enabled mediums. That is not the divergence; the divergence is composition.

## Expected behavior (agreed)

### Communication Types block setting
The admin sees all four options (Recipient Preference, Email, SMS, Push) and may check any combination, regardless of whether each type currently has an active, configured transport and medium. No change to the setting editor.

### Configuration validation on load (admin-only error, blocks sending)
- On block load, the wizard validates that every communication type selected in the setting is fully deliverable (its medium and transport are active and configured for the sending context).
- Recipient Preference requires **both** Email and SMS to be deliverable.
- If any selected type is not deliverable, the block shows an **admin-only** error banner at the top identifying the problem, and **sending is blocked** until an administrator corrects the configuration.
- "Admin-only" means a person with Edit or Administrate on the block. Regular senders do not see the detailed error; for them the affected flow is simply unavailable and sending is blocked.

### Medium picker (sender)
- Shows the standalone options selected in the setting (Email / SMS / Push), plus the Recipient Preference option when it is selected.
- "Recipient Preference only" shows **only** the Recipient Preference option. The sender cannot pick standalone Email or SMS.
- Because misconfiguration is an error-and-block condition rather than a silent filter, the picker reflects the setting instead of hiding selected options that lack transports.

### Recipient Preference behavior
- When selected and both Email and SMS are deliverable, the sender proceeds through the Email details, then the SMS details, in sequence (never Push).
- Delivery resolves each recipient to their preferred medium (Email or SMS). Because both are always composed under Recipient Preference, there is always content for the resolved medium.
- If either Email or SMS is not deliverable, Recipient Preference is unavailable and the admin-only error plus send block applies.

### Reachable-audience count
- The Recipient Preference count reflects recipients reachable via Email or SMS (the mediums Recipient Preference delivers through), consistent with the composed mediums.

## Plan

This has grown from the original picker fix into a validation-and-composition rework: the silent transport filtering is replaced by explicit deliverability validation, and Recipient Preference composition/count is decoupled from the standalone picker.

**Server:**
- Compute, per communication type selected in the setting, whether it is deliverable (active authorized transport + medium). Recipient Preference is deliverable only when both Email and SMS are.
- Expose to the client: the selected standalone picker options; the Recipient Preference mediums (Email + SMS) for composition/count; the admin-only validation messages; and a "sending blocked" flag.
- Keep the server-side `CommunicationType` clamp (`ClampToPickableCommunicationType`) as defense-in-depth at load, save/send, and test.
- Delivery resolver unchanged.

**Client:**
- Picker shows the selected standalone options plus Recipient Preference; "Recipient Preference only" shows only Recipient Preference.
- Admin-only error banner (Edit/Administrate) listing misconfigured selected types; disable sending while any exist.
- Recipient Preference editor flow composes Email then SMS in sequence, never Push.
- Count (`personalPreferenceRecipients`) and confirmation (`viewItems`) use the Recipient Preference mediums (Email + SMS), not the standalone picker set.

**Rework of the in-flight changes on this branch:**
- Replace the transport-based filtering in `GetPickableCommunicationTypes` with setting-based options plus a separate deliverability validation that drives the error banner and send block.
- Rework `confirmationStep.partial.obs` `viewItems` to show the Recipient Preference mediums rather than the standalone `allowedMediums`.
- Drop the "Recipient Preference only auto-enables standalone Email + SMS" change in `GetAllowedCommunicationTypes`.
- Keep the server-side clamp.

## Behavior after the change (assuming healthy configuration, Email and SMS transports active)

| "Communication Types" setting | Standalone picker | RP composes | RP reachable count | RP delivery |
|---|---|---|---|---|
| Recipient Preference + Email | Email, Recipient Preference | Email + SMS | email OR sms reachable | per preference (email/sms) |
| Recipient Preference + Email + SMS | Email, SMS, Recipient Preference | Email + SMS | email OR sms reachable | per preference |
| Recipient Preference only | Recipient Preference | Email + SMS | email OR sms reachable | per preference |
| Email only | Email | n/a | n/a | email |
| (none checked) | Email, SMS, Push, Recipient Preference | Email + SMS | email OR sms reachable | per preference |

Misconfiguration cases (any selected type not deliverable, including Recipient Preference when Email or SMS is not deliverable): admin-only error banner shown, sending blocked, until an administrator fixes the configuration.

## Verification

1. Build the C# project; lint the changed `.obs` files.
2. Healthy config, per row in the matrix: confirm the standalone picker options, the Recipient Preference editor steps (Email + SMS, never Push), and the reachable-audience count.
3. Healthy Recipient Preference send to a mix of Email-preferring and SMS-preferring recipients: the Email-preferring recipient receives Email with content, the SMS-preferring recipient receives SMS with content, and no empty sends, for the "Recipient Preference + Email", "Recipient Preference + SMS", and "Recipient Preference only" configs.
4. Misconfiguration: with a selected type lacking an active transport (and separately, Recipient Preference with only one of Email/SMS deliverable), confirm the admin-only error appears for an Edit/Administrate user, does not appear for a regular sender, and that sending is blocked in both cases until fixed.
5. Regression: selecting a specific medium (Email / SMS / Push) behaves as before when its transport is healthy.

## Open questions

1. Server signal shape for the Recipient Preference mediums and the validation result: reuse the existing `Mediums` list shape plus a separate validation/error field, or a small dedicated bag. To be settled during implementation, mirroring existing patterns. (Marked TBD in review.)

## Decisions captured in review

- Recipient Preference + Email (SMS excluded as a standalone option) again delivers SMS to recipients who prefer SMS. This matches pre-v19 behavior; #6938's concern was the standalone "SMS to everyone" option, which the picker change removes.
- Misconfiguration blocks sending (does not merely warn) and surfaces an admin-only error.
- Recipient Preference is unavailable until both Email and SMS are deliverable (stricter than `98c5308c9a`, which allowed Email-only Recipient Preference when SMS was not active).
- Recipient Preference always walks the sender through Email then SMS when both are deliverable.
- The admin-only configuration error uses the block-level error-message plus a top `NotificationBox`, matching `communicationDetail` and `chatConfiguration`. Users with Administrate on the block see the detailed message; everyone else sees a generic "contact your administrator" message. Sending is disabled while any selected type is not deliverable.
- The reachable-audience and medium logic is internal to this block, so the count and filtering changes are self-contained with no downstream consumer.

## Status

A focused fix for the reported #6938 symptom shipped on branch `claude/issue-6938-communication-v19.5` and is intentionally smaller than this spec. It keeps Recipient Preference composing and delivering Email and SMS exactly as before (no empty sends), and only narrows the standalone picker options to the types the admin explicitly enabled. Implementation:

- New server signal `CommunicationEntryWizardInitializationBox.StandaloneMediums`, built by `GetStandalonePickerMediumBags` (the block-setting picker set, transport-gated; Recipient Preference offered whenever it is enabled and Email or SMS has an active transport).
- The wide `Mediums` list is unchanged and still drives composition, counts, confirmation, and delivery.
- The client medium cards and the medium validation in `wizardStartStep.partial.obs` key off the new `standaloneMediums`; everything else continues to use the wide `allowedMediums`.

This spec remains the record for the **deferred** enhancements, which are not implemented: the admin-only configuration error banner, the send-block, and treating Recipient Preference as unavailable until both Email and SMS are deliverable. Those would be a separate change.

Separately flagged (not part of #6938): the `wizardStartStep` "Not Bulk" classification card is gated on Email being an allowed medium, which looks like a stray copy/paste; captured as its own follow-up task.
