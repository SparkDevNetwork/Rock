---
author: Jon Edmiston
date_created: 2026-08-17
summary: >-
  An AI agent skill that reads the Rock community knowledge base over HTTP.
  Seven read-only tools across three remote stores: hybrid keyword and semantic
  search over documentation and community content, semantic and literal search
  over the Rock source, and progressive disclosure of curated topic trees. The
  overview endpoint is a prerequisite for the rest and is the source of every
  valid filter value. Category scope is the skill's only setting, and its picker
  populates from the remote managed lists so it never drifts from the server.
  The host is fixed and the organization id is resolved from Rock, so neither is
  configured.
contributors: []
---

# Community Knowledge Base Agent Skill

## Summary

Seven read-only tools that query the Rock community knowledge base, a remote HTTP service
holding three separate stores. Nothing in this skill touches the Rock database except to
read its own configuration and its own version number.

This is the first agent skill whose data lives outside Rock. It assumes the
[shared tool conventions](260807-ai-agent-tool-conventions.md) and notes its departures,
of which there are several, because those conventions were written against `IQueryable`
over Rock entities and most of them do not survive the move to a remote API.

Companion in spirit to the [Core Administration skill](260807-ai-agent-core-administration-skill.md):
that one tells an agent what is configured in *this* Rock, this one tells it how Rock works
in general.

## Motivation

The knowledge base already holds the material. The problem this skill solves is not access,
it is **routing**. Three stores answer three different kinds of question, and an agent that
picks wrong does not fail loudly. It settles for a partial answer from the wrong store and
presents it with full confidence.

That is why the overview endpoint is not one tool among eight. It is the tool that makes the
other seven work, and the design below spends more effort forcing it to be called first than
it spends on any single search.

The second silent failure is filter values. A category, domain, or source the server does not
recognize is not an error. It returns an empty result that is indistinguishable from a search
that genuinely found nothing, so the agent rephrases the query instead of fixing the filter,
and keeps rephrasing. Every filter is therefore validated locally before the request leaves
Rock.

## Requirements

- The skill MUST validate every filter value against the remote managed lists before sending
  a request, and MUST return an error naming the valid values rather than passing an unknown
  value through to an empty result.
- An error raised by that validation MUST point the agent at `GetKnowledgeBaseOverview`,
  which reports the valid values for this deployment's actual corpus. Naming the values in
  the error is what lets the agent correct itself; naming the tool is what stops it guessing
  again on the next call.
- Every tool that accepts a Rock version MUST supply it from the running Rock instance. The
  agent MUST NOT be given a version parameter.
- The category picker in skill configuration MUST populate from the remote managed lists. When
  the remote is unreachable it MUST show nothing. There is no compiled-in fallback list.
- Every filter MUST be OR. Multiple values for one field are sent as a single comma-joined
  parameter meaning "any of these". No tool MAY emit a repeated filter parameter, which the service
  reads as AND.
- The configured category scope MUST be applied to every knowledge search, and the skill MUST
  state plainly where that scope does not reach.
- `GetKnowledgeBaseOverview` MUST be declared a prerequisite of every search tool, at both the
  skill and the tool level.
- A retrieval key for a topic or article MUST NOT be constructible by the agent. Keys are only
  ever taken from a table of contents or a parent article.
- Remote error detail MUST be surfaced, not swallowed. The service writes its `detail` strings
  to name the valid values, which is usually enough for a model to correct itself in one turn.
- A `429` MUST NOT be retried inside the tool.
- No tool MAY write. The remote API is read-only and this skill has no write surface.

## Design

### Skill declaration

```csharp
[Description( "Provides access to the Rock community knowledge base: product documentation, community content, the Rock source code, and curated topic guides." )]
[AgentSkillName( "Community Knowledge Base" )]
[AgentPurpose( "Answers questions about how Rock RMS works, from documentation, community content, curated guides, and the Rock source." )]
[AgentUsage( "Always call GetKnowledgeBaseOverview before the first search in a conversation. It reports what the knowledge base actually holds and which store answers which kind of question. Searching without it means guessing." )]
[AgentUsage( "Prefer SearchKnowledge for almost every question. Reach for the code tools only when the question is about implementation detail, or when SearchKnowledge has already failed to answer it." )]
[AgentSkillGuid( "DFCBFDE8-6BF2-4DDF-81FE-FDD436E5FD90" )]
[EntityTypeGuid( "959F0B92-A3BB-4AAA-9143-CF7D77895392" )]
internal sealed partial class CommunityKnowledgeBaseSkill : AgentSkillComponent
```

`[AgentSkillName]` is set explicitly because the class name reads as `CommunityKnowledgeBase`
and the orchestrator uses the skill name to decide whether these tools are relevant. Three
words beat one run-together one.

### The three stores, and why routing is the whole problem

| Store | Answers | Tools |
|---|---|---|
| **Knowledge** | "How does this work?", "What is the recommended way to?" Hybrid keyword and semantic search over documentation, guides, and community content. | `SearchKnowledge` |
| **Code** | "How does Rock actually implement this?" Semantic search, then literal grep, then read the lines. | `SearchCode`, `GrepCode`, `GetCodeLines` |
| **Topics** | "I do not know enough to know what to search for." Hand-curated trees, walked progressively. Never returned by search. | `GetTopic`, `GetArticle`, entered from the overview |

The routing rules, stated once here and repeated in the tool annotations:

1. **Knowledge is the default.** Most questions are answered there and carry a citation.
2. **Code is a second move, not a first.** Use it when the question is genuinely about
   implementation, or after knowledge search came back thin. A general question pushed into
   the code index returns plausible files and no answer.
3. **Topics are for orientation.** When the right search terms are not known yet, walk a topic
   tree rather than guessing at queries.

The overview endpoint reports which sources exist, how many documents each holds, which Rock
releases have code indexed, which topics are published, and operator-written guidance on when
to use which store. It is the only thing that turns those three rules from generic advice into
a decision about this deployment's actual corpus.

### 1. Configuration

**One setting.** The host is a compiled-in constant and the organization id is resolved from
Rock, so neither is configurable and neither can be typed wrong.

```csharp
private const string ApiHost = "https://knowledge.rockrms.com";

private static class ConfigurationKey
{
    public const string Categories = "categories";
}
```

| Setting | Type | Required | Notes |
|---|---|---|---|
| Categories | multi-select | no | Empty means every category. |

Configuration is a hand-rolled dynamic component rather than a `FieldAttribute` declaration,
following `PrayerSkill`. The picker has to be populated from a live HTTP call, and the base class
builds its bags from static attribute metadata, so the two cannot be mixed. Overriding
`GetComponentDefinition` replaces the whole surface.

#### The organization id is resolved, not configured

It is a UUID version 5 hash of the Rock organization GUID, computed under a namespace each
knowledge base deployment holds privately. **Rock cannot compute it.** It will be available from
Rock directly, at all times; the exact source is being decided separately and is recorded as an
open question below.

**When it cannot be read, send the empty GUID**, `00000000-0000-0000-0000-000000000000`. Not an
error, not a refusal, not a prompt. Log it once at debug level and carry on.

That is safe because of what the id is and is not:

1. **It is not a credential.** It authenticates nothing. Read endpoints are open, with no key, no
   sign-in, and no cookie. Do not build a permission model around it and do not store it
   encrypted.
2. **It is not verified.** The service does not check that an id corresponds to a real
   organization. Any well-formed UUID is accepted and recorded as an unknown organization.
3. **It exists for analytics and rate limiting.** Attribution and a per-organization quota, and
   nothing else.

So a missing id costs correct attribution and nothing else. Every result is still correct. A skill
that refused to answer over an analytics value would be trading the entire feature for a metric,
and the empty GUID is a legible "not supplied" marker on the receiving end rather than a
plausible-looking wrong one.

**The only hard rule is the shape.** A malformed id is rejected with a `400` of type
`invalid-organization`, before rate limiting and before logging. Send the bare hyphenated
8-4-4-4-12 form, lowercased. No braces, no `urn:uuid:` prefix. The empty GUID satisfies this.

