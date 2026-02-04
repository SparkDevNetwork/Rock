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

using Rock.Model;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 274, "18.1" )]
    public class MigrationRollupsForV18_2_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NA_RemoveDuplicateDefinedTypeCategories();
            NA_SetFacilitatorAndStudentRolesToCanViewUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            NA_SetFacilitatorAndStudentRolesToCanViewDown();
        }

        #region NA: Data Migration to Remove Duplicate Categories for DefinedType

        /// <summary>
        /// Remap DefinedType.CategoryId from duplicate Category rows to the keeper per name,
        /// then delete duplicates for 'Global', 'Group', 'Person' under the DefinedType
        /// EntityType.
        /// </summary>
        private void NA_RemoveDuplicateDefinedTypeCategories()
        {
            Sql( @"
-- Clean up the duplicate Categories ('Global', 'Group', and 'Person') 
-- that are for the DefinedType entity:

DECLARE @Global_KeepCategoryGuid UNIQUEIDENTIFIER = 'CB71E9CD-F11D-4EA0-A154-EDF3CECF6F77';
DECLARE @Group_KeepCategoryGuid UNIQUEIDENTIFIER = '8437CC1A-A799-4A49-B336-19CE03A06EF0';
DECLARE @Person_KeepCategoryGuid UNIQUEIDENTIFIER = '994992BC-BE9E-4555-9E76-F1D6219AAB13';

DECLARE @DefinedTypeEntityTypeGuid UNIQUEIDENTIFIER = '6028D502-79F4-4A74-9323-525E90F900C7';
DECLARE @DefinedTypeEntityTypeId INT;

SELECT @DefinedTypeEntityTypeId = et.[Id]
FROM [EntityType] et
WHERE et.[Guid] = @DefinedTypeEntityTypeGuid;

-- Global: remap then delete
UPDATE dt
SET dt.[CategoryId] = keep.[Id]
FROM [DefinedType] dt
JOIN [Category] dup
  ON dup.[Id] = dt.[CategoryId]
 AND dup.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND dup.[Name] = 'Global'
JOIN [Category] keep
  ON keep.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND keep.[Name] = 'Global'
 AND keep.[Guid] = @Global_KeepCategoryGuid
-- NULL-safe equality across the qualifiers and parent
 AND ISNULL(dup.[ParentCategoryId], -2147483648) = ISNULL(keep.[ParentCategoryId], -2147483648)
 AND ISNULL(dup.[EntityTypeQualifierColumn], '') = ISNULL(keep.[EntityTypeQualifierColumn], '')
 AND ISNULL(dup.[EntityTypeQualifierValue],  '') = ISNULL(keep.[EntityTypeQualifierValue],  '')
WHERE dup.[Guid] <> keep.[Guid];

DELETE [Category]
WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
  AND [Name] = 'Global'
  AND [Guid] <> @Global_KeepCategoryGuid
  AND EXISTS (
        SELECT 1
        FROM [Category] keep
        WHERE keep.[EntityTypeId] = @DefinedTypeEntityTypeId
          AND keep.[Name] = 'Global'
          AND keep.[Guid] = @Global_KeepCategoryGuid
          AND ISNULL(keep.[ParentCategoryId], -2147483648) = ISNULL([Category].[ParentCategoryId], -2147483648)
          AND ISNULL(keep.[EntityTypeQualifierColumn], '') = ISNULL([Category].[EntityTypeQualifierColumn], '')
          AND ISNULL(keep.[EntityTypeQualifierValue],  '') = ISNULL([Category].[EntityTypeQualifierValue],  '')
  );

-- Group: remap then delete
UPDATE dt
SET dt.[CategoryId] = keep.[Id]
FROM [DefinedType] dt
JOIN [Category] dup
  ON dup.[Id] = dt.[CategoryId]
 AND dup.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND dup.[Name] = 'Group'
JOIN [Category] keep
  ON keep.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND keep.[Name] = 'Group'
 AND keep.[Guid] = @Group_KeepCategoryGuid
 AND ISNULL(dup.[ParentCategoryId], -2147483648) = ISNULL(keep.[ParentCategoryId], -2147483648)
 AND ISNULL(dup.[EntityTypeQualifierColumn], '') = ISNULL(keep.[EntityTypeQualifierColumn], '')
 AND ISNULL(dup.[EntityTypeQualifierValue],  '') = ISNULL(keep.[EntityTypeQualifierValue],  '')
WHERE dup.[Guid] <> keep.[Guid];

DELETE [Category]
WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
  AND [Name] = 'Group'
  AND [Guid] <> @Group_KeepCategoryGuid
  AND EXISTS (
        SELECT 1
        FROM [Category] keep
        WHERE keep.[EntityTypeId] = @DefinedTypeEntityTypeId
          AND keep.[Name] = 'Group'
          AND keep.[Guid] = @Group_KeepCategoryGuid
          AND ISNULL(keep.[ParentCategoryId], -2147483648) = ISNULL([Category].[ParentCategoryId], -2147483648)
          AND ISNULL(keep.[EntityTypeQualifierColumn], '') = ISNULL([Category].[EntityTypeQualifierColumn], '')
          AND ISNULL(keep.[EntityTypeQualifierValue],  '') = ISNULL([Category].[EntityTypeQualifierValue],  '')
  );

-- Person: remap then delete
UPDATE dt
SET dt.[CategoryId] = keep.[Id]
FROM [DefinedType] dt
JOIN [Category] dup
  ON dup.[Id] = dt.[CategoryId]
 AND dup.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND dup.[Name] = 'Person'
JOIN [Category] keep
  ON keep.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND keep.[Name] = 'Person'
 AND keep.[Guid] = @Person_KeepCategoryGuid
 AND ISNULL(dup.[ParentCategoryId], -2147483648) = ISNULL(keep.[ParentCategoryId], -2147483648)
 AND ISNULL(dup.[EntityTypeQualifierColumn], '') = ISNULL(keep.[EntityTypeQualifierColumn], '')
 AND ISNULL(dup.[EntityTypeQualifierValue],  '') = ISNULL(keep.[EntityTypeQualifierValue],  '')
WHERE dup.[Guid] <> keep.[Guid];

DELETE [Category]
WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
  AND [Name] = 'Person'
  AND [Guid] <> @Person_KeepCategoryGuid
  AND EXISTS (
        SELECT 1
        FROM [Category] keep
        WHERE keep.[EntityTypeId] = @DefinedTypeEntityTypeId
          AND keep.[Name] = 'Person'
          AND keep.[Guid] = @Person_KeepCategoryGuid
          AND ISNULL(keep.[ParentCategoryId], -2147483648) = ISNULL([Category].[ParentCategoryId], -2147483648)
          AND ISNULL(keep.[EntityTypeQualifierColumn], '') = ISNULL([Category].[EntityTypeQualifierColumn], '')
          AND ISNULL(keep.[EntityTypeQualifierValue],  '') = ISNULL([Category].[EntityTypeQualifierValue],  '')
  );
          " );
        }

        #endregion

        #region NA: Migration to Fix Student and Facilitator LMS Roles

        /// <summary>
        /// Fix issue #6601 by setting CanView for both Faciliator and Student roles.
        /// </summary>
        private void NA_SetFacilitatorAndStudentRolesToCanViewUp()
        {
            Sql( @"
-- Fix issue #6601 by setting CanView for both Faciliator and Student roles 
DECLARE @Facilitator_RoleGuid UNIQUEIDENTIFIER = '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A';
DECLARE @Student_RoleGuid UNIQUEIDENTIFIER = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2';

UPDATE [GroupTypeRole]
SET [CanView] = 1
WHERE [Guid] IN ( @Facilitator_RoleGuid, @Student_RoleGuid );
          " );
        }

        /// <summary>
        /// Remove the fix for issue #6601
        /// </summary>
        private void NA_SetFacilitatorAndStudentRolesToCanViewDown()
        {
            Sql( @"
-- Remove fix for issue #6601 by setting CanView for both Faciliator and Student roles 
DECLARE @Facilitator_RoleGuid UNIQUEIDENTIFIER = '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A';
DECLARE @Student_RoleGuid UNIQUEIDENTIFIER = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2';

UPDATE [GroupTypeRole]
SET [CanView] = 0
WHERE [Guid] IN ( @Facilitator_RoleGuid, @Student_RoleGuid );
          " );
        }

        #endregion
    }
}