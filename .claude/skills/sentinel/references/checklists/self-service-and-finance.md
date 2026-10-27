# Self-Service and Finance

Load when the code touches public or self-service blocks, person matching, person tokens,
giving, saved accounts, scheduled transactions, pledges, assessments, or reminders.

Entity security doesn't express "this is mine." The financial entities have no ownership
logic in `IsAuthorized`, and a giver usually has no VIEW rule on their own records. These
checks are about ownership, not security roles.

## Ownership

- **The record is theirs.** Every record a person views or acts on in a self-service block is
  theirs: the record's `PersonAlias.PersonId` matches the current (or target) person. For
  ambiguous types, the record's type matches too (for example `AssessmentTypeId`).
- **Giving records.** For `FinancialScheduledTransaction` and `FinancialTransaction` loaded
  from client input, the owner's GivingId is the current person's GivingId or one from
  `PersonService.GetBusinesses( personId )`. A matching PersonId alone is not enough, and
  neither is a match to any family member.
- **Saved accounts** match by person: `PersonAlias.PersonId` equals the current person. The
  saved account also matches the block's gateway, and when editing a scheduled transaction it
  belongs to that transaction's `AuthorizedPersonAlias`.
- **Leader-only actions** (for example the Fundraising Leader Toolbox) re-check the leader role
  in the action, not only on page load.

## Person matching

- **A matched person is not signed in.** A person found by matching (anonymous gift, form
  entry, pre-registration) can see and manage only the records created in that session (for
  example the scheduled transaction created on that page), not the matched person's other
  records. A retry or receipt path that loads an existing transaction by a posted Guid checks
  that its GivingId matches the giver.
- **Don't overwrite the matched person.** A submission matched to an existing person without
  sign-in doesn't overwrite that person's profile (email, phone, photo, address).
  `PersonService.FindPerson( query, updatePrimaryEmail: true )` changes the matched person's
  email, so flag it on anonymous paths. A posted business or family member is used only if it
  is in the list the block offered.

## Person tokens

- **A token is not a sign-in.** A person found from a token (`rckipid`) is not signed in. Use
  `PersonService.GetByImpersonationToken( token, true, pageId )` so the usage limit, expiry,
  and page binding apply. The one-argument overloads skip all three.
- **Action identifiers never expire.** A person action identifier (`rckid`,
  `GetByPersonActionIdentifier`) only unlocks low-risk actions for that one person
  (unsubscribe, opt out, a single assessment). It never reveals giving, saved accounts, or
  other people.

## Inputs and side effects

- **Validate first.** Validate every input before any side effect such as creating Person
  records or charging a payment.
- **Gift lines are checked on the server.** Each account is in the block's allowed list
  (active, `IsPublic` unless private accounts are allowed, inside its start and end dates).
  Each amount is positive, and the frequency is an active Transaction Frequency defined value.
  The total charged is the server's sum of those lines, never a posted total.
- **Sender email.** Emails sent from a public form use the current person's email or the
  block's configured From address as the sender, never a posted value.
- **Throttle guessable codes.** Public actions that send or check short codes (sign-in,
  verification, pairing) limit attempts per IP or per person, as passwordless sign-in does with
  `ValidateIpCountWithinLimits`.

## Public pages

- **No person search for anonymous visitors.** No person picker or person search is reachable
  by anonymous visitors. On a public workflow form, a visible Person field searches the whole
  person database. Collect people with Person Entry instead.