**Do not derive it.** The namespace is per-deployment and not public. The skill must never accept
a Rock organization GUID and hash it, and must never offer to.

#### The category picker populates from the server

**This is possible, and it is the design.** The skill overrides `GetComponentDefinition` to fetch
`GET /{org}/managed-lists` and passes `data.categories` down as component options:

```csharp
public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
{
    return new DynamicComponentDefinitionBag
    {
        Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/AI/Skills/communityKnowledgeBaseSkill.obs" ),
        Options = new Dictionary<string, string>
        {
            ["categories"] = GetCategoryOptions().ToCamelCaseJson( false, false )
        }
    };
}
```

No parameters are threaded through, because there is nothing left to thread. The host is a
constant and the organization id resolves itself, so the fetch always has everything it needs and
works on a brand new skill instance with nothing filled in. This is what removing the two settings
bought, and it is why `ExecuteComponentRequest` is not needed at all.

`managed-lists` takes no parameters beyond `{org}`, no paging, and no version scoping, and every
key is always present, so an unconfigured list is an empty array rather than a missing key. That
makes it cheap enough to call on every render of the configuration screen.

**No fallback list.** When the fetch fails, for any reason, the picker shows nothing and the
component displays an inline warning saying the category list could not be retrieved. A
compiled-in list was specced and removed: a stale hard-coded set is worse than an empty one,
because an operator who picks from it gets a scope that silently matches nothing on the server,
and an empty picker is an obvious problem that gets reported.

An operator who saves with no categories selected gets the same behavior as selecting all of them,
which is the correct outcome for a configuration screen that could not reach the server.

**Display transform.** Values are stored raw, exactly as the server returns them. The picker shows
them with hyphens replaced by spaces and each word capitalized, so `product-documentation` displays
as *Product Documentation*. The transform is display only and never applied to a value sent back to
the server.

`GetPublicConfiguration` and `GetPrivateConfiguration` pass the one key straight through, stored as
a comma-delimited string of raw slugs. Follow `PrayerSkill`.

#### Where the category scope does not reach

Say this out loud in the setting's help text, because the setting reads like a security boundary
and is not one:

- **Knowledge search** is scoped. Every `SearchKnowledge` call sends the configured set as
  `filter_category`.
- **The overview** is scoped, but only in what it *describes*. The service applies `categories`
  to the corpus description and not to any later search.
- **Code search is not scoped at all.** Code files carry no categories. There is no way to
  express the restriction and the API does not offer one.
- **Topics are not scoped.** They are browsed by key, not searched, and the browse routes take
  no category filter.

An organization that restricts to `product-documentation` still has the whole source tree and
every published topic available. The setting narrows what search returns; it does not hide
anything. If hiding is what is wanted, turn off the code and topic tools on the skill instead.

### 2. Transport

One static `HttpClient`, held on an internal helper class rather than on the skill, because a
skill instance is constructed per request and a per-instance client exhausts sockets. Follows
`AzureBlobStorageClient`.

```csharp
internal sealed class CommunityKnowledgeBaseClient
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds( 30 )
    };
}
```

Thirty seconds because semantic search over a large corpus is not instant and a ten second
timeout turns a slow answer into a wrong one.

**Base path.** `https://knowledge.rockrms.com/api/v1/{org}/...`. The host is fixed and compiled
in. The API version lives in the path, so a breaking change ships as `/api/v2` and this skill
keeps working until it is deliberately moved.

**Response envelope.** Every JSON route returns `{ "data": ..., "meta": { ... } }`. `meta`
carries paging, totals, and the resolved values of anything left unset, including the Rock
version actually applied. Read `meta` and surface the parts that matter through
`.WithMetadata()`.

#### The OpenAPI document is not authoritative for response shapes

**Verify every field name against a live response, not against
`https://knowledge.rockrms.com/api/v1/openapi.json`.**

The document and the service are known to disagree. The service's own contract test asserts only
that every route *appears* in the document; nothing checks that a response matches its documented
shape, so drift accumulates silently and has already produced at least one live mismatch, on the
code document id at tool 3.

This is not a criticism of the document, which is accurate about routes, parameters, status codes,
and error types. It is a scoping note: it is verified for the request side and unverified for the
response side, and a C# result class is entirely response side.

Practically, that means a generated client is the wrong starting point for these result classes.
Curl the route, read what comes back, map that. Where this spec names a response field, it was
confirmed against the running service; where it names a parameter or an error type, the document is
fine.

**Errors** are RFC 9457 `application/problem+json` with `type`, `title`, `status`, and `detail`.

| Condition | Result |
|---|---|
| `400` type `invalid-organization` | Should be unreachable, since the id is either resolved from Rock or is the empty GUID, and both are well formed. Log it as a defect in the resolver and return an `Error` saying the failure is not something rephrasing can fix. |
| `400` type `unknown-rock-version` | `Error` carrying `detail` verbatim; it lists the versions held. Should be unreachable on content routes, since the version is checked against `rock_versions` first. On an infrastructure route it means a version was sent that should not have been; see Section 3. |
| `400`, unknown filter | `Error` carrying `detail` verbatim. The service lists the valid values in it. |
| `400` type `invalid-code-document` | A `documentId` that is not a well-formed GUID. `Error` instructing the agent to use a `DocumentId` exactly as returned by `SearchCode` or `GrepCode`. |
| `404` type `code-document-not-found` | A well-formed id that no longer resolves. `NoData`, with instructions to re-run `SearchCode` or `GrepCode` for a current id. Same treatment as a stale topic or article key. |
| `404` on a topic or article key | `NoData`, with instructions never to construct a key and to re-read the source that supplied it. |
| `429` | `Error` saying the rate limit was hit. **Read the `Retry-After` header and name the number of seconds in the message.** No retry inside the tool. |
| Timeout or network failure | `Error` naming the host, plus a note that this is a transport problem and rephrasing will not help. |
| Any other non-success | `Error` with `detail` if present, otherwise status and reason. |

`detail` is surfaced rather than replaced with a friendly message. The service writes those
strings to name the valid values, and a model that sees them corrects itself in one turn.

**On `Retry-After`.** The `429` carries the seconds until the window resets. Forbidding retry
inside the tool is still right, but "do not call again immediately" is a rule the agent has to
guess at, and *"the rate limit resets in 34 seconds"* is one it can act on. Surface the number
through `.WithMetadata()` as well as in the message.

### 3. The Rock version is resolved, never asked for

**No tool takes a version parameter.** The agent does not know it, cannot validate a guess, and
a wrong guess produces either a `400` or, worse, quiet filtering.

**The service expects the major release only.** `19`, `20`. Not `19.1`, and **not `19.0`** — a
major release carries no trailing minor. Getting this wrong produces a `400` at best and quiet
filtering at worst, so it is worth stating twice.

Resolution runs once per request and is cached:

1. Read `VersionInfo.GetRockSemanticVersionNumber()` and take the major component. `19.1.2` gives
   `19`. Send it as the bare string `"19"`.
2. Check it against `rock_versions` from the cached managed lists, which holds bare majors in the
   same format. A plain string comparison, no normalizing. See Section 4.
3. **Present:** send it. This is the normal path.
4. **Absent**, meaning Rock is running ahead of what has been indexed: send the highest version the
   list does hold, and attach `.WithInstructions()` naming both numbers, for example *"the
   knowledge base holds nothing for Rock 20; these results describe Rock 19."*

Step 2 is a check, not a lookup. The format comes from Rock, not from the server's list; the list
only answers whether that version is indexed.

Step 4 is a real case and the one worth handling well. Rock ships ahead of its documentation
routinely. The honest answer, *"here is Rock 19 material, 20 is not indexed"*, beats both an empty
result and a `400`, and it tells the reader exactly how much to trust what follows.

Content tagged as "all versions" matches every request regardless of what is sent.

**The version filter behaves differently across routes**, which is a property of the service and
not of this skill:

