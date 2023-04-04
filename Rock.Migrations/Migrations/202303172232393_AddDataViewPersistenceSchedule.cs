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
    public partial class AddDataViewPersistenceSchedule : Rock.Migrations.RockMigration
    {
        public const string SCHEDULE_PERSISTED_DATAVIEWS = "EEC7A935-BEF2-4450-9CBF-B85CEC6F7FEA";
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.DataView", "PersistedScheduleId", c => c.Int());
            CreateIndex("dbo.DataView", "PersistedScheduleId");
            AddForeignKey("dbo.DataView", "PersistedScheduleId", "dbo.Schedule", "Id");
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.SCHEDULE, "Persisted DataView Schedules", "", "", SCHEDULE_PERSISTED_DATAVIEWS );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteCategory( SCHEDULE_PERSISTED_DATAVIEWS );
            DropForeignKey("dbo.DataView", "PersistedScheduleId", "dbo.Schedule");
            DropIndex("dbo.DataView", new[] { "PersistedScheduleId" });
            DropColumn("dbo.DataView", "PersistedScheduleId");
        }
    }
}
