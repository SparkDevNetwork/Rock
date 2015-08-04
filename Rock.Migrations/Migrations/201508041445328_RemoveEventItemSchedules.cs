// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class RemoveEventItemSchedules : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn( "dbo.EventItemOccurrence", "CampusNote", "Note" );
            AddColumn( "dbo.EventItemOccurrence", "ScheduleId", c => c.Int() );
            CreateIndex( "dbo.EventItemOccurrence", "ScheduleId" );
            AddForeignKey( "dbo.EventItemOccurrence", "ScheduleId", "dbo.Schedule", "Id" );

            Sql( @"
    UPDATE O
    SET [ScheduleId] = ( 
        SELECT TOP 1 [ScheduleId] 
        FROM [EventItemSchedule] 
        WHERE [EventItemOccurrenceId] = O.[Id]
        ORDER BY [Id] )
    FROM [EventItemOccurrence] O

    DELETE [Schedule] 
    WHERE [Id] IN ( SELECT [ScheduleId] FROM [EventItemSchedule] )
    AND [Id] NOT IN ( SELECT [ScheduleId] FROM [EventItemOccurrence] )
" );

            DropForeignKey( "dbo.EventItemSchedule", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey("dbo.EventItemSchedule", "EventItemOccurrenceId", "dbo.EventItemOccurrence");
            DropForeignKey("dbo.EventItemSchedule", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItemSchedule", "ScheduleId", "dbo.Schedule");
            DropIndex("dbo.EventItemSchedule", new[] { "EventItemOccurrenceId" });
            DropIndex("dbo.EventItemSchedule", new[] { "ScheduleId" });
            DropIndex("dbo.EventItemSchedule", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EventItemSchedule", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EventItemSchedule", new[] { "Guid" });
            DropIndex("dbo.EventItemSchedule", new[] { "ForeignId" });
            DropTable( "dbo.EventItemSchedule" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            DropForeignKey( "dbo.EventItemOccurrence", "ScheduleId", "dbo.Schedule" );
            DropIndex( "dbo.EventItemOccurrence", new[] { "ScheduleId" } );
            DropColumn( "dbo.EventItemOccurrence", "ScheduleId" );
            RenameColumn( "dbo.EventItemOccurrence", "Note", "CampusNote" );

            CreateTable(
                "dbo.EventItemSchedule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventItemOccurrenceId = c.Int(nullable: false),
                        ScheduleId = c.Int(nullable: false),
                        ScheduleName = c.String(nullable: false, maxLength: 100),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.EventItemSchedule", "ForeignId");
            CreateIndex("dbo.EventItemSchedule", "Guid", unique: true);
            CreateIndex("dbo.EventItemSchedule", "ModifiedByPersonAliasId");
            CreateIndex("dbo.EventItemSchedule", "CreatedByPersonAliasId");
            CreateIndex("dbo.EventItemSchedule", "ScheduleId");
            CreateIndex("dbo.EventItemSchedule", "EventItemOccurrenceId");
            AddForeignKey("dbo.EventItemSchedule", "ScheduleId", "dbo.Schedule", "Id");
            AddForeignKey("dbo.EventItemSchedule", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.EventItemSchedule", "EventItemOccurrenceId", "dbo.EventItemOccurrence", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EventItemSchedule", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
        }
    }
}
