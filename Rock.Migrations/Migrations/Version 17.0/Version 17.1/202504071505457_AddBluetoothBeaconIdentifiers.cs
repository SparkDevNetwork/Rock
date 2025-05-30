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
    public partial class AddBluetoothBeaconIdentifiers : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Campus", "BeaconId", c => c.Int());
            AddColumn("dbo.Location", "BeaconId", c => c.Int());

            // Add RootGroupTypeId to AttendanceOccurrence to make reporting on check-in easier.
            AddColumn( "dbo.AttendanceOccurrence", "RootGroupTypeId", c => c.Int());
            CreateIndex("dbo.AttendanceOccurrence", "RootGroupTypeId");
            AddForeignKey("dbo.AttendanceOccurrence", "RootGroupTypeId", "dbo.GroupType", "Id", cascadeDelete: true);

            // Set the BeaconId to the Id for all existing Campuses.
            Sql( "UPDATE [Campus] SET [BeaconId] = [Id] WHERE [Id] <= 65535" );

            RockMigrationHelper.AddPostUpdateServiceJob(
                "Rock Update Helper v17.1 - Populate Attendance Root Group Type",
                "This job will populate the new RootGroupTypeId property on existing Attendance Occurrence records.",
                "Rock.Jobs.PostUpdateJobs.PostV171PopulateAttendanceRootGroupType",
                "0 15 2 1/1 * ? *",
                SystemGuid.ServiceJob.DATA_MIGRATIONS_171_POPULATE_ATTENDANCE_ROOT_GROUP_TYPE );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_171_POPULATE_ATTENDANCE_ROOT_GROUP_TYPE}'" );

            DropForeignKey("dbo.AttendanceOccurrence", "RootGroupTypeId", "dbo.GroupType");
            DropIndex("dbo.AttendanceOccurrence", new[] { "RootGroupTypeId" });
            DropColumn("dbo.AttendanceOccurrence", "RootGroupTypeId");

            DropColumn("dbo.Location", "BeaconId");
            DropColumn("dbo.Campus", "BeaconId");
        }
    }
}
