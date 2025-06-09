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
    public partial class DeleteGroupLocationHistoricalSchedule : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.GroupLocationHistoricalSchedule", "GroupLocationHistoricalId", "dbo.GroupLocationHistorical" );
            DropForeignKey( "dbo.GroupLocationHistoricalSchedule", "ScheduleId", "dbo.Schedule" );
            DropIndex( "dbo.GroupLocationHistoricalSchedule", new[] { "GroupLocationHistoricalId" } );
            DropIndex( "dbo.GroupLocationHistoricalSchedule", new[] { "ScheduleId" } );
            DropIndex( "dbo.GroupLocationHistoricalSchedule", new[] { "Guid" } );

            // The table will be dropped in the PostV18DeleteGroupLocationHistoricalSchedule post-update Job to ensure
            // a possible long-running deletion process doesn't cause the migration to time out.
            //DropTable("dbo.GroupLocationHistoricalSchedule");

            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.0 - Delete GroupLocationHistoricalSchedule",
                description: "This job will delete the deprecated GroupLocationHistoricalSchedule table from the database.",
                jobType: "Rock.Jobs.PostV18DeleteGroupLocationHistoricalSchedule",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_180_DELETE_GROUPLOCATIONHISTORICALSCHEDULE );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateTable(
                "dbo.GroupLocationHistoricalSchedule",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    GroupLocationHistoricalId = c.Int( nullable: false ),
                    ScheduleId = c.Int( nullable: false ),
                    ScheduleName = c.String(),
                    ScheduleModifiedDateTime = c.DateTime(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id );

            CreateIndex( "dbo.GroupLocationHistoricalSchedule", "Guid", unique: true );
            CreateIndex( "dbo.GroupLocationHistoricalSchedule", "ScheduleId" );
            CreateIndex( "dbo.GroupLocationHistoricalSchedule", "GroupLocationHistoricalId" );
            AddForeignKey( "dbo.GroupLocationHistoricalSchedule", "ScheduleId", "dbo.Schedule", "Id" );
            AddForeignKey( "dbo.GroupLocationHistoricalSchedule", "GroupLocationHistoricalId", "dbo.GroupLocationHistorical", "Id", cascadeDelete: true );
        }
    }
}
