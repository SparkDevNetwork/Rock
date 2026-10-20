---
author: Daniel Hazelbaker
date_created: 2026-10-20
summary: >-
  An allowlist HTML sanitizer for Rock that keeps a known-safe set of
  elements, attributes and CSS and removes everything else. One behavior
  shared by a C# implementation (server, on AngleSharp) and a TypeScript
  implementation (Obsidian, on the browser's own parser), proven equal by a
  single shared test suite.
contributors:
  - Jon Edmiston
---

# Allowlist HTML Sanitizer

## Summary

Rock renders HTML in many places that is built from templates or from user- or person-supplied values. This spec adds an allowlist sanitizer those sinks can run their HTML through: it parses the HTML, keeps only a known-safe set of elements, attributes, URL schemes and CSS, and removes everything else. It ships as two implementations that give the same output, a C# one for the server and a TypeScript one for Obsidian, held equal by a single shared test suite. This document is the decision record; the allowlists and the per-rule reasons live in the code (`Rock/Utility/HtmlSanitization/`), which stays the source of truth.

## Motivation

A denylist (block a fixed list of dangerous things, allow the rest) is the wrong model for a security boundary: it fails open on anything the author did not think to block. An allowlist fails closed, so a tag or attribute nobody anticipated is removed rather than passed. Rock already has a denylist sanitizer; this adds the allowlist model alongside it for sinks that render person-influenceable HTML.

The sanitizer is deliberately a general, reusable component rather than a fix for one caller. Which sinks adopt it, and with which profile, is decided per site and is out of scope here.

## Requirements

- MUST sanitize an HTML string, returning HTML that contains only allowlisted elements, attributes, URL schemes and CSS.
- MUST be DOM-based (parse, walk, remove, re-serialize), never regex-based, and MUST decode HTML entities as part of parsing.
- MUST offer two built-in profiles, `Simple` (small snippets) and `Rich` (larger admin/staff templates), and MUST let a caller adjust a copy of a profile through an options object without defining a new profile.
- MUST keep a set of fixed rules that no options object can loosen (see Fixed rules).
- MUST be safe to call concurrently once an options object is read-only.
- MUST leave the existing denylist sanitizer unchanged for backward compatibility.
- The C# and TypeScript implementations MUST give the same output for the same input, except for a small, documented set of differences that come from the two HTML parsers (see Parity). A single shared test suite is the contract for both.

## Design

### Engine

Parse once with a standards-compliant HTML5 parser, walk the resulting DOM, remove anything not allowed, then re-serialize.

- **C# (server):** built on AngleSharp, which Rock already references. The input is parsed as a document body, the body is walked, and its child nodes are serialized with AngleSharp's own formatter.
- **TypeScript (Obsidian):** built on the browser's own `DOMParser`, parsing into an inert document (no resource loads, no script execution). Using the browser's parser means the parser that sanitizes is the parser that renders, which removes most parser-versus-sanitizer disagreements. It is for HTML that only exists in the browser; HTML that comes from the server is sanitized on the server.
- **Not HtmlAgilityPack.** Its parser is lenient and not HTML5-spec, so what it accepts can differ from what a browser re-parses (a mutation-XSS risk), and it does not sanitize CSS.

### Profiles and options

Two read-only presets, `HtmlSanitizeOptions.Simple` and `HtmlSanitizeOptions.Rich`, modeled on `System.Text.Json`'s `JsonSerializerOptions`: read-only presets, a copy constructor for an editable copy, and `MakeReadOnly()`. Every rule is a set of allowed values; there are no deny lists on the options object. A caller builds its options once and keeps them in a static field. A caller whose needs do not match a preset starts from a bare options object and allows just what it needs (the Notes control allows only a few tags, such as a mention `span`), and a caller that wants a preset plus extras starts from one and adds to a copy (the Markdown field type starts from `Simple` and adds the tags Markdown produces). The options object is the whole configuration for a call; there is no separate profile enum.

A disallowed element is, by default, removed with its content (`Drop`). A caller can instead `Unwrap` a tag (keep its children in its place). The exact element, attribute and CSS lists per profile, and the reason for each, live in `HtmlAllowListTables.cs`; the develop and code are the source of truth, not this document.

### Fixed rules

These apply regardless of the options and cannot be loosened; each is commented with its reason in `HtmlAllowListTables.cs`:

- **Never-pass elements** (`script`, `style`, `iframe`, `object`, `embed`, `form`, `link`, `meta`, `base`, `noscript`, `template`, `frame`, `frameset`, `applet`, `xmp`, `noembed`, `noframes`, `plaintext`, `title`, `animate`, `set`, `foreignObject`, `annotation-xml`): always removed with their content, in any namespace. The reasons range from "runs script" to "parsed as raw text, a mutation-XSS source" to "re-enters HTML parsing inside foreign content".
- **`svg` and `math` are drop-only:** a caller may allow them (Rich allows a small SVG icon subset), but when not allowed they are always removed with their content, never unwrapped, because unwrapping would move foreign-namespace elements into HTML where the browser parses the result differently.
- **A namespace check**, as DOMPurify does, removes any element in a namespace its parent could not have produced, which is the root of namespace-confusion bypasses.
- **`on*` event-handler attributes** are always removed.
- **Blocked URL schemes** (`javascript`, `vbscript`, `file`, `data`): the URL rule allows only a short scheme list plus relative URLs, and these can never be added.
- **`formaction` and `srcdoc`** can never be allowed.
- **A `name`/`id` rule** removes values that would interfere with the one page-wide server `<form>` that wraps every WebForms page. This defends the WebForms model and can be removed once Rock no longer renders pages inside that form.
- **CSS value rules:** disallowed value constructs (CSS `expression`, and others) always remove a declaration, and `position: fixed` is always removed.
- **Nesting-depth limits:** elements nested past a fixed depth are removed, and input a quick pre-parse scan estimates as far deeper than that is not parsed at all and comes back as encoded text, so pathological nesting cannot exhaust the stack or stall the parser.

### Parity

One shared test suite (data rows of input, options name and expected output) defines the behavior. The C# tests own the rows; the TypeScript tests read the same rows and run them through the TypeScript implementation, and a second test compares the two implementations' tables. A rule changed on one side without the other fails the other's tests.

A small, documented set of outputs differs between the two, always because the two HTML parsers differ, never because the policy differs: the browser parser parses `noscript` content with scripting off, and newer browser/AngleSharp versions follow newer HTML parsing rules in a few edge cases. Each such row is listed with its reason.

## Known Limitations

These are accepted residuals, each with an owner, not open holes:

- **Overlays.** Sanitized content can be positioned or classed to cover surrounding page content (no script, no data read). The real fix is containment where the HTML is inserted (a contained wrapper), handled by the per-site remediation and the new page rendering engine, not the sanitizer.
- **Resource fetch.** An allowed external image or CSS `url()` fetches on render, which is a tracking vector. Accepted, the same residual an allowed `img` already carries; admins legitimately use both.
- **`data-*` and `id`/`name` activation.** These attributes are inert, but a later JS layer (a library that reads an attribute and injects it as HTML, or logic that trusts a DOM lookup) could activate them. Allowed where admins need them; auditing those JS consumers is a separate effort.

The detailed security analysis behind these residuals (the adversarial review, the per-element rationale corpus, and the sink survey) is kept in internal security material, not in this public spec.

## Considered but Rejected

### Keep extending the existing denylist sanitizer
Rejected. A denylist fails open; an allowlist fails closed. The existing denylist method is left unchanged for backward compatibility, not extended.

### Build the allowlist on HtmlAgilityPack
Rejected. Not HTML5-spec, so its output can differ from a browser's re-parse (mutation XSS), and it does not sanitize CSS. The HTML5 parsers (AngleSharp, the browser) are the right base.

### Client-side sanitization as the trust boundary
Rejected as the boundary. The trust boundary stays on the server. The TypeScript implementation exists only for HTML that never passes through the server; HTML from the server is sanitized on the server.

### Allow the `<style>` element
Rejected on every implementation, and not a reason to adopt a third-party library. A stylesheet's rules reach the whole page, not just the sanitized content: they can restyle or hide trusted UI, and a well-known scriptless CSS technique can read attribute values off the page. Per-declaration checking (what a CSS library gives you) does not address this, because the danger is in the selectors and reach, not the declarations. A safe `<style>` would need our own scoping policy (prefix every selector to the sanitized container, forbid attribute selectors and `url()` and at-rules), which barely needs a CSS parser. If a sanitized sink ever needs `<style>`, build that scoped version as a new opt-in (never a change to an existing profile, since the wrapper changes the output).

### A third "show as text" removal mode
Considered and left out. The two modes (`Drop`, `Unwrap`) cover the current callers. Showing markup as text is better done by HTML-encoding the string before it reaches the sanitizer. The mode can be added later as a new enum value without breaking callers if a real need appears.

### Use a maintained third-party sanitizer (Ganss.Xss) instead of our own C# implementation
Rejected, in favor of keeping our own implementation on every version. Ganss.Xss is allowlist-based, built on AngleSharp, and maintained, and adopting it would mean inheriting its fixes through NuGet. Against that:

- **Perfect C#/TypeScript parity is the deciding factor.** With one algorithm and one shared test suite, the server and Obsidian implementations stay in exact sync; a library on one side and hand-written code on the other would drift, and the accepted-difference list would grow.
- The policy that actually matters here (what reaches the whole page, foreign-content handling, the WebForms `name`/`id` rule, scoping) is ours to write regardless of who supplies the parser, because a per-declaration/per-element library check does not express it.
- Reviewed against Ganss.Xss's published advisories, none applied to our configuration.
- Routine NuGet updates are slow in Rock today because of the breadth of code and plugin compatibility, which weakens the "free fixes" benefit in practice.

A standing task to watch upstream sanitizer advisories (Ganss.Xss, DOMPurify) and port any rule that applies to our configuration takes the place of the library's automatic updates.

## Related

- C# implementation: `Rock/Utility/HtmlSanitization/` (the interface, options, tables, removal mode and sanitizer) and the `SanitizeHtml` string extension in `Rock/Utility/ExtensionMethods/StringHtmlSanitizationExtensions.cs`.
- TypeScript implementation: `Rock.JavaScript.Obsidian/Framework/Core/Utilities/htmlSanitization.ts` and `htmlSanitizationTables.partial.ts`.
- Shared tests: `Rock.Tests/Utility/HtmlAllowListSanitizerTests.cs`, `Rock.Tests/Utility/HtmlSanitizeOptionsTests.cs`, and `Rock.JavaScript.Obsidian/Tests/Core/Utilities/htmlSanitization.spec.ts` (reads the C# rows).
- Existing denylist sanitizer, left unchanged: `Rock/Web/Utilities/HtmlSanitizer.cs`.
