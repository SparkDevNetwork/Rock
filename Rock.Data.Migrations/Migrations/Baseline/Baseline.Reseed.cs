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
//
namespace Rock.Data.Migrations;

/*
    10/29/26 - CLAUDE

    This half of the baseline is written by hand and kept across squashes;
    the package command regenerates Baseline.cs only. Each step gives a
    value that must differ on every install a new random value after the
    data loads, because the baseline's data files hold the values of the
    database it was built from.

    Each step finds its rows by a key a squash cannot change (an Id or a
    Key), never by the frozen value, so it works on every future baseline
    without edits. Each step also checks its row counts and throws when
    they are wrong: a later migration that changed or deleted the row is
    folded into the next baseline, and the step must fail on every new
    install, test runs included, rather than quietly leave the value
    frozen. Fix the step, not the check.

    Reason: Keep per-install values unique on databases built from the baseline.
*/
internal partial class Baseline
{
    /// <summary>
    /// Gives the per-install values a new random value. Runs last in
    /// <see cref="Up"/>, in the baseline's transaction.
    /// </summary>
    private void RegeneratePerInstallValues()
    {
        RegenerateRockInstanceId();
        RegenerateAdminPersonGuids();
    }

    /// <summary>
    /// Gives the <c>RockInstanceId</c> system setting a new <c>Guid</c>,
    /// which identifies the install.
    /// </summary>
    private void RegenerateRockInstanceId()
    {
        Sql( @"
UPDATE [dbo].[Attribute]
SET [Guid] = NEWID()
WHERE [Key] = N'RockInstanceId'
    AND [EntityTypeQualifierColumn] = N'SystemSetting';

IF @@ROWCOUNT <> 1
    THROW 50000, N'Baseline step RegenerateRockInstanceId: expected one RockInstanceId system setting. A migration in the baseline changed or removed it; update Baseline.Reseed.cs.', 1;" );
    }

    /// <summary>
    /// Gives the built-in Admin person (<c>Person</c> 1) and its primary
    /// alias new <c>Guid</c> values, and updates every copy of them, as
    /// <c>Rollup_20251022</c> did for databases built by EF6.
    /// </summary>
    private void RegenerateAdminPersonGuids()
    {
        Sql( $@"
DECLARE @AliasId INT
    , @OldPersonGuid UNIQUEIDENTIFIER
    , @OldAliasGuid UNIQUEIDENTIFIER
    , @NewPersonGuid UNIQUEIDENTIFIER = NEWID()
    , @NewAliasGuid UNIQUEIDENTIFIER = NEWID();

SELECT @OldPersonGuid = [Guid]
    , @AliasId = [PrimaryAliasId]
FROM [dbo].[Person]
WHERE [Id] = 1
    AND [IsSystem] = 1
    AND [FirstName] = N'Admin'
    AND [LastName] = N'Admin';

IF @OldPersonGuid IS NULL
    THROW 50000, N'Baseline step RegenerateAdminPersonGuids: Person 1 is not the built-in Admin person. A migration in the baseline changed or removed it; update Baseline.Reseed.cs.', 1;

SELECT @OldAliasGuid = [Guid]
FROM [dbo].[PersonAlias]
WHERE [Id] = @AliasId
    AND [PersonId] = 1
    AND [AliasPersonId] = 1;

IF @OldAliasGuid IS NULL
    THROW 50000, N'Baseline step RegenerateAdminPersonGuids: the Admin person''s primary alias was not found. A migration in the baseline changed or removed it; update Baseline.Reseed.cs.', 1;

UPDATE [dbo].[Person]
SET [Guid] = @NewPersonGuid
    , [PrimaryAliasGuid] = @NewAliasGuid
WHERE [Id] = 1;

IF @@ROWCOUNT <> 1
    THROW 50000, N'Baseline step RegenerateAdminPersonGuids: expected to update one Person row.', 1;

UPDATE [dbo].[PersonAlias]
SET [Guid] = @NewAliasGuid
WHERE [Id] = @AliasId;

IF @@ROWCOUNT <> 1
    THROW 50000, N'Baseline step RegenerateAdminPersonGuids: expected to update one PersonAlias row.', 1;

UPDATE [dbo].[PersonAlias]
SET [AliasPersonGuid] = @NewPersonGuid
WHERE [AliasPersonGuid] = @OldPersonGuid;

-- Person field type values hold the alias Guid.
UPDATE [av]
SET [av].[Value] = CONVERT( NVARCHAR(36), @NewAliasGuid )
FROM [dbo].[AttributeValue] AS [av]
INNER JOIN [dbo].[Attribute] AS [a] ON [a].[Id] = [av].[AttributeId]
INNER JOIN [dbo].[FieldType] AS [ft] ON [ft].[Id] = [a].[FieldTypeId]
WHERE [ft].[Guid] = '{SystemGuid.FieldType.PERSON}'
    AND [av].[Value] = CONVERT( NVARCHAR(36), @OldAliasGuid );

-- A copy left anywhere known means a new kind of copy this step does not update.
IF EXISTS ( SELECT 1 FROM [dbo].[PersonAlias] WHERE [AliasPersonGuid] = @OldPersonGuid )
    OR EXISTS ( SELECT 1 FROM [dbo].[Person] WHERE [PrimaryAliasGuid] = @OldAliasGuid )
    OR EXISTS ( SELECT 1 FROM [dbo].[AttributeValue] WHERE [Value] LIKE N'%' + CONVERT( NVARCHAR(36), @OldAliasGuid ) + N'%' OR [Value] LIKE N'%' + CONVERT( NVARCHAR(36), @OldPersonGuid ) + N'%' )
    OR EXISTS ( SELECT 1 FROM [dbo].[Attribute] WHERE [DefaultValue] LIKE N'%' + CONVERT( NVARCHAR(36), @OldAliasGuid ) + N'%' OR [DefaultValue] LIKE N'%' + CONVERT( NVARCHAR(36), @OldPersonGuid ) + N'%' )
    THROW 50000, N'Baseline step RegenerateAdminPersonGuids: a copy of the old Admin person or alias Guid is still in the database. Update Baseline.Reseed.cs to replace it, or have the baseline tool''s cleanup remove it.', 1;" );
    }
}
