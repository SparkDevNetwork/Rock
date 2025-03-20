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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250319 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ResetPublicLearningCourseDetailBlockTypeAttributeValues();
            AddMissingIconToDefaultCheckinKioskAdsContentChannelUp();
            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddMissingIconFromDefaultCheckinKioskAdsContentChannelDown();
        }

        #region NA: Reset any existing/old attribute values for the Public Learning Course Detail block type

        private void ResetPublicLearningCourseDetailBlockTypeAttributeValues()
        {
            // Delete any existing attribute value for the Lava Template attribute of this block type.
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            Sql( "DELETE [AttributeValue] WHERE [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'DA6C3170-5264-427D-AC22-8D50D2F6D2F6' )" );
        }

        #endregion

        #region DH: Add missing icon to Default Check-in Kiosk Ads content channel.

        private void AddMissingIconToDefaultCheckinKioskAdsContentChannelUp()
        {
            Sql( "UPDATE [ContentChannel] SET [IconCssClass] = 'fa fa-tv' WHERE [Guid] = 'a57bdbcd-fa77-4a6e-967d-1c5ace962587'" );
        }

        private void AddMissingIconFromDefaultCheckinKioskAdsContentChannelDown()
        {
            Sql( "UPDATE [ContentChannel] SET [IconCssClass] = '' WHERE [Guid] = 'a57bdbcd-fa77-4a6e-967d-1c5ace962587'" );
        }

        #endregion

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
            Sql( @"
UPDATE [dbo].[__MigrationHistory]
SET [Model] = 0x
WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