- **Search, code, and overview routes** apply a server default when the version is omitted, and
  reject an unknown version with a `400` listing the versions held.
- **Browsing routes**, meaning `GetTopic` and `GetArticle`, apply *no filter at all* when the
  version is omitted, and treat an unknown version as omitted rather than as an error.

The skill sends the resolved version on every **content** route regardless. The difference is
documented here so nobody later "simplifies" the browse calls by dropping it and quietly changes
what those tools return.

#### Never send a version to `/managed-lists` or `/tags`

**This is load-bearing, and getting it wrong deadlocks the skill.**

Version validation runs in the shared request wrapper, so it applies to every route, including the
two that ignore the parameter entirely:

```
GET /managed-lists?rock_version=99  ->  400 unknown-rock-version
```

Step 2 above reads `rock_versions` from `/managed-lists` in order to discover whether the running
Rock version is indexed. If that call carried the version, then on the one occasion the check
exists for, Rock running ahead of the corpus, the call needed to detect it would be the call that
fails. The skill would report a transport error instead of *"Rock 21 is not indexed, here is 20."*

So both infrastructure routes are called **with no version, always**. `/tags` follows the same rule
for the same reason.

**This survives the fix, which has not shipped.**
`specs/260817-unscoped-route-version-tolerance-and-code-document-id.md` makes both routes accept and
ignore a version rather than reject an unknown one. It is `planned`, gated behind a changeset still
awaiting approval, so the deadlock above is live behavior today and the rule is load-bearing right
now.

It stays the rule afterwards. Tolerating a parameter is not the same as wanting one, and a
parameter a route ignores is noise that a later reader mistakes for intent.

The split is clean and worth stating as a rule rather than a list:

| Route class | Version |
|---|---|
| Content: search, code, overview, topics, articles | Resolved version, always |
| Infrastructure: `/managed-lists`, `/tags` | Never |

### 4. Managed lists are infrastructure, not a tool

`GET /{org}/managed-lists` returns the five closed sets a search can be filtered by:
`categories`, `rock_domains`, `code_source_types`, `code_roles`, and `rock_versions`.

**It is not exposed as a tool.** It answers "what may I type", which is a question the skill
answers on the agent's behalf by validating before sending. Exposing it would put a tool in the
inventory whose only correct use is one the skill already performs.

**`rock_versions` holds bare majors**, `["18", "19", "20"]`, matching the format the routes expect.
The hand-off document's example shows `["16.0", "17.0", "18.0"]`; that is an error in the document
and not a second format. Compare as strings, with no normalizing and no trimming of a trailing
`.0`. Anything that strips a suffix here is coding around a bug that does not exist, and it would
mask a real format change if one ever came.

Cached in `RockCache` under `Rock:AI:CommunityKnowledgeBase:ManagedLists:{organizationId}` with a
one hour expiration. The lists change only when an operator edits one.

Every filter parameter on every tool is checked before the request is built. This is the single
highest-value behavior in the skill, for the reason given in Motivation: the server treats a bad
filter as a legitimate search that found nothing, so without local validation the agent has nothing
to learn from and rephrases the query instead.

#### Say which kind of wrong it is

A rejected filter value is one of two different things, and telling the agent the wrong one causes
lasting damage.

**Misspelled or invented.** The value does not exist anywhere. Say it is not valid, list the valid
ones:

```csharp
return Error( $"'{category}' is not a valid category." )
    .WithInstructions( $"Valid categories are: {string.Join( ", ", validCategories )}. "
        + $"Call {nameof( GetKnowledgeBaseOverview )} to see which of these hold content for this Rock version." );
```

**Real, but not reachable right now.** The value exists and is spelled correctly; it is excluded by
the configured category scope, or it holds no documents in the current scope. **Do not call this
invalid.**

```csharp
return Error( $"The source '{source}' is not available in the current scope." )
    .WithInstructions( $"This skill is scoped to the categories: {string.Join( ", ", configuredCategories )}. "
        + $"Sources with content in that scope are: {string.Join( ", ", availableSources )}. "
        + $"Call {nameof( GetKnowledgeBaseOverview )} to see document counts per source." );
```

The distinction is not pedantry. *"'Podcast' is not a valid source"* is a false statement about a
source that exists, and an agent that believes it will stop reaching for that name **for the rest
of the conversation**, including in a later turn where the scope is different or where the name
would have been the right answer. A false "invalid" is learned and carried forward; a true "not in
scope right now" is not.

This applies to `source` and `domain`, which the overview narrows by scope, and to `category`
whenever the configured scope is what excluded it rather than a typo.

Both message shapes keep the valid-values list and the pointer at `GetKnowledgeBaseOverview`. The
list is what fixes the call just made; the pointer is what stops the next guess, and it is the more
useful of the two, because the overview reports which values hold content **for this deployment and
this version**, with counts. A category that is valid but empty produces the same empty result as
one that is misspelled, and only those counts separate them.

#### Facet filter semantics

A facet filter is a **list of groups**. Commas inside one value are OR, and repeating the parameter
is AND:

```
filter_category=Setup,Reporting&filter_category=Kiosk
```

means Kiosk **and** (Setup **or** Reporting). The builder emits `field = "x"` for a group of one,
`field IN ["a", "b"]` for a group of two or more, and joins the groups with AND.

This is current behavior as of the facet branch, and it is a **fix**, not a redesign. The previous
behavior made repetition narrow the search while commas meant nothing, which was the opposite of
what the OpenAPI text promised. Every call written against the documented intent still means what
it meant.

Three consequences shape this skill.

**1. Only `categories` and `tags` can take a repeated parameter.** They are the only fields stored
as arrays in the index. `filter_domain` and `filter_source` are scalar, so:

| Written as | Means | Result |
|---|---|---|
| `filter_domain=CheckIn,Finance` | one group, OR | Works. Either domain. |
| `filter_domain=CheckIn&filter_domain=Finance` | two groups, AND against a scalar | **Always empty.** No document has two values in one scalar field. |

The second form is a silent failure of exactly the kind this section exists to prevent: a
well-formed request, no error, zero results, and nothing to distinguish it from a genuine miss.
**The skill must never emit a repeated `filter_domain` or `filter_source`**, and the request builder
should assert this rather than trust the caller, because the mistake is invisible once it ships.

**2. Commas are reserved, and that makes splitting safe.** No facet value may contain a comma, and
that is enforced where values are written rather than escaped at query time. Categories and source
names refuse a comma on save, and generated tags and synced Rock domains have commas stripped
automatically since there is no operator to warn. So the skill can split any filter value on comma
with no escaping, no ambiguity, and no round trip. Validate each part after the split.

**3. The skill only ever emits one group per field.** Every filter it builds is a single parameter
whose parts are OR'd. AND across groups is reachable through the API and is deliberately not
exposed, for the reason in tool 2.

#### `/overview` does not share the builder

Confirmed, and worth writing down because the two surfaces read the repeated form differently:

| | comma inside one value | repeating the parameter |
|---|---|---|
| `filter_category` on search | OR | **AND** |
| `categories` on `/overview` | OR | **OR** |

The overview runs a separate parser that flattens every value into one set, splitting on commas and
deduping across repetitions, then treats the result as pure OR. `categories=a,b` and
`categories=a&categories=b` are identical there.

**This changes nothing for the skill**, which sends one comma-joined parameter to both. That form
means OR on either surface, which is why the difference is a footnote rather than a defect. It is
recorded so nobody discovers the divergence later, assumes one side is broken, and "fixes" the side
this skill depends on.

The comma-reservation rule protects the overview too, even though it does not use the shared
builder, since no category name may contain a comma in the first place.

#### The configured scope is validated at request time, not only at save time

This falls out of how the overview handles an unknown category, and it is the one place point 2
above has teeth.

If **every** named category is unknown, the service reports an empty corpus rather than ignoring
the filter. That is deliberate on its side and correct. But if **only some** are unknown, the
unknown ones are quietly dropped and the scope is wider than what was asked for, with no signal in
the response.

