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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Moves the Edit and Administrate rules that the 19.0 upgrade added for the RSR - Event Registration
    /// Administration role on top-level registration template categories behind any rule an administrator had already
    /// set on the same category and action.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 352, "19.9" )]
    public class RepairEventRegistrationAdminAuthOrder : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                11/20/26 - JPH

                The 19.0 upgrade added these rules at the top of each category's list, so the role's grant won over a
                restriction an administrator had already set. The added rules are found by what the upgrade wrote: an
                Allow for the role at Order 0, with no creator and never changed since (ModifiedDateTime equals
                CreatedDateTime); a rule added through the Security dialog always has a creator. Only rules older than
                the added rule (a lower Id) count. A list re-ordered by hand after the upgrade is left alone: dragging
                the added rule to the top does not save it, but it does save the older rules it passed, so an older
                rule changed after the added rule was created marks the list as re-ordered. Older rules keep their
                place; the added rule, and any rule added after it, move below the older rules in the same order. Nothing is deleted, and a rerun matches nothing
                because the repaired rows get a new ModifiedDateTime.

                Reason: Keep administrator security rules ahead of the role's upgrade-added rules.
            */
            Sql( @"
DECLARE @CategoryEntityTypeId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Category' );
DECLARE @RegistrationTemplateEntityTypeId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.RegistrationTemplate' );
DECLARE @RoleId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2A92086B-DFF0-4B9C-46CB-4DAD805615AF' );

/* The rule the upgrade added on each top-level registration template category and action. */
WITH [AddedRules] AS (
    SELECT a.[EntityId]
        , a.[Action]
        , a.[Id]
    FROM [Auth] AS a
    INNER JOIN [Category] AS c ON c.[Id] = a.[EntityId]
    WHERE a.[EntityTypeId] = @CategoryEntityTypeId
        AND c.[EntityTypeId] = @RegistrationTemplateEntityTypeId
        AND c.[ParentCategoryId] IS NULL
        AND a.[Action] IN ( 'Edit', 'Administrate' )
        AND a.[AllowOrDeny] = 'A'
        AND a.[SpecialRole] = 0
        AND a.[GroupId] = @RoleId
        AND a.[PersonAliasId] IS NULL
        AND a.[Order] = 0
        AND a.[CreatedByPersonAliasId] IS NULL
        AND a.[ModifiedDateTime] = a.[CreatedDateTime]
        AND NOT EXISTS (
            SELECT 1
            FROM [Auth] AS o
            WHERE o.[EntityTypeId] = a.[EntityTypeId]
                AND o.[EntityId] = a.[EntityId]
                AND o.[Action] = a.[Action]
                AND o.[Id] < a.[Id]
                AND o.[ModifiedDateTime] > a.[CreatedDateTime] )
)
/* Groups where a rule older than the added rule sorts after it, with the highest Order of the older rules. */
, [GroupsToRepair] AS (
    SELECT r.[EntityId]
        , r.[Action]
        , r.[Id] AS [AddedId]
        , ( SELECT MAX( o.[Order] )
            FROM [Auth] AS o
            WHERE o.[EntityTypeId] = @CategoryEntityTypeId
                AND o.[EntityId] = r.[EntityId]
                AND o.[Action] = r.[Action]
                AND o.[Id] < r.[Id] ) AS [MaxOlderOrder]
    FROM [AddedRules] AS r
    WHERE EXISTS (
        SELECT 1
        FROM [Auth] AS o
        WHERE o.[EntityTypeId] = @CategoryEntityTypeId
            AND o.[EntityId] = r.[EntityId]
            AND o.[Action] = r.[Action]
            AND o.[Id] < r.[Id]
            AND o.[Order] > 0 )
)
/* The added rule and any rule added after it, numbered in their current order, placed after the older rules. */
, [RulesToMove] AS (
    SELECT a.[Id]
        , g.[MaxOlderOrder] + ROW_NUMBER() OVER ( PARTITION BY a.[EntityId], a.[Action] ORDER BY a.[Order], a.[Id] ) AS [NewOrder]
    FROM [Auth] AS a
    INNER JOIN [GroupsToRepair] AS g ON g.[EntityId] = a.[EntityId] AND g.[Action] = a.[Action]
    WHERE a.[EntityTypeId] = @CategoryEntityTypeId
        AND a.[Id] >= g.[AddedId]
)
UPDATE a
SET a.[Order] = m.[NewOrder]
    , a.[ModifiedDateTime] = GETDATE()
FROM [Auth] AS a
INNER JOIN [RulesToMove] AS m ON m.[Id] = a.[Id];" );
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
