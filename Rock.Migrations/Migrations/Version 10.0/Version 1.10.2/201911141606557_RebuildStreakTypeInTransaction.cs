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
    public partial class RebuildStreakTypeInTransaction : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $"DELETE FROM ServiceJob WHERE Guid = '{Rock.SystemGuid.ServiceJob.REBUILD_STREAK}';" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $@"
                INSERT INTO ServiceJob (
                    IsSystem,
                    IsActive,
                    Name,
                    Description,
                    Class,
                    Guid,
                    CreatedDateTime,
                    NotificationStatus,
                    CronExpression
                ) VALUES (
                    1, -- IsSystem
                    1, -- IsActive
                    'Rebuild Streak Data', -- Name
                    'Rebuild streak maps. This runs on demand and has the cron expression set to the distant future since it does not run on a schedule.', -- Description
                    'Rock.Jobs.RebuildStreakMaps', -- Class
                    '{Rock.SystemGuid.ServiceJob.REBUILD_STREAK}', -- Guid
                    GETDATE(), -- Created
                    1, -- All notifications
                    '{Rock.Model.ServiceJob.NeverScheduledCronExpression}' -- In the year 2200, so basically never run this scheduled since it runs on demand
                );" );
        }
    }
}