Neither case can be reached by an agent, because tool parameters are validated before they are
sent. **The configured scope can reach both**, because it was saved once and the server's category
list moves independently. An operator scopes a skill to four categories, someone renames one of
them a month later, and the saved value is now a name the service does not recognize.

So the configured scope is filtered against the cached managed lists on every request:

1. **All configured categories still valid**, the normal path: send them.
2. **Some are stale:** send the valid ones and report the dropped names through
   `.WithMetadata()`. Log a warning naming the skill. The scope is wider than the operator
   intended, and the only thing worse than that is it being wider invisibly.
3. **All are stale:** `Error` naming the setting. Do not send an unfiltered request. Dropping every
   stale value leaves no `filter_category` at all, which silently widens a deliberately narrow
   scope to the entire corpus. That is the failure this whole section exists to prevent, arriving
   through the configuration screen instead of through a tool call.

Case 3 is the one that justifies the rule. Cases 1 and 2 are cheap; case 3 is a configuration that
reads as working and searches everything.

**Distinct from the overview.** Both report categories and domains, and the difference matters:
managed lists answer "what may I type" with no scoping and no counts; the overview answers "what
is in the corpus for my scope" with counts, and omits empty values. Validation uses managed lists.
Routing uses the overview.

### 5. Departures from the shared conventions

Recorded here rather than per tool, because they apply throughout and every one of them will
otherwise read as an oversight.

| Convention | Departure | Why |
|---|---|---|
| Results inherit `EntityResultBase`, carry `IdKey` and `Guid` | They do not | Nothing here is a Rock entity. There is no `Id` to hash into an IdKey and no `Guid` to populate. Results carry the remote's own identifiers, named for what they identify: `DocumentId`, `ArticleKey`, `TopicKey`. Convention section 5, note 2. |
| Paged database queries use `CursorPaginator` | Page number, mapped to the remote's `offset` and `limit` | There is no `IQueryable` and no `ISecured` entity. Cursor paging exists to enforce per item security while filling a page, and there is nothing here to enforce. The remote is offset based; a Rock cursor would be a re-encoding of an offset. |
| Results are security filtered and `Sanitize()`d | Neither | The endpoints are open and no returned item carries item level security. Stated explicitly so a reviewer can tell this was decided rather than forgotten. |
| Tool prefixes are `Lookup`, `List`, `Get`, `AddOrUpdate`, `Delete` | `Search` and `Grep` are added | `Search` is already established by `PersonSkill.SearchPerson`. `Grep` is used once, deliberately, and defended at that tool. |
| Writes open their own `RockContext` | No writes exist | Read-only API. |

### Tool inventory

| # | Tool | Store | Endpoint | Paging |
|---|---|---|---|---|
| 1 | `GetKnowledgeBaseOverview` | all | `/overview` | none |
| 2 | `SearchKnowledge` | knowledge | `/search/knowledge` | page number |
| 3 | `SearchCode` | code | `/search/code` | page number |
| 4 | `GrepCode` | code | `/code/grep` | none, capped by the server |
| 5 | `GetCodeLines` | code | `/code/documents/{id}/lines` | line range |
| 6 | `GetTopic` | topics | `/topics/{key}` | none |
| 7 | `GetArticle` | topics | `/articles/{key}` | none |

#### There is no `LookupTopics`

An earlier draft had one, wrapping `GET /{org}/topics`. It is dropped because the overview already
returns the published topics, each with its key and its hint, so a separate tool would return the
same list from a second endpoint.

Two tools returning the same thing is not merely redundant here, it is the exact failure this skill
exists to prevent. The agent would have to choose between them, the choice has no right answer, and
a wrong choice is invisible. Worse, a `LookupTopics` that skipped the overview would report topics
**unscoped**, while the overview reports them scoped to the configured categories and the running
Rock version. The two lists could legitimately differ, and nothing would explain why.

**Confirmed against the service.** Each entry in the overview's `data.topics` is
`{ key, name, hint, rock_version, article_count }`. `key` is exactly what `/topics/{key}` takes and
`hint` is the operator-written routing note, so the overview carries everything `/topics` would
have. Nothing is lost by dropping the tool.

So topics are entered from the overview, which the agent is already required to call first.

---

### 1. GetKnowledgeBaseOverview

`[AgentToolGuid( "7D3ED0C6-6B02-42F5-AB34-4815FE7FF00C" )]`

```csharp
public async Task<AgentToolResult> GetKnowledgeBaseOverview()
```

No parameters. The Rock version and the configured categories are supplied by the skill.

```csharp
[Description( "Describes everything the Rock community knowledge base holds: its knowledge sources and their document counts, which Rock releases have source code indexed, the curated topics available and how to open them, the valid values for every search filter, and guidance on which store answers which kind of question." )]
[AgentPurpose( "Establishes what the knowledge base actually contains before any search is attempted, so the right store is chosen on the first move and every filter value is known to be real." )]
[AgentUsage( "Call this once, before the first search of a conversation. Its result stays available for the rest of the conversation and does not need to be called again." )]
[AgentUsage( "This is also where curated topics are found. Take a topic key from here and open it with GetTopic." )]
[AgentToolReturnsDescription( "The knowledge sources with document counts, the indexed code repositories and the Rock releases they cover, the published topics with a key and a hint for each, the valid filter values, and operator-written guidance on store selection." )]
[AgentToolPreamble( "Checking the knowledge base" )]
```

**Output.** `Guidance`, `KnowledgeSources[]`, `CodeRepositories[]`, `Topics[]`, `Filters`,
`AppliedRockVersion`, `AppliedCategories[]`.

`Guidance` is the operator-written text the service returns, passed through unedited. It is the
deployment's own answer to "which store should I use", and it is more current than anything
compiled into this skill.

`Filters` maps `data.filters`, which the response carries on every overview:

```json
"filters": {
  "categories": [{ "name": "product-documentation", "document_count": 1342 }, ...],
  "domains": [{ "name": "Core", "document_count": 373 }, ...],
  "source_types_code": [...]
}
```

**Map it, do not drop it.** An earlier draft omitted this field while both `[Description]` and
`[AgentToolReturnsDescription]` promised "the valid values for every search filter", so the
annotations told the agent to expect something the result class would have deserialized and thrown
away. That is the worst version of this mistake: the agent is told the data is present, cannot find
it, and has no way to tell whether it asked wrong or the corpus is empty.

The counts are the point. They are what separates *"this category is misspelled"* from *"this
category is real but holds nothing in my scope"*, which is the distinction Section 4's error
wording depends on and the reason the overview is the pointer every validation failure names.

`Topics[]` is the **only** source of topic keys in the skill, since there is no `LookupTopics`.
Each entry in `data.topics` carries `key`, `name`, `hint`, `rock_version`, and `article_count`,
mapped to `TopicKey`, `Name`, `Hint`, `RockVersion`, and `ArticleCount`.

Return all five. `key` is exactly what `GetTopic` takes, and `hint` is the operator-written routing
note that the decision to open a topic gets made from, so neither can be trimmed. `ArticleCount` is
worth keeping too: it is the only signal of how much work opening a topic represents, and it comes
free in a response that is already small.

`data.code.no_code_in_scope` is also reported here when set, with its own `message`. It answers
"does anything in my scope have code", where scope includes the configured categories. Do not
confuse it with `meta.no_code_for_version` on the code routes; see tool 3.

`AppliedRockVersion` and `AppliedCategories` are read from `meta` and echoed, so the agent can see
what scope it is actually working within rather than inferring it.

#### `"all"` is a value that comes back

The all-versions marker is the literal string `all`. **It is never something to send**, and the
request side needs no handling for it: content tagged `all` matches every version request, which is
part of why always scoping to the running Rock version is correct.

It appears in output, though. A topic or a source can carry `rock_version: "all"`, and
`scope.versions_available` reads `["17", "18", "19", "20", "all"]`. The only requirement is
presentational: do not render `all` raw in a list of release numbers as though it were one. Render
it as "all versions", or omit it from a version list and state the fact separately.

