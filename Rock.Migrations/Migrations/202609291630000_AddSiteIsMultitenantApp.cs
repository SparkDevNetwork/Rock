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
    /// Adds the IsMultitenantApp flag to Site, which marks the Site served to the
    /// shared (multitenant) mobile application, and the daily job that publishes the
    /// church's campuses to the church directory for that application.
    /// </summary>
    public partial class AddSiteIsMultitenantApp : Rock.Migrations.RockMigration
    {
        private const string PublishCampusesJobClass = "Rock.Jobs.PublishPlatformMobileAppCampuses";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Site", "IsMultitenantApp", c => c.Boolean(nullable: false, defaultValue: false));

            AddPublishPlatformMobileAppCampusesJobUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddPublishPlatformMobileAppCampusesJobDown();

            DropColumn("dbo.Site", "IsMultitenantApp");
        }

        /// <summary>
        /// Adds the job that publishes the church's campuses to the church directory.
        /// </summary>
        private void AddPublishPlatformMobileAppCampusesJobUp()
        {
            // 3:30am daily. Campus addresses rarely change and a run that finds nothing new makes no call.
            var cronSchedule = "0 30 3 1/1 * ? *";

            Sql( $@"
IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.PUBLISH_PLATFORM_MOBILE_APP_CAMPUSES}' )
BEGIN
    INSERT INTO [ServiceJob] (
        [IsSystem],
        [IsActive],
        [Name],
        [Description],
        [Class],
        [CronExpression],
        [NotificationStatus],
        [Guid] )
    VALUES (
        1,
        1,
        'Publish Platform Mobile App Campuses',
        'Publishes the church''s active campuses to the church directory for the shared mobile application when they change. Does nothing unless the church is enrolled in the shared mobile application.',
        '{PublishCampusesJobClass}',
        '{cronSchedule}',
        1,
        '{SystemGuid.ServiceJob.PUBLISH_PLATFORM_MOBILE_APP_CAMPUSES}' );
END" );
        }

        /// <summary>
        /// Removes the job that publishes the church's campuses to the church directory.
        /// </summary>
        private void AddPublishPlatformMobileAppCampusesJobDown()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.PUBLISH_PLATFORM_MOBILE_APP_CAMPUSES}'" );
        }
    }
}
