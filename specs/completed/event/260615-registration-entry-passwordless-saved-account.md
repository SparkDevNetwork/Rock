---
title: Hide the Registration Entry saved-account option when no usable login method exists
issue: https://github.com/SparkDevNetwork/Rock/issues/6877
ticket: DEV-13533
domain: Event
version: 19.2
status: ready
author: JMH
created: 2026-06-15
---

# Registration Entry: gate the anonymous saved-account option

## Problem

In the Registration Entry block, an anonymous (not-logged-in) registrar who opts to
"Save account information for future payments" on the success step is forced to create a
Database (username/password) `UserLogin`. On an organization configured for Passwordless
authentication only (Database auth disabled or removed), that Database login is unusable: the
person can never sign in with it, so the credential the flow just forced them to create cannot
serve its stated purpose. The result is an orphaned, non-functional login plus a saved account
the customer cannot reach.

This is a catch-22 for Passwordless-only orgs:

- Leave "Enable Saved Account" on → anonymous registrars are forced to create a Database
  login, contradicting the org's Passwordless strategy.
- Turn "Enable Saved Account" off → no registrar can save a payment method at all.

This is long-standing behavior, not a regression. Registration checkout is the only place in
core Obsidian that surfaces the shared `SaveFinancialAccountForm` control. Only the
anonymous-saver path is affected: a logged-in registrar saves with no login-creation step at
all.

## Current behavior (root cause)

- The save endpoint hardcodes Database authentication and requires a username and password for
  anonymous savers ([ControlsController.cs:10394](../../../Rock.Rest/v2/ControlsController.cs),
  [ControlsController.cs:10462](../../../Rock.Rest/v2/ControlsController.cs)).
- The success step shows the save option whenever the gateway identifiers are present and the
  Enable Saved Account block setting is on, with no regard for whether an anonymous saver could
  ever use the resulting login
  ([success.partial.obs:19](../../../Rock.JavaScript.Obsidian.Blocks/src/Event/RegistrationEntry/success.partial.obs)).
- A logged-in saver already skips login creation entirely: the form shows only an account-name
  field and the account attaches to the registrar. Only the anonymous path forces a Database
  login, so that is the only case that needs gating.

## Decision

Do not build Passwordless account creation into Registration Entry. Instead, suppress the
"Save account information for future payments" option in exactly the situation where it cannot
work, so no one is forced to create an unusable login.

Hide the save option only when it would force an unusable login: when the registrar is
anonymous (not logged in) and Database authentication is disabled. Equivalently, show the
option when either holds:

- the person is logged in: an authenticated save never creates a login (Rock creates one only
  for anonymous savers), so there is no unusable credential to worry about, OR
- Database authentication is enabled: the existing anonymous username/password flow works.

Testing confirmed the saved account always attaches to the registrar regardless of who is
logged in, and that login creation only happens for anonymous savers, so being logged in at
all is sufficient. Whether the current person is the registrar does not affect attachment and
is not part of the condition.

## Why not add Passwordless account entry (out of scope)

Adding Passwordless account creation to the anonymous save path is a feature request, not part
of this bug fix, and is materially larger with risks beyond it:

- It introduces a new account-takeover attack vector that would have to be designed against.
- It needs duplicate-person prevention.
- It needs a multi-step verification process to guarantee the payment method is saved to the
  correct account.

Those belong in a dedicated feature, not this hotfix.

A lighter future option, if the feature is ever requested, would be to mirror the authenticated
flow on Passwordless-only sites: show only the account-name field, save to the registrar, and
create no login at all (no Passwordless login, no verification). This is also deferred, not
part of this fix.

## Approach

- The Registration Entry block computes whether the save option is supported and passes it to
  the success step, which gates `SaveFinancialAccountForm` on it together with the existing
  Enable Saved Account block setting and the gateway identifiers.
- "Database authentication enabled" is determined via
  `AuthenticationContainer.GetComponent( <AUTHENTICATION_DATABASE guid> )?.IsActive` (pattern
  per [Login.cs:1598](../../../Rock.Blocks/Security/Login.cs)).
- "Logged in" is read from the current person; the condition does not depend on whether the
  current person matches the registrar, since saving attaches to the registrar regardless. In
  practice the gate reduces to `EnableSaveAccount && ( currentPerson != null ||
  isDatabaseAuthEnabled )`, where only `isDatabaseAuthEnabled` needs to come from the block.
- The shared `SaveFinancialAccountForm` control is unchanged.
- As defense in depth, the `SaveFinancialAccountFormSaveAccount` endpoint also enforces this
  server-side: its anonymous branch refuses to create a login when Database authentication is
  inactive. The client gate only hides the option, so this stops a direct API caller (bypassing
  the UI) from creating an orphaned, unusable Database login.

## Scope

In scope:
- Visibility of the saved-account option in the Registration Entry success step.
- A server-side guard in the save endpoint that refuses anonymous Database-login creation when
  Database authentication is inactive.

Out of scope:
- Passwordless account creation in Registration Entry (feature request).
- The shared `SaveFinancialAccountForm` control.
- Any change to logged-in or Database-auth behavior.

## Backward compatibility

Orgs with Database auth enabled see no change: the option still shows and the existing flow is
untouched. Logged-in registrars see no change. Only the broken combination (anonymous saver,
Database auth disabled) changes: the option is hidden rather than forcing an unusable login.

## Edge cases

- Anonymous registrar, Database disabled, Passwordless enabled → option hidden (the reported
  case).
- Logged-in registrar, Database disabled → option shown; as in today's authenticated flow,
  only the account-name field appears, no login is created, and the account saves to the
  registrar.
- Database enabled (anonymous or logged in) → option shown; the existing flow is unchanged.

## Testing

- Test Gateway (`Rock.Financial.TestGateway`) on a registration with a cost.
- Anonymous, Database disabled, Passwordless enabled → confirm the save option does not appear.
- Anonymous, Database enabled → confirm the option appears and the existing flow is unchanged.
- Logged in, Database disabled → confirm the option appears, only the account-name field shows,
  no login is created, and the account saves to the registrar.

## Follow-up

- After the fix, add a comment to GitHub issue #6877 stating what changed (the option is hidden
  when no usable login method exists) and why (full Passwordless account entry is a separate
  feature, with account-takeover, duplicate-prevention, and multi-step-correctness concerns).

## Open Questions

None.
