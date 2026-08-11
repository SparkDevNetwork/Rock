# Tabler Icon Updater

Maintains the Tabler icon library data consumed by the Obsidian icon picker.

## What it does

Incrementally syncs Tabler outline icons rather than rebuilding from scratch:

1. Scans `node_modules/@tabler/icons/icons/outline/` for the current SVG set.
2. Loads `styleclass-searchterms.json` (checked in) and adds entries only for icons that are new or missing. Existing keywords are never overwritten, so re-running is safe.
3. For each missing icon, calls the OpenAI Chat Completions API to generate 1 to 4 human-friendly search keywords based on the icon name.
4. Rebuilds `RockWeb/Assets/Icons/Libraries/tabler-icons.json` deterministically from the search-terms map and the SVG files, inlining each SVG alongside the title, style class, and keywords.

This script only updates the icon picker JSON. The Tabler webfont (`.woff`, `.woff2`, and the accompanying CSS) is a separate manual step, described below.

## Prerequisites

- Node.js 18 or newer (uses global `fetch`).
- An OpenAI API key with access to `gpt-4.1-mini`. Triumph engineers can pull this from the shared credential vault; anyone else needs to supply their own.

## Instructions

1. From the workspace root, `cd "Rock/Dev Tools/tabler-icon-updater"` and open it in your editor.
2. In `package.json`, bump the `@tabler/icons-webfont` version to the target release.
   Only upgrades are supported. To back out, discard the git changes and start over.
3. Run `npm install` to pull the new package.
4. Set the OpenAI key in your PowerShell session:
   ```powershell
   $env:OPENAI_KEY="sk-..."
   ```
5. Run `npm run generateIcon`.
6. Update the webfont assets (not automated). Copy from `node_modules/@tabler/icons-webfont/dist/`:
   - `fonts/tabler-icons.woff` and `fonts/tabler-icons.woff2` overwrite the same-named files in `RockWeb/Assets/Fonts/TablerFont/`.
   - Bump the `?v3.XX.X` cache-buster in `Rock.Frontend.Styles/src/styles/styles-v2/icons/tabler-icon.css` so browsers pick up the new font.
7. Restart Rock and clear the browser cache.

## Verify

1. Open a block that uses the icon picker. The Control Gallery under Admin Tools > Power Tools has one.
2. Confirm the Tabler option in the library dropdown loads and any icon added in this release appears.
3. Spot-check that an existing Tabler icon still renders elsewhere in the app UI so you know the font path still works.

## How the picker finds the JSON

The picker loads its library list from the `Icon Libraries` defined type (SystemGuid `EEFC6AA8-3946-47AB-8DAF-149EEF347DF3`). Each defined value's `Description` column holds the URL to a library JSON file. The Tabler entry points at `~/Assets/Icons/Libraries/tabler-icons.json`, which is what this script writes. If icons stop appearing after an update, verify that defined value still exists and is active.
