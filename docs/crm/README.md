# CRM Documentation

CRM is the people side of Rock: `Person`, the alias machinery that lets identity be merged without losing referential integrity, and the supporting metadata. Almost every other domain joins to `PersonAlias`, not directly to `Person`, and that single decision drives most of what is unusual about this domain.

If you are new, start with [crm-overview.md](crm-overview.md). Sub-topics worth their own docs (Person Merge, Search Keys, Badges, Assessments, Background Checks, Record Source) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Background Checks](background-checks.md) | Pluggable provider components (Checkr/PMM), the `BackgroundCheck` entity lifecycle, response-data shape, Group Requirement integration. |
| [Badges and Assessments](badges-and-assessments.md) | Configurable badge widgets via `BadgeComponent`; assessment instruments (DISC/Spiritual Gifts/Motivators/EQ/Conflict) with per-instrument services. |
| [CRM Domain Overview](crm-overview.md) | The Person / PersonAlias / Family-as-Group model, why merges work, and the conventions every cross-domain reference follows. |
| [Family and Addresses](family-and-addresses.md) | Family as a `GroupType`, addresses on the family Group via `GroupLocation`, phones on Person, primary-campus recomputation. |
| [Person Merge](person-merge.md) | `PersonService.MergePeople`, alias repointing, Previous Last Name retention, the v2 People API integration. |
| [Person Search and Duplicates](person-search-and-duplicates.md) | `GetByMatch`, `PersonSearchKey` for legacy/external identifiers, the duplicate-detection job and `PersonDuplicate` candidates. |
