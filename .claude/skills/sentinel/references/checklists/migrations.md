# Migrations and Hotfixes

Load when the code touches `Rock.Migrations/`, `Rock/Plugin/HotFixes/`, data migrations, or
`Auth` rows.

- **No dynamic SQL from input.** Watch for interpolated `$@"..."` strings with values that
  aren't constants.
- **No secrets** or environment-specific values in migration files.
- **Default security ships with the data.** New system records that hold data (workflow types,
  document types, and so on) ship with explicit View and Edit `Auth` rows. With no rules, VIEW
  is allowed for everyone and EDIT is denied. Put the Deny for All Users row last, with the
  highest Order, because the first matching rule wins. For a type that should never be public,
  consider overriding `IsAllowedByDefault` on both the model and the cache, as EventCalendar
  and UserLogin do.
- **Set Order explicitly.** Pass an explicit Order to the record-level `AddSecurityAuthFor*`
  helpers. The `int.MaxValue` append option reads the entity type's rows, not the record's, so
  a Deny can land ahead of the Allows.
- **Leave admin choices alone.** The `AddSecurityAuthFor*` helpers only skip an exact duplicate
  row. They don't skip a record an admin already secured. Guard the insert with `NOT EXISTS` on
  any `Auth` row for that record and action, as hotfixes 290 and 291 do.
- **When a feature's security moves** to a different entity (for example from BinaryFileType
  to DocumentType), a migration copies the existing `Auth` rows over, with the same
  `NOT EXISTS` guard.
- **Backfill links security depends on.** If a fix starts setting a link that security reads
  (such as `BinaryFile.ParentEntityTypeId` / `ParentEntityId`), a data migration fixes the
  existing rows too.