**Paging.** None. The whole response is small enough to read in one piece; that is a deliberate
property of the endpoint.

**History.** `.WithHistoryKey( "kb-overview" )`. One entry, replaced rather than accumulated.

**Caching.** Five minutes in `RockCache`, keyed by organization id, version, and category set.
The service caches it briefly on its own side, so a newly ingested source takes a few minutes to
appear either way; a short local cache adds nothing to that staleness and removes a round trip
from the front of every conversation.

**Why it comes first, mechanically.** The prerequisite is asserted in three places, because one
is not enough:

1. `[AgentUsage]` at the skill level, quoted in the declaration above.
2. `[AgentToolPrerequisite( "Call GetKnowledgeBaseOverview first..." )]` on tools 2 through 7.
3. On a `NoData` from any search tool, `.WithInstructions()` naming this tool.

Point 3 is the one that matters. A model that skipped the overview and found nothing will
otherwise rephrase its query, which is exactly the wrong correction.

Dropping `LookupTopics` strengthens all of this. The overview is now the only door into the topic
store, so an agent that wants curated content has to walk through it.

---

### 2. SearchKnowledge

`[AgentToolGuid( "2A6D26DA-F889-4AD7-B9F2-B26B80902229" )]`

```csharp
public async Task<AgentToolResult> SearchKnowledge(
    string query,
    string category = null,
    string domain = null,
    string source = null,
    string tags = null,
    int pageNumber = 1 )
```

```csharp
[Description( "Searches Rock documentation, guides, and community content using combined keyword and meaning-based matching. This is the right first move for almost every question about Rock." )]
[AgentPurpose( "Answers questions about how Rock works, with a citation for every result." )]
[AgentUsage( "Prefer this over the code tools. Use the code tools only when the question is about implementation detail, or when this tool has already failed to answer it." )]
[AgentToolPrerequisite( "Call GetKnowledgeBaseOverview first so the available sources and their coverage are known." )]
[AgentToolReturnsDescription( "Matching passages, each with its title, snippet, source, and the citation link it came from. One document may return several passages." )]
[AgentToolPreamble( "Searching the knowledge base" )]
```

| Input | Required | Notes |
|---|---|---|
| `query` | **yes** | Maps to `q`. |
| `category` | no | Validated against managed lists **and** against the configured scope. |
| `domain` | no | Maps to `filter_domain`. Comma-separated means any of them. Validated against `rock_domains`. **Never repeated**, see Section 4. |
| `source` | no | Maps to `filter_source`. Same rules as `domain`. Validated against the sources reported by the overview. |
| `tags` | no | Maps to `filter_tags`. Comma-separated means any of them, emitted as **one** parameter. Validated against `/tags`. |
| `pageNumber` | no | Default 1. |

