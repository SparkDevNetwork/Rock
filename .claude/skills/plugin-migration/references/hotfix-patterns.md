# Plugin Migration (Hotfix) Patterns

Common patterns with complete examples from actual Rock plugin migrations in `Rock/Plugin/HotFixes/`.

---

## Pattern 1: Update HTML Content by Guid

From migration 279 — fix HTML content for a known block:

```csharp
public override void Up()
{
    Sql( @"
DECLARE @Guid UNIQUEIDENTIFIER = '18dbda15-5ed7-4fe8-bc30-da872f6a3c22';

UPDATE [HtmlContent]
SET [Content] =
N'<div class=""alert alert-danger"">
    <strong>Warning!</strong>
    <p>Running SQL commands directly against the database while powerful, can be extremely dangerous.</p>
    <p>If you are unsure of the SQL you are about to run <strong>DO NOT</strong> proceed.</p>
</div>'
WHERE [Guid] = @Guid;
" );
}
```

**Note:** Use `N'...'` for nvarchar string literals. Double up quotes inside: `""` not `\"`.

---

## Pattern 2: Update Page Layout

From migration 272:

```csharp
public override void Up()
{
    Sql( @"
UPDATE dbo.[Page]
SET [LayoutId] = (
    SELECT [Id]
    FROM dbo.[Layout]
    WHERE [Guid] = 'C2467799-BB45-4251-8EE6-F0BF27201535'
)
WHERE [Guid] = '053C3F1D-8BF2-48B2-A8E6-55184F8A87F4';
" );
}
```

---

## Pattern 3: Add Post-Update Service Job

From migration 278 — run-once job to fix data:

```csharp
using Rock.Model;  // needed for typeof()

// In Up():
RockMigrationHelper.AddPostUpdateServiceJob(
    name: "Rock Update Helper v18.3 - Fix Broken Achievement Types",
    description: "This job fixes broken achievement types that could exist as a result of adding new Achievement Types using the new Obsidian block.",
    jobType: typeof( Rock.Jobs.PostUpdateJobs.PostV183UpdateAchievementTypes ).FullName,
    cronExpression: "0 0 20 1/1 * ? *",
    guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_183_FIX_BROKEN_ACHIEVEMENT_TYPES );
```

**Notes:**
- `typeof().FullName` gives the assembly-qualified type name
- Cron expression is mandatory but the job only runs once (post-update)
- Use a SystemGuid constant for the job Guid so it can be referenced in startup code

---

## Pattern 4: Update Stored Procedure

From migration 277 — update a stored procedure with proper session settings:

```csharp
public override void Up()
{
    UpdateSynchronizeListRecipients_Up();
}

public override void Down()
{
    UpdateSynchronizeListRecipients_Down();
}

private void UpdateSynchronizeListRecipients_Up()
{
    // Save current session settings
    var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
    var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

    // Set required settings for stored procedure creation
    Sql( "SET ANSI_NULLS ON;" );
    Sql( "SET QUOTED_IDENTIFIER ON;" );

    // Drop existing procedure
    Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spMyProcedure]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spMyProcedure];" );

    // Create updated procedure (from embedded resource for large SQL)
    Sql( HotFixMigrationResource._277_ExcludeArchivedAndDuplicateListMembers_spCommunication_SynchronizeListRecipients );

    // Restore original settings
    Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
    Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
}

private void UpdateSynchronizeListRecipients_Down()
{
    // Same pattern but restores the PREVIOUS version of the procedure
    var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
    var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

    Sql( "SET ANSI_NULLS ON;" );
    Sql( "SET QUOTED_IDENTIFIER ON;" );

    Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spMyProcedure]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spMyProcedure];" );

    // Restore previous version from an earlier migration's resource
    Sql( HotFixMigrationResource._268_PreviousVersion_spMyProcedure );

    Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
    Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
}
```

**Note:** Stored procedure migrations are one of the few cases where Down() has real logic in plugin migrations.

---

## Pattern 5: Add/Update Block Type Attribute

Using RockMigrationHelper:

```csharp
public override void Up()
{
    RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
        "BLOCK-TYPE-GUID",           // blockTypeGuid
        "1EDAFDED-DFE6-4334-B019-6EECBA89E05A",  // fieldTypeGuid (Boolean)
        "Show Header",               // name
        "ShowHeader",                // key
        "CustomSetting",             // category
        "Whether to show the header panel.",  // description
        0,                           // order
        "True",                      // defaultValue
        "NEW-ATTRIBUTE-GUID" );      // guid
}
```

---

## Pattern 6: Add Security Auth

```csharp
public override void Up()
{
    RockMigrationHelper.AddSecurityAuthForEntityType(
        "Rock.Model.MyEntity",       // entityTypeName
        0,                           // order
        Rock.Security.Authorization.VIEW,  // action
        true,                        // allow
        Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,  // groupGuid
        0,                           // specialRole (0 = none)
        "NEW-AUTH-GUID" );           // authGuid
}
```

---

## Pattern 7: Conditional Data Fix with IF NOT EXISTS

```csharp
public override void Up()
{
    Sql( $@"
IF NOT EXISTS (SELECT 1 FROM [Person] WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}')
BEGIN
    DECLARE @PersonRecordTypeValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON}')
    
    INSERT INTO [Person] ([IsSystem], [RecordTypeValueId], [FirstName], [NickName], [LastName], [Gender], [IsEmailActive], [Guid], [EmailPreference])
    VALUES (1, @PersonRecordTypeValueId, 'System', 'System', 'Sender', 0, 0, '{SystemGuid.Person.SYSTEM_SENDER}', 0)
END
" );
}
```

---

## File Template (Complete)

```csharp
// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// [Description of what this migration does]
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( NNN, "XX.X" )]
    public class DescriptiveClassName : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Migration logic here
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
```

---

## Naming Conventions

- **Filename:** `{number}_{PascalCaseDescription}.cs` (e.g., `280_FixSqlRunnerWarningHtml.cs`)
- **Class name:** Matches the PascalCase description (e.g., `FixSqlRunnerWarningHtml`)
- **Private methods:** Developer initials + description + date: `NA_FixHtmlContent_Up_20260409()`
- **XML doc comments:** Always present on class and public methods
