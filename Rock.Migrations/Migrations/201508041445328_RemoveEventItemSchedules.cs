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

    -- Mark other schedules to be deleted (can't delete yet because of foreign key restraint to EventItemSchedule)
    UPDATE [Schedule] SET [ForeignId] = 'DeleteMe'
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

            Sql( @"
    DELETE [Schedule]
    WHERE [ForeignId] = 'DeleteMe'
" );
            // Update parent label to include last name
            Sql( @"
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'CE57450F-634A-420A-BF5A-B43E9B20ABF2' )
    DECLARE @BinaryFileId int = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = '9B098DB0-952C-43FB-A5BD-511E3C2B72FB' )
    IF @AttributeId IS NOT NULL AND @BinaryFileId IS NOT NULL
    BEGIN

        UPDATE [BinaryFileData]
        SET [Content] = 0xEFBBBF1043547E7E43442C7E43435E7E43547E0D0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534431355E4A55535E4C524E5E4349305E585A0D0A5E58410D0A5E4D4D540D0A5E50573831320D0A5E4C4C303430360D0A5E4C53300D0A5E46543435392C3131385E41304E2C3133352C3133345E46423333332C312C302C525E46485C5E46445757575E46530D0A5E465431312C3234385E41304E2C3133352C3134365E46485C5E4644315E46530D0A5E465431322C3238345E41304E2C32382C32385E46485C5E4644325E46530D0A5E465431342C3334325E41304E2C33392C33385E46485C5E46444368696C64205069636B2D757020526563656970745E46530D0A5E465431352C3336395E41304E2C32332C32345E46485C5E4644466F722074686520736166657479206F6620796F7572206368696C642C20796F75206D7573742070726573656E7420746869732072656365697074207768656E207069636B696E675E46530D0A5E465431352C3339335E41304E2C32332C32345E46485C5E4644757020796F7572206368696C642E20496620796F75206C6F7365207468697320706C6561736520736565207468652061726561206469726563746F722E5E46530D0A5E4C52595E464F302C305E47423831322C302C3133365E46535E4C524E0D0A5E5051312C302C312C595E585A0D0A
        WHERE [Id] = @BinaryFileId
        AND [ModifiedDateTime] IS NULL

        UPDATE [AttributeValue]
        SET [Value] = 'WWW^34|1^35|2^36'
        WHERE [AttributeId] = @AttributeId
        AND [EntityId] = @BinaryFileId
        AND [Value] = 'MMM^34|2^35'

    END
" );

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