All four filters accept a comma-separated list and mean "any of these". Each part is validated
separately after the split, which is safe because no facet value may contain a comma. See
[Facet filter semantics](#facet-filter-semantics).

**On `tags`.** `filter_tags` is a real parameter on this route, and its validation source is
`GET /{org}/tags` rather than `managed-lists`. That matters more than it sounds, because **`/tags`
is paginated** and `managed-lists` is not: it returns `offset`, `limit`, and `total`, so building
the validation set means paging until `total` is reached rather than reading one response.

**Page it at `limit=250`, not the default 50.** The set has to be assembled in full regardless, so
the default costs five times the round trips for nothing. 250 is the route maximum.

Send no `rock_version`; `/tags` is an infrastructure route, see Section 3. Cache the assembled set
exactly like the managed lists, one hour, since it changes at the same rate and for the same
reason.

This is the one place in the skill where a validation list costs more than a single call. Do not
validate a tag against the first page and call it done; a tag that is real but sits on page two
would be rejected as invalid, which is the precise failure this validation exists to prevent,
inverted.

**Tags are OR'd, not AND'd.** `tags` is an array field, so a repeated parameter would work and
would mean "carries every one of these tags". **AND is not exposed anywhere in this skill.** This
is a decided requirement, not a default awaiting review.

The reasoning, recorded so it is not relitigated: a model handed a `tags` parameter and writing
`checkin,troubleshooting` almost certainly means "about either of these", not "tagged with both".
Guessing wrong in the AND direction returns nothing, which is the failure mode with no diagnosis,
since the agent cannot tell an over-narrow filter from an empty corpus and will rephrase the query,
which cannot fix it. Guessing wrong in the OR direction returns too much, which the agent can see
and narrow.

When the two failures are unequal, take the one that leaves evidence.

**Category resolution**, in order:

1. Configured scope empty and no `category` given: send no `filter_category`.
2. Configured scope set and no `category` given: send the whole configured set as **one**
   comma-joined parameter, which means any of them.
3. `category` given and inside the configured scope: send those values, again as one parameter.
4. `category` given and outside it: `Error` naming the categories this skill is scoped to. Do not
   silently fall back to the configured set; a narrowed search that quietly widened is a wrong
   answer wearing the right shape.

Step 2 is the case the facet semantics make or break. The configured scope is an allow-list, so
"any of these" is the only reading of it that is correct. Emitting one parameter per configured
category would AND them together and return only documents carrying **every** category at once,
which is close to nothing. An operator who scoped a skill to four categories to get more relevant
answers would instead get none, with no error and no way to connect the empty result to the
setting that caused it. One parameter, commas, always.

Steps 2 and 3 send the configured scope only after it has been filtered against the cached managed
lists, since a saved category name can go stale between configuration and use. See
[The configured scope is validated at request time](#the-configured-scope-is-validated-at-request-time-not-only-at-save-time).

**Output.** For each hit: `Title`, `Snippet`, `SourceName`, `Category`, `Domain`, `Citation`,
`PublishDate`, `Score`.

`PublishDate` is **nullable**. `published_at` is `string | null` in the result contract and plenty
of documents have none. Model it as `DateTime?`, and do not substitute a default: the ranking code
treats an absent date as unaffected rather than as old, and a result class that invents
`DateTime.MinValue` would present undated content as ancient.

`Citation` is `original_location` from the response, and it is **mandatory on every result**. It is
non-null on every knowledge hit, so no null handling is needed.

**Call it a citation, not a citation link.** For manually uploaded documents it is a reference to
the uploaded file rather than a resolvable URL. Describing it to the agent as a link invites the
agent to present it as one, and a fabricated hyperlink in an answer is worse than a plain
reference. `[AgentUsage]` should say answers built from these results carry the citation, without
promising it can be clicked.

Citations are the main thing the corpus offers over training data, and dropping them wastes it.

**On `publish_date_weight`.** The endpoint accepts a 0 to 1 weight shifting how much a result's
publish date counts toward its position. It is **not** exposed as a parameter and the server
default is used. A model has no basis for choosing a value, and the knob's effect is invisible in
the result, so a wrong choice cannot be noticed or corrected. If staleness turns out to be a real
problem in practice, it belongs as a skill setting, not a tool parameter.

**Paging.** `pageNumber` mapped to `offset = (pageNumber - 1) * 50` and `limit = 50`. Server
maximum is 250.

**Why not a cursor.** The source is a remote HTTP endpoint, not an `IQueryable`, and it exposes
offset and limit. There is no Rock entity security to enforce while filling a page, which is the
only reason the conventions force cursors. A Rock cursor here would encrypt an offset and hand it
back unchanged.

**On `estimated_total`.** `meta` carries it, and it is approximate on large result sets. Surface
it through `.WithMetadata()` **labelled as approximate**, or not at all. Never present it as a
count.

**Empty result.** `NoData()`, echoing the query and every filter applied, including filters the
skill supplied rather than the agent. An agent that does not know a category scope was applied
will conclude the corpus is empty.

---

### 3. SearchCode

`[AgentToolGuid( "A60CA1BC-5E68-481B-8561-27F6AE57D500" )]`

```csharp
public async Task<AgentToolResult> SearchCode(
    string query,
    string sourceType = null,
    int pageNumber = 1 )
```

```csharp
[Description( "Searches the Rock source code by meaning, to find which files implement a given behavior. Returns file locations and metadata only, never code." )]
[AgentPurpose( "Finds the file that implements something, as the first step of reading how Rock actually does it." )]
[AgentUsage( "Use for implementation questions, not general ones. A question about how to use a feature belongs in SearchKnowledge." )]
[AgentToolPrerequisite( "Call GetKnowledgeBaseOverview first to confirm this Rock release has code indexed." )]
[AgentToolReturnsDescription( "Matching files with their path, repository, and document id. No code content; read it with GrepCode or GetCodeLines." )]
[AgentToolPreamble( "Searching the Rock source" )]
```

`sourceType` is validated against `code_source_types`, for example `cs`, `js`, `sql`.

**Output.** `DocumentId`, `FilePath`, `FileUrl`, `Repository`, `SourceType`, `Score`.

`DocumentId` is what `GetCodeLines` takes. It is returned on every hit for that reason and no
other.

#### The id field name differs by route, and the OpenAPI is wrong about it

Read this before writing the result class. It is the one place in this spec where following the
published documentation produces broken code.

| Route | Field the response actually carries | Field the OpenAPI documents |
|---|---|---|
| `SearchCode` | **`id`** | `code_document_id` |
| `GrepCode` | `code_document_id` | `code_document_id` |
| `GetCodeLines` | `code_document_id` | `code_document_id` |

**On a code-search hit, read `id`.** Do not read `code_document_id` there; it is not in the response
today, and reading it yields a null with nothing in the payload to explain why. Grep and
`GetCodeLines` are correct as documented.

Map all of them to `DocumentId`, reading `id` first and falling back to `code_document_id`. That is
correct now and stays correct after the fix below, so it never needs revisiting.

Two ways to get this wrong, and they are opposites, which is why both are named:

1. **Working from the OpenAPI**, you write `code_document_id` for search and every `SearchCode` hit
   comes back with a null id.
2. **Working from a live search response**, you write `id` everywhere and every `GrepCode` hit
   comes back with a null id.

Either way the symptom surfaces one call later, as a `GetCodeLines` invoked with a null id, which
reads as the agent having lost track of a value rather than as a mapping bug.

**Do not treat the OpenAPI as authoritative for response shapes.** See Section 2.

**Pending fix, not blocking.** `specs/260817-unscoped-route-version-tolerance-and-code-document-id.md`
adds `code_document_id` to code search alongside `id` and marks `id` deprecated in the document. It
is `planned`, gated behind a changeset still awaiting approval, and **has not shipped**. `id` is not
being removed in v1, since that is a breaking change belonging to a future path version. So the
mapping above is right before and after, and nothing in this skill waits on it.

**The three code tools are a sequence.** Search to find the file, grep to find the line, read the
lines around it. Say this in `[AgentUsage]` on all three. A model that searches and then asks for
a whole file has skipped the middle step and is about to spend its context on it.

**An empty result may not mean "no match".** Code is indexed per Rock release and has no
"all versions" option. The response reports this as **`meta.no_code_for_version`**, a boolean that
is always present on the REST code routes.

**Read it before returning `NoData`.** When it is `true`, return an `Error` naming the release and
saying plainly that this is a coverage gap and not a search failure. Left as `NoData`, a model
rephrases the query indefinitely against a store that holds nothing for it.

Three things about this flag are easy to get wrong, and all three have bitten someone already:

1. **It is only computed when a version-scoped search returns zero results.** A non-empty result
   set always reports `false` without the service checking anything. **It is not a probe.** Never
   call this tool with a throwaway query to ask "does Rock 20 have code indexed"; the answer will
   be `false` whenever the query happens to match, which says nothing.
2. **The MCP variant has a different shape.** Over MCP, `search_code` and `grep_code` put
   `no_code_for_version: true` plus a human-readable `message` at the **top level** of the result,
   and omit the key entirely when code exists. This skill uses REST, where the key lives in `meta`
   and is always present. Do not port a null check across.
3. **The overview's flag is a different question.** `/overview` reports
   `data.code.no_code_in_scope`, present only when true and carrying its own `message`. "In scope"
   includes the category filter, not just the version, so the two can disagree legitimately. They
   are not interchangeable and neither substitutes for the other.

**Paging.** As tool 2, same reasoning.

---

### 4. GrepCode

`[AgentToolGuid( "D0EA7BC3-3DAF-4481-A1B0-483FE1A4834E" )]`

```csharp
public async Task<AgentToolResult> GrepCode(
    string pattern,
    bool isRegex = false,
    string sourceType = null,
    string pathFilter = null,
    int contextLines = 3 )
```

```csharp
[Description( "Finds exact text or a regular expression in the Rock source, returning each matching line with its line number and surrounding context." )]
[AgentPurpose( "Locates the precise line that defines or uses a known symbol, once the file or area is known." )]
[AgentUsage( "Use this when the exact text is known, such as a method name, class name, or constant. When only the concept is known, use SearchCode first." )]
[AgentToolPrerequisite( "Call GetKnowledgeBaseOverview first to confirm this Rock release has code indexed." )]
[AgentToolReturnsDescription( "Each match with its file path, line number, matched line, and surrounding context lines. Reports whether a cap truncated the search." )]
[AgentToolPreamble( "Searching the Rock source" )]
```

**On the name.** `Grep` is not in the conventions' prefix table, and it is used here on purpose.
`SearchCode` is already the semantic search over this same store. A second tool called
`SearchCodeText` or `SearchCodeLines` would sit next to it in the inventory differing by a suffix,
and the difference between them, meaning-based against literal, is exactly the thing a model must
get right to use them in sequence. `Grep` carries that distinction in the name, it is universally
understood vocabulary for literal and regex line matching, and it is the name the endpoint itself
uses. This is the only departure of its kind in the skill.

**Output.** For each match: `FilePath`, `DocumentId`, `LineNumber`, `Line`, `ContextBefore[]`,
`ContextAfter[]`. Plus `IsTruncated` and `MatchCount` at the result level.

`DocumentId` maps from **`code_document_id`** here, not from `id` as on `SearchCode`. See the id
field table on tool 3, and note that the OpenAPI is wrong about the search side.

**Truncation.** The endpoint reports whether a cap truncated the search. This is the conventions'
clip-and-flag case, not the forbidden silent-cap case: the flag is surfaced, and the recovery path
is a narrower `pattern` or a `pathFilter`. When `IsTruncated` is set, attach
`.WithInstructions()` saying the result is partial and naming both parameters. Never present a
truncated grep as complete.

**`max_matches` is not exposed.** The server default stands. Raising it trades context for
completeness, and a model asked to make that trade will raise it.

**`meta.no_code_for_version` applies here too.** The REST grep route carries the same always-present
boolean as `SearchCode`, with the same meaning and the same three traps. Handle it identically:
read it before returning `NoData`, and on `true` return an `Error` naming the release rather than
letting the agent conclude its pattern was wrong. A grep is if anything the more dangerous of the
two, because a literal pattern that finds nothing reads as conclusive proof the symbol does not
exist.

**Paging.** None. The server caps and reports the cap, which the truncation flag above handles.

---

### 5. GetCodeLines

`[AgentToolGuid( "DB33743D-B2A7-4CD8-A6BA-9576EA83DD35" )]`

```csharp
public async Task<AgentToolResult> GetCodeLines( string documentId, int startLine, int endLine )
```

```csharp
[Description( "Reads a range of lines from one Rock source file." )]
[AgentPurpose( "Reads the code around a known location, after SearchCode or GrepCode has found it." )]
[AgentUsage( "Ask for the smallest range that answers the question, then widen if needed. Whole files are not available through this skill." )]
[AgentToolReturnsDescription( "The requested lines with their line numbers, the file path, the file's total line count, and whether more lines follow the range returned." )]
[AgentToolPreamble( "Reading source" )]
```

`documentId` comes from `SearchCode` or `GrepCode`, mapped from `id` there and `code_document_id`
here. The range is clamped to the file by the server and capped per call.

**Output.** `FilePath`, `StartLine`, `EndLine`, `TotalLines`, `HasMore`, `Lines[]`.

**Use `meta.has_more`; do not infer it.** The response carries `has_more` alongside `start_line`,
`end_line`, and `total_lines`. An earlier draft omitted it and had the agent compare `EndLine`
against `TotalLines` instead. That is a derived check standing in for a direct signal, and derived
checks fail at boundaries: an off-by-one against a clamped `endLine`, or a per-call cap that
returns fewer lines than were asked for without the range reaching the end of the file. The service
already computed the answer. Read it.

`TotalLines` stays, because it is what lets the agent widen deliberately rather than by trial.
`HasMore` answers a different question, namely whether *this* response stopped early.

**Errors specific to this tool**, both reachable from a malformed or stale id:

| Condition | Result |
|---|---|
| `documentId` is not a well-formed GUID | `400` type `invalid-code-document`. `Error` telling the agent to pass a `DocumentId` exactly as returned, unedited. |
| Well-formed id that no longer resolves | `404` type `code-document-not-found`. `NoData` instructing a fresh `SearchCode` or `GrepCode` to get a current id. |

The `404` is `NoData` rather than `Error` for the same reason a stale topic key is: the agent did
nothing wrong, it used an id that has since moved, and the recovery is to re-search rather than to
re-word.

**Why `/raw` is not exposed.** See [Out of Scope](#out-of-scope).

---

### 6. GetTopic

`[AgentToolGuid( "F0179643-6979-416B-8D30-E45CBD96E49E" )]`

```csharp
public async Task<AgentToolResult> GetTopic( string topicKey )
```

```csharp
[Description( "Returns one topic's table of contents: its guidance plus its top-level articles, each with the key needed to retrieve it." )]
[AgentPurpose( "Opens a topic so its articles can be read in order." )]
[AgentToolPrerequisite( "Take topicKey from the Topics list returned by GetKnowledgeBaseOverview. Never construct or edit a key." )]
[AgentToolReturnsDescription( "The topic's guidance text and its top-level articles, each with a retrieval key and title." )]
[AgentToolPreamble( "Reading topic" )]
```

**Output.** `TopicKey`, `Guidance`, `Articles[]` of `ArticleKey`, `Title`, `Summary`.

The response is `{ topic, instructions, articles }`. `topic` is the key echoed back, not a display
name, so it maps to `TopicKey` and `instructions` maps to `Guidance`.

**There is no `Title`, and the field is dropped.** An earlier draft had one with nothing to
populate it. The route carries no display name at all, and the alternative, carrying `name` forward
from the overview's `Topics` list, was rejected: it makes the tool's own result depend on state
from a different call, so the field would be populated in a normal conversation and null in a
replay, a resumed session, or a test that calls this tool alone. A field that is usually present is
worse than one that never is.

Worth asking Griffin to add `name` to the TOC response, which would make the route self-contained
and the field trivial. Until then, the agent already has the name from the overview and does not
need it repeated.

#### An empty `Articles[]` is a miss, not a success

**The `404` does not catch every miss, and this is the important part of this tool.**

The route returns `404` only when the topic has neither TOC instructions nor articles. A topic that
exists and is published but is scoped to a **different Rock release** returns its instructions with
an empty article list, so it comes back `200` with zero articles. The skill always sends a resolved
version, so this is reachable in normal use, and which of the two responses you get depends on
whether an operator happened to write instructions for that topic.

That is a coin flip deciding whether a miss looks like a miss. So the skill does not branch on the
status code:

```csharp
if ( response.StatusCode == HttpStatusCode.NotFound || !result.Articles.Any() )
{
    return NoData()
        .WithInstructions( $"No articles are available for topic '{topicKey}' on Rock {rockVersion}. "
            + $"Call {nameof( GetKnowledgeBaseOverview )} and take a key from its Topics list. Never edit a key." );
}
```

One condition covering both paths. Echo the key and the version, since the version is usually the
actual cause and the agent has no other way to see it. A miss is a result, per convention section
9.5, and adjusting the key is precisely the wrong recovery.

---

### 7. GetArticle

`[AgentToolGuid( "BCE7AD22-3768-4DEE-A2E1-71BC324905EE" )]`

```csharp
public async Task<AgentToolResult> GetArticle( string articleKey )
```

```csharp
[Description( "Returns one article's full content along with the keys of its child articles." )]
[AgentPurpose( "Reads a curated article and reveals what sits beneath it." )]
[AgentToolPrerequisite( "Take articleKey from GetTopic or from a parent article's child list. Never construct or edit a key." )]
[AgentToolReturnsDescription( "The article's full content, its summary, the topic it belongs to, and its child articles with their keys." )]
[AgentToolPreamble( "Reading article" )]
```

**Output.** `ArticleKey`, `Topic`, `Title`, `Summary`, `Content`, `ChildArticles[]`, mapped from
`retrieval_key`, `topic`, `title`, `summary`, `content`, and `child_articles`.

#### There is no segment paging over REST

An earlier draft had a `segmentNumber` parameter and a `HasMoreSegments` flag. Both are deleted.
`GET /{org}/articles/{...key}` returns the whole `content` in one response and carries no paging
fields at all.

**The segmentation is an MCP-side mechanism, not a property of the article store.** The MCP
`get_article` tool takes a `content_offset` and returns `content_offset`,
`content_total_length`, `has_more_content`, and `next_content_offset`, cutting at a paragraph or
line break near a configurable ceiling. None of that exists on the REST route this skill calls.

Getting this backwards is expensive in a specific way: paging logic written against fields the
response does not contain will read `has_more_content` as absent, treat that as false, and appear
to work perfectly, because the single response really did contain everything. The bug is invisible
and the code is dead. Delete it rather than carrying it "just in case".

**A long article therefore arrives whole**, and that is the service's decision rather than this
skill's. It is consistent with the never-cap rule: the alternative would be this skill clipping a
value the endpoint returned complete, with no companion tool to recover the rest. If article
length becomes a context problem in practice, the fix belongs on the service or in a move to the
MCP surface, not in a truncation added here.

#### Never percent-encode a retrieval key

Article keys contain slashes and the route is a **catch-all** path segment. The service splits the
path into segments *before* decoding them, so `%2F` never survives as a separator. A single

```csharp
Uri.EscapeDataString( articleKey )   // WRONG
```

turns every namespaced key into a `404`, and it is the exact call a careful implementer reaches for
when interpolating user-supplied text into a URL. Being careful is what breaks it.

**Escape each segment separately, or do not escape at all.** Splitting the key on `/`, escaping the
parts, and rejoining with `/` is correct. So is passing the key through untouched, which is safe
here because keys are never constructed by the agent and never contain a comma or a query
character.

Related constraint, for whoever changes the key format later: `/topics/{key}` is a **single**
segment, not a catch-all, so a topic key containing a slash would be unreachable no matter how it
is encoded. Live topic keys use underscores, `rock_schema_v19`, `lava_shortcodes`, so this is not a
problem today. It is a problem the day someone namespaces a topic key with a slash to match the
article convention.

#### Retrieval keys are never constructed

Keys contain slashes, `db-schema/attendance-model`, and are used as a path segment. They are taken
only from the overview's `Topics` list, a topic's table of contents, or a parent article's child
list. An invented key returns not-found.

This is asserted three ways: `[AgentToolPrerequisite]` on tools 6 and 7, the instruction text on
every `NoData` from either, and an `[AgentGuardrail]` on the skill. It is worth the repetition
because a key that looks like a readable slug is exactly the kind of value a model will happily
assemble from a heading it just read.

---

### Result classes

In `Rock.AI.Agent/Classes/Skills/CommunityKnowledgeBaseSkill/`. None inherit `EntityResultBase`,
for the reason in Departures.

| Class | Used by |
|---|---|
| `KnowledgeBaseOverviewResult` | 1 |
| `KnowledgeSourceSummaryResult` | 1 |
| `CodeRepositorySummaryResult` | 1 |
| `TopicSummaryResult` | 1 |
| `KnowledgeSearchHitResult` | 2 |
| `CodeSearchHitResult` | 3 |
| `CodeGrepMatchResult` | 4 |
| `CodeLinesResult` | 5 |
| `TopicTableOfContentsResult` | 6 |
| `ArticleResult` | 7 |

One shared internal envelope type deserializes `{ data, meta }`, and one problem-details type
deserializes the error shape. Neither is a result class and neither is returned to the agent.

## Decided

### Only the category scope is configurable

An earlier draft made the organization id and the host skill settings, on the reasoning that the
id is opaque operator-supplied configuration. Both are gone.

The host is a constant because there is one knowledge base and it lives at
`knowledge.rockrms.com`. A setting for it would be a field nobody should ever change, sitting in
front of every operator, inviting exactly one kind of mistake.

The id is resolved from Rock because Rock will have it. A setting for a value the software can
read itself is a transcription step with a failure mode and no upside, and the failure mode here
is the bad one: a valid-but-wrong UUID returns correct answers under someone else's attribution
and reports nothing.

What is left is category scope, which is genuinely a choice, and genuinely per agent. A developer
agent scoped to `developer-reference` and `plugins` alongside a staff agent scoped to
`product-documentation` is a reasonable thing to want, and only per-skill configuration can
express it.

The removals also paid for themselves elsewhere. With nothing to enter first, the configuration
screen can always reach the server, so the category picker works on a brand new skill instance and
`ExecuteComponentRequest` is not needed at all.

### A missing organization id is not an error

The id is analytics and rate limiting, not authentication. When it cannot be read, the skill sends
the empty GUID and answers the question.

The alternative, refusing to run without it, trades the whole feature for a metric. The empty GUID
is also better than any other placeholder: it is well formed, so it clears the only check the
service actually performs, and it is unmistakably a "not supplied" marker rather than a
plausible-looking wrong organization that would quietly pollute someone else's numbers.

**One cost beyond attribution, accepted knowingly.** The rate limiter keys on the same GUID, so
every deployment that falls back shares a single bucket. That is fine for a rare fallback and would
not be fine as a steady state: if the resolver is ever found to be failing widely, the symptom will
be `429`s that no single organization's usage explains, and the fix is the resolver rather than the
limit. Worth knowing before someone debugs that from the wrong end.

### The overview is a tool, not an automatic preamble

It could be fetched implicitly and prepended to the skill's instructions, guaranteeing it is
always present. It is a tool instead, for two reasons. It costs a round trip on every request,
including the ones that never search. And the agent choosing to call it is what makes the result
land as a decision it made rather than as background text, which is the behavior the whole routing
design depends on.

The prerequisite annotations plus the redirect on empty results are the mechanism that makes the
choice reliable. If they prove insufficient in practice, the fallback is to inline the overview's
`Guidance` field into the skill instructions while keeping the tool. That is a smaller change than
it sounds and does not affect anything else here.

### Filters are validated locally rather than passed through

Passing an unknown filter through is one fewer round trip and one fewer thing to keep in sync. It
is rejected because of how the failure looks: an empty result, identical to a genuine miss. The
agent has nothing to learn from and rephrases the query, which cannot fix a bad filter. Local
validation converts a silent wrong answer into a message naming the valid values.

### No retry on 429

Rate limiting is per organization per minute, so a retry inside the tool consumes the same budget
the next tool call needs. The agent is told the limit was hit and told not to call again
immediately. Backing off is a decision about the whole conversation, not about one call.

## Out of Scope

- **`GET /{org}/code/documents/{id}/raw`.** Returns a whole file as `text/plain`, outside the
  usual envelope. A large source file will consume the agent's context and leave no room for the
  answer. `GetCodeLines` reaches every line of every file through a bounded range, so nothing is
  unreachable; the missing capability is only "all of it at once", which is the part that causes
  the harm.
- **`GET /api/health`.** Operational, not something an agent should reason about. A failed call is
  already reported as a transport error naming the host.
- **Deriving the organization id.** The namespace is per-deployment and not public. The skill must
  not accept a Rock organization GUID and hash it, and must not offer to.
- **A configurable host.** One knowledge base, one address, compiled in.
- **Writes.** The API is read-only.
- **A tool wrapping `managed-lists`.** See Section 4.
- **`GET /{org}/topics` and a `LookupTopics` tool.** See the note under the tool inventory.
- **`/sources` and `/tags` as tools.** Both are validation infrastructure rather than tools, the
  same as `managed-lists`. The overview already reports the knowledge sources for `filter_source`,
  and `/tags` is consumed by the `tags` validation on tool 2.
- **The MCP surface.** This skill speaks REST throughout. The service also exposes MCP tools whose
  responses differ in shape, notably `get_article` segmentation and the placement of
  `no_code_for_version`. Those differences are documented at tools 3 and 7 so nobody ports logic
  across, not because a move is planned.

## Open Questions

One remains, and it is a dependency on other work rather than a design question.

1. **Where does the organization id come from in Rock?** Being decided separately. Until it lands,
   send the empty GUID, which is also the permanent fallback for when the value cannot be read.
   Whatever the source turns out to be, it is read behind one resolver method and nothing else in
   the skill changes.

### Noted, upstream

Not blocking, and not this skill's to fix. `/overview` parses its `categories` parameter with its
own flattening parser rather than the shared facet builder, which is why the two surfaces read a
repeated parameter differently. Folding the overview onto the shared parser would make that split a
documented choice rather than an artifact of two parsers written a week apart.

If that consolidation happens, **check this spec's Section 4 table before changing the overview's
behavior**, since the skill's correctness on both surfaces currently rests on one comma-joined
parameter meaning OR everywhere. Consolidating onto the facet builder preserves that. Consolidating
the other way, or making the overview treat repetition as AND, would not.

### Resolved

| Question | Answer | Where it landed |
|---|---|---|
| Does `rock_versions` return bare majors? | Yes. The hand-off's `["16.0", "17.0", "18.0"]` example is an error in that document. | Section 4, with an explicit warning against suffix-stripping |
| Does `/search/knowledge` accept `filter_tags`? | Yes, as a repeatable array. Validation source is `/tags`, which unlike `managed-lists` **is paginated**. | Tool 2, with the paging trap called out |
| Do the overview's topic entries carry the key and the hint? | Yes, both, plus `name`, `rock_version`, and `article_count`. `LookupTopics` stays dropped and the inventory does not shift. | Tool inventory note and tool 1 |
| What are the article segment paging field names? | **Premise was wrong.** REST returns the whole article with no paging. Segmentation exists only on the MCP `get_article` tool. | Tool 7, where the paging parameter and flag were deleted |
| What flag marks "no code indexed for this release"? | `meta.no_code_for_version` on the REST code routes, always present, on **both** search and grep. | Tools 3 and 4, with the probe, MCP-shape, and `no_code_in_scope` traps documented |
| Is `filter_category` repeatable or comma-separated? | **Both, meaning different things.** Commas are OR within a group, a repeated parameter is AND across groups. Only `categories` and `tags` are array fields, so only they can take a repeated parameter at all. | Section 4, "Facet filter semantics", plus the category resolution rules on tool 2 |
| Does `/overview` follow the same rules? | No. It uses a separate parser that flattens everything to one OR'd set, so repetition means OR there and AND on search. The skill's one comma-joined parameter is correct on both. | Section 4, "`/overview` does not share the builder" |

## Related

- [260807-ai-agent-tool-conventions.md](260807-ai-agent-tool-conventions.md) — the conventions this
  skill follows, and departs from in Section 5.
- [260807-ai-agent-core-administration-skill.md](260807-ai-agent-core-administration-skill.md) —
  the read-only skill this one is shaped after.
- `Rock.AI.Agent/Skills/PrayerSkill.cs` — the precedent for a hand-rolled configuration component.
- `Rock.JavaScript.Obsidian/Framework/Controls/Internal/AI/Skills/prayerSkill.obs` — the precedent
  for the Obsidian side of it.
