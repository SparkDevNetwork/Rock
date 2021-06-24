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
    /// Add CampusSchedule model and supporting Defined Type.
    /// </summary>
    public partial class AddCampusSchedules : Rock.Migrations.RockMigration
    {
        #region Up

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddCampusScheduleTable();
            AddScheduleTypeDefinedType();
        }

        /// <summary>
        /// Add CampusSchedule Table.
        /// </summary>
        private void AddCampusScheduleTable()
        {
            CreateTable(
                "dbo.CampusSchedule",
                c => new
                    {
                        Id = c.Int( nullable: false, identity: true ),
                        CampusId = c.Int( nullable: false ),
                        ScheduleId = c.Int( nullable: false ),
                        ScheduleTypeValueId = c.Int( nullable: false ),
                        Order = c.Int( nullable: false ),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid( nullable: false ),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String( maxLength: 100 ),
                    } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Campus", t => t.CampusId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Schedule", t => t.ScheduleId, cascadeDelete: true )
                .ForeignKey( "dbo.DefinedValue", t => t.ScheduleTypeValueId )
                .Index( t => t.CampusId )
                .Index( t => t.ScheduleId )
                .Index( t => t.ScheduleTypeValueId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        /// <summary>
        /// Add Schedule Type Defined Type and Weekend Service Defined Value.
        /// </summary>
        private void AddScheduleTypeDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "Campus", "Schedule Type", "List of schedule types a campus can have.", Rock.SystemGuid.DefinedType.SCHEDULE_TYPE, @"");
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.SCHEDULE_TYPE, "Weekend Service", "", Rock.SystemGuid.DefinedValue.SCHEDULE_TYPE_WEEKEND_SERVICE, true );
        }

        #endregion Up

        #region Down

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveCampusScheduleTable();
            RemoveScheduleTypeDefinedType();
        }

        /// <summary>
        /// Remove CampusSchedule Table.
        /// </summary>
        private void RemoveCampusScheduleTable()
        {
            DropForeignKey( "dbo.CampusSchedule", "ScheduleTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.CampusSchedule", "ScheduleId", "dbo.Schedule" );
            DropForeignKey( "dbo.CampusSchedule", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CampusSchedule", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CampusSchedule", "CampusId", "dbo.Campus" );
            DropIndex( "dbo.CampusSchedule", new[] { "Guid" } );
            DropIndex( "dbo.CampusSchedule", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CampusSchedule", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CampusSchedule", new[] { "ScheduleTypeValueId" } );
            DropIndex( "dbo.CampusSchedule", new[] { "ScheduleId" } );
            DropIndex( "dbo.CampusSchedule", new[] { "CampusId" } );
            DropTable( "dbo.CampusSchedule" );
        }

        /// <summary>
        /// Remove Schedule Type Defined Type and Weekend Service Defined Value.
        /// </summary>
        private void RemoveScheduleTypeDefinedType()
        {
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.SCHEDULE_TYPE_WEEKEND_SERVICE );
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.SCHEDULE_TYPE);
        }

        #endregion Down
    }
}
