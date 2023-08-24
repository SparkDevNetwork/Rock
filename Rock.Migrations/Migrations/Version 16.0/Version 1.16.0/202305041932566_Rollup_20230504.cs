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
namespace Rock.Migrations
{

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20230504 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateLogSettingsBlockTypeNameUp();
            UpdateRockCleanupCacheDirectorySettingUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateLogSettingsBlockTypeNameDown();
        }

        /// <summary>
        /// JPH: Update Log Settings BlockType Name Up.
        /// </summary>
        private void UpdateLogSettingsBlockTypeNameUp()
        {
            Sql( @"
UPDATE [BlockType]
SET [Name] = 'Log Settings'
WHERE [Guid] = '6ABC44FD-C4D7-4E30-8537-3A065B493453';" );
        }

        /// <summary>
        /// JPH: Update Log Settings BlockType Name Down.
        /// </summary>
        private void UpdateLogSettingsBlockTypeNameDown()
        {
            Sql( @"
UPDATE [BlockType]
SET [Name] = 'Logs'
WHERE [Guid] = '6ABC44FD-C4D7-4E30-8537-3A065B493453';" );
        }

        /// <summary>
        /// DL: Update the RockCleanup job base cache directory.
        /// </summary>
        private void UpdateRockCleanupCacheDirectorySettingUp()
        {
            Sql( $@"
-- Identify the Attribute Value.
DECLARE @attributeKey VARCHAR(max) = 'BaseCacheDirectory'
DECLARE @oldValue VARCHAR(max) = '~/Cache'
DECLARE @newValue VARCHAR(max) = '~/App_Data/Cache'
DECLARE @entityTypeId INT = (
        SELECT TOP 1 Id
        FROM [EntityType]
        WHERE [Guid] = '{Rock.SystemGuid.EntityType.SERVICE_JOB}'
        )
      , @entityId INT = (
        SELECT TOP 1 id
        FROM [ServiceJob]
        WHERE [Class] = 'Rock.Jobs.RockCleanup'
        )

-- Update the Attribute default value and existing values that match the old value.
DECLARE @attributeId INT = (
        SELECT TOP 1 Id
        FROM [Attribute]
        WHERE [Key] = @attributeKey
            AND [EntityTypeId] = @entityTypeId
        )

UPDATE [Attribute]
SET [DefaultValue] = @newValue
WHERE Id = @attributeId
    AND [DefaultValue] = @oldValue;

UPDATE [AttributeValue]
SET [Value] = @newValue
WHERE [Value] = @oldValue
    AND [AttributeId] = @attributeId
    AND [EntityId] = @entityId
" );
        }
    }
}
