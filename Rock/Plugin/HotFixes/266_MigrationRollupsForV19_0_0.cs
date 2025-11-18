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
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 266, "19.0" )]
    public class MigrationRollupsForV19_0_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddEventRegistrationAdminAuthToTopLevelCategories();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
        }

        #region NA: Migration to Add EDIT/ADMINISTRATE Auth to the "RSR - Event Registration Administration" group on all top-level RegistrationTemplate categories

        private void AddEventRegistrationAdminAuthToTopLevelCategories()
        {
            Sql( @"
-- Add Edit and Administrate auth to the RSR Event Registration Administration group
-- on all top-level RegistrationTemplate categories, without duplicates.

DECLARE @Now DATETIME = GETDATE();
DECLARE @RSREventRegistrationAdministrationRoleId INT =
(
    SELECT [Id]
    FROM [Group]
    WHERE [Guid] = '2A92086B-DFF0-4B9C-46CB-4DAD805615AF'
);

DECLARE @RegistrationTemplateEntityTypeId INT =
(
    SELECT [Id]
    FROM [EntityType]
    WHERE [Name] = 'Rock.Model.RegistrationTemplate'
);

DECLARE @CategoryEntityTypeId INT =
(
    SELECT [Id]
    FROM [EntityType]
    WHERE [Name] = 'Rock.Model.Category'
);

;WITH [TopLevelRegistrationTemplateCategories] AS
(
    SELECT
        c.[Id] AS [CategoryId]
    FROM [Category] AS c
    WHERE
        c.[ParentCategoryId] IS NULL
        AND c.[EntityTypeId] = @RegistrationTemplateEntityTypeId
),
[DesiredAuth] AS
(
    SELECT
          tl.[CategoryId]
        , a.[Action]
        , a.[Order]
    FROM [TopLevelRegistrationTemplateCategories] AS tl
    CROSS APPLY (VALUES
        ('Edit', 0),
        ('Administrate', 0)
    ) AS a([Action], [Order])
)
INSERT INTO [Auth]
(
      [EntityTypeId]
    , [EntityId]
    , [Order]
    , [Action]
    , [AllowOrDeny]
    , [SpecialRole]
    , [GroupId]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
)
SELECT
      @CategoryEntityTypeId                       -- [EntityTypeId]
    , d.[CategoryId]                              -- [EntityId]
    , d.[Order]                                   -- [Order]
    , d.[Action]                                  -- [Action]
    , 'A'                                         -- [AllowOrDeny]
    , 0                                           -- [SpecialRole]
    , @RSREventRegistrationAdministrationRoleId   -- [GroupId]
    , NEWID()                                     -- [Guid]
    , @Now                                        -- [CreatedDateTime]
    , @Now                                        -- [ModifiedDateTime]
FROM [DesiredAuth] AS d
WHERE NOT EXISTS
(
    SELECT 1
    FROM [Auth] AS a
    WHERE
        a.[EntityTypeId] = @CategoryEntityTypeId
        AND a.[EntityId] = d.[CategoryId]
        AND a.[Action] = d.[Action]
        AND a.[SpecialRole] = 0
        AND a.[GroupId] = @RSREventRegistrationAdministrationRoleId
);
" );
        }
        #endregion

    }
}