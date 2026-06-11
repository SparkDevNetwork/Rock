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
    public partial class AddScheduleDateTable : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ScheduleDate",
                c => new
                {
                    ScheduleId = c.Int( nullable: false ),
                    StartDateTime = c.DateTime( nullable: false ),
                    EndDateTime = c.DateTime( nullable: false ),
                    StartDateKey = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.ScheduleId, t.StartDateTime } )
                // Do not create foreign key so that the table can be rebuilt as needed.
                //.ForeignKey( "dbo.AnalyticsSourceDate", t => t.StartDateKey )
                .ForeignKey( "dbo.Schedule", t => t.ScheduleId, cascadeDelete: true )
                .Index( t => new { t.ScheduleId, t.StartDateTime } )
                .Index( t => t.StartDateKey );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.ScheduleDate", "ScheduleId", "dbo.Schedule" );
            //DropForeignKey( "dbo.ScheduleDate", "StartDateKey", "dbo.AnalyticsSourceDate" );
            DropIndex( "dbo.ScheduleDate", new[] { "StartDateKey" } );
            DropIndex( "dbo.ScheduleDate", new[] { "ScheduleId", "StartDateTime" } );
            DropTable( "dbo.ScheduleDate" );
        }
    }
}
