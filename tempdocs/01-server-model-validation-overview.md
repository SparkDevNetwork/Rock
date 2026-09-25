# Server Model Validation: Overview

## Summary

Starting with Rock **17.9, 18.5, 19.5, and 20.0**, Rock can check the text that gets saved to the database and reject content that does not belong there. For example, a group name should never contain HTML or Lava.

This protection is controlled by a single setting, **Enable Server Model Validation**. While it is off, Rock still performs every check, but instead of blocking the save it writes an entry to the Exception Log and lets the save go through. That gives you a safe window to find and fix problems before anything is enforced.

**Our recommendation:** review your Exception Log, resolve what you find, and then turn the setting on. In the near future, this setting will be removed and validation will always be on, so doing the work now means the change is a non-event when it arrives.

## Why It Matters

Information entered into Rock gets shown in many places: staff pages, your website, emails, and the mobile app. Most fields are meant to hold simple text, like a name or a title. When a field ends up holding something other than what it was designed for, it can cause problems, and in some cases it can be misused by people with bad intentions.

Turning validation on gives you:

- **Stronger security.** Rock makes sure each field only accepts the kind of content it was designed for. This closes off a category of risks that would otherwise depend on every screen and every process being perfect.
- **Cleaner data.** Names, titles, and other simple fields stay simple, so they look right wherever they appear.
- **Protection everywhere.** The check happens at the moment information is saved, so it applies no matter how the information came in: a staff member, a website visitor, a workflow, an integration, or an import.

## What Changes When You Turn It On

| | Setting off (today) | Setting on |
|-|-|-|
| Invalid content is saved | Yes | No |
| An Exception Log entry is created | Yes | Usually |
| The person saving sees an error | No | Yes, the save fails |

In short: anything showing up in your Exception Log today **will become a failed save** once the setting is on. Fixing those entries first is what makes the switch painless.

## How to Get Ready

1. **Confirm your version.** You need Rock 17.9, 18.5, 19.5, 20.0, or a later release in the same version line.
2. **Review and fix.** Over a few weeks, check your Exception Log for validation entries and resolve them. Most are quick settings changes.
3. **Turn it on.** Check **Enable Server Model Validation** on the Security Settings page, then restart Rock.

The step-by-step details, including exactly what to look for and how to fix each entry, are in **Server Model Validation: Fixing Exception Log Entries** (`02-fixing-validation-exceptions.md`).

## If Something Goes Wrong

If an important process starts failing after the setting is turned on, uncheck the setting and restart Rock to return to log-only mode. Fix the underlying issue, then turn it back on. This is a temporary safety valve: in the near future, this setting will be removed and validation will always be on.
