---
author: Carter Henning
date_created: 2026-06-25
summary: >-
  The Obsidian grid's Communicate action builds its redirect URL with a
  {CommunicationId} placeholder. On sites whose communication page has no
  {CommunicationId} route, BuildUrl() puts the token in the query string and
  encodes the braces to %7b/%7d, so the client-side replace fails and the
  communication opens with no recipients. Switching the token to
  ((CommunicationId)), matching the existing ((Key)) navigation-URL convention,
  fixes it everywhere because parentheses are URL-safe.
contributors: []
---

# Grid Communicate URL Placeholder Encoding

## Summary

While converting the Fundraising Leader Toolbox block to Obsidian, I noticed an issue with the grid's Communicate URL builder on external sites. If a site's communication page has not been configured with a route that contains `{CommunicationId}`, the Obsidian grid builds a broken redirect URL when Communicate is clicked. The `{` and `}` in the query parameter get URL-encoded to `%7b`/`%7d`, so the client-side JavaScript never matches the placeholder and the communication opens with no recipients.

The fix is to follow the navigation-URL pattern and use `((CommunicationId))` instead of `{CommunicationId}`, since `(( ))` does not get encoded by `BuildUrl()`. This is the same convention Obsidian navigation URLs already use with `((Key))`.

## Motivation

I found this on the external website because the Fundraising Leader Toolbox block lives there. On the internal admin site Communicate worked fine; on the external site the same action redirected to a broken URL (`/?CommunicationId=%7bCommunicationId%7d`) and the communication opened with no recipients.

At first it looked like a conversion regression, but it is not. It is a latent issue in the shared Obsidian grid machinery. It affects every Obsidian grid with a Communicate action whenever the target communication page lacks a `{CommunicationId}` route. The internal site has that route by default, which is why the issue is easy to miss. WebForms never required the route, so the discrepancy is not obvious.

The current workaround (add a page route, point the site setting at it, restart Rock) is non-obvious, environment-specific, and required for something that should work out of the box.

## Problem Statement

The Obsidian grid Communicate action produces a redirect URL containing an encoded placeholder (`%7bCommunicationId%7d`) instead of a usable token whenever the site's communication page is not configured with a route carrying `{CommunicationId}`. The client-side substitution then fails, so the destination communication page loads with zero recipients (and in the fully-unrouted fallback the redirect lands on the site root).

## Reproduction

1. On a site whose configured Communication page has **no** page route containing `{CommunicationId}` (a plain page reached as `/page/{id}`), place any Obsidian block whose grid exposes the Communicate action with a `personKeyField` set.
2. Select one or more rows and click **Communicate**.
3. Observe the redirect URL. It contains `CommunicationId=%7bCommunicationId%7d` (the literal placeholder, percent-encoded) rather than a real id.
4. The destination communication page opens with no pre-filled recipients.

Affected configuration: any site whose `CommunicationPageReference` resolves to a page without a `{CommunicationId}` route. The default internal site is **not** affected because its communication page is routed; external/custom sites frequently are.

## Root Cause

The grid builder inserts `{CommunicationId}` as the route parameter value before calling `BuildUrl()`. The URL is assembled at **page-render time**, before any communication exists, so the server cannot embed a real id and instead embeds a placeholder token. `BuildUrl()` then checks whether a route exists. If it does, the token rides through the route path unencoded. If it does not, the token goes into the query string and the braces are encoded to `%7b`/`%7d`. The client-side replace, which looks for the literal `{CommunicationId}`, then matches nothing.

Producer, `GetCommunicationRoute` in `Rock/Obsidian/UI/GridBuilderExtensions.cs:476`:

```csharp
pageRef.Parameters.AddOrReplace( "CommunicationId", "{CommunicationId}" );
return pageRef.BuildUrl();
```

Inside `PageReference.BuildUrl()` (`Rock/Web/PageReference.cs`), the value's fate depends on whether a route is found:

- **Route path branch**, `Rock/Web/PageReference.cs:581`: substituted via `HttpUtility.UrlPathEncode(...)`, which leaves `{` and `}` intact. The token survives, so routed pages work.
- **Query-string branches** (`Rock/Web/PageReference.cs:436` in the `page/{id}` fallback, and `Rock/Web/PageReference.cs:593` for leftover params in `BuildRouteURL`): emitted via `HttpUtility.UrlEncode(...)`, which encodes `{`/`}` to `%7b`/`%7d`. The token is destroyed.

Consumer, `onCommunicate` in `Rock.JavaScript.Obsidian/Framework/Controls/Grid/grid.partial.obs:1431`:

