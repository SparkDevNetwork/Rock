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
    [MigrationNumber( 252, "17.1" )]
    public class MigrationRollupsForV17_2_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateStatusTemplateAttributeUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // No down
        }

        #region KH: Update the Default Value for the Status Template Attribute

        private void UpdateStatusTemplateAttributeUp()
        {
            var targetAttributeColumn = RockMigrationHelper.NormalizeColumnCRLF( "DefaultValue" );
            var targetAttributeValueColumn = RockMigrationHelper.NormalizeColumnCRLF( "Value" );

            string oldValue = @"<span class='badge badge-danger badge-circle js-legend-badge'>{{ IdleTooltip }}</span>";

            string newValue = @"<span class='badge badge-danger badge-circle js-legend-badge' data-toggle='tooltip' data-html='true' title='{{IdleTooltipList}}'>{{ IdleTooltip }}</span>";

            oldValue = oldValue.Replace( "'", "''" );
            newValue = newValue.Replace( "'", "''" );

            Sql( $@"
DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '23438CBC-105B-4ADB-8B9A-D5DDDCDD7643')

UPDATE [Attribute]
SET [DefaultValue] = REPLACE({targetAttributeColumn}, '{oldValue}', '{newValue}')
, [DefaultPersistedTextValue] = NULL
, [DefaultPersistedHtmlValue] = NULL
, [DefaultPersistedCondensedTextValue] = NULL
, [DefaultPersistedCondensedHtmlValue] = NULL
, [IsDefaultPersistedValueDirty] = 1
WHERE [EntityTypeId] = @BlockEntityTypeId
    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
    AND [EntityTypeQualifierValue] = @BlockTypeId
    AND [Key] = 'StatusTemplate';

UPDATE [AttributeValue]
SET [Value] = REPLACE({targetAttributeValueColumn}, '{oldValue}', '{newValue}')
, [PersistedTextValue] = NULL
, [PersistedHtmlValue] = NULL
, [PersistedCondensedTextValue] = NULL
, [PersistedCondensedHtmlValue] = NULL
, [IsPersistedValueDirty] = 1
WHERE [AttributeId] IN (
    SELECT [Id] FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = @BlockTypeId
        AND [Key] = 'StatusTemplate'
)
AND {targetAttributeValueColumn} LIKE '%{oldValue}%';"
            );
        }

        #endregion
    }
}