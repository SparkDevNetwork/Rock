# Environment

What a verification run needs, and how to check it before running anything. `/verify-plan` copies the Required state and Test users sections into every plan.

## Required state

- **Site:** a local Rock site (localhost) running the `RockWeb` build of the commit the plan was built from.
- **Database:** the database named in `RockWeb/web.ConnectionStrings.config`, with all migrations applied (starting the site applies them).
- **Sample data:** loaded with **Admin Tools > Power Tools > Sample Data**, with the sample people's password set to `password`.
- **Mostly clean:** the database does not have to be freshly created, but it should not carry months of unrelated development changes. Old security rules on the test users, leftover test pages, and changed role memberships are the usual cause of misleading results. If in doubt, restore a fresh database and load sample data.

## Test users

| User | Login | Access |
|---|---|---|
| Admin (person record "Admin Admin" on the stock database; may be named Alisha Marble on some) | `admin` / `admin` | Full administrator (RSR - Rock Administration). Passes every authorization check, so use it for setup and golden paths, never to prove a check blocks someone. |
| Ted Decker | `tdecker` / `password` | Elevated staff: Calendar, Communication, Connection and Event Registration Administration, WEB - Administration, Pastoral Workers, Prayer Access, Staff Workers. No Finance Administration. |
| Pete Foster | `pfoster` / `password` | Normal staff: Staff Workers, Pastoral Workers, Prayer Access. |

Ted's family (Cindy, Noah, Alex Decker) and the other sample families are available as data. When a test needs a user with VIEW but not EDIT on a block, page, or entity, set it up as admin through the item's security (lock icon) and remove it afterward.

## Local settings

Machine-specific values live in `.verification/environment.json` at the repo root (git-ignored):

```json
{
    "siteUrl": "http://localhost:6229",
    "sqlServer": "localhost"
}
```

If the file does not exist, look for the site first: read the IIS Express bindings in `.vs/Rock/config/applicationhost.config` (for example `bindingInformation="*:6229:localhost"`) and request `http://localhost:<port>/`. If Rock answers (a redirect to its login page counts), use that URL and tell the user which one you picked. Otherwise ask the user for the site URL (and SQL Server name if `localhost` does not work). Then create the file. The database name always comes from `RockWeb/web.ConnectionStrings.config` (`Initial Catalog`), so it follows whichever database the site is pointed at.

SQL access: `sqlcmd -S <sqlServer> -d <Database> -E -W -I -b -Q "SET NOCOUNT ON; <query>"`. Always pass `-I` (QUOTED_IDENTIFIER on) or deletes on tables with filtered indexes fail, and `-b` so an error stops the batch.

## Preflight checks

Run all of these and show one table: Check, Result (OK / Warn / Fail), Detail.

| # | Check | How | Fail or Warn |
|---|---|---|---|
| 1 | Database reachable | `SELECT DB_NAME()` | Fail if it errors. |
| 2 | Site reachable | `GET <siteUrl>` returns 200 (PowerShell `Invoke-WebRequest` or curl). | Fail if not; ask the user to start the site. |
| 3 | EF migrations current | `SELECT TOP 1 [MigrationId] FROM [__MigrationHistory] ORDER BY [MigrationId] DESC` equals the newest file name (without `.cs`) under `Rock.Migrations/Migrations/` that is not `.Designer.cs`. | Fail if the database is behind: the site has not been started on this build. |
| 4 | Hotfix migrations current | `SELECT MAX([MigrationNumber]) FROM [PluginMigration] WHERE [PluginAssemblyName] = 'Rock'` equals the highest number prefix in `Rock/Plugin/HotFixes/`. | Warn if behind (some hotfixes target older versions and can be skipped by design; ask the user). |
| 5 | Build is current | Compare `git rev-parse --short HEAD` with the plan's "Built from" hash, and `RockWeb/bin/Rock.dll` modified time with the newest commit or working-tree change. | Warn if the plan's hash differs or the DLL is older than the source. Ask the user to confirm the site runs the right build. |
| 6 | Sample data present | `SELECT [UserName] FROM [UserLogin] WHERE [UserName] IN ('admin','tdecker','pfoster')` returns 3 rows, and Ted Decker's family exists. | Fail if a user is missing. |
| 7 | Test logins work | For each test user, `POST <siteUrl>/api/Auth/Login` with JSON `{"Username":"...","Password":"...","Persisted":false}` from a scripted session (see `techniques.md`); 204 means success. | Fail if any login is rejected. |
| 8 | Role membership as expected | Security roles (`[Group].[IsSecurityRole] = 1`) of Ted and Pete compared with the Test users table. | Warn on any difference; list it. |
| 9 | Leftover security rules | `SELECT COUNT(*) FROM [Auth] AS [a] JOIN [PersonAlias] AS [pa] ON pa.[Id] = a.[PersonAliasId] JOIN [UserLogin] AS [u] ON u.[PersonId] = pa.[PersonId] WHERE u.[UserName] IN ('tdecker','pfoster')` | Warn if more than 0: person-specific rules from earlier testing can change results. List the entities. |
| 10 | Leftover test data | Child pages of the `Installed Plugins` page (`[Page].[InternalName] = 'Installed Plugins'`), and rows named `Verif %` in `[Page]`, `[Group]`, `[DefinedValue]`, `[ContentChannel]`, `[WorkflowType]`. | Warn if any exist. |
| 11 | Database age | `CreatedDateTime` of Ted Decker's person record. | Warn if older than 90 days; long-lived databases drift. |

On Warns, ask once whether to continue, clean up first, or stop. Do not clean up leftover data or rules without the user's go-ahead.