```ts
const finalUrl = makeUrlRedirectSafe(url.replace("{CommunicationId}", `${result.data}`));
```

This is a literal string replace. When the URL contains `%7bCommunicationId%7d`, the replace matches nothing and the encoded placeholder rides through to the browser.

The underlying issue is a layering seam: the code that knows `{CommunicationId}` is a token (`GetCommunicationRoute` and the client replace) is not the code that does the encoding (`BuildUrl`, a generic shared utility). `BuildUrl` correctly encodes query values because they are normally real data; it cannot know this one value is meant to pass through verbatim.

### Why WebForms does not have this problem

The legacy grid builds the URL **after** creating the communication, with the real id already known, so there is no placeholder to preserve. `Rock/Web/UI/Controls/Grid/Grid.cs:2067`:

```csharp
pageRef.Parameters.AddOrReplace( "CommunicationId", communication.Id.ToString() ); // e.g. "42"
url = pageRef.BuildUrl();   // ?CommunicationId=42  — a plain integer, encoding is a no-op
```

A query-string parameter with a real integer is valid and needs no route. The timing difference (build-after-create vs build-template-then-substitute) is the entire reason Obsidian needs the route and WebForms does not.

### Why parentheses fix it

`HttpUtility.UrlEncode` treats `(` and `)` as URL-safe and does not encode them; `{` and `}` are not safe and are encoded. `UrlPathEncode` also leaves parentheses intact. So a `((CommunicationId))` token survives **both** the path substitution and the query-string fallback. This is exactly why Obsidian's grid navigation URLs already use `((Key))` rather than `{Key}`; the Communicate token is the lone deviation from that established convention.

## Affected Code Paths

Primary (the fix lands here):

- `Rock/Obsidian/UI/GridBuilderExtensions.cs:476` — producer, token value in the routed/site-configured branch.
- `Rock/Obsidian/UI/GridBuilderExtensions.cs:483` — producer, token value in the no-site literal fallback (`/Communication/{CommunicationId}`).
- `Rock.JavaScript.Obsidian/Framework/Controls/Grid/grid.partial.obs:1431` — consumer, the client-side `replace`.

Must NOT change:

- `Rock/Obsidian/UI/GridBuilderExtensions.cs:481` — `IsAuthorizedForRoute( ..., "/Communication/{CommunicationId}" )` resolves the actual route pattern for an authorization check. This is route syntax, not a client token, and must stay `{CommunicationId}`.
- All page route definitions in migrations (e.g. `AddOrUpdatePageRoute( ..., "Communication/{CommunicationId}", ... )`). `{param}` is required ASP.NET/Rock route syntax and is unrelated to the substitution token.

Confirmed independent (audited, unaffected):

- EntitySetId-based actions (Merge Person/Business, Bulk Update, Merge Template, Launch Workflow, custom routes) use a separate `{EntitySetId}` token built from literal route strings that never pass through `BuildUrl`, so they are never encoded (`Rock.JavaScript.Obsidian/Framework/Controls/Grid/grid.partial.obs:1456`, `:1963`).
- Legacy WebForms grids (`Rock/Web/UI/Controls/Grid/Grid.cs`) build the URL with a real id and use no token.
- Mobile shell has no grid-communicate token usage.
- The destination Communication Entry block reads the `CommunicationId` page parameter as a plain integer after the client has already substituted it.

## Workarounds

User-side, available today without a code change:

1. Add a page route ending in `{CommunicationId}` to the site's communication page (must be globally unique, e.g. `external/Communication/{CommunicationId}`).
2. Point the site's **Communication Page** setting at that page and select the route.
3. Restart Rock so the new route registers in the live `RouteTable.Routes` (a cache clear is not sufficient; routes register at application startup).

This works because the token then lands in the route path (un-encoded) instead of the query string. It is non-obvious, must be repeated per site, and the restart requirement is a common stumbling block.

## Proposed Fix

Follow the navigation-URL pattern and use `((CommunicationId))` instead of `{CommunicationId}` in the producer and the consumer, so the token survives URL encoding in every branch and no route is required. This aligns the Communicate token with the `((Key))` convention already used by grid navigation URLs.

- `Rock/Obsidian/UI/GridBuilderExtensions.cs:476`:
  ```csharp
  pageRef.Parameters.AddOrReplace( "CommunicationId", "((CommunicationId))" );
  ```
- `Rock/Obsidian/UI/GridBuilderExtensions.cs:483`:
  ```csharp
  return "/Communication/((CommunicationId))";
  ```
- `Rock.JavaScript.Obsidian/Framework/Controls/Grid/grid.partial.obs:1431`:
  ```ts
  const finalUrl = makeUrlRedirectSafe(url.replace("((CommunicationId))", `${result.data}`));
  ```

Only the parameter **value** changes; the parameter **key** stays `"CommunicationId"`, so route selection (which matches on the key) is unaffected and routed pages continue to resolve their route exactly as before. The route definitions themselves are untouched.

Add an engineering note at line 476 explaining the `(( ))` convention and why line 481 intentionally keeps `{CommunicationId}`, so the adjacent mismatch is not "corrected" by a future contributor.

A prototype of this change is applied in the working branch for the Fundraising Leader Toolbox conversion and has been verified in both routed and unrouted configurations.

## Fix Risks

- **Atomic deploy required.** The producer (C#) and consumer (TypeScript) must ship in the same build. A C#-only or JS-only deploy would mismatch the token and break Communicate for every Obsidian grid. Low risk in practice since both live in core and ship together, but worth stating.
- **Third-party / plugin code** that reimplemented its own grid Communicate handler and hard-coded the literal `{CommunicationId}` would break. Very low risk: `GridActionUrlKey` is `internal` and the token is an undocumented implementation detail, not a public contract.
- **No data migration, no config change, no restart.** The token is internal (emitted by the server, replaced by the client, never persisted), so there is no schema or stored-data impact and no backward-compatibility surface beyond the two code points above.

## Verification Steps

1. **Unrouted site (the bug):** on a site whose communication page has no `{CommunicationId}` route, click Communicate from any Obsidian grid with rows selected. Confirm the redirect URL contains `CommunicationId=((CommunicationId))` before substitution and `CommunicationId=42` (a real id) after, and that the communication page opens with the selected recipients.
2. **Routed site (regression check):** on the internal site (communication page routed as `Communication/{CommunicationId}`), repeat. Confirm the final URL is still `/Communication/42` and recipients are pre-filled, identical to pre-change behavior.
3. **Selected vs all:** confirm selecting specific rows sends only those recipients, and selecting none sends all rows (existing behavior, unchanged).
4. **Adjacent actions:** confirm Merge Person, Bulk Update, Merge Template, and Launch Workflow still work (they use the independent `{EntitySetId}` token and should be unaffected).
5. **Legacy grid:** confirm a WebForms grid's Communicate still works (separate code path, should be untouched).

## Out of Scope

- The "added route requires an app restart to register in `RouteTable.Routes`" behavior. This is a separate, pre-existing characteristic of Rock route registration and is not addressed here.
- The legacy WebForms grid's Communicate visibility heuristic (`CanViewTargetPage` round-trip), which hides the button when the communication page is routed in certain ways. Legacy-only; not changed.
- Any change to `PageReference.BuildUrl()` encoding behavior. The fix deliberately avoids touching the shared URL builder.

## Considered but Rejected

### Require a page route on every communication page (the current workaround)
Rejected as the permanent answer. It works but pushes non-obvious, per-site setup onto every install (route + site setting + restart) for behavior that should be automatic. It also only masks the underlying token-encoding mismatch.

### Build the URL without the token, then append `{CommunicationId}` after `BuildUrl`
Rejected. If `CommunicationId` is not supplied as a parameter, `BuildUrl` cannot satisfy the route's `{CommunicationId}` parameter, so routed pages fall back to `page/{id}` and lose their clean route URLs (`/Communication/42` becomes `/page/362?CommunicationId=42`). Functional, but a regression for every currently-routed communication page.

### Decode the encoded token after `BuildUrl` (normalize `%7b...%7d` back to `{...}`)
Rejected as the shipped fix, though it is correct in shape. It hard-codes an assumption about the encoded form (lowercase `%7b`, exact casing), patches the output of a shared utility, and is more fragile than simply using a token that never gets encoded. Useful only as a temporary per-block stopgap.

### Change `BuildUrl` to skip encoding for placeholder-looking values
Rejected. Special-casing a generic, widely-used URL builder to recognize `{...}` tokens is invasive, risks unintended effects on every caller, and inverts the correct default (encode query values). The token should adapt to the builder, not the reverse.

## Related

- WebForms producer for comparison: `Rock/Web/UI/Controls/Grid/Grid.cs:2067`.
- Existing `((Key))` navigation-URL convention this change aligns with: `Rock.JavaScript.Obsidian/Framework/Controls/Grid/grid.partial.obs` (navigation handlers) and per-block `GetBoxNavigationUrls` usages such as the Fundraising Leader Toolbox conversion (`((Key))`).
- Route selection and encoding mechanics: `Rock/Web/PageReference.cs:414` (route selection), `:539` (`BuildRouteURL`), `:581` / `:593` (path vs query encoding).
